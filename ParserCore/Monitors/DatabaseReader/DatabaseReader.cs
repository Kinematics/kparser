using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using WaywardGamers.KParser.Interface;

namespace WaywardGamers.KParser.Monitoring
{
    /// <summary>
    /// Class to handle interfacing with system RAM reading
    /// in order to monitor the FFXI process space and read
    /// log info to be parsed.
    /// </summary>
    public class DatabaseReader : AbstractReader
    {
        #region Singleton Constructor
        // Make the class a singleton
        private static readonly DatabaseReader instance = new DatabaseReader();

        /// <summary>
        /// Gets the singleton instance of the LogParser class.
        /// This is the only internal way to access the singleton.
        /// </summary>
        public static DatabaseReader Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Private constructor ensures singleton purity.
        /// </summary>
        private DatabaseReader()
        {
        }
        #endregion

        #region Member Variables
        private Thread readerThread;
        public event ReaderStatusHandler ReparseProgressChanged;
        #endregion

        #region Event Management
        protected virtual void OnRowProcessed(ReaderStatusEventArgs e)
        {
            if (ReparseProgressChanged != null)
            {
                // Invokes the delegates. 
                ReparseProgressChanged(this, e);
            }
        }

        #endregion

        #region Interface Control Methods and Properties
        /// <summary>
        /// Return type of DataSource this reader works on.
        /// </summary>
        public override DataSource ParseModeType { get { return DataSource.Database; } }

        /// <summary>
        /// Start a thread that reads log files for parsing.
        /// </summary>
        public override void Start()
        {
            IsRunning = true;

            try
            {
                // Reset the thread
                if ((readerThread != null) &&
                    ((readerThread.ThreadState == System.Threading.ThreadState.Running) ||
                    (readerThread.ThreadState == System.Threading.ThreadState.Background)))
                {
                    readerThread.Abort();
                }

                // Begin the thread
                readerThread = new Thread(RunThread);
                readerThread.IsBackground = true;
                readerThread.Name = "Read database thread";
                readerThread.Start();

                // Notify MessageManager that we're starting.
                MessageManager.Instance.StartParsing(false);
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e);
                IsRunning = false;
                MessageManager.Instance.CancelParsing();
                DatabaseReadingManager.Instance.CloseDatabase();
            }
        }

