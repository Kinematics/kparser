using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WaywardGamers.KParser.Plugin
{
    public partial class NewBasePluginControl : UserControl, IPlugin
    {
        #region Font Variables
        protected Font normFont;
        protected Font boldFont;
        protected Font underFont;
        protected Font buFont;
        #endregion

        #region Constructor
        public NewBasePluginControl()
        {
            InitializeComponent();

            normFont = new Font(FontFamily.GenericMonospace, 9.00f, FontStyle.Regular);
            boldFont = new Font(FontFamily.GenericMonospace, 9.00f, FontStyle.Bold);
            underFont = new Font(FontFamily.GenericMonospace, 9.00f, FontStyle.Underline);
            buFont = new Font(FontFamily.GenericMonospace, 9.00f, FontStyle.Bold | FontStyle.Underline);
        }
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

        public string TextContents
        {
            get { return this.richTextBox.Text; }
        }

        public void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void WatchDatabaseChanged(object sender, DatabaseWatchEventArgs e)
        {
            throw new NotImplementedException();
        }

        public virtual void DatabaseOpened(KPDatabaseDataSet dataSet)
        {
            ResetTextBox();
            HandleDataset(dataSet);
        }

        public virtual void Reset()
        {
            throw new NotImplementedException();
        }

        public virtual bool IsDebug
        {
            get { return false; }
        }

        #endregion

        #region Accessory IPlugin Functions
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
                richTextBox.Select(richTextBox.Text.Length, richTextBox.Text.Length);
                richTextBox.ResumeLayout();
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

        #region Helper functions for RTF TextBox
        protected void ResetTextBox()
        {
            if (this.InvokeRequired)
            {
                Action thisFunc = ResetTextBox;
                Invoke(thisFunc);
                return;
            }

            this.richTextBox.Clear();
        }

        protected void AppendText(string textToInsert)
        {
            AppendText(textToInsert, Color.Black, false, false);
        }

        protected void AppendText(string textToInsert, Color color)
        {
            AppendText(textToInsert, color, false, false);
        }

        protected void AppendText(string textToInsert, bool bold, bool underline)
        {
            AppendText(textToInsert, Color.Black, bold, underline);
        }

        protected void AppendText(string textToInsert, Color color, bool bold, bool underline)
        {
            if (this.InvokeRequired)
            {
                Action<string, Color, bool, bool> thisFunc = AppendText;
                Invoke(thisFunc, new object[] { textToInsert, color, bold, underline });
                return;
            }

            int start = richTextBox.Text.Length;
            richTextBox.AppendText(textToInsert);
            richTextBox.Select(start, textToInsert.Length);

            richTextBox.SelectionColor = color;

            // Set font to use
            if ((bold == true) && (underline == true))
            {
                richTextBox.SelectionFont = buFont;
            }
            else if (bold == true)
            {
                richTextBox.SelectionFont = boldFont;
            }
            else if (underline == true)
            {
                richTextBox.SelectionFont = underFont;
            }
            else
            {
                richTextBox.SelectionFont = normFont;
            }
        }

        protected void PushStrings(StringBuilder sb, List<StringMods> strModList)
        {
            int start = richTextBox.Text.Length;

            richTextBox.AppendText(sb.ToString());
            richTextBox.Select(start, sb.Length);
            richTextBox.SelectionFont = normFont;
            richTextBox.SelectionColor = Color.Black;

            foreach (var strMod in strModList)
            {
                richTextBox.Select(strMod.Start + start, strMod.Length);

                if ((strMod.Bold == true) && (strMod.Underline == true))
                {
                    richTextBox.SelectionFont = buFont;
                }
                else if (strMod.Bold == true)
                {
                    richTextBox.SelectionFont = boldFont;
                }
                else if (strMod.Underline == true)
                {
                    richTextBox.SelectionFont = underFont;
                }
                else
                {
                    richTextBox.SelectionFont = normFont;
                }

                richTextBox.SelectionColor = strMod.Color;
            }

            richTextBox.Select(0, 0);
        }

        #endregion
    }
}
