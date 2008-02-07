using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.IO;
using WaywardGamers.KParser.Monitoring.Memory;
using WaywardGamers.KParser.Interface;

namespace WaywardGamers.KParser.Monitoring
{
    /// <summary>
    /// Class to handle interfacing with system RAM reading
    /// in order to monitor the FFXI process space and read
    /// log info to be parsed.
    /// </summary>
    internal class RamReader : IReader
    {
        #region Singleton Constructor
        // Make the class a singleton
        private static readonly RamReader instance = new RamReader();

        /// <summary>
        /// Gets the singleton instance of the LogParser class.
        /// This is the only internal way to access the singleton.
        /// </summary>
        internal static RamReader Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Private constructor ensures singleton purity.
        /// </summary>
        private RamReader()
		{
            memoryWatcher = new MemoryAccess();
            appSettings = new WaywardGamers.KParser.Properties.Settings();
        }
		#endregion

        #region Member Variables
        MemoryAccess memoryWatcher;
        Properties.Settings appSettings;
        Thread readerThread;
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

                // Make sure we have the latest version of the app settings data.
                appSettings.Reload();

                // Add the event handler
                memoryWatcher.RamDataChanged += new RamWatchEventHandler(MonitorRam);

                // Update the memory offset of the thread class before starting.
                memoryWatcher.InitialMemoryOffset = appSettings.MemoryOffset;

                // Begin the thread
                readerThread = new Thread(memoryWatcher.Monitor);
                readerThread.IsBackground = true;
                readerThread.Name = "Memory Monitor Thread";
                readerThread.Start();

                // Notify MessageManager that we're starting.
                MessageManager.Instance.StartParsing();
            }
            catch (Exception)
            {
                IsRunning = false;
                memoryWatcher.RamDataChanged -= new RamWatchEventHandler(MonitorRam);
                MessageManager.Instance.StopParsing();
                throw;
            }
        }

        /// <summary>
        /// Stop the active reader thread.
        /// </summary>
        public void Stop()
        {
            if (IsRunning == false)
                return;

            // Remove event handler and stop thread.
            memoryWatcher.RamDataChanged -= new RamWatchEventHandler(MonitorRam);
            memoryWatcher.Abort();

            // Notify MessageManager that we're done so it can turn off its timer loop.
            MessageManager.Instance.StopParsing();

            IsRunning = false;
        }
        #endregion

        #region Event handlers

        /// <summary>
        /// Event handler for when new chat data is being processed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MonitorRam(object sender, RamWatchEventArgs e)
        {
            if (e == null)
            {
                // Notification that the ram watcher lost connection to FFXI
                return;
            }

            // Incoming ram data in e. Process it
            foreach (ChatLine chat in e.ChatLineCollection)
            {
                MessageManager.Instance.AddChatLine(chat);
            }
        }
        #endregion

    }
}
