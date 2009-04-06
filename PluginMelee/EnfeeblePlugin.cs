﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WaywardGamers.KParser;
using WaywardGamers.KParser.Interface;
using WaywardGamers.KParser.Database;

namespace WaywardGamers.KParser.Plugin
{
    public class EnfeeblePlugin : BasePluginControl
    {
        #region Member Variables
        bool flagNoUpdate = false;
        bool groupMobs = true;
        bool exclude0XPMobs = false;
        bool customMobSelection = false;
        bool showDetails = false;

        ToolStripComboBox categoryCombo = new ToolStripComboBox();
        ToolStripComboBox mobsCombo = new ToolStripComboBox();

        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();
        ToolStripMenuItem groupMobsOption = new ToolStripMenuItem();
        ToolStripMenuItem exclude0XPOption = new ToolStripMenuItem();
        ToolStripMenuItem customMobSelectionOption = new ToolStripMenuItem();

        ToolStripButton editCustomMobFilter = new ToolStripButton();
        #endregion

        #region Constructor
        public EnfeeblePlugin()
        {
            ToolStripLabel catLabel = new ToolStripLabel();
            catLabel.Text = "Category:";
            toolStrip.Items.Add(catLabel);

            categoryCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            categoryCombo.Items.Add("All");
            categoryCombo.Items.Add("Enfeeble Durations");
            categoryCombo.Items.Add("Paralyze");
            //categoryCombo.Items.Add("Attack Speed");
            categoryCombo.Items.Add("TP Moves");
            categoryCombo.SelectedIndex = 0;
            categoryCombo.SelectedIndexChanged += new EventHandler(this.categoryCombo_SelectedIndexChanged);
            toolStrip.Items.Add(categoryCombo);


            ToolStripLabel mobLabel = new ToolStripLabel();
            mobLabel.Text = "Mobs:";
            toolStrip.Items.Add(mobLabel);

            mobsCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            mobsCombo.MaxDropDownItems = 10;
            mobsCombo.AutoSize = false;
            mobsCombo.Width = 175;
            mobsCombo.Items.Add("All");
            mobsCombo.SelectedIndex = 0;
            mobsCombo.SelectedIndexChanged += new EventHandler(this.mobsCombo_SelectedIndexChanged);
            toolStrip.Items.Add(mobsCombo);

            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;
            optionsMenu.Text = "Options";

            groupMobsOption.Text = "Group Mobs";
            groupMobsOption.CheckOnClick = true;
            groupMobsOption.Checked = true;
            groupMobsOption.Click += new EventHandler(groupMobs_Click);
            optionsMenu.DropDownItems.Add(groupMobsOption);

            exclude0XPOption.Text = "Exclude 0 XP Mobs";
            exclude0XPOption.CheckOnClick = true;
            exclude0XPOption.Checked = false;
            exclude0XPOption.Click += new EventHandler(exclude0XPMobs_Click);
            optionsMenu.DropDownItems.Add(exclude0XPOption);

            customMobSelectionOption.Text = "Custom Mob Selection";
            customMobSelectionOption.CheckOnClick = true;
            customMobSelectionOption.Checked = false;
            customMobSelectionOption.Click += new EventHandler(customMobSelection_Click);
            optionsMenu.DropDownItems.Add(customMobSelectionOption);

            ToolStripSeparator bSeparator = new ToolStripSeparator();
            optionsMenu.DropDownItems.Add(bSeparator);

            ToolStripMenuItem showDetailOption = new ToolStripMenuItem();
            showDetailOption.Text = "Show Detail";
            showDetailOption.CheckOnClick = true;
            showDetailOption.Checked = false;
            showDetailOption.Click += new EventHandler(showDetailOption_Click);
            optionsMenu.DropDownItems.Add(showDetailOption);


            toolStrip.Items.Add(optionsMenu);

            ToolStripSeparator aSeparator = new ToolStripSeparator();
            toolStrip.Items.Add(aSeparator);

            editCustomMobFilter.Text = "Edit Mob Filter";
            editCustomMobFilter.Enabled = false;
            editCustomMobFilter.Click += new EventHandler(editCustomMobFilter_Click);

            toolStrip.Items.Add(editCustomMobFilter);

        }
        #endregion



