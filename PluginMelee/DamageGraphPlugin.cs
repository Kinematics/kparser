using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WaywardGamers.KParser.Database;
using ZedGraph;

namespace WaywardGamers.KParser.Plugin
{
    public class DamageGraphPlugin : BaseGraphPluginControl
    {
        #region Member Variables

        ToolStripLabel playersLabel = new ToolStripLabel();
        ToolStripComboBox playersCombo = new ToolStripComboBox();
        ToolStripLabel buffsLabel = new ToolStripLabel();
        ToolStripComboBox buffsCombo = new ToolStripComboBox();
        ToolStripLabel mobsLabel = new ToolStripLabel();
        ToolStripComboBox mobsCombo = new ToolStripComboBox();
        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();

        ToolStripMenuItem groupMobsOption = new ToolStripMenuItem();
        ToolStripMenuItem exclude0XPOption = new ToolStripMenuItem();
        ToolStripMenuItem customMobSelectionOption = new ToolStripMenuItem();
        ToolStripMenuItem cumulativeDamageOption = new ToolStripMenuItem();
        ToolStripMenuItem individualDamageOption = new ToolStripMenuItem();

        ToolStripButton editCustomMobFilter = new ToolStripButton();

        bool flagNoUpdate;
        bool groupMobs = true;
        bool exclude0XPMobs = false;
        bool customMobSelection = false;
        bool showCumulativeDamage = true;
        bool showIndividualDamage = true;

        int xAxisScale;
        int xAxisSize;

        Color[] indexOfColors = new Color[18];

        BuffsByTimePlugin bbt = new BuffsByTimePlugin();

        // Localized strings

        string lsAll;
        string lsNone;
        string lsTotal;

        string lsXAxisTitle;
        string lsYAxisTitle;
        string lsGraphTitle;
        string lsGraphTitleCumulative;
        string lsSequenceDamage;
        string lsCumulativeDamage;

        #endregion

        #region Constructor
        public DamageGraphPlugin()
        {
            LoadLocalizedUI();

            SetColorIndexes();

            playersCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            playersCombo.MaxDropDownItems = 10;
            playersCombo.SelectedIndexChanged += new EventHandler(this.playersCombo_SelectedIndexChanged);

            buffsCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            buffsCombo.Enabled = false;
            buffsCombo.MaxDropDownItems = 10;
            buffsCombo.SelectedIndexChanged += new EventHandler(this.buffsCombo_SelectedIndexChanged);

            mobsCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            mobsCombo.AutoSize = false;
            mobsCombo.Width = 175;
            mobsCombo.MaxDropDownItems = 10;
            mobsCombo.SelectedIndexChanged += new EventHandler(this.mobsCombo_SelectedIndexChanged);

            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;

            groupMobsOption.CheckOnClick = true;
            groupMobsOption.Checked = true;
            groupMobsOption.Click += new EventHandler(groupMobs_Click);

            exclude0XPOption.CheckOnClick = true;
            exclude0XPOption.Checked = false;
            exclude0XPOption.Click += new EventHandler(exclude0XPMobs_Click);

            customMobSelectionOption.CheckOnClick = true;
            customMobSelectionOption.Checked = false;
            customMobSelectionOption.Click += new EventHandler(customMobSelection_Click);

            ToolStripSeparator aSeparator = new ToolStripSeparator();

            individualDamageOption.CheckOnClick = true;
            individualDamageOption.Checked = true;
            individualDamageOption.Click += new EventHandler(individualDamageOption_Click);

            cumulativeDamageOption.CheckOnClick = true;
            cumulativeDamageOption.Checked = true;
            cumulativeDamageOption.Click += new EventHandler(cumulativeDamageOption_Click);

            optionsMenu.DropDownItems.Add(groupMobsOption);
            optionsMenu.DropDownItems.Add(exclude0XPOption);
            optionsMenu.DropDownItems.Add(customMobSelectionOption);
            optionsMenu.DropDownItems.Add(aSeparator);
            optionsMenu.DropDownItems.Add(individualDamageOption);
            optionsMenu.DropDownItems.Add(cumulativeDamageOption);


            editCustomMobFilter.Enabled = false;
            editCustomMobFilter.Click += new EventHandler(editCustomMobFilter_Click);


            toolStrip.Items.Add(playersLabel);
            toolStrip.Items.Add(playersCombo);
            toolStrip.Items.Add(buffsLabel);
            toolStrip.Items.Add(buffsCombo);
            toolStrip.Items.Add(mobsLabel);
            toolStrip.Items.Add(mobsCombo);
            toolStrip.Items.Add(optionsMenu);
            toolStrip.Items.Add(editCustomMobFilter);
        }
        #endregion

