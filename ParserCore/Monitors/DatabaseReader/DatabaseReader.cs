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
        private IDBReader dbReaderManager;
        #endregion

        #region Interface Control Methods and Properties
        /// <summary>
        /// Return type of DataSource this reader works on.
        /// </summary>
        public override DataSource ParseModeType { get { return DataSource.Database; } }

        public override void Import(ImportSourceType importSource, IDBReader dbReaderManager)
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

                this.dbReaderManager = dbReaderManager;
                readerThread = null;

                // Create the thread
                switch (importSource)
                {
                    case ImportSourceType.KParser:
                        if (dbReaderManager is KParserReadingManager)
                            readerThread = new Thread(ImportKParserDB);
                        break;
                    case ImportSourceType.DVSParse:
                    case ImportSourceType.DirectParse:
                        if (dbReaderManager is DirectParseReadingManager)
                            readerThread = new Thread(ImportDirectParseDB);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                readerThread.IsBackground = true;
                readerThread.Name = "Read database thread";
                readerThread.Start();
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e);
                IsRunning = false;
                OnReaderStatusChanged(new ReaderStatusEventArgs(0, 0, false, true));

                dbReaderManager.CloseDatabase();
            }


        }

        /// <summary>
        /// Stop the active reader thread.
        /// </summary>
        public override void Stop()
        {
            //if ((readerThread != null) &&
            //    ((readerThread.ThreadState == System.Threading.ThreadState.Running) ||
            //    (readerThread.ThreadState == System.Threading.ThreadState.Background)))
            //{
            //    readerThread.Abort();
            //}

            IsRunning = false;
        }
        #endregion

        #region Importing implementations
        /// <summary>
        /// Import (reparse) KParser database files.
        /// </summary>
        private void ImportKParserDB()
        {
            int rowCount = 0;
            int totalCount = 0;
            bool completed = false;

            try
            {
                KParserReadingManager readingManager = dbReaderManager as KParserReadingManager;
                if (readingManager == null)
                    throw new ArgumentNullException();

                KPDatabaseReadOnly readDataSet = readingManager.Database;
                if (readDataSet != null)
                {
                    List<ChatLine> chatLines = new List<ChatLine>(100);

                    totalCount = readDataSet.RecordLog.Count;

                    // Read the (fixed) record log from the database, reconstruct
                    // the chat line, and send it to the new database.
                    using (new RegionProfiler("Reparse: Read database and parse"))
                    {
                        foreach (var logLine in readDataSet.RecordLog)
                        {
                            rowCount++;
                            if (IsRunning == false)
                                break;

                            chatLines.Add(new ChatLine(logLine.MessageText, logLine.Timestamp));

                            OnReaderStatusChanged(new ReaderStatusEventArgs(rowCount, totalCount, completed, false));

                            if (chatLines.Count > 99)
                            {
                                OnReaderDataChanged(new ReaderDataEventArgs(chatLines));
                                chatLines = new List<ChatLine>(100);
                            }
                        }

                        if (chatLines.Count > 0)
                        {
                            OnReaderDataChanged(new ReaderDataEventArgs(chatLines));
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
                OnReaderStatusChanged(new ReaderStatusEventArgs(rowCount, totalCount, completed, (completed == false)));
                dbReaderManager.CloseDatabase();
            }
        }

        /// <summary>
        /// Import database files created by DVSParse and DirectParse.
        /// </summary>
        private void ImportDirectParseDB()
        {
            int rowCount = 0;
            int totalCount = 0;
            bool completed = false;

            string originalChatLine;
            string breakCodes;
            string breakCodesChars;
            uint breakCharVal;
            char breakChar;

            List<ChatLine> chatLines = new List<ChatLine>(100);

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
                DirectParseReadingManager readingManager = dbReaderManager as DirectParseReadingManager;
                if (readingManager == null)
                    throw new ArgumentNullException();

                DPDatabaseImportV1 readDataSet = readingManager.Database;
                if (readDataSet != null)
                {
                    totalCount = readDataSet.ChatLog.Count;

                    // Read the (fixed) record log from the database, reconstruct
                    // the chat line, and send it to the new database.
                    using (new RegionProfiler("Import: Read database and parse"))
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

                                chatLines.Add(new ChatLine(originalChatLine, logLine.DateTime));
                            }

                            OnReaderStatusChanged(new ReaderStatusEventArgs(rowCount, totalCount, completed, false));

                            if (chatLines.Count > 99)
                            {
                                OnReaderDataChanged(new ReaderDataEventArgs(chatLines));
                                chatLines = new List<ChatLine>(100);
                            }
                        }

                        if (chatLines.Count > 0)
                        {
                            OnReaderDataChanged(new ReaderDataEventArgs(chatLines));
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
                OnReaderStatusChanged(new ReaderStatusEventArgs(rowCount, totalCount, completed, (completed != true)));
                dbReaderManager.CloseDatabase();
            }
        }
        #endregion
    }
}
