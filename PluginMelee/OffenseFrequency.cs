using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using WaywardGamers.KParser;
using System.Diagnostics;

namespace WaywardGamers.KParser.Plugin
{
    public class OffenseFrequencyDataPlugin : BasePluginControlWithDropdown
    {
        #region Member variables
        bool checkBox1Changed = false;
        #endregion

        #region IPlugin Overrides
        public override string TabName
        {
            get { return "Offense Detail"; }
        }

        public override void Reset()
        {
            richTextBox.Clear();
            richTextBox.WordWrap = false;

            label1.Text = "Players";
            comboBox1.Left = label1.Right + 10;
            comboBox1.MaxDropDownItems = 9;
            comboBox1.Items.Clear();

            label2.Left = comboBox1.Right + 20;
            label2.Text = "Enemies";
            comboBox2.Left = label2.Right + 10;
            comboBox2.Width = 150;
            comboBox2.MaxDropDownItems = 10;
            comboBox2.Items.Clear();
            comboBox2.Items.Add("All");
            comboBox2.SelectedIndex = 0;

            checkBox1.Left = comboBox2.Right + 20;
            checkBox1.Text = "Group Enemies";
            checkBox1.Checked = false;

            checkBox2.Left = checkBox1.Right + 10;
            checkBox2.Text = "Show Detail";
            checkBox2.Checked = false;

            richTextBox.Clear();
        }

