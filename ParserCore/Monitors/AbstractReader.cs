using System;
using WaywardGamers.KParser.Interface;

namespace WaywardGamers.KParser.Monitoring
{
    public abstract class AbstractReader : IReader
    {
        #region IReader Members

        public abstract void Run();

        public abstract void Import(ImportSource importSource);

        public abstract void Stop();

        public abstract DataSource ParseModeType { get; }

        /// <summary>
        /// Gets the state of the reader thread.
        /// </summary>
        public bool IsRunning { get; protected set; }

        /// <summary>
        /// Event to link to to receive the data the reader reads.
        /// </summary>
        public event ReaderDataHandler ReaderDataChanged;

        /// <summary>
        /// Event to link to to receive the reader's current progress/status.
        /// </summary>
        public event ReaderStatusHandler ReaderStatusChanged;
        #endregion

        #region Functions for event handling
        protected virtual void OnReaderDataChanged(ReaderDataEventArgs e)
        {
            ReaderDataHandler copyReaderDataChanged = ReaderDataChanged;
            if (copyReaderDataChanged != null)
                copyReaderDataChanged(this, e);
            
        }

        protected virtual void OnReaderStatusChanged(ReaderStatusEventArgs e)
        {
            ReaderStatusHandler copyReaderStatusChanged = ReaderStatusChanged;
            if (copyReaderStatusChanged != null)
                copyReaderStatusChanged(this, e);

        }
        #endregion
    }
}