        #region IPlugin Overrides
        public override string TabName
        {
            get { return "Enfeebling"; }
        }

        public override void Reset()
        {
            ResetTextBox();
        }

        public override void NotifyOfUpdate()
        {
            ResetTextBox();

            UpdateMobList();

            try
            {
                flagNoUpdate = true;
                mobsCombo.CBSelectIndex(0);
            }
            finally
            {
                flagNoUpdate = false;
            }

            HandleDataset(null);
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            // Check for new mobs being fought.  If any exist, update the Mob Group dropdown list.
            if (e.DatasetChanges.Battles.Count > 0)
            {
                string mobSelected = mobsCombo.CBSelectedItem();
                flagNoUpdate = true;
                UpdateMobList();

                try
                {
                    flagNoUpdate = true;
                    mobsCombo.CBSelectItem(mobSelected);
                }
                finally
                {
                    flagNoUpdate = false;
                }
            }

            if (e.DatasetChanges.Interactions.Count != 0)
            {
                var enfeebles = from i in e.DatasetChanges.Interactions
                                where i.HarmType == (byte)HarmType.Enfeeble
                                select i;

                if (enfeebles.Count() > 0)
                {
                    HandleDataset(null);
                }
            }
        }
        #endregion

        #region Private functions
        private void UpdateMobList()
        {
            mobsCombo.UpdateWithMobList(groupMobs, exclude0XPMobs);
            if (mobsCombo.CBSelectedIndex() < 0)
                mobsCombo.CBSelectIndex(0);

        }
        #endregion

        #region Processing Functions
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            if (dataSet == null)
                return;

            ResetTextBox();

            MobFilter mobFilter;
            if (customMobSelection)
                mobFilter = MobXPHandler.Instance.CustomMobFilter;
            else
                mobFilter = mobsCombo.CBGetMobFilter();

            #region LINQ group construction

            IEnumerable<DebuffGroup> durationSet = null;
            EnumerableRowCollection<TPMovesGroup> tpMoveSet = null;
            EnumerableRowCollection<EnfeebleGroup> paralyzeSet = null;
            EnumerableRowCollection<EnfeebleGroup> slowSet = null;
            // Process debuffs used by players

            durationSet = from c in dataSet.Combatants
                          where (((EntityType)c.CombatantType == EntityType.Player) ||
                                ((EntityType)c.CombatantType == EntityType.Pet) ||
                                ((EntityType)c.CombatantType == EntityType.CharmedMob) ||
                                ((EntityType)c.CombatantType == EntityType.Fellow))
                          orderby c.CombatantType, c.CombatantName
                          select new DebuffGroup
                          {
                              DebufferName = c.CombatantName,
                              Debuffs = from b in c.GetInteractionsRowsByActorCombatantRelation()
                                        where (((HarmType)b.HarmType == HarmType.Enfeeble ||
                                                (HarmType)b.HarmType == HarmType.Dispel ||
                                                (HarmType)b.HarmType == HarmType.Unknown) &&
                                                b.Preparing == false && b.IsActionIDNull() == false)
                                               ||
                                               (b.Preparing == false &&
                                                b.IsActionIDNull() == false &&
                                                (b.ActionsRow.ActionName.StartsWith("Dia") ||
                                                  b.ActionsRow.ActionName.StartsWith("Bio")))
                                               ||
                                               (b.Preparing == false && b.IsActionIDNull() == false &&
                                                b.ActionsRow.GetInteractionsRows()
                                                 .Any(q => (HarmType)q.SecondHarmType == HarmType.Enfeeble ||
                                                           (HarmType)q.SecondHarmType == HarmType.Dispel))
                                        group b by b.ActionsRow.ActionName into ba
                                        orderby ba.Key
                                        select new Debuffs
                                        {
                                            DebuffName = ba.Key,
                                            DebuffTargets = from bt in ba
                                                            where (bt.IsTargetIDNull() == false &&
                                                                   mobFilter.CheckFilterMobTarget(bt))
                                                            group bt by bt.CombatantsRowByTargetCombatantRelation.CombatantName into btn
                                                            orderby btn.Key
                                                            select new DebuffTargets
                                                            {
                                                                TargetName = btn.Key,
                                                                DebuffData = btn.OrderBy(i => i.Timestamp)
                                                            }
                                        }
                          };


