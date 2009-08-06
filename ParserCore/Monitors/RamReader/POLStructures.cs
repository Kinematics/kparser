using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace WaywardGamers.KParser.Monitoring.Memory
{
    #region Custom classes for marshalling data

    internal class POL
    {
        internal Process Process { get; private set; }
        internal IntPtr FFXIBaseAddress { get; private set; }

        internal POL(Process process, IntPtr address)
        {
            Process = process;
            FFXIBaseAddress = address;
        }
    }

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


    [StructLayout(LayoutKind.Sequential)]
    internal struct ChatLogInfoStruct
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        internal short[] newLogOffsets;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        internal short[] oldLogOffsets;

        internal int NumberOfLines;
        //internal byte NumberOfLines;

        //internal byte dummy1;
        //internal byte dummy2;
        //internal byte dummy3;

        internal IntPtr NewChatLogPtr;
        internal IntPtr OldChatLogPtr;
        internal int ChatLogBytes;
        internal short FinalOffset;
    }

    internal class ChatLogInfoClass
    {
        readonly internal ChatLogInfoStruct ChatLogInfo;

        public ChatLogInfoClass(ChatLogInfoStruct chatLogInfo)
        {
            ChatLogInfo = chatLogInfo;
        }

        public override bool Equals(object obj)
        {
            ChatLogInfoClass rhs = obj as ChatLogInfoClass;
            if (rhs == null)
                return false;
            if (ChatLogInfo.NumberOfLines != rhs.ChatLogInfo.NumberOfLines)
                return false;
            if (ChatLogInfo.NewChatLogPtr != rhs.ChatLogInfo.NewChatLogPtr)
                return false;
            if (ChatLogInfo.OldChatLogPtr != rhs.ChatLogInfo.OldChatLogPtr)
                return false;
            if (ChatLogInfo.FinalOffset != rhs.ChatLogInfo.FinalOffset)
                return false;
            if (ChatLogInfo.ChatLogBytes != rhs.ChatLogInfo.ChatLogBytes)
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            return ChatLogInfo.NewChatLogPtr.GetHashCode() ^ ChatLogInfo.OldChatLogPtr.GetHashCode();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MemScanStringStruct
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
        internal char[] memScanCharacters;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MemScanAddressStruct
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
        internal uint[] addressValues;
    }

    #endregion
}
