using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WaywardGamers.KParser;

namespace WaywardGamers.KParser.Plugin
{
    public class ExtraAttacksPlugin : BasePluginControl
    {
        #region Member Variables
        bool flagNoUpdate;
        bool showDetails = false;
        ToolStripLabel playersLabel = new ToolStripLabel();
        ToolStripComboBox playersCombo = new ToolStripComboBox();
        ToolStripLabel attLabel = new ToolStripLabel();
        ToolStripComboBox attacksCombo = new ToolStripComboBox();
        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();
        ToolStripMenuItem showDetailOption = new ToolStripMenuItem();

        ToolStripMenuItem alternateProcessOption = new ToolStripMenuItem();
        bool alternateProcessMode = true;

        // Localized strings

        string lsRestrictedWarning;

        string lsAll;
        string lsYes;
        string lsNo;

        string lsMainSectionTitle;
        string lsMainHeader1;
        string lsMainHeader2;
        string lsMainHeader3;
        string lsMainFormat1;
        string lsMainFormat2;
        string lsMainFormat3;

        string lsSectionTreatAs;

        string lsSectionMultiAttacksTitle;
        string lsSectionMultiAttacksHeader;
        string lsSectionMultiAttacksFormat;

        string lsSectionKicksTitle;
        string lsSectionKicksHeader;
        string lsSectionKicksFormat;

        string lsSectionZanshinTitle;
        string lsSectionZanshinHeader;
        string lsSectionZanshinFormat;
        string lsSectionZanshinAccHeader;
        string lsSectionZanshinAccFormat;

        string lsSectionUncorrectedDetails;
        #endregion

        #region Constructor
        public ExtraAttacksPlugin()
        {
            LoadLocalizedUI();

            playersCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            playersCombo.MaxDropDownItems = 10;
            playersCombo.SelectedIndexChanged += new EventHandler(this.playersCombo_SelectedIndexChanged);

            attacksCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            attacksCombo.SelectedIndexChanged += new EventHandler(this.attacksCombo_SelectedIndexChanged);

            showDetailOption.CheckOnClick = true;
            showDetailOption.Checked = false;
            showDetailOption.Click += new EventHandler(showDetailOption_Click);

            alternateProcessOption.CheckOnClick = true;
            alternateProcessOption.Checked = true;
            alternateProcessOption.Click += new EventHandler(alternateProcessOption_Click);

            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;
            optionsMenu.DropDownItems.Add(showDetailOption);

            // Only add the option to turn off the new method in Debug mode, for comparative purposes.
#if DEBUG
            optionsMenu.DropDownItems.Add(alternateProcessOption);
#endif

            toolStrip.Items.Add(playersLabel);
            toolStrip.Items.Add(playersCombo);
            toolStrip.Items.Add(attLabel);
            toolStrip.Items.Add(attacksCombo);
            toolStrip.Items.Add(optionsMenu);

        }
        #endregion

        #region IPlugin Overrides
        public override void Reset()
        {
            ResetTextBox();
            showDetailOption.Checked = false;
            showDetails = false;
        }

        public override void NotifyOfUpdate()
        {
            UpdatePlayerList();
            showDetailOption.Checked = false;
            showDetails = false;

            playersCombo.CBSelectIndex(0);
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            ResetTextBox();
            AppendText(lsRestrictedWarning, Color.Black, true, false);

            //string currentlySelectedPlayer = lsAll;

            //if (playersCombo.CBSelectedIndex() > 0)
            //    currentlySelectedPlayer = playersCombo.CBSelectedItem();

            //if ((e.DatasetChanges.Combatants != null) &&
            //    (e.DatasetChanges.Combatants.Count > 0))
            //{
            //    UpdatePlayerList(e.Dataset);

            //    flagNoUpdate = true;
            //    playersCombo.CBSelectItem(currentlySelectedPlayer);
            //}

            //if (e.DatasetChanges.Interactions != null)
            //{
            //    if (e.DatasetChanges.Interactions.Count != 0)
            //    {
            //        datasetToUse = e.Dataset;
            //        return true;
            //    }
            //}
        }
        #endregion

        #region Private Methods
        private void UpdatePlayerList()
        {
            playersCombo.CBReset();
            playersCombo.CBAddStrings(GetPlayerListing());
        }
        #endregion

        #region Processing sections
        private class AttackCalculations
        {
            internal string Name { get; set; }
            internal int Attacks { get; set; }
            internal int Rounds { get; set; }
            internal int AttacksPerRound { get; set; }
            internal int ExtraAttacks { get; set; }
            internal int RoundsWithExtraAttacks { get; set; }
            internal int Plus1Rounds { get; set; }
            internal int Plus2Rounds { get; set; }
            internal int Plus3Rounds { get; set; }
            internal int Plus4Rounds { get; set; }
            internal int PlusNRounds { get; set; }
            internal int TotalMultiRounds { get; set; }
            internal int Minus1Rounds { get; set; }
            internal int AttackRoundCountKills { get; set; }
            internal int AttackRoundUnderCountKills { get; set; }
            internal int AttackRoundsNonKill { get; set; }
            internal int MissedFirstAttacks { get; set; }
            internal int PossibleZanshin { get; set; }
            internal double FirstAttackAcc { get; set; }
            internal double SecondAttackAcc { get; set; }
        }

        private DateTime ClosestTimestamp(DateTime timestamp, List<DateTime> timestampList)
        {
            int index = timestampList.BinarySearch(timestamp);

            if (index >= 0)
                return timestampList[index];
            else
                return timestampList[~index - 1];
        }

        private Dictionary<string, List<DateTime>> GetInitialDictionary(List<string> playersList)
        {
            Dictionary<string, List<DateTime>> timestampLists = new Dictionary<string, List<DateTime>>();

            foreach (string player in playersList)
            {
                timestampLists.Add(player, new List<DateTime>());
            }

            return timestampLists;
        }

