using System;
using System.Diagnostics;

namespace WaywardGamers.KParser
{
    /// <summary>
    /// This class describes a profiled region
    /// </summary>
    public class RegionProfiler : IDisposable
    {
        private string regionName;

        private Stopwatch stopwatch = new Stopwatch();
        private TimeSpan watermark = new TimeSpan(0, 0, 2);

        /// <summary>
        /// Gets the elapsed time.
        /// </summary>
        /// <value>The elapsed time.</value>
        public TimeSpan ElapsedTime
        {
            get
            {
                return stopwatch.Elapsed;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileRegion"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public RegionProfiler(string name)
        {
            regionName = name;

            stopwatch.Reset();
            stopwatch.Start();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileRegion"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="watermarkParam">The watermark param.</param>
        public RegionProfiler(string name, TimeSpan watermarkParam)
            : this(name)
        {
            watermark = watermarkParam;
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="ProfileRegion"/> is reclaimed by garbage collection.
        /// </summary>
        ~RegionProfiler()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        public void Dispose(bool disposing)
        {
            stopwatch.Stop();

            if (disposing == true)
            {
                string outputLine = string.Concat("Profiling Region (", regionName, "): ", stopwatch.Elapsed.TotalMilliseconds, " ms");

                if (stopwatch.Elapsed < watermark)
                {
                    Debug.WriteLine(outputLine);
                }
                else
                {
                    Trace.WriteLine(outputLine);
                }

                //Logger.Instance.Log("Region Profiling", outputLine);
            }
        }
    }
}
