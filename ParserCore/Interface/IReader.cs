using System;

namespace WaywardGamers.KParser.Interface
{
    /// <summary>
    /// Interface for readers of incoming FFXI information logs.
    /// </summary>
    internal interface IReader
    {
        void Start();
        void Stop();

        DataSource ParseModeType { get; }

        bool IsRunning { get; }

        void Import(ImportSource importSource);

        event ReaderDataHandler ReaderDataChanged;
        event ReaderStatusHandler ReaderStatusChanged;

    }
}