            tpMoveSet = from b in dataSet.Battles
                        where (mobFilter.CheckFilterBattle(b) == true &&
                               b.DefaultBattle == false) &&
                              (b.IsKillerIDNull() == true ||
                               RegexUtility.ExcludedPlayer.Match(b.CombatantsRowByBattleKillerRelation.PlayerInfo).Success == false)
                        orderby b.BattleID
                        select new TPMovesGroup
                        {
                            FightLength = (b.IsOver == true)
                                        ? (b.EndTime - b.StartTime)
                                        : (DateTime.Now.ToUniversalTime() - b.StartTime) < TimeSpan.FromDays(1)
                                          ? (DateTime.Now.ToUniversalTime() - b.StartTime)
                                          : TimeSpan.Zero,
                            TPMoves = from i in b.GetInteractionsRows()
                                      where i.IsActionIDNull() == false &&
                                            i.IsActorIDNull() == false &&
                                            i.ActorID == b.EnemyID &&
                                            (ActionType)i.ActionType == ActionType.Ability
                                      select i
                        };

            paralyzeSet = from b in dataSet.Battles
                          where (mobFilter.CheckFilterBattle(b) == true &&
                                 b.DefaultBattle == false) &&
                                (b.IsKillerIDNull() == true ||
                                 RegexUtility.ExcludedPlayer.Match(b.CombatantsRowByBattleKillerRelation.PlayerInfo).Success == false)
                          orderby b.BattleID
                          select new EnfeebleGroup
                          {
                              Enfeebled = from i in b.GetInteractionsRows()
                                          where i.IsActionIDNull() == false &&
                                                i.IsTargetIDNull() == false &&
                                                i.TargetID == b.EnemyID &&
                                                (i.ActionsRow.ActionName.StartsWith(SpellNames.Paralyze) ||
                                                 i.ActionsRow.ActionName.StartsWith(SpellNames.Jubaku)) &&
                                                i.Preparing == false &&
                                                (DefenseType)i.DefenseType == DefenseType.None
                                          select i,
                              Actions = from i in b.GetInteractionsRows()
                                        where (i.IsActorIDNull() == false &&
                                              i.ActorID == b.EnemyID &&
                                              ((ActionType)i.ActionType == ActionType.Melee ||
                                               (ActionType)i.ActionType == ActionType.Spell)) ||
                                              ((DefenseType)i.DefenseType == DefenseType.Shadow)
                                        select i,
                              Paralyzed = from i in b.GetInteractionsRows()
                                          where i.IsActorIDNull() == false &&
                                                i.ActorID == b.EnemyID &&
                                                (FailedActionType)i.FailedActionType == FailedActionType.Paralyzed
                                          select i,
                              FightEndTime = b.EndTime
                          };

            slowSet = from b in dataSet.Battles
                          where (mobFilter.CheckFilterBattle(b) == true &&
                                 b.DefaultBattle == false) &&
                                (b.IsKillerIDNull() == true ||
                                 RegexUtility.ExcludedPlayer.Match(b.CombatantsRowByBattleKillerRelation.PlayerInfo).Success == false)
                          orderby b.BattleID
                          select new EnfeebleGroup
                          {
                              Enfeebled = from i in b.GetInteractionsRows()
                                          where i.IsActionIDNull() == false &&
                                                i.IsTargetIDNull() == false &&
                                                i.TargetID == b.EnemyID &&
                                                (i.ActionsRow.ActionName.StartsWith(SpellNames.Slow) ||
                                                 i.ActionsRow.ActionName.StartsWith(SpellNames.Hojo) ||
                                                 i.ActionsRow.ActionName.EndsWith(SpellNames.Elegy)) &&
                                                i.Preparing == false &&
                                                (DefenseType)i.DefenseType == DefenseType.None
                                          select i,
                              Actions = from i in b.GetInteractionsRows()
                                        where (i.IsActorIDNull() == false &&
                                               i.ActorID == b.EnemyID &&
                                               (ActionType)i.ActionType == ActionType.Melee) ||
                                              (DefenseType)i.DefenseType == DefenseType.Shadow
                                        select i,
                              FightEndTime = b.EndTime,
                              FightStartTime = b.StartTime
                          };
            
            
            #endregion


            string analysisType = categoryCombo.CBSelectedItem();

