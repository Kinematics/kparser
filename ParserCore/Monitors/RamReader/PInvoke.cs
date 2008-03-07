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

    internal enum ProcessAccessRights : uint
    {
        Terminate                = 0x0001,
        CreateThread             = 0x0002,
        VMOperation              = 0x0008,
        VMRead                   = 0x0010,
        VMWrite                  = 0x0020,
        DuplicateHandle          = 0x0040,
        CreateProcess            = 0x0080,
        SetQuota                 = 0x0100,
        SetInformation           = 0x0200,
        QueryInformation         = 0x0400,
        SuspendResume            = 0x0800,
        QueryLimitedInformation  = 0x1000,
        Synchronize              = 0x00100000,
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

    /// <summary>
    /// Class to handling calling kernal functions.
    /// </summary>
    internal class PInvoke
    {
        /// <summary>
        /// Import kernal function to read process memory.
        /// </summary>
        /// <param name="processHandle">A handle to the process with memory that is being read.
        /// The handle must have PROCESS_VM_READ access to the process.</param>
        /// <param name="address">A pointer to the base address in the specified process from which to read.</param>
        /// <param name="outputBuffer">A pointer to a buffer that receives the contents from the address
        /// space of the specified process.</param>
        /// <param name="nBufferSize">The number of bytes to be read from the specified process.</param>
        /// <param name="lpNumberOfBytesRead">A pointer to a variable that receives the number of bytes
        /// transferred into the specified buffer. If lpNumberOfBytesRead is NULL,
        /// the parameter is ignored.</param>
        /// <returns>Returns true if completed successfully, false otherwise.</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr processHandle, IntPtr address, IntPtr outputBuffer,
            UIntPtr nBufferSize, out UIntPtr lpNumberOfBytesRead);

        //[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //private static extern bool ReadProcessMemory(IntPtr processHandle, IntPtr address, IntPtr outputBuffer,
        //    UIntPtr nBufferSize, out UIntPtr lpNumberOfBytesRead);


        /// <summary>
        /// Wrapper to read process memory with error handling.
        /// </summary>
        /// <param name="processHandle">See above.</param>
        /// <param name="address">See above.  Note that this is platform-specific,
        /// so an x64 OS will have a 64 bit pointer.</param>
        /// <param name="nBytesToRead">See above.</param>
        /// <returns>Returns a pointer to a global memory buffer.
        /// Call DoneReadingProcessMemory to release the memory.</returns>
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
                    Logger.Instance.Log(e);
                }
            }
        }
    }


}
