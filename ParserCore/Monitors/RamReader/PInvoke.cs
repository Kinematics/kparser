using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace WaywardGamers.KParser.Monitoring.Memory
{
    #region Win32 Enumerations

    [Flags]
    internal enum MemoryRegionProtection : uint
    {
        NoAccess         =  0x01,
        ReadOnly         =  0x02,
        ReadWrite        =  0x04,
        WriteCopy        =  0x08,
        Execute          =  0x10,
        ExecuteRead      =  0x20,
        ExecuteReadWrite =  0x40,
        ExecuteWriteCopy =  0x80,
        Guard            = 0x100,
        NoCache          = 0x200,
        WriteCombine     = 0x400
    }

    internal enum MemoryRegionState : uint
    {
        Commit  = 0x1000,
        Reserve = 0x2000,
        Free    = 0x10000,
    }

    [Flags]
    internal enum MemoryRegionType : uint
    {
        Private = 0x20000,
        Mapped  = 0x40000,
        Image   = 0x1000000,
    }

    [Flags]
    internal enum FileMappingProtection : uint
    {
        Page_Readonly         = 0x02,
        Page_ReadWrite        = 0x04,
        Page_WriteCopy        = 0x08,
        Page_ExecuteRead      = 0x20,
        Page_ExecuteReadWrite = 0x40,
        Section_Image         = 0x1000000,
        Section_Commit        = 0x8000000,
        Section_Reserve       = 0x4000000,
        Section_NoCache       = 0x10000000
    }

    internal enum FileMappingAccess : uint
    {
        Copy    = 0x0001,
        Write   = 0x0002,
        Read    = 0x0004,
        Execute = 0x0020
    }

    #endregion

    #region Win32 Structures

    [StructLayout(LayoutKind.Sequential)]
    internal struct MemoryInformation
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public MemoryRegionProtection AllocationProtect;
        public uint RegionSize;
        public MemoryRegionState State;
        public MemoryRegionProtection Protect;
        public MemoryRegionType Type;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal class SecurityAttributes
    {
        uint StructLength;
        IntPtr SecurityDescriptor;
        bool InheritHandle;
    }

    #endregion

    internal class PInvoke
    {
        // Wrap the first three calls, below.
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CreateFileMapping(IntPtr fileHandle, SecurityAttributes attributes,
            uint protection, uint maxSizeHigh, uint maxSizeLow, string name);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr MapViewOfFile(IntPtr fileMappingHandle, uint desiredAccess, uint fileOffsetHigh,
            uint fileOffsetLow, uint numBytesToMap);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr processHandle, IntPtr address, IntPtr outputBuffer,
            UIntPtr nBufferSize, out UIntPtr lpNumberOfBytesRead);

        // These three can be called directly
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, EntryPoint = "MoveMemory", SetLastError = true)]
        internal static extern bool MoveMemory(IntPtr destination, IntPtr source, int length);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, EntryPoint = "UnmapViewOfFile", SetLastError = true)]
        internal static extern bool UnmapViewOfFile(IntPtr viewHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, EntryPoint = "CloseHandle", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr handle);



        internal static IntPtr CreateFileMapping(IntPtr fileHandle, FileMappingProtection protection, long maxMappingSize, string mappingName)
        {
            uint mappingLow = (uint)(maxMappingSize & 0xFFFFFFFF);
            uint mappingHigh = (uint)(maxMappingSize >> 32);

            IntPtr result = CreateFileMapping(fileHandle, null, (uint)protection, mappingHigh, mappingLow, mappingName);
            if (result == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

            return result;
        }

        internal static IntPtr MapViewOfFile(IntPtr fileMappingHandle, FileMappingAccess access, long fileOffset, uint numBytesToMap)
        {
            uint fileOffsetLow = (uint)(fileOffset & 0xFFFFFFFF);
            uint fileOffsetHigh = (uint)(fileOffset >> 32);

            IntPtr result = MapViewOfFile(fileMappingHandle, (uint)access, fileOffsetHigh, fileOffsetLow, numBytesToMap);
            if (result == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());

            return result;
        }

        internal static IntPtr ReadProcessMemory(IntPtr processHandle, IntPtr address, uint nBytesToRead)
        {
            IntPtr buffer = Marshal.AllocHGlobal((int)nBytesToRead);

            UIntPtr bytesRead = UIntPtr.Zero;
            UIntPtr bytesToRead = (UIntPtr)nBytesToRead;

            if (!ReadProcessMemory(processHandle, address, buffer, bytesToRead, out bytesRead))
            {
                int Error = Marshal.GetLastWin32Error();
                if (Error == 299)	//ERROR_PARTIAL_COPY
                    return buffer;
                Marshal.FreeHGlobal(buffer);
                return IntPtr.Zero;
            }

            return buffer;
        }

        internal static void DoneReadingProcessMemory(IntPtr memoryPointer)
        {
            if (memoryPointer != IntPtr.Zero)
            {
                try
                {
                    Marshal.FreeHGlobal(memoryPointer);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }
    }


}
