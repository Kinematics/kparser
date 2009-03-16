using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using WaywardGamers.KParser.Monitoring.Memory;
using WaywardGamers.KParser.Interface;

namespace WaywardGamers.KParser.Monitoring
{
    /// <summary>
    /// This class is created and has its monitor function run as a thread.
    /// It checks for changes in the POL chat log, and when detected reads
    /// that log data as chat data lines.  That collection of lines is stored
    /// in the event data of RamWatchEventArgs, and then any watchers of
    /// this class (ie: RamReader) are notified by firing the event delegates.
    /// The watching class then has the duty of dissecting the chat lines and
    /// sending them to be parsed.
    /// </summary>

    /// <summary>
    /// Class to handle interfacing with system RAM reading
    /// in order to monitor the FFXI process space and read
    /// log info to be parsed.
    /// </summary>
    internal class RamReader : AbstractReader
    {
        #region Singleton Constructor
        // Make the class a singleton
        private static readonly RamReader instance = new RamReader();

        /// <summary>
        /// Gets the singleton instance of the LogParser class.
        /// This is the only internal way to access the singleton.
        /// </summary>
        internal static RamReader Instance { get { return instance; } }

        /// <summary>
        /// Private constructor ensures singleton purity.
        /// </summary>
        private RamReader()
		{
        }
		#endregion

        #region Member Variables
        Properties.Settings appSettings = new WaywardGamers.KParser.Properties.Settings();

        Thread readerThread;

        int polPID = 0;
        POL pol;
        uint initialMemoryOffset;
        ChatLogLocationInfo chatLogLocation;

        bool abortMonitorThread;
        #endregion

        #region Interface Control Methods and Properties

        /// <summary>
        /// Return type of DataSource this reader works on.
        /// </summary>
        public override DataSource ParseModeType { get { return DataSource.Ram; } }

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

                // Make sure we have the latest version of the app settings data.
                appSettings.Reload();

                // Update the memory offset of the thread class before starting.
                initialMemoryOffset = appSettings.MemoryOffset;

                // If the user requests that they be allowed to specify the particular
                // POL process, bring up a form to determine that value.  If not found
                // or not requested, set the polPID to 0 to signal that the later
                // functions should search for it normally.
                if (appSettings.SpecifyPID == true)
                {
                    SelectPOLProcess selectPID = new SelectPOLProcess();
                    if (selectPID.ShowDialog() == DialogResult.OK)
                    {
                        polPID = selectPID.SelectedPID;
                    }
                    else
                    {
                        polPID = 0;
                    }
                }
                else
                {
                    polPID = 0;
                }

