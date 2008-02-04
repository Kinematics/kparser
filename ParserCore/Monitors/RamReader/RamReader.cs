using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.IO;
using WaywardGamers.KParser.Monitoring.Memory;

namespace WaywardGamers.KParser.Monitoring
{
    internal class RamReader
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
        List<string> saveList = new List<string>();
        MemoryAccess memoryWatcher;
        Thread readerThread;

        bool isRunning;

        Properties.Settings appSettings;
        #endregion

        #region Control Methods
        /// <summary>
        /// Start a thread that reads log files for parsing.
        /// </summary>
        /// <param name="settings">The program settings for the thread to use.</param>
        /// <param name="fileName">The name of the file that data is going to be output to.</param>
        internal void Run(string outputFileName)
        {
            isRunning = true;

            try
            {
                // Reset the thread
                if ((readerThread != null) && (readerThread.ThreadState == System.Threading.ThreadState.Running))
                {
                    readerThread.Abort();
                }

                readerThread = new Thread(memoryWatcher.Monitor);
                readerThread.IsBackground = true;
                readerThread.Name = "Memory Monitor Thread";

                // Create the output database
                DatabaseManager.Instance.CreateDatabase(outputFileName);

                // Add the event handler
                memoryWatcher.RamDataChanged += new RamWatchEventHandler(MonitorRam);

                // Make sure we have the latest version of the app settings data.
                appSettings.Reload();

                // Update the memory offset of the thread class before starting.
                memoryWatcher.InitialMemoryOffset = appSettings.MemoryOffset;

                MessageManager.Instance.StartParsing();

                // Begin the thread
                readerThread.Start();
            }
            catch (Exception)
            {
                isRunning = false;
                throw;
            }
        }

        /// <summary>
        /// Stop the active thread.
        /// </summary>
        internal void Stop()
        {
            // Remove event handler and stop thread.
            memoryWatcher.RamDataChanged -= new RamWatchEventHandler(MonitorRam);
            memoryWatcher.Abort();

            // Notify MessageManager that we're done so it can turn off its timer loop.
            MessageManager.Instance.StopParsing();

            isRunning = false;
        }

        internal bool IsRunning
        {
            get
            {
                return isRunning;
            }
        }

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
