using System;
using WaywardGamers.KParser.Monitoring;

namespace WaywardGamers.KParser
{
    /// <summary>
    /// Class to handle directing requests to start and stop monitoring
    /// of FFXI data to the proper type of reader based on settings pref.
    /// </summary>
    public class Monitor
    {
        static Properties.Settings settings = new Properties.Settings();

        /// <summary>
        /// Initiate monitoring of FFXI RAM/logs for data to be parsed.
        /// </summary>
        /// <param name="outputFileName">The name of the database file
        /// that will be stored to.</param>
        public static void Start(string outputFileName)
        {
            if (LogReader.Instance.IsRunning == true ||
                RamReader.Instance.IsRunning == true)
                throw new InvalidOperationException("A monitor is already running.");

            // Only reload settings values on Start.  Other calls between
            // Starts will return whatever the original setting was.
            settings.Reload();

            // Create the output database
            DatabaseManager.Instance.CreateDatabase(outputFileName);

            if (settings.ParseMode == DataSource.Log)
            {
                LogReader.Instance.Run();
            }
            else
            {
                RamReader.Instance.Run();
            }
        }

        public static void Stop()
        {
            if (LogReader.Instance.IsRunning == true)
            {
                LogReader.Instance.Stop();
            }

            if (RamReader.Instance.IsRunning == true)
            {
                RamReader.Instance.Stop();
            }
        }

        public static bool IsRunning
        {
            get
            {
                if (settings.ParseMode == DataSource.Log)
                {
                    return LogReader.Instance.IsRunning;
                }
                else
                {
                    return RamReader.Instance.IsRunning;
                }
            }
        }

        public static DataSource ParseMode
        {
            get { return settings.ParseMode; }
        }
    }
}
