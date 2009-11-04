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

            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;
            optionsMenu.DropDownItems.Add(showDetailOption);

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
            internal string DisplayName { get; set; }
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
                attackCalc.Name = attacker.Name;
                attackCalc.DisplayName = attacker.DisplayName;

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
                    var missedFirstAttacks = attacker.MeleeRounds
                        .Where(a => (DefenseType)a.First().DefenseType != DefenseType.None);

                    attackCalc.MissedFirstAttacks = missedFirstAttacks.Count();

                    attackCalc.PossibleZanshin = missedFirstAttacks
                        .Where(a => a.Count() == 2).Count();
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
                    attacker.DisplayName,
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
                    attacker.DisplayName,
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
                    attacker.DisplayName,
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
                    attacker.DisplayName,
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
                    attacker.DisplayName,
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
                        attacker.DisplayName,
                        attacker.MissedFirstAttacks,
                        attacker.PossibleZanshin,
                        attacker.MissedFirstAttacks > 0 ? (double)attacker.PossibleZanshin / attacker.MissedFirstAttacks : 0);
                    sb.Append("\n");
                }
                sb.Append("\n");
            }
            #endregion

            PushStrings(sb, strModList);
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

            lsSectionUncorrectedDetails = Resources.Combat.ExtraAttPluginUncorrectedDetails;
        }
        #endregion

    }
}
