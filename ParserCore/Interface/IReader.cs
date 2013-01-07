using System;

namespace WaywardGamers.KParser.Interface
{
    /// <summary>
    /// Interface for readers of incoming FFXI information logs.
    /// </summary>
    public interface IReader
    {
        void Start();
        void Import(ImportSourceType importSource, IDBReader dbReaderManager, bool modifyTimestamp);
        void ImportRange(ImportSourceType importSource, IDBReader dbReaderManager, bool modifyTimestamp,
            DateTime startOfRange, DateTime endOfRange);
        void Join(ImportSourceType importSource, IDBReader dbReaderManager, IDBReader dbReaderManager2);
        void Stop();

        DataSource ParseModeType { get; }

        bool IsRunning { get; }

        event ReaderDataHandler ReaderDataChanged;
        event ReaderStatusHandler ReaderStatusChanged;

    }
}
