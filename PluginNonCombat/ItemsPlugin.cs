using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using WaywardGamers.KParser;
using System.Windows.Forms;

namespace WaywardGamers.KParser.Plugin
{
    public class ItemsPlugin : BasePluginControl
    {
        #region Member Variables
        bool flagNoUpdate = false;
        bool showDetails = false;
        ToolStripComboBox playersCombo = new ToolStripComboBox();
        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();
        ToolStripLabel playerLabel = new ToolStripLabel();
        ToolStripMenuItem showDetailOption = new ToolStripMenuItem();

        string generalHeader;
        #endregion

        #region Constructor
        public ItemsPlugin()
        {
            LoadLocalizedUI();

            playersCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            playersCombo.MaxDropDownItems = 10;
            playersCombo.SelectedIndex = 0;
            playersCombo.SelectedIndexChanged += new EventHandler(this.playersCombo_SelectedIndexChanged);

            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;
            
            showDetailOption.CheckOnClick = true;
            showDetailOption.Checked = false;
            showDetailOption.Click += new EventHandler(showDetailOption_Click);
            optionsMenu.DropDownItems.Add(showDetailOption);

            toolStrip.Items.Add(playerLabel);
            toolStrip.Items.Add(playersCombo);
            toolStrip.Items.Add(optionsMenu);
        }
        #endregion

        #region IPlugin Overrides
        public override void Reset()
        {
            ResetTextBox();
        }

        public override void NotifyOfUpdate()
        {
            ResetTextBox();

            UpdatePlayerList();

            playersCombo.CBSelectIndex(0);
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            bool changesFound = false;
            string currentlySelectedPlayer = Resources.PublicResources.All;

            if (playersCombo.CBSelectedIndex() > 0)
                currentlySelectedPlayer = playersCombo.CBSelectedItem();

            if ((e.DatasetChanges.Combatants != null) &&
                (e.DatasetChanges.Combatants.Count > 0))
            {
                UpdatePlayerList();
                changesFound = true;

                flagNoUpdate = true;
                playersCombo.CBSelectItem(currentlySelectedPlayer);
            }

            if ((e.DatasetChanges.Items != null) &&
                (e.DatasetChanges.Items.Count > 0))
            {
                changesFound = true;
            }


            if (changesFound == true)
            {
                HandleDataset(null);
            }
        }
        #endregion

        #region Private functions
        private void UpdatePlayerList()
        {
            playersCombo.CBReset();
            playersCombo.CBAddStrings(GetPlayerListing());
        }
        #endregion

        #region Processing Functions
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            // If we get here during initialization, skip.
            if (playersCombo.Items.Count == 0)
                return;

            ResetTextBox();

            List<StringMods> strModList = new List<StringMods>();
            StringBuilder sb = new StringBuilder();

            #region Filter
            string selectedPlayer = playersCombo.CBSelectedItem();
            List<string> playerList = new List<string>();

            if (selectedPlayer == Resources.PublicResources.All)
            {
                foreach (string player in playersCombo.CBGetStrings())
                {
                    if (player != Resources.PublicResources.All)
                        playerList.Add(player.ToString());
                }
            }
            else
            {
                playerList.Add(selectedPlayer);
            }

            if (playerList.Count == 0)
                return;

            #endregion

            #region LINQ

            var itemUsage = from c in dataSet.Combatants
                            where (playerList.Contains(c.CombatantName) &&
                                   (EntityType)c.CombatantType == EntityType.Player)
                            orderby c.CombatantName
                            select new
                            {
                                Name = c.CombatantNameOrJobName,
                                Items = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where n.IsItemIDNull() == false &&
                                              (AidType)n.AidType == AidType.Item
                                        orderby n.ItemsRow.ItemName, n.Timestamp
                                        group n by n.ItemsRow.ItemName
                            };

            #endregion

            if (itemUsage.Count() == 0)
                return;

            if (itemUsage.Sum(a => a.Items.Count()) == 0)
                return;


            foreach (var player in itemUsage)
            {
                if (player.Items.Any())
                {
                    strModList.Add(new StringMods
                    {
                        Start = sb.Length,
                        Length = player.Name.Length,
                        Bold = true,
                        Color = Color.Blue
                    });
                    sb.Append(player.Name);
                    sb.Append("\n");

                    strModList.Add(new StringMods
                    {
                        Start = sb.Length,
                        Length = generalHeader.Length,
                        Bold = true,
                        Underline = true,
                        Color = Color.Black
                    });
                    sb.Append(generalHeader + "\n");


                    foreach (var item in player.Items)
                    {
                        sb.AppendFormat("{0,-32}{1,10}\n",
                            item.Key,
                            item.Count());

                        if (showDetails == true)
                        {
                            foreach (var itemEntry in item)
                            {
                                sb.AppendFormat("{0,-32}{1,10}\n",
                                    string.Empty,
                                    itemEntry.Timestamp.ToLocalTime().ToShortTimeString());
                            }
                        }
                    }

                    sb.Append("\n");
                }
            }

            PushStrings(sb, strModList);
        }
        #endregion

        #region Event Handlers
        protected void playersCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(null);

            flagNoUpdate = false;
        }

        protected void showDetailOption_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            showDetails = sentBy.Checked;

            if (flagNoUpdate == false)
                HandleDataset(null);

            flagNoUpdate = false;
        }
        #endregion

        #region Localization Overrides
        protected override void LoadLocalizedUI()
        {
            playerLabel.Text = Resources.NonCombat.ItemsPluginPlayerLabel;
            optionsMenu.Text = Resources.PublicResources.Options;
            showDetailOption.Text = Resources.NonCombat.ItemsPluginShowDetail;

            UpdatePlayerList();
        }

        protected override void LoadResources()
        {
            this.tabName = Resources.NonCombat.ItemsPluginTabName;
            generalHeader = Resources.NonCombat.ItemsPluginGeneralHeader;
        }
        #endregion

    }
}
