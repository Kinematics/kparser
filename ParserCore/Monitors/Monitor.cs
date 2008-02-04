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
        static DataSource parseMode = DataSource.Ram;

        public static void Start(string outputFileName)
        {
            if (LogReader.Instance.IsRunning == true ||
                RamReader.Instance.IsRunning == true)
                throw new InvalidOperationException("A monitor is already running.");

            Properties.Settings settings;
            settings = new Properties.Settings();
            settings.Reload();

            parseMode = settings.ParseMode;

            if (parseMode == DataSource.Log)
            {
                LogReader.Instance.Run(outputFileName);
            }
            else
            {
                RamReader.Instance.Run(outputFileName);
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
                if (parseMode == DataSource.Log)
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
            get { return parseMode; }
        }
    }
}
