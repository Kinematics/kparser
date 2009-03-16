﻿using System;
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
    public class KParserReadingManager : IDisposable, IDBReader
    {
        #region Singleton Construction
        /// <summary>
        /// Make the class a singleton
        /// </summary>
        private static readonly KParserReadingManager instance = new KParserReadingManager();

        /// <summary>
        /// Private constructor ensures singleton purity.
        /// </summary>
        private KParserReadingManager()
        {
            defaultSaveDirectory = System.Windows.Forms.Application.CommonAppDataPath;
        }

        /// <summary>
        /// Gets the singleton instance of the DatabaseManager class.
        /// </summary>
        public static KParserReadingManager Instance
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

        private KPDatabaseReadOnly localDB;
        private KPDatabaseReadOnlyTableAdapters.TableAdapterManager localTAManager;

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

        public KPDatabaseReadOnly Database
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
            localDB = new KPDatabaseReadOnly();
            localTAManager = new KPDatabaseReadOnlyTableAdapters.TableAdapterManager();

            localTAManager.RecordLogTableAdapter = new KPDatabaseReadOnlyTableAdapters.RecordLogTableAdapter();
            localTAManager.VersionTableAdapter = new KPDatabaseReadOnlyTableAdapters.VersionTableAdapter();
            localTAManager.CombatantsTableAdapter = new KPDatabaseReadOnlyTableAdapters.CombatantsTableAdapter();


            System.Data.SqlServerCe.SqlCeConnection sqlConn =
                new System.Data.SqlServerCe.SqlCeConnection(databaseConnectionString);

            localTAManager.Connection = sqlConn;

            localTAManager.RecordLogTableAdapter.Connection = sqlConn;
            localTAManager.VersionTableAdapter.Connection = sqlConn;
            localTAManager.CombatantsTableAdapter.Connection = sqlConn;


            // If opening an existing database, need to check version info before filling data

            localTAManager.RecordLogTableAdapter.Fill(localDB.RecordLog);
            localTAManager.VersionTableAdapter.Fill(localDB.Version);
            localTAManager.CombatantsTableAdapter.Fill(localDB.Combatants);
        }
        #endregion

    }
}
