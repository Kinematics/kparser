﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WaywardGamers.KParser.Plugin
{
    public partial class BasePluginControlWithDropdown : UserControl, IPlugin
    {
        #region Constructor
        public BasePluginControlWithDropdown()
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
            this.richTextBox.Clear();
            HandleDataset(dataSet);
        }

        public virtual void Reset()
        {
            throw new NotImplementedException();
        }

        public virtual bool IsDebug
        { get { return false; } }
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

        #region Helper Methods for reading and/or updating the UI via secondary threads.
        protected void GetMobFilter(ComboBox combo, out string mobFilterSet, out string mobName, out int mobXP)
        {
            if (combo == null)
                throw new ArgumentNullException("combo");

            if (combo.SelectedIndex >= 0)
                mobFilterSet = combo.SelectedItem.ToString();
            else
                mobFilterSet = "All";

            mobName = "All";
            mobXP = 0;

            if (mobFilterSet != "All")
            {
                Regex mobAndXP = new Regex(@"((?<mobName>.*(?<! \())) \(((?<xp>\d+)\))|(?<mobName>.*)");
                Match mobAndXPMatch = mobAndXP.Match(mobFilterSet);

                if (mobAndXPMatch.Success == true)
                {
                    mobName = mobAndXPMatch.Groups["mobName"].Value;

                    if ((mobAndXPMatch.Groups["xp"] != null) && (mobAndXPMatch.Groups["xp"].Value != string.Empty))
                    {
                        mobXP = int.Parse(mobAndXPMatch.Groups["xp"].Value);
                    }
                }
            }
        }

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

        protected void ResetComboBox2()
        {
            if (this.InvokeRequired)
            {
                Action thisFunc = ResetComboBox2;
                Invoke(thisFunc);
                return;
            }

            this.comboBox2.Items.Clear();
        }

        protected void AddToComboBox2(string p)
        {
            if (this.InvokeRequired)
            {
                Action<string> thisFunc = AddToComboBox2;
                Invoke(thisFunc, new object[] { p });
                return;
            }

            this.comboBox2.Items.Add(p);
        }

        protected void RemoveFromComboBox2(string p)
        {
            if (this.InvokeRequired)
            {
                Action<string> thisFunc = AddToComboBox2;
                Invoke(thisFunc, new object[] { p });
                return;
            }

            int foundIndex = this.comboBox2.Items.IndexOf(p);
            if (foundIndex > 0)
                this.comboBox2.Items.RemoveAt(foundIndex);
        }

        protected void InitComboBox2Selection()
        {
            if (this.InvokeRequired)
            {
                Action thisFunc = InitComboBox2Selection;
                Invoke(thisFunc);
                return;
            }

            if (this.comboBox2.Items.Count > 0)
                this.comboBox2.SelectedIndex = 0;
        }

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

        #region Event Handlers
        protected virtual void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        protected virtual void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        protected virtual void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        protected virtual void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }
        #endregion

    }
}
