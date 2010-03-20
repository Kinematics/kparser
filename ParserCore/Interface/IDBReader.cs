using System;

namespace WaywardGamers.KParser.Interface
{
    /// <summary>
    /// Interface for different database readers.
    /// </summary>
    public interface IDBReader
    {
        void OpenDatabase(string openDatabaseFilename);
        void CloseDatabase();

        string DatabaseFilename { get; }
        string DatabaseParseVersion { get; }
    }
}
