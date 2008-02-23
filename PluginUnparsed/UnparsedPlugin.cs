using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace WaywardGamers.KParser.Plugin
{
    public class UnparsedPlugin : BasePluginControl
    {
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
            if (dataSet.Tables.Contains("RecordLog"))
            {
                foreach (var row in dataSet.RecordLog)
                {
                    if (row.ParseSuccessful == false)
                    {
                        int startPos;
                        int endPos;

                        string timestampMsg = string.Format("[{0}]", row.Timestamp.ToLongTimeString());

                        startPos = richTextBox.Text.Length;
                        endPos = startPos + timestampMsg.Length;

                        richTextBox.AppendText(string.Format("{0} ", timestampMsg));
                        richTextBox.Select(startPos, endPos);
                        richTextBox.SelectionColor = Color.Purple;

                        startPos = richTextBox.Text.Length;
                        endPos = startPos + row.MessageText.Length;

                        richTextBox.AppendText(string.Format("{0}\n", row.MessageText));
                        richTextBox.Select(startPos, endPos);
                        richTextBox.SelectionColor = Color.Black;
                    }
                }
            }
        }
    }
}
