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
    public class DatabaseReadingManager : IDisposable
    {
        #region Singleton Construction
        /// <summary>
        /// Make the class a singleton
        /// </summary>
        private static readonly DatabaseReadingManager instance = new DatabaseReadingManager();

        /// <summary>
        /// Private constructor ensures singleton purity.
        /// </summary>
        private DatabaseReadingManager()
        {
            string applicationDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            defaultSaveDirectory = Path.Combine(applicationDirectory, Properties.Settings.Default.DefaultSaveSubdirectory);

            defaultCopyDatabaseFilename = Path.Combine(defaultSaveDirectory, Properties.Settings.Default.DefaultUnnamedDBFileName);

            Version assemVersion = Assembly.GetExecutingAssembly().GetName().Version;
            assemblyVersionString = string.Format("{0}.{1}", assemVersion.Major, assemVersion.Minor);
        }

        /// <summary>
        /// Gets the singleton instance of the DatabaseManager class.
        /// </summary>
        public static DatabaseReadingManager Instance
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

        private bool disposed = false;
        #endregion

        #region Public Methods/Properties
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

        public string DatabaseFilename
        {
            get
            {
                return databaseFilename;
            }
        }

        public void CloseDatabase()
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
        #endregion

    }
}
