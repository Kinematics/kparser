using System;
using WaywardGamers.KParser.Interface;

namespace WaywardGamers.KParser.Monitoring
{
    public abstract class AbstractReader : IReader
    {
        #region IReader Members

        public virtual void Start()
        {
            throw new NotImplementedException();
        }

        public virtual void Import(ImportSourceType importSource, IDBReader dbReaderManager, bool modifyTimestamp)
        {
            throw new NotImplementedException();
        }

        public virtual void ImportRange(ImportSourceType importSource, IDBReader dbReaderManager,
            bool modifyTimestamp, DateTime startOfRange, DateTime endOfRange)
        {
            throw new NotImplementedException();
        }

        public virtual void Join(ImportSourceType importSource, IDBReader dbReaderManager,
            bool modifyTimestamp, IDBReader dbReaderManager2, bool modifyTimestamp2)
        {
            throw new NotImplementedException();
        }

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
