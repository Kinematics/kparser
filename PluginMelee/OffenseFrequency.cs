using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using WaywardGamers.KParser;
using System.Windows.Forms;
using WaywardGamers.KParser.Database;

namespace WaywardGamers.KParser.Plugin
{
    public class OffenseFrequencyDataPlugin : BasePluginControl
    {
        #region Member Variables
        bool flagNoUpdate;
        bool groupMobs = false;
        bool exclude0XPMobs = false;
        bool customMobSelection = false;
        bool showDetails = false;

        ToolStripLabel playersLabel = new ToolStripLabel();
        ToolStripComboBox playersCombo = new ToolStripComboBox();
        ToolStripLabel mobsLabel = new ToolStripLabel();
        ToolStripComboBox mobsCombo = new ToolStripComboBox();

        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();
        ToolStripMenuItem groupMobsOption = new ToolStripMenuItem();
        ToolStripMenuItem exclude0XPOption = new ToolStripMenuItem();
        ToolStripMenuItem customMobSelectionOption = new ToolStripMenuItem();
        ToolStripMenuItem showDetailOption = new ToolStripMenuItem();

        ToolStripButton editCustomMobFilter = new ToolStripButton();

        // Localized strings

        string lsAll;

        string lsMelee;
        string lsMeleeCrits;
        string lsMeleeAE;
        string lsRanged;
        string lsRangedCrits;
        string lsRangedAE;
        string lsSpells;
        string lsMagicBursts;
        string lsAbility;
        string lsWeaponskills;
        string lsSpikes;

        #endregion

        #region Constructor
        public OffenseFrequencyDataPlugin()
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
            groupMobsOption.Checked = false;
            groupMobsOption.Click += new EventHandler(groupMobs_Click);

            exclude0XPOption.CheckOnClick = true;
            exclude0XPOption.Checked = false;
            exclude0XPOption.Click += new EventHandler(exclude0XPMobs_Click);

            customMobSelectionOption.CheckOnClick = true;
            customMobSelectionOption.Checked = false;
            customMobSelectionOption.Click += new EventHandler(customMobSelection_Click);

            showDetailOption.CheckOnClick = true;
            showDetailOption.Checked = false;
            showDetailOption.Click += new EventHandler(showDetailOption_Click);


            ToolStripSeparator bSeparator = new ToolStripSeparator();

            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;
            optionsMenu.DropDownOpening += new EventHandler(optionsMenu_DropDownOpening);
            optionsMenu.DropDownItems.Add(groupMobsOption);
            optionsMenu.DropDownItems.Add(exclude0XPOption);
            optionsMenu.DropDownItems.Add(customMobSelectionOption);
            optionsMenu.DropDownItems.Add(bSeparator);
            optionsMenu.DropDownItems.Add(showDetailOption);


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

            // Don't generate an update on the first combo box change
            flagNoUpdate = true;
            playersCombo.CBSelectIndex(0);

            // Setting the second combo box will cause the display to load.
            mobsCombo.CBSelectIndex(0);
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            bool changesFound = false;
            string currentlySelectedPlayer = lsAll;

            if (playersCombo.CBSelectedIndex() > 0)
                currentlySelectedPlayer = playersCombo.CBSelectedItem();

            if ((e.DatasetChanges.Combatants != null) &&
                (e.DatasetChanges.Combatants.Any(c => c.RowState == DataRowState.Added)))
            {
                UpdatePlayerList();
                changesFound = true;

                flagNoUpdate = true;
                playersCombo.CBSelectIndex(0);
            }

            // Check for new mobs being fought.  If any exist, update the Mob Group dropdown list.
            if ((e.DatasetChanges.Battles != null) &&
                (e.DatasetChanges.Battles.Any(
                 b => b.RowState == DataRowState.Added || b.RowState == DataRowState.Modified)))
            {
                UpdateMobList(true);
                changesFound = true;

                flagNoUpdate = true;
                mobsCombo.CBSelectIndex(-1);
            }