            switch (analysisType)
            {
                case "Enfeeble Durations":
                    ProcessDurations(durationSet);
                    break;
                case "Paralyze":
                    ProcessParalyze(paralyzeSet);
                    break;
                case "TP Moves":
                    ProcessTPMoves(tpMoveSet);
                    break;
                case "Attack Speed":
                    ProcessAttackSpeed(slowSet);
                    break;
                case "All":
                default:
                    ProcessDurations(durationSet);
                    ProcessParalyze(paralyzeSet);
                    //ProcessAttackSpeed(slowSet);
                    ProcessTPMoves(tpMoveSet);
                    break;
            }
        }

        private void ProcessDurations(IEnumerable<DebuffGroup> debuffSet)
        {
            int used;
            int count;
            string debuffName;
            TimeSpan totalRemainingFight, avgRemainingFight;
            string totalDurationString, avgDurationString;
            bool playerHeader;

            AppendText("Enfeeble Durations\n\n", Color.Red, true, false);

            foreach (var player in debuffSet)
            {
                if ((player.Debuffs == null) || (player.Debuffs.Count() == 0))
                    continue;

                if (player.Debuffs.Sum(d => d.DebuffTargets.Count()) == 0)
                    continue;

                playerHeader = false;


                foreach (var debuff in player.Debuffs)
                {
                    debuffName = debuff.DebuffName;

                    totalRemainingFight = TimeSpan.Zero;
                    avgRemainingFight = TimeSpan.Zero;
                    count = 0;

                    foreach (var target in debuff.DebuffTargets)
                    {
                        used = target.DebuffData.Count();

                        if (used > 0)
                        {
                            var successDebuff = target.DebuffData.Where(d =>
                                (((HarmType)d.HarmType == HarmType.Dispel ||
                                 (HarmType)d.HarmType == HarmType.Enfeeble ||
                                 (HarmType)d.HarmType == HarmType.Unknown ||
                                 (d.IsActionIDNull() == false && (d.ActionsRow.ActionName.StartsWith("Dia") ||
                                 d.ActionsRow.ActionName.StartsWith("Bio")))) &&
                                 ((DefenseType)d.DefenseType == DefenseType.None &&
                                 (FailedActionType)d.FailedActionType == FailedActionType.None)) ||
                                ((HarmType)d.SecondHarmType == HarmType.Dispel ||
                                 (HarmType)d.SecondHarmType == HarmType.Enfeeble));


                            foreach (var sDebuff in successDebuff)
                            {
                                if (sDebuff.IsBattleIDNull() == false)
                                {
                                    if (sDebuff.BattlesRow.EndTime > sDebuff.Timestamp)
                                    {
                                        count++;
                                        totalRemainingFight += sDebuff.BattlesRow.EndTime - sDebuff.Timestamp;
                                    }
                                }
                            }
                        }
                    }

                    if (count > 0)
                    {
                        if (playerHeader == false)
                        {
                            AppendText(string.Format("{0}\n", player.DebufferName), Color.Blue, true, false);
                            AppendText("Debuff               #Successful     Total Duration     Avg Duration\n", Color.Black, true, true);
                            playerHeader = true;
                        }

                        AppendText(debuffName.PadRight(20));

                        avgRemainingFight = TimeSpan.FromMilliseconds(
                            totalRemainingFight.TotalMilliseconds / count);

                        totalDurationString = totalRemainingFight.FormattedString(true);

                        avgDurationString = avgRemainingFight.FormattedString(false);

                        AppendText(string.Format("{0,12:d}{1,19}{2,17}\n",
                            count, totalDurationString, avgDurationString));
                    }
                }
            
                if (playerHeader == true)
                    AppendText("\n");
            }

            AppendText("\n");
        }

