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
        internal IntPtr BaseAddress { get; private set; }

        internal POL(Process process, IntPtr address)
        {
            Process = process;
            BaseAddress = address;
        }
    }

    internal class ChatLogLocationInfo
    {
        internal IntPtr ChatLogOffset { get; private set; }

        internal ChatLogLocationInfo(IntPtr offset)
        {
            ChatLogOffset = offset;
        }
    }

    internal class ChatLogDetails
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
    internal struct ChatLogInfoStruct
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
