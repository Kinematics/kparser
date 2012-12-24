using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using WaywardGamers.KParser.KPDatabaseDataSetTableAdapters;
using WaywardGamers.KParser.Database;

namespace WaywardGamers.KParser
{
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
            Properties.Settings prefs = new WaywardGamers.KParser.Properties.Settings();
            prefs.Reload();

            string defaultPath = prefs.DefaultParseSaveDirectory;
            if (defaultPath == string.Empty)
                defaultPath = System.Windows.Forms.Application.CommonAppDataPath;

            defaultCopyDatabaseFilename = Path.Combine(defaultPath, prefs.DefaultUnnamedDBFileName);

            Version assemVersion = Assembly.GetExecutingAssembly().GetName().Version;
            assemblyVersionString = string.Format("{0}.{1}", assemVersion.Major, assemVersion.Minor);
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
        private const int databaseVersion = 2;
        private string assemblyVersionString;

        private string defaultCopyDatabaseFilename;
        private string databaseFilename;
        private string databaseConnectionString;

        private KPDatabaseDataSet localDB;
        private KPDatabaseDataSetTableAdapters.TableAdapterManager localTAManager;

        private DateTime lastUpdateTime = DateTime.Now;
        private TimeSpan updateDelayWindow = TimeSpan.FromSeconds(5);

        public event DatabaseWatchEventHandler DatabaseChanging;
        public event DatabaseWatchEventHandler DatabaseChanged;

        public event ReaderStatusHandler ReparseProgressChanged;

        private int maxRecordLogChatLineLength = 320;

        // Allow global access to the value set in the main window.
        bool showJobInsteadOfName;

        private bool disposed = false;

        private Mutex databaseAccessMutex = new Mutex();

        DatabaseEntry databaseEntry = new DatabaseEntry();
        #endregion

        #region Methods for creating/opening/closing a database.
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

        /// <summary>
        /// Open the requested database.
        /// </summary>
        /// <param name="openDatabaseFilename">The filename of the database to open.</param>
        public void OpenDatabase(string openDatabaseFilename)
        {
            if (File.Exists(openDatabaseFilename) == false)
                throw new ApplicationException("File does not exist.");

            // Close the existing one, if applicable
            CloseDatabase();

            try
            {
                databaseFilename = openDatabaseFilename;
                databaseConnectionString = string.Format("Data Source={0}", databaseFilename);

                Properties.Settings.Default.Properties["KPDatabaseConnectionString"].DefaultValue = databaseConnectionString;

                CreateConnections();

                // Default parsed culture value.
                string parsedCulture = "";

                if (localDB.Version.Rows.Count > 0)
                {
                    // Get the parser version from the database.
                    string parserVersion = localDB.Version[0].ParserVersion;

                    if (string.IsNullOrEmpty(parserVersion) == false)
                    {
                        // Parser version string is assembly version number (eg: 1.4)
                        // plus an optional culture language tag (eg: "fr", "de", "ja").

                        Match parsedLangMatch = Regex.Match(parserVersion, @"(?<dbVer>\d\.\d+)(?<lang>fr-FR|de-DE|ja-JP)?");
                        if (parsedLangMatch.Success)
                        {
                            DatabaseParseVersion = parsedLangMatch.Groups["dbVer"].Value;

                            parsedCulture = parsedLangMatch.Groups["lang"].Value;

                            if (parsedCulture == null)
                                parsedCulture = string.Empty;
                        }
                    }
                }

                Resources.ParsedStrings.Culture = new System.Globalization.CultureInfo(parsedCulture);
                DatabaseParseCulture = parsedCulture;

                // Reset the static string classes to get the properly translated
                // version of the resource strings.
                JobAbilities.Reset();
                ParseExpressions.Reset(parsedCulture);
            }
            catch (Exception)
            {
                CloseDatabase();
                throw;
            }
        }

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
            Properties.Settings appSettings = new WaywardGamers.KParser.Properties.Settings();

            string parsedCulture = appSettings.ParsingCulture;
            if (parsedCulture == null)
                parsedCulture = string.Empty;

            if ((parsedCulture != "fr-FR") &&
                (parsedCulture != "de-DE") &&
                (parsedCulture != "ja-JP"))
            {
                parsedCulture = string.Empty;
            }

            string parserVersionString = string.Format("{0}{1}", assemblyVersionString, parsedCulture);

            // Insert version information
            if (localDB.Version.Rows.Count == 0)
                localDB.Version.AddVersionRow(databaseVersion, parserVersionString);

            // Insert default battle row
            if (localDB.Battles.Rows.Count == 0)
                localDB.Battles.AddBattlesRow(null, DateTime.Now.ToUniversalTime(),
                    DateTime.Now.ToUniversalTime(), false, null, 0, 0, 0, 0, true);

            UpdateDatabase();

            // Make sure we're using the proper translated strings when we start parsing.
            Resources.ParsedStrings.Culture = new System.Globalization.CultureInfo(parsedCulture);
            DatabaseParseCulture = parsedCulture;

