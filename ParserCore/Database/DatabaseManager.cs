using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using WaywardGamers.KParser.KPDatabaseDataSetTableAdapters;

namespace WaywardGamers.KParser
{
    #region Event classes
    public delegate void DatabaseWatchEventHandler(object sender, DatabaseWatchEventArgs dbArgs);

    public class DatabaseWatchEventArgs : EventArgs
    {
        /// <summary>
        /// Gets and sets the database that is accessible for this event.
        /// </summary>
        public KPDatabaseDataSet Dataset { get; private set; }

        /// <summary>
        /// Gets and sets just the changes that are being applied to the database.
        /// </summary>
        public KPDatabaseDataSet DatasetChanges { get; private set; }

        /// <summary>
        /// Constructor is internal; only created by the DatabaseManager.
        /// </summary>
        /// <param name="managedDataset">The dataset provided by the database manager.</param>
        internal DatabaseWatchEventArgs(KPDatabaseDataSet managedDataset, KPDatabaseDataSet changedDataset)
        {
            Dataset = managedDataset;
            DatasetChanges = changedDataset;
        }
    }
    #endregion

    public sealed class DatabaseManager : IDisposable
    {
        #region Singleton Construction
        /// <summary>
        /// Make the class a singleton
        /// </summary>
        private static readonly DatabaseManager instance = new DatabaseManager();

         /// <summary>
        /// Private constructor ensures singleton purity.
        /// </summary>
        private DatabaseManager()
		{
            string applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            defaultSaveDirectory = Path.Combine(applicationDirectory, Properties.Settings.Default.DefaultSaveSubdirectory);

            defaultCopyDatabaseFilename = Path.Combine(defaultSaveDirectory, Properties.Settings.Default.DefaultUnnamedDBFileName);

            Version assemVersion = Assembly.GetExecutingAssembly().GetName().Version;
            assemblyVersionString = string.Format("{0}.{1}", assemVersion.Major, assemVersion.Minor);

            // Initialize record-keeping variables
            lastKilledList = new Dictionary<string, KPDatabaseDataSet.BattlesRow>();
            activeBattleList = new Dictionary<KPDatabaseDataSet.BattlesRow, DateTime>();
            activeMobBattleList = new Dictionary<string, KPDatabaseDataSet.BattlesRow>();
        }

		/// <summary>
        /// Gets the singleton instance of the DatabaseManager class.
		/// </summary>
        public static DatabaseManager Instance
		{
			get
			{
				return instance;
			}
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                CloseDatabase();

                // Note disposing has been done.
                disposed = true;
            }
        }

        #endregion

        #region Member Variables
        private const int databaseVersion = 1;
        private string assemblyVersionString;

        private string defaultSaveDirectory;

        private string defaultCopyDatabaseFilename;
        private string databaseFilename;
        private string databaseConnectionString;

        private KPDatabaseDataSet localDB;
        private KPDatabaseDataSetTableAdapters.TableAdapterManager localTAManager;

        private DateTime lastUpdateTime = DateTime.Now;
        private TimeSpan updateDelayWindow = TimeSpan.FromSeconds(5);

        public event DatabaseWatchEventHandler DatabaseChanging;
        public event DatabaseWatchEventHandler DatabaseChanged;

        private bool disposed = false;

        private Dictionary<string, KPDatabaseDataSet.BattlesRow> lastKilledList;
        private KPDatabaseDataSet.BattlesRow lastFinishedBattle;

        private Dictionary<KPDatabaseDataSet.BattlesRow, DateTime> activeBattleList;
        private Dictionary<string, KPDatabaseDataSet.BattlesRow> activeMobBattleList;

        #endregion

