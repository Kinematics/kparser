using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace WaywardGamers.KParser.Plugin
{
    public partial class BasePluginControlListView : UserControl, IPlugin
    {
        #region Constructor
        public BasePluginControlListView()
        {
            InitializeComponent();

            boldFont = new Font(FontFamily.GenericMonospace, 10.00f, FontStyle.Bold);
            normFont = new Font(FontFamily.GenericMonospace, 10.00f, FontStyle.Regular);
            buFont = new Font(FontFamily.GenericMonospace, 10.00f, FontStyle.Bold | FontStyle.Underline);
        }
        #endregion

        #region Member Variables
        protected Font boldFont;
        protected Font normFont;
        protected Font buFont;
        #endregion

        #region IPlugin Members

        public virtual string TabName
        {
            get { throw new NotImplementedException(); }
        }

        public UserControl Control
        {
            get { return (this as UserControl); }
        }

        public void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            KPDatabaseDataSet dataSet;
            if (FilterOnDatabaseChanging(e, out dataSet))
                HandleDataset(dataSet);
        }

        public void WatchDatabaseChanged(object sender, DatabaseWatchEventArgs e)
        {
            KPDatabaseDataSet dataSet;
            if (FilterOnDatabaseChanged(e, out dataSet))
                HandleDataset(dataSet);
        }

        public virtual void DatabaseOpened(KPDatabaseDataSet dataSet)
        {
            this.
            HandleDataset(dataSet);
        }

        public virtual void Reset()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Assistant Methods for datamining
        protected virtual bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
        {
            datasetToUse = null;
            return false;
        }

        protected virtual bool FilterOnDatabaseChanged(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
        {
            datasetToUse = null;
            return false;
        }

        protected void HandleDataset(KPDatabaseDataSet dataSet)
        {
            if (dataSet == null)
                return;

            if (InvokeRequired)
            {
                DatasetInvoker reReadDatabase = new DatasetInvoker(HandleDataset);
                object[] passDataset = new object[1] { dataSet };
                Invoke(reReadDatabase, passDataset);
                return;
            }

            try
            {
                listView.SuspendLayout();
                ProcessData(dataSet);
                listView.ResumeLayout();
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e);
                MessageBox.Show("Error while processing plugin: \n" + e.Message);
            }
        }

        protected virtual void ProcessData(KPDatabaseDataSet dataSet)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