        private void FillTimestampList(List<DateTime> timestampList, IEnumerable<KPDatabaseDataSet.InteractionsRow> meleeRows)
        {
            if (meleeRows.Count() == 0)
                throw new InvalidOperationException();

            if (meleeRows.Count() == 1)
            {
                timestampList.Add(meleeRows.First().Timestamp);
                return;
            }

            double sumTimeDiffs = 0;
            int countTimeDiffs = 0;
            DateTime thistime;
            DateTime lasttime = meleeRows.First().Timestamp;
            double timeDiff;

            foreach (var round in meleeRows.Where((a,index) => index > 0))
            {
                // Get the current interval since the previous baseline timestamp
                thistime = round.Timestamp;
                timeDiff = (thistime - lasttime).TotalSeconds;

                // If we're at least 2 seconds past the previous entry, this is a valid interval
                // and we can update the lasttime value
                if (timeDiff > 2)
                {
                    lasttime = thistime;

                    // Ignore inordinately long periods as possible breaks between fights, etc.
                    // Otherwise update the sum and count values.
                    if (timeDiff < 20)
                    {
                        sumTimeDiffs += timeDiff;
                        countTimeDiffs++;
                    }
                }
            }

            // Find the average, and set the threshold at 2/3 that value.
            double timeDiffThreshold = 0;
            if (countTimeDiffs > 0)
                timeDiffThreshold = (sumTimeDiffs / countTimeDiffs) * 2 / 3;

            if (timeDiffThreshold < 2)
                timeDiffThreshold = 2;

            DateTime lastTS;

            foreach (var melee in meleeRows)
            {
                if (timestampList.Count == 0)
                {
                    timestampList.Add(melee.Timestamp);
                    continue;
                }

                lastTS = timestampList.LastOrDefault(t =>
                    t <= melee.Timestamp &&
                    t.AddSeconds(timeDiffThreshold) >= melee.Timestamp);

                if ((lastTS == null) || (lastTS == DateTime.MinValue))
                    timestampList.Add(melee.Timestamp);
            }

            timestampList.Sort();
        }

        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            if (alternateProcessMode)
            {
                ProcessDataAlternateMode(dataSet);
                return;
            }

            ResetTextBox();

            string playerFilter = playersCombo.CBSelectedItem();
            List<string> playersList = new List<string>();

            if (playerFilter == lsAll)
            {
                string[] players = playersCombo.CBGetStrings();

                foreach (string player in players)
                {
                    if (player != lsAll)
                    {
                        playersList.Add(player);
                    }
                }
            }
            else
            {
                playersList.Add(playerFilter);
            }

            Dictionary<string, List<DateTime>> timestampLists = GetInitialDictionary(playersList);

            if (timestampLists.Count == 0)
                return;

            // Setup work for calculating attack round timing
            var simpleAttackList = from c in dataSet.Combatants
                                   where (((EntityType)c.CombatantType == EntityType.Player) ||
                                         ((EntityType)c.CombatantType == EntityType.Pet) ||
                                         ((EntityType)c.CombatantType == EntityType.CharmedMob) ||
                                         ((EntityType)c.CombatantType == EntityType.Fellow)) &&
                                         (playersList.Contains(c.CombatantName))
                                   orderby c.CombatantType, c.CombatantName
                                   let actions = c.GetInteractionsRowsByActorCombatantRelation()
                                   select new
                                   {
                                       Name = c.CombatantName,
                                       DisplayName = c.CombatantNameOrJobName,
                                       HasMelee = actions.Any(a => (ActionType)a.ActionType == ActionType.Melee),
                                       SimpleMelee = from ma in actions
                                                     where (ActionType)ma.ActionType == ActionType.Melee
                                                     orderby ma.Timestamp
                                                     select ma,
                                   };

            // Fill in the timestamp lists at the start.
            foreach (var combatant in simpleAttackList)
            {
                if (combatant.HasMelee == true)
                    FillTimestampList(timestampLists[combatant.Name], combatant.SimpleMelee);
            }



            // Now for the real work

            #region LINQ query
            var attacksMade = from c in dataSet.Combatants
                              where (((EntityType)c.CombatantType == EntityType.Player) ||
                                     ((EntityType)c.CombatantType == EntityType.Pet) ||
                                     ((EntityType)c.CombatantType == EntityType.CharmedMob) ||
                                     ((EntityType)c.CombatantType == EntityType.Fellow)) &&
                                    (playersList.Contains(c.CombatantName))
                              orderby c.CombatantType, c.CombatantName
                              let actions = c.GetInteractionsRowsByActorCombatantRelation()
                              select new
                              {
                                  Name = c.CombatantName,
                                  DisplayName = c.CombatantNameOrJobName,
                                  CombatantRow = c,
                                  HasMelee = actions.Any(a => (ActionType)a.ActionType == ActionType.Melee),
                                  SimpleMelee = from ma in actions
                                                where (ActionType)ma.ActionType == ActionType.Melee
                                                select ma,
                                  MeleeRounds = from ma in actions
                                                where (ActionType)ma.ActionType == ActionType.Melee
                                                group ma by ClosestTimestamp(ma.Timestamp, timestampLists[c.CombatantName])
                              };
            #endregion

            // If no results, just exit.
            if (attacksMade.Any(a => a.HasMelee == true) == false)
                return;


            List<AttackCalculations> attackCalcs = new List<AttackCalculations>();
            AttackCalculations attackCalc;
            int defaultAttacksPerRound = attacksCombo.CBSelectedIndex();

            #region Calculations