        #region Public Methods/Properties
        /// <summary>
        /// Create a new database at the filename provided.
        /// </summary>
        /// <param name="newDatabaseFilename">The name of the new database to create.</param>
        public void CreateDatabase(string newDatabaseFilename)
        {
            // Close the existing one, if applicable
            CloseDatabase();

            if ((newDatabaseFilename == null) || (newDatabaseFilename == string.Empty))
            {
                // If no name provided, duplicate the default database.
                databaseFilename = defaultCopyDatabaseFilename;
            }
            else
            {
                // Create the connection string to the new database.
                databaseFilename = newDatabaseFilename;
            }

            // If the file already exists, we're going to reopen it.
            // Otherwise (or if it's the default file) create a new instance of the
            // database file from the embedded version.
            if ((File.Exists(databaseFilename) == false) || (databaseFilename == defaultCopyDatabaseFilename))
            {
                System.Reflection.Assembly a = Assembly.GetAssembly(this.GetType());
                Stream str = a.GetManifestResourceStream("WaywardGamers.KParser." +
                    Properties.Settings.Default.DBResourceName);

                if (str == null)
                    throw new ApplicationException("Cannot proceed.  Embedded database is missing.");

                int strSize = (int)str.Length;
                byte[] streamBuffer = new byte[strSize];
                str.Read(streamBuffer, 0, strSize);
                str.Close();

                using (FileStream fs = File.Create(databaseFilename, strSize))
                {
                    fs.Write(streamBuffer, 0, strSize);
                }
            }

            // Update the connection string.
            databaseConnectionString = string.Format("Data Source={0}", databaseFilename);
            Properties.Settings.Default.Properties["KPDatabaseConnectionString"].DefaultValue = databaseConnectionString;

            CreateConnections();

            InitializeNewDatabase();
        }

        public void OpenDatabase(string openDatabaseFilename)
        {
            if (File.Exists(openDatabaseFilename) == false)
                throw new ApplicationException("File does not exist.");

            // Close the existing one, if applicable
            CloseDatabase();

            databaseFilename = openDatabaseFilename;
            databaseConnectionString = string.Format("Data Source={0}", databaseFilename);

            Properties.Settings.Default.Properties["KPDatabaseConnectionString"].DefaultValue = databaseConnectionString;

            CreateConnections();
        }

        public KPDatabaseDataSet Database
        {
            get
            {
                if (localDB == null)
                    return null;

                lock (localDB)
                {
                    // Create a copy if parse is running to avoid add/read conflicts
                    if (Monitor.IsRunning)
                        return (KPDatabaseDataSet)localDB.Copy();
                    else
                        return localDB;
                }
            }
        }

        public int DatabaseVersion
        {
            get { return databaseVersion; }
        }

        public string DatabaseFilename
        {
            get
            {
                return databaseFilename;
            }
        }

        internal void ProcessNewMessages(List<Message> messageList, bool parseEnded)
        {
            if ((messageList == null) || (messageList.Count == 0))
                return;

            int count = messageList.Count;

            using (new ProfileRegion("Add messages to database"))
            {
                lock (localDB)
                {
                    foreach (var message in messageList)
                        AddMessageToDatabase(message);
                }
            }

            UpdateActiveBattleList(false);

            // Only process updates every 5 seconds, unless parse is ending.
            if (parseEnded == false)
            {
                DateTime currentTime = DateTime.Now;
                if (lastUpdateTime + updateDelayWindow > currentTime)
                    return;

                lastUpdateTime = currentTime;
            }

            KPDatabaseDataSet datasetChanges = null;
            KPDatabaseDataSet datasetCopy = null;

            try
            {
                lock (localDB)
                {
                    datasetChanges = (KPDatabaseDataSet)localDB.GetChanges();
                    datasetCopy = (KPDatabaseDataSet)localDB.Copy();
                }

                // Notify watchers so that they can view the database with
                // Row changed/inserted/deleted flags still visible
                OnDatabaseChanging(new DatabaseWatchEventArgs(datasetCopy, datasetChanges));

                UpdateDatabase();

                // Notify watchers when database has been fully updated.
                OnDatabaseChanged(new DatabaseWatchEventArgs(datasetCopy, datasetChanges));
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e);
            }
            finally
            {
                if (datasetChanges != null)
                    datasetChanges.Dispose();
                if (datasetCopy != null)
                    datasetCopy.Dispose();
            }

