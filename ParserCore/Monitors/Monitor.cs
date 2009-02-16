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
        #region Class members
        static IReader currentReader = RamReader.Instance;
        #endregion

        #region Current reader properties
        /// <summary>
        /// Gets whether the current reader is running.
        /// </summary>
        public static bool IsRunning
        {
            get
            {
                return currentReader.IsRunning;
            }
        }

        /// <summary>
        /// Gets the DataSource type of the current reader.
        /// </summary>
        public static DataSource ParseMode
        {
            get
            {
                return currentReader.ParseModeType;
            }
        }
        #endregion

        #region Functions to start and stop monitoring.

        public static void Start(DataSource dataSourceType, string sourceFile)
        {
            if (currentReader.IsRunning == true)
                throw new InvalidOperationException(string.Format(
                    "{0} is already running", currentReader.GetType().Name));

            switch (dataSourceType)
            {
                case DataSource.Log:
                    currentReader = LogReader.Instance;
                    break;
                case DataSource.Ram:
                    currentReader = RamReader.Instance;
                    break;
                case DataSource.Database:
                    currentReader = DatabaseReader.Instance;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("dataSourceType",
                        string.Format("Unknown DataSource value: {0}", dataSourceType.ToString()));
            }

            currentReader.Start();
        }


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
            Properties.Settings settings = new Properties.Settings();
            settings.Reload();

            // Set the currentReader to the appropriate reader instance
            // based on program settings.
            if (settings.ParseMode == DataSource.Log)
                currentReader = LogReader.Instance;
            else
                currentReader = RamReader.Instance;


            // Create the output database in preperation for a new run.
            DatabaseManager.Instance.CreateDatabase(outputFileName);

            currentReader.Start();
        }

        /// <summary>
        /// Continue parsing against an existing database.
        /// </summary>
        public static void Continue()
        {
            if (currentReader.IsRunning == true)
                throw new InvalidOperationException(string.Format(
                    "{0} is already running", currentReader.GetType().Name));

            if (DatabaseManager.Instance.IsDatabaseOpen == false)
            {
                throw new InvalidOperationException(
                    "You must have a database already open in order to continue parsing.");
            }

            // Only reload settings values on Start.  Other calls between
            // Starts will use whatever the setting was at the time of the
            // last Start.
            Properties.Settings settings = new Properties.Settings();
            settings.Reload();

            // Set the currentReader to the appropriate reader instance
            // based on program settings.
            if (settings.ParseMode == DataSource.Log)
                currentReader = LogReader.Instance;
            else
                currentReader = RamReader.Instance;

            currentReader.Start();
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

            currentReader = DatabaseReader.Instance;

            DatabaseManager.Instance.CreateDatabase(outputFileName);
            System.Threading.Thread.Sleep(100);
            DatabaseReadingManager.Instance.OpenDatabase(inFilename);

            currentReader.Start();
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
        #endregion

        #region Debugging functions
        /// <summary>
        /// Artificially set the current reader for specific parse modes.
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG")]
        internal static void SetParseMode(DataSource dataSource)
        {
            if (IsRunning == false)
            {
                switch (dataSource)
                {
                    case DataSource.Log:
                        currentReader = LogReader.Instance;
                        break;
                    case DataSource.Ram:
                        currentReader = RamReader.Instance;
                        break;
                    case DataSource.Database:
                        currentReader = DatabaseReader.Instance;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("dataSourceType",
                            string.Format("Unknown DataSource value: {0}", dataSource.ToString()));
                }
            }
        }

        /// <summary>
        /// Initiate functions to analyze RAM when seeking for new Memloc.
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG")]
        public static void ScanRAM()
        {
            currentReader = RamReader.Instance;
            RamReader.Instance.ScanRAM();
        }
        #endregion
    }
}