        public override void DatabaseOpened(KPDatabaseDataSet dataSet)
        {
            ResetTextBox();

            UpdatePlayerList(dataSet);
            UpdateMobList(dataSet);
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
        {
            // Check for new mobs being fought.  If any exist, update the Mob Group dropdown list.
            if (e.DatasetChanges.Battles != null)
            {
                if (e.DatasetChanges.Battles.Count > 0)
                {
                    var mobsFought = from b in e.DatasetChanges.Battles
                                     where ((b.DefaultBattle == false) &&
                                            (b.IsEnemyIDNull() == false) &&
                                            (b.CombatantsRowByEnemyCombatantRelation.CombatantType == (byte)EntityType.Mob))
                                     group b by b.CombatantsRowByEnemyCombatantRelation.CombatantName into bn
                                     select new
                                     {
                                         Name = bn.Key,
                                         XP = from xb in bn
                                              group xb by xb.MinBaseExperience() into xbn
                                              orderby xbn.Key
                                              select new { BaseXP = xbn.Key }
                                     };


                    if (mobsFought.Count() > 0)
                    {
                        string mobWithXP;

                        foreach (var mob in mobsFought)
                        {
                            if (comboBox2.Items.Contains(mob.Name) == false)
                                AddToComboBox2(mob.Name);

                            foreach (var xp in mob.XP)
                            {
                                if (xp.BaseXP > 0)
                                {
                                    mobWithXP = string.Format("{0} ({1})", mob.Name, xp.BaseXP);

                                    if (comboBox2.Items.Contains(mobWithXP) == false)
                                        AddToComboBox2(mobWithXP);

                                    // Check for existing entry with higher min base xp
                                    mobWithXP = string.Format("{0} ({1})", mob.Name, xp.BaseXP + 1);

                                    if (comboBox2.Items.Contains(mobWithXP))
                                        RemoveFromComboBox2(mobWithXP);
                                }
                            }
                        }
                    }
                }
            }

            if (e.DatasetChanges.Interactions.Count != 0)
            {
                datasetToUse = e.Dataset;
                return true;
            }

            datasetToUse = null;
            return false;
        }
        #endregion

        #region Private functions
        private void UpdatePlayerList(KPDatabaseDataSet dataSet)
        {
            ResetComboBox1();
            AddToComboBox1("All");

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

            foreach (var player in playersFighting)
                playerStrings.Add(player.Name);

            if (playersFighting.Count() > 0)
                AddToComboBox1(playerStrings.ToArray());

            InitComboBox1Selection();
        }

        private void UpdateMobList(KPDatabaseDataSet dataSet)
        {
            ResetComboBox2();
            AddToComboBox2("All");

            // Group enemies check

            if (checkBox1.Checked == true)
            {
                // Enemy group listing

                var mobsKilled = from b in dataSet.Battles
                                 where ((b.DefaultBattle == false) &&
                                        (b.IsEnemyIDNull() == false) &&
                                        (b.CombatantsRowByEnemyCombatantRelation.CombatantType == (byte)EntityType.Mob))
                                 orderby b.CombatantsRowByEnemyCombatantRelation.CombatantName
                                 group b by b.CombatantsRowByEnemyCombatantRelation.CombatantName into bn
                                 select new
                                 {
                                     Name = bn.Key,
                                     XP = from xb in bn
                                          group xb by xb.MinBaseExperience() into xbn
                                          orderby xbn.Key
                                          select new { BaseXP = xbn.Key }
                                 };

                List<string> mobXPStrings = new List<string>();

                foreach (var mob in mobsKilled)
                {
                    mobXPStrings.Add(mob.Name);

                    foreach (var xp in mob.XP)
                    {
                        if (xp.BaseXP > 0)
                            mobXPStrings.Add(string.Format("{0} ({1})", mob.Name, xp.BaseXP));
                    }
                }

                if (mobXPStrings.Count > 0)
                    AddToComboBox2(mobXPStrings.ToArray());
            }
            else
            {
                // Enemy battle listing

                var mobsKilled = from b in dataSet.Battles
                                 where ((b.DefaultBattle == false) &&
                                        (b.IsEnemyIDNull() == false) &&
                                        ((EntityType)b.CombatantsRowByEnemyCombatantRelation.CombatantType == EntityType.Mob))
                                 orderby b.EndTime
                                 select new
                                 {
                                     Name = b.CombatantsRowByEnemyCombatantRelation.CombatantName,
                                     Battle = b.BattleID
                                 };

                List<string> mobXPStrings = new List<string>();

                int numMobsKilled = mobsKilled.Count();

                foreach (var mob in mobsKilled)
                {
                    mobXPStrings.Add(string.Format("{0,3}: {1}", mob.Battle,
                            mob.Name));
                }

                AddToComboBox2(mobXPStrings.ToArray());
            }

            InitComboBox2Selection();
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
            if (comboBox1.Items.Count == 0)
                return;

            if (comboBox2.Items.Count == 0)
                return;

            if (checkBox1Changed == true)
            {
                checkBox1Changed = false;
                UpdateMobList(dataSet);
            }


            string selectedPlayer = comboBox1.SelectedItem.ToString();
            string selectedMob = comboBox2.SelectedItem.ToString();

            List<string> playerList = new List<string>();

            if (selectedPlayer == "All")
            {
                foreach (var player in comboBox1.Items)
                {
                    if (player.ToString() != "All")
                        playerList.Add(player.ToString());
                }
            }
            else
            {
                playerList.Add(selectedPlayer);
            }

            if (playerList.Count == 0)
                return;

            if (selectedMob == "All")
                ProcessAllMobs(dataSet, playerList.ToArray());
            else if (checkBox1.Checked == true)
                ProcessMobGroup(dataSet, playerList.ToArray(), selectedMob);
            else
                ProcessBattle(dataSet, playerList.ToArray(), selectedMob);
        }

        /// <summary>
        /// Process all mobs
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="selectedPlayers"></param>
        private void ProcessAllMobs(KPDatabaseDataSet dataSet, string[] selectedPlayers)
        {
            var attackSet = from c in dataSet.Combatants
                            where (selectedPlayers.Contains(c.CombatantName))
                            select new AttackGroup
                            {
                                Player = c.CombatantName,
                                Melee = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Melee &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                        select n,
                                Range = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Ranged &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                        select n,
                                Spell = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Spell &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                                n.Preparing == false &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                        select n,
                                Ability = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                          where ((ActionType)n.ActionType == ActionType.Ability &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain ||
                                                (HarmType)n.HarmType == HarmType.Unknown) &&
                                                n.Preparing == false &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                          select n,
                                WSkill = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where ((ActionType)n.ActionType == ActionType.Weaponskill &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                                n.Preparing == false &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                         select n,
                                SC = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                     where ((ActionType)n.ActionType == ActionType.Skillchain &&
                                            ((HarmType)n.HarmType == HarmType.Damage ||
                                             (HarmType)n.HarmType == HarmType.Drain) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                     select n,
                                Counter = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                          where (ActionType)n.ActionType == ActionType.Counterattack &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)
                                          select n,
                                Retaliate = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                            where (ActionType)n.ActionType == ActionType.Retaliation &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)
                                            select n,
                                Spikes = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where (ActionType)n.ActionType == ActionType.Spikes &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)
                                         select n
                            };

            ProcessAttackSet(attackSet);
        }

