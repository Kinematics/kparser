using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace WaywardGamers.KParser.Plugin
{
    // For use when needing to invoke into the main messagepump thread.
    public delegate void DatasetInvoker(KPDatabaseODataSet dataset);

    public interface IPlugin
    {
        string TabName { get; }
        UserControl Control { get; }

        void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e);
        void WatchDatabaseChanged(object sender, DatabaseWatchEventArgs e);

        void DatabaseOpened(KPDatabaseODataSet dataSet);
        void Reset();
    }
}
