﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace WaywardGamers.KParser.Plugin
{
    public class UnparsedPlugin : BasePluginControl
    {
        #region Constructor
        public UnparsedPlugin()
        {
            toolStrip.Enabled = false;
            toolStrip.Visible = false;
        }
        #endregion

        #region IPlugin Overrides
        public override string TabName
        {
            get { return "Unparsed Data"; }
        }

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

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
        {
            datasetToUse = e.DatasetChanges;
            return true;
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
                        sb.AppendFormat("[{0}] ", row.Timestamp.ToLongTimeString());

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
    }
}
