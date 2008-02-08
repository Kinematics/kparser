using System;

namespace WaywardGamers.KParser.Interface
{
    /// <summary>
    /// Interface for readers of incoming FFXI information logs.
    /// </summary>
    internal interface IReader
    {
        void Run();
        void Stop();

        bool IsRunning { get; }
    }
}