        /// <summary>
        /// Process a mob type, or mob level type
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="selectedPlayers"></param>
        /// <param name="selectedMob"></param>
        private void ProcessMobGroup(KPDatabaseDataSet dataSet, string[] selectedPlayers, string selectedMob)
        {
            Regex mobAndXP = new Regex(@"((?<mobName>.*(?<! \())) \(((?<xp>\d+)\))|(?<mobName>.*)");
            Match mobAndXPMatch = mobAndXP.Match(selectedMob);

            if (mobAndXPMatch.Success == false)
                return;

            string mobName;
            int xp = 0;

            mobName = mobAndXPMatch.Groups["mobName"].Value;

            if ((mobAndXPMatch.Groups["xp"] != null) && (mobAndXPMatch.Groups["xp"].Value != string.Empty))
            {
                xp = int.Parse(mobAndXPMatch.Groups["xp"].Value);
            }


            var attackSet = from c in dataSet.Combatants
                            where (selectedPlayers.Contains(c.CombatantName))
                            select new AttackGroup
                            {
                                Player = c.CombatantName,
                                Melee = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Melee &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                               n.IsTargetIDNull() == false &&
                                               n.CombatantsRowByTargetCombatantRelation.CombatantName == mobName &&
                                               n.IsBattleIDNull() == false &&
                                               (xp == 0 ||
                                               n.BattlesRow.MinBaseExperience() == xp) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                        select n,
                                Range = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Ranged &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                               n.IsTargetIDNull() == false &&
                                               n.CombatantsRowByTargetCombatantRelation.CombatantName == mobName &&
                                               n.IsBattleIDNull() == false &&
                                               (xp == 0 ||
                                               n.BattlesRow.MinBaseExperience() == xp) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                        select n,
                                Spell = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Spell &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                               n.IsTargetIDNull() == false &&
                                               n.CombatantsRowByTargetCombatantRelation.CombatantName == mobName &&
                                               n.IsBattleIDNull() == false &&
                                               (xp == 0 ||
                                               n.BattlesRow.MinBaseExperience() == xp) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                        select n,
                                Ability = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                          where ((ActionType)n.ActionType == ActionType.Ability &&
                                                 ((HarmType)n.HarmType == HarmType.Damage ||
                                                  (HarmType)n.HarmType == HarmType.Drain ||
                                                  (HarmType)n.HarmType == HarmType.Unknown) &&
                                                 n.IsTargetIDNull() == false &&
                                                 n.CombatantsRowByTargetCombatantRelation.CombatantName == mobName &&
                                                 n.IsBattleIDNull() == false &&
                                                 (xp == 0 ||
                                                 n.BattlesRow.MinBaseExperience() == xp) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                          select n,
                                WSkill = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where ((ActionType)n.ActionType == ActionType.Weaponskill &&
                                                ((HarmType)n.HarmType == HarmType.Damage ||
                                                 (HarmType)n.HarmType == HarmType.Drain) &&
                                                n.IsTargetIDNull() == false &&
                                                n.CombatantsRowByTargetCombatantRelation.CombatantName == mobName &&
                                                n.IsBattleIDNull() == false &&
                                                (xp == 0 ||
                                                n.BattlesRow.MinBaseExperience() == xp) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                         select n,
                                SC = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                     where ((ActionType)n.ActionType == ActionType.Skillchain &&
                                            ((HarmType)n.HarmType == HarmType.Damage ||
                                             (HarmType)n.HarmType == HarmType.Drain) &&
                                            n.IsTargetIDNull() == false &&
                                            n.CombatantsRowByTargetCombatantRelation.CombatantName == mobName &&
                                            n.IsBattleIDNull() == false &&
                                            (xp == 0 ||
                                            n.BattlesRow.MinBaseExperience() == xp) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                     select n,
                                Counter = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                          where ((ActionType)n.ActionType == ActionType.Counterattack &&
                                                 n.IsTargetIDNull() == false &&
                                                 n.CombatantsRowByTargetCombatantRelation.CombatantName == mobName &&
                                                 n.IsBattleIDNull() == false &&
                                                 (xp == 0 ||
                                                 n.BattlesRow.MinBaseExperience() == xp) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                          select n,
                                Retaliate = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                            where ((ActionType)n.ActionType == ActionType.Retaliation &&
                                                   n.IsTargetIDNull() == false &&
                                                   n.CombatantsRowByTargetCombatantRelation.CombatantName == mobName &&
                                                   n.IsBattleIDNull() == false &&
                                                   (xp == 0 ||
                                                   n.BattlesRow.MinBaseExperience() == xp) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                            select n,
                                Spikes = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where ((ActionType)n.ActionType == ActionType.Spikes &&
                                                n.IsTargetIDNull() == false &&
                                                n.CombatantsRowByTargetCombatantRelation.CombatantName == mobName &&
                                                n.IsBattleIDNull() == false &&
                                                (xp == 0 ||
                                                n.BattlesRow.MinBaseExperience() == xp) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                         select n,
                            };

            ProcessAttackSet(attackSet);
        }

        /// <summary>
        /// Process a single battle
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="selectedPlayers"></param>
        /// <param name="selectedMob"></param>
        private void ProcessBattle(KPDatabaseDataSet dataSet, string[] selectedPlayers, string selectedMob)
        {
            Regex mobBattle = new Regex(@"\s*(?<battleID>\d+):\s+(?<mobName>.*)");
            Match mobBattleMatch = mobBattle.Match(selectedMob);

            if (mobBattleMatch.Success == false)
                return;

            int battleID = int.Parse(mobBattleMatch.Groups["battleID"].Value);

            var attackSet = from c in dataSet.Combatants
                            where (selectedPlayers.Contains(c.CombatantName))
                            select new AttackGroup
                            {
                                Player = c.CombatantName,
                                Melee = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((n.IsBattleIDNull() == false) &&
                                               (n.BattleID == battleID) &&
                                               ((ActionType)n.ActionType == ActionType.Melee) &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                        select n,
                                Range = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((n.IsBattleIDNull() == false) &&
                                               (n.BattleID == battleID) &&
                                               ((ActionType)n.ActionType == ActionType.Ranged) &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                        select n,
                                Spell = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((n.IsBattleIDNull() == false) &&
                                               (n.BattleID == battleID) &&
                                               ((ActionType)n.ActionType == ActionType.Spell) &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                                n.Preparing == false &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                        select n,
                                Ability = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                          where ((n.IsBattleIDNull() == false) &&
                                               (n.BattleID == battleID) &&
                                               ((ActionType)n.ActionType == ActionType.Ability) &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain ||
                                                (HarmType)n.HarmType == HarmType.Unknown) &&
                                                n.Preparing == false &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                          select n,
                                WSkill = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where ((n.IsBattleIDNull() == false) &&
                                               (n.BattleID == battleID) &&
                                               ((ActionType)n.ActionType == ActionType.Weaponskill) &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                                n.Preparing == false &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                         select n,
                                SC = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                     where ((n.IsBattleIDNull() == false) &&
                                               (n.BattleID == battleID) &&
                                               ((ActionType)n.ActionType == ActionType.Skillchain) &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain))
                                     select n,
                                Counter = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                          where (n.IsBattleIDNull() == false) &&
                                               (n.BattleID == battleID) &&
                                               ((ActionType)n.ActionType == ActionType.Counterattack &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                          select n,
                                Retaliate = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                            where (n.IsBattleIDNull() == false) &&
                                               (n.BattleID == battleID) &&
                                               ((ActionType)n.ActionType == ActionType.Retaliation &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                            select n,
                                Spikes = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where (n.IsBattleIDNull() == false) &&
                                               (n.BattleID == battleID) &&
                                               ((ActionType)n.ActionType == ActionType.Spikes)
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
                    AppendBoldText(player.Player + "\n", Color.Red);

                    if ((player.Melee.Count() > 0) &&
                        (player.Melee.Any(n => (DamageModifier)n.DamageModifier != DamageModifier.Critical)))
                    {
                        AppendBoldText("  Melee\n", Color.Blue);
                        if (checkBox2.Checked == true)
                            ShowDetailedDamage(player.Melee.Where(m => (DamageModifier)m.DamageModifier != DamageModifier.Critical));

                        var meleeFreq = player.Melee.Where(m => (DamageModifier)m.DamageModifier != DamageModifier.Critical)
                            .GroupBy(m => m.Amount).OrderBy(m => m.Key);

                        ShowFrequency(meleeFreq);
                    }

                    if ((player.Melee.Count() > 0) &&
                        (player.Melee.Any(n => (DamageModifier)n.DamageModifier == DamageModifier.Critical)))
                    {
                        AppendBoldText("  Melee Crits\n", Color.Blue);
                        if (checkBox2.Checked == true)
                            ShowDetailedDamage(player.Melee.Where(m => (DamageModifier)m.DamageModifier == DamageModifier.Critical));

                        var critFreq = player.Melee.Where(m => (DamageModifier)m.DamageModifier == DamageModifier.Critical)
                            .GroupBy(m => m.Amount).OrderBy(m => m.Key);

                        ShowFrequency(critFreq);
                    }

                    if ((player.Range.Count() > 0) &&
                        (player.Range.Any(n => (DamageModifier)n.DamageModifier != DamageModifier.Critical)))
                    {
                        AppendBoldText("  Range\n", Color.Blue);
                        if (checkBox2.Checked == true)
                            ShowDetailedDamage(player.Range.Where(m => (DamageModifier)m.DamageModifier != DamageModifier.Critical));

                        var rangeFreq = player.Range.Where(m => (DamageModifier)m.DamageModifier != DamageModifier.Critical)
                            .GroupBy(m => m.Amount).OrderBy(m => m.Key);

                        ShowFrequency(rangeFreq);
                    }

                    if ((player.Range.Count() > 0) &&
                        (player.Range.Any(n => (DamageModifier)n.DamageModifier == DamageModifier.Critical)))
                    {
                        AppendBoldText("  Range Crits\n", Color.Blue);
                        if (checkBox2.Checked == true)
                            ShowDetailedDamage(player.Range.Where(m => (DamageModifier)m.DamageModifier == DamageModifier.Critical));

                        var critFreq = player.Range.Where(m => (DamageModifier)m.DamageModifier == DamageModifier.Critical)
                            .GroupBy(m => m.Amount).OrderBy(m => m.Key);

                        ShowFrequency(critFreq);
                    }

                    if ((player.Spell.Count() > 0) &&
                        (player.Spell.Any(n => (DamageModifier)n.DamageModifier != DamageModifier.MagicBurst)))
                    {
                        AppendBoldText("  Spells\n", Color.Blue);

                        var spellGroups = player.Spell.GroupBy(s => s.ActionsRow.ActionName)
                            .OrderBy(s => s.Key);

                        foreach (var spell in spellGroups)
                        {
                            AppendBoldText(string.Format("    {0}\n", spell.Key), Color.Black);

                            if (checkBox2.Checked == true)
                                ShowDetailedDamage(spell.Where(m => (DamageModifier)m.DamageModifier != DamageModifier.MagicBurst));

                            var spellFreq = spell.Where(m => (DamageModifier)m.DamageModifier != DamageModifier.MagicBurst)
                                .GroupBy(m => m.Amount).OrderBy(m => m.Key);

                            ShowFrequency(spellFreq);
                        }
                    }

                    if ((player.Spell.Count() > 0) &&
                        (player.Spell.Any(n => (DamageModifier)n.DamageModifier == DamageModifier.MagicBurst)))
                    {
                        AppendBoldText("  Magic Bursts\n", Color.Blue);

                        var spellGroups = player.Spell.GroupBy(s => s.ActionsRow.ActionName)
                            .OrderBy(s => s.Key);

                        foreach (var spell in spellGroups)
                        {
                            AppendBoldText(string.Format("    {0}\n", spell.Key), Color.Black);

                            if (checkBox2.Checked == true)
                                ShowDetailedDamage(spell.Where(m => (DamageModifier)m.DamageModifier == DamageModifier.MagicBurst));

                            var spellFreq = spell.Where(m => (DamageModifier)m.DamageModifier == DamageModifier.MagicBurst)
                                .GroupBy(m => m.Amount).OrderBy(m => m.Key);

                            ShowFrequency(spellFreq);
                        }
                    }

                    if (player.Ability.Count() > 0)
                    {
                        AppendBoldText("  Ability\n", Color.Blue);

                        var abilityGroups = player.Ability.GroupBy(s => s.ActionsRow.ActionName)
                            .OrderBy(s => s.Key);

                        foreach (var ability in abilityGroups)
                        {
                            AppendBoldText(string.Format("    {0}\n", ability.Key), Color.Black);

                            if (checkBox2.Checked == true)
                                ShowDetailedDamage(ability);

                            var abilFreq = ability.GroupBy(m => m.Amount).OrderBy(m => m.Key);

                            ShowFrequency(abilFreq);
                        }
                    }

                    if (player.WSkill.Count() > 0)
                    {
                        AppendBoldText("  Weaponskill\n", Color.Blue);

                        var wsGroups = player.WSkill.GroupBy(s => s.ActionsRow.ActionName)
                            .OrderBy(s => s.Key);

                        foreach (var wskill in wsGroups)
                        {
                            AppendBoldText(string.Format("    {0}\n", wskill.Key), Color.Black);

                            if (checkBox2.Checked == true)
                                ShowDetailedDamage(wskill);

                            var wsFreq = wskill.GroupBy(m => m.Amount).OrderBy(m => m.Key);

                            ShowFrequency(wsFreq);
                        }
                    }

                    AppendNormalText("\n");
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

            AppendNormalText(strBuilder.ToString());
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

            AppendNormalText(strBuilder.ToString());
        }
        #endregion

        #region Event Handlers
        protected override void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            HandleDataset(DatabaseManager.Instance.Database);
        }

        protected override void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            HandleDataset(DatabaseManager.Instance.Database);
        }

        protected override void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            checkBox1Changed = true;
            HandleDataset(DatabaseManager.Instance.Database);
        }

        protected override void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            HandleDataset(DatabaseManager.Instance.Database);
        }
        #endregion

    }
}
