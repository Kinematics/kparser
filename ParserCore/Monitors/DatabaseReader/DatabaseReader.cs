using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.IO;
using WaywardGamers.KParser.Interface;

namespace WaywardGamers.KParser.Monitoring
{
    /// <summary>
    /// Class to handle interfacing with system RAM reading
    /// in order to monitor the FFXI process space and read
    /// log info to be parsed.
    /// </summary>
    internal class DatabaseReader : IReader
    {
        #region Singleton Constructor
        // Make the class a singleton
        private static readonly DatabaseReader instance = new DatabaseReader();

        /// <summary>
        /// Gets the singleton instance of the LogParser class.
        /// This is the only internal way to access the singleton.
        /// </summary>
        internal static DatabaseReader Instance
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
                // Notify MessageManager that we're starting.
                MessageManager.Instance.StartParsing();

                KPDatabaseDataSet readDataSet = DatabaseManager.SecondInstance.Database;

                // Read the (fixed) record log from the database, reconstruct
                // the chat line, and send it to the new database.
                foreach (var logLine in readDataSet.RecordLog)
                {
                    ChatLine chat = new ChatLine(logLine.MessageText, logLine.Timestamp);
                    MessageManager.Instance.AddChatLine(chat);
                }

                DatabaseManager.SecondInstance.CloseDatabase();

            }
            catch (Exception)
            {
                IsRunning = false;
                MessageManager.Instance.StopParsing();
                throw;
            }
        }

        private void ReadDatabase()
        {
            KPDatabaseDataSet readDataSet = DatabaseManager.SecondInstance.Database;

            // Read the (fixed) record log from the database, reconstruct
            // the chat line, and send it to the new database.
            foreach (var logLine in readDataSet.RecordLog)
            {
                ChatLine chat = new ChatLine(logLine.MessageText, logLine.Timestamp);
                MessageManager.Instance.AddChatLine(chat);
            }

            DatabaseManager.SecondInstance.CloseDatabase();
        }

        /// <summary>
        /// Stop the active reader thread.
        /// </summary>
        public void Stop()
        {
            if (IsRunning == false)
                return;

            // Notify MessageManager that we're done so it can turn off its timer loop.
            MessageManager.Instance.StopParsing();

            IsRunning = false;
        }
        #endregion

    }
}