        private void ProcessParalyze(EnumerableRowCollection<EnfeebleGroup> paralyzeSet)
        {
            AppendText("Paralyzed Actions\n\n", Color.Red, true, false);
            AppendText("# Fights      # Paralyze Cast    # Times Paralyzed    Max # Paralyzable Actions    Paralyze Rate\n", Color.Black, true, true);

            int totalFights = paralyzeSet.Count();
            int totalParaCast = paralyzeSet.Sum(a => a.Enfeebled.Count());

            int totalParalyzed = 0;
            int totalActions = 0;
            int totalMobsParalyzed = 0;
            double paralyzeRate = 0;

            DateTime paraBufferTime = DateTime.MinValue;
            DateTime paraLimitTime = DateTime.MinValue;

            foreach (var mob in paralyzeSet)
            {
                if (mob.Enfeebled.Count() > 0)
                {
                    totalMobsParalyzed++;
                    totalParalyzed += mob.Paralyzed.Count();
                }

                for (int i = 0; i < mob.Enfeebled.Count(); i++)
                {
                    // Get the limits of the time period we will look at for possible
                    // paralyze effects.

                    var enf = mob.Enfeebled.ElementAt(i);
                    
                    // Start point
                    paraBufferTime = enf.Timestamp.AddSeconds(-3);

                    // Preliminary end point
                    paraLimitTime = mob.FightEndTime;

                    if ((i + 1) < mob.Enfeebled.Count())
                    {
                        paraLimitTime = mob.Enfeebled.ElementAt(i + 1).Timestamp.AddSeconds(-2);
                    }

                    if (paraLimitTime > enf.Timestamp.AddMinutes(3))
                        paraLimitTime = enf.Timestamp.AddMinutes(3);

                    // Final end point determined

                    // Get all actions within that window
                    var actionWindow = mob.Actions.Where(a =>
                        a.Timestamp >= paraBufferTime &&
                        a.Timestamp <= paraLimitTime);

                    totalActions += actionWindow.Count();

                }
            }

            if (totalActions > 0)
            {
                paralyzeRate = (double)totalParalyzed / totalActions;
            }

            AppendText(string.Format("{0,8:d}{1,21}{2,21}{3,29}{4,17:p2}\n\n\n",
                totalFights, totalMobsParalyzed, totalParalyzed, totalActions, paralyzeRate));

        }

        private void ProcessAttackSpeed(EnumerableRowCollection<EnfeebleGroup> slowSet)
        {
            AppendText("Slowed Actions\n", Color.Red, true, false);
            AppendText("                                              (Slow)       (Slow)            (Normal)     (Normal)\n", Color.Black, true, false);
            AppendText("# Fights    # Slows Cast     # Mobs Slowed    # Actions    Attacks/Minute    # Actions    Attacks/Minute\n", Color.Black, true, true);

            int totalFights = slowSet.Count();
            int totalSlowCast = slowSet.Sum(a => a.Enfeebled.Count());

            int slowedActions = 0;
            int normalActions = 0;
            int totalMobsSlowed = 0;
            double slowedAttackRate = 0;
            double normalAttackRate = 0;

            DateTime slowBufferTime = DateTime.MinValue;
            DateTime slowLimitTime = DateTime.MinValue;

            TimeSpan totalWindowTime = TimeSpan.Zero;
            TimeSpan unslowedTime = TimeSpan.Zero;

            foreach (var mob in slowSet)
            {
                if (mob.Enfeebled.Count() > 0)
                {
                    totalMobsSlowed++;
                }

                var mobActionList = mob.Actions.ToList();
                unslowedTime += mob.FightEndTime - mob.FightStartTime;

                for (int i = 0; i < mob.Enfeebled.Count(); i++)
                {
                    // Get the limits of the time period we will look at.

                    var enf = mob.Enfeebled.ElementAt(i);

                    // Start point
                    slowBufferTime = enf.Timestamp.AddSeconds(-3);

                    // Preliminary end point
                    slowLimitTime = mob.FightEndTime;

                    if ((i + 1) < mob.Enfeebled.Count())
                    {
                        slowLimitTime = mob.Enfeebled.ElementAt(i + 1).Timestamp.AddSeconds(-2);
                    }

                    if (slowLimitTime > enf.Timestamp.AddMinutes(3))
                        slowLimitTime = enf.Timestamp.AddMinutes(3);

                    // Final end point determined

                    totalWindowTime += slowLimitTime - slowBufferTime;

                    // Get all actions within that window
                    var actionsInWindow = mob.Actions.Where(a =>
                        a.Timestamp >= slowBufferTime &&
                        a.Timestamp <= slowLimitTime);

                    slowedActions += actionsInWindow.Count();

                    foreach (var action in actionsInWindow)
                    {
                        mobActionList.Remove(action);
                    }

                }

                normalActions += mobActionList.Count;
            }

            unslowedTime -= totalWindowTime;

            if (totalWindowTime > TimeSpan.Zero)
            {
                slowedAttackRate = ((double)slowedActions / totalWindowTime.TotalSeconds) * 60;
            }

            if (unslowedTime > TimeSpan.Zero)
            {
                normalAttackRate = ((double)normalActions / unslowedTime.TotalSeconds) * 60;
            }

            AppendText(string.Format("{0,8:d}{1,16}{2,18}{3,13}{4,18:f2}{5,13}{6,18:f2}\n\n\n",
                totalFights, totalSlowCast, totalMobsSlowed, slowedActions, slowedAttackRate,
                normalActions, normalAttackRate));
        }

