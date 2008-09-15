using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace WaywardGamers.KParser.Plugin
{
    // For use when needing to invoke into the main messagepump thread.
    public delegate void DatasetInvoker(KPDatabaseDataSet dataset);

    public interface IPlugin
    {
        string TabName { get; }
        UserControl Control { get; }
        string TextContents { get; }

        void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e);
        void WatchDatabaseChanged(object sender, DatabaseWatchEventArgs e);

        void DatabaseOpened(KPDatabaseDataSet dataSet);
        void Reset();

        bool IsDebug { get; }
    }
}
