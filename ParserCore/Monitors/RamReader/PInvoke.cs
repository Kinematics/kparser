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
    /// Class to handle pointer manipulation.
    /// </summary>
    internal static class Pointers
    {
        /// <summary>
        /// This function allows us to do basic pointer arithmatic, incrementing
        /// a pointer by a set number of bytes.
        /// </summary>
        /// <param name="pointer">The initial memory address.</param>
        /// <param name="numBytes">The number of bytes to move relative to the initial address.</param>
        /// <returns>Returns the new pointer address.</returns>
        internal static IntPtr IncrementPointer(IntPtr pointer, uint numBytes)
        {
            return (IntPtr)((uint)pointer + numBytes);
        }

        /// <summary>
        /// This function dereferences a pointer and returns the address that
        /// the pointer pointed to.
        /// </summary>
        /// <param name="processHandle">The pointer to the process whose memory
        /// space we're examining.</param>
        /// <param name="pointerToFollow">The original pointer.</param>
        /// <returns>The 'value' of the pointer; the location the pointer pointed to.
        /// Returns IntPtr.Zero (null pointer) if we are unable to read the memory address.</returns>
        internal static IntPtr FollowPointer(IntPtr processHandle, IntPtr pointerToFollow)
        {
            if (pointerToFollow == IntPtr.Zero)
                throw new ArgumentOutOfRangeException("pointerToFollow", "Cannot dereference a null pointer.");

            using (ProcessMemoryReading pmr = new ProcessMemoryReading(processHandle, pointerToFollow, (uint)IntPtr.Size))
            {
                if (pmr.ReadBufferPtr == IntPtr.Zero)
                    return IntPtr.Zero;

                return Marshal.ReadIntPtr(pmr.ReadBufferPtr);
            }
        }
    }

    /// <summary>
    /// Class to allow encapsulation of accessing process memory in a using() block.
    /// EG:
    /// using (ProcessMemoryReading pmr = new ProcessMemoryReading(pol.Process.Handle, bufferStartAddress, (uint)bufferSize))
    /// {
    ///     Marshal.PtrToStringUni(pmr.ReadBufferPtr, (int)bufferSize);
    /// }
    /// 
    /// This class automatically disposes/releases the PInvoke pointer at the end of the using clause.
    /// </summary>
    internal class ProcessMemoryReading : IDisposable
    {
        #region Imported Functionality

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
        private static extern bool ReadProcessMemory(
            IntPtr processHandle,
            IntPtr address,
            IntPtr outputBuffer,
            uint nBufferSize,
            ref uint lpNumberOfBytesRead);

        #endregion

        #region Member Variables
        bool disposed;
        private IntPtr pointerToMemoryBuffer;
        #endregion

        #region Properties
        internal IntPtr ReadBufferPtr
        {
            get { return pointerToMemoryBuffer; }
        }
        #endregion

        #region Constructor / Destructor

        internal ProcessMemoryReading(IntPtr processHandle, IntPtr address, uint nBytesToRead)
        {
            pointerToMemoryBuffer = ReadProcessMemory(processHandle, address, nBytesToRead);
        }

        ~ProcessMemoryReading()
        {
            Dispose(false);
        }
        #endregion

        #region Private Functions
        /// <summary>
        /// Wrapper to read process memory with error handling.
        /// </summary>
        /// <param name="processHandle">See above.</param>
        /// <param name="address">See above.  Note that this is platform-specific,
        /// so an x64 OS will have a 64 bit pointer.</param>
        /// <param name="nBytesToRead">See above.</param>
        /// <returns>Returns a pointer to a global memory buffer.
        /// Returns IntPtr.Zero if an error occured in the kernel call.
        /// Throws an exception if nBytesToRead overflows an int value.
        /// Call DoneReadingProcessMemory to release the memory.</returns>
        private IntPtr ReadProcessMemory(IntPtr processHandle, IntPtr address, uint nBytesToRead)
        {
            // This will throw an exception on overflow conversion.
            int bufferBytes = Convert.ToInt32(nBytesToRead);

            IntPtr buffer = Marshal.AllocHGlobal(bufferBytes);

            uint bytesRead = 0;

            if (ReadProcessMemory(processHandle, address, buffer, nBytesToRead, ref bytesRead) == false)
            {
                // Returned false, that means an error occured.

                int Error = Marshal.GetLastWin32Error();

                // Go ahead and allow partial copies through
                if (Error == 299)	//ERROR_PARTIAL_COPY
                    return buffer;

                // Otherwise release the buffer immediately and return a null pointer.
                Marshal.FreeHGlobal(buffer);
                return IntPtr.Zero;
            }

            return buffer;
        }

        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool p)
        {
            if (disposed == false)
            {
                if (pointerToMemoryBuffer != IntPtr.Zero)
                {
                    try
                    {
                        Marshal.FreeHGlobal(pointerToMemoryBuffer);
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.Log(e);
                    }
                }

                disposed = true;
            }
        }
        #endregion

    }
}