            if (parseEnded == true)
                DoneParsing();
        }
        #endregion

        #region Private Methods
        private void UpdateDatabase()
        {
            if ((localTAManager == null) || (localDB == null))
                return;

            try
            {
                lock (localDB)
                {
                    localTAManager.UpdateAll(localDB);
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e);
            }
        }

        private void DoneParsing()
        {
            UpdateActiveBattleList(true);

            lastKilledList.Clear();
            activeBattleList.Clear();
            activeMobBattleList.Clear();

            lastFinishedBattle = null;
        }

        public void CloseDatabase()
        {
            UpdateDatabase();

            if (localTAManager != null)
            {
                localTAManager.Dispose();
                localTAManager = null;
            }

            if (localDB != null)
            {
                localDB.Dispose();
                localDB = null;
            }

            lastKilledList.Clear();
            activeBattleList.Clear();
            activeMobBattleList.Clear();

            lastFinishedBattle = null;

        }
        #endregion

        #region Event Handling and Notification
        private void OnDatabaseChanging(DatabaseWatchEventArgs databaseWatchEventArgs)
        {
            if (DatabaseChanging != null)
            {
                // Invokes the delegates. 
                DatabaseChanging(this, databaseWatchEventArgs);
            }
        }

        private void OnDatabaseChanged(DatabaseWatchEventArgs databaseWatchEventArgs)
        {
            if (DatabaseChanged != null)
            {
                // Invokes the delegates. 
                DatabaseChanged(this, databaseWatchEventArgs);
            }
        }
        #endregion

        #region Initialization Methods
        /// <summary>
        /// Set up the SQL connections for the table adapters when creating/opening
        /// a database.
        /// </summary>
        private void CreateConnections()
        {
            localDB = new KPDatabaseDataSet();
            localTAManager = new TableAdapterManager();

            localTAManager.CombatantsTableAdapter = new CombatantsTableAdapter();
            localTAManager.BattlesTableAdapter = new BattlesTableAdapter();
            localTAManager.InteractionsTableAdapter = new InteractionsTableAdapter();
            localTAManager.ActionsTableAdapter = new ActionsTableAdapter();
            localTAManager.LootTableAdapter = new LootTableAdapter();
            localTAManager.ItemsTableAdapter = new ItemsTableAdapter();
            localTAManager.RecordLogTableAdapter = new RecordLogTableAdapter();
            localTAManager.VersionTableAdapter = new VersionTableAdapter();
            localTAManager.ChatMessagesTableAdapter = new ChatMessagesTableAdapter();
            localTAManager.ChatSpeakersTableAdapter = new ChatSpeakersTableAdapter();


            System.Data.SqlServerCe.SqlCeConnection sqlConn =
                new System.Data.SqlServerCe.SqlCeConnection(databaseConnectionString);

            localTAManager.Connection = sqlConn;


            localTAManager.CombatantsTableAdapter.Connection = sqlConn;
            localTAManager.BattlesTableAdapter.Connection = sqlConn;
            localTAManager.InteractionsTableAdapter.Connection = sqlConn;
            localTAManager.ActionsTableAdapter.Connection = sqlConn;
            localTAManager.LootTableAdapter.Connection = sqlConn;
            localTAManager.ItemsTableAdapter.Connection = sqlConn;
            localTAManager.ChatMessagesTableAdapter.Connection = sqlConn;
            localTAManager.ChatSpeakersTableAdapter.Connection = sqlConn;
            localTAManager.RecordLogTableAdapter.Connection = sqlConn;
            localTAManager.VersionTableAdapter.Connection = sqlConn;


            // If opening an existing database, need to check version info before filling data

            localTAManager.CombatantsTableAdapter.Fill(localDB.Combatants);
            localTAManager.BattlesTableAdapter.Fill(localDB.Battles);
            localTAManager.InteractionsTableAdapter.Fill(localDB.Interactions);
            localTAManager.ActionsTableAdapter.Fill(localDB.Actions);
            localTAManager.LootTableAdapter.Fill(localDB.Loot);
            localTAManager.ItemsTableAdapter.Fill(localDB.Items);
            localTAManager.ChatMessagesTableAdapter.Fill(localDB.ChatMessages);
            localTAManager.ChatSpeakersTableAdapter.Fill(localDB.ChatSpeakers);
            localTAManager.RecordLogTableAdapter.Fill(localDB.RecordLog);
            localTAManager.VersionTableAdapter.Fill(localDB.Version);
        }

        /// <summary>
        /// Inserts a couple required pieces of information into newly-created databases.
        /// </summary>
        private void InitializeNewDatabase()
        {
            // Insert version information
            if (localDB.Version.Rows.Count == 0)
                localDB.Version.AddVersionRow(databaseVersion, assemblyVersionString);

            // Insert default battle row
            if (localDB.Battles.Rows.Count == 0)
                localDB.Battles.AddBattlesRow(null, DateTime.Now, DateTime.Now, false, null, 0, 0, 0, 0, true);

            UpdateDatabase();
        }
        #endregion

        #region Methods for inserting message information
        /// <summary>
        /// Insert raw (pre-parsed) message lines to the database for consistency.
        /// </summary>
        /// <param name="messageLine"></param>
        internal void AddMessageToRecordLog(MessageLine messageLine)
        {
            // Set ParseSuccessful to False at this point since we don't know the parse outcome.
            lock (localDB)
            {
                if (messageLine != null)
                    localDB.RecordLog.AddRecordLogRow(messageLine.Timestamp, messageLine.OriginalText, false);
            }
        }

        /// <summary>
        /// Takes a message object and inserts all its various values into
        /// the appropriate locations within the database.
        /// </summary>
        /// <param name="message">The message to add to the database.</param>
        private void AddMessageToDatabase(Message message)
        {
            // Don't try to insert data from unsuccessful parses.
            if (message.ParseSuccessful == false)
            {
                return;
            }

            // For successful parses, update the RecordLog table entries for each messageline in the
            // message.
            foreach (var msgLine in message.MessageLineCollection)
            {
                var recordRow = localDB.RecordLog.Where(t => t.Timestamp == msgLine.Timestamp)
                    .Where(t => t.MessageText == msgLine.OriginalText).SingleOrDefault();

                if (recordRow != null)
                    recordRow.ParseSuccessful = true;
            }

            // Call functions depending on type of message.
            switch (message.MessageCategory)
            {
                case MessageCategoryType.Chat:
                    InsertChat(message);
                    break;
                case MessageCategoryType.Event:
                    // If this is a loot or experience message it needs to be attached to the most recently killed battle.
                    switch (message.EventDetails.EventMessageType)
                    {
                        case EventMessageType.Experience:
                            InsertExperience(message);
                            break;
                        case EventMessageType.Loot:
                            InsertLoot(message);
                            break;
                        case EventMessageType.Interaction:
                            InsertCombat(message);
                            break;
                    }
                    // Other message types (fishing, crafting, etc) are ignored.
                    break;
                case MessageCategoryType.System:
                    // Ignore system messages
                    break;
            }
        }

        private void UpdateActiveBattleList(bool closeOutAllBattles)
        {
            lock (activeBattleList)
            {
                // When closing database, close out all remaining active battles.
                var oldBattles = activeBattleList.Where(b => true);

                // Search for any battles in our active list that haven't had any
                // activity in the last 10 minutes.  Close them out.
                if (closeOutAllBattles == false)
                {
                    DateTime tenMinutesAgo = DateTime.Now.Subtract(TimeSpan.FromMinutes(10));
                    oldBattles = activeBattleList.Where(b => b.Value < tenMinutesAgo);
                }

                // Cannot use a foreach loop here since modifying the battlerow endtime
                // causes the enumeration to break.
                while (oldBattles.Count() > 0)
                {
                    oldBattles.First().Key.EndTime = DateTime.Now;
                    activeBattleList.Remove(oldBattles.First().Key);
                    //oldBattles = oldBattles.Skip(1);
                }
            }
        }

        /// <summary>
        /// Add chat messages to database.
        /// </summary>
        /// <param name="message">Chat message to add.</param>
        private void InsertChat(Message message)
        {
            var chatSpeakerRow = localDB.ChatSpeakers.GetSpeaker(message.ChatDetails.ChatSpeakerName);

            localDB.ChatMessages.AddChatMessagesRow(message.Timestamp, (byte)message.ChatDetails.ChatMessageType,
                 chatSpeakerRow, message.CompleteMessageText);
        }

        /// <summary>
        /// Update battles with experience info.
        /// </summary>
        /// <param name="message">The message containing experience data.</param>
        private void InsertExperience(Message message)
        {
            if ((lastFinishedBattle == null) || (lastFinishedBattle.ExperiencePoints != 0))
                lastFinishedBattle = localDB.Battles.AddBattlesRow(null,
                    message.Timestamp, message.Timestamp, true,
                    null, (byte)ActorType.Unknown,
                    0, 0, // XP Points & Chain
                    (byte)MobDifficulty.Unknown, false);

            lastFinishedBattle.ExperiencePoints = message.EventDetails.ExperienceDetails.ExperiencePoints;
            lastFinishedBattle.ExperienceChain = message.EventDetails.ExperienceDetails.ExperienceChain;
        }

        /// <summary>
        /// Subset code for inserting loot table information.
        /// </summary>
        /// <param name="message">The message containing loot data.</param>
        private void InsertLoot(Message message)
        {
            // Messages for when items are found on mob.
            if (message.EventDetails.LootDetails.IsFoundMessage == true)
            {
                // Check our local list of recent kills
                KPDatabaseDataSet.BattlesRow lastKill = null;

                if (lastKilledList.TryGetValue(message.EventDetails.LootDetails.MobName, out lastKill) == false)
                {
                    // No record of the last kill for this mob type; create a
                    // new battle record for it.

                    // First locate the mob in the combatants table
                    var mobCombatant = localDB.Combatants.GetCombatant(message.EventDetails.LootDetails.MobName, EntityType.Mob);

                    lastKill = localDB.Battles.AddBattlesRow(mobCombatant, message.Timestamp,
                        message.Timestamp, true, null, (byte)ActorType.Unknown, 0, 0,
                        (byte)MobDifficulty.Unknown, false);

                    lastKilledList[message.EventDetails.LootDetails.MobName] = lastKill;
                }

                // If last kill is more than 5 minutes 30 seconds ago (30 sec buffer for
                // default drop time), create a new battle instead.
                if (lastKill.EndTime < (message.Timestamp.Subtract(TimeSpan.FromSeconds(330))))
                {
                    // First locate the mob in the combatants table
                    var mobCombatant = localDB.Combatants.GetCombatant(message.EventDetails.LootDetails.MobName, EntityType.Mob);

                    lastKill = localDB.Battles.AddBattlesRow(mobCombatant, message.Timestamp,
                        message.Timestamp, true, null, (byte)ActorType.Unknown, 0, 0,
                        (byte)MobDifficulty.Unknown, false);

                    lastKilledList[message.EventDetails.LootDetails.MobName] = lastKill;
                }

                // Locate the item by name in the item table.
                var itemRow = localDB.Items.GetItem(message.EventDetails.LootDetails.ItemName);

                // Add the entry to the loot table.
                localDB.Loot.AddLootRow(itemRow, lastKill, null, 0, false);
            }
            else
            {
                // Messages for when items or gil are distributed to players.

                if (message.EventDetails.LootDetails.Gil == 0)
                {
                    // handle item drops

                    // Locate the item in the item names table.
                    var itemRow = localDB.Items.GetItem(message.EventDetails.LootDetails.ItemName);

                    // Get an array of all loot entries for this item.
                    var lootEntries = itemRow.GetLootRows();
                    KPDatabaseDataSet.LootRow lootEntry = null;

                    if (lootEntries != null)
                    {
                        // If there are any loot entries of the given type, search for an unclaimed one
                        lootEntry = lootEntries.FirstOrDefault(l => l.Lost == false && l.CombatantsRow == null);
                    }

                    // If there is no (free) loot entry, create a new one.
                    if (lootEntry == null)
                        lootEntry = localDB.Loot.AddLootRow(itemRow, null, null, 0, false);

                    // Update table based on whether someone got the drop or if it was lost
                    if (message.EventDetails.LootDetails.WasLost == true)
                    {
                        // Update Lost value of the entry
                        lootEntry.Lost = true;
                    }
                    else
                    {
                        // Ok, someone got the drop.  Check for WhoObtained;
                        // if it's empty, this is a bogus message (eg: warning when
                        // player isn't qualified to receive drop).
                        if (message.EventDetails.LootDetails.WhoObtained != string.Empty)
                        {
                            var player = localDB.Combatants.GetCombatant(message.EventDetails.LootDetails.WhoObtained, EntityType.Player);

                            lootEntry.CombatantsRow = player;
                        }
                    }
                }
                else
                {
                    // handle gil drops

                    // If no record of the last kill for this mob type, we cannot create a new one
                    // because we have no mob name.

                    if (lastFinishedBattle != null)
                    {
                        // Get the "Gil" item from the items table (created if necessary).
                        var itemRow = localDB.Items.GetItem("Gil");

                        if (message.EventDetails.LootDetails.WhoObtained != string.Empty)
                        {
                            var player = localDB.Combatants.GetCombatant(message.EventDetails.LootDetails.WhoObtained, EntityType.Player);
                            localDB.Loot.AddLootRow(itemRow, lastFinishedBattle, player, message.EventDetails.LootDetails.Gil, false);
                        }
                        else
                        {
                            localDB.Loot.AddLootRow(itemRow, lastFinishedBattle, null, message.EventDetails.LootDetails.Gil, false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Subset code for inserting combat detail information.
        /// </summary>
        /// <param name="message">The message containing combat data.</param>
        private void InsertCombat(Message message)
        {
            // Handle death events separately
            if (message.EventDetails.CombatDetails.InteractionType == InteractionType.Death)
            {
                ProcessDeath(message);
                return;
            }

            KPDatabaseDataSet.CombatantsRow actor = null;
            KPDatabaseDataSet.BattlesRow battle = null;
            KPDatabaseDataSet.ActionsRow action = null;
            KPDatabaseDataSet.ActionsRow secondAction = null;

            // Get the actor combatant, if any.
            if (message.EventDetails.CombatDetails.ActorName != string.Empty)
                actor = localDB.Combatants.GetCombatant(message.EventDetails.CombatDetails.ActorName,
                    message.EventDetails.CombatDetails.ActorEntityType);

            // Get the action row, if any is applicable to the message.
            if (message.EventDetails.CombatDetails.ActionName != string.Empty)
                action = localDB.Actions.GetAction(message.EventDetails.CombatDetails.ActionName);

            // Bogus target for passing in data on incomplete messages.
            TargetDetails bogusTarget = message.EventDetails.CombatDetails.Targets.SingleOrDefault(t => t.Name == "");

            // Get the battle (if any) this interaction is associated with.
            if ((message.EventDetails.CombatDetails.Targets.Count == 0) || (bogusTarget != null))
            {
                // No targets, so preparing a move

                if (message.EventDetails.CombatDetails.ActorEntityType == EntityType.Mob)
                {
                    // If a mob is taking action, look it up in the battle list, or
                    // create a new battle for it.

                    if (activeMobBattleList.TryGetValue(message.EventDetails.CombatDetails.ActorName, out battle) == false)
                    {
                        battle = localDB.Battles.AddBattlesRow(actor, message.Timestamp,
                            MagicNumbers.MinSQLDateTime, false, null, 0, 0, 0, (byte)MobDifficulty.Unknown, false);

                        activeMobBattleList[message.EventDetails.CombatDetails.ActorName] = battle;
                    }
                }
                else
                {
                    // If a player is taking action, put this in the most
                    // recent active battle.
                    battle = MostRecentActiveBattle();
                }
            }
            else
            {
                // Ok, in this case there are targets
                if (message.EventDetails.CombatDetails.ActorEntityType == EntityType.Mob)
                {
                    // If there is none, create a new battle.
                    // If it's one mob buffing another, assume this is a link and must be
                    // considered the start of a new battle.
                    if (activeMobBattleList.TryGetValue(message.EventDetails.CombatDetails.ActorName, out battle) == false)
                    {
                        battle = localDB.Battles.AddBattlesRow(actor, message.Timestamp,
                            MagicNumbers.MinSQLDateTime, false, null, 0, 0, 0, (byte)MobDifficulty.Unknown, false);

                        activeMobBattleList[message.EventDetails.CombatDetails.ActorName] = battle;
                    }
                }
                else if (actor == null)
                {
                    // No actor specified (eg: shadows absorb attack)
                    // Place in most recent battle.
                    battle = MostRecentActiveBattle();
                }
            }

            if (battle != null)
            {
                // Update the most recent activity record for this fight.
                lock (activeBattleList)
                    activeBattleList[battle] = message.Timestamp;
            }

            if (message.EventDetails.CombatDetails.Targets.Count == 0)
            {
                localDB.Interactions.AddInteractionsRow(
                    message.Timestamp,
                    actor,
                    null, // no target
                    battle,
                    (byte)message.EventDetails.CombatDetails.ActorType,
                    message.EventDetails.CombatDetails.IsPreparing,
                    action,
                    (byte)message.EventDetails.CombatDetails.ActionType,
                    (byte)message.EventDetails.CombatDetails.FailedActionType,
                    (byte)DefenseType.None,
                    0,
                    (byte)message.EventDetails.CombatDetails.AidType,
                    (byte)RecoveryType.None,
                    (byte)message.EventDetails.CombatDetails.HarmType,
                    0,
                    (byte)DamageModifier.None,
                    (byte)AidType.None,
                    (byte)RecoveryType.None,
                    (byte)HarmType.None,
                    0,
                    secondAction);

            }
            else if (bogusTarget != null)
            {
                var targetRow = battle.CombatantsRowByEnemyCombatantRelation;

                localDB.Interactions.AddInteractionsRow(
                    message.Timestamp,
                    actor,
                    targetRow,
                    battle,
                    (byte)message.EventDetails.CombatDetails.ActorType,
                    message.EventDetails.CombatDetails.IsPreparing,
                    action,
                    (byte)message.EventDetails.CombatDetails.ActionType,
                    (byte)message.EventDetails.CombatDetails.FailedActionType,
                    (byte)bogusTarget.DefenseType,
                    0,
                    (byte)message.EventDetails.CombatDetails.AidType,
                    (byte)RecoveryType.None,
                    (byte)message.EventDetails.CombatDetails.HarmType,
                    0,
                    (byte)DamageModifier.None,
                    (byte)AidType.None,
                    (byte)RecoveryType.None,
                    (byte)HarmType.None,
                    0,
                    secondAction);
            }
            else
            {

                foreach (var target in message.EventDetails.CombatDetails.Targets)
                {
                    // Get database row for target combatant.
                    KPDatabaseDataSet.CombatantsRow targetRow = null;
                    if (target.Name != null)
                        targetRow = localDB.Combatants.GetCombatant(target.Name, target.EntityType);

                    if (target.SecondaryAction != string.Empty)
                        secondAction = localDB.Actions.GetAction(target.SecondaryAction);

                    // Get the battle each time through the loop if the targets are mobs.
                    if (target.EntityType == EntityType.Mob)
                    {
                        if (activeMobBattleList.TryGetValue(target.Name, out battle) == false)
                        {
                            battle = localDB.Battles.AddBattlesRow(targetRow, message.Timestamp,
                                MagicNumbers.MinSQLDateTime, false, null, 0, 0, 0, (byte)MobDifficulty.Unknown, false);

                            activeMobBattleList[target.Name] = battle;
                        }

                        // Update the most recent activity record for the fight.
                        lock (activeBattleList)
                            activeBattleList[battle] = message.Timestamp;
                    }

                    // Special double-entry for counters
                    if (message.EventDetails.CombatDetails.ActionType == ActionType.Counterattack)
                    {
                        // Initial attack is stopped by counter.
                        localDB.Interactions.AddInteractionsRow(
                           message.Timestamp,
                           targetRow, // swap with actor
                           actor, // swap with target
                           battle,
                           (byte)ActorType.Unknown,
                           message.EventDetails.CombatDetails.IsPreparing,
                           action,
                           (byte)ActionType.Melee, // original attack -must- be melee to be countered
                           (byte)target.FailedActionType,
                           (byte)DefenseType.Counter, // defense type, of course
                           0, // no shadows used here
                           (byte)target.AidType,
                           (byte)target.RecoveryType,
                           (byte)target.HarmType,
                           0, // no damage done
                           (byte)target.DamageModifier,
                           (byte)target.SecondaryAidType,
                           (byte)target.SecondaryRecoveryType,
                           (byte)target.SecondaryHarmType,
                           target.SecondaryAmount,
                           secondAction);

                        // The attack that the combatant that countered got
                        localDB.Interactions.AddInteractionsRow(
                            message.Timestamp,
                            actor,
                            targetRow,
                            battle,
                            (byte)message.EventDetails.CombatDetails.ActorType,
                            message.EventDetails.CombatDetails.IsPreparing,
                            action,
                            (byte)message.EventDetails.CombatDetails.ActionType,
                            (byte)target.FailedActionType,
                            (byte)target.DefenseType,
                            target.ShadowsUsed,
                            (byte)target.AidType,
                            (byte)target.RecoveryType,
                            (byte)target.HarmType,
                            target.Amount,
                            (byte)target.DamageModifier,
                            (byte)target.SecondaryAidType,
                            (byte)target.SecondaryRecoveryType,
                            (byte)target.SecondaryHarmType,
                            target.SecondaryAmount,
                            secondAction);
                    }
                    else
                    {
                        localDB.Interactions.AddInteractionsRow(
                            message.Timestamp,
                            actor,
                            targetRow,
                            battle,
                            (byte)message.EventDetails.CombatDetails.ActorType,
                            message.EventDetails.CombatDetails.IsPreparing,
                            action,
                            (byte)message.EventDetails.CombatDetails.ActionType,
                            (byte)target.FailedActionType,
                            (byte)target.DefenseType,
                            target.ShadowsUsed,
                            (byte)target.AidType,
                            (byte)target.RecoveryType,
                            (byte)target.HarmType,
                            target.Amount,
                            (byte)target.DamageModifier,
                            (byte)target.SecondaryAidType,
                            (byte)target.SecondaryRecoveryType,
                            (byte)target.SecondaryHarmType,
                            target.SecondaryAmount,
                            secondAction);
                    }
                }
            }
        }

        private void ProcessDeath(Message message)
        {
            KPDatabaseDataSet.CombatantsRow actor = null;
            KPDatabaseDataSet.BattlesRow battle = null;

            // If there are no targets, we can't do anything.
            if ((message.EventDetails.CombatDetails.Targets == null) || (message.EventDetails.CombatDetails.Targets.Count == 0))
                return;

            // If no actor name given, target just "fell to the ground".
            if (message.EventDetails.CombatDetails.ActorName != string.Empty)
            {
                // Determine the acting combatant for this event
                actor = localDB.Combatants.GetCombatant(message.EventDetails.CombatDetails.ActorName,
                    message.EventDetails.CombatDetails.ActorEntityType);
            }


            // Record an interaction entry for each death
            foreach (var target in message.EventDetails.CombatDetails.Targets)
            {
                var targetRow = localDB.Combatants.GetCombatant(target.Name, target.EntityType);

                if (target.EntityType == EntityType.Mob)
                {
                    // If target is mob, pick that battle
                    if (activeMobBattleList.TryGetValue(target.Name, out battle) == true)
                    {
                        // close out this battle
                        battle.EndTime = message.Timestamp;
                        battle.Killed = true;
                        battle.CombatantsRowByBattleKillerRelation = actor;
                        battle.KillerPlayerType = (byte)message.EventDetails.CombatDetails.ActorType;

                        activeMobBattleList.Remove(target.Name);
                        lock (activeBattleList)
                            activeBattleList.Remove(battle);
                    }
                    else
                    {
                        // Make a new one if one doesn't exist, created already killed
                        battle = localDB.Battles.AddBattlesRow(actor, message.Timestamp, message.Timestamp,
                            true, actor, (byte)message.EventDetails.CombatDetails.ActorType, 0, 0,
                            (byte)MobDifficulty.Unknown, false);
                    }

                    lastKilledList[target.Name] = battle;
                    lastFinishedBattle = battle;
                }
                else
                {
                    // If target is player, find the battle to put this in
                    if (actor == null)
                    {
                        // If no actor, get the most recent battle
                        battle = MostRecentActiveBattle();
                    }
                    else
                    {
                        // Otherwise get the actor's battle
                        if (activeMobBattleList.TryGetValue(actor.CombatantName, out battle) == false)
                        {
                            // If they're not in the active battle list, create a new one.
                            battle = localDB.Battles.AddBattlesRow(actor, message.Timestamp,
                                MagicNumbers.MinSQLDateTime, false, null, 0, 0, 0,
                                (byte)MobDifficulty.Unknown, false);

                            activeMobBattleList.Add(actor.CombatantName, battle);
                            activeBattleList.Add(battle, message.Timestamp);
                        }
                    }
                }

                // Make a note of the death event in the interactions table
                localDB.Interactions.AddInteractionsRow(
                       message.Timestamp,
                       actor,
                       targetRow,
                       battle,
                       (byte)ActorType.Unknown,
                       message.EventDetails.CombatDetails.IsPreparing,
                       null,
                       (byte)message.EventDetails.CombatDetails.ActionType,
                       (byte)message.EventDetails.CombatDetails.FailedActionType,
                       (byte)target.DefenseType,
                       target.ShadowsUsed,
                       (byte)message.EventDetails.CombatDetails.AidType,
                       (byte)target.RecoveryType,
                       (byte)message.EventDetails.CombatDetails.HarmType,
                       target.Amount,
                       (byte)target.DamageModifier,
                       (byte)target.SecondaryAidType,
                       (byte)target.SecondaryRecoveryType,
                       (byte)target.SecondaryHarmType,
                       target.SecondaryAmount,
                       null);
            }
        }

        private KPDatabaseDataSet.BattlesRow MostRecentActiveBattle()
        {
            if (activeBattleList.Count == 0)
                return localDB.Battles.GetDefaultBattle();

            // find the most recent non-default battle.
            KPDatabaseDataSet.BattlesRow mostRecentBattle;
            lock (activeBattleList)
                mostRecentBattle = (KPDatabaseDataSet.BattlesRow)activeBattleList
                    .Where(ab => ab.Key.DefaultBattle == false)
                    .MaxEntry(ab => ab.Value, ab => ab.Key);

            if (mostRecentBattle == null)
                mostRecentBattle = localDB.Battles.GetDefaultBattle();

            return mostRecentBattle;
        }

        #endregion

        public void Reset()
        {
            throw new NotImplementedException();
        }

    }
}
