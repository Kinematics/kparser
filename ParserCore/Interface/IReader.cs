using System;

namespace WaywardGamers.KParser.Interface
{
    /// <summary>
    /// Interface for readers of incoming FFXI information logs.
    /// </summary>
    public interface IReader
    {
        void Start();
        void Import(ImportSourceType importSource, IDBReader dbReaderManager);
        void Stop();

        DataSource ParseModeType { get; }

        bool IsRunning { get; }

        event ReaderDataHandler ReaderDataChanged;
        event ReaderStatusHandler ReaderStatusChanged;

    }
}