            foreach (var attacker in attacksMade.Where(a => a.HasMelee == true))
            {
                // Create a new object to store the info
                attackCalc = new AttackCalculations();

                // Note the name of the player
                attackCalc.Name = attacker.DisplayName;

                // Quick counts to be used further
                attackCalc.Attacks = attacker.SimpleMelee.Count();
                attackCalc.Rounds = attacker.MeleeRounds.Count();


                // First we need to determine the number of attacks per round.
                // If the UI setting is on Auto (0), need to automatically figure
                // it out.
                if (defaultAttacksPerRound == 0)
                {
                    // Attempt to auto-calculate default attacks per round.
                    var attacksPerRoundGroups = attacker.MeleeRounds.GroupBy(m => m.Count());
                    var attacksPerRoundThreshold = attacksPerRoundGroups
                        .Where(m => m.Count() > attackCalc.Rounds / 4);

                    if (attacksPerRoundThreshold.Count() > 0)
                    {
                        attackCalc.AttacksPerRound = attacksPerRoundThreshold.Min(m => m.Key);
                        if (attackCalc.AttacksPerRound > 2)
                            attackCalc.AttacksPerRound = attacksPerRoundGroups.Min(m => m.Key);
                    }
                    else
                    {
                        attackCalc.AttacksPerRound = attacksPerRoundGroups.Min(m => m.Key);
                    }
                }
                else
                {
                    attackCalc.AttacksPerRound = defaultAttacksPerRound;
                }

                // Then we need the list of killshots made by the player where they
                // only hit once when they have a default of 2 attacks per round.
                var madeKill = dataSet.Battles.Where(b => b.IsKillerIDNull() == false &&
                    b.KillerID == attacker.CombatantRow.CombatantID);

                List<DateTime> killTimestamps = new List<DateTime>();

                attackCalc.AttackRoundCountKills = 0;
                attackCalc.AttackRoundUnderCountKills = 0;

                foreach (var kill in madeKill)
                {
                    var killerActions = kill.GetInteractionsRows()
                        .Where(a => a.IsActorIDNull() == false && a.ActorID == attacker.CombatantRow.CombatantID);

                    var damageActions = killerActions.Where(n => (HarmType)n.HarmType == HarmType.Damage ||
                                            (HarmType)n.HarmType == HarmType.Drain);

                    if (damageActions.Count() > 0)
                    {
                        var lastAction = damageActions.Last();

                        if ((ActionType)lastAction.ActionType == ActionType.Melee)
                        {
                            DateTime meleeKillEvent = ClosestTimestamp(
                                lastAction.Timestamp, timestampLists[attacker.Name]);

                            killTimestamps.Add(meleeKillEvent);

                            int attacksOnMeleeKill = attacker.MeleeRounds.First(m => m.Key == meleeKillEvent).Count();

                            if (attacksOnMeleeKill == attackCalc.AttacksPerRound)
                                attackCalc.AttackRoundCountKills++;
                            if (attacksOnMeleeKill < attackCalc.AttacksPerRound)
                                attackCalc.AttackRoundUnderCountKills++;
                        }
                    }
                }



                // Put together possible Zanshin data
                if (attackCalc.AttacksPerRound == 1)
                {
                    // Count possible zanshin rounds
                    var missedFirstAttacks = attacker.MeleeRounds
                        .Where(a => (DefenseType)a.First().DefenseType != DefenseType.None);

                    attackCalc.MissedFirstAttacks = missedFirstAttacks.Count();

                    var possibleZanshinRounds = missedFirstAttacks.Where(a => a.Count() == 2);

                    attackCalc.PossibleZanshin = possibleZanshinRounds.Count();

                    // Get acc of first hits of each round (all rounds)
                    int firstAttHit = attacker.MeleeRounds
                        .Where(a => (DefenseType)a.First().DefenseType == DefenseType.None).Count();

                    attackCalc.FirstAttackAcc = (double)firstAttHit / (firstAttHit + attackCalc.MissedFirstAttacks);

                    // Get acc of second hits of each round

                    if (attackCalc.PossibleZanshin > 0)
                    {
                        int secondAttHit = possibleZanshinRounds.Where(a => (DefenseType)a.Last().DefenseType == DefenseType.None).Count();
                        int secondAttMiss = possibleZanshinRounds.Where(a => (DefenseType)a.Last().DefenseType != DefenseType.None).Count();

                        attackCalc.SecondAttackAcc = (double)secondAttHit / (secondAttHit + secondAttMiss);
                    }
                    else
                    {
                        attackCalc.SecondAttackAcc = 0;
                    }
                }


                // Here we can calculate the distribution of extra attacks.


                var minusOneRounds = attacker.MeleeRounds.Where(a => a.Count() < attackCalc.AttacksPerRound);

                attackCalc.Minus1Rounds = minusOneRounds.Count();

                var roundsWithExtraAttacks = attacker.MeleeRounds
                    .Where(m => m.Count() > attackCalc.AttacksPerRound);

                if (roundsWithExtraAttacks.Count() > 0)
                {
                    attackCalc.RoundsWithExtraAttacks = roundsWithExtraAttacks.Count();

                    attackCalc.Plus1Rounds = roundsWithExtraAttacks.Count(
                        m => (m.Count() - attackCalc.AttacksPerRound) == 1);
                    attackCalc.Plus2Rounds = roundsWithExtraAttacks.Count(
                        m => (m.Count() - attackCalc.AttacksPerRound) == 2);
                    attackCalc.Plus3Rounds = roundsWithExtraAttacks.Count(
                        m => (m.Count() - attackCalc.AttacksPerRound) == 3);
                    attackCalc.Plus4Rounds = roundsWithExtraAttacks.Count(
                        m => (m.Count() - attackCalc.AttacksPerRound) == 4);
                    attackCalc.PlusNRounds = roundsWithExtraAttacks.Count(
                        m => (m.Count() - attackCalc.AttacksPerRound) > 4);

                    // Corrections to results due to -1 rounds
                    foreach (var minusOne in minusOneRounds)
                    {
                        if (killTimestamps.Contains(minusOne.Key) == true)
                            continue;


                        // If a sequence of 1 1 1 is found, turn it into a +1 entry and remove
                        // the first two -1's (the third will be removed in the next block).
                        var middleOne = attacker.MeleeRounds.Where((a, index) => a.Key == minusOne.Key &&
                            index != 0 &&
                            index != attacker.MeleeRounds.Count() - 1 &&
                            attacker.MeleeRounds.ElementAt(index - 1).Count() < attackCalc.AttacksPerRound &&
                            attacker.MeleeRounds.ElementAt(index + 1).Count() < attackCalc.AttacksPerRound)
                            .SingleOrDefault();

                        if (middleOne != null)
                        {
                            attackCalc.Plus1Rounds++;
                            //attackCalc.Minus1Rounds -= 2;
                            continue;
                        }

                        // If a sequence of 1 1 is found, ignore if this is the first of the two entries.
                        // It will be corrected in the next pass.
                        var priorOne = attacker.MeleeRounds.Where((a, index) => a.Key == minusOne.Key &&
                                index != 0 &&
                                index != attacker.MeleeRounds.Count() - 1 &&
                                attacker.MeleeRounds.ElementAt(index + 1).Count() < attackCalc.AttacksPerRound)
                                .SingleOrDefault();

                        if (priorOne != null)
                        {
                            continue;
                        }

                        // If a sequence of 1 1 is found where this is the second of two entries,
                        // treat it as a +0 entry and remove the -1
                        var adjacentOne = attacker.MeleeRounds.Where((a, index) => a.Key == minusOne.Key &&
                                index != 0 &&
                                index != attacker.MeleeRounds.Count() - 1 &&
                                killTimestamps.Contains(attacker.MeleeRounds.ElementAt(index - 1).Key) == false &&
                                attacker.MeleeRounds.ElementAt(index - 1).Count() < attackCalc.AttacksPerRound)
                                .SingleOrDefault();

                        if (adjacentOne != null)
                        {
                            //attackCalc.Minus1Rounds--;
                            continue;
                        }

                        // If this is a -1 next to any +0's, add an extra +1 round and remove this -1.
                        middleOne = attacker.MeleeRounds.Where((a, index) => a.Key == minusOne.Key &&
                                index != 0 &&
                                index != attacker.MeleeRounds.Count() - 1 &&
                                (attacker.MeleeRounds.ElementAt(index - 1).Count() == attackCalc.AttacksPerRound ||
                                attacker.MeleeRounds.ElementAt(index + 1).Count() == attackCalc.AttacksPerRound))
                                .SingleOrDefault();

                        if (middleOne != null)
                        {
                            attackCalc.Plus1Rounds++;
                            //attackCalc.Minus1Rounds--;
                            continue;
                        }

                        int first = 0, second = 0;

                        // If this is a -1 between multiple +1 or highers, add to the lower of the +'s.
                        middleOne = attacker.MeleeRounds.Where((a, index) => a.Key == minusOne.Key &&
                                index != 0 &&
                                index != attacker.MeleeRounds.Count() - 1 &&
                                attacker.MeleeRounds.ElementAt(index - 1).Count() > attackCalc.AttacksPerRound &&
                                attacker.MeleeRounds.ElementAt(index + 1).Count() > attackCalc.AttacksPerRound &&
                                (killTimestamps.Contains(attacker.MeleeRounds.ElementAt(index - 1).Key) == false ||
                                ((first = attacker.MeleeRounds.ElementAt(index - 1).Count()) > 0)) &&
                                (killTimestamps.Contains(attacker.MeleeRounds.ElementAt(index + 1).Key) == false ||
                                ((second = attacker.MeleeRounds.ElementAt(index + 1).Count()) > 0)))
                                .SingleOrDefault();

                        if (middleOne != null)
                        {
                            if ((first == 0) && (second == 0))
                                continue;

                            if (first == 0)
                                first = second;
                            else if ((second != 0) && (second < first))
                                first = second;

                            switch (first)
                            {
                                case 3:
                                    attackCalc.Plus1Rounds--;
                                    attackCalc.Plus2Rounds++;
                                    break;
                                case 4:
                                    attackCalc.Plus2Rounds--;
                                    attackCalc.Plus3Rounds++;
                                    break;
                                case 5:
                                    attackCalc.Plus3Rounds--;
                                    attackCalc.Plus4Rounds++;
                                    break;
                                case 6:
                                    attackCalc.Plus4Rounds--;
                                    attackCalc.PlusNRounds++;
                                    break;
                                default:
                                    attackCalc.PlusNRounds++;
                                    break;
                            }

                            //attackCalc.Minus1Rounds--;
                            continue;
                        }

                    }


                    // And put together a total for multi-attack rounds
                    attackCalc.TotalMultiRounds = attackCalc.Plus1Rounds +
                        attackCalc.Plus2Rounds + attackCalc.Plus3Rounds +
                        attackCalc.Plus4Rounds + attackCalc.PlusNRounds;
                }

                // Now that we have a corrected version for multi-attack rounds, figure out
                // the number of base rounds.
                attackCalc.Rounds = ((attackCalc.Attacks -
                    attackCalc.AttackRoundUnderCountKills -
                    attackCalc.Plus1Rounds -
                    2 * attackCalc.Plus2Rounds -
                    3 * attackCalc.Plus3Rounds -
                    4 * attackCalc.Plus4Rounds -
                    5 * attackCalc.PlusNRounds) / attackCalc.AttacksPerRound) + attackCalc.AttackRoundUnderCountKills;

                // From there we can determine the number of extra attacks generated overall.
                if (attackCalc.AttacksPerRound == 2)
                {
                    attackCalc.ExtraAttacks = (attackCalc.Attacks - attackCalc.AttackRoundUnderCountKills)
                        - (attackCalc.Rounds - attackCalc.AttackRoundUnderCountKills) * 2;
                }
                else
                {
                    attackCalc.ExtraAttacks = attackCalc.Attacks - attackCalc.Rounds;
                }


                // Calculate the number of rounds that are valid to look at for percentages.
                attackCalc.AttackRoundsNonKill = attackCalc.Rounds
                    - attackCalc.AttackRoundCountKills - attackCalc.AttackRoundUnderCountKills;



                // And then store it for output
                attackCalcs.Add(attackCalc);
            }


