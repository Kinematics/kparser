using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using WaywardGamers.KParser;
using WaywardGamers.KParser.Database;

namespace WaywardGamers.KParser.Plugin
{
    public class DeathPlugin : BasePluginControl
    {
        #region Member Variables

        bool groupMobs = true;
        bool exclude0XPMobs = false;
        bool flagNoUpdate = false;
        bool customMobSelection = false;

        // UI controls
        ToolStripLabel playersLabel = new ToolStripLabel();
        ToolStripComboBox playersCombo = new ToolStripComboBox();

        ToolStripLabel mobsLabel = new ToolStripLabel();
        ToolStripComboBox mobsCombo = new ToolStripComboBox();

        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();
        ToolStripMenuItem groupMobsOption = new ToolStripMenuItem();
        ToolStripMenuItem exclude0XPOption = new ToolStripMenuItem();
        ToolStripMenuItem customMobSelectionOption = new ToolStripMenuItem();

        ToolStripButton editCustomMobFilter = new ToolStripButton();


        // Localized strings
        string lsAll;

        string lsTitle;

        string lsSummaryTitle;
        string lsDetailsTitle;

        string lsSummaryHeader;
        string lsSummaryFormat;
        string lsDetailsHeader;
        string lsDetailsFormat;

        string lsUnknown;
        #endregion

