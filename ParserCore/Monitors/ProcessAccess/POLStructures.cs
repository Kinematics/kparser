using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace WaywardGamers.KParser.Monitoring.Memory
{
    #region Custom classes for marshalling data

    /// <summary>
    /// Class for maintaining reference pointers to the chat log text as stored in memory.
    /// </summary>
    internal class ChatLogLocationInfo
    {
        internal IntPtr ChatLogControlAddress { get; private set; }
        internal IntPtr ChatLogInfoAddress { get; private set; }

        internal ChatLogLocationInfo(IntPtr controlAddress, IntPtr infoAddress)
        {
            ChatLogControlAddress = controlAddress;
            ChatLogInfoAddress = infoAddress;
        }
    }

    /// <summary>
    /// The memory structure used for referencing the chat log meta-information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ChatLogControlStruct
    {
        internal uint Unknown1;

        internal IntPtr ChatLogInfoPtr; // Points to ChatLogInfoStruct

        internal int NumberOfLinesInChatHistory;

        internal IntPtr UnknownPtr1;

        internal uint Dummy1;
        internal uint Dummy2;
        internal uint Dummy3;

        internal uint Unknown2;

        internal short Unknown3;
        internal short Unknown4;
        internal short Unknown5;
        internal short Unknown6;

    }

    /// <summary>
    /// The memory structure for referencing the chat log information.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ChatLogInfoStruct
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        internal short[] currLogOffsets;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        internal short[] prevLogOffsets;

        // Number of lines currently stored in the new log offsets list
        internal byte NumberOfLines;
        // Number appears to be an int most of the time, but apparently
        // other data is stored in the following bytes.  Probably
        // 2 shorts, but safer this way.
        internal byte dummy1;
        internal byte dummy2;
        internal byte dummy3;

        // Pointer to the buffers that store the current/previous log buffers
        internal IntPtr PtrToCurrentChatLog;
        internal IntPtr PtrToPreviousChatLog;

        // The number of bytes allocated in the current log buffer.
        // This value seems to default to 3200 bytes, and doubles to 6400 bytes when needed.
        // Maximum length chat line (English) is 175 bytes, including the 53 bytes used by
        // the header values.  JP text can use 2 bytes per character, so presumably max
        // length for a JP line would be about 295 bytes.  That would put the maximum
        // buffer length at 14,750 bytes.
        // Observed buffer size progression each time the current buffer length is exceeded:
        // 3200, 6400, 9600 bytes.  Predicted additional progression for maximum JP text
        // would be 12,800 and 16,000 bytes.
        internal int ChatLogBytes;

        // The total number of bytes used by 'real' chat lines in the current chat buffer.
        internal short FinalOffset;
    }

    /// <summary>
    /// A wrapper class for the chat log information structure, used so that
    /// equality comparisons can be made.
    /// </summary>
    internal class ChatLogDetails
    {
        readonly internal ChatLogInfoStruct ChatLogInfo;

        public ChatLogDetails(ChatLogInfoStruct chatLogInfo)
        {
            ChatLogInfo = chatLogInfo;
        }

        public override bool Equals(object obj)
        {
            ChatLogDetails rhs = obj as ChatLogDetails;
            if (rhs == null)
                return false;
            if (ChatLogInfo.NumberOfLines != rhs.ChatLogInfo.NumberOfLines)
                return false;
            if (ChatLogInfo.PtrToCurrentChatLog != rhs.ChatLogInfo.PtrToCurrentChatLog)
                return false;
            if (ChatLogInfo.PtrToPreviousChatLog != rhs.ChatLogInfo.PtrToPreviousChatLog)
                return false;
            if (ChatLogInfo.FinalOffset != rhs.ChatLogInfo.FinalOffset)
                return false;
            if (ChatLogInfo.ChatLogBytes != rhs.ChatLogInfo.ChatLogBytes)
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            return ChatLogInfo.PtrToCurrentChatLog.GetHashCode() ^ ChatLogInfo.PtrToPreviousChatLog.GetHashCode();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MemScanStringStruct
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3200)]
        internal char[] memScanCharacters;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MemScanAddressStruct
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3200)]
        internal uint[] addressValues;
    }

    #endregion
}
