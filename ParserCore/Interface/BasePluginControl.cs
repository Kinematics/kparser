using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace WaywardGamers.KParser.Plugin
{
    public partial class BasePluginControl : UserControl, IPlugin
    {
        #region Constructor
        public BasePluginControl()
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

        public void DatabaseOpened(KPDatabaseDataSet dataSet)
        {
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
                richTextBox.SuspendLayout();
                ProcessData(dataSet);
                richTextBox.Select(0, 0);
                richTextBox.ResumeLayout();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error while processing plugin: \n" + e.Message);
            }
        }

        protected virtual void ProcessData(KPDatabaseDataSet dataSet)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Helper Methods for adding text to RTF control
        protected void AppendBoldText(string textToInsert, Color color)
        {
            int start = richTextBox.Text.Length;

            richTextBox.AppendText(textToInsert);

            richTextBox.Select(start, textToInsert.Length);

            richTextBox.SelectionFont = boldFont;

            richTextBox.SelectionColor = color;
        }

        protected void AppendBoldUnderText(string textToInsert, Color color)
        {
            int start = richTextBox.Text.Length;

            richTextBox.AppendText(textToInsert);

            richTextBox.Select(start, textToInsert.Length);

            richTextBox.SelectionFont = buFont;

            richTextBox.SelectionColor = color;
        }

        protected void AppendNormalText(string textToInsert)
        {
            int start = richTextBox.Text.Length;

            richTextBox.AppendText(textToInsert);

            richTextBox.Select(start, textToInsert.Length);

            richTextBox.SelectionFont = normFont;

            richTextBox.SelectionColor = Color.Black;
        }
        #endregion

    }
}