        #region IPlugin Overrides
        public override void Reset()
        {
            ResetGraph();
            xAxisScale = 1;
            xAxisSize = 1;
            SetGraphLabels();
        }

        public override void NotifyOfUpdate()
        {
            UpdatePlayerList();
            UpdateMobList();

            DisableOptions(Monitoring.Monitor.Instance.IsRunning);

            flagNoUpdate = true;
            mobsCombo.CBSelectIndex(0);

            flagNoUpdate = true;
            playersCombo.CBSelectIndex(0);

            Reset();

            HandleDataset(null);
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            // Parse is running.
            DisableOptions(true);

            if ((e.DatasetChanges.Combatants != null) &&
                (e.DatasetChanges.Combatants.Any(c => c.RowState == DataRowState.Added)))
            {
                UpdatePlayerList();

                if (playersCombo.CBSelectedIndex() < 0)
                {
                    flagNoUpdate = true;
                    playersCombo.CBSelectIndex(0);
                }
            }

            // Check for new mobs being fought.  If any exist, update the Mob Group dropdown list.
            if (e.DatasetChanges.Battles != null)
            {
                if (e.DatasetChanges.Battles.Any(b => b.RowState == DataRowState.Added))
                {
                    string currentSelection = mobsCombo.CBSelectedItem();

                    UpdateMobList();

                    if (groupMobs == false)
                    {
                        mobsCombo.CBSelectIndex(-1);
                    }
                    else
                    {
                        // Selected index will only get reset to -1 if the mob list changed.
                        if (mobsCombo.CBSelectedIndex() < 0)
                            mobsCombo.CBSelectItem(currentSelection);
                    }
                }
            }

            if (e.DatasetChanges.Interactions.Count != 0)
            {
                HandleDataset(null);
            }
        }
        #endregion

        #region Private Methods
        private void UpdatePlayerList()
        {
            playersCombo.CBReset();
            playersCombo.CBAddStrings(GetPlayerListing());
        }

        private void UpdateLockedMobList()
        {
            mobsCombo.UpdateWithMobList(false, false);
        }

        private void UpdateMobList()
        {
            mobsCombo.UpdateWithMobList(groupMobs, exclude0XPMobs);
        }

        private void FillBuffsCombo()
        {
            string player = playersCombo.CBSelectedItem();
            if (player != lsAll)
            {
                IEnumerable<SimpleInteractionGroup> playerBuffs = null;

                using (var db = new Database.AccessToTheDatabase())
                {
                    playerBuffs = GetPlayerBuffs(db.Database, player);
                }

                if (playerBuffs == null)
                    return;

                List<string> buffStrings = new List<string>();
                buffStrings.Add(Resources.PublicResources.None);

                var targetBuffs = from b in playerBuffs
                                  from c in b.IRows1
                                  where c.IsActionIDNull() == false
                                  select c.ActionsRow.ActionName;

                var selfBuffs = from b in playerBuffs
                                from c in b.IRows2
                                where c.IsActionIDNull() == false
                                select c.ActionsRow.ActionName;

                var allBuffs = targetBuffs.Concat(selfBuffs).Distinct()
                    .Where(b => bbt.TrackedBuffNames.Contains(b))
                    .OrderBy(b => b);

                buffStrings.AddRange(allBuffs);

                buffsCombo.Items.Clear();
                buffsCombo.Items.AddRange(buffStrings.ToArray());

                buffsCombo.CBSelectIndex(0);
            }
        }

