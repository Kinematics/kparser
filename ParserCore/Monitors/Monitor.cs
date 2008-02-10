using System;
using WaywardGamers.KParser.Monitoring;
using WaywardGamers.KParser.Interface;

namespace WaywardGamers.KParser
{
    /// <summary>
    /// Class to handle directing requests to start and stop monitoring
    /// of FFXI data to the proper type of reader based on settings pref.
    /// </summary>
    public static class Monitor
    {
        static Properties.Settings settings = new Properties.Settings();
        static IReader currentReader = RamReader.Instance;

        /// <summary>
        /// Initiate monitoring of FFXI RAM/logs for data to be parsed.
        /// </summary>
        /// <param name="outputFileName">The name of the database file
        /// that will be stored to.</param>
        public static void Start(string outputFileName)
        {
            if (currentReader.IsRunning == true)
                throw new InvalidOperationException(string.Format(
                    "{0} is already running", currentReader.GetType().Name));

            // Only reload settings values on Start.  Other calls between
            // Starts will use whatever the setting was at the time of the
            // last Start.
            settings.Reload();

            // Set the currentReader to the appropriate reader instance
            // based on program settings.
            if (settings.ParseMode == DataSource.Log)
                currentReader = LogReader.Instance;
            else
                currentReader = RamReader.Instance;


            // Create the output database in preperation for a new run.
            DatabaseManager.Instance.CreateDatabase(outputFileName);

            currentReader.Run();
        }

        public static void Reparse(string outputFileName)
        {
            if (currentReader.IsRunning == true)
                throw new InvalidOperationException(string.Format(
                    "{0} is already running", currentReader.GetType().Name));

            if (DatabaseManager.Instance.Database == null)
                throw new InvalidOperationException("Open a database before trying to reparse.");

            string oldDBName = DatabaseManager.Instance.DatabaseFilename;

            currentReader = DatabaseReader.Instance;

            try
            {
                DatabaseManager.Instance.CreateDatabase(outputFileName);
                DatabaseReadingManager.Instance.OpenDatabase(oldDBName);

                currentReader.Run();
            }
            finally
            {
                DatabaseReadingManager.Instance.CloseDatabase();
            }
        }

        /// <summary>
        /// Stop the current reader's monitoring.
        /// </summary>
        public static void Stop()
        {
            currentReader.Stop();
        }

        /// <summary>
        /// Gets whether the current reader is running.
        /// </summary>
        public static bool IsRunning
        {
            get { return currentReader.IsRunning; }
        }

        /// <summary>
        /// Gets the current parse mode, as far as the monitor is aware.
        /// </summary>
        public static DataSource ParseMode
        {
            get { return settings.ParseMode; }
        }
    }
}