        #region Constructor
        public DeathPlugin()
        {
            LoadLocalizedUI();

            playersCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            playersCombo.MaxDropDownItems = 10;
            playersCombo.SelectedIndexChanged += new EventHandler(this.playersCombo_SelectedIndexChanged);

            mobsCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            mobsCombo.AutoSize = false;
            mobsCombo.Width = 175;
            mobsCombo.MaxDropDownItems = 10;
            mobsCombo.SelectedIndexChanged += new EventHandler(this.mobsCombo_SelectedIndexChanged);


            groupMobsOption.CheckOnClick = true;
            groupMobsOption.Checked = true;
            groupMobsOption.Click += new EventHandler(groupMobs_Click);

            exclude0XPOption.CheckOnClick = true;
            exclude0XPOption.Checked = false;
            exclude0XPOption.Click += new EventHandler(exclude0XPMobs_Click);

            customMobSelectionOption.CheckOnClick = true;
            customMobSelectionOption.Checked = false;
            customMobSelectionOption.Click += new EventHandler(customMobSelection_Click);


            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;
            optionsMenu.DropDownItems.Add(groupMobsOption);
            optionsMenu.DropDownItems.Add(exclude0XPOption);
            optionsMenu.DropDownItems.Add(customMobSelectionOption);


            editCustomMobFilter.Enabled = false;
            editCustomMobFilter.Click += new EventHandler(editCustomMobFilter_Click);


            ToolStripSeparator aSeparator = new ToolStripSeparator();

            toolStrip.Items.Add(playersLabel);
            toolStrip.Items.Add(playersCombo);
            toolStrip.Items.Add(mobsLabel);
            toolStrip.Items.Add(mobsCombo);
            toolStrip.Items.Add(optionsMenu);
            toolStrip.Items.Add(aSeparator);
            toolStrip.Items.Add(editCustomMobFilter);
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
            UpdateMobList(false);

            try
            {
                // Don't generate an update on the first combo box change
                flagNoUpdate = true;
                playersCombo.CBSelectIndex(0);

                // Setting the second combo box will cause the display to load.
                mobsCombo.CBSelectIndex(0);

            }
            finally
            {
                flagNoUpdate = false;
            }

            HandleDataset(null);
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            bool changesFound = false;
            string currentlySelectedPlayer = lsAll;

            // Check for new mobs being fought.  If any exist, update the Mob Group dropdown list.
            if (e.DatasetChanges.Battles != null)
            {
                if (e.DatasetChanges.Battles.Any(x => x.RowState == DataRowState.Added))
                {
                    changesFound = true;
                }
            }

            if (e.DatasetChanges.Interactions != null)
            {
                if (e.DatasetChanges.Interactions.Any(x => x.RowState == DataRowState.Added))
                {
                    changesFound = true;
                }
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

        private void UpdateMobList()
        {
            UpdateMobList(false);
        }

        private void UpdateMobList(bool overrideGrouping)
        {
            if (overrideGrouping == true)
                mobsCombo.UpdateWithMobList(false, exclude0XPMobs);
            else
                mobsCombo.UpdateWithMobList(groupMobs, exclude0XPMobs);
        }
        #endregion

        #region Processing and Display functions
        /// <summary>
        /// General branching for processing data
        /// </summary>
        /// <param name="dataSet"></param>
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            if (dataSet == null)
                return;

            // If we get here during initialization, skip.
            if (playersCombo.Items.Count == 0)
                return;

            if (mobsCombo.Items.Count == 0)
                return;

            ResetTextBox();

            string selectedPlayer = playersCombo.CBSelectedItem();

            List<string> playerList = new List<string>();

            if (selectedPlayer == lsAll)
            {
                foreach (var player in playersCombo.CBGetStrings())
                {
                    if (player != lsAll)
                        playerList.Add(player.ToString());
                }
            }
            else
            {
                playerList.Add(selectedPlayer);
            }

            if (playerList.Count == 0)
                return;

            string[] selectedPlayers = playerList.ToArray();


            MobFilter mobFilter;
            if (customMobSelection)
                mobFilter = MobXPHandler.Instance.CustomMobFilter;
            else
                mobFilter = mobsCombo.CBGetMobFilter(exclude0XPMobs);


            IEnumerable<AttackGroup> attackSet;

            if (mobFilter.AllMobs == false)
            {
                // For single or grouped mobs

                // If we have any mob filter subset, get that data starting
                // with the battle table and working outwards.  Significantly
                // faster (eg: 5-25 ms instead of 400 ms on a 200 mob parse).

                var bSet = from b in dataSet.Battles
                           where (mobFilter.CheckFilterBattle(b) == true)
                           orderby b.BattleID
                           select b.GetInteractionsRows();

                if (bSet.Count() == 0)
                    return;


                IEnumerable<KPDatabaseDataSet.InteractionsRow> iRows = bSet.First();

                var bSetSkip = bSet.Skip(1);

                foreach (var b in bSetSkip)
                {
                    iRows = iRows.Concat(b);
                }

                if (iRows.Any())
                {
                    DateTime initialTime = iRows.First().Timestamp - TimeSpan.FromSeconds(70);
                    DateTime endTime = iRows.Last().Timestamp;

                    var dSet = dataSet.Battles.GetDefaultBattle().GetInteractionsRows()
                        .Where(i => i.Timestamp >= initialTime && i.Timestamp <= endTime);

                    iRows = iRows.Concat(dSet);
                }

                attackSet = from c in iRows
                            where (c.IsTargetIDNull() == false) &&
                                  (selectedPlayers.Contains(c.CombatantsRowByTargetCombatantRelation.CombatantName))
                            group c by c.CombatantsRowByTargetCombatantRelation into ca
                            orderby ca.Key.CombatantType, ca.Key.CombatantName
                            select new AttackGroup
                            {
                                Name = ca.Key.CombatantNameOrJobName,
                                ComType = (EntityType)ca.Key.CombatantType,
                                Death = from q in ca
                                        where ((ActionType)q.ActionType == ActionType.Death)
                                        orderby q.Timestamp
                                        select q,
                            };
            }
            else
            {
                // For all mobs

                attackSet = from c in dataSet.Combatants
                            where (selectedPlayers.Contains(c.CombatantName))
                            select new AttackGroup
                            {
                                Name = c.CombatantNameOrJobName,
                                ComType = (EntityType)c.CombatantType,
                                Death = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Death)
                                        orderby n.Timestamp
                                        select n,
                            };
            }


            List<StringMods> strModList = new List<StringMods>();
            StringBuilder sb = new StringBuilder();

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsTitle.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(lsTitle + "\n\n");


            ProcessSummary(attackSet, ref sb, ref strModList);
            ProcessDetails(attackSet, ref sb, ref strModList);

            PushStrings(sb, strModList);
        }

        /// <summary>
        /// Summary output
        /// </summary>
        /// <param name="attackSet"></param>
        /// <param name="sb"></param>
        /// <param name="strModList"></param>
        private void ProcessSummary(IEnumerable<AttackGroup> attackSet,
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            if (attackSet.Count() == 0)
                return;

            if (attackSet.Sum(a => a.Death.Count()) == 0)
                return;

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsSummaryTitle.Length,
                Bold = true,
                Color = Color.Blue
            });
            sb.Append(lsSummaryTitle + "\n\n");

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsSummaryHeader.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(lsSummaryHeader + "\n");

            // Per-player processing
            foreach (var player in attackSet.OrderBy(a => a.Name))
            {
                // Get a weaponskill count
                int deathCount = player.Death.Count();

                if (deathCount == 0)
                    continue;

                sb.AppendFormat(lsSummaryFormat,
                        player.Name,
                        deathCount);
                sb.Append("\n");
            }

