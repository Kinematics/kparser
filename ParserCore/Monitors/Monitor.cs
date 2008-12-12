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
            ParseMode = settings.ParseMode;

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

        /// <summary>
        /// Continue parsing against an existing database.
        /// </summary>
        public static void Continue()
        {
            if (currentReader.IsRunning == true)
                throw new InvalidOperationException(string.Format(
                    "{0} is already running", currentReader.GetType().Name));

            if (DatabaseManager.Instance.Database == null)
            {
                throw new InvalidOperationException(
                    "You must have a database already open in order to continue parsing.");
            }

            // Only reload settings values on Start.  Other calls between
            // Starts will use whatever the setting was at the time of the
            // last Start.
            settings.Reload();
            ParseMode = settings.ParseMode;

            // Set the currentReader to the appropriate reader instance
            // based on program settings.
            if (settings.ParseMode == DataSource.Log)
                currentReader = LogReader.Instance;
            else
                currentReader = RamReader.Instance;

            currentReader.Run();
        }

        /// <summary>
        /// Reparse an existing database into a new one.
        /// </summary>
        /// <param name="outputFileName">The name of the new database.</param>
        public static void Reparse(string inFilename, string outputFileName)
        {
            if (currentReader.IsRunning == true)
                throw new InvalidOperationException(string.Format(
                    "{0} is already running", currentReader.GetType().Name));

            ParseMode = DataSource.Database;
            currentReader = DatabaseReader.Instance;

            DatabaseManager.Instance.CreateDatabase(outputFileName);
            System.Threading.Thread.Sleep(100);
            DatabaseReadingManager.Instance.OpenDatabase(inFilename);

            currentReader.Run();
        }

        /// <summary>
        /// Import data from another parser's database (DVS, DirectParse, etc)
        /// </summary>
        /// <param name="outputFileName">The name of the new database.</param>
        public static void Import(string inFilename, string outputFileName, ImportSource importSource)
        {
            if (currentReader.IsRunning == true)
                throw new InvalidOperationException(string.Format(
                    "{0} is already running", currentReader.GetType().Name));

            ParseMode = DataSource.Database;
            currentReader = DatabaseReader.Instance;

            DatabaseManager.Instance.CreateDatabase(outputFileName);
            System.Threading.Thread.Sleep(100);

            switch (importSource)
            {
                case ImportSource.DirectParse:
                case ImportSource.DVSParse:
                    ImportDirectParseManager.Instance.OpenDatabase(inFilename);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            currentReader.Import(importSource);
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
        public static DataSource ParseMode { get; private set; }

        internal static DataSource TestParseMode
        {
            get { return ParseMode; }
            set { if (IsRunning == false) ParseMode = value; }
        }

        public static void ScanRAM()
        {
            currentReader = RamReader.Instance;
            RamReader.Instance.ScanRAM();
        }

        public static void Import(string inFilename, string outFilename)
        {
            throw new NotImplementedException();
        }
    }
}
