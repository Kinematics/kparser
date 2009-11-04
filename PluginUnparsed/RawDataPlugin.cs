using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Resources;

namespace WaywardGamers.KParser.Plugin
{
    public class RawDataPlugin : BasePluginControl
    {
        #region Member Variables
        ToolStripButton showUnparsedData = new ToolStripButton();
        ToolStripButton showAllData = new ToolStripButton();

        bool flagNoUpdate = false;
        bool showAllDataFlag = false;
        #endregion

        #region Constructor
        public RawDataPlugin()
        {
            LoadLocalizedUI();

            showUnparsedData.CheckOnClick = true;
            showAllData.CheckOnClick = true;
            showUnparsedData.Checked = true;
            showAllData.Checked = false;

            showUnparsedData.CheckedChanged += new EventHandler(showUnparsedData_CheckedChanged);
            showAllData.CheckedChanged += new EventHandler(showAllData_CheckedChanged);

            toolStrip.Items.Add(showUnparsedData);
            toolStrip.Items.Add(showAllData);

        }
        #endregion

        #region IPlugin Overrides
        public override bool IsDebug
        {
            get
            {
                return true;
            }
        }

        public override void Reset()
        {
            ResetTextBox();
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            HandleDataset(e.DatasetChanges);
        }

        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            StringBuilder sb = new StringBuilder();
            List<StringMods> strModList = new List<StringMods>();
            int start;

            if (dataSet.Tables.Contains("RecordLog"))
            {
                if (showAllDataFlag)
                {
                    foreach (var row in dataSet.RecordLog)
                    {
                        start = sb.Length;
                        sb.AppendFormat("[{0}] ", row.Timestamp.ToLocalTime().ToLongTimeString());

                        strModList.Add(new StringMods
                        {
                            Start = start,
                            Length = sb.Length - start,
                            Color = Color.Purple
                        });

                        sb.AppendFormat("{0}\n", row.MessageText);
                    }
                }
                else
                {
                    foreach (var row in dataSet.RecordLog)
                    {
                        if (row.ParseSuccessful == false)
                        {

                            start = sb.Length;
                            sb.AppendFormat("[{0}] ", row.Timestamp.ToLocalTime().ToLongTimeString());

                            strModList.Add(new StringMods
                            {
                                Start = start,
                                Length = sb.Length - start,
                                Color = Color.Purple
                            });

                            sb.AppendFormat("{0}\n", row.MessageText);
                        }
                    }
                }

                PushStrings(sb, strModList);
            }
        }
        #endregion

        #region Event Handlers
        void showAllData_CheckedChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
            {
                try
                {
                    flagNoUpdate = true;
                    showAllDataFlag = showAllData.Checked;
                    showUnparsedData.Checked = !showAllDataFlag;
                }
                finally
                {
                    flagNoUpdate = false;
                }

                HandleDataset(null);
            }
        }

        void showUnparsedData_CheckedChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
            {
                try
                {
                    flagNoUpdate = true;
                    showAllDataFlag = !(showUnparsedData.Checked);
                    showAllData.Checked = showAllDataFlag;
                }
                finally
                {
                    flagNoUpdate = false;
                }

                HandleDataset(null);
            }
        }
        #endregion

        #region Localization Overrides
        protected override void LoadLocalizedUI()
        {
            showUnparsedData.Text = Resources.Debugging.ShowUnparsedData;
            showAllData.Text = Resources.Debugging.ShowAllData;
        }

        protected override void LoadResources()
        {
            this.tabName = Resources.Debugging.RawDataPluginTabName;
        }
        #endregion
    }
}