            #endregion

            PrintOutput(attackCalcs);

            #region Dump Details
            if (showDetails == true)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\n\n\n");
                sb.Append(lsSectionUncorrectedDetails);
                sb.Append("\n\n");
                foreach (var attacker in attacksMade.Where(a => a.MeleeRounds.Count() > 0))
                {
                    sb.AppendLine(attacker.Name);

                    foreach (var round in attacker.MeleeRounds)
                    {
                        sb.Append(string.Format("  {0}  -- #{1}\n", round.Key, round.Count()));
                    }

                    sb.AppendLine();
                }

                PushStrings(sb, null);
            }
            #endregion
        }

        private void PrintOutput(List<AttackCalculations> attackCalcs)
        {
            if (attackCalcs.Count == 0)
                return;

            StringBuilder sb = new StringBuilder();
            List<StringMods> strModList = new List<StringMods>();

            #region Basic Data

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsMainSectionTitle.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(lsMainSectionTitle + "\n\n");


            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsMainHeader1.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(lsMainHeader1 + "\n");

            foreach (var attacker in attackCalcs)
            {
                sb.AppendFormat(lsMainFormat1,
                    attacker.Name,
                    attacker.Attacks,
                    attacker.Rounds,
                    attacker.AttacksPerRound,
                    attacker.ExtraAttacks);
                sb.Append("\n");
            }
            sb.Append("\n");


            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsMainHeader2.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(lsMainHeader2 + "\n");

            foreach (var attacker in attackCalcs)
            {
                sb.AppendFormat(lsMainFormat2,
                    attacker.Name,
                    attacker.Minus1Rounds,
                    attacker.Plus1Rounds,
                    attacker.Plus2Rounds,
                    attacker.Plus3Rounds,
                    attacker.Plus4Rounds,
                    attacker.PlusNRounds);
                sb.Append("\n");
            }
            sb.Append("\n");


            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsMainHeader3.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(lsMainHeader3 + "\n");

            foreach (var attacker in attackCalcs)
            {
                sb.AppendFormat(lsMainFormat3,
                    attacker.Name,
                    attacker.TotalMultiRounds,
                    attacker.AttackRoundsNonKill > 0 ? (double)attacker.TotalMultiRounds / attacker.AttackRoundsNonKill : 0,
                    attacker.AttackRoundCountKills,
                    attacker.AttackRoundUnderCountKills);
                sb.Append("\n");
            }
            sb.Append("\n\n");


            #endregion

            ///////////////////////////////////////////////////////////////

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsSectionTreatAs.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(lsSectionTreatAs + "\n\n");


            #region Double/Triple Attacks

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsSectionMultiAttacksTitle.Length,
                Bold = true,
                Color = Color.Blue
            });
            sb.Append(lsSectionMultiAttacksTitle + "\n\n");

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsSectionMultiAttacksHeader.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(lsSectionMultiAttacksHeader + "\n");

            foreach (var attacker in attackCalcs.Where(a => a.RoundsWithExtraAttacks > 0))
            {
                int doubleAttacks;
                int tripleAttacks;

                if (attacker.AttacksPerRound == 1)
                {
                    doubleAttacks = attacker.Plus1Rounds;
                    tripleAttacks = attacker.Plus2Rounds;
                }
                else
                {
                    if ((attacker.Plus3Rounds > 0) || (attacker.Plus4Rounds > 0))
                    {
                        // can triple; treat plus2 rounds as triples instead of 2 doubles
                        doubleAttacks = attacker.Plus1Rounds + attacker.Plus3Rounds;
                        tripleAttacks = attacker.Plus2Rounds + attacker.Plus3Rounds + 2 * attacker.Plus4Rounds;
                    }
                    else
                    {
                        // can't triple; treat plus2 rounds as 2 doubles
                        doubleAttacks = attacker.Plus1Rounds + attacker.Plus2Rounds * 2;
                        tripleAttacks = 0;
                    }
                }

                sb.AppendFormat(lsSectionMultiAttacksFormat,
                    attacker.Name,
                    doubleAttacks,
                    (double)doubleAttacks / (attacker.AttackRoundsNonKill * attacker.AttacksPerRound),
                    doubleAttacks + tripleAttacks > 0 ? (double)doubleAttacks / (doubleAttacks + tripleAttacks) : 0,
                    tripleAttacks,
                    (double)tripleAttacks / (attacker.AttackRoundsNonKill * attacker.AttacksPerRound),
                    doubleAttacks + tripleAttacks > 0 ? (double)tripleAttacks / (doubleAttacks + tripleAttacks) : 0);
                sb.Append("\n");
            }
            sb.Append("\n");

            #endregion

            #region Kicks

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsSectionKicksTitle.Length,
                Bold = true,
                Color = Color.Blue
            });
            sb.Append(lsSectionKicksTitle + "\n\n");

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsSectionKicksHeader.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(lsSectionKicksHeader + "\n");

            foreach (var attacker in attackCalcs.Where(a => a.RoundsWithExtraAttacks > 0))
            {
                int baseDen = attacker.AttackRoundsNonKill -
                    attacker.Plus2Rounds -
                    attacker.Plus3Rounds -
                    attacker.Plus4Rounds -
                    attacker.PlusNRounds;

                sb.AppendFormat(lsSectionKicksFormat,
                    attacker.Name,
                    attacker.AttacksPerRound == 1 ? lsYes : lsNo,
                    attacker.Plus1Rounds,
                    baseDen > 0 ? (double)attacker.Plus1Rounds / baseDen : 0);
                sb.Append("\n");
            }
            sb.Append("\n");

            #endregion

            #region Zanshin

            if (attackCalcs.Any(a => a.AttacksPerRound == 1 && a.RoundsWithExtraAttacks > 0) == true)
            {
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsSectionZanshinTitle.Length,
                    Bold = true,
                    Color = Color.Blue
                });
                sb.Append(lsSectionZanshinTitle + "\n\n");

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsSectionZanshinHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(lsSectionZanshinHeader + "\n");

                foreach (var attacker in attackCalcs.Where(a => a.RoundsWithExtraAttacks > 0
                    && a.AttacksPerRound == 1))
                {
                    sb.AppendFormat(lsSectionZanshinFormat,
                        attacker.Name,
                        attacker.MissedFirstAttacks,
                        attacker.PossibleZanshin,
                        attacker.MissedFirstAttacks > 0 ? (double)attacker.PossibleZanshin / attacker.MissedFirstAttacks : 0);
                    sb.Append("\n");
                }
                sb.Append("\n");

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsSectionZanshinAccHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(lsSectionZanshinAccHeader + "\n");

                foreach (var attacker in attackCalcs.Where(a => a.RoundsWithExtraAttacks > 0
                    && a.AttacksPerRound == 1))
                {
                    sb.AppendFormat(lsSectionZanshinAccFormat,
                        attacker.Name,
                        attacker.FirstAttackAcc,
                        attacker.SecondAttackAcc);
                    sb.Append("\n");
                }
                sb.Append("\n");
            }
            #endregion

            PushStrings(sb, strModList);
        }

        #endregion

        #region Alternate processing
        private void ProcessDataAlternateMode(KPDatabaseDataSet dataSet)
        {
            ResetTextBox();

            string playerFilter = playersCombo.CBSelectedItem();
            List<string> playersList = new List<string>();

            StringBuilder sbDetails = new StringBuilder();
            if (showDetails)
                AddHeaderForDetailsOutput(ref sbDetails);


            if (playerFilter == lsAll)
            {
                string[] players = playersCombo.CBGetStrings();

                foreach (string player in players)
                {
                    if (player != lsAll)
                    {
                        playersList.Add(player);
                    }
                }
            }
            else
            {
                playersList.Add(playerFilter);
            }

            int defaultAttacksPerRound = attacksCombo.CBSelectedIndex();

            // Setup work for calculating attack round timing
            var simpleAttackList = from c in dataSet.Combatants
                                   where (((EntityType)c.CombatantType == EntityType.Player) ||
                                         ((EntityType)c.CombatantType == EntityType.Pet) ||
                                         ((EntityType)c.CombatantType == EntityType.CharmedMob) ||
                                         ((EntityType)c.CombatantType == EntityType.Fellow)) &&
                                         (playersList.Contains(c.CombatantName))
                                   orderby c.CombatantType, c.CombatantName
                                   let actions = c.GetInteractionsRowsByActorCombatantRelation()
                                   select new
                                   {
                                       Combatant = c,
                                       Name = c.CombatantName,
                                       DisplayName = c.CombatantNameOrJobName,
                                       SimpleMelee = from ma in actions
                                                     where (ActionType)ma.ActionType == ActionType.Melee
                                                     select ma,
                                   };


            int attacksPerRound;
            IEnumerable<IGrouping<DateTime, KPDatabaseDataSet.InteractionsRow>> timestampedAttackGroups;
            List<AttackCalculations> attackCalcs = new List<AttackCalculations>();

            // Fill in the timestamp lists at the start.
            foreach (var combatant in simpleAttackList)
            {
                if (combatant.SimpleMelee.Count() > 0)
                {
                    // fill in buckets of count of .5 second intervals in the timespan list.
                    int[] bucketCounts = FillBuckets(GetAltTimespanList(combatant.SimpleMelee));

                    // determine the timespans that indicate a division between rounds vs
                    // double attacks
                    List<TimeSpan> valleyTimepoints = AnalyzeBuckets(bucketCounts);

                    // As long as we get any distinctive valley profile, use the first one as
                    // the separator between non-DA and DA attacks.
                    if ((valleyTimepoints.Count > 0))
                    {
                        // Lazy grouping for attacks within the timespan
                        // specified by the first valley marker.
                        timestampedAttackGroups = combatant.SimpleMelee.
                            GroupAdjacentByTimeLimit<KPDatabaseDataSet.InteractionsRow, DateTime>(
                            i => i.Timestamp, valleyTimepoints.First());

                        // auto-determine # of attacks per round?
                        if (defaultAttacksPerRound == 0)
                        {
                            // At least 20% of attacks must be 1 per round to consider 1/round as the base
                            int threshhold = timestampedAttackGroups.Count() / 5;

                            if (timestampedAttackGroups.Count(t => t.Count() == 1) > threshhold)
                                attacksPerRound = 1;
                            else
                                attacksPerRound = 2;
                        }
                        else
                        {
                            attacksPerRound = defaultAttacksPerRound;
                        }

                        var playerKills = from b in dataSet.Battles
                                          where b.IsKillerIDNull() == false &&
                                                b.CombatantsRowByBattleKillerRelation == combatant.Combatant
                                          select b;


                        // Run the calculations for this combatant and add it to the list.
                        attackCalcs.Add(RunCalculations(combatant.DisplayName, timestampedAttackGroups, attacksPerRound, playerKills));

                        if (showDetails)
                            AddDetailsForOutput(ref sbDetails, combatant.DisplayName, timestampedAttackGroups);
                    }
                }
            }

            PrintOutput(attackCalcs);

            if (showDetails)
                PushStrings(sbDetails, null);

        }

        private List<TimeSpan> GetAltTimespanList(IEnumerable<KPDatabaseDataSet.InteractionsRow> simpleMelee)
        {
            List<TimeSpan> tsList = new List<TimeSpan>();

            DateTime prev = simpleMelee.First().Timestamp;

            var skipFirst = simpleMelee.Skip(1);

            foreach (var meleeAction in skipFirst)
            {
                tsList.Add(meleeAction.Timestamp - prev);
                prev = meleeAction.Timestamp;
            }

            return tsList;
        }

        private int[] FillBuckets(List<TimeSpan> attackIntervals)
        {
            // fill buckets in 1/2 second intervals
            // initialize at 4 buckets, for up to 2 seconds
            int[] bucketList = new int[4];

            int bucket = 0;

            foreach (var interval in attackIntervals)
            {
                bucket = (int)Math.Floor(interval.TotalSeconds * 2);

                // resize if needed, to a max of 50 (25 seconds)
                if ((bucket > bucketList.Length) && (bucket <= 50))
                {
                    Array.Resize<int>(ref bucketList, bucket + 1);
                }

                if (bucket < bucketList.Length)
                    bucketList[bucket]++;
            }

            return bucketList;
        }

        private List<TimeSpan> AnalyzeBuckets(int[] bucketCounts)
        {
            List<int> peaks = new List<int>();
            List<int> valleys = new List<int>();
            List<TimeSpan> valleyTimepoints = new List<TimeSpan>();

            int totalCount = bucketCounts.Sum();

            if (totalCount == 0)
                return valleyTimepoints;

            double threshhold = 0.04;  // 4% of all attacks to lift a bucket above valley status
            double prevRatio = 0;
            double nextRatio = 0;
            double currRatio = (double)bucketCounts[0] / totalCount;

            for (int i = 0; i < bucketCounts.Length; i++)
            {
                if (i < bucketCounts.Length - 1)
                    nextRatio = (double)bucketCounts[i + 1] / totalCount;

                if (currRatio > threshhold)
                    peaks.Add(i);
                else if ((prevRatio > threshhold) || (nextRatio > threshhold))
                    valleys.Add(i);

                prevRatio = currRatio;
                currRatio = nextRatio;
            }

            foreach (int peak in peaks)
            {
                foreach (int otherPeak in peaks)
                {
                    if (otherPeak != peak)
                    {
                        for (int i = peak; i < otherPeak; i++)
                        {
                            if ((peaks.Contains(i) == false) && (valleys.Contains(i) == false))
                                valleys.Add(i);
                        }
                    }
                }
            }

            // If we have no peaks, we can't filter anything
            if (peaks.Count == 0)
                return valleyTimepoints;

            // ignore low points before initial peak
            int firstPeak = peaks.First();

            var laterValleys = from v in valleys
                               where v > firstPeak
                               orderby v
                               select v;


            // if there are any 
            if ((laterValleys.Count() > 0) && (peaks.Count > 0))
            {
                // Find center point of each valley
                int start;
                int end;
                double center;

                List<double> valleyCenters = new List<double>();
                List<int> valleySubset = laterValleys.ToList<int>();

                start = valleySubset.First();
                end = start;
                valleySubset = valleySubset.Skip(1).ToList<int>();

                while (valleySubset.Count > 0)
                {
                    if (valleySubset.First() == (end + 1))
                    {
                        end++;
                    }
                    else
                    {
                        center = ((double)end - start) / 2 + start;
                        valleyCenters.Add(center);

                        start = valleySubset.First();
                        end = start;
                    }

                    valleySubset = valleySubset.Skip(1).ToList<int>();
                }

                center = ((double)end - start) / 2 + start;
                valleyCenters.Add(center);


                foreach (var valley in valleyCenters)
                {
                    double seconds = valley * 0.5 + 0.25;
                    valleyTimepoints.Add(TimeSpan.FromSeconds(seconds));
                }
            }

            return valleyTimepoints;
        }

        private AttackCalculations RunCalculations(string name,
            IEnumerable<IGrouping<DateTime, KPDatabaseDataSet.InteractionsRow>> timestampedAttackGroups,
            int attacksPerRound, EnumerableRowCollection<KPDatabaseDataSet.BattlesRow> playerKills)
        {
            // Put the results of all the calculations in an AttackCalculations object to return.
            AttackCalculations attackCalc = new AttackCalculations();

            // Summary of basic data

            attackCalc.AttacksPerRound = attacksPerRound;

            attackCalc.Name = name;

            attackCalc.Attacks = timestampedAttackGroups.Sum(a => a.Count());
            attackCalc.Rounds = timestampedAttackGroups.Count();

            var roundsWithExtraAttacks = timestampedAttackGroups.Where(r => r.Count() > attacksPerRound);

            attackCalc.Plus1Rounds = roundsWithExtraAttacks.Count(r => r.Count() == (attacksPerRound + 1));
            attackCalc.Plus2Rounds = roundsWithExtraAttacks.Count(r => r.Count() == (attacksPerRound + 2));
            attackCalc.Plus3Rounds = roundsWithExtraAttacks.Count(r => r.Count() == (attacksPerRound + 3));
            attackCalc.Plus4Rounds = roundsWithExtraAttacks.Count(r => r.Count() == (attacksPerRound + 4));
            attackCalc.PlusNRounds = roundsWithExtraAttacks.Count(r => r.Count() > (attacksPerRound + 4));

            attackCalc.ExtraAttacks = roundsWithExtraAttacks.Sum(r => r.Count() - attacksPerRound);
            attackCalc.RoundsWithExtraAttacks = roundsWithExtraAttacks.Count();
            attackCalc.TotalMultiRounds = roundsWithExtraAttacks.Count();

            if (attacksPerRound > 1)
                attackCalc.Minus1Rounds = timestampedAttackGroups.Count(r => r.Count() < attacksPerRound);
            else
                attackCalc.Minus1Rounds = 0;

            attackCalc.AttackRoundsNonKill = attackCalc.Rounds;
            attackCalc.AttackRoundUnderCountKills = 0;
            attackCalc.AttackRoundCountKills = 0;

            // Correct for melee rounds that kill the mob
            foreach (var battle in playerKills)
            {
                var actions = battle.GetInteractionsRows().Where(i => i.IsActorIDNull() == false &&
                    i.CombatantsRowByActorCombatantRelation == battle.CombatantsRowByBattleKillerRelation &&
                    (HarmType)i.HarmType == HarmType.Damage);

                if (actions.Count() > 0)
                {
                    var finalAction = actions.Last();

                    if ((ActionType)finalAction.ActionType == ActionType.Melee)
                    {
                        // get melee round that this belongs to
                        var killRound = timestampedAttackGroups.LastOrDefault(a =>
                            Math.Abs((finalAction.Timestamp - a.Key).TotalSeconds) < 2.0);

                        if (killRound != null)
                        {
                            if (killRound.Count() < attacksPerRound)
                            {
                                attackCalc.AttackRoundUnderCountKills++;
                                attackCalc.AttackRoundsNonKill--;
                            }
                            else if (killRound.Count() == attacksPerRound)
                            {
                                attackCalc.AttackRoundCountKills++;
                                attackCalc.AttackRoundsNonKill--;
                            }
                        }
                    }
                }
            }



            // End summary section

            // Calculate Zanshin stuff
            if (attacksPerRound == 1)
            {
                var missedFirstAttacks = timestampedAttackGroups.Where(r =>
                    (DefenseType)r.First().DefenseType != DefenseType.None);

                attackCalc.MissedFirstAttacks = missedFirstAttacks.Count();

                var possibleZanshinRounds = missedFirstAttacks.Where(a => a.Count() == 2);

                attackCalc.PossibleZanshin = possibleZanshinRounds.Count();

                // Get acc of first hits of each round (all rounds)
                int firstAttHit = timestampedAttackGroups
                    .Where(a => (DefenseType)a.First().DefenseType == DefenseType.None).Count();

                attackCalc.FirstAttackAcc = (double)firstAttHit / (firstAttHit + attackCalc.MissedFirstAttacks);

                // Get acc of second hits of each round

                if (attackCalc.PossibleZanshin > 0)
                {
                    int secondAttHit = possibleZanshinRounds.Where(a => (DefenseType)a.Last().DefenseType == DefenseType.None).Count();
                    int secondAttMiss = possibleZanshinRounds.Where(a => (DefenseType)a.Last().DefenseType != DefenseType.None).Count();

                    attackCalc.SecondAttackAcc = (double)secondAttHit / (secondAttHit + secondAttMiss);
                }
                else
                {
                    attackCalc.SecondAttackAcc = 0;
                }
            }

            return attackCalc;
        }

        private void AddHeaderForDetailsOutput(ref StringBuilder sbDetails)
        {
            sbDetails.Append("\n\n\n");
            sbDetails.Append(lsSectionUncorrectedDetails);
            sbDetails.Append("\n\n");
        }

        private void AddDetailsForOutput(ref StringBuilder sbDetails,
            string name,
            IEnumerable<IGrouping<DateTime, KPDatabaseDataSet.InteractionsRow>> timestampedAttackGroups)
        {
            sbDetails.Append("\n");
            sbDetails.AppendLine(name);

            int count;

            foreach (var round in timestampedAttackGroups)
            {
                count = round.Count();

                sbDetails.Append(string.Format("  {0}  -- {1} {2}\n", round.Key.ToLocalTime(), count,
                    count > 1 ? "attacks" : "attack"));
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

        protected void attacksCombo_SelectedIndexChanged(object sender, EventArgs e)
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

        protected void alternateProcessOption_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            alternateProcessMode = sentBy.Checked;

            if (flagNoUpdate == false)
                HandleDataset(null);

            flagNoUpdate = false;
        }
        #endregion

        #region Localization Overrides
        protected override void LoadLocalizedUI()
        {
            playersLabel.Text = Resources.PublicResources.PlayersLabel;

            attLabel.Text = Resources.Combat.ExtraAttPluginBaseAttNumLabel;

            attacksCombo.Items.Clear();
            attacksCombo.Items.Add(Resources.Combat.ExtraAttPluginCategoryAuto);
            attacksCombo.Items.Add(Resources.Combat.ExtraAttPluginCategory1);
            attacksCombo.Items.Add(Resources.Combat.ExtraAttPluginCategory2);
            attacksCombo.SelectedIndex = 0;

            optionsMenu.Text = Resources.PublicResources.Options;
            showDetailOption.Text = Resources.PublicResources.ShowDetail;
            alternateProcessOption.Text = "Alternate Calculation Mode";

            UpdatePlayerList();
            playersCombo.SelectedIndex = 0;

        }

        protected override void LoadResources()
        {
            this.tabName = Resources.Combat.ExtraAttPluginTabName;

            lsRestrictedWarning = Resources.Combat.ExtraAttPluginRestrictedWarning;
            lsAll = Resources.PublicResources.All;
            lsYes = Resources.PublicResources.Yes;
            lsNo = Resources.PublicResources.No;

            lsMainSectionTitle = Resources.Combat.ExtraAttPluginMainSectionTitle;
            lsMainHeader1 = Resources.Combat.ExtraAttPluginHeaderMain1;
            lsMainHeader2 = Resources.Combat.ExtraAttPluginHeaderMain2;
            lsMainHeader3 = Resources.Combat.ExtraAttPluginHeaderMain3;
            lsMainFormat1 = Resources.Combat.ExtraAttPluginFormatMain1;
            lsMainFormat2 = Resources.Combat.ExtraAttPluginFormatMain2;
            lsMainFormat3 = Resources.Combat.ExtraAttPluginFormatMain3;

            lsSectionTreatAs = Resources.Combat.ExtraAttPluginSectionTreatAs;

            lsSectionMultiAttacksTitle = Resources.Combat.ExtraAttPluginSectionMultiAttacksTitle;
            lsSectionMultiAttacksHeader = Resources.Combat.ExtraAttPluginSectionMultiAttacksHeader;
            lsSectionMultiAttacksFormat = Resources.Combat.ExtraAttPluginSectionMultiAttacksFormat;

            lsSectionKicksTitle = Resources.Combat.ExtraAttPluginSectionKicksTitle;
            lsSectionKicksHeader = Resources.Combat.ExtraAttPluginSectionKicksHeader;
            lsSectionKicksFormat = Resources.Combat.ExtraAttPluginSectionKicksFormat;

            lsSectionZanshinTitle = Resources.Combat.ExtraAttPluginSectionZanshinTitle;
            lsSectionZanshinHeader = Resources.Combat.ExtraAttPluginSectionZanshinHeader;
            lsSectionZanshinFormat = Resources.Combat.ExtraAttPluginSectionZanshinFormat;
            lsSectionZanshinAccHeader = Resources.Combat.ExtraAttPluginSectionZanshinAccHeader;
            lsSectionZanshinAccFormat = Resources.Combat.ExtraAttPluginSectionZanshinAccFormat;

            lsSectionUncorrectedDetails = Resources.Combat.ExtraAttPluginUncorrectedDetails;
        }
        #endregion

    }
}