                // Begin the thread
                readerThread = new Thread(Monitor);
                readerThread.IsBackground = true;
                readerThread.Name = "Memory Monitor Thread";
                readerThread.Start();
            }
            catch (Exception)
            {
                IsRunning = false;
                throw;
            }
        }

        /// <summary>
        /// Stop the active reader thread.
        /// </summary>
        public override void Stop()
        {
            if (IsRunning == false)
                return;

            Abort();

            IsRunning = false;
        }
        #endregion

        #region Monitor RAM
        /// <summary>
        /// This function is run as a thread to read ram and raise events when new
        /// data shows up.
        /// </summary>
        internal void Monitor()
        {
            try
            {
                abortMonitorThread = false;

                if (FindFFXIProcess(false) == false)
                    return;

                ChatLogDetails oldDetails = null;
                ChatLogDetails currentDetails;
                uint highestLineProcessed = 0;

                // Loop until notified to stop or the FFXI process exits.
                while (abortMonitorThread == false)
                {
                    // If polProcess is ever lost (player disconnects), block on trying to reacquire it.
                    if (pol == null)
                    {
                        if (FindFFXIProcess(false) == false)
                        {
                            // End here if the FindFFXIProcess returns false,
                            // as that means the monitor thread has been aborted.
                            return;
                        }
                    }

                    try
                    {
                        //Fetch details such as how many lines are in the chat log, pointers to
                        //the memory containing the actual text, etc.
                        currentDetails = ReadChatLogDetails(chatLogLocation);

                        // If read failed, it will return null.
                        if (currentDetails == null)
                            continue;

                        // If every single field is the same as it was the last time we checked,
                        // assume that the chat log hasn't changed and continue on.
                        if (currentDetails.Equals(oldDetails))
                            continue;

                        //If there are zero lines in the NEW chat log, they are not logged into
                        //the game (e.g. at the character selection screen).  The log doesn't fill
                        //up and get copied over to the old log until there is one too many lines.  
                        //And then this log will still contain 1 line, the new one.  Obviously let's
                        //not do anything if they aren't logged in ;-)
                        if ((currentDetails.Info.NumberOfLines <= 0) || (currentDetails.Info.NumberOfLines > 50))
                        {
                            highestLineProcessed = 0;
                            oldDetails = null;
                            continue;
                        }

                        // Determine the first line number of the current chat log so we know
                        // where we are with respect to where the last line we processed was.
                        int firstOffset = 0;
                        int secondOffset = GetLineEndingOffset(currentDetails, 0);
                        int lineBytes = secondOffset - firstOffset;

                        // Read the chat log strings
                        string[] newChatLines = ReadChatLines(currentDetails.Info.NewChatLogPtr,
                            currentDetails.Info.FinalOffset, currentDetails.Info.NumberOfLines);

                        // Extract the unique line number from the first line so we know where
                        // we are in the chat sequence.
                        uint firstLineNumber = GetChatLineLineNumber(newChatLines[0]);

                        if (oldDetails == null)
                        {
                            //This is our first pass through the loop (i.e. parser just started)
                            //Set our counter to the last line currently in memory, so that next
                            //time through the loop, we start with the first line that wasn't there
                            //when the parser started.

                            oldDetails = currentDetails;
                            highestLineProcessed = firstLineNumber + (uint)currentDetails.Info.NumberOfLines - 1;

                            continue;
                        }

                        string[] missedChatLines;
                        string[] linesToProcess = null;

                        //It's not our first pass through the loop.  Check if lines were missed
                        if (firstLineNumber > (highestLineProcessed + 1))
                        {
                            // If we're here, that means lines were missed, and we need to get 
                            // them from the OLD log.  We know that numberOfLinesMissed must be
                            // 1 or greater.
                            byte numberOfLinesMissed = (byte)(firstLineNumber - highestLineProcessed - 1);

                            // However we can't deal with more than 50 missed lines. That's as
                            // many as the old log holds.
                            if (numberOfLinesMissed > 50)
                                numberOfLinesMissed = 50;

                            // Find the first index we want to read by counting back from 50 (the
                            // maximum array size) by the number of lines missed.
                            int indexOfFirstMissedLine = 50 - numberOfLinesMissed;

                            IntPtr startOfFirstMissedLine = Pointers.IncrementPointer(currentDetails.Info.OldChatLogPtr,
                                (uint)currentDetails.Info.oldLogOffsets[indexOfFirstMissedLine]);

                            uint numberOfBytesUntilStartOfLastMissedLine = (uint)(currentDetails.Info.oldLogOffsets[49]
                                - currentDetails.Info.oldLogOffsets[indexOfFirstMissedLine]);

                            //There is a field "FinalOffset" in the main Chat log meta struct that tells
                            //us where the last byte of actual chat log text is for the NEW log.  Unfortunately
                            //there is no analogous field for the OLD log.  Because of this, the best
                            //we can do is overestimate.  This has the nasty side effect of leading to 
                            //rare ReadProcessMemory() errors where we attempt to read past the end
                            //of a memory region, and into a block with different protection settings.
                            //Fortunately, even though ReadProcessMemory() returns an error in this case
                            //we can sleep well knowing we got everything we needed.
                            short totalBytesToRead = (short)(numberOfBytesUntilStartOfLastMissedLine + 240);

                            // Read lines from the old chat log.
                            missedChatLines = ReadChatLines(startOfFirstMissedLine, totalBytesToRead, numberOfLinesMissed);

                            // We know we missed lines or we wouldn't be here.  On the other hand, when 
                            // we tried to read them we got back nothing.  Log a warning and continue.
                            if (missedChatLines.Length == 0)
                                Trace.WriteLine(String.Format(Thread.CurrentThread.Name + ": Chat monitor missed line(s) [{0}, {1}]",
                                    indexOfFirstMissedLine, indexOfFirstMissedLine + numberOfLinesMissed));

                            // Make a single array containing all missed lines plus all new lines.
                            linesToProcess = new string[missedChatLines.Length + newChatLines.Length];

                            Array.ConstrainedCopy(missedChatLines, 0, linesToProcess, 0, missedChatLines.Length);
                            Array.ConstrainedCopy(newChatLines, 0, linesToProcess, missedChatLines.Length, newChatLines.Length);
                        }
                        else
                        {
                            // We have probably already processed some of the lines in the current array.
                            // So the only ones we want to copy to the array of items to process are ones
                            // with a line number higher that the highest line number we've seen.
                            uint indexOfFirstLineToProcess = (highestLineProcessed + 1) - firstLineNumber;
                            int numberOfNewLines = (int)(currentDetails.Info.NumberOfLines - indexOfFirstLineToProcess);

                            linesToProcess = new string[numberOfNewLines];
                            Array.ConstrainedCopy(newChatLines, (int)indexOfFirstLineToProcess, linesToProcess, 0, numberOfNewLines);
                        }



                        if ((linesToProcess != null) && (linesToProcess.Length > 0))
                        {
                            highestLineProcessed = GetChatLineLineNumber(linesToProcess[linesToProcess.Length - 1]);

                            // Add chat data to eventArgs array for watchers to process
                            List<ChatLine> chatData = new List<ChatLine>(linesToProcess.Length);

                            foreach (string newLine in linesToProcess)
                            {
                                chatData.Add(new ChatLine(newLine));
                            }

                            // Notify watchers
                            this.OnReaderDataChanged(new ReaderDataEventArgs(chatData));
                        }

                        // Set for the next time through the loop
                        oldDetails = currentDetails;
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.Log(e);
                    }
                    finally
                    {
                        // Make sure the thread doesn't hammer the system with constant
                        // monitoring, but often enough to get good event time resolution.
                        // Ram contents seem to be updated once per second, to set the
                        // monitor delay to half that for best update time.
                        Thread.Sleep(500);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e);
            }
        }

        /// <summary>
        /// Call this function rather than aborting the thread directly.
        /// </summary>
        internal void Abort()
        {
            abortMonitorThread = true;
        }
        #endregion

        #region Utility functions for reading memory data
        /// <summary>
        /// Fetch details such as how many lines are in the chat log, pointers to
        /// the memory containing the actual text, etc.
        /// </summary>
        /// <param name="chatLogLocation"></param>
        /// <returns>Returns a completed ChatLogDetails object if successful.
        /// If unsuccessful, returns null.</returns>
        private ChatLogDetails ReadChatLogDetails(ChatLogLocationInfo chatLogLocation)
        {
            IntPtr lineOffsetsBuffer = IntPtr.Zero;

            try
            {
                // Layout of total structure we're reading:
                // 50 offsets to new log records (short): 100 bytes
                // 50 offsets to old log offsets (short): 100 bytes
                // ChatLogInfoStruct block
                uint bytesToRead = (uint)(Marshal.SizeOf(typeof(ChatLogInfoStruct)));

                // Get the pointer to the overall structure.
                lineOffsetsBuffer = PInvoke.ReadProcessMemory(pol.Process.Handle, chatLogLocation.ChatLogOffset, bytesToRead);

                if (lineOffsetsBuffer != IntPtr.Zero)
                {
                    ChatLogDetails details = new ChatLogDetails();

                    // Copy the structure from memory buffer to managed class.
                    details.Info = (ChatLogInfoStruct)Marshal.PtrToStructure(lineOffsetsBuffer, typeof(ChatLogInfoStruct));

                    return details;
                }

                return null;
            }
            finally
            {
                PInvoke.DoneReadingProcessMemory(lineOffsetsBuffer);
            }
        }

        /// <summary>
        /// Read memory starting at the provided buffer point and break
        /// it out into an array of strings.
        /// </summary>
        /// <param name="bufferStart">Where to start reading.</param>
        /// <param name="bufferSize">The size of the buffer being read.</param>
        /// <param name="maxLinesToRead">Maximum number of lines to read.</param>
        /// <returns>Returns an array of strings from the buffer.</returns>
        private string[] ReadChatLines(IntPtr bufferStart, short bufferSize, byte maxLinesToRead)
        {
            if (maxLinesToRead == 0)
                return new string[0];

            // Don't try to read more than 50 lines
            if (maxLinesToRead > 50)
                maxLinesToRead = 50;


            IntPtr linesBuffer = IntPtr.Zero;

            try
            {
                // Read the raw databuffer from the process space
                linesBuffer = PInvoke.ReadProcessMemory(pol.Process.Handle, bufferStart, (uint)bufferSize);
                if (linesBuffer == IntPtr.Zero)
                    return new string[0];

                // Marshall the entire databuffer into a string with embedded nulls.
                string nullDelimitedChatLines = Marshal.PtrToStringAnsi(linesBuffer, (int)bufferSize);

                // Split the marshalled string on the null delimiter, but allow for an extra
                // element in case of trailing data.
                string[] splitStringArray = nullDelimitedChatLines.Split(
                    new char[] { '\0' }, maxLinesToRead + 1, StringSplitOptions.RemoveEmptyEntries);

                string[] chatLineArray = new string[maxLinesToRead];

                // Copy the split array into the array we're returning while removing any
                // potential extraneous data from the last array slot.
                Array.ConstrainedCopy(splitStringArray, 0, chatLineArray, 0, maxLinesToRead);

                return chatLineArray;
            }
            finally
            {
                PInvoke.DoneReadingProcessMemory(linesBuffer);
            }
        }

        /// <summary>
        /// Find the offset for the ending line of the specified log.
        /// </summary>
        /// <param name="currentDetails"></param>
        /// <param name="lineIndex"></param>
        /// <returns></returns>
        private int GetLineEndingOffset(ChatLogDetails currentDetails, int lineIndex)
        {
            //If this is the very last line index (i.e. the chat log JUST filled up)
            //make sure we don't step over the bounds.  Instead just set the next
            //line offset to one byte after the end of the chat log.
            if (lineIndex == currentDetails.Info.NumberOfLines - 1)
                return (int)currentDetails.Info.FinalOffset;
            else
                return (int)currentDetails.Info.newLogOffsets[lineIndex + 1];
        }

        /// <summary>
        /// Extract the line number from the provided chat line for use
        /// in tracking line position while monitoring RAM.
        /// </summary>
        /// <param name="chatLine">The chat line string.</param>
        /// <returns>The parsed out line number.</returns>
        private uint GetChatLineLineNumber(string chatLine)
        {
            return uint.Parse(chatLine.Substring(27, 8), System.Globalization.NumberStyles.AllowHexSpecifier);
        }
        #endregion

        #region Process Location Methods
        /// <summary>
        /// This function searches the processes on the computer system
        /// to locate FFXI.  It stores the process information when found.
        /// </summary>
        /// <returns>Returns true if process was found,
        /// false if monitoring was aborted before it was found.</returns>
        private bool FindFFXIProcess(bool scanning)
        {
            // Keep going as long as we're still attempting to monitor
            while (abortMonitorThread == false)
            {
                try
                {
                    Trace.WriteLine(Thread.CurrentThread.Name + ": Attempting to connect to Final Fantasy.");

                    if (polPID != 0)
                    {
                        Process processByID = Process.GetProcessById(polPID);

                        if (string.Compare(processByID.ProcessName, "pol", true) == 0)
                        {
                            foreach (ProcessModule module in processByID.Modules)
                            {
                                if (string.Compare(module.ModuleName, "ffximain.dll", true) == 0)
                                {
                                    Trace.WriteLine(string.Format("Module: {0}  Base Address: {1:X8}", module.ModuleName, module.BaseAddress));
                                    pol = new POL(processByID, module.BaseAddress);
                                    processByID.Exited += new EventHandler(PolExited);
                                    // Turn this off if scanning ram:
                                    if (scanning == false)
                                        LocateChatLog();
                                    return true;
                                }
                            }
                        }

                        System.Windows.Forms.MessageBox.Show(
                            string.Format("Specified process ID ({0}) is not a POL process.",
                              polPID),
                            "Process not found", System.Windows.Forms.MessageBoxButtons.OK);
                    }
                    else
                    {
                        Process[] polProcesses = Process.GetProcessesByName("pol");

                        if (polProcesses != null)
                        {
                            foreach (Process process in polProcesses)
                            {
                                foreach (ProcessModule module in process.Modules)
                                {
                                    if (string.Compare(module.ModuleName, "ffximain.dll", true) == 0)
                                    {
                                        Trace.WriteLine(string.Format("Module: {0}  Base Address: {1:X8}", module.ModuleName, module.BaseAddress));
                                        pol = new POL(process, module.BaseAddress);
                                        process.Exited += new EventHandler(PolExited);
                                        // Turn this off if scanning ram:
                                        if (scanning == false)
                                            LocateChatLog();
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (ArgumentException e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message, "Process not found", System.Windows.Forms.MessageBoxButtons.OK);
                }
                catch (Exception e)
                {
                    Logger.Instance.Log("Memory access", String.Format(Thread.CurrentThread.Name + ": ERROR: An exception occured while trying to connect to Final Fantasy.  Message = {0}", e.Message));
                    System.Threading.Thread.Sleep(5000);
                }
                finally
                {
                    // Wait before trying again.
                    System.Threading.Thread.Sleep(5000);
                }
            }

            return false;
        }

        /// <summary>
        /// This function digs into the FFXI address space to locate the
        /// address of where the chat log data is stored.
        /// </summary>
        private void LocateChatLog()
        {
            // Loop until we find the address, or the thread is requested to stop
            while (abortMonitorThread == false)
            {
                //FFXIMainStaticOffset is the offset to the address that contains the
                //first pointer in the hierarchy of FFXI's chat log data structures.  Obviously,
                //it is an offset from the base address that FFXIMain.dll is loaded at.
                // :: This is a pointer to a data structure we want to read
                IntPtr rootAddress = Pointers.IncrementPointer(pol.BaseAddress, initialMemoryOffset);

                //Dereference that pointer to get our next address.
                IntPtr dataStructurePointer = Pointers.FollowPointer(pol.Process.Handle, rootAddress);

                if (dataStructurePointer == IntPtr.Zero)
                {
                    Trace.WriteLine("Error dereferencing first pointer.");
                    System.Threading.Thread.Sleep(700);
                    continue;
                }

                //This is just the way it is (discovered through trial and error).  4 bytes from
                //where the first pointer takes us is where the second pointer of interest lives.
                // :: The second dword of the data structure is a pointer to our location of interest.
                IntPtr fieldPointer = Pointers.IncrementPointer(dataStructurePointer, 4);


                //Follow the second pointer inside the address space of the FFXI process.
                IntPtr destination = Pointers.FollowPointer(pol.Process.Handle, fieldPointer);
                if (destination == IntPtr.Zero)
                {
                    Trace.WriteLine("Error dereferencing second pointer.");
                    System.Threading.Thread.Sleep(700);
                    continue;
                }

                //Finally, we've arrived at the address of the "line offsets arrays".  
                //Save this, as we'll read the Line Offsets arrays later, and also use it
                //to get to other chat log related information.
                chatLogLocation = new ChatLogLocationInfo(destination);

                return;
            }
        }

        /// <summary>
        /// This is an event handler for if/when FFXI exits while we're still running
        /// so that we can clean up properly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void PolExited(object sender, EventArgs e)
        {
            // Halt monitoring
            Stop();
            pol = null;
        }
        #endregion

        #region Utility functions for examining RAM to determine new base address
        [Conditional("DEBUG")]
        internal void ScanRAM()
        {
            try
            {
                IsRunning = true;
                initialMemoryOffset = appSettings.MemoryOffset;

                // Note: Automatically disables LocateChatLog in FindFFXIProcess().

                if (FindFFXIProcess(true) == false)
                    return;

                // Locate a known string in memory space.  From there, determine the start
                // of the array of chat log messages.
                //FindString();
                // Take scanAddress and increment it by the index in scanStruct.memScanCharacters
                // Result: 0x043e86a0 + 0x1c8 = 0x043E8868
                // Result: 0x029CF920 + 0x01dd0000 (base) = 0x479F920

                // Locate a pointer to the start of the chat log messages. (0x03ec0fac)
                // From that, determine the start of the ChatLogInfoStruct.
                //FindAddress(0x479F920);
                // Take scanAddress + j*4 at breakpoint
                // Result: 0x043e8000 + 0x213*4 = 0x043e884c
                // Result: 0x0479f000 + 0x241*4 = 0x0479F904

                // The start of ChatLogInfoStruct is (4 bytes + 50 shorts + 50 shorts =
                // 204 bytes (0xCC) before the located pointer.
                // Result: 0x043e884c - 0xCC = 0x043E8780
                // Result: 0x0479F904 - 0xCC = 0x0479F838


                // Examine the ChatLogInfoStruct from the previous address
                // to make sure things match up.
                //CheckStructure(0x0479F838);

                // Since we know where the structure lives, find the address
                // that points to that.
                //FindAddress(0x0479F838);
                // Take scanAddress + j*4 at breakpoint
                // Result: 0x043e8000 + 0x1d7*4 = 0x043E875C
                // Result: 0x0479F000 + 0x205*4 = 0x0479F814

                // That pointer is the second in a structure that is pointed
                // to by an initial address point.  Locate the address of our
                // pointer - 4.
                //FindAddress(0x0479F810);
                // Take scanAddress + j*4 at breakpoint
                // Result: 0x02065000 + 0x25a*4 = 0x02065968
                // Result: 0x0234a000 + 0x0b2*4 = 0x0234A2C8

                // Base offset address is the above pointer relative to the
                // POL base address.  Remove that value.
                // Result: 0x02065968 - 0x01af0000 == 0x00575968
                // Result: 0x02346D58 - 0x01dd0000 == 0x00576D58
                // Result: 0x0234A2C8 - 0x01dd0000 == 0x0057A2C8

                // Base address before patch for 2008-03-10: 0x0056A788
                // Base address after patch for 2008-03-10:  0x0056DA48
                // Base address after update on 2008-06-09:  0x00575968
                // Base address after update on 2008-09-08:  0x00576D58
                // Base address after update on 2008-12-08:  0x0057A2C8
            }
            finally
            {
                IsRunning = false;
            }
        }

        [Conditional("DEBUG")]
        private void CheckStructure(uint checkAddress)
        {
            try
            {
                //Dereference that pointer to get our next address.
                IntPtr dataStructurePointer = new IntPtr(checkAddress);

                ChatLogLocationInfo scanChatLogLocation = new ChatLogLocationInfo(dataStructurePointer);

                ChatLogDetails examineDetails = ReadChatLogDetails(scanChatLogLocation);

                string[] scanChatLines = ReadChatLines(examineDetails.Info.NewChatLogPtr,
                        examineDetails.Info.FinalOffset, examineDetails.Info.NumberOfLines);
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e);
            }
            finally
            {
            }
        }

        [Conditional("DEBUG")]
        private void FindAddress(uint findTotalAddress)
        {
            uint scanMemoryOffset = 0;
            MemScanAddressStruct scanStruct = new MemScanAddressStruct();

            uint bytesToRead = (uint)(Marshal.SizeOf(typeof(MemScanAddressStruct)));

            //uint findTotalAddress = 0x03EC0FC8;
            //uint findTotalAddress = 0x03EC0EE0;
            //uint findTotalAddress = 0x03EC0EB8;

            for (int i = 0; i < 64000; i++)
            {
                IntPtr scanAddress = Pointers.IncrementPointer(pol.BaseAddress, scanMemoryOffset);
                IntPtr scanResults = IntPtr.Zero;

                try
                {
                    scanResults = PInvoke.ReadProcessMemory(pol.Process.Handle, scanAddress, bytesToRead);

                    scanStruct = (MemScanAddressStruct)Marshal.PtrToStructure(scanResults, typeof(MemScanAddressStruct));

                    int j = Array.IndexOf(scanStruct.addressValues, findTotalAddress);
                    if (j >= 0)
                        Trace.WriteLine(string.Format("Total Index j = {0}\n", j));
                }
                catch (Exception e)
                {
                    Logger.Instance.Log(e);
                }
                finally
                {
                    PInvoke.DoneReadingProcessMemory(scanResults);
                }

                scanMemoryOffset += bytesToRead;
            }
        }

        [Conditional("DEBUG")]
        private void FindString()
        {
            uint scanMemoryOffset = 0x029CF920;
            //uint scanMemoryOffset = 0x029CF000;
            uint blockSize = 1024;
            uint blockOffset = blockSize - 32;
            MemScanStringStruct scanStruct = new MemScanStringStruct();

            string byteString;
            string prevByteString;

            for (int i = 0; i < 64000; i++)
            {
                IntPtr scanAddress = Pointers.IncrementPointer(pol.BaseAddress, scanMemoryOffset);
                IntPtr scanResults = IntPtr.Zero;

                try
                {
                    scanResults = PInvoke.ReadProcessMemory(pol.Process.Handle, scanAddress, blockSize);

                    scanStruct = (MemScanStringStruct)Marshal.PtrToStructure(scanResults, typeof(MemScanStringStruct));

                    byteString = new string(scanStruct.memScanCharacters);

                    int j = byteString.IndexOf("Unicorn Claymore");

                    if (j >= 0)
                        Trace.WriteLine(string.Format("Offset = {0}, Index j = {1}\n", scanMemoryOffset, j));

                    prevByteString = byteString;
                }
                catch (Exception e)
                {
                    Logger.Instance.Log(e);
                }
                finally
                {
                    PInvoke.DoneReadingProcessMemory(scanResults);
                }

                scanMemoryOffset += blockOffset;
            }
        }
        #endregion
    }
}
