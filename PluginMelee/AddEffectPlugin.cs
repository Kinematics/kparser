using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WaywardGamers.KParser;
using WaywardGamers.KParser.Database;

namespace WaywardGamers.KParser.Plugin
{
    public class AdditionalEffect : BasePluginControl
    {
        #region Member Variables

        ToolStripLabel playersLabel = new ToolStripLabel();
        ToolStripComboBox playersCombo = new ToolStripComboBox();
        ToolStripLabel mobsLabel = new ToolStripLabel();
        ToolStripComboBox mobsCombo = new ToolStripComboBox();
        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();

        ToolStripMenuItem groupMobsOption = new ToolStripMenuItem();
        ToolStripMenuItem exclude0XPOption = new ToolStripMenuItem();
        ToolStripMenuItem customMobSelectionOption = new ToolStripMenuItem();

        ToolStripButton editCustomMobFilter = new ToolStripButton();

        bool flagNoUpdate;
        bool groupMobs = true;
        bool exclude0XPMobs = false;
        bool customMobSelection = false;

        // Localized strings

        string lsAll;
        string lsNone;
        string lsTotal;

        #endregion

        #region Constructor
        public AdditionalEffect()
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

            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;

            groupMobsOption.CheckOnClick = true;
            groupMobsOption.Checked = true;
            groupMobsOption.Click += new EventHandler(groupMobs_Click);

            exclude0XPOption.CheckOnClick = true;
            exclude0XPOption.Checked = false;
            exclude0XPOption.Click += new EventHandler(exclude0XPMobs_Click);

            customMobSelectionOption.CheckOnClick = true;
            customMobSelectionOption.Checked = false;
            customMobSelectionOption.Click += new EventHandler(customMobSelection_Click);

            optionsMenu.DropDownItems.Add(groupMobsOption);
            optionsMenu.DropDownItems.Add(exclude0XPOption);
            optionsMenu.DropDownItems.Add(customMobSelectionOption);

            editCustomMobFilter.Enabled = false;
            editCustomMobFilter.Click += new EventHandler(editCustomMobFilter_Click);


            toolStrip.Items.Add(playersLabel);
            toolStrip.Items.Add(playersCombo);
            toolStrip.Items.Add(mobsLabel);
            toolStrip.Items.Add(mobsCombo);
            toolStrip.Items.Add(optionsMenu);
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
            UpdatePlayerList();
            UpdateMobList();

            DisableOptions(Monitoring.Monitor.Instance.IsRunning);

            flagNoUpdate = true;
            mobsCombo.CBSelectIndex(0);

            flagNoUpdate = true;
            playersCombo.CBSelectIndex(0);

            HandleDataset(null);
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            // Parse is running.
            DisableOptions(true);

            if ((e.DatasetChanges.Combatants != null) &&
                (e.DatasetChanges.Combatants.Any(c => c.RowState == DataRowState.Added)))
            {
                UpdatePlayerList();

                if (playersCombo.CBSelectedIndex() < 0)
                {
                    flagNoUpdate = true;
                    playersCombo.CBSelectIndex(0);
                }
            }

            // Check for new mobs being fought.  If any exist, update the Mob Group dropdown list.
            if (e.DatasetChanges.Battles != null)
            {
                if (e.DatasetChanges.Battles.Any(b => b.RowState == DataRowState.Added))
                {
                    string currentSelection = mobsCombo.CBSelectedItem();

                    UpdateMobList();

                    if (groupMobs == false)
                    {
                        mobsCombo.CBSelectIndex(-1);
                    }
                    else
                    {
                        // Selected index will only get reset to -1 if the mob list changed.
                        if (mobsCombo.CBSelectedIndex() < 0)
                            mobsCombo.CBSelectItem(currentSelection);
                    }
                }
            }

            if (e.DatasetChanges.Interactions.Count != 0)
            {
                HandleDataset(null);
            }
        }
        #endregion

        #region Private Methods
        private void UpdatePlayerList()
        {
            playersCombo.CBReset();
            playersCombo.CBAddStrings(GetPlayerListing());
        }

        private void UpdateLockedMobList()
        {
            mobsCombo.UpdateWithMobList(false, false);
        }

