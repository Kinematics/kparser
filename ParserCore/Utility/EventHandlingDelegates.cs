using System;
using System.Threading;

#region Event classes
public delegate void DatabaseReparseEventHandler(object sender, DatabaseReparseEventArgs ramArgs);

public class DatabaseReparseEventArgs : EventArgs
{
    public int RowsRead { get; private set; }
    public int TotalRows { get; private set; }
    public bool Complete { get; private set; }

    internal DatabaseReparseEventArgs(int rowRead, int totalRows, bool complete)
    {
        RowsRead = rowRead;
        TotalRows = totalRows;
        Complete = complete;
    }
}
#endregion
