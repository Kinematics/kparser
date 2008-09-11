using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using WaywardGamers.KParser;

namespace WaywardGamers.KParser.Plugin
{
    public class ChatPlugin : BasePluginControl
    {
        #region Constructor
        ToolStripComboBox chatTypeCombo = new ToolStripComboBox();
        ToolStripComboBox speakerCombo = new ToolStripComboBox();
        bool flagNoUpdate = false;

        public ChatPlugin()
        {
            richTextBox.WordWrap = true;

            ToolStripLabel catLabel = new ToolStripLabel();
            catLabel.Text = "Chat Type:";
            toolStrip.Items.Add(catLabel);

            chatTypeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            chatTypeCombo.MaxDropDownItems = 10;
            chatTypeCombo.Items.Add("All");
            for (var chat = ChatMessageType.Say; chat <= ChatMessageType.Arena; chat++)
            {
                chatTypeCombo.Items.Add(chat.ToString());
            }

            chatTypeCombo.SelectedIndex = 0;
            chatTypeCombo.SelectedIndexChanged += new EventHandler(this.chatTypeCombo_SelectedIndexChanged);
            toolStrip.Items.Add(chatTypeCombo);


            ToolStripLabel speakerLabel = new ToolStripLabel();
            speakerLabel.Text = "Speaker:";
            toolStrip.Items.Add(speakerLabel);

            speakerCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            speakerCombo.MaxDropDownItems = 10;
            speakerCombo.Items.Add("All");
            speakerCombo.SelectedIndex = 0;
            speakerCombo.SelectedIndexChanged += new EventHandler(this.speakerCombo_SelectedIndexChanged);
            toolStrip.Items.Add(speakerCombo);
        }
        #endregion

        #region IPlugin members
        public override string TabName
        {
            get { return "Chat"; }
        }

        public override void Reset()
        {
            ResetTextBox();

            speakerCombo.CBReset();
            speakerCombo.CBAddStrings(new string[1] { "All" });
            speakerCombo.CBSelectIndex(0);
        }

        public override void DatabaseOpened(KPDatabaseDataSet dataSet)
        {
            ResetTextBox();
            UpdateSpeakerList(dataSet);
            flagNoUpdate = true;
            speakerCombo.CBSelectItem("All");

            ProcessData(dataSet);
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
        {
            if (e.DatasetChanges.ChatSpeakers.Count != 0)
            {
                string currentSelection = speakerCombo.CBSelectedItem();
                flagNoUpdate = true;
                UpdateSpeakerList(e.DatasetChanges);
                flagNoUpdate = true;
                speakerCombo.CBSelectItem(currentSelection);

                datasetToUse = e.DatasetChanges;
                return true;
            }

            if (e.DatasetChanges.ChatMessages.Count != 0)
            {
                datasetToUse = e.DatasetChanges;
                return true;
            }

            datasetToUse = null;
            return false;
        }

        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            // Update chat messages list based on filtering
            ChatMessageType chatFilter = (ChatMessageType)chatTypeCombo.CBSelectedIndex();
            string player = speakerCombo.CBSelectedItem();

            var filteredChat = dataSet.ChatMessages.Where(m =>
                chatFilter == ChatMessageType.Unknown || chatFilter == (ChatMessageType)m.ChatType);

            if (player != "All")
            {
                filteredChat = filteredChat.Where(m =>
                    player == m.ChatSpeakersRow.SpeakerName);
            }


            List<StringMods> strModList = new List<StringMods>();
            StringBuilder sb = new StringBuilder();
            int start = 0;
            Color chatColor;

            foreach (var row in filteredChat)
            {
                start = sb.Length;
                sb.AppendFormat("[{0}] ", row.Timestamp.ToLongTimeString());

                strModList.Add(new StringMods
                {
                    Start = start,
                    Length = sb.Length - start,
                    Color = Color.Purple
                });

                switch ((ChatMessageType)row.ChatType)
                {
                    case ChatMessageType.Say:
                        chatColor = Color.Gray;
                        break;
                    case ChatMessageType.Shout:
                        chatColor = Color.Orange;
                        break;
                    case ChatMessageType.Tell:
                        chatColor = Color.Magenta;
                        break;
                    case ChatMessageType.Party:
                        chatColor = Color.Blue;
                        break;
                    case ChatMessageType.Linkshell:
                        chatColor = Color.Green;
                        break;
                    case ChatMessageType.Emote:
                        chatColor = Color.Indigo;
                        break;
                    case ChatMessageType.NPC:
                        chatColor = Color.Black;
                        break;
                    default:
                        chatColor = Color.Black;
                        break;
                }

                start = sb.Length;
                sb.AppendFormat("{0}\n", row.Message);

                strModList.Add(new StringMods
                {
                    Start = start,
                    Length = sb.Length - start,
                    Color = chatColor
                });

            }

            PushStrings(sb, strModList);

        }
        #endregion

        #region Private functions
        /// <summary>
        /// Update the drop-down combo box with the currently known list of speaker
        /// names.
        /// </summary>
        /// <param name="dataSet">Dataset with possibly new speakers to add to the list.</param>
        private void UpdateSpeakerList(KPDatabaseDataSet dataSet)
        {
            string[] currentSpeakerList = speakerCombo.CBGetStrings();
            string[] newSpeakerList = dataSet.ChatSpeakers.OrderBy(s => s.SpeakerName).Select(s => s.SpeakerName).ToArray();

            List<string> newSpeakers = new List<string>();

            foreach (var speaker in newSpeakerList)
            {
                if (currentSpeakerList.Contains(speaker) == false)
                    newSpeakers.Add(speaker);
            }

            if (newSpeakers.Count > 0)
            {
                string[] completeSpeakerList = currentSpeakerList
                    .Concat(newSpeakerList).Distinct().ToArray();
                Array.Sort(completeSpeakerList);

                newSpeakers.Clear();
                newSpeakers.AddRange(completeSpeakerList);
                newSpeakers.RemoveAll(s => s == "All");
                newSpeakers.Insert(0, "All");

                completeSpeakerList = newSpeakers.ToArray();

                speakerCombo.CBReset();
                speakerCombo.CBAddStrings(completeSpeakerList);
            }
        }
        #endregion

        #region Event handlers
        protected void chatTypeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
            {
                ResetTextBox();
                HandleDataset(DatabaseManager.Instance.Database);
            }

            flagNoUpdate = false;
        }

        protected void speakerCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
            {
                ResetTextBox();
                HandleDataset(DatabaseManager.Instance.Database);
            }

            flagNoUpdate = false;
        }
        #endregion
    }
}