        private void ProcessTPMoves(EnumerableRowCollection<TPMovesGroup> tpMoveSet)
        {
            AppendText("TP Moves\n\n", Color.Red, true, false);
            AppendText("# Moves      Total Time      # Fights    Avg Time/TP Move      Avg TP Moves/Minute\n", Color.Black, true, true);

            TimeSpan sumFightLengths = TimeSpan.Zero;
            int countMoves = 0;
            int numFights = tpMoveSet.Count();

            foreach (var tpUser in tpMoveSet)
            {
                var groupedMoves = tpUser.TPMoves.Where(a => a.Preparing == false).GroupBy(m => m.Timestamp);

                countMoves += groupedMoves.Count();
                sumFightLengths += tpUser.FightLength;
            }

            TimeSpan timePerTPMove = TimeSpan.Zero;
            double avgTPMovesPerMinute = 0;

            if (countMoves > 0)
                timePerTPMove = TimeSpan.FromSeconds(sumFightLengths.TotalSeconds / countMoves);

            if (timePerTPMove.TotalSeconds > 0)
                avgTPMovesPerMinute = 60 / timePerTPMove.TotalSeconds;


            string subFightsLengthString = sumFightLengths.FormattedString(true);

            string timePerTPMoveString = timePerTPMove.FormattedString(true);

            if (countMoves > 0)
            {
                AppendText(string.Format("{0,7:d}{1,16}{2,14}{3,20}{4,25:f1}\n",
                    countMoves, subFightsLengthString, numFights, timePerTPMoveString, avgTPMovesPerMinute));
            }

            if (showDetails == true)
            {
                AppendText("\nDetails:\n", Color.Blue, true, false);

                foreach (var tpUser in tpMoveSet)
                {
                    var groupedMoves = tpUser.TPMoves.Where(a => a.Preparing == false).GroupBy(m => m.Timestamp);

                    foreach (var move in groupedMoves)
                    {
                        AppendText(string.Format("  {0} - {1}\n", move.Key.ToLocalTime().ToLongTimeString(),
                            move.First().ActionsRow.ActionName));
                    }
                }
            }

            AppendText("\n");
        }

        #endregion

        #region Event Handlers
        protected void categoryCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(null);

            flagNoUpdate = false;
        }

        protected void mobsCombo_SelectedIndexChanged(object sender, EventArgs e)
        {

            try
            {
                if (flagNoUpdate == false)
                    HandleDataset(null);
            }
            finally
            {
                flagNoUpdate = false;
            }
        }

        protected void groupMobs_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            groupMobs = sentBy.Checked;

            try
            {
                if (flagNoUpdate == false)
                {
                    flagNoUpdate = true;
                    UpdateMobList();

                    HandleDataset(null);
                }
            }
            finally
            {
                flagNoUpdate = false;
            }
        }

        protected void exclude0XPMobs_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            exclude0XPMobs = sentBy.Checked;

            try
            {
                if (flagNoUpdate == false)
                {
                    flagNoUpdate = true;
                    UpdateMobList();

                    HandleDataset(null);
                }
            }
            finally
            {
                flagNoUpdate = false;
            }
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

            try
            {
                if (flagNoUpdate == false)
                {
                    HandleDataset(null);
                }
            }
            finally
            {
                flagNoUpdate = false;
            }
        }

        protected void showDetailOption_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            showDetails = sentBy.Checked;

            try
            {
                if (flagNoUpdate == false)
                    HandleDataset(null);
            }
            finally
            {
                flagNoUpdate = false;
            }
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


    }
}
