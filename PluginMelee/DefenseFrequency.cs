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
    public class DefenseFrequencyDataPlugin : BasePluginControl
    {
        #region Constructor
        bool flagNoUpdate;
        bool groupMobs = false;
        bool exclude0XPMobs = false;
        bool showDetails = false;
        ToolStripComboBox playersCombo = new ToolStripComboBox();
        ToolStripComboBox mobsCombo = new ToolStripComboBox();
        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();

        public DefenseFrequencyDataPlugin()
        {
            ToolStripLabel catLabel = new ToolStripLabel();
            catLabel.Text = "Players:";
            toolStrip.Items.Add(catLabel);

            playersCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            playersCombo.Items.Add("All");
            playersCombo.MaxDropDownItems = 10;
            playersCombo.SelectedIndex = 0;
            playersCombo.SelectedIndexChanged += new EventHandler(this.playersCombo_SelectedIndexChanged);
            toolStrip.Items.Add(playersCombo);


            ToolStripLabel mobsLabel = new ToolStripLabel();
            mobsLabel.Text = "Mobs:";
            toolStrip.Items.Add(mobsLabel);

            mobsCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            mobsCombo.AutoSize = false;
            mobsCombo.Width = 175;
            mobsCombo.Items.Add("All");
            mobsCombo.MaxDropDownItems = 10;
            mobsCombo.SelectedIndex = 0;
            mobsCombo.SelectedIndexChanged += new EventHandler(this.mobsCombo_SelectedIndexChanged);
            toolStrip.Items.Add(mobsCombo);


            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;
            optionsMenu.Text = "Options";

            ToolStripMenuItem groupMobsOption = new ToolStripMenuItem();
            groupMobsOption.Text = "Group Mobs";
            groupMobsOption.CheckOnClick = true;
            groupMobsOption.Checked = false;
            groupMobsOption.Click += new EventHandler(groupMobs_Click);
            optionsMenu.DropDownItems.Add(groupMobsOption);

            ToolStripMenuItem exclude0XPOption = new ToolStripMenuItem();
            exclude0XPOption.Text = "Exclude 0 XP Mobs";
            exclude0XPOption.CheckOnClick = true;
            exclude0XPOption.Checked = false;
            exclude0XPOption.Click += new EventHandler(exclude0XPMobs_Click);
            optionsMenu.DropDownItems.Add(exclude0XPOption);

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
            get { return "Defense Detail"; }
        }

        public override void Reset()
        {
            ResetTextBox();
        }

        public override void NotifyOfUpdate(KPDatabaseDataSet dataSet)
        {
            ResetTextBox();

            UpdatePlayerList(dataSet);
            UpdateMobList(dataSet, false);

            // Don't generate an update on the first combo box change
            flagNoUpdate = true;
            playersCombo.CBSelectIndex(0);

            // Setting the second combo box will cause the display to load.
            mobsCombo.CBSelectIndex(0);
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
                playersCombo.CBSelectIndex(0);
            }

            // Check for new mobs being fought.  If any exist, update the Mob Group dropdown list.
            if ((e.DatasetChanges.Battles != null) &&
                (e.DatasetChanges.Battles.Count > 0))
            {
                UpdateMobList(e.Dataset, true);
                changesFound = true;

                flagNoUpdate = true;
                mobsCombo.CBSelectIndex(-1);
            }

            if ((playersCombo.CBSelectedIndex() < 0) ||
                (currentlySelectedPlayer != playersCombo.CBSelectedItem()))
            {
                flagNoUpdate = true;
                playersCombo.CBSelectItem(currentlySelectedPlayer);
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

        private void UpdateMobList()
        {
            UpdateMobList(DatabaseManager.Instance.Database, false);
            mobsCombo.CBSelectIndex(0);
        }

        private void UpdateMobList(KPDatabaseDataSet dataSet, bool overrideGrouping)
        {
            mobsCombo.CBReset();

            if (overrideGrouping == false)
                mobsCombo.CBAddStrings(GetMobListing(dataSet, groupMobs, exclude0XPMobs));
            else
                mobsCombo.CBAddStrings(GetMobListing(dataSet, false, exclude0XPMobs));
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
            MobFilter mobFilter = mobsCombo.CBGetMobFilter();

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

            ProcessFilteredMobs(dataSet, playerList.ToArray(), mobFilter);
        }

        private void ProcessFilteredMobs(KPDatabaseDataSet dataSet, string[] selectedPlayers, MobFilter mobFilter)
        {
            var attackSet = from c in dataSet.Combatants
                            where (selectedPlayers.Contains(c.CombatantName))
                            select new AttackGroup
                            {
                                Name = c.CombatantName,
                                Melee = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Melee &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobActor(n) == true
                                        select n,
                                Range = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Ranged &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobActor(n) == true
                                        select n,
                                Spell = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Spell &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                                n.Preparing == false &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobActor(n) == true
                                        select n,
                                Ability = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                          where ((ActionType)n.ActionType == ActionType.Ability &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain ||
                                                (HarmType)n.HarmType == HarmType.Unknown) &&
                                                n.Preparing == false &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobActor(n) == true
                                          select n,
                                WSkill = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                         where ((ActionType)n.ActionType == ActionType.Weaponskill &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                                n.Preparing == false &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobActor(n) == true
                                         select n,
                                SC = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                     where ((ActionType)n.ActionType == ActionType.Skillchain &&
                                            ((HarmType)n.HarmType == HarmType.Damage ||
                                             (HarmType)n.HarmType == HarmType.Drain) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobActor(n) == true
                                     select n,
                                Counter = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                          where (ActionType)n.ActionType == ActionType.Counterattack &&
                                               ((DefenseType)n.DefenseType == DefenseType.None) &&
                                               mobFilter.CheckFilterMobActor(n) == true
                                          select n,
                                Retaliate = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                            where (ActionType)n.ActionType == ActionType.Retaliation &&
                                               ((DefenseType)n.DefenseType == DefenseType.None) &&
                                               mobFilter.CheckFilterMobActor(n) == true
                                            select n,
                                Spikes = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                         where (ActionType)n.ActionType == ActionType.Spikes &&
                                               ((DefenseType)n.DefenseType == DefenseType.None) &&
                                               mobFilter.CheckFilterMobActor(n) == true
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
                        AppendText("  Melee\n", Color.Blue, true, false);
                        if (showDetails == true)
                            ShowDetailedDamage(player.Melee.Where(m => (DamageModifier)m.DamageModifier != DamageModifier.Critical));

                        var meleeFreq = player.Melee.Where(m => (DamageModifier)m.DamageModifier != DamageModifier.Critical)
                            .GroupBy(m => m.Amount).OrderBy(m => m.Key);

                        ShowFrequency(meleeFreq);
                    }

                    if ((player.Melee.Count() > 0) &&
                        (player.Melee.Any(n => (DamageModifier)n.DamageModifier == DamageModifier.Critical)))
                    {
                        AppendText("  Melee Crits\n", Color.Blue, true, false);
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
                        AppendText("  Melee Additional Effects\n", Color.Blue, true, false);
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
                        AppendText("  Range\n", Color.Blue, true, false);
                        if (showDetails == true)
                            ShowDetailedDamage(player.Range.Where(m => (DamageModifier)m.DamageModifier != DamageModifier.Critical));

                        var rangeFreq = player.Range.Where(m => (DamageModifier)m.DamageModifier != DamageModifier.Critical)
                            .GroupBy(m => m.Amount).OrderBy(m => m.Key);

                        ShowFrequency(rangeFreq);
                    }

                    if ((player.Range.Count() > 0) &&
                        (player.Range.Any(n => (DamageModifier)n.DamageModifier == DamageModifier.Critical)))
                    {
                        AppendText("  Range Crits\n", Color.Blue, true, false);
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
                        AppendText("  Range Additional Effects\n", Color.Blue, true, false);
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
                        AppendText("  Spells\n", Color.Blue, true, false);

                        var spellGroups = player.Spell.GroupBy(s => s.ActionsRow.ActionName)
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
                        AppendText("  Magic Bursts\n", Color.Blue, true, false);

                        var spellGroups = player.Spell.GroupBy(s => s.ActionsRow.ActionName)
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
                        AppendText("  Ability\n", Color.Blue, true, false);

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
                        AppendText("  Weaponskill\n", Color.Blue, true, false);

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
                        AppendText("  Spikes\n", Color.Blue, true, false);
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
                HandleDataset(DatabaseManager.Instance.Database);

            flagNoUpdate = false;
        }

        protected void mobsCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(DatabaseManager.Instance.Database);

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

                HandleDataset(DatabaseManager.Instance.Database);
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

                HandleDataset(DatabaseManager.Instance.Database);
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
                HandleDataset(DatabaseManager.Instance.Database);

            flagNoUpdate = false;
        }
        #endregion
    }
}