            if (currentlySelectedPlayer != playersCombo.CBSelectedItem())
            {
                flagNoUpdate = true;
                playersCombo.CBSelectItem(currentlySelectedPlayer);
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
            // If we get here during initialization, skip.
            if (playersCombo.Items.Count == 0)
                return;

            if (mobsCombo.Items.Count == 0)
                return;


            string selectedPlayer = playersCombo.CBSelectedItem();

            MobFilter mobFilter;
            if ((customMobSelection == true) && (Monitoring.Monitor.Instance.IsRunning == false))
                mobFilter = MobXPHandler.Instance.CustomMobFilter;
            else
                mobFilter = mobsCombo.CBGetMobFilter();

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

            ProcessFilteredMobs(dataSet, playerList.ToArray(), mobFilter);
        }

        private void ProcessFilteredMobs(KPDatabaseDataSet dataSet, string[] selectedPlayers, MobFilter mobFilter)
        {
            var attackSet = from c in dataSet.Combatants
                            where (selectedPlayers.Contains(c.CombatantName))
                            orderby c.CombatantType, c.CombatantName
                            select new AttackGroup
                            {
                                Name = c.CombatantNameOrJobName,
                                Melee = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Melee &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                        select n,
                                Range = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Ranged &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                        select n,
                                Spell = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Spell &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                                n.Preparing == false &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                        select n,
                                Ability = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                          where ((ActionType)n.ActionType == ActionType.Ability &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain ||
                                                (HarmType)n.HarmType == HarmType.Unknown) &&
                                                n.Preparing == false &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                          select n,
                                WSkill = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where ((ActionType)n.ActionType == ActionType.Weaponskill &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                                n.Preparing == false &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                         select n,
                                SC = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                     where ((ActionType)n.ActionType == ActionType.Skillchain &&
                                            ((HarmType)n.HarmType == HarmType.Damage ||
                                             (HarmType)n.HarmType == HarmType.Drain) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                     select n,
                                Counter = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                          where (ActionType)n.ActionType == ActionType.Counterattack &&
                                               ((DefenseType)n.DefenseType == DefenseType.None) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                          select n,
                                Retaliate = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                            where (ActionType)n.ActionType == ActionType.Retaliation &&
                                               ((DefenseType)n.DefenseType == DefenseType.None) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                            select n,
                                Spikes = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where (ActionType)n.ActionType == ActionType.Spikes &&
                                               ((DefenseType)n.DefenseType == DefenseType.None) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                         select n
                            };

            ProcessAttackSet(attackSet);
        }

        /// <summary>
        /// Process the attack set generated by the mob collection functions
        /// </summary>
        /// <param name="attackSet"></param>
        private void ProcessAttackSet(EnumerableRowCollection<AttackGroup> attackSet)
        {

            ResetTextBox();

            int countAttacks;

            foreach (var player in attackSet)
            {
                countAttacks = player.Melee.Count() +
                    player.Range.Count() +
                    player.Spell.Count() +
                    player.Ability.Count() +
                    player.WSkill.Count();

                if (countAttacks > 0)
                {
                    AppendText(player.Name + "\n", Color.Red, true, false);

                    if ((player.Melee.Count() > 0) &&
                        (player.Melee.Any(n => (DamageModifier)n.DamageModifier != DamageModifier.Critical)))
                    {
                        AppendText(lsMelee + "\n", Color.Blue, true, false);
                        if (showDetails == true)
                            ShowDetailedDamage(player.Melee.Where(m => (DamageModifier)m.DamageModifier != DamageModifier.Critical));

                        var meleeFreq = player.Melee.Where(m => (DamageModifier)m.DamageModifier != DamageModifier.Critical)
                            .GroupBy(m => m.Amount).OrderBy(m => m.Key);

                        ShowFrequency(meleeFreq);
                    }

                    if ((player.Melee.Count() > 0) &&
                        (player.Melee.Any(n => (DamageModifier)n.DamageModifier == DamageModifier.Critical)))
                    {
                        AppendText(lsMeleeCrits + "\n", Color.Blue, true, false);
                        if (showDetails == true)
                            ShowDetailedDamage(player.Melee.Where(m => (DamageModifier)m.DamageModifier == DamageModifier.Critical));

                        var critFreq = player.Melee.Where(m => (DamageModifier)m.DamageModifier == DamageModifier.Critical)
                            .GroupBy(m => m.Amount).OrderBy(m => m.Key);

                        ShowFrequency(critFreq);
                    }

                    if ((player.Melee.Count() > 0) &&
                        (player.Melee.Any(n => (HarmType)n.SecondHarmType == HarmType.Damage ||
                            (HarmType)n.SecondHarmType == HarmType.Drain)))
                    {
                        AppendText(lsMeleeAE + "\n", Color.Blue, true, false);
                        if (showDetails == true)
                            ShowDetailedSecondaryDamage(player.Melee.Where(n => (HarmType)n.SecondHarmType == HarmType.Damage ||
                            (HarmType)n.SecondHarmType == HarmType.Drain));

                        var meleeFreq = player.Melee.Where(n => (HarmType)n.SecondHarmType == HarmType.Damage ||
                            (HarmType)n.SecondHarmType == HarmType.Drain)
                            .GroupBy(m => m.SecondAmount).OrderBy(m => m.Key);

                        ShowFrequency(meleeFreq);
                    }

                    if ((player.Range.Count() > 0) &&
                        (player.Range.Any(n => (DamageModifier)n.DamageModifier != DamageModifier.Critical)))
                    {
                        AppendText(lsRanged + "\n", Color.Blue, true, false);
                        if (showDetails == true)
                            ShowDetailedDamage(player.Range.Where(m => (DamageModifier)m.DamageModifier != DamageModifier.Critical));

                        var rangeFreq = player.Range.Where(m => (DamageModifier)m.DamageModifier != DamageModifier.Critical)
                            .GroupBy(m => m.Amount).OrderBy(m => m.Key);

                        ShowFrequency(rangeFreq);
                    }

                    if ((player.Range.Count() > 0) &&
                        (player.Range.Any(n => (DamageModifier)n.DamageModifier == DamageModifier.Critical)))
                    {
                        AppendText(lsRangedCrits + "\n", Color.Blue, true, false);
                        if (showDetails == true)
                            ShowDetailedDamage(player.Range.Where(m => (DamageModifier)m.DamageModifier == DamageModifier.Critical));

                        var critFreq = player.Range.Where(m => (DamageModifier)m.DamageModifier == DamageModifier.Critical)
                            .GroupBy(m => m.Amount).OrderBy(m => m.Key);

                        ShowFrequency(critFreq);
                    }

                    if ((player.Range.Count() > 0) &&
                        (player.Range.Any(n => (HarmType)n.SecondHarmType == HarmType.Damage ||
                            (HarmType)n.SecondHarmType == HarmType.Drain)))
                    {
                        AppendText(lsRangedAE + "\n", Color.Blue, true, false);
                        if (showDetails == true)
                            ShowDetailedSecondaryDamage(player.Range.Where(n => (HarmType)n.SecondHarmType == HarmType.Damage ||
                            (HarmType)n.SecondHarmType == HarmType.Drain));

                        var rangeFreq = player.Range.Where(n => (HarmType)n.SecondHarmType == HarmType.Damage ||
                            (HarmType)n.SecondHarmType == HarmType.Drain)
                            .GroupBy(m => m.SecondAmount).OrderBy(m => m.Key);

                        ShowFrequency(rangeFreq);
                    }

                    if ((player.Spell.Count() > 0) &&
                        (player.Spell.Any(n => (DamageModifier)n.DamageModifier != DamageModifier.MagicBurst)))
                    {
                        AppendText(lsSpells + "\n", Color.Blue, true, false);

                        var spellGroups = player.Spell.Where(s => (DamageModifier)s.DamageModifier != DamageModifier.MagicBurst)
                            .GroupBy(s => s.ActionsRow.ActionName)
                            .OrderBy(s => s.Key);

                        foreach (var spell in spellGroups)
                        {
                            AppendText(string.Format("    {0}\n", spell.Key), Color.Black, true, false);

                            if (showDetails == true)
                                ShowDetailedDamage(spell.Where(m => (DamageModifier)m.DamageModifier != DamageModifier.MagicBurst));

                            var spellFreq = spell.Where(m => (DamageModifier)m.DamageModifier != DamageModifier.MagicBurst)
                                .GroupBy(m => m.Amount).OrderBy(m => m.Key);

                            ShowFrequency(spellFreq);
                        }
                    }

                    if ((player.Spell.Count() > 0) &&
                        (player.Spell.Any(n => (DamageModifier)n.DamageModifier == DamageModifier.MagicBurst)))
                    {
                        AppendText(lsMagicBursts + "\n", Color.Blue, true, false);

                        var spellGroups = player.Spell.Where(s => (DamageModifier)s.DamageModifier == DamageModifier.MagicBurst)
                            .GroupBy(s => s.ActionsRow.ActionName)
                            .OrderBy(s => s.Key);


                        foreach (var spell in spellGroups)
                        {
                            AppendText(string.Format("    {0}\n", spell.Key), Color.Black, true, false);

                            if (showDetails == true)
                                ShowDetailedDamage(spell.Where(m => (DamageModifier)m.DamageModifier == DamageModifier.MagicBurst));

                            var spellFreq = spell.Where(m => (DamageModifier)m.DamageModifier == DamageModifier.MagicBurst)
                                .GroupBy(m => m.Amount).OrderBy(m => m.Key);

                            ShowFrequency(spellFreq);
                        }
                    }

                    if (player.Ability.Count() > 0)
                    {
                        AppendText(lsAbility + "\n", Color.Blue, true, false);

                        var abilityGroups = player.Ability.GroupBy(s => s.ActionsRow.ActionName)
                            .OrderBy(s => s.Key);

                        foreach (var ability in abilityGroups)
                        {
                            AppendText(string.Format("    {0}\n", ability.Key), Color.Black, true, false);

                            if (showDetails == true)
                                ShowDetailedDamage(ability);

                            var abilFreq = ability.GroupBy(m => m.Amount).OrderBy(m => m.Key);

                            ShowFrequency(abilFreq);
                        }
                    }

                    if (player.WSkill.Count() > 0)
                    {
                        AppendText(lsWeaponskills + "\n", Color.Blue, true, false);

                        var wsGroups = player.WSkill.GroupBy(s => s.ActionsRow.ActionName)
                            .OrderBy(s => s.Key);

                        foreach (var wskill in wsGroups)
                        {
                            AppendText(string.Format("    {0}\n", wskill.Key), Color.Black, true, false);

                            if (showDetails == true)
                                ShowDetailedDamage(wskill);

                            var wsFreq = wskill.GroupBy(m => m.Amount).OrderBy(m => m.Key);

                            ShowFrequency(wsFreq);
                        }
                    }

                    if (player.Spikes.Count() > 0)
                    {
                        AppendText(lsSpikes + "\n", Color.Blue, true, false);
                        if (showDetails == true)
                            ShowDetailedDamage(player.Spikes);

                        var spikeFreq = player.Spikes
                            .GroupBy(m => m.Amount).OrderBy(m => m.Key);

                        ShowFrequency(spikeFreq);
                    }

                    AppendText("\n");
                }
            }
        }

        /// <summary>
        /// Show frequency data for the provided damage grouping
        /// </summary>
        /// <param name="freqGrouping"></param>
        private void ShowFrequency(IOrderedEnumerable<IGrouping<int, KPDatabaseDataSet.InteractionsRow>> freqGrouping)
        {
            if (freqGrouping.Count() == 0)
                return;

            StringBuilder strBuilder = new StringBuilder();
            int max = freqGrouping.Max(f => f.Count());
            int total = freqGrouping.Sum(f => f.Count());
            int sum = 0;
            int half = total / 2;
            var medianStart = freqGrouping.SkipWhile(f => (sum += f.Count()) <= half);
            var median = medianStart.FirstOrDefault();

            foreach (var freq in freqGrouping)
            {
                if (freq.Count() == max)
                    strBuilder.Append("+");
                else
                    strBuilder.Append(" ");

                if ((median != null) && (freq.Key == median.Key))
                    strBuilder.Append("^");
                else
                    strBuilder.Append(" ");


                strBuilder.AppendFormat("   {0,4}: {1,4}\n", freq.Key, freq.Count());
            }

            AppendText(strBuilder.ToString());
        }

        /// <summary>
        /// Show detailed damage listing for the provided group.
        /// </summary>
        /// <param name="rows"></param>
        private void ShowDetailedDamage(IEnumerable<KPDatabaseDataSet.InteractionsRow> rows)
        {
            int count = 0;

            StringBuilder strBuilder = new StringBuilder();

            foreach (var row in rows)
            {
                if (count % 10 == 0)
                    strBuilder.Append("   ");

                strBuilder.AppendFormat(" {0,4}", row.Amount);

                if (count % 10 == 9)
                    strBuilder.Append("\n");

                count++;
            }

            if (count % 10 != 0)
                strBuilder.Append("\n");

            AppendText(strBuilder.ToString());
        }

        /// <summary>
        /// Show detailed damage listing for the provided group.
        /// </summary>
        /// <param name="rows"></param>
        private void ShowDetailedSecondaryDamage(IEnumerable<KPDatabaseDataSet.InteractionsRow> rows)
        {
            int count = 0;

            StringBuilder strBuilder = new StringBuilder();

            foreach (var row in rows)
            {
                if (count % 10 == 0)
                    strBuilder.Append("   ");

                strBuilder.AppendFormat(" {0,4}", row.SecondAmount);

                if (count % 10 == 9)
                    strBuilder.Append("\n");

                count++;
            }

            if (count % 10 != 0)
                strBuilder.Append("\n");

            AppendText(strBuilder.ToString());
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

        protected void groupMobs_Click(object sender, EventArgs e)
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

        void optionsMenu_DropDownOpening(object sender, EventArgs e)
        {
            groupMobsOption.Enabled = !Monitoring.Monitor.Instance.IsRunning & !customMobSelectionOption.Checked;
            customMobSelectionOption.Enabled = !Monitoring.Monitor.Instance.IsRunning;
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
            showDetailOption.Text = Resources.PublicResources.ShowDetail;
            editCustomMobFilter.Text = Resources.PublicResources.EditMobFilter;

            UpdatePlayerList();
            playersCombo.SelectedIndex = 0;

            UpdateMobList();
            mobsCombo.SelectedIndex = 0;
        }

        protected override void LoadResources()
        {
            this.tabName = Resources.Combat.OffenseFreqTabName;

            lsAll = Resources.PublicResources.All;

            lsMelee = Resources.Combat.DefenseFreqPluginMelee;
            lsMeleeCrits = Resources.Combat.DefenseFreqPluginMeleeCrits;
            lsMeleeAE = Resources.Combat.DefenseFreqPluginMeleeAE;
            lsRanged = Resources.Combat.DefenseFreqPluginRange;
            lsRangedCrits = Resources.Combat.DefenseFreqPluginRangeCrits;
            lsRangedAE = Resources.Combat.DefenseFreqPluginRangeAE;
            lsSpells = Resources.Combat.DefenseFreqPluginSpells;
            lsMagicBursts = Resources.Combat.DefenseFreqPluginMagicBursts;
            lsAbility = Resources.Combat.DefenseFreqPluginAbility;
            lsWeaponskills = Resources.Combat.DefenseFreqPluginWeaponskill;
            lsSpikes = Resources.Combat.DefenseFreqPluginSpikes;

        }
        #endregion

    }
}
