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

        public override void Reset()
        {
            richTextBox.Clear();
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseODataSet datasetToUse)
        {
            datasetToUse = e.DatasetChanges;
            return true;
        }

        protected override void ProcessData(KPDatabaseODataSet dataSet)
        {
            if (dataSet.Tables.Contains("ChatLogRecord"))
            {
                foreach (var row in dataSet.ChatLogRecord)
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
                        endPos = startPos + row.OriginalMessageText.Length;

                        richTextBox.AppendText(string.Format("{0}\n", row.OriginalMessageText));
                        richTextBox.Select(startPos, endPos);
                        richTextBox.SelectionColor = Color.Black;
                    }
                }
            }
        }
    }
}