            sb.Append("\n\n");
        }

        /// <summary>
        /// Details output
        /// </summary>
        /// <param name="attackSet"></param>
        /// <param name="sb"></param>
        /// <param name="strModList"></param>
        private void ProcessDetails(IEnumerable<AttackGroup> attackSet, ref StringBuilder sb, ref List<StringMods> strModList)
        {
            if (attackSet.Count() == 0)
                return;

            if (attackSet.Sum(a => a.Death.Count()) == 0)
                return;

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsDetailsTitle.Length,
                Bold = true,
                Color = Color.Blue
            });
            sb.Append(lsDetailsTitle + "\n\n");

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsDetailsHeader.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(lsDetailsHeader + "\n");


            var allDeaths = from d in attackSet
                            from dd in d.Death
                            orderby dd.Timestamp
                            select dd;

            // Per-player processing
            foreach (var death in allDeaths)
            {
                if (death.IsTargetIDNull() == false)
                {
                    string player = death.CombatantsRowByTargetCombatantRelation.CombatantNameOrJobName;
                    string timeOfDeath = death.Timestamp.ToLocalTime().ToShortTimeString();
                    string killer = death.IsActorIDNull() ? lsUnknown : death.CombatantsRowByActorCombatantRelation.CombatantName;


                    sb.AppendFormat(lsDetailsFormat,
                            player,
                            timeOfDeath,
                            killer);
                    sb.Append("\n");
                }
            }
        }

        #endregion

        #region Event Handlers
        protected void playersCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(null);

            flagNoUpdate = false;
        }

        protected void mobsCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(null);

            flagNoUpdate = false;
        }

        void groupMobs_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            groupMobs = sentBy.Checked;

            if (flagNoUpdate == false)
            {
                flagNoUpdate = true;
                UpdateMobList();

                HandleDataset(null);
            }

            flagNoUpdate = false;
        }

        protected void exclude0XPMobs_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            exclude0XPMobs = sentBy.Checked;

            if (flagNoUpdate == false)
            {
                flagNoUpdate = true;
                UpdateMobList();

                HandleDataset(null);
            }

            flagNoUpdate = false;
        }

        protected void customMobSelection_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            customMobSelection = sentBy.Checked;

            mobsCombo.Enabled = !customMobSelection;
            groupMobsOption.Enabled = !customMobSelection;
            exclude0XPOption.Enabled = !customMobSelection;

            editCustomMobFilter.Enabled = customMobSelection;

            if (flagNoUpdate == false)
            {
                HandleDataset(null);
            }

            flagNoUpdate = false;
        }

        protected void editCustomMobFilter_Click(object sender, EventArgs e)
        {
            MobXPHandler.Instance.ShowCustomMobFilter();
        }

        protected override void OnCustomMobFilterChanged()
        {
            HandleDataset(null);
        }

        #endregion

        #region Localization Overrides
        protected override void LoadLocalizedUI()
        {
            playersLabel.Text = Resources.PublicResources.PlayersLabel;
            mobsLabel.Text = Resources.PublicResources.MobsLabel;

            optionsMenu.Text = Resources.PublicResources.Options;
            groupMobsOption.Text = Resources.PublicResources.GroupMobs;
            exclude0XPOption.Text = Resources.PublicResources.Exclude0XPMobs;
            customMobSelectionOption.Text = Resources.PublicResources.CustomMobSelection;
            editCustomMobFilter.Text = Resources.PublicResources.EditMobFilter;

            UpdatePlayerList();
            playersCombo.SelectedIndex = 0;

            UpdateMobList();
            mobsCombo.SelectedIndex = 0;
        }

        protected override void LoadResources()
        {
            this.tabName = Resources.Combat.DeathPluginTabName;

            lsAll = Resources.PublicResources.All;

            lsTitle = Resources.Combat.DeathPluginTitle;
            lsSummaryTitle = Resources.Combat.DeathPluginSummaryTitle;
            lsDetailsTitle = Resources.Combat.DeathPluginDetailsTitle;

            lsSummaryHeader = Resources.Combat.DeathPluginSummaryHeader;
            lsSummaryFormat = Resources.Combat.DeathPluginSummaryFormat;
            lsDetailsHeader = Resources.Combat.DeathPluginDetailsHeader;
            lsDetailsFormat = Resources.Combat.DeathPluginDetailsFormat;

            lsUnknown = Resources.Combat.DeathPluginUnknown;
        }
        #endregion

    }
}
