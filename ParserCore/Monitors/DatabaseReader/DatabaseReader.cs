using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.IO;
using WaywardGamers.KParser.Interface;

namespace WaywardGamers.KParser.Monitoring
{
    #region Event classes
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

    /// <summary>
    /// Class to handle interfacing with system RAM reading
    /// in order to monitor the FFXI process space and read
    /// log info to be parsed.
    /// </summary>
    public class DatabaseReader : IReader
    {
        #region Singleton Constructor
        // Make the class a singleton
        private static readonly DatabaseReader instance = new DatabaseReader();

        /// <summary>
        /// Gets the singleton instance of the LogParser class.
        /// This is the only internal way to access the singleton.
        /// </summary>
        public static DatabaseReader Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Private constructor ensures singleton purity.
        /// </summary>
        private DatabaseReader()
        {
        }
        #endregion

        #region Member Variables
        Thread readerThread;
        private event DatabaseReparseEventHandler reparseProgressWatchers;
        #endregion

        #region Event Management
        public DatabaseReparseEventHandler ReparseProgressChanged
        {
            get
            {
                return reparseProgressWatchers;
            }
            set
            {
                reparseProgressWatchers = value;
            }
        }

        protected virtual void OnRowProcessed(DatabaseReparseEventArgs e)
        {
            if (reparseProgressWatchers != null)
            {
                // Invokes the delegates. 
                reparseProgressWatchers(this, e);
            }
        }

        #endregion

        #region Interface Control Methods and Properties
        /// <summary>
        /// Gets (publicly) and sets (privately) the state of the
        /// reader thread.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Start a thread that reads log files for parsing.
        /// </summary>
        public void Run()
        {
            IsRunning = true;

            try
            {
                // Reset the thread
                if ((readerThread != null) && (readerThread.ThreadState == System.Threading.ThreadState.Running))
                {
                    readerThread.Abort();
                }

                // Begin the thread
                readerThread = new Thread(RunThread);
                readerThread.IsBackground = true;
                readerThread.Name = "Read database thread";
                readerThread.Start();

                // Notify MessageManager that we're starting.
                MessageManager.Instance.StartParsing(false);
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e);
                IsRunning = false;
                MessageManager.Instance.CancelParsing();
            }
        }

        public void RunThread()
        {
            int rowCount = 0;
            int totalCount = 0;
            bool completed = false;

            try
            {
                KPDatabaseDataSet readDataSet = DatabaseReadingManager.Instance.Database;
                totalCount = readDataSet.RecordLog.Count;

                // Read the (fixed) record log from the database, reconstruct
                // the chat line, and send it to the new database.
                using (new ProfileRegion("Reparse: Read database and parse"))
                {
                    foreach (var logLine in readDataSet.RecordLog)
                    {
                        rowCount++;
                        if (IsRunning == false)
                            break;

                        ChatLine chat = new ChatLine(logLine.MessageText, logLine.Timestamp);
                        MessageManager.Instance.AddChatLine(chat);

                        OnRowProcessed(new DatabaseReparseEventArgs(rowCount, totalCount, completed));
                    }

                    completed = IsRunning;
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e);
            }
            finally
            {
                IsRunning = false;
                if (completed == true)
                    MessageManager.Instance.StopParsing();
                else
                    MessageManager.Instance.CancelParsing();

                OnRowProcessed(new DatabaseReparseEventArgs(rowCount, totalCount, completed));
            }

        }

        /// <summary>
        /// Stop the active reader thread.
        /// </summary>
        public void Stop()
        {
            if (IsRunning == false)
                return;

            // Notify MessageManager that we're done so it can turn off its timer loop.
            MessageManager.Instance.CancelParsing();

            IsRunning = false;
        }
        #endregion

    }
}
