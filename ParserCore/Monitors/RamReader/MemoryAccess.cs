using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace WaywardGamers.KParser.Monitoring.Memory
{
    #region Event classes
    internal delegate void RamWatchEventHandler(object sender, RamWatchEventArgs ramArgs);

    internal class RamWatchEventArgs : EventArgs
    {
        private List<ChatLine> chatLineCollection;

        internal RamWatchEventArgs(int numLines)
        {
            chatLineCollection = new List<ChatLine>(numLines);
        }

        internal List<ChatLine> ChatLineCollection
        {
            get
            {
                return chatLineCollection;
            }
        }

        internal void AddChatLine(ChatLine chatLine)
        {
            chatLineCollection.Add(chatLine);
        }
    }
    #endregion

    /// <summary>
    /// This class is created and has its monitor function run as a thread.
    /// It checks for changes in the POL chat log, and when detected reads
    /// that log data as chat data lines.  That collection of lines is stored
    /// in the event data of RamWatchEventArgs, and then any watchers of
    /// this class (ie: RamReader) are notified by firing the event delegates.
    /// The watching class then has the duty of dissecting the chat lines and
    /// sending them to be parsed.
    /// </summary>
    internal class MemoryAccess
    {
        #region Private internal classes

        private class POL
        {
            internal Process Process { get; private set; }
            internal IntPtr BaseAddress { get; private set; }

            internal POL(Process process, IntPtr address)
            {
                Process = process;
                BaseAddress = address;
            }
        }

        private class ChatLogLocationInfo
        {
            internal IntPtr ChatLogOffset { get; private set; }

            internal ChatLogLocationInfo(IntPtr offset)
            {
                ChatLogOffset = offset;
            }
        }

        private class ChatLogDetails
        {
            internal ChatLogInfoStruct Info;

            public override bool Equals(object obj)
            {
                ChatLogDetails rhs = obj as ChatLogDetails;
                if (rhs == null)
                    return false;
                if (Info.NumberOfLines != rhs.Info.NumberOfLines)
                    return false;
                if (Info.NewChatLogPtr != rhs.Info.NewChatLogPtr)
                    return false;
                if (Info.OldChatLogPtr != rhs.Info.OldChatLogPtr)
                    return false;
                if (Info.FinalOffset != rhs.Info.FinalOffset)
                    return false;
                if (Info.ChatLogBytes != rhs.Info.ChatLogBytes)
                    return false;
                return true;
            }

            public override int GetHashCode()
            {
                return Info.NewChatLogPtr.GetHashCode() ^ Info.OldChatLogPtr.GetHashCode();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ChatLogInfoStruct
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            internal short[] newLogOffsets;
            
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            internal short[] oldLogOffsets;

            internal byte NumberOfLines;

            internal byte dummy1;
            internal byte dummy2;
            internal byte dummy3;

            internal IntPtr NewChatLogPtr;
            internal IntPtr OldChatLogPtr;
            internal int ChatLogBytes;
            internal short FinalOffset;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MemScanStringStruct
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
            internal char[] memScanCharacters;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MemScanAddressStruct
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
            internal uint[] addressValues;
        }

        #endregion

        #region Member Variables
        POL pol;
        ChatLogLocationInfo chatLogLocation;

        bool abortMonitorThread;

        private event RamWatchEventHandler ramDataWatchers;
        #endregion

        #region Monitoring Methods
        /// <summary>
        /// The protected OnRamData method raises the event by invoking 
        /// the delegates. The sender is always 'this', the current instance 
        /// of the class.
        /// </summary>
        /// <param name="e">The event args object that will be sent to
        /// all the functions on the delegate list.</param>
        protected virtual void OnRamDataChanged(RamWatchEventArgs e)
        {
            if (ramDataWatchers != null)
            {
                // Invokes the delegates. 
                ramDataWatchers(this, e);
            }
        }

        /// <summary>
        /// This function is run as a thread to read ram and raise events when new
        /// data shows up.
        /// </summary>
        internal void Monitor()
        {
            try
            {
                abortMonitorThread = false;

                if (FindFFXIProcess() == false)
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
                        if (FindFFXIProcess() == false)
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

                            IntPtr startOfFirstMissedLine = IncrementPointer(currentDetails.Info.OldChatLogPtr,
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
                            RamWatchEventArgs chatData = new RamWatchEventArgs(linesToProcess.Length);

                            foreach (string newLine in linesToProcess)
                            {
                                chatData.ChatLineCollection.Add(new ChatLine(newLine));
                            }

                            // Notify watchers
                            OnRamDataChanged(chatData);
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
                        // monitoring.
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e);
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
                string[] splitStringArray = nullDelimitedChatLines.Split(new char[] { '\0' }, maxLinesToRead+1);

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
        /// Fetch details such as how many lines are in the chat log, pointers to
        /// the memory containing the actual text, etc.
        /// </summary>
        /// <param name="chatLogLocation"></param>
        /// <returns></returns>
        private ChatLogDetails ReadChatLogDetails(ChatLogLocationInfo chatLogLocation)
        {
            IntPtr lineOffsetsBuffer = IntPtr.Zero;

            try
            {
                ChatLogDetails details = new ChatLogDetails();

                // Layout of total structure we're reading:
                // 50 offsets to new log records (short): 100 bytes
                // 50 offsets to old log offsets (short): 100 bytes
                // ChatLogInfoStruct block
                uint bytesToRead = (uint)(Marshal.SizeOf(typeof(ChatLogInfoStruct)));

                // Get the pointer to the overall structure.
                lineOffsetsBuffer = PInvoke.ReadProcessMemory(pol.Process.Handle, chatLogLocation.ChatLogOffset, bytesToRead);

                // Copy the structure from memory buffer to managed class.
                details.Info = (ChatLogInfoStruct)Marshal.PtrToStructure(lineOffsetsBuffer, typeof(ChatLogInfoStruct));

                return details;
            }
            finally
            {
                PInvoke.DoneReadingProcessMemory(lineOffsetsBuffer);
            }
        }

        /// <summary>
        /// Call this function rather than aborting the thread directly.
        /// </summary>
        internal void Abort()
        {
            abortMonitorThread = true;
        }

        /// <summary>
        /// This is an event handler for if/when FFXI exits while we're still running
        /// so that we can clean up properly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void polExited(object sender, EventArgs e)
        {
            // Halt monitoring
            pol = null;
        }
        #endregion

        #region Properties
        internal uint InitialMemoryOffset { get; set; }

        internal RamWatchEventHandler RamDataChanged
        {
            get
            {
                return ramDataWatchers;
            }
            set
            {
                ramDataWatchers = value;
            }
        }
        #endregion

        #region Process Location Methods
        /// <summary>
        /// This function searches the processes on the computer system
        /// to locate FFXI.  It stores the process information when found.
        /// </summary>
        /// <returns>Returns true if process was found,
        /// false if monitoring was aborted before it was found.</returns>
        private bool FindFFXIProcess()
        {
            // Keep going as long as we're still attempting to monitor
            while (abortMonitorThread == false)
            {
                try
                {
                    Trace.WriteLine(Thread.CurrentThread.Name + ": Attempting to connect to Final Fantasy.");

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
                                    process.Exited += new EventHandler(polExited);
                                    // Turn this off if scanning ram:
                                    LocateChatLog();
                                    return true;
                                }
                            }
                        }
                    }

                    System.Threading.Thread.Sleep(5000);
                }
                catch (Exception e)
                {
                    Logger.Instance.Log("Memory access", String.Format(Thread.CurrentThread.Name + ": ERROR: An exception occured while trying to connect to Final Fantasy.  Message = {0}", e.Message));
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
                IntPtr rootAddress = IncrementPointer(pol.BaseAddress, InitialMemoryOffset);

                //Dereference that pointer to get our next address.
                IntPtr dataStructurePointer = FollowPointer(rootAddress);

                if (dataStructurePointer == IntPtr.Zero)
                {
                    Trace.WriteLine("Error dereferencing first pointer.");
                    System.Threading.Thread.Sleep(700);
                    continue;
                }

                //This is just the way it is (discovered through trial and error).  4 bytes from
                //where the first pointer takes us is where the second pointer of interest lives.
                // :: The second dword of the data structure is a pointer to our location of interest.
                IntPtr fieldPointer = IncrementPointer(dataStructurePointer, 4);


                //Follow the second pointer inside the address space of the FFXI process.
                IntPtr destination = FollowPointer(fieldPointer);
                if (destination == IntPtr.Zero)
                {
                    Trace.WriteLine("Error dereferencing second pointer.");
                    System.Threading.Thread.Sleep(700);
                    continue;
                }

                //Finally, we've arrived at the address of the "line offsets arrays".  
                //Save this, as we'll read the Line Offsets arrays later, and also use it
                //to get to other interesting chat log related information.
                chatLogLocation = new ChatLogLocationInfo(destination);

                return;
            }
        }
        #endregion

        #region Pointer Functions
        /// <summary>
        /// This function allows us to do basic pointer arithmatic, incrementing
        /// a pointer by a set number of bytes.
        /// </summary>
        /// <param name="pointer">The initial memory address.</param>
        /// <param name="numBytes">The number of bytes to move relative to the initial address.</param>
        /// <returns>Returns the new pointer address.</returns>
        private IntPtr IncrementPointer(IntPtr pointer, uint numBytes)
        {
            return (IntPtr)((uint)pointer + numBytes);
        }

        /// <summary>
        /// This function dereferences a pointer and returns the address that
        /// the pointer pointed to.
        /// </summary>
        /// <param name="pointerToFollow">The original pointer.</param>
        /// <returns>The 'value' of the pointer; the location the pointer pointed to.
        /// Returns IntPtr.Zero (null pointer) if we are unable to read the memory address.</returns>
        private IntPtr FollowPointer(IntPtr pointerToFollow)
        {
            IntPtr pointerToRead = IntPtr.Zero;

            try
            {
                pointerToRead = PInvoke.ReadProcessMemory(pol.Process.Handle, pointerToFollow, (uint)IntPtr.Size);

                if (pointerToRead == IntPtr.Zero)
                    return IntPtr.Zero;

                return Marshal.ReadIntPtr(pointerToRead);
            }
            finally
            {
                PInvoke.DoneReadingProcessMemory(pointerToRead);
            }
        }
        #endregion

        #region Functions for examining RAM to determine new base address
        internal void ScanRAM()
        {
            // Note: Disable LocateChatLog in FindFFXIProcess() before running this function.

            if (FindFFXIProcess() == false)
                return;

            // Locate a known string in memory space.  From there, determine the start
            // of the array of chat log messages.
            //FindString();
            // Take scanAddress and increment it by the index in scanStruct.memScanCharacters
            // Result: 0x03EC0FC8
            // Result: 0x043e86a0 + 0x1c8 = 0x043E8868

            // Locate a pointer to the start of the chat log messages. (0x03ec0fac)
            // From that, determine the start of the ChatLogInfoStruct.
            //FindAddress(0x043E8868);
            // Take scanAddress + j*4 at breakpoint
            // Result: 0x03EC0EE0
            // Result: 0x043e8000 + 0x213*4 = 0x043e884c
            // The start of ChatLogInfoStruct is (4 bytes + 50 shorts + 50 shorts =
            // 204 bytes (0xCC) before the located pointer.
            // Result: 0x043e884c - 0xCC = 0x043E8780


            // Examine the ChatLogInfoStruct from the previous address
            // to make sure things match up.
            //CheckStructure(0x043E8780);

            // Since we know where the structure lives, find the address
            // that points to that.
            //FindAddress(0x043E8780);
            // Take scanAddress + j*4 at breakpoint
            // Result: 0x03EC0EBC
            // Result: 0x043e8000 + 0x1d7*4 = 0x043E875C

            // That pointer is the second in a structure that is pointed
            // to by an initial address point.  Locate the address of our
            // pointer - 4.
            //FindAddress(0x043E8758);
            // Take scanAddress + j*4 at breakpoint
            // Result: 0x0203DA48
            // Result: 0x02065000 + 0x25a*4 = 0x02065968

            // Base offset address is the above pointer relative to the
            // POL base address.  Remove that value.
            // Result: 0x0203DA48 - 0x01ad0000 == 0x0056DA48
            // Result: 0x02065968 - 0x01af0000 == 0x00575968

            // Base address before patch for 2008-03-10: 0x0056A788
            // Base address after patch for 2008-03-10:  0x0056DA48
            // Base address after update on 2008-06-09:  0x00575968
        }

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
                IntPtr scanAddress = IncrementPointer(pol.BaseAddress, scanMemoryOffset);
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

        private void FindString()
        {
            uint scanMemoryOffset = 0;
            uint blockSize = 1024;
            uint blockOffset = blockSize - 32;
            MemScanStringStruct scanStruct = new MemScanStringStruct();

            string byteString;

            for (int i = 0; i < 64000; i++)
            {
                IntPtr scanAddress = IncrementPointer(pol.BaseAddress, scanMemoryOffset);
                IntPtr scanResults = IntPtr.Zero;

                try
                {
                    scanResults = PInvoke.ReadProcessMemory(pol.Process.Handle, scanAddress, blockSize);

                    scanStruct = (MemScanStringStruct)Marshal.PtrToStructure(scanResults, typeof(MemScanStringStruct));

                    byteString = new string(scanStruct.memScanCharacters);

                    int j = byteString.IndexOf("Unicorn Claymore");

                    if (j >= 0)
                        Trace.WriteLine(string.Format("Index j = {0}\n", j));
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
