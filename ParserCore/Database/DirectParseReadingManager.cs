using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using WaywardGamers.KParser.Interface;

namespace WaywardGamers.KParser
{
    public class DirectParseReadingManager : IDisposable, IDBReader
    {
        #region Singleton Construction
        /// <summary>
        /// Make the class a singleton
        /// </summary>
        private static readonly DirectParseReadingManager instance = new DirectParseReadingManager();

        /// <summary>
        /// Private constructor ensures singleton purity.
        /// </summary>
        private DirectParseReadingManager()
        {
            defaultSaveDirectory = System.Windows.Forms.Application.CommonAppDataPath;
        }

        /// <summary>
        /// Gets the singleton instance of the DatabaseManager class.
        /// </summary>
        public static DirectParseReadingManager Instance
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
        private string defaultSaveDirectory;

        private string databaseFilename;
        private string databaseConnectionString;
        private string tempDatabaseName = string.Empty;

        private DPDatabaseImportV1 localDB;
        private DPDatabaseImportV1TableAdapters.TableAdapterManager localTAManager;

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

            Properties.Settings.Default.Properties["DvsParse_SaveConnectionString"].DefaultValue = databaseConnectionString;

            CreateConnections();
        }

        public DPDatabaseImportV1 Database
        {
            get
            {
                return localDB;
            }
        }

        public string DatabaseFilename
        {
            get
            {
                return databaseFilename;
            }
        }

        public string DatabaseParseVersion
        {
            get
            {
                return string.Empty;
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

            if (tempDatabaseName != string.Empty)
            {
                if (File.Exists(tempDatabaseName))
                {
                    File.Delete(tempDatabaseName);
                }
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
            localDB = new DPDatabaseImportV1();
            localTAManager = new DPDatabaseImportV1TableAdapters.TableAdapterManager();

            localTAManager.ChatLogTableAdapter = new DPDatabaseImportV1TableAdapters.ChatLogTableAdapter();

            // DVS/DirectParse uses SQLCE 3.1, so we can always do an upgrade here.
            System.Data.SqlServerCe.SqlCeEngine sqlCeEngine = new System.Data.SqlServerCe.SqlCeEngine(databaseConnectionString);
            // Creates a 0-byte file.  We need the name, but don't want the file to exist.
            tempDatabaseName = Path.GetTempFileName();
            File.Delete(tempDatabaseName);
            string tempConnectionString = string.Format("Data Source={0}", tempDatabaseName);
            sqlCeEngine.Upgrade(tempConnectionString);

            Properties.Settings.Default.Properties["DvsParse_SaveConnectionString"].DefaultValue = tempConnectionString;


            System.Data.SqlServerCe.SqlCeConnection sqlConn =
                new System.Data.SqlServerCe.SqlCeConnection(tempConnectionString);

            localTAManager.Connection = sqlConn;

            localTAManager.ChatLogTableAdapter.Connection = sqlConn;

            localTAManager.ChatLogTableAdapter.Fill(localDB.ChatLog);
        }
        #endregion

    }
}
