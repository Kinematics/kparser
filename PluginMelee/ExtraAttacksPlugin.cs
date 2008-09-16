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
        #region Constructor
        string header1 = "Player               # Melee Attacks    Total # Rounds    Min Attacks/Round     Total Extra Attacks\n";
        string header2 = "Player               # Rounds w/Extra Attacks       % Rounds w/Extra Attacks    Avg # Attacks/Round\n";
        string header3 = "Player               # +1 Rounds           # +2 Rounds          # >+2 Rounds    Avg # Extra Attacks\n";
        string header4 = "Player               # -1 Rounds   # Countered Attacks   Kills w/Min Attacks   Kills w/<Min Attacks\n";

        bool flagNoUpdate;
        bool showDetails = false;
        ToolStripComboBox playersCombo = new ToolStripComboBox();
        ToolStripComboBox attacksCombo = new ToolStripComboBox();
        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();
        ToolStripMenuItem showDetailOption = new ToolStripMenuItem();

        public ExtraAttacksPlugin()
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


            ToolStripLabel attLabel = new ToolStripLabel();
            attLabel.Text = "Base # of Attacks:";
            toolStrip.Items.Add(attLabel);

            attacksCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            attacksCombo.Items.Add("Auto");
            attacksCombo.Items.Add("1");
            attacksCombo.Items.Add("2");
            attacksCombo.SelectedIndex = 0;
            attacksCombo.SelectedIndexChanged += new EventHandler(this.attacksCombo_SelectedIndexChanged);
            toolStrip.Items.Add(attacksCombo);

            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;
            optionsMenu.Text = "Options";

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
            get { return "Extra Attacks"; }
        }

        public override void Reset()
        {
            ResetTextBox();
            showDetailOption.Checked = false;
            showDetails = false;
        }

        public override void DatabaseOpened(KPDatabaseDataSet dataSet)
        {
            UpdatePlayerList(dataSet);
            showDetailOption.Checked = false;
            showDetails = false;

            playersCombo.CBSelectIndex(0);
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
        {
            ResetTextBox();
            AppendText("Restricted while parse is running.", Color.Black, true, false);

            //string currentlySelectedPlayer = "All";

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

            datasetToUse = null;
            return false;
        }
        #endregion

        #region Private Methods
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
            internal double FracRoundsWithExtraAttacks { get; set; }
            internal double AvgExtraAttacks { get; set; }
            internal double AvgAttacksPerRound { get; set; }
            internal int Plus1Rounds { get; set; }
            internal int Plus2Rounds { get; set; }
            internal int PlusNRounds { get; set; }
            internal int Minus1Rounds { get; set; }
            internal int Counters { get; set; }
            internal int AttackRoundCountKills { get; set; }
            internal int AttackRoundUnderCountKills { get; set; }
        }

        private class TimestampList
        {
            internal string Name { get; set; }
            internal List<DateTime> Timestamps { get; set; }
        }

        private DateTime ClosestTimestamp(string name, DateTime timestamp, ref List<TimestampList> timestampLists)
        {
            var namedList = timestampLists.FirstOrDefault(l => l.Name == name);
            if (namedList == null)
            {
                TimestampList tmpNewList = new TimestampList { Name = name, Timestamps = new List<DateTime>() };
                tmpNewList.Timestamps.Add(timestamp);
                timestampLists.Add(tmpNewList);

                return timestamp;
            }

            if (namedList.Timestamps.Count() == 0)
            {
                namedList.Timestamps.Add(timestamp);
                return timestamp;
            }

            DateTime lastTS = namedList.Timestamps.Last(t => t <= timestamp);

            if (lastTS.AddSeconds(2) >= timestamp)
                return lastTS;

            namedList.Timestamps.Add(timestamp);
            return timestamp;
        }

        private bool InitTimestampList(ref List<TimestampList> timestampLists)
        {
            string[] players = playersCombo.CBGetStrings();

            foreach (string player in players)
            {
                if (player != "All")
                {
                    var namedList = timestampLists.FirstOrDefault(l => l.Name == player);

                    if (namedList == null)
                    {
                        // add a new one
                        timestampLists.Add(
                            new TimestampList { Name = player, Timestamps = new List<DateTime>() });
                    }
                    else
                    {
                        // clear the existing one
                        namedList.Timestamps.Clear();
                    }
                }
            }

            return true;
        }

        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            ResetTextBox();
            string playerFilter = playersCombo.CBSelectedItem();

            List<TimestampList> timestampLists = new List<TimestampList>();
            InitTimestampList(ref timestampLists);

            #region LINQ query
            var attacksMade = from c in dataSet.Combatants
                              where ((c.CombatantType == (byte)EntityType.Player) ||
                                    (c.CombatantType == (byte)EntityType.Pet) ||
                                    (c.CombatantType == (byte)EntityType.Fellow)) &&
                                    ((playerFilter == "All") ||
                                     (playerFilter == c.CombatantName))
                              orderby c.CombatantType, c.CombatantName
                              select new
                              {
                                  Name = c.CombatantName,
                                  Combatant = c,
                                  Counters = from ma in c.GetInteractionsRowsByTargetCombatantRelation()
                                          where (ActionType)ma.ActionType == ActionType.Counterattack
                                          select ma,
                                  MeleeRounds = from ma in c.GetInteractionsRowsByActorCombatantRelation()
                                                where (ActionType)ma.ActionType == ActionType.Melee
                                                group ma by ClosestTimestamp(c.CombatantName, ma.Timestamp, ref timestampLists)
                              };
            #endregion

            List<AttackCalculations> attackCalcs = new List<AttackCalculations>();
            AttackCalculations attackCalc;
            int defaultAttacksPerRound = attacksCombo.CBSelectedIndex();

            #region Calculations

            if (attacksMade.Any(a => a.MeleeRounds.Count() > 0) == false)
                return;

            foreach (var attacker in attacksMade.Where(a => a.MeleeRounds.Count() > 0))
            {
                attackCalc = new AttackCalculations();

                attackCalc.Name = attacker.Name;

                attackCalc.Attacks = attacker.MeleeRounds.Sum(m => m.Count());
                attackCalc.Rounds = attacker.MeleeRounds.Count();
                attackCalc.Counters = attacker.Counters.Count();

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

                attackCalc.Minus1Rounds = attacker.MeleeRounds
                    .Where(m => m.Count() < attackCalc.AttacksPerRound).Count();

                var roundsWithExtraAttacks = attacker.MeleeRounds
                    .Where(m => m.Count() > attackCalc.AttacksPerRound);

                if (roundsWithExtraAttacks.Count() > 0)
                {
                    attackCalc.RoundsWithExtraAttacks = roundsWithExtraAttacks.Count();

                    attackCalc.Plus1Rounds = roundsWithExtraAttacks.Count(
                        m => (m.Count() - attackCalc.AttacksPerRound) == 1);
                    attackCalc.Plus2Rounds = roundsWithExtraAttacks.Count(
                        m => (m.Count() - attackCalc.AttacksPerRound) == 2);
                    attackCalc.PlusNRounds = roundsWithExtraAttacks.Count(
                        m => (m.Count() - attackCalc.AttacksPerRound) > 2);

                    attackCalc.ExtraAttacks = roundsWithExtraAttacks.Sum(
                        m => m.Count() - attackCalc.AttacksPerRound);

                    attackCalc.FracRoundsWithExtraAttacks = (double)attackCalc.RoundsWithExtraAttacks / attackCalc.Rounds;

                    attackCalc.AvgExtraAttacks = (double)attackCalc.ExtraAttacks / attackCalc.RoundsWithExtraAttacks;

                }

                attackCalc.AvgAttacksPerRound = (double)attackCalc.Attacks / attackCalc.Rounds;


                var madeKill = dataSet.Battles.Where(b => b.IsKillerIDNull() == false &&
                    b.KillerID == attacker.Combatant.CombatantID);

                attackCalc.AttackRoundCountKills = 0;
                attackCalc.AttackRoundUnderCountKills = 0;

                foreach (var kill in madeKill)
                {
                    var killerActions = kill.GetInteractionsRows()
                        .Where(a => a.IsActorIDNull() == false && a.ActorID == attacker.Combatant.CombatantID);

                    var meleeActions = killerActions.Where(a => (ActionType)a.ActionType == ActionType.Melee);

                    if (meleeActions.Count() > 0)
                    {
                        var lastAction = meleeActions.Last();

                        DateTime meleeKillEvent = ClosestTimestamp(attacker.Name,
                            lastAction.Timestamp, ref timestampLists);

                        int attacksOnMeleeKill = attacker.MeleeRounds.First(m => m.Key == meleeKillEvent).Count();

                        if (attacksOnMeleeKill == attackCalc.AttacksPerRound)
                            attackCalc.AttackRoundCountKills++;
                        if (attacksOnMeleeKill < attackCalc.AttacksPerRound)
                            attackCalc.AttackRoundUnderCountKills++;
                        
                    }
                }

                
                attackCalcs.Add(attackCalc);
            }


            #endregion

            PrintOutput(attackCalcs);

            #region Dump Data
            if (showDetails == true)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\n\n\nDetails\n\n");
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

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = header1.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(header1);

            foreach (var attacker in attackCalcs)
            {
                sb.AppendFormat("{0,-20}{1,16}{2,18}{3,21}{4,24}\n",
                    attacker.Name,
                    attacker.Attacks,
                    attacker.Rounds,
                    attacker.AttacksPerRound,
                    attacker.ExtraAttacks);
            }
            sb.Append("\n");

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = header2.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(header2);

            foreach (var attacker in attackCalcs)
            {
                sb.AppendFormat("{0,-20}{1,25}{2,31:p2}{3,23:f2}\n",
                    attacker.Name,
                    attacker.RoundsWithExtraAttacks,
                    attacker.FracRoundsWithExtraAttacks,
                    attacker.AvgAttacksPerRound);
            }
            sb.Append("\n");

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = header3.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(header3);

            foreach (var attacker in attackCalcs)
            {
                sb.AppendFormat("{0,-20}{1,12}{2,22}{3,22}{4,23:f2}\n",
                    attacker.Name,
                    attacker.Plus1Rounds,
                    attacker.Plus2Rounds,
                    attacker.PlusNRounds,
                    attacker.AvgExtraAttacks);
            }
            sb.Append("\n");

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = header4.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(header4);

            foreach (var attacker in attackCalcs)
            {
                sb.AppendFormat("{0,-20}{1,12}{2,22}{3,22}{4,23}\n",
                    attacker.Name,
                    attacker.Minus1Rounds,
                    attacker.Counters,
                    attacker.AttackRoundCountKills,
                    attacker.AttackRoundUnderCountKills);
            }
            sb.Append("\n");


            PushStrings(sb, strModList);
        }
        #endregion

        #region Event Handlers
        protected void playersCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(DatabaseManager.Instance.Database);

            flagNoUpdate = false;
        }

        protected void attacksCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(DatabaseManager.Instance.Database);

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
