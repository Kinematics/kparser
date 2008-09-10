using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;

namespace WaywardGamers.KParser.Plugin
{
    public class ChatPlugin : BasePluginControlWithDropdown
    {
        public override string TabName
        {
            get { return "Chat"; }
        }

        public override void Reset()
        {
            richTextBox.Clear();

            label1.Text = "Chat Type";
            comboBox1.Left = label1.Right + 10;
            comboBox1.MaxDropDownItems = 9;
            comboBox1.Items.Clear();
            comboBox1.Items.Add("All");
            for (var chat = ChatMessageType.Say; chat <= ChatMessageType.Arena; chat++)
            {
                comboBox1.Items.Add(chat.ToString());
            }
            comboBox1.SelectedIndex = 0;

            label2.Left = comboBox1.Right + 20;
            label2.Text = "Speaker";
            comboBox2.Left = label2.Right + 10;
            comboBox2.Items.Clear();
            comboBox2.Items.Add("All");
            comboBox2.SelectedIndex = 0;

            checkBox1.Enabled = false;
            checkBox1.Visible = false;
            checkBox2.Enabled = false;
            checkBox2.Visible = false;
        }

        public override void DatabaseOpened(KPDatabaseDataSet dataSet)
        {
            ResetComboBox2();
            AddStringToComboBox2("All");
            ResetTextBox();

            var speakerList = dataSet.ChatSpeakers.OrderBy(s => s.SpeakerName);

            foreach (var speaker in speakerList)
            {
                AddStringToComboBox2(speaker.SpeakerName);
            }

            InitComboBox2Selection();
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
        {
            if (e.DatasetChanges != null)
            {
                if (e.DatasetChanges.ChatSpeakers != null)
                {
                    if (e.DatasetChanges.ChatSpeakers.Count != 0)
                    {
                        foreach (var speaker in e.DatasetChanges.ChatSpeakers)
                        {
                            AddStringToComboBox2(speaker.SpeakerName);
                        }

                        datasetToUse = e.DatasetChanges;
                        return true;
                    }
                }

                if (e.DatasetChanges.ChatMessages != null)
                {
                    if (e.DatasetChanges.ChatMessages.Count != 0)
                    {
                        datasetToUse = e.DatasetChanges;
                        return true;
                    }
                }
            }


            datasetToUse = null;
            return false;
        }

        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            // Update chat messages list based on filtering
            ChatMessageType chatFilter = (ChatMessageType)comboBox1.SelectedIndex;
            string player = comboBox2.SelectedItem.ToString();

            var filteredChat = dataSet.ChatMessages.Where(m =>
                chatFilter == ChatMessageType.Unknown || chatFilter == (ChatMessageType)m.ChatType);

            var filteredPlayerChat = filteredChat.Where(m =>
                player == "All" || player == m.ChatSpeakersRow.SpeakerName);

            foreach (var row in filteredPlayerChat)
            {
                AddMessageLine(row.Message, (ChatMessageType)row.ChatType, row.Timestamp);
            }
        }

        /// <summary>
        /// Adds a message line to the current rich text display.
        /// </summary>
        /// <param name="message">Message text to add.</param>
        /// <param name="chatMessageType">The type of message, so we can color-code it.</param>
        /// <param name="timestamp">The timestamp for the message.</param>
        private void AddMessageLine(string message, ChatMessageType chatMessageType, DateTime timestamp)
        {
            int startPos;
            int endPos;

            string timestampMsg = string.Format("[{0}]", timestamp.ToLongTimeString());

            startPos = richTextBox.Text.Length;
            endPos = startPos + timestampMsg.Length;

            richTextBox.AppendText(string.Format("{0} ", timestampMsg));
            richTextBox.Select(startPos, endPos);
            richTextBox.SelectionColor = Color.Purple;

            startPos = richTextBox.Text.Length;
            endPos = startPos + message.Length;

            richTextBox.AppendText(string.Format("{0}\n", message));
            richTextBox.Select(startPos, endPos);

            switch (chatMessageType)
            {
                case ChatMessageType.Say:
                    richTextBox.SelectionColor = Color.Gray;
                    break;
                case ChatMessageType.Shout:
                    richTextBox.SelectionColor = Color.Orange;
                    break;
                case ChatMessageType.Tell:
                    richTextBox.SelectionColor = Color.Magenta;
                    break;
                case ChatMessageType.Party:
                    richTextBox.SelectionColor = Color.Blue;
                    break;
                case ChatMessageType.Linkshell:
                    richTextBox.SelectionColor = Color.Green;
                    break;
                case ChatMessageType.Emote:
                    richTextBox.SelectionColor = Color.Indigo;
                    break;
                case ChatMessageType.NPC:
                    richTextBox.SelectionColor = Color.Black;
                    break;
                default:
                    richTextBox.SelectionColor = Color.Black;
                    break;
            }
        }

        protected override void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            richTextBox.Clear();
            HandleDataset(DatabaseManager.Instance.Database);
        }

        protected override void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            richTextBox.Clear();
            HandleDataset(DatabaseManager.Instance.Database);
        }
    }
}
