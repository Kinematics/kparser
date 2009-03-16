using System;
using System.Collections.Generic;
using System.Threading;

namespace WaywardGamers.KParser
{
    #region IReader delegates
    public delegate void ReaderDataHandler(object sender, ReaderDataEventArgs readerStatusArgs);

    public class ReaderDataEventArgs : EventArgs
    {
        public List<ChatLine> ChatLines { get; private set; }

        internal ReaderDataEventArgs(List<ChatLine> chatLines)
        {
            ChatLines = chatLines;
        }
    }

    public delegate void ReaderStatusHandler(object sender, ReaderStatusEventArgs readerStatusArgs);

    public class ReaderStatusEventArgs : EventArgs
    {
        public int ProcessedItems { get; private set; }
        public int TotalItems { get; private set; }
        public bool Completed { get; private set; }
        public bool Failed { get; private set; }

        internal ReaderStatusEventArgs(int processed, int totalItems, bool completed, bool failed)
        {
            ProcessedItems = processed;
            TotalItems = totalItems;
            Completed = completed;
            Failed = failed;
        }
    }

    #endregion


    #region Watching the database
    public delegate void DatabaseWatchEventHandler(object sender, DatabaseWatchEventArgs dbArgs);

    public class DatabaseWatchEventArgs : EventArgs
    {
        /// <summary>
        /// Gets and sets just the changes that are being applied to the database.
        /// </summary>
        public KPDatabaseDataSet DatasetChanges { get; private set; }

        /// <summary>
        /// Constructor is internal; only created by the DatabaseManager.
        /// </summary>
        /// <param name="managedDataset">The dataset provided by the database manager.</param>
        internal DatabaseWatchEventArgs(KPDatabaseDataSet changedDataset)
        {
            DatasetChanges = changedDataset;
        }
    }
    #endregion
}

