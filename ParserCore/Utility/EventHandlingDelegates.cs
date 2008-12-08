using System;
using System.Threading;

namespace WaywardGamers.KParser
{
    #region Reparsing
    public delegate void DatabaseReparseEventHandler(object sender, DatabaseReparseEventArgs ramArgs);

    public class DatabaseReparseEventArgs : EventArgs
    {
        public int RowsRead { get; private set; }
        public int TotalRows { get; private set; }
        public bool Complete { get; private set; }

        internal DatabaseReparseEventArgs(int rowRead, int totalRows, bool complete)
        {
            RowsRead = rowRead;
            TotalRows = totalRows;
            Complete = complete;
        }
    }
    #endregion

    #region Watching the database
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
}