        /// <summary>
        /// Set up an array of colors to be used for individual lines
        /// in the graph.
        /// </summary>
        private void SetColorIndexes()
        {
            indexOfColors = new Color[18] {
                Color.Red,
                Color.Blue,
                Color.Purple,
                Color.Green,
                Color.Orange,
                Color.Orchid,
                Color.SandyBrown,
                Color.DimGray,
                Color.Sienna,
                Color.SkyBlue,
                Color.Tan,
                Color.Tomato,
                Color.Turquoise,
                Color.DarkSlateBlue,
                Color.DarkOrchid,
                Color.Yellow,
                Color.MediumPurple,
                Color.Maroon};
        }

        private void SetGraphLabels()
        {
            SetGraphTitle();

            StringBuilder lsXAxisScaled = new StringBuilder();
            if (xAxisScale > 1)
            {
                int endPoint = lsXAxisTitle.IndexOf(')');
                if (endPoint > 0)
                {
                    lsXAxisScaled.Append(lsXAxisTitle.Substring(0, endPoint));
                    lsXAxisScaled.AppendFormat(" x{0}", xAxisScale);
                    lsXAxisScaled.Append(lsXAxisTitle.Substring(endPoint));
                }
                else
                {
                    lsXAxisScaled.Append(lsXAxisTitle);
                }
            }
            else
            {
                lsXAxisScaled.Append(lsXAxisTitle);
            }

            zedGraphControl.GraphPane.XAxis.Title.Text = lsXAxisScaled.ToString();
            zedGraphControl.GraphPane.YAxis.Title.Text = lsYAxisTitle;
        }

        private void SetGraphTitle()
        {
            var pane = zedGraphControl.GraphPane;

            if (showCumulativeDamage)
                pane.Title.Text = lsGraphTitleCumulative;
            else
                pane.Title.Text = lsGraphTitle;

            zedGraphControl.Invalidate();
        }
        #endregion

        #region Processing Methods
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            // If we get here during initialization, skip.
            if (playersCombo.Items.Count == 0)
                return;

            if (mobsCombo.Items.Count == 0)
                return;

            if (dataSet == null)
                return;

            Reset();

            MobFilter mobFilter;
            if (customMobSelection)
                mobFilter = MobXPHandler.Instance.CustomMobFilter;
            else
                mobFilter = mobsCombo.CBGetMobFilter(exclude0XPMobs);


            string selectedPlayer = playersCombo.CBSelectedItem();

            List<string> playerList = new List<string>();

