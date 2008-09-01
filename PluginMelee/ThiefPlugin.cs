﻿using System;
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
    public class ThiefPlugin : BasePluginControlWithDropdown
    {
        #region SATA support classes and functions
        internal enum SATATypes
        {
            None,
            SneakAttack,
            TrickAttack,
            Hide
        }

        internal class SATAEvent
        {
            internal HashSet<SATATypes> SATAActions { get; set; }
            internal bool UsedHide { get; set; }
            internal bool SATASuccess { get; set; }

            internal DateTime DamageTimestamp { get; set; }

            internal ActionType DamageType { get; set; }
            internal DamageModifier DamageModifier { get; set; }
            internal int DamageAmount { get; set; }
            internal string WeaponskillName { get; set; }
        }

        internal SATATypes GetSATAType(string actionName)
        {
            switch (actionName)
            {
                case "Sneak Attack":
                    return SATATypes.SneakAttack;
                case "Trick Attack":
                    return SATATypes.TrickAttack;
                case "Hide":
                    return SATATypes.Hide;
                default:
                    return SATATypes.None;
            }
        }
        #endregion

        #region Member variables
        bool checkBox1Changed = false;
        bool flagNoUpdate = false;

        HashSet<SATATypes> SASet = new HashSet<SATATypes> { SATATypes.SneakAttack };
        HashSet<SATATypes> TASet = new HashSet<SATATypes> { SATATypes.TrickAttack };
        HashSet<SATATypes> SATASet = new HashSet<SATATypes> { SATATypes.SneakAttack, SATATypes.TrickAttack };

        List<SATAEvent> SATAEvents = new List<SATAEvent>();
        #endregion

        #region IPlugin Overrides
        public override string TabName
        {
            get { return "Thief"; }
        }

        public override void Reset()
        {
            richTextBox.Clear();
            richTextBox.WordWrap = false;

            label1.Text = "Players";
            comboBox1.Left = label1.Right + 10;
            comboBox1.MaxDropDownItems = 9;
            comboBox1.Items.Clear();
            comboBox1.Items.Add("All");
            flagNoUpdate = true;
            comboBox1.SelectedIndex = 0;

            label2.Left = comboBox1.Right + 20;
            label2.Text = "Enemies";
            comboBox2.Left = label2.Right + 10;
            comboBox2.Width = 150;
            comboBox2.MaxDropDownItems = 10;
            comboBox2.Items.Clear();
            comboBox2.Items.Add("All");
            flagNoUpdate = true;
            comboBox2.SelectedIndex = 0;

            checkBox1.Left = comboBox2.Right + 20;
            checkBox1.Text = "Group Enemies";
            flagNoUpdate = true;
            checkBox1.Checked = false;

            checkBox2.Enabled = false;
            checkBox2.Visible = false;
            //checkBox2.Left = checkBox1.Right + 10;
            //checkBox2.Text = "Show Detail";
            //flagNoUpdate = true;
            //checkBox2.Checked = false;

        }

        public override void DatabaseOpened(KPDatabaseDataSet dataSet)
        {
            ResetTextBox();

            UpdatePlayerList(dataSet);
            UpdateMobList(dataSet);

            // Don't generate an update on the first combo box change
            flagNoUpdate = true;
            InitComboBox1Selection();

            // Setting the second combo box will cause the display to load.
            InitComboBox2Selection();
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
        {
            bool changesFound = false;
            string currentlySelectedPlayer = "All";

            if (GetComboBox1Index() > 0)
                currentlySelectedPlayer = GetComboBox1Value();

            if ((e.DatasetChanges.Combatants != null) &&
                (e.DatasetChanges.Combatants.Count > 0))
            {
                UpdatePlayerList(e.Dataset);
                changesFound = true;

                flagNoUpdate = true;
                InitComboBox1Selection();
            }

            // Check for new mobs being fought.  If any exist, update the Mob Group dropdown list.
            if ((e.DatasetChanges.Battles != null) &&
                (e.DatasetChanges.Battles.Count > 0))
            {
                if (checkBox1.Checked == true)
                    checkBox1.Checked = false;

                UpdateMobList(e.Dataset);
                changesFound = true;

                flagNoUpdate = true;
                InitComboBox2SelectionLast();
            }

            if (currentlySelectedPlayer != GetComboBox1Value())
            {
                flagNoUpdate = true;
                InitComboBox1Selection(currentlySelectedPlayer);
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
            ResetComboBox1();

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

            AddArrayToComboBox1(playerStrings.ToArray());
        }

        private void UpdateMobList(KPDatabaseDataSet dataSet)
        {
            ResetComboBox2();

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
                mobXPStrings.Add("All");

                foreach (var mob in mobsKilled)
                {
                    mobXPStrings.Add(mob.Name);

                    foreach (var xp in mob.XP)
                    {
                        if (xp.BaseXP > 0)
                            mobXPStrings.Add(string.Format("{0} ({1})", mob.Name, xp.BaseXP));
                    }
                }

                AddArrayToComboBox2(mobXPStrings.ToArray());
            }
            else
            {
                // Enemy battle listing

                var mobsKilled = from b in dataSet.Battles
                                 where ((b.DefaultBattle == false) &&
                                        (b.IsEnemyIDNull() == false) &&
                                        ((EntityType)b.CombatantsRowByEnemyCombatantRelation.CombatantType == EntityType.Mob))
                                 orderby b.BattleID
                                 select new
                                 {
                                     Name = b.CombatantsRowByEnemyCombatantRelation.CombatantName,
                                     Battle = b.BattleID
                                 };

                List<string> mobXPStrings = new List<string>();
                mobXPStrings.Add("All");

                foreach (var mob in mobsKilled)
                {
                    mobXPStrings.Add(string.Format("{0,3}: {1}", mob.Battle,
                            mob.Name));
                }

                AddArrayToComboBox2(mobXPStrings.ToArray());
            }
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
                flagNoUpdate = true;
                InitComboBox2Selection();
                flagNoUpdate = false;
            }


            string selectedPlayer = GetComboBox1Value();
            string selectedMob = GetComboBox2Value();

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
                                Name = c.CombatantName,
                                Melee = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Melee &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                        select n,
                                Ability = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                          where ((ActionType)n.ActionType == ActionType.Ability &&
                                                 (AidType)n.AidType == AidType.Enhance &&
                                                 n.Preparing == false)
                                          select n,
                                WSkill = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where ((ActionType)n.ActionType == ActionType.Weaponskill &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                                n.Preparing == false &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                         select n,
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
                                Name = c.CombatantName,
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
                                Ability = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                          where ((ActionType)n.ActionType == ActionType.Ability &&
                                                 (AidType)n.AidType == AidType.Enhance &&
                                                 n.IsBattleIDNull() == false &&
                                                (n.BattlesRow.DefaultBattle == true ||
                                                 xp == 0 ||
                                                 n.BattlesRow.MinBaseExperience() == xp) &&
                                                n.Preparing == false)
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
                                Name = c.CombatantName,
                                Melee = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((n.IsBattleIDNull() == false) &&
                                               (n.BattleID == battleID ||
                                                n.BattlesRow.DefaultBattle == true) &&
                                               ((ActionType)n.ActionType == ActionType.Melee) &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                        select n,
                                Ability = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                          where ((n.IsBattleIDNull() == false) &&
                                               (n.BattleID == battleID) &&
                                               ((ActionType)n.ActionType == ActionType.Ability) &&
                                                (AidType)n.AidType == AidType.Enhance &&
                                                n.Preparing == false)
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

            foreach (var player in attackSet)
            {
                var sataActions = player.Ability.Where(
                        a => a.IsActionIDNull() == false &&
                        (a.ActionsRow.ActionName == "Sneak Attack" ||
                         a.ActionsRow.ActionName == "Trick Attack" ||
                         a.ActionsRow.ActionName == "Hide"));

                if (sataActions.Count() > 0)
                {
                    AppendBoldText(player.Name + "\n", Color.Red);

                    SATAEvents.Clear();
                    sataActions = sataActions.OrderBy(a => a.InteractionID);

                    while (sataActions.Count() > 0)
                    {
                        var firstAction = sataActions.First();
                        sataActions = sataActions.Skip(1);

                        SATATypes firstActionType = GetSATAType(firstAction.ActionsRow.ActionName);

                        SATAEvent sataEvent = new SATAEvent();
                        sataEvent.SATAActions = new HashSet<SATATypes>();
                        SATAEvents.Add(sataEvent);

                        sataEvent.SATAActions.Add(firstActionType);

                        var nextMelee = player.Melee.FirstOrDefault(m => m.Timestamp >= firstAction.Timestamp);
                        var nextWS = player.WSkill.FirstOrDefault(w => w.Timestamp >= firstAction.Timestamp);

                        KPDatabaseDataSet.InteractionsRow sataDamage;

                        if ((nextMelee != null) && (nextWS != null))
                        {
                            if (nextMelee.InteractionID < nextWS.InteractionID)
                            {
                                sataDamage = nextMelee;
                            }
                            else
                            {
                                sataDamage = nextWS;
                            }
                        }
                        else if (nextMelee != null)
                        {
                            sataDamage = nextMelee;
                        }
                        else if (nextWS != null)
                        {
                            sataDamage = nextWS;
                        }
                        else
                        {
                            continue;
                        }


                        if (sataDamage.Timestamp >= firstAction.Timestamp.AddMinutes(1))
                        {
                            sataEvent.SATASuccess = false;
                            continue;
                        }


                        sataEvent.DamageTimestamp = sataDamage.Timestamp;
                        sataEvent.DamageType = (ActionType)sataDamage.ActionType;
                        if ((ActionType)sataDamage.ActionType == ActionType.Melee)
                        {
                            sataEvent.DamageModifier = (DamageModifier)sataDamage.DamageModifier;
                        }
                        else if ((ActionType)sataDamage.ActionType == ActionType.Weaponskill)
                        {
                            sataEvent.WeaponskillName = sataDamage.ActionsRow.ActionName;
                        }
                        sataEvent.DamageAmount = sataDamage.Amount;


                        while (sataActions.Count() > 0)
                        {
                            var nextAction = sataActions.First();

                            if ((nextAction.Timestamp < sataDamage.Timestamp) ||
                               (nextAction.InteractionID < sataDamage.InteractionID))
                            {
                                sataActions = sataActions.Skip(1);

                                if ((nextAction.ActionsRow.ActionName == "Hide") &&
                                    (FailedActionType)nextAction.FailedActionType == FailedActionType.Discovered)
                                    continue;

                                sataEvent.SATAActions.Add(GetSATAType(nextAction.ActionsRow.ActionName));
                            }
                            else
                            {
                                break;
                            }
                        }
                        

                        if (sataEvent.SATAActions.Contains(SATATypes.Hide))
                            sataEvent.UsedHide = true;

                        if ((DefenseType)sataDamage.DefenseType != DefenseType.None)
                        {
                            sataEvent.SATASuccess = false;
                        }
                        else if ((ActionType)sataDamage.ActionType == ActionType.Melee)
                        {
                            //if (sataEvent.DamageModifier != DamageModifier.Critical)
                            //    sataEvent.SATASuccess = false;

                            sataEvent.SATASuccess = true;
                        }
                        else if (sataEvent.SATAActions.Intersect(SATASet).Count() == 0)
                        {
                            sataEvent.SATASuccess = false;
                        }
                        else
                        {
                            sataEvent.SATASuccess = true;
                        }

                    }

                    // Finished building event list
                    
                    // Now try to display data

                    var SATAList = SATAEvents.Where(s => s.SATASuccess == true &&
                        s.SATAActions.IsSupersetOf(SATASet));

                    var SAList = SATAEvents.Where(s => s.SATASuccess == true &&
                         s.SATAActions.IsSupersetOf(SASet)).Except(SATAList);

                    var TAList = SATAEvents.Where(s => s.SATASuccess == true &&
                         s.SATAActions.IsSupersetOf(TASet)).Except(SATAList);

                    if (SATAList.Count() > 0)
                    {
                        AppendBoldText("  SATA\n", Color.Blue);

                        foreach (var sEvent in SATAList)
                        {
                            string dataLine = string.Format("    {0,-15}{1,15}{2,10}{3,10}\n",
                                sEvent.DamageType,
                                sEvent.DamageType == ActionType.Weaponskill ? sEvent.WeaponskillName : sEvent.DamageModifier.ToString(),
                                sEvent.UsedHide ? "+Hide" : "",
                                sEvent.DamageAmount);

                            AppendNormalText(dataLine);
                        }

                        int totalDmg = SATAList.Sum(s => s.DamageAmount);
                        double avgDmg = (double)totalDmg / SATAList.Count();

                        AppendNormalText(string.Format("\n    {0,6}{1,44}\n",
                            "Total:",
                            totalDmg));
                        AppendNormalText(string.Format("    {0,8}{1,42:f2}\n\n",
                            "Average:",
                            avgDmg));
                    }

                    if (SAList.Count() > 0)
                    {
                        AppendBoldText("  SA\n", Color.Blue);
                        foreach (var sEvent in SAList)
                        {
                            string dataLine = string.Format("    {0,-15}{1,15}{2,10}{3,10}\n",
                                sEvent.DamageType,
                                sEvent.DamageType == ActionType.Weaponskill ? sEvent.WeaponskillName : sEvent.DamageModifier.ToString(),
                                sEvent.UsedHide ? "+Hide" : "",
                                sEvent.DamageAmount);

                            AppendNormalText(dataLine);
                        }

                        int totalDmg = SAList.Sum(s => s.DamageAmount);
                        double avgDmg = (double)totalDmg / SAList.Count();

                        AppendNormalText(string.Format("\n    {0,6}{1,44}\n",
                            "Total:",
                            totalDmg));
                        AppendNormalText(string.Format("    {0,8}{1,42:f2}\n\n",
                            "Average:",
                            avgDmg));
                    }

                    if (TAList.Count() > 0)
                    {
                        AppendBoldText("  TA\n", Color.Blue);
                        foreach (var sEvent in TAList)
                        {
                            string dataLine = string.Format("    {0,-15}{1,15}{2,10}{3,10}\n",
                                sEvent.DamageType,
                                sEvent.DamageType == ActionType.Weaponskill ? sEvent.WeaponskillName : sEvent.DamageModifier.ToString(),
                                sEvent.UsedHide ? "+Hide" : "",
                                sEvent.DamageAmount);

                            AppendNormalText(dataLine);
                        }

                        int totalDmg = TAList.Sum(s => s.DamageAmount);
                        double avgDmg = (double)totalDmg / TAList.Count();

                        AppendNormalText(string.Format("\n    {0,6}{1,44}\n",
                            "Total:",
                            totalDmg));
                        AppendNormalText(string.Format("    {0,8}{1,42:f2}\n\n",
                            "Average:",
                            avgDmg));
                    }
                }

            }

        }

        #endregion

        #region Event Handlers
        protected override void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(DatabaseManager.Instance.Database);

            flagNoUpdate = false;
        }

        protected override void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(DatabaseManager.Instance.Database);

            flagNoUpdate = false;
        }

        protected override void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
            {
                checkBox1Changed = true;
                HandleDataset(DatabaseManager.Instance.Database);
            }

            flagNoUpdate = false;
        }

        protected override void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(DatabaseManager.Instance.Database);

            flagNoUpdate = false;
        }
        #endregion

    }
}
