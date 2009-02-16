using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WaywardGamers.KParser.Interface;

namespace WaywardGamers.KParser.Monitoring
{
    /// <summary>
    /// Class that handles parsing the FFXI Log files.
    /// </summary>
    internal class LogReader : AbstractReader
    {
        #region Singleton Constructor
        // Make the class a singleton
        private static readonly LogReader instance = new LogReader();

        /// <summary>
        /// Gets the singleton instance of the LogParser class.
        /// This is the only internal way to access the singleton.
        /// </summary>
        internal static LogReader Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Private constructor ensures singleton purity.
        /// </summary>
        private LogReader()
        {
            fileSystemWatcher = new FileSystemWatcher();
            fileSystemWatcher.Filter = "*.log";
            fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite;
            fileSystemWatcher.Changed += new FileSystemEventHandler(MonitorLogDirectory);
            fileSystemWatcher.Created += new FileSystemEventHandler(MonitorLogDirectory);

            appSettings = new WaywardGamers.KParser.Properties.Settings();
        }
        #endregion

        #region Member Variables
        Properties.Settings appSettings;

        string watchDirectory = string.Empty; // Only access from the Property
        FileSystemWatcher fileSystemWatcher;

        DateTime networkWatchTimestamp;
        Timer networkWatchTimer;

        string lastChangedFile = string.Empty;
        #endregion

        #region Properties
        private string WatchDirectory
        {
            get
            {
                if (watchDirectory == string.Empty)
                    throw new ArgumentNullException();

                return watchDirectory;
            }
            set
            {
                DirectoryInfo dInfo = new DirectoryInfo(value);
                if (dInfo.Exists == false)
                    throw new ArgumentException(
                        string.Format("Directory '{0}' does not exist.", value));

                watchDirectory = value;
            }
        }
        #endregion

        #region Interface Control Methods and Properties

        /// <summary>
        /// Return type of DataSource this reader works on.
        /// </summary>
        public override DataSource ParseModeType { get { return DataSource.Log; } }

        /// <summary>
        /// Activate the file system watcher so that we can catch events when files change.
        /// If the option to parse existing files is true, run the parsing code on them.
        /// </summary>
        public override void Start()
        {
            IsRunning = true;

            try
            {
                // Get the settings so that we know where to look for the log files.
                appSettings.Reload();
                WatchDirectory = appSettings.FFXILogDirectory;

                // Verify the drive
                DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(WatchDirectory));

                if (driveInfo.IsReady == false)
                    throw new InvalidOperationException("Drive for the specified log directory is not available.");

                if ((driveInfo.DriveType != DriveType.Fixed) && (driveInfo.DriveType != DriveType.Network))
                    throw new InvalidOperationException("Drive is not of a type that can be monitored.");


                // Notify MessageManager that we're starting (preparatory).
                MessageManager.Instance.PrepareToStartParsing();

                // Run the parser on any logs already in existance before starting to monitor,
                // if that option is set.
                if (appSettings.ParseExistingLogs == true)
                {
                    ReadExistingFFXILogs();
                }

                // Notify MessageManager that we're starting.
                MessageManager.Instance.StartParsing(true);


                // Set up monitoring of log files for changes.
                fileSystemWatcher.Path = WatchDirectory;

                // Begin watching.
                if (driveInfo.DriveType == DriveType.Fixed)
                    fileSystemWatcher.EnableRaisingEvents = true;
                else if (driveInfo.DriveType == DriveType.Network)
                    StartMonitoringNetworkDrive();
            }
            catch (Exception)
            {
                IsRunning = false;
                fileSystemWatcher.EnableRaisingEvents = false;
                if (networkWatchTimer != null)
                {
                    networkWatchTimer.Dispose();
                    networkWatchTimer = null;
                }
                MessageManager.Instance.CancelParsing();
                throw;
            }
        }

        /// <summary>
        /// Stop monitoring the FFXI log directory.
        /// </summary>
        public override void Stop()
        {
            if (IsRunning == false)
                return;

            try
            {
                // Stop watching for new files.
                fileSystemWatcher.EnableRaisingEvents = false;

                if (networkWatchTimer != null)
                {
                    networkWatchTimer.Dispose();
                    networkWatchTimer = null;
                }

                // Notify MessageManager that we're done so it can turn off its timer loop.
                MessageManager.Instance.StopParsing();

            }
            finally
            {
                IsRunning = false;
            }
        }
        #endregion

        #region Implement specific monitoring functions
        /// <summary>
        /// Handle reading each of any existing log files in the FFXI directory.
        /// </summary>
        private void ReadExistingFFXILogs()
        {
            string[] files = Directory.GetFiles(WatchDirectory, "*.log");

            // Sort the files by timestamp written to
            SortedList<DateTime, string> sortedFiles = new SortedList<DateTime, string>(files.Length);
            FileInfo fi;

            foreach (string file in files)
            {
                fi = new FileInfo(file);
                sortedFiles.Add(fi.LastWriteTimeUtc, file);
            }

            // Process the files in sorted order
            foreach (var file in sortedFiles)
            {
                ReadFFXILog(file.Value);
            }
        }

        /// <summary>
        /// Initiate the timer for periodic polling of the network drive.
        /// </summary>
        private void StartMonitoringNetworkDrive()
        {
            // Create timer to run every 5 seconds.
            networkWatchTimestamp = DateTime.Now;
            networkWatchTimer = new Timer(MonitorNetworkDrive, null, 5000, 5000);
        }

