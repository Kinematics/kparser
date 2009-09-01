using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Resources;

namespace WaywardGamers.KParser.Plugin
{
    public class UnparsedPlugin : BasePluginControl
    {
        #region Constructor
        public UnparsedPlugin()
        {
            toolStrip.Enabled = false;
            toolStrip.Visible = false;

            richTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left |
                System.Windows.Forms.AnchorStyles.Right |
                System.Windows.Forms.AnchorStyles.Bottom;
            richTextBox.Top -= toolStrip.Height;
            richTextBox.Height += toolStrip.Height;
            richTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Left |
                System.Windows.Forms.AnchorStyles.Right |
                System.Windows.Forms.AnchorStyles.Bottom;
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
            richTextBox.Clear();
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

                PushStrings(sb, strModList);
            }
        }
        #endregion

        #region Localization Overrides
        protected override void LoadResources()
        {
            base.LoadResources();

            this.tabName = Resources.Debugging.unparsedDataPluginTabName;
        }
        #endregion
    }
}
