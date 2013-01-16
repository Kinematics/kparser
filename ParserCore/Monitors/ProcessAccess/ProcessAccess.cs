using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using WaywardGamers.KParser.Monitoring.Memory;

namespace WaywardGamers.KParser.Monitoring
{
    internal static class ProcessAccess
    {
        /// <summary>
        /// This function searches the computer processes to locate the FFXI process.
        /// If a particular process ID is specified, it will restrict its search to
        /// that process.
        /// </summary>
        /// <param name="polPID">Optional process ID.  Set to 0 to find the first
        /// instance of FFXI on the computer.</param>
        /// <param name="_abort">A resettable event that can be set to indicate
        /// that the attempt to monitor is being aborted.
        /// Passing a null will cause this to loop forever until the process is found.
        /// Only do so if calling from debuggable code, never from production code.</param>
        /// <returns>Returns a POL object containing the process information needed,
        /// or null if no process was found and the request was aborted.</returns>
        internal static POL GetFFXIProcess(int polPID, ManualResetEvent _abort)
        {
#if !DEBUG
            if (_abort == null)
                throw new ArgumentNullException("_abort");
#endif


            // Keep going as long as we're still attempting to monitor
            while ((_abort == null) || (!_abort.WaitOne(0)))
            {
                try
                {
                    Trace.WriteLine(Thread.CurrentThread.Name + ": Attempting to connect to Final Fantasy.");

                    Process[] polProcesses;

                    // If we're given a specific process to connect to, try for that.
                    if (polPID != 0)
                    {
                        polProcesses = new Process[1];
                        polProcesses[0] = Process.GetProcessById(polPID);
                    }
                    else
                    {
                        // If we're not given a specific process, scan all processes for POL.
                        polProcesses = Process.GetProcessesByName("pol");
                    }


                    // If we've found any POL processes, examine them for the proper module.
                    if (polProcesses != null)
                    {
                        foreach (Process process in polProcesses)
                        {
                            foreach (ProcessModule module in process.Modules)
                            {
                                if (string.Compare(module.ModuleName, "ffximain.dll", true) == 0)
                                {
                                    Trace.WriteLine(string.Format("Module: {0}  Base Address: 0x{1:X8}", module.ModuleName, (uint)module.BaseAddress));
                                    return new POL(process, module.BaseAddress);
                                }
                            }
                        }

                        if (polPID != 0)
                            throw new InvalidOperationException("Specified process ID is not a POL process.");
                    }
                }
                catch (ArgumentException e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message,
                        "Process not found", System.Windows.Forms.MessageBoxButtons.OK);
                }
                catch (InvalidOperationException e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message,
                        Resources.PublicResources.Error, System.Windows.Forms.MessageBoxButtons.OK);
                }
                catch (Exception e)
                {
                    Logger.Instance.Log("Process access", String.Format(Thread.CurrentThread.Name + ": ERROR: An exception occured while trying to connect to Final Fantasy.  Message = {0}", e.Message));
                }

                // Wait before trying again.
                System.Threading.Thread.Sleep(5000);
            }

            // If we got here, attempt to acquire process was aborted.  Return null.
            return null;
        }

    }
}
