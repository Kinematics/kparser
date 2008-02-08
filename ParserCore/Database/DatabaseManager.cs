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
        KPDatabaseDataSet fullDataset;
        KPDatabaseDataSet changedDataset;

        /// <summary>
        /// Constructor is internal; only created by the DatabaseManager.
        /// </summary>
        /// <param name="coreDataset"></param>
        internal DatabaseWatchEventArgs(KPDatabaseDataSet coreDataset)
        {
            if (coreDataset == null)
            {
                fullDataset = null;
                changedDataset = null;
                return;
            }

            fullDataset = coreDataset;

            changedDataset = (KPDatabaseDataSet)coreDataset.GetChanges();
        }

        /// <summary>
        /// Gets the dataset containing all changes that are about to be
        /// committed to the database, for the Changing event.
        /// </summary>
        public KPDatabaseDataSet DatasetChanges
        {
            get
            {
                return changedDataset;
            }
        }

        public KPDatabaseDataSet FullDataset
        {
            get
            {
                return fullDataset;
            }
        }

    }
    #endregion

    public class DatabaseManager : IDisposable
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
                UpdateDatabase();
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

        public event DatabaseWatchEventHandler DatabaseChanging;
        public event DatabaseWatchEventHandler DatabaseChanged;

        private bool disposed = false;
        // For diagnostics:
        private Stopwatch stopwatch = new Stopwatch();

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
                return localDB;
            }
        }

        public int DatabaseVersion
        {
            get { return databaseVersion; }
        }

        internal void ProcessNewMessages(List<Message> messageList, bool parseEnded)
        {
            if ((messageList == null) || (messageList.Count == 0))
                return;

            int count = messageList.Count;

            foreach (var message in messageList)
                AddMessageToDatabase(message);

            UpdateActiveBattleList(false);

            stopwatch.Reset();
            stopwatch.Start();

            // Notify watchers so that they can view the database with
            // Row changed/inserted/deleted flags still visible
            OnDatabaseChanging(new DatabaseWatchEventArgs(localDB));

            stopwatch.Stop();
            Debug.WriteLine(string.Format("Processing {0} messages in all plugins took {1}.", count, stopwatch.Elapsed));


            UpdateDatabase();

            // Notify watchers when database has been fully updated.
            OnDatabaseChanged(new DatabaseWatchEventArgs(localDB));

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
                localTAManager.UpdateAll(localDB);
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

        private void CloseDatabase()
        {
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
        }
        #endregion

        #region Event Handling and Notification
        protected virtual void OnDatabaseChanged(DatabaseWatchEventArgs databaseWatchEventArgs)
        {
            if (DatabaseChanged != null)
            {
                // Invokes the delegates. 
                DatabaseChanged(this, databaseWatchEventArgs);
            }
        }

        protected virtual void OnDatabaseChanging(DatabaseWatchEventArgs databaseWatchEventArgs)
        {
            if (DatabaseChanging != null)
            {
                // Invokes the delegates. 
                DatabaseChanging(this, databaseWatchEventArgs);
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
        /// Takes a message object and inserts all its various values into
        /// the appropriate locations within the database.
        /// </summary>
        /// <param name="message">The message to add to the database.</param>
        private void AddMessageToDatabase(Message message)
        {
            // First add all messages to the log record table.
            foreach (var msgLine in message.MessageLineCollection)
                localDB.RecordLog.AddRecordLogRow(msgLine.Timestamp, msgLine.OriginalText, message.ParseSuccessful);

            // Don't try to insert data from unsuccessful parses.
            if (message.ParseSuccessful == false)
            {
                return;
            }

            // Ignore system messages
            if (message.MessageCategory == MessageCategoryType.System)
            {
                return;
            }

            // Add chat messages to chat table, update speakers table.
            if (message.MessageCategory == MessageCategoryType.Chat)
            {
                var chatSpeakerRow = localDB.ChatSpeakers.SingleOrDefault(sp => sp.SpeakerName == message.ChatDetails.ChatSpeakerName);
                if (chatSpeakerRow == null)
                    chatSpeakerRow = localDB.ChatSpeakers.AddChatSpeakersRow(message.ChatDetails.ChatSpeakerName);

                localDB.ChatMessages.AddChatMessagesRow(message.Timestamp, (byte)message.ChatDetails.ChatMessageType,
                     chatSpeakerRow, message.CompleteMessageText);

                return;
            }


            // From here on are action messages.
            if (message.MessageCategory == MessageCategoryType.Event)
            {

                // If this is a loot or experience message it needs to be attached to the most recently killed battle.

                if (message.EventDetails.EventMessageType == EventMessageType.Experience)
                {
                    InsertExperience(message);
                    return;
                }

                if (message.EventDetails.EventMessageType == EventMessageType.Loot)
                {
                    InsertLoot(message);
                    return;
                }

                if (message.EventDetails.EventMessageType == EventMessageType.Interaction)
                {
                    InsertCombat(message);
                    return;
                }

                // Other message types (fishing, crafting, etc) are ignored.
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
        /// Subset code for adding experience info to battles.
        /// </summary>
        /// <param name="message">The message containing experience data.</param>
        private void InsertExperience(Message message)
        {
            if ((lastFinishedBattle == null) || (lastFinishedBattle.ExperiencePoints != 0))
                lastFinishedBattle = localDB.Battles.AddBattlesRow(null, message.Timestamp,
                    message.Timestamp, true, null, 0, 0, 0, (byte)MobDifficulty.Unknown, false);

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

                if (lastKilledList.Keys.Contains(message.EventDetails.LootDetails.MobName))
                    lastKill = lastKilledList[message.EventDetails.LootDetails.MobName];
                
                if (lastKill == null)
                {
                    // No record of the last kill for this mob type; create a
                    // new battle record for it.

                    // First locate the mob in the combatants table
                    var mobCombatant = localDB.Combatants.GetCombatant(message.EventDetails.LootDetails.MobName, EntityType.Mob);

                    lastKill = localDB.Battles.AddBattlesRow(mobCombatant, message.Timestamp,
                        message.Timestamp, true, null, 0, 0, 0, (byte)MobDifficulty.Unknown, false);

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

                    // If there are any loot entries of the given type, search for an unclaimed one
                    if (lootEntries != null)
                    {
                        // Of that list, find the last one where no one has claimed it and it hasn't been lost
                        lootEntry = localDB.Loot.LastOrDefault(l => l.Lost == false && l.CombatantsRow == null);
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

            // If this is a buff/unknown message, it should be attached to the current active battle.
            // If there are no active battles, attach it to the default battle.

            // If this is a death message, it needs to be attached to the active battle against
            // a mob of the appropriate name.  If there is no such active battle, create one solely
            // to close it out.

            KPDatabaseDataSet.CombatantsRow actor = null;
            KPDatabaseDataSet.BattlesRow battle = null;
            KPDatabaseDataSet.ActionsRow action = null;


            switch (message.EventDetails.CombatDetails.InteractionType)
            {
                #region Attack
                case InteractionType.Harm:
                    if (message.EventDetails.CombatDetails.HarmType == HarmType.Death)
                    {
                        ProcessDeath(message);
                        return;
                    }

                    actor = localDB.Combatants.GetCombatant(message.EventDetails.CombatDetails.ActorName,
                        message.EventDetails.CombatDetails.ActorEntityType);

                    // Get the action row, if any is applicable to the message.
                    action = localDB.Actions.GetAction(message.EventDetails.CombatDetails.ActionName);

                    // If preparing, only the primary actor and battle is of consequence.
                    if (message.EventDetails.CombatDetails.IsPreparing == true)
                    {
                        if (message.EventDetails.CombatDetails.ActorEntityType == EntityType.Mob)
                        {
                            battle = activeMobBattleList[message.EventDetails.CombatDetails.ActorName];

                            if (battle == null)
                            {
                                battle = localDB.Battles.AddBattlesRow(actor, message.Timestamp,
                                    MagicNumbers.MinSQLDateTime, false, null, 0, 0, 0, (byte)MobDifficulty.Unknown, false);

                                activeMobBattleList[message.EventDetails.CombatDetails.ActorName] = battle;
                            }
                        }
                        else
                        {
                            battle = MostRecentActiveBattle();
                        }

                        lock (activeBattleList)
                            activeBattleList[battle] = message.Timestamp;

                        localDB.Interactions.AddInteractionsRow(message.Timestamp,
                            actor,
                            null,
                            battle,
                            (byte)ActorType.Unknown,
                            message.EventDetails.CombatDetails.IsPreparing,
                            action,
                            (byte)message.EventDetails.CombatDetails.ActionSource,
                            (byte)message.EventDetails.CombatDetails.FailedActionType,
                            (byte)DefenseType.None, 0,
                            (byte)message.EventDetails.CombatDetails.AidType,
                            (byte)RecoveryType.None,
                            (byte)message.EventDetails.CombatDetails.HarmType,
                            0,
                            (byte)DamageModifier.None,
                            (byte)AidType.Unknown,
                            (byte)RecoveryType.None,
                            (byte)HarmType.Unknown,
                            0);

                        return;
                    }

                    // Not preparing, so casting/action is being completed.

                    // If mob is acting, get any current battle with this mob type
                    if (message.EventDetails.CombatDetails.ActorEntityType == EntityType.Mob)
                    {
                        if (activeMobBattleList.Keys.Contains(message.EventDetails.CombatDetails.ActorName))
                            battle = activeMobBattleList[message.EventDetails.CombatDetails.ActorName];

                        // If there is none, create a new battle.
                        if (battle == null)
                        {
                            battle = localDB.Battles.AddBattlesRow(actor, message.Timestamp,
                                MagicNumbers.MinSQLDateTime, false, null, 0, 0, 0, (byte)MobDifficulty.Unknown, false);

                            activeMobBattleList[message.EventDetails.CombatDetails.ActorName] = battle;
                        }
                        
                        // Update the most recent activity record for this fight.
                        lock (activeBattleList)
                            activeBattleList[battle] = message.Timestamp;

                        // Add combat detail entries for each player in this message.

                        //  Create an entry for each target
                        foreach (var target in message.EventDetails.CombatDetails.Targets)
                        {
                            var targetRow = localDB.Combatants.GetCombatant(target.Name, target.EntityType);

                            localDB.Interactions.AddInteractionsRow(
                                message.Timestamp,
                                actor,
                                targetRow,
                                battle,
                                (byte)ActorType.Unknown,
                                message.EventDetails.CombatDetails.IsPreparing,
                                action,
                                (byte)message.EventDetails.CombatDetails.ActionSource,
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
                                target.SecondaryAmount);
                        }
                    }
                    else
                    {
                        // If player or pet is acting, need to find the battle record for each target.

                        foreach (var target in message.EventDetails.CombatDetails.Targets)
                        {
                            var targetRow = localDB.Combatants.GetCombatant(target.Name, EntityType.Mob);

                            if (activeMobBattleList.Keys.Contains(target.Name))
                            {
                                battle = activeMobBattleList[target.Name];
                            }
                            else
                            {
                                battle = localDB.Battles.AddBattlesRow(targetRow, message.Timestamp,
                                    MagicNumbers.MinSQLDateTime, false, null, 0, 0, 0, (byte)MobDifficulty.Unknown, false);

                                activeMobBattleList[target.Name] = battle;
                            }

                            // Update the most recent activity record for this fight.
                            lock (activeBattleList)
                                activeBattleList[battle] = message.Timestamp;

                            localDB.Interactions.AddInteractionsRow(
                                message.Timestamp,
                                actor,
                                targetRow,
                                battle,
                                (byte)ActorType.Unknown,
                                message.EventDetails.CombatDetails.IsPreparing,
                                action,
                                (byte)message.EventDetails.CombatDetails.ActionSource,
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
                                target.SecondaryAmount);

                        }
                    }
                    break;
                #endregion
                #region Buff
                case InteractionType.Aid:
                    actor = localDB.Combatants.GetCombatant(message.EventDetails.CombatDetails.ActorName,
                        message.EventDetails.CombatDetails.ActorEntityType);

                    // Get the action row, if any is applicable to the message.
                    action = localDB.Actions.GetAction(message.EventDetails.CombatDetails.ActionName);

                    if (message.EventDetails.CombatDetails.IsPreparing == true)
                    {
                        // If preparing, only the primary actor is of consequence.

                        if (message.EventDetails.CombatDetails.ActorEntityType == EntityType.Mob)
                        {
                            if (activeMobBattleList.Keys.Contains(message.EventDetails.CombatDetails.ActorName))
                                battle = activeMobBattleList[message.EventDetails.CombatDetails.ActorName];
                            else
                                battle = localDB.Battles.GetDefaultBattle();
                        }
                        else
                        {
                            battle = MostRecentActiveBattle();
                        }

                        lock (activeBattleList)
                            activeBattleList[battle] = message.Timestamp;

                        if (message.EventDetails.CombatDetails.ActorEntityType == EntityType.Mob)
                            activeMobBattleList[message.EventDetails.CombatDetails.ActorName] = battle;

                        localDB.Interactions.AddInteractionsRow(
                            message.Timestamp,
                            actor,
                            null,
                            battle,
                            (byte)ActorType.Unknown,
                            message.EventDetails.CombatDetails.IsPreparing,
                            action,
                            (byte)message.EventDetails.CombatDetails.ActionSource,
                            (byte)message.EventDetails.CombatDetails.FailedActionType,
                            (byte)DefenseType.None,
                            0,
                            (byte)message.EventDetails.CombatDetails.AidType,
                            (byte)RecoveryType.None,
                            (byte)message.EventDetails.CombatDetails.HarmType,
                            0,
                            (byte)DamageModifier.None,
                            (byte)AidType.Unknown,
                            (byte)RecoveryType.None,
                            (byte)HarmType.Unknown,
                            0);

                        return;
                    }

                    // Not preparing, so casting is being completed.

                    if (message.EventDetails.CombatDetails.ActorEntityType == EntityType.Mob)
                    {
                        // look for an active battle record with either acting or target mob
                        if (activeMobBattleList.Keys.Contains(message.EventDetails.CombatDetails.ActorName))
                            battle = activeMobBattleList[message.EventDetails.CombatDetails.ActorName];
                        else
                        {
                            // if actor isn't in one of the battles, check for targets.
                            foreach (var target in message.EventDetails.CombatDetails.Targets)
                            {
                                var targetRow = localDB.Combatants.FindMobByName(target.Name);

                                if (targetRow != null)
                                {
                                    var ambl = activeMobBattleList.FirstOrDefault(am => am.Key == target.Name);

                                    if ((ambl.Key != null) && (ambl.Key != string.Empty))
                                        battle = ambl.Value;

                                    if (battle != null)
                                        break;
                                }
                            }
                        }

                        // If we can't find it by now, just dump it in the default battle
                        if (battle == null)
                            battle = localDB.Battles.GetDefaultBattle();
                    }
                    else
                    {
                        // If player or pet acting (or we just don't know), put in the most current battle
                        battle = MostRecentActiveBattle();
                    }

                    lock (activeBattleList)
                        activeBattleList[battle] = message.Timestamp;

                    //  Create an entry for each target
                    foreach (var target in message.EventDetails.CombatDetails.Targets)
                    {
                        var targetRow = localDB.Combatants.FindByNameAndType(target.Name, target.EntityType);

                        if ((targetRow != null) && (targetRow.CombatantType != (byte)target.EntityType))
                            targetRow.CombatantType = (byte)target.EntityType;

                        if (targetRow == null)
                            targetRow = localDB.Combatants.AddCombatantsRow(target.Name,
                                (byte)target.EntityType, null);

                        localDB.Interactions.AddInteractionsRow(
                            message.Timestamp,
                            actor,
                            targetRow,
                            battle,
                            (byte)ActorType.Unknown,
                            message.EventDetails.CombatDetails.IsPreparing,
                            action,
                            (byte)message.EventDetails.CombatDetails.ActionSource,
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
                            target.SecondaryAmount);
                    }
                    break;
                #endregion
                #region Unknown
                case InteractionType.Unknown:
                    if (message.EventDetails.CombatDetails.IsPreparing == true)
                    {
                        actor = localDB.Combatants.GetCombatant(message.EventDetails.CombatDetails.ActorName,
                            message.EventDetails.CombatDetails.ActorEntityType);

                        action = localDB.Actions.GetAction(message.EventDetails.CombatDetails.ActionName);

                        battle = MostRecentActiveBattle();

                        lock (activeBattleList)
                            activeBattleList[battle] = message.Timestamp;

                        localDB.Interactions.AddInteractionsRow(
                            message.Timestamp,
                            actor,
                            null,
                            battle,
                            (byte)ActorType.Unknown,
                            message.EventDetails.CombatDetails.IsPreparing,
                            action,
                            (byte)message.EventDetails.CombatDetails.ActionSource,
                            (byte)message.EventDetails.CombatDetails.FailedActionType,
                            (byte)DefenseType.None,
                            0,
                            (byte)message.EventDetails.CombatDetails.AidType,
                            (byte)RecoveryType.None,
                            (byte)message.EventDetails.CombatDetails.HarmType,
                            0,
                            (byte)DamageModifier.None,
                            (byte)AidType.Unknown,
                            (byte)RecoveryType.None,
                            (byte)HarmType.Unknown,
                            0);
                    }
                    break;
                #endregion
            }
        }

        private void ProcessDeath(Message message)
        {
            KPDatabaseDataSet.CombatantsRow actor = null;
            KPDatabaseDataSet.BattlesRow battle = null;
            
            // If there are no targets, we can't do anything.
            if ((message.EventDetails.CombatDetails.Targets == null) || (message.EventDetails.CombatDetails.Targets.Count == 0))
                return;

            // If no actor name give, target just "fell to the ground".
            if (message.EventDetails.CombatDetails.ActorName != string.Empty)
            {
                // Determine the acting combatant for this event
                actor = localDB.Combatants.FindByNameAndType(message.EventDetails.CombatDetails.ActorName,
                    message.EventDetails.CombatDetails.ActorEntityType);

                if (actor == null)
                    actor = localDB.Combatants.AddCombatantsRow(message.EventDetails.CombatDetails.ActorName,
                        (byte)message.EventDetails.CombatDetails.ActorEntityType, null);
            }

            if (actor.CombatantType == (byte)EntityType.Mob)
            {
                // For mobs killing players, or when we don't know the actor type,
                // just make a combat detail entry per death.

                // Determine the battle the death(s) need to be linked against.
                if (activeMobBattleList.Keys.Contains(message.EventDetails.CombatDetails.ActorName))
                    battle = activeMobBattleList[message.EventDetails.CombatDetails.ActorName];
                else
                    // Make a new one if one doesn't exist
                    battle = localDB.Battles.AddBattlesRow(actor, message.Timestamp,
                        MagicNumbers.MinSQLDateTime, false, null, 0, 0, 0, (byte)MobDifficulty.Unknown, false);

                foreach (var target in message.EventDetails.CombatDetails.Targets)
                {
                    var targetRow = localDB.Combatants.FindByNameAndType(target.Name, target.EntityType);

                    if (targetRow == null)
                        targetRow = localDB.Combatants.AddCombatantsRow(target.Name,
                            (byte)target.EntityType, null);

                    localDB.Interactions.AddInteractionsRow(
                        message.Timestamp,
                        actor,
                        targetRow,
                        battle,
                        (byte)ActorType.Unknown,
                        message.EventDetails.CombatDetails.IsPreparing,
                        null,
                        (byte)message.EventDetails.CombatDetails.ActionSource,
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
                        target.SecondaryAmount);
                }
            }


            if ((message.EventDetails.CombatDetails.ActorEntityType == EntityType.Player) ||
                (message.EventDetails.CombatDetails.ActorEntityType == EntityType.Pet))
            {
                // Processing players killing mobs

                foreach (var target in message.EventDetails.CombatDetails.Targets)
                {
                    var targetRow = localDB.Combatants.FindByNameAndType(target.Name, target.EntityType);

                    if (activeMobBattleList.Keys.Contains(target.Name))
                        battle = activeMobBattleList[target.Name];
                    else
                        // Make a new one if one doesn't exist
                        battle = localDB.Battles.AddBattlesRow(actor, message.Timestamp,
                            MagicNumbers.MinSQLDateTime, false, null, 0, 0, 0, (byte)MobDifficulty.Unknown, false);

                    battle.Killed = true;
                    battle.EndTime = message.Timestamp;

                    lastKilledList[target.Name] = battle;
                    lastFinishedBattle = battle;

                    // If the one who killed the mob is given, note that in the killer ID
                    if (message.EventDetails.CombatDetails.ActorName != "")
                    {
                        var killer = localDB.Combatants.FindPlayerOrPetByName(message.EventDetails.CombatDetails.ActorName);

                        if (killer == null)
                            killer = localDB.Combatants.AddCombatantsRow(message.EventDetails.CombatDetails.ActorName,
                                (byte)EntityType.Unknown, null);

                        battle.CombatantsRowByBattleKillerRelation = killer;
                    }

                    localDB.Interactions.AddInteractionsRow(
                        message.Timestamp,
                        actor,
                        targetRow,
                        battle,
                        (byte)ActorType.Unknown,
                        message.EventDetails.CombatDetails.IsPreparing,
                        null,
                        (byte)message.EventDetails.CombatDetails.ActionSource,
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
                        target.SecondaryAmount);

                    // Remove mob from battle lists
                    activeMobBattleList.Remove(target.Name);
                    lock (activeBattleList)
                        activeBattleList.Remove(battle);
                }

                // Target 'just died'.
                if ((actor.CombatantType == (byte)EntityType.Unknown) &&
                    (message.EventDetails.CombatDetails.ActorName == string.Empty))
                {
                    foreach (var target in message.EventDetails.CombatDetails.Targets)
                    {
                        if (target.EntityType == EntityType.Mob)
                        {
                            var targetRow = localDB.Combatants.FindByNameAndType(target.Name, target.EntityType);

                            // if mob died, need to end battle
                            // Determine the battle the death(s) need to be linked against.
                            if (activeMobBattleList.Keys.Contains(message.EventDetails.CombatDetails.ActorName))
                                battle = activeMobBattleList[message.EventDetails.CombatDetails.ActorName];
                            else
                                // Make a new one if one doesn't exist
                                battle = localDB.Battles.AddBattlesRow(actor, message.Timestamp,
                                    MagicNumbers.MinSQLDateTime, false, null, 0, 0, 0, (byte)MobDifficulty.Unknown, false);

                            battle.Killed = true;
                            battle.EndTime = message.Timestamp;

                            lastKilledList[target.Name] = battle;
                            lastFinishedBattle = battle;

                            localDB.Interactions.AddInteractionsRow(
                                message.Timestamp,
                                actor,
                                targetRow,
                                battle,
                                (byte)ActorType.Unknown,
                                message.EventDetails.CombatDetails.IsPreparing,
                                null,
                                (byte)message.EventDetails.CombatDetails.ActionSource,
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
                                target.SecondaryAmount);

                            // Remove mob from battle lists
                            activeMobBattleList.Remove(target.Name);
                            lock (activeBattleList)
                                activeBattleList.Remove(battle);
                        }
                        else
                        {
                            // if anyone else died, just make a note of it
                            var targetRow = localDB.Combatants.FindByNameAndType(target.Name, target.EntityType);

                            if (targetRow == null)
                                targetRow = localDB.Combatants.AddCombatantsRow(target.Name,
                                    (byte)target.EntityType, null);

                            localDB.Interactions.AddInteractionsRow(
                                message.Timestamp,
                                actor,
                                targetRow,
                                battle,
                                (byte)ActorType.Unknown,
                                message.EventDetails.CombatDetails.IsPreparing,
                                null,
                                (byte)message.EventDetails.CombatDetails.ActionSource,
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
                                target.SecondaryAmount);
                        }
                    }
                }
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
