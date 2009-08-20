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
        #region Member Variables
        ToolStripComboBox chatTypeCombo = new ToolStripComboBox();
        ToolStripComboBox speakerCombo = new ToolStripComboBox();
        ToolStripLabel catLabel = new ToolStripLabel();
        ToolStripLabel speakerLabel = new ToolStripLabel();

        bool flagNoUpdate = false;
        #endregion

        #region Constructor
        public ChatPlugin()
        {
            richTextBox.WordWrap = true;

            LoadLocalizedUI();

            chatTypeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            chatTypeCombo.MaxDropDownItems = 10;
            chatTypeCombo.SelectedIndexChanged += new EventHandler(this.chatTypeCombo_SelectedIndexChanged);

            speakerCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            speakerCombo.MaxDropDownItems = 10;
            speakerCombo.SelectedIndex = 0;
            speakerCombo.SelectedIndexChanged += new EventHandler(this.speakerCombo_SelectedIndexChanged);

            toolStrip.Items.Add(catLabel);
            toolStrip.Items.Add(chatTypeCombo);
            toolStrip.Items.Add(speakerLabel);
            toolStrip.Items.Add(speakerCombo);
        }
        #endregion

        #region IPlugin members
        public override void Reset()
        {
            ResetTextBox();

            flagNoUpdate = true;
            speakerCombo.CBReset();
            speakerCombo.CBAddStrings(new string[1] { Resources.PublicResources.All });

            speakerCombo.CBSelectIndex(0);
        }

        public override void NotifyOfUpdate()
        {
            ResetTextBox();
            UpdateSpeakerList();
            flagNoUpdate = true;
            speakerCombo.CBSelectItem(Resources.PublicResources.All);

            HandleDataset(null);
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            if (e.DatasetChanges.ChatSpeakers != null)
            {
                if (e.DatasetChanges.ChatSpeakers.Count != 0)
                {
                    string currentSelection = speakerCombo.CBSelectedItem();
                    flagNoUpdate = true;
                    UpdateSpeakerList();
                    flagNoUpdate = true;
                    speakerCombo.CBSelectItem(currentSelection);

                    HandleDataset(e.DatasetChanges);
                    return;
                }
            }

            if (e.DatasetChanges.ChatMessages.Count != 0)
            {
                HandleDataset(e.DatasetChanges);
                return;
            }
        }

        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            // Update chat messages list based on filtering
            ChatMessageType chatFilter = (ChatMessageType)chatTypeCombo.CBSelectedIndex();
            string player = speakerCombo.CBSelectedItem();

            var filteredChat = dataSet.ChatMessages.Where(m =>
                chatFilter == ChatMessageType.Unknown || chatFilter == (ChatMessageType)m.ChatType);

            if (player != Resources.PublicResources.All)
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
                sb.AppendFormat("[{0}] ", row.Timestamp.ToLocalTime().ToLongTimeString());

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
        private void UpdateSpeakerList()
        {
            speakerCombo.UpdateWithSpeakerList();
        }

        /// <summary>
        /// Update the drop-down combo box with localized names of the different chat types.
        /// </summary>
        private void PopulateChatTypes()
        {
            if (this.InvokeRequired)
            {
                Action thisFunc = PopulateChatTypes;
                this.Invoke(thisFunc);
                return;
            }

            chatTypeCombo.Items.Clear();

            chatTypeCombo.Items.Add(Resources.PublicResources.All);

            for (var chatType = ChatMessageType.Say; chatType <= ChatMessageType.Arena; chatType++)
            {
                switch (chatType)
                {
                    case ChatMessageType.Arena:
                        chatTypeCombo.Items.Add(Resources.NonCombat.ChatPluginChatTypeArena);
                        break;
                    case ChatMessageType.Echo:
                        chatTypeCombo.Items.Add(Resources.NonCombat.ChatPluginChatTypeEcho);
                        break;
                    case ChatMessageType.Emote:
                        chatTypeCombo.Items.Add(Resources.NonCombat.ChatPluginChatTypeEmote);
                        break;
                    case ChatMessageType.Linkshell:
                        chatTypeCombo.Items.Add(Resources.NonCombat.ChatPluginChatTypeLinkshell);
                        break;
                    case ChatMessageType.NPC:
                        chatTypeCombo.Items.Add(Resources.NonCombat.ChatPluginChatTypeNPC);
                        break;
                    case ChatMessageType.Party:
                        chatTypeCombo.Items.Add(Resources.NonCombat.ChatPluginChatTypeParty);
                        break;
                    case ChatMessageType.Say:
                        chatTypeCombo.Items.Add(Resources.NonCombat.ChatPluginChatTypeSay);
                        break;
                    case ChatMessageType.Shout:
                        chatTypeCombo.Items.Add(Resources.NonCombat.ChatPluginChatTypeShout);
                        break;
                    case ChatMessageType.Tell:
                        chatTypeCombo.Items.Add(Resources.NonCombat.ChatPluginChatTypeTell);
                        break;
                }
            }
        }
        #endregion

        #region Event handlers
        protected void chatTypeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
            {
                ResetTextBox();
                HandleDataset(null);
            }

            flagNoUpdate = false;
        }

        protected void speakerCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
            {
                ResetTextBox();
                HandleDataset(null);
            }

            flagNoUpdate = false;
        }
        #endregion

        #region Localization Overrides
        protected override void LoadLocalizedUI()
        {
            catLabel.Text = Resources.NonCombat.ChatPluginCategoryLabel;
            speakerLabel.Text = Resources.NonCombat.ChatPluginSpeakerLabel;

            int prevChatTypeIndex = chatTypeCombo.SelectedIndex;
            PopulateChatTypes();
            chatTypeCombo.SelectedIndex = prevChatTypeIndex;

            UpdateSpeakerList();
        }

        protected override void LoadResources()
        {
            this.tabName = Resources.NonCombat.ChatPluginTabName;
        }
        #endregion

    }
}