            // Reset the static string classes to get the properly translated
            // version of the resource strings.
            JobAbilities.Reset();
            ParseExpressions.Reset(parsedCulture);
        }

        /// <summary>
        /// Close the current database.
        /// Saves any pending changes first.
        /// </summary>
        public void CloseDatabase()
        {
            databaseEntry.Reset();

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
        }
        #endregion

        #region Public properties of the database.
        public bool IsDatabaseOpen
        {
            get { return (localDB != null); }
        }

        public int DatabaseVersion
        {
            get { return databaseVersion; }
        }

        public string DatabaseFilename
        {
            get { return databaseFilename; }
        }

        public bool ShowJobInsteadOfName
        {
            get { return showJobInsteadOfName; }
            set { showJobInsteadOfName = value; }
        }

        public string DatabaseParseVersion
        {
            get;
            private set;
        }

        public string DatabaseParseCulture
        {
            get;
            private set;
        }
        #endregion

        #region Internal methods for getting access to the database.
        /// <summary>
        /// Try for up to 2 seconds to acquire access to the dataset.
        /// This can only be called internally (within this assembly).
        /// All external assemblies must use AccessToTheDatabase to
        /// acquire a reference to the DB.
        /// </summary>
        /// <returns>Returns the dataset if it acquires the mutex.  Otherwise
        /// returns null.</returns>
        internal KPDatabaseDataSet GetDatabaseForReading()
        {
            // Wait for up to 2 seconds to try to acquire the mutex.
            if (databaseAccessMutex.WaitOne(2000) == true)
                return localDB;
            else
                return null;
        }

        /// <summary>
        /// Call this method when done using the database reference
        /// acquired from GetDatabaseForReading().
        /// </summary>
        internal void DoneReadingDatabase()
        {
            databaseAccessMutex.ReleaseMutex();

            // If we requested the database for reading, don't
            // allow any changes to be made.
            // Note- causes major bug in invocation code. Leaving out for now.
            //if (localDB.HasChanges())
            //    localDB.RejectChanges();
        }
        #endregion

        #region Public method calls to modify the database.
        /// <summary>
        /// Insert raw (pre-parsed) message lines to the database for consistency.
        /// </summary>
        /// <param name="messageLine"></param>
        internal void AddChatLineToRecordLog(ChatLine chatLine)
        {
            if (chatLine == null)
                return;

            // Set ParseSuccessful to False at this point since we don't know the parse outcome.
            if (databaseAccessMutex.WaitOne())
            {
                try
                {
                    string chatText = chatLine.ChatText;

                    if (chatText.Length > maxRecordLogChatLineLength)
                        chatText = chatLine.ChatText.Substring(0, 320);

                    var logRow = localDB.RecordLog.AddRecordLogRow(chatLine.Timestamp, chatText, false);
                    chatLine.RecordLogID = logRow.RecordLogID;
                }
                finally
                {
                    databaseAccessMutex.ReleaseMutex();
                }
            }
        }

        /// <summary>
        /// Given a list of messages, add them to the database in the
        /// appropriate structure.
        /// </summary>
        /// <param name="messageList">The list of messages to add.</param>
        /// <param name="parseEnded">A flag to indicate whether the parsing
        /// has ended.  If so, close out all waiting values.</param>
        internal void ProcessNewMessages(List<Message> messageList, bool parseEnded)
        {
            // Can't process messages if no db present.
            if (localDB == null)
                return;

            if ((messageList == null) || (messageList.Count == 0))
            {
                databaseEntry.UpdatePlayerInfo(localDB, Parsing.MsgManager.Instance.PlayerInfoList);
                return;
            }

            int totalMessageCount = messageList.Count;
            int messageNumber = 0;

            // lock database while we're modifying it
            if (databaseAccessMutex.WaitOne())
            {
                try
                {
                    foreach (var message in messageList)
                    {
                        databaseEntry.AddMessageToDatabase(localDB, message);

                        OnMessageProcessed(new ReaderStatusEventArgs(++messageNumber, totalMessageCount, false, false));
                    }

                    databaseEntry.UpdatePlayerInfo(localDB, Parsing.MsgManager.Instance.PlayerInfoList);
                }
                catch (Exception e)
                {
                    OnMessageProcessed(new ReaderStatusEventArgs(++messageNumber, totalMessageCount, false, true));
                    Logger.Instance.Log(e);
                }
                finally
                {
                    databaseAccessMutex.ReleaseMutex();
                    OnMessageProcessed(new ReaderStatusEventArgs(++messageNumber, totalMessageCount, true, false));
                }
            }

            databaseEntry.MessageBatchSent();

            // Only push updates to disk every 5 seconds, unless parse is ending.
            if (parseEnded == false)
            {
                DateTime currentTime = DateTime.Now;
                if (lastUpdateTime + updateDelayWindow > currentTime)
                    return;

                lastUpdateTime = currentTime;
            }

            KPDatabaseDataSet datasetChanges = null;

            try
            {
                if (localDB.HasChanges())
                {
                    datasetChanges = (KPDatabaseDataSet)localDB.GetChanges();

                    // Notify watchers so that they can view the database with
                    // Row changed/inserted/deleted flags still visible
                    OnDatabaseChanging(new DatabaseWatchEventArgs(datasetChanges));

                    UpdateDatabase();

                    // Notify watchers when database has been fully updated.
                    OnDatabaseChanged(new DatabaseWatchEventArgs(null));
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e);
            }
            finally
            {
                if (datasetChanges != null)
                    datasetChanges.Dispose();
            }

            if (parseEnded == true)
                databaseEntry.Reset();
        }

        /// <summary>
        /// Request that the database's player info be updated with the
        /// supplied list.
        /// </summary>
        /// <param name="playerInfoList">Player information list, used for updating.</param>
        public void UpdatePlayerInfo(List<PlayerInfo> playerInfoList)
        {
            if (databaseAccessMutex.WaitOne())
            {
                try
                {
                    databaseEntry.UpdatePlayerInfo(localDB, playerInfoList);

                    if (localDB.HasChanges())
                    {
                        KPDatabaseDataSet datasetChanges = (KPDatabaseDataSet)localDB.GetChanges();

                        if (datasetChanges != null)
                        {
                            try
                            {
                                // Notify watchers so that they can view the database with
                                // Row changed/inserted/deleted flags still visible
                                OnDatabaseChanging(new DatabaseWatchEventArgs(datasetChanges));

                                UpdateDatabase();

                                // Notify watchers when database has been fully updated.
                                OnDatabaseChanged(new DatabaseWatchEventArgs(null));
                            }
                            finally
                            {
                                datasetChanges.Dispose();
                            }
                        }
                    }
                }
                finally
                {
                    databaseAccessMutex.ReleaseMutex();
                }
            }
        }

        /// <summary>
        /// Request that the database be purged of (possibly sensitive) chat information.
        /// </summary>
        public void PurgeChatInfo()
        {
            if (databaseAccessMutex.WaitOne())
            {
                try
                {
                    databaseEntry.PurgeChatInfo(localDB);

                    if (localDB.HasChanges())
                    {
                        KPDatabaseDataSet datasetChanges = (KPDatabaseDataSet)localDB.GetChanges();

                        if (datasetChanges != null)
                        {
                            try
                            {
                                // Notify watchers so that they can view the database with
                                // Row changed/inserted/deleted flags still visible
                                OnDatabaseChanging(new DatabaseWatchEventArgs(datasetChanges));

                                UpdateDatabase();

                                // Notify watchers when database has been fully updated.
                                OnDatabaseChanged(new DatabaseWatchEventArgs(null));
                            }
                            finally
                            {
                                datasetChanges.Dispose();
                            }
                        }
                    }
                }
                finally
                {
                    databaseAccessMutex.ReleaseMutex();
                }
            }
        }

        /// <summary>
        /// Push database changes to disk.
        /// </summary>
        private void UpdateDatabase()
        {
            if ((localTAManager == null) || (localDB == null))
                return;

            if (databaseAccessMutex.WaitOne())
            {
                try
                {
                    localTAManager.UpdateAll(localDB);
                }
                catch (Exception e)
                {
                    Logger.Instance.Log(e);
                }
                finally
                {
                    databaseAccessMutex.ReleaseMutex();
                }
            }
        }
        #endregion

        #region Event Handling and Notification
        /// <summary>
        /// Notify listeners if changes are about to be made to the database.
        /// </summary>
        /// <param name="databaseWatchEventArgs"></param>
        private void OnDatabaseChanging(DatabaseWatchEventArgs databaseWatchEventArgs)
        {
            DatabaseWatchEventHandler localDatabaseChanging = DatabaseChanging;
            if (localDatabaseChanging != null)
            {
                localDatabaseChanging(this, databaseWatchEventArgs);
            }
        }

        /// <summary>
        /// Notify listeners if changes have just been made to the database.
        /// </summary>
        /// <param name="databaseWatchEventArgs"></param>
        private void OnDatabaseChanged(DatabaseWatchEventArgs databaseWatchEventArgs)
        {
            DatabaseWatchEventHandler localDatabaseChanged = DatabaseChanged;
            if (localDatabaseChanged != null)
            {
                localDatabaseChanged(this, databaseWatchEventArgs);
            }
        }

        /// <summary>
        /// Notify listeners if the status of saving reparse data has changed.
        /// </summary>
        /// <param name="e"></param>
        private void OnMessageProcessed(ReaderStatusEventArgs e)
        {
            ReaderStatusHandler localReparseProgress = ReparseProgressChanged;
            if (localReparseProgress != null)
            {
                localReparseProgress(this, e);
            }
        }
        #endregion
    }
}