        /// <summary>
        /// Examine the network drive for file changes.  Process any newly modified files
        /// by creating a bogus event to mimic the local drive monitoring code.
        /// </summary>
        /// <param name="stateInfo">Unused.</param>
        private void MonitorNetworkDrive(Object stateInfo)
        {
            string[] files = Directory.GetFiles(WatchDirectory, "*.log");

            // Sort files by modification timestamp
            SortedList<DateTime, FileInfo> sortedFiles = new SortedList<DateTime, FileInfo>(files.Length);
            FileInfo fi;

            foreach (string file in files)
            {
                fi = new FileInfo(file);
                sortedFiles.Add(fi.LastWriteTimeUtc, fi);
            }

            // Check each entry in the sorted list.  If it's a newly modified file,
            // generate a new event for it.
            foreach (var file in sortedFiles)
            {
                if (file.Value.LastWriteTime > networkWatchTimestamp)
                {
                    FileSystemEventArgs eArgs = new FileSystemEventArgs(WatcherChangeTypes.Changed,
                        file.Value.Directory.Name, file.Value.Name);

                    MonitorLogDirectory(this, eArgs);

                    networkWatchTimestamp = file.Value.LastWriteTime;
                }
            }
        }

        /// <summary>
        /// Event handler that is activated when any changes are made to files
        /// in the log directory.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e">EventArgs about the file that changed.</param>
        private void MonitorLogDirectory(object source, FileSystemEventArgs eArg)
        {
            try
            {
                if ((eArg.ChangeType == WatcherChangeTypes.Changed) ||
                    (eArg.ChangeType == WatcherChangeTypes.Created))
                {
                    if (lastChangedFile != eArg.FullPath)
                    {
                        ReadFFXILog(eArg.FullPath);
                        lastChangedFile = eArg.FullPath;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e);
            }
        }

        #endregion

        #region General log file reading and writing
        /// <summary>
        /// Read the specified FFXI log and send the extracted data for further processing.
        /// Save the extracted text.
        /// </summary>
        /// <param name="fileName">The name of the file to read.</param>
        private void ReadFFXILog(string fileName)
        {
            if (File.Exists(fileName) == false)
                throw new ArgumentException(string.Format("File: {0}\ndoes not exist.", fileName));
            
            DateTime fileTimeStamp = File.GetLastWriteTime(fileName);

            string fileText = string.Empty;
            bool finishedReading = false;
            uint totalWaitTime = 0;

            while ((finishedReading == false) && (totalWaitTime < 5000))
            {
                try
                {
                    using (FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (StreamReader sr = new StreamReader(fileStream, Encoding.ASCII))
                        {
                            // Ignore 0x64 (100) byte header
                            sr.BaseStream.Seek(0x64, SeekOrigin.Begin);

                            // Read the remainder of the file
                            fileText = sr.ReadToEnd();

                            finishedReading = true;
                        }
                    }
                }
                catch (IOException)
                {
                    // Catch exceptions of unable to open file because another process (FFXI)
                    // is using it. Wait briefly before trying again.  
                    Thread.Sleep(500);
                    totalWaitTime += 500;
                }
            }

            if (finishedReading == true)
                ProcessRawLogText(fileText, fileTimeStamp);
        }

        /// <summary>
        /// Read the specified FFXI parsed log and send the extracted data for further processing.
        /// Do not save (we're reading from a saved compilation already).
        /// </summary>
        /// <param name="fileName">The name of the file to read.</param>
        [Obsolete("No longer saving parses as text file, but as database files.  Shouldn't need this.")]
        private void ReadParserLog(string fileName)
        {
            if (File.Exists(fileName) == false)
                throw new ArgumentException(string.Format("File: {0}\ndoes not exist.", fileName));

            string fileText;

            // Create an instance of StreamReader to read from a file.
            // The using statement also closes the StreamReader.
            using (StreamReader sr = new StreamReader(fileName, Encoding.ASCII))
            {
                // There is no header in saved parses, so just read the entire file.
                fileText = sr.ReadToEnd();
            }

            ProcessRawLogText(fileText, File.GetLastWriteTime(fileName));
        }

        /// <summary>
        /// Breaks the text glob provided into component chat lines to be parsed.
        /// </summary>
        /// <param name="fileText">The parseable portion of a log file or saved parse.</param>
        /// <param name="timeStamp">The timestamp from the file being read.</param>
        private void ProcessRawLogText(string fileText, DateTime timeStamp)
        {
            string[] fileLines;

            // Each line in the log file is delimited by a value of 0x00.
            string delimStr = "\0";
            char[] delimiter = delimStr.ToCharArray();

            // Split the text up into individual lines.
            fileLines = fileText.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

            if (fileLines.Length > 0)
            {
                List<ChatLine> chatLines = new List<ChatLine>(fileLines.Length);

                foreach (string line in fileLines)
                {
                    try
                    {
                        ChatLine chatLine = new ChatLine(line, timeStamp);
                        chatLines.Add(chatLine);

                        // TODO: Remove this once the MessageManager is set up to
                        // watch for the event handler pushes.
                        MessageManager.Instance.AddChatLine(chatLine);
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.Log(ex);
                    }
                }

                if (chatLines.Count > 0)
                    OnReaderDataChanged(new ReaderDataEventArgs(chatLines));
            }
        }
        #endregion
    }
}