        private void UpdateMobList()
        {
            mobsCombo.UpdateWithMobList(groupMobs, exclude0XPMobs);
        }
        #endregion

        #region Processing sections
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            // If we get here during initialization, skip.
            if (playersCombo.Items.Count == 0)
                return;

            if (mobsCombo.Items.Count == 0)
                return;

            if (dataSet == null)
                return;

            ResetTextBox();

            MobFilter mobFilter;
            if (customMobSelection)
                mobFilter = MobXPHandler.Instance.CustomMobFilter;
            else
                mobFilter = mobsCombo.CBGetMobFilter(exclude0XPMobs);


            string selectedPlayer = playersCombo.CBSelectedItem();

            List<string> playerList = new List<string>();

            if (selectedPlayer == lsAll)
            {
                foreach (string player in playersCombo.CBGetStrings())
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

            int mobCount = mobFilter.Count;

            if (mobCount == 0)
                return;

            #region LINQ
            var attackSet = from c in dataSet.Combatants
                            where (playerList.Contains(c.CombatantName) &&
                                   RegexUtility.ExcludedPlayer.Match(c.PlayerInfo).Success == false)
                            orderby c.CombatantType, c.CombatantName
                            select new AttackGroup
                            {
                                Name = c.CombatantName,
                                DisplayName = c.CombatantNameOrJobName,
                                Melee = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Melee &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain)) &&
                                               (DefenseType)n.DefenseType == DefenseType.None &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                        select n,
                                Range = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Ranged &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain)) &&
                                               (DefenseType)n.DefenseType == DefenseType.None &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                        select n,
                            };

            #endregion

            ProcessSetOfMobs(dataSet, attackSet, mobFilter);
        }

        private void ProcessSetOfMobs(KPDatabaseDataSet dataSet, EnumerableRowCollection<AttackGroup> attackSet,
            MobFilter mobFilter)
        {
            StringBuilder sb = new StringBuilder();
            List<StringMods> strModList = new List<StringMods>();

            string meleeHeader = "Melee Effect           # Procs     # Hits     Raw Proc %    # Restricted Hits   Restricted Proc %";
            string rangeHeader = "Range Effect           # Procs     # Hits     Raw Proc %";


            string tmp = "Additional Effect Status Inflictions";
            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = tmp.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(tmp + "\n\n");


            foreach (var player in attackSet)
            {
                if ((player.Melee.Count() == 0) && (player.Range.Count() == 0))
                    continue;

                var meleeByAE = player.Melee.Where(a =>
                    a.IsSecondActionIDNull() == false &&
                    (HarmType)a.SecondHarmType == HarmType.Enfeeble);

                var rangeByAE = player.Range.Where(a =>
                    a.IsSecondActionIDNull() == false &&
                    (HarmType)a.SecondHarmType == HarmType.Enfeeble);

                if ((meleeByAE.Count() == 0) && (rangeByAE.Count() == 0))
                    continue;

                // Ok, this player has generated some AE effects.

                tmp = string.Format("{0}", player.DisplayName);
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmp.Length,
                    Bold = true,
                    Color = Color.Blue
                });
                sb.Append(tmp + "\n\n");

                int meleeCount = player.Melee.Count();

                if (meleeCount > 0)
                {
                    if (meleeByAE.Count() > 0)
                    {
                        strModList.Add(new StringMods
                        {
                            Start = sb.Length,
                            Length = meleeHeader.Length,
                            Bold = true,
                            Underline = true,
                            Color = Color.Black
                        });
                        sb.Append(meleeHeader + "\n");

                        var groupMeleeByAE = meleeByAE.GroupBy(a => a.ActionsRowBySecondaryActionNameRelation.ActionName);

                        foreach (var ae in groupMeleeByAE)
                        {
                            var meleeAfterAE = player.Melee.Where(m =>
                                ae.Any(a => m.Timestamp >= a.Timestamp && m.Timestamp <= a.Timestamp.AddSeconds(30)));

                            int countMeleeAfterAE = meleeAfterAE.Count();
                            int outsideMelee = meleeCount - countMeleeAfterAE;

                            sb.AppendFormat("{0,-20}{1,10}{2,11}{3,15:p2}{4,21}{5,20:p2}\n",
                                ae.Key,
                                ae.Count(),
                                meleeCount,
                                (double)ae.Count() / meleeCount,
                                outsideMelee,
                                (outsideMelee > 0) ? (double)ae.Count() / outsideMelee : 0
                                );
                        }
                    }
                }

                if (player.Range.Count() > 0)
                {

                    if (rangeByAE.Count() > 0)
                    {
                        strModList.Add(new StringMods
                        {
                            Start = sb.Length,
                            Length = rangeHeader.Length,
                            Bold = true,
                            Underline = true,
                            Color = Color.Black
                        });
                        sb.Append(rangeHeader + "\n");

                        var groupRangeByAE = rangeByAE.GroupBy(a => a.ActionsRowBySecondaryActionNameRelation.ActionName);

                        foreach (var ae in groupRangeByAE)
                        {
                            sb.AppendFormat("{0,-20}{1,10}{2,11}{3,15:p2}\n",
                                ae.Key,
                                ae.Count(),
                                player.Range.Count(),
                                (double)ae.Count() / player.Range.Count()
                                );
                        }
                    }
                }

                sb.Append("\n");
            }
            
            PushStrings(sb, strModList);
        }
        #endregion

        #region Event Handlers
        private void DisableOptions(bool isParseRunning)
        {
            if (this.InvokeRequired)
            {
                Action<bool> disableOptions = new Action<bool>(DisableOptions);
                object[] boolParam = new object[1] { isParseRunning };

                Invoke(disableOptions, boolParam);
                return;
            }

            if (isParseRunning)
            {
                optionsMenu.Enabled = false;
                editCustomMobFilter.Enabled = false;
            }
            else
            {
                optionsMenu.Enabled = true;
                editCustomMobFilter.Enabled = customMobSelection;
            }
        }

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

        protected void exclude0XPMobs_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            exclude0XPMobs = sentBy.Checked;

            if (flagNoUpdate == false)
            {
                try
                {
                    UpdateMobList();
                    flagNoUpdate = true;
                    mobsCombo.CBSelectIndex(0);
                    HandleDataset(null);
                }
                catch (Exception ex)
                {
                    Logger.Instance.Log(ex);
                }
            }

            flagNoUpdate = false;
        }

        protected void groupMobs_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            groupMobs = sentBy.Checked;

            if (flagNoUpdate == false)
            {
                try
                {
                    UpdateMobList();
                    flagNoUpdate = true;
                    mobsCombo.CBSelectIndex(0);

                }
                catch (Exception ex)
                {
                    Logger.Instance.Log(ex);
                }
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
                try
                {
                    HandleDataset(null);
                }
                catch (Exception ex)
                {
                    Logger.Instance.Log(ex);
                }
            }

            flagNoUpdate = false;
        }

        protected void editCustomMobFilter_Click(object sender, EventArgs e)
        {
            MobXPHandler.Instance.ShowCustomMobFilter();
        }

        protected override void OnCustomMobFilterChanged()
        {
            try
            {
                HandleDataset(null);
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
        }
        #endregion

        #region Localization Overrides
        protected override void LoadLocalizedUI()
        {
            playersLabel.Text = Resources.PublicResources.PlayersLabel;
            mobsLabel.Text = Resources.PublicResources.MobsLabel;

            optionsMenu.Text = Resources.PublicResources.Options;

            UpdatePlayerList();
            playersCombo.CBSelectIndex(0);

            UpdateMobList();
            //if (mobsCombo.Items[0] == Resources.PublicResources.All)
            //    mobsCombo.Items[0] = Resources.PublicResources.None;

            mobsCombo.CBSelectIndex(0);

            optionsMenu.Text = Resources.PublicResources.Options;
            groupMobsOption.Text = Resources.PublicResources.GroupMobs;
            exclude0XPOption.Text = Resources.PublicResources.Exclude0XPMobs;
            customMobSelectionOption.Text = Resources.PublicResources.CustomMobSelection;
            editCustomMobFilter.Text = Resources.PublicResources.EditMobFilter;

        }

        protected override void LoadResources()
        {
            this.tabName = "Add'l Effect";

            lsAll = Resources.PublicResources.All;
            lsNone = Resources.PublicResources.None;
            lsTotal = Resources.PublicResources.Total;

        }
        #endregion

    }
}
