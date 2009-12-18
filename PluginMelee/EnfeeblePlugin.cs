using System;
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

        ToolStripLabel catLabel = new ToolStripLabel();
        ToolStripComboBox categoryCombo = new ToolStripComboBox();
        ToolStripLabel mobsLabel = new ToolStripLabel();
        ToolStripComboBox mobsCombo = new ToolStripComboBox();

        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();
        ToolStripMenuItem groupMobsOption = new ToolStripMenuItem();
        ToolStripMenuItem exclude0XPOption = new ToolStripMenuItem();
        ToolStripMenuItem showDetailOption = new ToolStripMenuItem();
        ToolStripMenuItem customMobSelectionOption = new ToolStripMenuItem();

        ToolStripButton editCustomMobFilter = new ToolStripButton();

        // Localized Strings

        string lsAll;
        string lsDetails;

        string lsDurationsTitle;
        string lsParalyzeTitle;
        string lsTPMovesTitle;
        string lsSpeedTitle;

        string lsDurationsHeader;
        string lsParalyzeHeader;
        string lsTPMovesHeader;
        string lsSpeedHeader1;
        string lsSpeedHeader2;

        string lsDurationsFormat;
        string lsParalyzeFormat;
        string lsTPMovesFormat;
        string lsSpeedFormat;
        

        // Localized regexes
        Regex diaRegex;
        Regex bioRegex;
        Regex paralyzeRegex;
        Regex jubakuRegex;
        Regex slowRegex;
        Regex hojoRegex;
        Regex elegyRegex;
        #endregion

        #region Constructor
        public EnfeeblePlugin()
        {
            LoadLocalizedUI();

            categoryCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            categoryCombo.SelectedIndexChanged += new EventHandler(this.categoryCombo_SelectedIndexChanged);

            mobsCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            mobsCombo.MaxDropDownItems = 10;
            mobsCombo.AutoSize = false;
            mobsCombo.Width = 175;
            mobsCombo.SelectedIndexChanged += new EventHandler(this.mobsCombo_SelectedIndexChanged);


            groupMobsOption.CheckOnClick = true;
            groupMobsOption.Checked = true;
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
            optionsMenu.DropDownItems.Add(groupMobsOption);
            optionsMenu.DropDownItems.Add(exclude0XPOption);
            optionsMenu.DropDownItems.Add(customMobSelectionOption);
            optionsMenu.DropDownItems.Add(bSeparator);
            optionsMenu.DropDownItems.Add(showDetailOption);


            editCustomMobFilter.Enabled = false;
            editCustomMobFilter.Click += new EventHandler(editCustomMobFilter_Click);

            ToolStripSeparator aSeparator = new ToolStripSeparator();

            toolStrip.Items.Add(catLabel);
            toolStrip.Items.Add(categoryCombo);
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
                                where (HarmType)i.HarmType == HarmType.Enfeeble
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
                mobFilter = mobsCombo.CBGetMobFilter(exclude0XPMobs);

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
                              DebufferName = c.CombatantNameOrJobName,
                              Debuffs = from b in c.GetInteractionsRowsByActorCombatantRelation()
                                        where (((HarmType)b.HarmType == HarmType.Enfeeble ||
                                                (HarmType)b.HarmType == HarmType.Dispel ||
                                                (HarmType)b.HarmType == HarmType.Unknown) &&
                                                b.Preparing == false && b.IsActionIDNull() == false)
                                               ||
                                               (b.Preparing == false &&
                                                b.IsActionIDNull() == false &&
                                                (diaRegex.Match(b.ActionsRow.ActionName).Success ||
                                                 bioRegex.Match(b.ActionsRow.ActionName).Success))
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
                        where (b.IsEnemyIDNull() == false &&
                               mobFilter.CheckFilterBattle(b) == true &&
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
                          where (b.IsEnemyIDNull() == false &&
                                 mobFilter.CheckFilterBattle(b) == true &&
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
                                                (paralyzeRegex.Match(i.ActionsRow.ActionName).Success ||
                                                 jubakuRegex.Match(i.ActionsRow.ActionName).Success) &&
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
                      where (b.IsEnemyIDNull() == false &&
                             mobFilter.CheckFilterBattle(b) == true &&
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
                                                (slowRegex.Match(i.ActionsRow.ActionName).Success ||
                                                 hojoRegex.Match(i.ActionsRow.ActionName).Success ||
                                                 elegyRegex.Match(i.ActionsRow.ActionName).Success) &&
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


            List<StringMods> strModList = new List<StringMods>();
            StringBuilder sb = new StringBuilder();

            int analysisTypeIndex = categoryCombo.CBSelectedIndex();

            switch (analysisTypeIndex)
            {
                case 1: // "Enfeeble Durations"
                    ProcessDurations(durationSet, ref sb, ref strModList);
                    break;
                case 2: // "Paralyze"
                    ProcessParalyze(paralyzeSet, ref sb, ref strModList);
                    break;
                case 3: // "TP Moves"
                    ProcessTPMoves(tpMoveSet, ref sb, ref strModList);
                    break;
                case 4: // "Attack Speed"
                    ProcessAttackSpeed(slowSet, ref sb, ref strModList);
                    break;
                case 0: // "All"
                default:
                    ProcessDurations(durationSet, ref sb, ref strModList);
                    ProcessParalyze(paralyzeSet, ref sb, ref strModList);
                    ProcessTPMoves(tpMoveSet, ref sb, ref strModList);
                    //ProcessAttackSpeed(slowSet, ref sb, ref strModList);
                    break;
            }

            PushStrings(sb, strModList);
        }

        private void ProcessDurations(IEnumerable<DebuffGroup> debuffSet,
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            int used;
            int count;
            string debuffName;
            TimeSpan totalRemainingFight, avgRemainingFight;
            string totalDurationString, avgDurationString;
            bool playerHeader;

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsDurationsTitle.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(lsDurationsTitle + "\n\n");

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
                                 (d.IsActionIDNull() == false &&
                                    (diaRegex.Match(d.ActionsRow.ActionName).Success ||
                                     bioRegex.Match(d.ActionsRow.ActionName).Success))) &&
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
                            strModList.Add(new StringMods
                            {
                                Start = sb.Length,
                                Length = player.DebufferName.Length,
                                Bold = true,
                                Color = Color.Blue
                            });
                            sb.Append(player.DebufferName + "\n");

                            strModList.Add(new StringMods
                            {
                                Start = sb.Length,
                                Length = lsDurationsHeader.Length,
                                Bold = true,
                                Underline = true,
                                Color = Color.Black
                            });
                            sb.Append(lsDurationsHeader + "\n");

                            playerHeader = true;
                        }

                        sb.AppendFormat("{0,-20}", debuffName);

                        avgRemainingFight = TimeSpan.FromMilliseconds(
                            totalRemainingFight.TotalMilliseconds / count);

                        totalDurationString = totalRemainingFight.FormattedShortTimeString(true);

                        avgDurationString = avgRemainingFight.FormattedShortTimeString(false);

                        sb.AppendFormat(lsDurationsFormat,
                            count,
                            totalDurationString,
                            avgDurationString);
                        sb.Append("\n");
                    }
                }
            
                if (playerHeader == true)
                    sb.Append("\n");
            }

            sb.Append("\n");
        }

        private void ProcessParalyze(EnumerableRowCollection<EnfeebleGroup> paralyzeSet,
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsParalyzeTitle.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(lsParalyzeTitle + "\n\n");

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsParalyzeHeader.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(lsParalyzeHeader + "\n");

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

            sb.AppendFormat(lsParalyzeFormat,
                totalFights,
                totalMobsParalyzed,
                totalParalyzed,
                totalActions,
                paralyzeRate);
            sb.Append("\n\n\n");

        }

        private void ProcessAttackSpeed(EnumerableRowCollection<EnfeebleGroup> slowSet,
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsSpeedTitle.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(lsSpeedTitle + "\n");

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsSpeedHeader1.Length,
                Bold = true,
                Color = Color.Black
            });
            sb.Append(lsSpeedHeader1 + "\n");

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsSpeedHeader2.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(lsSpeedHeader2 + "\n");


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


            sb.AppendFormat(lsSpeedFormat,
                totalFights, totalSlowCast, totalMobsSlowed, slowedActions, slowedAttackRate,
                normalActions, normalAttackRate);
            sb.Append("\n\n\n");
        }

        private void ProcessTPMoves(EnumerableRowCollection<TPMovesGroup> tpMoveSet,
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsTPMovesTitle.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(lsTPMovesTitle + "\n\n");

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsTPMovesHeader.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(lsTPMovesHeader + "\n");

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


            string subFightsLengthString = sumFightLengths.FormattedShortTimeString(true);

            string timePerTPMoveString = timePerTPMove.FormattedShortTimeString(true);

            if (countMoves > 0)
            {
                sb.AppendFormat(lsTPMovesFormat,
                    countMoves, subFightsLengthString, numFights, timePerTPMoveString, avgTPMovesPerMinute);
                sb.Append("\n");
            }

            if (showDetails == true)
            {
                sb.Append("\n");
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsDetails.Length,
                    Bold = true,
                    Color = Color.Blue
                });
                sb.Append(lsDetails + "\n");

                foreach (var tpUser in tpMoveSet)
                {
                    var groupedMoves = tpUser.TPMoves.Where(a => a.Preparing == false).GroupBy(m => m.Timestamp);

                    foreach (var move in groupedMoves)
                    {
                        sb.AppendFormat("  {0} - {1}\n",
                            move.Key.ToLocalTime().ToLongTimeString(),
                            move.First().ActionsRow.ActionName);
                    }
                }
            }

            sb.Append("\n");
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

        #region Localization Overrides
        protected override void LoadLocalizedUI()
        {
            catLabel.Text = Resources.PublicResources.CategoryLabel;
            mobsLabel.Text = Resources.PublicResources.MobsLabel;

            optionsMenu.Text = Resources.PublicResources.Options;
            groupMobsOption.Text = Resources.PublicResources.GroupMobs;
            exclude0XPOption.Text = Resources.PublicResources.Exclude0XPMobs;
            customMobSelectionOption.Text = Resources.PublicResources.CustomMobSelection;
            showDetailOption.Text = Resources.PublicResources.ShowDetail;
            editCustomMobFilter.Text = Resources.PublicResources.EditMobFilter;

            categoryCombo.Items.Clear();
            categoryCombo.Items.Add(Resources.PublicResources.All);
            categoryCombo.Items.Add(Resources.Combat.EnfeeblePluginCategoryDurations);
            categoryCombo.Items.Add(Resources.Combat.EnfeeblePluginCategoryParalyze);
            categoryCombo.Items.Add(Resources.Combat.EnfeeblePluginCategoryTPMoves);
            //categoryCombo.Items.Add(Resources.Combat.EnfeeblePluginCategoryAttackSpeed);
            categoryCombo.SelectedIndex = 0;


            UpdateMobList();
            mobsCombo.SelectedIndex = 0;
        }

        protected override void LoadResources()
        {
            this.tabName = Resources.Combat.EnfeeblePluginTabName;

            lsAll = Resources.PublicResources.All;
            lsDetails = Resources.PublicResources.DetailsLabel;

            diaRegex = new Regex(Resources.ParsedStrings.DiaRegex);
            bioRegex = new Regex(Resources.ParsedStrings.BioRegex);
            paralyzeRegex = new Regex(Resources.ParsedStrings.ParalyzeRegex);
            jubakuRegex = new Regex(Resources.ParsedStrings.JubakuRegex);
            slowRegex = new Regex(Resources.ParsedStrings.SlowRegex);
            hojoRegex = new Regex(Resources.ParsedStrings.HojoRegex);
            elegyRegex = new Regex(Resources.ParsedStrings.ElegyRegex);


            lsDurationsTitle = Resources.Combat.EnfeeblePluginTitleDurations;
            lsParalyzeTitle = Resources.Combat.EnfeeblePluginTitleParalyze;
            lsTPMovesTitle = Resources.Combat.EnfeeblePluginTitleTPMoves;
            lsSpeedTitle = Resources.Combat.EnfeeblePluginTitleSpeed;

            lsDurationsHeader = Resources.Combat.EnfeeblePluginHeaderDurations;
            lsParalyzeHeader = Resources.Combat.EnfeeblePluginHeaderParalyze;
            lsTPMovesHeader = Resources.Combat.EnfeeblePluginHeaderTPMoves;
            lsSpeedHeader1 = Resources.Combat.EnfeeblePluginHeaderSpeed1;
            lsSpeedHeader2 = Resources.Combat.EnfeeblePluginHeaderSpeed2;

            lsDurationsFormat = Resources.Combat.EnfeeblePluginFormatDurations;
            lsParalyzeFormat = Resources.Combat.EnfeeblePluginFormatParalyze;
            lsTPMovesFormat = Resources.Combat.EnfeeblePluginFormatTPMoves;
            lsSpeedFormat = Resources.Combat.EnfeeblePluginFormatSpeed;

        }
        #endregion

    }
}
