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
    public class ThiefPlugin : BasePluginControl
    {
        #region SATA support classes and functions
        private enum SATATypes
        {
            None,
            SneakAttack,
            TrickAttack,
            Hide
        }

        private class SATAEvent
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

        private SATATypes GetSATAType(string actionName)
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
        bool groupMobs = false;
        bool exclude0XPMobs = false;
        bool flagNoUpdate = false;
        bool customMobSelection = false;

        HashSet<SATATypes> SASet = new HashSet<SATATypes> { SATATypes.SneakAttack };
        HashSet<SATATypes> TASet = new HashSet<SATATypes> { SATATypes.TrickAttack };
        HashSet<SATATypes> SATASet = new HashSet<SATATypes> { SATATypes.SneakAttack, SATATypes.TrickAttack };

        List<SATAEvent> SATAEvents = new List<SATAEvent>();

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

        string lsParsedSneakAttack;
        string lsParsedTrickAttack;
        string lsParsedHide;

        string lsSneakAttack;
        string lsTrickAttack;
        string lsSneakAndTrick;
        string lsSoloWS;

        string lsWeaponskill;
        string lsFailed;
        string lsMiss;
        string lsAndHide;

        string lsFormatDataLineShort;
        string lsFormatDataLineLong;

        string lsLabelTotal;
        string lsLabelSuccessTotal;
        string lsLabelSuccessCount;
        string lsLabelMelee;
        string lsLabelSuccessMelee;
        string lsLabelWeaponskill;
        string lsLabelCount;
        string lsLabelAverage;

        string lsFormatSummaryP;
        string lsFormatSummary1;
        string lsFormatSummary2;
        string lsFormatSummary3;
        #endregion

        #region Constructor
        public ThiefPlugin()
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

            playersCombo.Items.Clear();
            playersCombo.Items.Add(lsAll);
            flagNoUpdate = true;
            playersCombo.SelectedIndex = 0;

            mobsCombo.Items.Clear();
            mobsCombo.Items.Add(lsAll);
            flagNoUpdate = true;
            mobsCombo.SelectedIndex = 0;
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

            if (e.DatasetChanges.Combatants != null)
            {
                if (e.DatasetChanges.Combatants.Any(x => x.RowState == DataRowState.Added))
                {
                    UpdatePlayerList();
                    changesFound = true;

                    flagNoUpdate = true;
                    playersCombo.CBSelectIndex(0);
                }
            }

            // Check for new mobs being fought.  If any exist, update the Mob Group dropdown list.
            if (e.DatasetChanges.Battles != null)
            {
                if (e.DatasetChanges.Battles.Any(x => x.RowState == DataRowState.Added))
                {
                    UpdateMobList(true);
                    changesFound = true;

                    flagNoUpdate = true;
                    mobsCombo.CBSelectIndex(-1);
                }
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

                if (iRows.Count() > 0)
                {
                    DateTime initialTime = iRows.First().Timestamp - TimeSpan.FromSeconds(70);
                    DateTime endTime = iRows.Last().Timestamp;

                    var dSet = dataSet.Battles.GetDefaultBattle().GetInteractionsRows()
                        .Where(i => i.Timestamp >= initialTime && i.Timestamp <= endTime);

                    iRows = iRows.Concat(dSet);
                }

                attackSet = from c in iRows
                            where (c.IsActorIDNull() == false) &&
                                  (selectedPlayers.Contains(c.CombatantsRowByActorCombatantRelation.CombatantName))
                            group c by c.CombatantsRowByActorCombatantRelation into ca
                            orderby ca.Key.CombatantType, ca.Key.CombatantName
                            select new AttackGroup
                            {
                                Name = ca.Key.CombatantNameOrJobName,
                                ComType = (EntityType)ca.Key.CombatantType,
                                Melee = from q in ca
                                        where ((ActionType)q.ActionType == ActionType.Melee &&
                                               ((HarmType)q.HarmType == HarmType.Damage ||
                                                (HarmType)q.HarmType == HarmType.Drain))
                                        orderby q.Timestamp
                                        select q,
                                Ability = from q in ca
                                          where ((ActionType)q.ActionType == ActionType.Ability &&
                                                 (AidType)q.AidType == AidType.Enhance &&
                                                 q.Preparing == false)
                                          select q,
                                WSkill = from q in ca
                                         where ((ActionType)q.ActionType == ActionType.Weaponskill &&
                                               ((HarmType)q.HarmType == HarmType.Damage ||
                                                (HarmType)q.HarmType == HarmType.Drain) &&
                                                q.Preparing == false)
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
                                                n.Preparing == false)
                                         select n,
                            };
            }

            ProcessAttackSet(attackSet);

        }

        /// <summary>
        /// Process the attack set generated by the mob collection functions
        /// </summary>
        /// <param name="attackSet"></param>
        private void ProcessAttackSet(IEnumerable<AttackGroup> attackSet)
        {
            foreach (var player in attackSet)
            {
                var sataActions = player.Ability.Where(
                        a => a.IsActionIDNull() == false &&
                        (a.ActionsRow.ActionName == lsParsedSneakAttack ||
                         a.ActionsRow.ActionName == lsParsedTrickAttack||
                         a.ActionsRow.ActionName == lsParsedHide));

                if (sataActions.Count() > 0)
                {
                    List<KPDatabaseDataSet.InteractionsRow> sataWeaponskills = new List<KPDatabaseDataSet.InteractionsRow>();

                    AppendText(player.Name + "\n", Color.Red, true, false);

                    SATAEvents.Clear();
                    sataActions = sataActions.OrderBy(a => a.InteractionID);

                    var avgNonCritSet = player.Melee.Where(m => ((DefenseType)m.DefenseType == DefenseType.None) &&
                        ((DamageModifier)m.DamageModifier == DamageModifier.None));

                    double avgNonCrit = 0;
                    if (avgNonCritSet.Count() > 0)
                    {
                        // Limit the average calculation to the first 200 hits
                        var avgNonCritSubset = avgNonCritSet.Take(200);
                        avgNonCrit = avgNonCritSubset.Average(m => m.Amount);
                    }

                    double critThreshold = avgNonCrit * 4;
                    double nonCritThreshold = avgNonCrit * 2.75;

                    while (sataActions.Count() > 0)
                    {
                        var firstAction = sataActions.First();
                        sataActions = sataActions.Skip(1);

                        SATATypes firstActionType = GetSATAType(firstAction.ActionsRow.ActionName);

                        SATAEvent sataEvent = new SATAEvent();
                        sataEvent.SATAActions = new HashSet<SATATypes>();
                        SATAEvents.Add(sataEvent);

                        sataEvent.SATAActions.Add(firstActionType);

                        DateTime preTime = firstAction.Timestamp.AddSeconds(-4);
                        DateTime postTime = firstAction.Timestamp.AddSeconds(4);
                        DateTime minutePost = firstAction.Timestamp.AddMinutes(1);


                        var nextMelee = player.Melee.FirstOrDefault(m => m.Timestamp >= firstAction.Timestamp &&
                            m.Timestamp <= firstAction.Timestamp.AddMinutes(1));
                        var nextWS = player.WSkill.FirstOrDefault(w => w.Timestamp >= firstAction.Timestamp &&
                            w.Timestamp <= firstAction.Timestamp.AddMinutes(1));


                        KPDatabaseDataSet.InteractionsRow sataDamage = null;


                        // First check next melee hit for abnormally high values.
                        // If present, indicates a successeful JA hit.
                        if (sataDamage == null)
                        {
                            if (nextMelee != null)
                            {
                                if ((DamageModifier)nextMelee.DamageModifier == DamageModifier.Critical)
                                {
                                    if (nextMelee.Amount > critThreshold)
                                    {
                                        sataDamage = nextMelee;
                                        sataEvent.ActionType = ActionType.Melee;
                                        sataEvent.SATASuccess = true;
                                    }
                                }
                                else
                                {
                                    if (firstActionType == SATATypes.TrickAttack)
                                    {
                                        if (nextMelee.Amount > nonCritThreshold)
                                        {
                                            sataDamage = nextMelee;
                                            sataEvent.ActionType = ActionType.Melee;
                                            sataEvent.SATASuccess = true;
                                        }
                                    }
                                }
                            }
                        }

                        // If it's not the next melee hit, check all nearby melee hits
                        // in case of out-of-order text.
                        if (sataDamage == null)
                        {
                            var nearMelee = player.Melee.Where(m => m.Timestamp >= preTime &&
                                m.Timestamp <= postTime);

                            var beforeMelee = nearMelee.TakeWhile(m => m.Timestamp < firstAction.Timestamp);
                            var afterMelee = nearMelee.SkipWhile(m => m.Timestamp < firstAction.Timestamp);

                            if (afterMelee.Any(m => ((DamageModifier)m.DamageModifier == DamageModifier.Critical
                                && m.Amount > critThreshold) || (m.Amount > nonCritThreshold)))
                            {

                                sataDamage = afterMelee.First(m =>
                                                          ((DamageModifier)m.DamageModifier == DamageModifier.Critical
                                                             && m.Amount > critThreshold)
                                                           || (m.Amount > nonCritThreshold));

                                sataEvent.ActionType = ActionType.Melee;
                                sataEvent.SATASuccess = true;
                            }
                            else if (beforeMelee.Any(m => ((DamageModifier)m.DamageModifier == DamageModifier.Critical
                                && m.Amount > critThreshold) || (m.Amount > nonCritThreshold)))
                            {
                                sataDamage = beforeMelee.First(m =>
                                                          ((DamageModifier)m.DamageModifier == DamageModifier.Critical
                                                             && m.Amount > critThreshold)
                                                           || (m.Amount > nonCritThreshold));
                            }
                        }


                        // If it's not a melee hit, check for nearby weaponskills
                        if (sataDamage == null)
                        {
                            var nearWS = player.WSkill.Where(m => m.Timestamp >= firstAction.Timestamp.AddSeconds(-4) &&
                                m.Timestamp <= firstAction.Timestamp.AddSeconds(4));

                            if (nearWS.Count() > 0)
                            {
                                sataEvent.ActionType = ActionType.Weaponskill;
                                sataEvent.SATASuccess = true;

                                if (nearWS.Any(w => w.Timestamp >= firstAction.Timestamp))
                                {
                                    sataDamage = nearWS.Where(w => w.Timestamp >= firstAction.Timestamp).First();
                                }
                                else
                                {
                                    sataDamage = nearWS.First();
                                }
                            }
                        }

                        // If no nearby weaponskills, look for weaponskill within the next minute
                        if (sataDamage == null)
                        {
                            if (nextWS != null)
                            {
                                // But only if we have a melee action to check against
                                if (nextMelee != null)
                                {
                                    if ((nextWS.Timestamp < nextMelee.Timestamp) ||
                                        (nextMelee.Timestamp == firstAction.Timestamp))
                                    {
                                        sataDamage = nextWS;
                                        sataEvent.ActionType = ActionType.Weaponskill;
                                        sataEvent.SATASuccess = true;
                                    }
                                }
                                else
                                {
                                    // If no melee, and we have a weaponskill within a minute,
                                    // use the weaponskill
                                    sataDamage = nextWS;
                                    sataEvent.ActionType = ActionType.Weaponskill;
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
                            sataEvent.ActionName = lsFailed;
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

                                    if ((nextAction.ActionsRow.ActionName == lsParsedHide) &&
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

                    var SATAList = SATAEvents.Where(s =>
                        s.SATAActions.IsSupersetOf(SATASet));

                    var SAList = SATAEvents.Where(s =>
                         s.SATAActions.IsSupersetOf(SASet)).Except(SATAList);

                    var TAList = SATAEvents.Where(s =>
                         s.SATAActions.IsSupersetOf(TASet)).Except(SATAList);

                    PrintOutput(lsSneakAndTrick, SATAList);
                    PrintOutput(lsSneakAttack, SAList);
                    PrintOutput(lsTrickAttack, TAList);

                    var soloWeaponskills = from w in player.WSkill.Except(sataWeaponskills)
                                           select new SATAEvent
                                           {
                                               ActionName = lsWeaponskill,
                                               DamageAmount = w.Amount,
                                               ActionType = ActionType.Weaponskill,
                                               WeaponskillName = w.ActionsRow.ActionName
                                           };

                    PrintOutput(lsSoloWS, soloWeaponskills);

                }
            }
        }

        private void PrintOutput(string title, IEnumerable<SATAEvent> SATAList)
        {

            string dataLine;

            if (SATAList.Count() > 0)
            {
                AppendText("  " + title + "\n", Color.Blue, true, false);

                foreach (var sEvent in SATAList)
                {
                    if (sEvent.ActionType == ActionType.Unknown)
                    {
                        dataLine = string.Format(lsFormatDataLineShort,
                            sEvent.ActionName);
                    }
                    else
                    {
                        dataLine = string.Format(lsFormatDataLineLong,
                            sEvent.ActionName,
                            sEvent.ActionType == ActionType.Weaponskill ? sEvent.WeaponskillName
                            : (sEvent.SATASuccess == true ? sEvent.DamageModifier.ToString() : lsMiss),
                            sEvent.UsedHide ? lsAndHide : string.Empty,
                            sEvent.DamageAmount);
                    }

                    AppendText(dataLine + "\n");
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


                /////////////////////////////

                StringBuilder sb = new StringBuilder();

                sb.Append("\n");
                sb.AppendFormat(lsFormatSummary1,
                    lsLabelTotal,
                    SATAList.Count(),
                    totalDmg);
                sb.Append("\n");

                sb.AppendFormat(lsFormatSummary2,
                    lsLabelSuccessTotal,
                    totalSDmg);
                sb.Append("\n");

                sb.AppendFormat(lsFormatSummaryP,
                    lsLabelSuccessCount,
                     successCount,
                     (double)successCount / SATAList.Count());
                sb.Append("\n\n");

                sb.AppendFormat(lsFormatSummary3,
                    lsLabelCount,
                    lsLabelAverage);
                sb.Append("\n");

                sb.AppendFormat(lsFormatSummaryP,
                    lsLabelMelee,
                    meleeDmgList.Count(),
                    avgMeleeDmg);
                sb.Append("\n");

                sb.AppendFormat(lsFormatSummaryP,
                    lsLabelSuccessMelee,
                    smeleeDmgList.Count(),
                    avgSMeleeDmg);
                sb.Append("\n");

                sb.AppendFormat(lsFormatSummaryP,
                    lsLabelWeaponskill,
                     wsDmgList.Count(),
                    avgWSDmg);
                sb.Append("\n");

                sb.Append("\n\n");


                AppendText(sb.ToString());
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
            this.tabName = Resources.Combat.ThiefPluginTabName;

            lsAll = Resources.PublicResources.All;

            lsParsedSneakAttack = Resources.ParsedStrings.SneakAttack;
            lsParsedTrickAttack = Resources.ParsedStrings.TrickAttack;
            lsParsedHide = Resources.ParsedStrings.Hide;

            lsSneakAttack = Resources.Combat.ThiefPluginSneakAttack;
            lsTrickAttack = Resources.Combat.ThiefPluginTrickAttack;
            lsSneakAndTrick = Resources.Combat.ThiefPluginSneakAndTrick;
            lsSoloWS = Resources.Combat.ThiefPluginSoloWS;

            lsWeaponskill = Resources.Combat.ThiefPluginEventTypeWeaponskill;
            lsFailed = Resources.Combat.ThiefPluginEventTypeFailed;
            lsMiss = Resources.Combat.ThiefPluginMiss;
            lsAndHide = Resources.Combat.ThiefPluginAndHide;

            lsFormatDataLineShort = Resources.Combat.ThiefPluginFormatDataLineShort;
            lsFormatDataLineLong = Resources.Combat.ThiefPluginFormatDataLineLong;

            lsLabelTotal = Resources.Combat.ThiefPluginLabelTotal;
            lsLabelSuccessTotal = Resources.Combat.ThiefPluginLabelSuccessfulTotal;
            lsLabelSuccessCount = Resources.Combat.ThiefPluginLabelSuccessCount;
            lsLabelMelee = Resources.Combat.ThiefPluginLabelMelee;
            lsLabelSuccessMelee = Resources.Combat.ThiefPluginLabelSuccessfulMelee;
            lsLabelWeaponskill = Resources.Combat.ThiefPluginLabelWeaponskills;
            lsLabelCount = Resources.Combat.ThiefPluginLabelCount;
            lsLabelAverage = Resources.Combat.ThiefPluginLabelAverage;

            lsFormatSummaryP = Resources.Combat.ThiefPluginFormatSummaryP;
            lsFormatSummary1 = Resources.Combat.ThiefPluginFormatSummary1;
            lsFormatSummary2 = Resources.Combat.ThiefPluginFormatSummary2;
            lsFormatSummary3 = Resources.Combat.ThiefPluginFormatSummary3;
        }
        #endregion

    }
}
