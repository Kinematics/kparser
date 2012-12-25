using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using WaywardGamers.KParser.Monitoring.Memory;
using WaywardGamers.KParser.Interface;
using ZMQ;

namespace WaywardGamers.KParser.Monitoring
{
    public class PacketReader : AbstractReader
    {
        #region Singleton Constructor
        // Make the class a singleton
        private static readonly PacketReader instance = new PacketReader();

        /// <summary>
        /// Gets the singleton instance of the LogParser class.
        /// This is the only internal way to access the singleton.
        /// </summary>
        internal static PacketReader Instance { get { return instance; } }

        /// <summary>
        /// Private constructor ensures singleton purity.
        /// </summary>
        private PacketReader()
		{
        }
		#endregion

        #region Member Variables
        Properties.Settings appSettings = new WaywardGamers.KParser.Properties.Settings();

        Thread readerThread;

        int polPID = 0;
        POL pol;
        uint initialMemoryOffset;

        ManualResetEvent abortMonitorThread = new ManualResetEvent(false);
        #endregion


        #region Interface Control Methods and Properties

        /// <summary>
        /// Return type of DataSource this reader works on.
        /// </summary>
        public override DataSource ParseModeType { get { return DataSource.Packet; } }

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

                abortMonitorThread.Reset();

                // Begin the thread
                readerThread = new Thread(Monitor);
                readerThread.IsBackground = true;
                readerThread.Name = "Packet Monitor Thread";
                readerThread.Start();
            }
            catch (System.Exception)
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

            if (pol != null)
                pol.Process.Exited -= new EventHandler(PolExited);

            IsRunning = false;
        }

        /// <summary>
        /// Call this function rather than aborting the thread directly.
        /// </summary>
        internal void Abort()
        {
            abortMonitorThread.Set();
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

            OnReaderStatusChanged(new ReaderStatusEventArgs()
            {
                Active = true,
                DataSourceType = this.ParseModeType,
                StatusMessage = "FFXI Exited"
            });
        }

        #endregion


        #region Monitor RAM
        /// <summary>
        /// This function is run as a thread to read ram and raise events when new
        /// data shows up.
        /// </summary>
        internal void Monitor()
        {
            pol = ProcessAccess.GetFFXIProcess(polPID, abortMonitorThread);
            if (pol == null)
            {
                OnReaderStatusChanged(new ReaderStatusEventArgs()
                {
                    Active = true,
                    DataSourceType = this.ParseModeType,
                    StatusMessage = "Failed to find FFXI"
                });

                return;
            }
            else
            {
                OnReaderStatusChanged(new ReaderStatusEventArgs()
                {
                    Active = true,
                    DataSourceType = this.ParseModeType,
                    StatusMessage = "Found FFXI"
                });

                pol.Process.Exited += new EventHandler(PolExited);
            }

            while (!abortMonitorThread.WaitOne(0))
            {
                // Monitor packets.
            }
        }
        #endregion
    }
}
