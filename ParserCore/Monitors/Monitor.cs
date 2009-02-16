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

        /// <summary>
        /// Initiate monitoring of FFXI RAM/logs for data to be parsed.
        /// </summary>
        /// <param name="dataSourceType">The type of data source to monitor.</param>
        /// <param name="outputFileName">The name of the database file
        /// that the parsed data will be stored in.</param>
        public static void Start(DataSource dataSourceType, string outputFileName)
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

            // Create the output database in preperation for a new run.
            DatabaseManager.Instance.CreateDatabase(outputFileName);

            currentReader.Start();
        }

        /// <summary>
        /// Continue parsing against an existing database.
        /// </summary>
        public static void Continue(DataSource dataSourceType)
        {
            if (currentReader.IsRunning == true)
                throw new InvalidOperationException(string.Format(
                    "{0} is already running", currentReader.GetType().Name));

            if (DatabaseManager.Instance.IsDatabaseOpen == false)
            {
                throw new InvalidOperationException(
                    "You must have a database already open in order to continue parsing.");
            }

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
        /// Import data from another parser's database (DVS, DirectParse, etc)
        /// </summary>
        /// <param name="outputFileName">The name of the new database.</param>
        public static void Import(string inFilename, string outputFileName, ImportSourceType importSource)
        {
            if (currentReader.IsRunning == true)
                throw new InvalidOperationException(string.Format(
                    "{0} is already running", currentReader.GetType().Name));

            currentReader = DatabaseReader.Instance;

            DatabaseManager.Instance.CreateDatabase(outputFileName);
            System.Threading.Thread.Sleep(100);

            IDBReader dbReader;

            switch (importSource)
            {
                case ImportSourceType.KParser:
                    dbReader = KParserReadingManager.Instance;
                    break;
                case ImportSourceType.DirectParse:
                case ImportSourceType.DVSParse:
                    dbReader = DirectParseReadingManager.Instance;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            dbReader.OpenDatabase(inFilename);

            currentReader.Import(importSource, dbReader);
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
