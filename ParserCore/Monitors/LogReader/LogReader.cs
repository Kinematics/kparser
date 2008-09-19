using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WaywardGamers.KParser.Interface;

namespace WaywardGamers.KParser.Monitoring
{
    /// <summary>
    /// Class that handles parsing the FFXI Log files.
    /// </summary>
    internal class LogReader : IReader
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

        DriveInfo driveInfo;
        FileSystemWatcher fileSystemWatcher;
        Timer networkWatchTimer;

        DateTime networkWatchTimestamp;
        #endregion

        #region Interface Control Methods and Properties
        /// <summary>
        /// Gets (publicly) and sets (privately) the state of the
        /// reader thread.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Activate the file system watcher so that we can catch events when files change.
        /// If the option to parse existing files is true, run the parsing code on them.
        /// </summary>
        public void Run()
        {
            IsRunning = true;

            try
            {
                // Get the settings so that we know where to look for the log files.
                appSettings.Reload();

                // Verify the drive
                string directoryRoot = Path.GetPathRoot(appSettings.FFXILogDirectory);
                driveInfo = new DriveInfo(directoryRoot);

                if (driveInfo.IsReady == false)
                    throw new InvalidOperationException("Drive for the specified log directory is not available.");

                MessageManager.Instance.PrepareToStartParsing();

                // Run the parser on any logs already in existance before starting to monitor,
                // if that option is set.
                if (appSettings.ParseExistingLogs == true)
                {
                    ReadExistingFFXILogs(appSettings.FFXILogDirectory);
                }

                // Notify MessageManager that we're starting.
                MessageManager.Instance.StartParsing(true);

                if ((driveInfo.DriveType != DriveType.Fixed) && (driveInfo.DriveType != DriveType.Network))
                {
                    // Stop the message manager in case user wants to just run parse on existing
                    // files from a fixed medium (eg: CDROM).
                    MessageManager.Instance.StopParsing();
                    throw new InvalidOperationException("Drive is not of a type that can be monitored");
                }

                // Set up monitoring of log files for changes.
                fileSystemWatcher.Path = appSettings.FFXILogDirectory;

                // Begin watching.
                if (driveInfo.DriveType == DriveType.Fixed)
                    fileSystemWatcher.EnableRaisingEvents = true;
                else if (driveInfo.DriveType == DriveType.Network)
                    StartMonitorNetworkDrive();
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

        private void StartMonitorNetworkDrive()
        {
            // Create timer to run every 5 seconds.
            networkWatchTimer = new Timer(MonitorNetworkDrive, null, 5000, 5000);
            networkWatchTimestamp = DateTime.Now;
        }

        private void MonitorNetworkDrive(Object stateInfo)
        {
            // Reuse info from fileSystemWatcher
            string watchDirectory = fileSystemWatcher.Path;

            string[] files = Directory.GetFiles(watchDirectory, "*.log");

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
            for (int i = 0; i < sortedFiles.Count; i++)
            {
                if (sortedFiles.Values[i].LastWriteTime > networkWatchTimestamp)
                {
                    FileSystemEventArgs eArgs = new FileSystemEventArgs(WatcherChangeTypes.Changed,
                        sortedFiles.Values[i].Directory.Name, sortedFiles.Values[i].Name);

                    MonitorLogDirectory(this, eArgs);

                    networkWatchTimestamp = sortedFiles.Values[i].LastWriteTime;
                }
            }
        }


        /// <summary>
        /// Stop monitoring the FFXI log directory.
        /// </summary>
        public void Stop()
        {
            if (IsRunning == false)
                return;

            // Stop watching for new files.
            fileSystemWatcher.EnableRaisingEvents = false;

            if (networkWatchTimer != null)
            {
                networkWatchTimer.Dispose();
                networkWatchTimer = null;
            }
        
            // Notify MessageManager that we're done so it can turn off its timer loop.
            MessageManager.Instance.StopParsing();

            IsRunning = false;
        }


        public void Import(ImportSource importSource)
        {
            throw new InvalidOperationException();
        }
        #endregion

        #region General log file reading and writing
        /// <summary>
        /// Event handler that is activated when any changes are made to files
        /// in the log directory.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void MonitorLogDirectory(object source, FileSystemEventArgs eArg)
        {
            try
            {
                if (eArg.ChangeType == WatcherChangeTypes.Changed)
                    ReadFFXILog(eArg.FullPath);
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e);
            }
        }

        /// <summary>
        /// Handle reading each of any existing log files in the FFXI directory.
        /// </summary>
        private void ReadExistingFFXILogs(string watchDirectory)
        {
            string[] files = Directory.GetFiles(watchDirectory, "*.log");

            SortedList sortedFiles = new SortedList(files.Length);
            FileInfo fi;

            foreach (string file in files)
            {
                fi = new FileInfo(file);
                sortedFiles.Add(fi.LastWriteTimeUtc, file);
            }

            for (int i = 0; i < sortedFiles.Count; i++)
            {
                ReadFFXILog(sortedFiles.GetByIndex(i).ToString());
            }
        }

        /// <summary>
        /// Read the specified FFXI log and send the extracted data for further processing.
        /// Save the extracted text.
        /// </summary>
        /// <param name="fileName">The name of the file to read.</param>
        private void ReadFFXILog(string fileName)
        {
            if (File.Exists(fileName) == false)
                throw new ArgumentException(string.Format("File: {0}\ndoes not exist.", fileName));

            string fileText = string.Empty;
            bool finishedReading = false;
            uint totalWaitTime = 0;

            while ((finishedReading == false) && (totalWaitTime < 5000))
            {
                try
                {
                    using (FileStream fileStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        using (StreamReader sr = new StreamReader(fileStream, Encoding.ASCII))
                        {
                            // Ignore 0x64 (100) byte header
                            sr.BaseStream.Seek(0x64, SeekOrigin.Begin);

                            // There is no header in saved parses, so just read the entire file.
                            fileText = sr.ReadToEnd();

                            finishedReading = true;
                        }
                    }
                }
                catch (IOException iox)
                {
                    // Catch exceptions of unable to open file because another process (FFXI)
                    // is using it. Wait briefly before trying again.  
                    Thread.Sleep(500);
                    totalWaitTime += 500;
                }
            }


            // Create an instance of StreamReader to read from a file.
            // The using statement also closes the StreamReader.
            //using (StreamReader sr = new StreamReader(fileName, Encoding.ASCII))
            //{
            //    // Ignore 0x64 (100) byte header
            //    sr.BaseStream.Seek(0x64, SeekOrigin.Begin);

            //    // There is no header in saved parses, so just read the entire file.
            //    fileText = sr.ReadToEnd();
            //}

            if (finishedReading == true)
                ProcessRawLogText(fileText, File.GetLastWriteTime(fileName));
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
                foreach (string line in fileLines)
                {
                    // Ignore empty lines (it's possible the split may leave one dangling).
                    if (line != "")
                    {
                        ChatLine chatLine = new ChatLine(line, timeStamp);

                        MessageManager.Instance.AddChatLine(chatLine);
                    }
                }
            }

            return;
        }
        #endregion
    }
}