        public void RunThread()
        {
            int rowCount = 0;
            int totalCount = 0;
            bool completed = false;

            try
            {
                KPDatabaseReadOnly readDataSet = DatabaseReadingManager.Instance.Database;
                if (readDataSet != null)
                {
                    totalCount = readDataSet.RecordLog.Count;

                    // Read the (fixed) record log from the database, reconstruct
                    // the chat line, and send it to the new database.
                    using (new ProfileRegion("Reparse: Read database and parse"))
                    {
                        foreach (var logLine in readDataSet.RecordLog)
                        {
                            rowCount++;
                            if (IsRunning == false)
                                break;

                            ChatLine chat = new ChatLine(logLine.MessageText, logLine.Timestamp);
                            MessageManager.Instance.AddChatLine(chat);

                            OnRowProcessed(new ReaderStatusEventArgs(rowCount, totalCount, completed));
                        }

                        completed = IsRunning;
                    }
                }
                else
                {
                    throw new InvalidDataException("No database to parse.");
                }
            }
            catch (ThreadAbortException e)
            {
                Logger.Instance.Log(e);
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e);
            }
            finally
            {
                IsRunning = false;
                if (completed == true)
                    MessageManager.Instance.StopParsing();
                else
                    MessageManager.Instance.CancelParsing();

                OnRowProcessed(new ReaderStatusEventArgs(rowCount, totalCount, completed));
                DatabaseReadingManager.Instance.CloseDatabase();
            }
        }

        /// <summary>
        /// Stop the active reader thread.
        /// </summary>
        public override void Stop()
        {
            if (IsRunning == false)
                return;

            // Notify MessageManager that we're done so it can turn off its timer loop.
            MessageManager.Instance.CancelParsing();

            //if ((readerThread != null) &&
            //    ((readerThread.ThreadState == System.Threading.ThreadState.Running) ||
            //    (readerThread.ThreadState == System.Threading.ThreadState.Background)))
            //{
            //    readerThread.Abort();
            //}

            IsRunning = false;
        }


        public override void Import(ImportSource importSource)
        {
            switch (importSource)
            {
                case ImportSource.DVSParse:
                case ImportSource.DirectParse:
                    ImportDirectParseV1();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        #endregion

        #region Import functions (slightly rewritten Reparse functions)
        private void ImportDirectParseV1()
        {
            IsRunning = true;

            try
            {
                // Reset the thread
                if ((readerThread != null) && (readerThread.ThreadState == System.Threading.ThreadState.Running))
                {
                    readerThread.Abort();
                }

                // Begin the thread
                readerThread = new Thread(ImportDirectParseV1IMPL);
                readerThread.IsBackground = true;
                readerThread.Name = "Import database thread";
                readerThread.Start();

                // Notify MessageManager that we're starting.
                MessageManager.Instance.StartParsing(false);
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e);
                IsRunning = false;
                MessageManager.Instance.CancelParsing();
                ImportDirectParseManager.Instance.CloseDatabase();
            }
        }

        private void ImportDirectParseV1IMPL()
        {
            int rowCount = 0;
            int totalCount = 0;
            bool completed = false;

            string originalChatLine;
            string breakCodes;
            string breakCodesChars;
            uint breakCharVal;
            char breakChar;

            // For DVSD files:
            // logLine.RawHeader == "bf,00,00,60808080,00000288,000002f1,0034,00,01,00,00,(1E011E01)"
            // logLine.Text == "The Goblin Tinkerer seems like a decent challenge."
            // For DPD files:
            // logLine.RawHeader == "bf,00,00,60808080,00000288,000002f1,0034,00,01,00,00,"
            // logLine.Text == "The Goblin Tinkerer seems like a decent challenge."
            // Need to combine those two lines back into the original

            Regex rawHeaderRegex = new Regex(@"^(?<codes>((\w|\d)+,){11})(\((?<breakCodes>(\w|\d)+)\))?$");
            Match rawHeaderMatch;

            try
            {

                DPDatabaseImportV1 readDataSet = ImportDirectParseManager.Instance.Database;
                if (readDataSet != null)
                {
                    totalCount = readDataSet.ChatLog.Count;

                    // Read the (fixed) record log from the database, reconstruct
                    // the chat line, and send it to the new database.
                    using (new ProfileRegion("Import: Read database and parse"))
                    {
                        foreach (var logLine in readDataSet.ChatLog)
                        {
                            rowCount++;
                            if (IsRunning == false)
                                break;

                            rawHeaderMatch = rawHeaderRegex.Match(logLine.RawHeader);

                            if (rawHeaderMatch.Success == true)
                            {
                                originalChatLine = rawHeaderMatch.Groups["codes"].Value;
                                breakCodes = rawHeaderMatch.Groups["breakCodes"].Value;

                                if (breakCodes.Length > 0)
                                {
                                    while (breakCodes.Length > 0)
                                    {
                                        breakCodesChars = breakCodes.Substring(0, 2);

                                        if (uint.TryParse(breakCodesChars, NumberStyles.AllowHexSpecifier, null, out breakCharVal))
                                        {
                                            breakChar = (char)breakCharVal;
                                            originalChatLine += breakChar;
                                        }

                                        if (breakCodes.Length > 2)
                                            breakCodes = breakCodes.Substring(2);
                                        else
                                            breakCodes = string.Empty;
                                    }
                                }
                                else
                                {
                                    // Force insert basic break characters if they weren't saved in the
                                    // imported database, so that they get parsed properly by our tokenizer.

                                    originalChatLine += (char)0x1e;
                                    originalChatLine += (char)0x01;
                                    originalChatLine += (char)0x1e;
                                    originalChatLine += (char)0x01;
                                }

                                originalChatLine += logLine.Text;

                                ChatLine chat = new ChatLine(originalChatLine, logLine.DateTime);
                                MessageManager.Instance.AddChatLine(chat);
                            }

                            OnRowProcessed(new ReaderStatusEventArgs(rowCount, totalCount, completed));
                        }

                        completed = IsRunning;
                    }
                }
                else
                {
                    throw new InvalidDataException("No database to parse.");
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e);
            }
            finally
            {
                IsRunning = false;
                if (completed == true)
                    MessageManager.Instance.StopParsing();
                else
                    MessageManager.Instance.CancelParsing();

                OnRowProcessed(new ReaderStatusEventArgs(rowCount, totalCount, completed));
                ImportDirectParseManager.Instance.CloseDatabase();
            }
        }
        #endregion

    }
}
