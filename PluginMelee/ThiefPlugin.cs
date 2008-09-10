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

            internal ActionType ActionType { get; set; }
            internal string ActionName { get; set; }
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
                SelectAllMobs(dataSet, playerList.ToArray());
            else if (checkBox1.Checked == true)
                SelectMobGroup(dataSet, playerList.ToArray(), selectedMob);
            else
                SelectBattle(dataSet, playerList.ToArray(), selectedMob);
        }

        /// <summary>
        /// Process all mobs
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="selectedPlayers"></param>
        private void SelectAllMobs(KPDatabaseDataSet dataSet, string[] selectedPlayers)
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
        private void SelectMobGroup(KPDatabaseDataSet dataSet, string[] selectedPlayers, string selectedMob)
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
        private void SelectBattle(KPDatabaseDataSet dataSet, string[] selectedPlayers, string selectedMob)
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
                    List<KPDatabaseDataSet.InteractionsRow> sataWeaponskills = new List<KPDatabaseDataSet.InteractionsRow>();

                    AppendBoldText(player.Name + "\n", Color.Red);

                    SATAEvents.Clear();
                    sataActions = sataActions.OrderBy(a => a.InteractionID);

                    var avgNonCritSet = player.Melee.Where(m => ((DefenseType)m.DefenseType == DefenseType.None) &&
                        ((DamageModifier)m.DamageModifier == DamageModifier.None));

                    double avgNonCrit = 0;
                    if (avgNonCritSet.Count() > 0)
                        avgNonCrit = avgNonCritSet.Average(m => m.Amount);

                    double critThreshold = avgNonCrit * 5;
                    double nonCritThreshold = avgNonCrit * 3;

                    while (sataActions.Count() > 0)
                    {
                        var firstAction = sataActions.First();
                        sataActions = sataActions.Skip(1);

                        SATATypes firstActionType = GetSATAType(firstAction.ActionsRow.ActionName);

                        SATAEvent sataEvent = new SATAEvent();
                        sataEvent.SATAActions = new HashSet<SATATypes>();
                        SATAEvents.Add(sataEvent);

                        sataEvent.SATAActions.Add(firstActionType);

                        var nextMelee = player.Melee.FirstOrDefault(m => m.Timestamp >= firstAction.Timestamp &&
                            m.Timestamp <= firstAction.Timestamp.AddMinutes(1));
                        var nextWS = player.WSkill.FirstOrDefault(w => w.Timestamp >= firstAction.Timestamp &&
                            w.Timestamp <= firstAction.Timestamp.AddMinutes(1));

                        KPDatabaseDataSet.InteractionsRow sataDamage = null;


                        // First check if there are valid attacks following the JA use.
                        // If not, continue the loop.  If there are no valid attacks of
                        // one of the types (melee or weaponskill), use the other by default.
                        if ((nextMelee == null) && (nextWS == null))
                        {
                            sataEvent.SATASuccess = false;
                            sataEvent.ActionType = ActionType.Unknown;
                            sataEvent.ActionName = "Failed";
                            continue;
                        }
                        else if (nextMelee == null)
                        {
                            sataDamage = nextWS;
                            sataEvent.SATASuccess = true;
                        }

                        if (sataDamage != null)
                        {
                            if (sataDamage.Timestamp >= firstAction.Timestamp.AddMinutes(1))
                            {
                                sataEvent.SATASuccess = false;
                                continue;
                            }
                        }

                        // If no attack has been selected, check for abnormally high values
                        // in the next melee attack (crit or non-crit).  If present, indicates
                        // a successeful JA hit.
                        if (sataDamage == null)
                        {
                            if ((DamageModifier)nextMelee.DamageModifier == DamageModifier.Critical)
                            {
                                if (nextMelee.Amount > critThreshold)
                                {
                                    sataDamage = nextMelee;
                                    sataEvent.SATASuccess = true;
                                }
                            }
                            else
                            {
                                if (nextMelee.Amount > nonCritThreshold)
                                {
                                    sataDamage = nextMelee;
                                    sataEvent.SATASuccess = true;
                                }
                            }
                        }

                        // If no entry yet selected, recheck next melee attack by forcing it
                        // to check for > firstAction.Timestamp to avoid entries where logged
                        // attacks were out of order (JA use showed up before previous attack
                        // round).
                        if (sataDamage == null)
                        {
                            nextMelee = player.Melee.FirstOrDefault(m => m.Timestamp > firstAction.Timestamp);
                            if (nextMelee != null)
                            {
                                if ((DamageModifier)nextMelee.DamageModifier == DamageModifier.Critical)
                                {
                                    if (nextMelee.Amount > critThreshold)
                                    {
                                        sataDamage = nextMelee;
                                        sataEvent.SATASuccess = true;
                                    }
                                }
                                else
                                {
                                    if (nextMelee.Amount > nonCritThreshold)
                                    {
                                        sataDamage = nextMelee;
                                        sataEvent.SATASuccess = true;
                                    }
                                }
                            }
                        }

                        // Haven't found a melee hit to match up with this yet, so next
                        // check for weaponskills in the period between JA use and melee.
                        if (sataDamage == null)
                        {
                            if (nextWS != null)
                            {
                                if (nextMelee != null)
                                {
                                    if (nextWS.Timestamp <= nextMelee.Timestamp)
                                    {
                                        sataDamage = nextWS;
                                        sataEvent.SATASuccess = true;
                                    }
                                }
                                else
                                {
                                    sataDamage = nextWS;
                                    sataEvent.SATASuccess = true;
                                }
                            }
                        }

                        // No exception crits or normal hits, and no weaponskills.  Must
                        // be a miss, if there's a melee hit within the time limit.
                        if (sataDamage == null)
                        {
                            if (nextMelee != null)
                            {
                                sataDamage = nextMelee;
                                sataEvent.SATASuccess = false;
                            }
                        }

                        if (sataDamage == null)
                        {
                            sataEvent.SATASuccess = false;
                            sataEvent.ActionType = ActionType.Unknown;
                            sataEvent.ActionName = "Failed";
                        }
                        else
                        {

                            sataEvent.DamageTimestamp = sataDamage.Timestamp;
                            sataEvent.ActionType = (ActionType)sataDamage.ActionType;
                            if ((ActionType)sataDamage.ActionType == ActionType.Melee)
                            {
                                sataEvent.DamageModifier = (DamageModifier)sataDamage.DamageModifier;
                            }
                            else if ((ActionType)sataDamage.ActionType == ActionType.Weaponskill)
                            {
                                sataEvent.WeaponskillName = sataDamage.ActionsRow.ActionName;
                                sataWeaponskills.Add(sataDamage);
                            }
                            sataEvent.DamageAmount = sataDamage.Amount;


                            while (sataActions.Count() > 0)
                            {
                                var nextAction = sataActions.First();

                                if ((nextAction.Timestamp <= sataDamage.Timestamp) ||
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
                            else if (sataEvent.SATAActions.Intersect(SATASet).Count() == 0)
                            {
                                sataEvent.SATASuccess = false;
                            }

                            sataEvent.ActionName = sataEvent.ActionType.ToString();
                        }
                    }

                    // Finished building event list

                    // Now try to display data

                    var SATAList = SATAEvents.Where(s => //s.SATASuccess == true &&
                        s.SATAActions.IsSupersetOf(SATASet));

                    var SAList = SATAEvents.Where(s => //s.SATASuccess == true &&
                         s.SATAActions.IsSupersetOf(SASet)).Except(SATAList);

                    var TAList = SATAEvents.Where(s => //s.SATASuccess == true &&
                         s.SATAActions.IsSupersetOf(TASet)).Except(SATAList);

                    PrintOutput("Sneak Attack + Trick Attack", SATAList);
                    PrintOutput("Sneak Attack", SAList);
                    PrintOutput("Trick Attack", TAList);

                    var soloWeaponskills = from w in player.WSkill.Except(sataWeaponskills)
                                           select new SATAEvent
                                           {
                                               ActionName = "Weaponskill",
                                               DamageAmount = w.Amount,
                                               ActionType = ActionType.Weaponskill,
                                               WeaponskillName = w.ActionsRow.ActionName
                                           };

                    PrintOutput("Solo Weaponskills", soloWeaponskills);
                }
            }
        }

        private void PrintOutput(string title, IEnumerable<SATAEvent> SATAList)
        {

            string dataLine;

            if (SATAList.Count() > 0)
            {
                AppendBoldText("  " + title + "\n", Color.Blue);

                foreach (var sEvent in SATAList)
                {
                    if (sEvent.ActionType == ActionType.Unknown)
                    {
                        dataLine = string.Format("    {0,-15}\n",
                            sEvent.ActionName);
                    }
                    else
                    {
                        dataLine = string.Format("    {0,-15}{1,15}{2,10}{3,10}\n",
                            sEvent.ActionName,
                            sEvent.ActionType == ActionType.Weaponskill ? sEvent.WeaponskillName
                            : (sEvent.SATASuccess == true ? sEvent.DamageModifier.ToString() : "Miss"),
                            sEvent.UsedHide ? "+Hide" : "",
                            sEvent.DamageAmount);
                    }

                    AppendNormalText(dataLine);
                }

                // All
                var meleeDmgList = SATAList.Where(s => s.ActionType == ActionType.Melee);
                var wsDmgList = SATAList.Where(s => s.ActionType == ActionType.Weaponskill);

                int totalMeleeDmg = meleeDmgList.Sum(s => s.DamageAmount);
                int totalWSDmg = wsDmgList.Sum(s => s.DamageAmount);

                int totalDmg = totalMeleeDmg + totalWSDmg;

                double avgMeleeDmg = meleeDmgList.Count() > 0 ?
                    (double)totalMeleeDmg / meleeDmgList.Count() : 0;
                double avgWSDmg = wsDmgList.Count() > 0 ?
                    (double)totalWSDmg / wsDmgList.Count() : 0;

                // Only successful
                int successCount = SATAList.Where(s => s.SATASuccess == true).Count();

                var smeleeDmgList = SATAList.Where(s => s.ActionType == ActionType.Melee &&
                    s.SATASuccess == true);

                int totalSMeleeDmg = smeleeDmgList.Sum(s => s.DamageAmount);

                int totalSDmg = totalSMeleeDmg + totalWSDmg;

                double avgSMeleeDmg = smeleeDmgList.Count() > 0 ?
                    (double)totalSMeleeDmg / smeleeDmgList.Count() : 0;


                AppendNormalText(string.Format("\n    {0,-20}{1,10}{2,20}\n",
                    "Total:",
                    SATAList.Count(),
                    totalDmg));
                AppendNormalText(string.Format("    {0,-20}{1,30}\n",
                    "Successful Total:",
                    totalSDmg));
                AppendNormalText(string.Format("    {0,-20}{1,10}{2,20:p2}\n\n",
                    "Success Count:",
                     successCount,
                     (double)successCount / SATAList.Count()));

                AppendNormalText(string.Format("    {0,30}{1,20}\n", "Count", "Average"));
                AppendNormalText(string.Format("    {0,-20}{1,10}{2,20:f2}\n",
                    "Melee:",
                    meleeDmgList.Count(),
                    avgMeleeDmg));
                AppendNormalText(string.Format("    {0,-20}{1,10}{2,20:f2}\n",
                    "Successful Melee:",
                    smeleeDmgList.Count(),
                    avgSMeleeDmg));
                AppendNormalText(string.Format("    {0,-20}{1,10}{2,20:f2}\n\n\n",
                    "Weaponskill:",
                     wsDmgList.Count(),
                    avgWSDmg));
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
