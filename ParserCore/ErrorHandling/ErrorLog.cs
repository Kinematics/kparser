using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;
using System.Reflection;

namespace WaywardGamers.KParser
{
	/// <summary>
	/// Error logging class.
	/// </summary>
	public class Logger
	{
		#region Static Singleton Members
		private static Logger instance;
		public static Logger Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new Logger();
				}

				return instance;
			}
		}
		#endregion

		#region Member Variables
		string logFileName;
		string breakString;

        Properties.Settings programSettings;

        Exception lastException = null;
        int duplicateCount = 0;
		#endregion

		#region Constructor
		/// <summary>
		/// Construct a new instance of the Logger.
		/// </summary>
		private Logger()
		{
            programSettings = new WaywardGamers.KParser.Properties.Settings();
            FileInfo assemInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            logFileName = Path.Combine(assemInfo.DirectoryName, "error.log");
			breakString = "---------------------------------------------------------------";

            TrimLogFile();
		}
		#endregion

		#region Properties
		/// <summary>
		/// The path and filename of the file to save log information to.
		/// </summary>
		public string LogFileName
		{
			get
			{
				return logFileName;
			}
			set
			{
				logFileName = value;
			}
		}
		#endregion

		#region Public Methods
		/// <summary>
		/// Log arbitrary text to the log file, primarily for debugging purposes.
		/// </summary>
		/// <param name="label">The text to go before the message to give
		/// it a distinguishable label.</param>
		/// <param name="message">The text to be logged.</param>
		public void Log(string label, string message)
		{
			this.Log(label, message, ErrorLevel.Info);
		}

		/// <summary>
		/// Log arbitrary text to the log file, primarily for debugging purposes.
		/// </summary>
		/// <param name="label">The text to go before the message to give
		/// it a distinguishable label.</param>
		/// <param name="message">The text to be logged.</param>
		public void Log(string label, string message, ErrorLevel severity)
		{
            programSettings.Reload();

			// If error logging is turned off, just return.
            if (programSettings.ErrorLoggingLevel == ErrorLevel.None)
				return;

            if (severity >= programSettings.ErrorLoggingLevel)
			{
				try
				{
                    using (StreamWriter sw = File.AppendText(logFileName))
                    {
                        WriteHeader(sw, label, message, severity);
                        WriteSeparator(sw);
                    }
                }
				catch (Exception)
				{
                    Debug.WriteLine("Error writing log:\n"+message);
				}
			}
		}

        /// <summary>
        /// Log a parsed Message to the error log.
        /// </summary>
        /// <param name="label">Label for the logged error/message.</param>
        /// <param name="message">Message to be written to the log.</param>
        internal void Log(string label, Message message)
        {
            programSettings.Reload();

            // If error logging is turned off, just return.
            if (programSettings.ErrorLoggingLevel == ErrorLevel.None)
                return;

            try
            {
                using (StreamWriter sw = File.AppendText(logFileName))
                {
                    WriteHeader(sw, label, message.ToString(), ErrorLevel.Debug);
                    WriteSeparator(sw);
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Error writing log:\n" + message);
            }
        }

        /// <summary>
        /// Log a parsed Message to the error log.
        /// </summary>
        /// <param name="label">Label for the logged error/message.</param>
        /// <param name="message">Message to be written to the log.</param>
        internal void Log(Exception e, Message message)
        {
            programSettings.Reload();

            // If error logging is turned off, just return.
            if (programSettings.ErrorLoggingLevel == ErrorLevel.None)
                return;

            try
            {
                using (StreamWriter sw = File.AppendText(logFileName))
                {
                    Log(e);
                    WriteHeader(sw, "Error generated with message:", message.ToString(), ErrorLevel.Debug);
                    WriteSeparator(sw);
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Error writing log:\n" + message + "\n" + e.StackTrace);
            }
        }

        /// <summary>
        /// Log a parsed MesssageLine to the error log with an exception.
        /// </summary>
        /// <param name="e">Exception that was thrown.</param>
        /// <param name="messageLine">MessageLine to be logged.</param>
        internal void Log(Exception e, MessageLine messageLine)
        {
            if (messageLine == null)
                Log(e, "");
            else
                Log(e, messageLine.OriginalText);
        }

        /// <summary>
        /// Shortcut versions for the call to log exceptions.
        /// </summary>
        /// <param name="e">The exception to be logged.</param>
        public void Log(Exception e)
        {
            Log(e, "");
        }

        /// <summary>
		/// Log any other exceptions.
		/// </summary>
		/// <param name="e">The exception to be logged.</param>
		/// <param name="message">Optional message that can be passed
		/// in at the time the exception is caught.</param>
		public void Log(Exception e, string message)
		{
            if (IsDuplicate(e))
            {
                using (StreamWriter sw = File.AppendText(logFileName))
                {
                    if (duplicateCount == 0)
                        sw.WriteLine();

                    sw.WriteLine(string.Format("Duplicate: {0} @ {1:f}", ++duplicateCount, DateTime.Now));
                }
                return;
            }
            else
            {
                duplicateCount = 0;
            }

			try
			{
                using (StreamWriter sw = File.AppendText(logFileName))
                {
                    WriteSeparator(sw);

                    WriteHeader(sw, e.GetType().ToString(), message, ErrorLevel.Error);
                    sw.Write(e.ToString());
                    sw.WriteLine();

                    Exception subException = e.InnerException;
                    int innerLoopCheck = 0;
                    while ((subException != null) && (innerLoopCheck < 5))
                    {
                        sw.WriteLine("Contained Exception:");
                        sw.Write(subException.ToString());
                        sw.WriteLine();
                        subException = subException.InnerException;
                        innerLoopCheck++;
                    }

                    if (innerLoopCheck >= 5)
                    {
                        sw.WriteLine("--Interrupt possible infinite loop--");
                    }
                }
			}
			catch (Exception)
			{
                Debug.WriteLine("Error writing log:\n" + message + "\n" + e.StackTrace);
            }
		}

        /// <summary>
        /// Check to see whether the specified exception is a duplicate
        /// of the most recent exception.  If so, we'll want to suppress
        /// the full log output.
        /// </summary>
        /// <param name="e">The exception to check against.</param>
        /// <returns>True if it's a duplicate of the last recorded exception.  Otherwise false.</returns>
        private bool IsDuplicate(Exception e)
        {
            if (lastException == null)
            {
                lastException = e;
                return false;
            }

            if (e.GetType() == lastException.GetType())
            {
                if ((e.Message == lastException.Message) &&
                    (e.StackTrace == lastException.StackTrace))
                {
                    if (!((e.InnerException == null) ^ (lastException.InnerException == null)))
                    {
                        return true;
                    }
                }
            }

            lastException = e;
            return false;
        }

		/// <summary>
		/// Log any exceptions that cause the program to terminate.
		/// </summary>
		/// <param name="e">The exception to be logged.</param>
		public void FatalLog(Exception e)
		{
			try
			{
                using (StreamWriter sw = File.AppendText(logFileName))
                {
                    WriteSeparator(sw);
                    sw.WriteLine("******* FATAL EXCEPTION !!!! *********\n");
                    WriteHeader(sw, e.GetType().ToString(), "", ErrorLevel.Error);
                    sw.Write(e.ToString());
                    sw.WriteLine();
                }
			}
			catch (Exception)
			{
				MessageBox.Show("FATAL EXCEPTION (unable to write log file):\n" + e.Message);
			}
		}

		/// <summary>
		/// Trim excess log file data from the log to prevent unbounded growth.
		/// </summary>
		public void TrimLogFile()
		{
            programSettings.Reload();

			Queue logQueue = new Queue();
			Array logArray;
			StreamReader sr;
			DateTime timestamp;
			int findMarker;
			TimeSpan timeSpan;
            int keepFromLine;

			if (File.Exists(logFileName) == true)
			{
				try
				{
					using (sr = new StreamReader(logFileName)) 
					{
						string line;
						// Read lines from file and place in queue for processing.
						while ((line = sr.ReadLine()) != null) 
						{
							logQueue.Enqueue(line);
						}
					}

					// If no lines in file, quit.
					if (logQueue.Count == 0)
						return;

					// Copy to array for processing
					logArray = logQueue.ToArray();
                    keepFromLine = 0;
					
					for (int i = 0; i < logArray.Length; i++)
					{
                        if (DateTime.TryParse(logArray.GetValue(i).ToString(), out timestamp) == true)
                        {
 							timeSpan = DateTime.Now - timestamp;

							// Determine if the log entry is outside our retention period.
                            if (timeSpan > TimeSpan.FromDays(programSettings.DaysToRetainErrorLogs))
							{
								findMarker = Array.IndexOf(logArray, breakString, i);

								// If we can't find another marker, we've reached the end of the file.
								if (findMarker < 0)
								{
									i = logArray.Length;
									break;
								}

								// Update loop position
                                keepFromLine = findMarker + 1;
                                i = findMarker;
							}
							else
							{
								// End loop when we find a log inside the time limit
								break;
							}
                        }
					}

                    using (StreamWriter sw = File.CreateText(logFileName))
                    {
                        for (int j = keepFromLine; j < logArray.Length; j++)
                        {
                            sw.WriteLine(logArray.GetValue(j).ToString());
                        }
                    }
				}
				catch (Exception)
				{
                    Debug.WriteLine("Error trimming log file.");
                }
			}
		}
		#endregion

		#region Private Methods
        /// <summary>
        /// Write the preliminary log text, including timestamp and version info.
        /// </summary>
        /// <param name="sw">The stream to write the log to.</param>
        /// <param name="title">Title associated with log.</param>
        /// <param name="message">Message to be included with log.</param>
        /// <param name="severity">The severity of the error being logged.</param>
        private void WriteHeader(StreamWriter sw, string label, string message, ErrorLevel severity)
        {
            int count = 0;

            sw.WriteLine("{0:f}", DateTime.Now);
            sw.WriteLine(Assembly.GetCallingAssembly().FullName);
            sw.WriteLine("Error severity level: {0}", severity.ToString());
            sw.WriteLine();

            if ((label != null) && (label != ""))
            {
                sw.WriteLine(label);
                count++;
            }

            if ((message != null) && (message != ""))
            {
                sw.WriteLine(string.Format("Message: {0}", message));
                count++;
            }

            if (count > 0)
                sw.WriteLine();
        }

        /// <summary>
        /// Write the footer for the log to divide log entries.
        /// </summary>
        /// <param name="sw">The stream to write the log to.</param>
        private void WriteSeparator(StreamWriter sw)
        {
            sw.WriteLine();
            sw.WriteLine(breakString);
        }
		#endregion
	}
}