            if (selectedPlayer == lsAll)
            {
                foreach (string player in playersCombo.CBGetStrings())
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

            int mobCount = mobFilter.Count;

            if (mobCount == 0)
                return;

            #region LINQ
            var attackSet = from c in dataSet.Combatants
                            where (((EntityType)c.CombatantType == EntityType.Player ||
                                    (EntityType)c.CombatantType == EntityType.Pet) &&
                                   playerList.Contains(c.CombatantName) &&
                                   RegexUtility.ExcludedPlayer.Match(c.PlayerInfo).Success == false)
                            orderby c.CombatantType, c.CombatantName
                            select new AttackGroup
                            {
                                Name = c.CombatantName,
                                DisplayName = c.CombatantNameOrJobName,
                                AnyAction = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                                       .Where(a => a.IsBattleIDNull() == false)
                                            where ((HarmType)n.HarmType == HarmType.Damage ||
                                                    (HarmType)n.HarmType == HarmType.Drain) &&
                                                   mobFilter.CheckFilterMobTarget(n) == true
                                            select n,
                                Melee = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Melee &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain)) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                        select n,
                                Range = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Ranged &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain)) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                        select n,
                                Spell = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Spell &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                                n.Preparing == false) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                        select n,
                                Ability = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                          where ((ActionType)n.ActionType == ActionType.Ability &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain ||
                                                (HarmType)n.HarmType == HarmType.Unknown) &&
                                                n.Preparing == false) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                          select n,
                                WSkill = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where ((ActionType)n.ActionType == ActionType.Weaponskill &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                                n.Preparing == false) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                         select n,
                                Counter = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                          where (ActionType)n.ActionType == ActionType.Counterattack &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                          select n,
                                Retaliate = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                            where (ActionType)n.ActionType == ActionType.Retaliation &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                            select n,
                                Spikes = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where (ActionType)n.ActionType == ActionType.Spikes &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                         select n
                            };

            #endregion

            double[] xAxis = GetXAxis(attackSet);
            SetGraphLabels();

            if (showIndividualDamage)
            {
                if (playerList.Count == 1)
                    ProcessSingleIndividualDamage(dataSet, attackSet, mobFilter, xAxis);
                else
                    ProcessIndividualDamage(dataSet, attackSet, mobFilter, xAxis);
            }
            else
            {
                ProcessCollectiveDamage(dataSet, attackSet, mobFilter, xAxis);
            }

        }

        private void ProcessCollectiveDamage(KPDatabaseDataSet dataSet,
            EnumerableRowCollection<AttackGroup> attackSet, MobFilter mobFilter,
            double[] xAxis)
        {
            DateTime startTime;
            DateTime endTime;

            GetTimeRange(attackSet, out startTime, out endTime);

            double[] sequenceDamage = GetCollectiveSequenceDamage(attackSet, startTime, endTime);
            string label = lsSequenceDamage;

            if (showCumulativeDamage)
            {
                sequenceDamage = AccumulateDamage(sequenceDamage);
                label = lsCumulativeDamage;
            }

            PointPairList ppl = new PointPairList(xAxis, sequenceDamage);
            zedGraphControl.GraphPane.AddCurve(label, ppl, Color.Red, SymbolType.None);

            zedGraphControl.AxisChange();

        }

        private void ProcessIndividualDamage(KPDatabaseDataSet dataSet,
            EnumerableRowCollection<AttackGroup> attackSet, MobFilter mobFilter,
            double[] xAxis)
        {
            DateTime startTime;
            DateTime endTime;

            GetTimeRange(attackSet, out startTime, out endTime);

            int colorIndex = 0;

            foreach (var player in attackSet)
            {
                if (player.AnyAction.Count() > 0)
                {
                    double[] playerDamage = GetIndividualSequenceDamage(player, startTime, endTime);
                    string label = player.DisplayName;

                    if (showCumulativeDamage)
                    {
                        playerDamage = AccumulateDamage(playerDamage);
                    }

                    PointPairList ppl = new PointPairList(xAxis, playerDamage);
                    zedGraphControl.GraphPane.AddCurve(label, ppl, indexOfColors[colorIndex], SymbolType.None);

                    colorIndex = (colorIndex + 1) % 18;
                }
            }

            zedGraphControl.AxisChange();

        }

        private void ProcessSingleIndividualDamage(KPDatabaseDataSet dataSet,
            EnumerableRowCollection<AttackGroup> attackSet, MobFilter mobFilter,
            double[] xAxis)
        {
            DateTime startTime;
            DateTime endTime;

            GetTimeRange(attackSet, out startTime, out endTime);

            int colorIndex = 0;

            foreach (var player in attackSet)
            {
                if (player.AnyAction.Count() > 0)
                {
                    double[] playerDamage = GetIndividualSequenceDamage(player, startTime, endTime);
                    string label = player.DisplayName;
                    string buffName = buffsCombo.CBSelectedItem();

                    if (showCumulativeDamage)
                    {
                        playerDamage = AccumulateDamage(playerDamage);
                    }

                    if (buffName != lsNone)
                    {
                        var intervals = bbt.GetTimeIntervals(dataSet, new List<string>() { player.Name });

                        var playerIntervals = intervals.FirstOrDefault(i => i.PlayerName == player.Name);

                        if (playerIntervals != null)
                        {
                            var actionIntervals = playerIntervals.TimeIntervalSets.FirstOrDefault(
                                i => i.SetName == buffName);

                            if ((actionIntervals != null) && (actionIntervals.TimeIntervals.Count > 0))
                            {
                                //double[] playerInDamage = new double[playerDamage.Length];
                                double[] playerOutDamage = new double[playerDamage.Length];

                                DateTime timePoint;

                                for (int i = 0; i < playerDamage.Length; i++)
                                {
                                    timePoint = startTime.AddSeconds(i * xAxisScale);

                                    if (!actionIntervals.Contains(timePoint))
                                    {
                                        //    playerInDamage[i] = playerDamage[i];
                                        //else
                                        playerOutDamage[i] = playerDamage[i];

                                    }
                                }

                                string label1 = label + " w/" + buffName;
                                string label2 = label + " wo/" + buffName;

                                PointPairList ppl2 = new PointPairList(xAxis, playerOutDamage);
                                var li2 = zedGraphControl.GraphPane.AddCurve(label2, ppl2, Color.Red, SymbolType.None);
                                li2.Line.Fill = new Fill(Color.Red, Color.Bisque);


                                PointPairList ppl1 = new PointPairList(xAxis, playerDamage);
                                var li1 = zedGraphControl.GraphPane.AddCurve(label1, ppl1, Color.Blue, SymbolType.None);
                                li1.Line.Fill = new Fill(Color.Blue, Color.LightBlue);

                                continue;
                            }
                        }
                    }
                    else
                    {
                        PointPairList ppl = new PointPairList(xAxis, playerDamage);
                        zedGraphControl.GraphPane.AddCurve(label, ppl, indexOfColors[colorIndex], SymbolType.None);

                        colorIndex = (colorIndex + 1) % 18;
                    }
                }
            }

            zedGraphControl.AxisChange();

        }

        #endregion

        #region Private helper functions
        /// <summary>
        /// Given a collection of AttackGroup data, determine the earliest and
        /// latest time any action took place.  Return those values in the out
        /// params for startTime and endTime.  If no valid time can be determined,
        /// throw an exception.
        /// </summary>
        /// <param name="attackSet"></param>
        /// <param name="startTime">Earliest time of any action in the attackSet.</param>
        /// <param name="endTime">Latest time of any action in the attackSet.</param>
        private void GetTimeRange(EnumerableRowCollection<AttackGroup> attackSet,
            out DateTime startTime, out DateTime endTime)
        {
            startTime = default(DateTime);

            // Start time should be based on the first battle rather than
            // the first action, so that changing between players for display
            // yields the same scale in 'event time'.

            var minBattleTimes = from a in attackSet.Select(s => s.AnyAction)
                           select a.MinEntry(c => c.Timestamp, v => v.BattlesRow.StartTime);

            var validMinTimes = minBattleTimes.Where(t => t != default(DateTime));

            if (validMinTimes.Count() > 0)
                startTime = validMinTimes.Min();


            // Determine the time of the last action in the attack set.
            endTime = default(DateTime);

            var maxTimes = from a in attackSet.Select(s => s.AnyAction)
                           select a.MaxEntry(c => c.Timestamp, v => v.Timestamp);

            var validMaxTimes = maxTimes.Where(t => t != default(DateTime));

            if (validMaxTimes.Count() > 0)
                endTime = validMaxTimes.Max();

            // Verify we ended up with actual valid times.
            if ((startTime == default(DateTime)) && (endTime != default(DateTime)))
                throw new ArgumentOutOfRangeException("startTime");

            if ((endTime == default(DateTime)) && (startTime != default(DateTime)))
                throw new ArgumentOutOfRangeException("endTime");
        }

        /// <summary>
        /// Create an array of the seconds that cover the duration between
        /// the specified start and end times.
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns>An array of doubles filled with the sequence
        /// of seconds between the start time and the end time.</returns>
        private double[] GetXAxis(EnumerableRowCollection<AttackGroup> attackSet)
        {
            DateTime startTime;
            DateTime endTime;

            GetTimeRange(attackSet, out startTime, out endTime);

            CalculateAxisSize(startTime, endTime);

            double[] xAxis = new double[xAxisSize];

            for (int i = 0; i < xAxis.Length; i++)
            {
                xAxis[i] = i * xAxisScale;
            }

            return xAxis;
        }

        /// <summary>
        /// Calculate the integral number of seconds elapsed between
        /// the given start and end time values.  Will always return
        /// a positive value.
        /// </summary>
        /// <param name="startTime">The time the axis should start (the first second)</param>
        /// <param name="endTime">The time the axis should end.</param>
        /// <returns>Returns a value of at least 1 for the number of seconds
        /// between the start and the end.</returns>
        private void CalculateAxisSize(DateTime startTime, DateTime endTime)
        {
            int axisSize = (int)Math.Abs((endTime - startTime).TotalSeconds) + 1;

            // To prevent out of memory errors, put a limit on the x axis size.
            // Current limit is set to 604,800 elements (seconds), or one
            // week's worth of seconds.
            xAxisScale = 1;

            while ((axisSize/xAxisScale) > 604800)
            {
                xAxisScale *= 2;
            }

            xAxisSize = axisSize / xAxisScale + 1;
        }

        private double[] GetCollectiveSequenceDamage(EnumerableRowCollection<AttackGroup> attackSet,
            DateTime startTime, DateTime endTime)
        {
            double[] totalSequenceDamage = new double[xAxisSize];

            foreach (var player in attackSet)
            {
                double[] playerSequence = GetIndividualSequenceDamage(player, startTime, endTime);

                for (int i = 0; i < totalSequenceDamage.Length; i++)
                {
                    totalSequenceDamage[i] += playerSequence[i];
                }
            }

            return totalSequenceDamage;
        }


        private double[] GetIndividualSequenceDamage(AttackGroup attackSet,
            DateTime startTime, DateTime endTime)
        {
            double[] playerSequenceDamage = new double[xAxisSize];

            foreach (var action in attackSet.AnyAction)
            {
                int seconds = (int)(action.Timestamp - startTime).TotalSeconds;

                if (seconds < 0)
                    seconds = 0;

                playerSequenceDamage[seconds / xAxisScale] += action.Amount + action.SecondAmount;
            }

            return playerSequenceDamage;
        }


        /// <summary>
        /// Convert an array of damage values to a cumulative damage array.
        /// </summary>
        /// <param name="sequenceDamage"></param>
        /// <returns></returns>
        private double[] AccumulateDamage(double[] sequenceDamage)
        {
            if (sequenceDamage == null)
                throw new ArgumentNullException();

            if (sequenceDamage.Length == 0)
                throw new ArgumentException();

            double[] accumulatedDamage = new double[sequenceDamage.Length];

            accumulatedDamage[0] = sequenceDamage[0];

            for (int i = 1; i < sequenceDamage.Length; i++)
            {
                accumulatedDamage[i] = accumulatedDamage[i - 1] + sequenceDamage[i];
            }

            return accumulatedDamage;
        }


        private IEnumerable<SimpleInteractionGroup> GetPlayerBuffs(
            KPDatabaseDataSet dataSet, string playerName)
        {
            var buffSet = from c in dataSet.Combatants
                          where c.CombatantName == playerName
                          select new SimpleInteractionGroup
                          {
                              Name = c.CombatantName,
                              DisplayName = c.CombatantNameOrJobName,
                              IRows1 = from n in c.GetInteractionsRowsByTargetCombatantRelation()
                                       where (n.IsBattleIDNull() == false &&
                                              (AidType)n.AidType == AidType.Enhance)
                                       select n,
                              IRows2 = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                       where (n.IsBattleIDNull() == false &&
                                              n.IsTargetIDNull() == true &&
                                              (AidType)n.AidType == AidType.Enhance)
                                       select n
                          };

            return buffSet;
        }
        #endregion

        #region Event Handlers
        private void DisableOptions(bool isParseRunning)
        {
            if (this.InvokeRequired)
            {
                Action<bool> disableOptions = new Action<bool>(DisableOptions);
                object[] boolParam = new object[1] { isParseRunning };

                Invoke(disableOptions, boolParam);
                return;
            }

            if (isParseRunning)
            {
                optionsMenu.Enabled = false;
                editCustomMobFilter.Enabled = false;
            }
            else
            {
                optionsMenu.Enabled = true;
                editCustomMobFilter.Enabled = customMobSelection;
            }
        }

        protected void playersCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(null);

            buffsCombo.Enabled = !(playersCombo.SelectedItem.ToString() == lsAll);
            if (buffsCombo.Enabled)
                FillBuffsCombo();
            else
                buffsCombo.Items.Clear();

            flagNoUpdate = false;
        }

        protected void buffsCombo_SelectedIndexChanged(object sender, EventArgs e)
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

        protected void exclude0XPMobs_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            exclude0XPMobs = sentBy.Checked;

            if (flagNoUpdate == false)
            {
                try
                {
                    UpdateMobList();
                    flagNoUpdate = true;
                    mobsCombo.CBSelectIndex(0);
                    HandleDataset(null);
                }
                catch (Exception ex)
                {
                    Logger.Instance.Log(ex);
                }
            }

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
                try
                {
                    UpdateMobList();
                    flagNoUpdate = true;
                    mobsCombo.CBSelectIndex(0);
                }
                catch (Exception ex)
                {
                    Logger.Instance.Log(ex);
                }
            }

            flagNoUpdate = false;
        }

        protected void individualDamageOption_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            showIndividualDamage = sentBy.Checked;

            if (flagNoUpdate == false)
                HandleDataset(null);

            flagNoUpdate = false;
        }

        protected void cumulativeDamageOption_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            showCumulativeDamage = sentBy.Checked;
            SetGraphTitle();

            if (flagNoUpdate == false)
                HandleDataset(null);

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
                try
                {
                    HandleDataset(null);
                }
                catch (Exception ex)
                {
                    Logger.Instance.Log(ex);
                }
            }

            flagNoUpdate = false;
        }

        protected void editCustomMobFilter_Click(object sender, EventArgs e)
        {
            MobXPHandler.Instance.ShowCustomMobFilter();
        }

        protected override void OnCustomMobFilterChanged()
        {
            try
            {
                HandleDataset(null);
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
            }
        }
        #endregion

        #region Localization Overrides
        protected override void LoadLocalizedUI()
        {
            playersLabel.Text = Resources.PublicResources.PlayersLabel;
            buffsLabel.Text = Resources.PublicResources.BuffsLabel;
            mobsLabel.Text = Resources.PublicResources.MobsLabel;

            optionsMenu.Text = Resources.PublicResources.Options;

            UpdatePlayerList();
            playersCombo.CBSelectIndex(0);

            UpdateMobList();

            mobsCombo.CBSelectIndex(0);

            optionsMenu.Text = Resources.PublicResources.Options;
            groupMobsOption.Text = Resources.PublicResources.GroupMobs;
            exclude0XPOption.Text = Resources.PublicResources.Exclude0XPMobs;
            customMobSelectionOption.Text = Resources.PublicResources.CustomMobSelection;
            editCustomMobFilter.Text = Resources.PublicResources.EditMobFilter;
            individualDamageOption.Text = Resources.Combat.DamageGraphPluginIndividualDamageOption;
            cumulativeDamageOption.Text = Resources.Combat.DamageGraphPluginCumulativeDamageOption;
        }

        protected override void LoadResources()
        {
            this.tabName = Resources.Combat.DamageGraphPluginTabName;

            lsAll = Resources.PublicResources.All;
            lsNone = Resources.PublicResources.None;
            lsTotal = Resources.PublicResources.Total;

            lsXAxisTitle = Resources.Combat.DamageGraphPluginXAxisTitle;
            lsYAxisTitle = Resources.Combat.DamageGraphPluginYAxisTitle;
            lsGraphTitle = Resources.Combat.DamageGraphPluginGraphTitle;
            lsGraphTitleCumulative = Resources.Combat.DamageGraphPluginGraphTitleCumulative;

            lsSequenceDamage = Resources.Combat.DamageGraphPluginSequenceDamage;
            lsCumulativeDamage = Resources.Combat.DamageGraphPluginCumulativeDamage;

        }
        #endregion
    }
}
