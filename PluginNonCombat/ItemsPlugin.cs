﻿using System;
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
        #region Constructor
        bool flagNoUpdate = false;
        bool showDetails = false;
        ToolStripComboBox playersCombo = new ToolStripComboBox();
        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();

        string generalHeader = "Item                                  Used\n";
        //string detailsHeader = "Item                 Time Used\n";


        public ItemsPlugin()
        {
            ToolStripLabel playerLabel = new ToolStripLabel();
            playerLabel.Text = "Players:";
            toolStrip.Items.Add(playerLabel);

            playersCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            playersCombo.Items.Add("All");
            playersCombo.MaxDropDownItems = 10;
            playersCombo.SelectedIndex = 0;
            playersCombo.SelectedIndexChanged += new EventHandler(this.playersCombo_SelectedIndexChanged);
            toolStrip.Items.Add(playersCombo);



            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;
            optionsMenu.Text = "Options";

            ToolStripMenuItem showDetailOption = new ToolStripMenuItem();
            showDetailOption.Text = "Show Detail";
            showDetailOption.CheckOnClick = true;
            showDetailOption.Checked = false;
            showDetailOption.Click += new EventHandler(showDetailOption_Click);
            optionsMenu.DropDownItems.Add(showDetailOption);

            toolStrip.Items.Add(optionsMenu);
        }
        #endregion

        #region IPlugin Overrides
        public override string TabName
        {
            get { return "Items"; }
        }

        public override void Reset()
        {
            ResetTextBox();
        }

        public override void NotifyOfUpdate(KPDatabaseDataSet dataSet)
        {
            ResetTextBox();

            UpdatePlayerList(dataSet);

            playersCombo.CBSelectIndex(0);
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
        {
            bool changesFound = false;
            string currentlySelectedPlayer = "All";

            if (playersCombo.CBSelectedIndex() > 0)
                currentlySelectedPlayer = playersCombo.CBSelectedItem();

            if ((e.DatasetChanges.Combatants != null) &&
                (e.DatasetChanges.Combatants.Count > 0))
            {
                UpdatePlayerList(e.Dataset);
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
                datasetToUse = e.Dataset;
                return true;
            }
            else
            {
                datasetToUse = null;
                return false;
            }
        }

        #endregion

        #region Private functions
        private void UpdatePlayerList(KPDatabaseDataSet dataSet)
        {
            playersCombo.CBReset();

            var playersFighting = from b in dataSet.Combatants
                                  where ((b.CombatantType == (byte)EntityType.Player ||
                                         b.CombatantType == (byte)EntityType.Pet ||
                                         b.CombatantType == (byte)EntityType.Fellow) &&
                                         b.GetInteractionsRowsByActorCombatantRelation().Any() == true)
                                  orderby b.CombatantName
                                  select new
                                  {
                                      Name = b.CombatantName
                                  };

            List<string> playerStrings = new List<string>();
            playerStrings.Add("All");

            foreach (var player in playersFighting)
                playerStrings.Add(player.Name);

            playersCombo.CBAddStrings(playerStrings.ToArray());
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

            if (selectedPlayer == "All")
            {
                foreach (string player in playersCombo.CBGetStrings())
                {
                    if (player != "All")
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
                                Name = c.CombatantName,
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
                if (player.Items.Count() > 0)
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
                    sb.Append(generalHeader);


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
                                    itemEntry.Timestamp.ToShortTimeString());
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
                HandleDataset(DatabaseManager.Instance.Database);

            flagNoUpdate = false;
        }

        protected void showDetailOption_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            showDetails = sentBy.Checked;

            if (flagNoUpdate == false)
                HandleDataset(DatabaseManager.Instance.Database);

            flagNoUpdate = false;
        }
        #endregion
    }
}
