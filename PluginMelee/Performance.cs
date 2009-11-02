using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WaywardGamers.KParser;
using WaywardGamers.KParser.Database;

namespace WaywardGamers.KParser.Plugin
{
    public class Performance : BasePluginControl
    {
        #region Member Variables

        ToolStripLabel playersLabel = new ToolStripLabel();
        ToolStripComboBox playersCombo = new ToolStripComboBox();
        ToolStripLabel mobsLabel = new ToolStripLabel();
        ToolStripComboBox mobsCombo = new ToolStripComboBox();
        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();

        ToolStripMenuItem groupMobsOption = new ToolStripMenuItem();
        ToolStripMenuItem exclude0XPOption = new ToolStripMenuItem();
        ToolStripMenuItem customMobSelectionOption = new ToolStripMenuItem();

        ToolStripButton editCustomMobFilter = new ToolStripButton();

        bool flagNoUpdate;
        bool groupMobs = true;
        bool exclude0XPMobs = false;
        bool customMobSelection = false;

        // Localized strings

        string lsAll;
        string lsNone;
        string lsTotal;

        string lsOverallTitle;
        string lsOverallHeader;
        string lsOverallFormat;

        string lsParticipateTitle;
        string lsParticipateFightsHeader;
        string lsParticipateFightsFormat;
        string lsParticipateTimeHeader;
        string lsParticipateTimeFormat;

        string lsDPSTitle;
        string lsDPSHeader;
        string lsDPSFormat;
        #endregion

        #region Constructor
        public Performance()
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

            optionsMenu.DropDownItems.Add(groupMobsOption);
            optionsMenu.DropDownItems.Add(exclude0XPOption);
            optionsMenu.DropDownItems.Add(customMobSelectionOption);

            editCustomMobFilter.Enabled = false;
            editCustomMobFilter.Click += new EventHandler(editCustomMobFilter_Click);


            toolStrip.Items.Add(playersLabel);
            toolStrip.Items.Add(playersCombo);
            toolStrip.Items.Add(mobsLabel);
            toolStrip.Items.Add(mobsCombo);
            toolStrip.Items.Add(optionsMenu);
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
            UpdatePlayerList();
            UpdateMobList();

            DisableOptions(Monitoring.Monitor.Instance.IsRunning);

            flagNoUpdate = true;
            mobsCombo.CBSelectIndex(0);

            flagNoUpdate = true;
            playersCombo.CBSelectIndex(0);

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
        #endregion

        #region Processing sections
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            // If we get here during initialization, skip.
            if (playersCombo.Items.Count == 0)
                return;

            if (mobsCombo.Items.Count == 0)
                return;

            if (dataSet == null)
                return;

            ResetTextBox();

            MobFilter mobFilter;
            if (customMobSelection)
                mobFilter = MobXPHandler.Instance.CustomMobFilter;
            else
                mobFilter = mobsCombo.CBGetMobFilter();


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
                            where (playerList.Contains(c.CombatantName))
                            orderby c.CombatantType, c.CombatantName
                            select new AttackGroup
                            {
                                Name = c.CombatantName,
                                AnyAction = from n in c.GetInteractionsRowsByActorCombatantRelation()
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

            ProcessSetOfMobs(dataSet, attackSet, mobFilter);
            //ProcessSingleMob(dataSet, attackSet, mobFilter);
        }

        private void ProcessSetOfMobs(KPDatabaseDataSet dataSet, EnumerableRowCollection<AttackGroup> attackSet, 
            MobFilter mobFilter)
        {
            StringBuilder sb = new StringBuilder();
            List<StringMods> strModList = new List<StringMods>();

            int numBattles = 0;
            TimeSpan totalFightsLength;

            ProcessOverall(dataSet, attackSet, mobFilter, ref sb, ref strModList,
                out numBattles, out totalFightsLength);


            Dictionary<string, TimeSpan> playerCombatTime;
            
            ProcessParticipation(dataSet, attackSet, mobFilter, ref sb, ref strModList,
                numBattles, totalFightsLength, 
                out playerCombatTime);


            ProcessDPM(dataSet, attackSet, mobFilter, ref sb, ref strModList, playerCombatTime);

            PushStrings(sb, strModList);
        }

        private void ProcessOverall(KPDatabaseDataSet dataSet,
            EnumerableRowCollection<AttackGroup> attackSet,
            MobFilter mobFilter,
            ref StringBuilder sb,
            ref List<StringMods> strModList,
            out int numBattles,
            out TimeSpan totalFightsLength)
        {
            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsOverallTitle.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(lsOverallTitle + "\n\n");


            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsOverallHeader.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(lsOverallHeader + "\n");

            totalFightsLength = new TimeSpan();

            foreach (var battleID in mobFilter.SelectedBattles)
            {
                totalFightsLength += dataSet.Battles.FindByBattleID(battleID).FightLength();
            }

            numBattles = mobFilter.Count;

            sb.AppendFormat(lsOverallFormat, numBattles, FormatTimeSpan(totalFightsLength));

            sb.Append("\n\n\n");
        }

        private void ProcessParticipation(KPDatabaseDataSet dataSet,
            EnumerableRowCollection<AttackGroup> attackSet,
            MobFilter mobFilter,
            ref StringBuilder sb,
            ref List<StringMods> strModList,
            int numBattles,
            TimeSpan totalFightsLength,
            out Dictionary<string, TimeSpan> playerCombatTime)
        {
            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsParticipateTitle.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(lsParticipateTitle + "\n\n");


            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsParticipateFightsHeader.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(lsParticipateFightsHeader + "\n");

            Dictionary<string, int> playerFightCounts = new Dictionary<string, int>();

            foreach (var player in attackSet)
            {
                int playerFightCount = 0;

                foreach (var fight in mobFilter.SelectedBattles)
                {
                    if (player.AnyAction.Any(b => b.BattleID == fight))
                        playerFightCount++;
                }

                double participatingFights = (double)playerFightCount / numBattles;

                if (playerFightCount > 0)
                {
                    playerFightCounts[player.Name] = playerFightCount;

                    sb.AppendFormat(lsParticipateFightsFormat, player.Name,
                        playerFightCount, participatingFights);
                    sb.Append("\n");
                }
            }


            sb.Append("\n\n");

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsParticipateTimeHeader.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(lsParticipateTimeHeader + "\n");


            playerCombatTime = new Dictionary<string, TimeSpan>();

            foreach (var player in attackSet)
            {
                TimeSpan playerTimeFighting = new TimeSpan();
                TimeSpan playerFightLengths = new TimeSpan();

                foreach (var fight in mobFilter.SelectedBattles)
                {
                    if (player.AnyAction.Any(b => b.BattleID == fight))
                    {
                        var battle = dataSet.Battles.FindByBattleID(fight);
                        playerFightLengths += battle.FightLength();

                        var firstAction = player.AnyAction.Where(b => b.BattleID == fight).FirstOrDefault();
                        var lastAction = player.AnyAction.Where(b => b.BattleID == fight).LastOrDefault();

                        // Assume 2 seconds to initiate first action of any fight.
                        var timeSpentInFight = (lastAction.Timestamp - firstAction.Timestamp) + TimeSpan.FromSeconds(2);

                        playerTimeFighting += timeSpentInFight;
                    }
                }

                if (playerCombatTime.ContainsKey(player.Name))
                    playerCombatTime[player.Name] += playerTimeFighting;
                else
                    playerCombatTime[player.Name] = playerTimeFighting;

                double percentFightsLength = playerTimeFighting.TotalSeconds / playerFightLengths.TotalSeconds;
                double percentOverallTime = playerTimeFighting.TotalSeconds / totalFightsLength.TotalSeconds;

                if (playerTimeFighting.TotalSeconds > 0)
                {
                    double avgCombatTimePerFight = 0;

                    int fights = 0;
                    if (playerFightCounts.TryGetValue(player.Name, out fights))
                    {
                        if (fights > 0)
                            avgCombatTimePerFight = playerTimeFighting.TotalSeconds / fights;
                    }

                    sb.AppendFormat(lsParticipateTimeFormat, player.Name,
                        FormatTimeSpan(playerTimeFighting), FormatTimeSpan(playerFightLengths),
                        FormatSeconds(avgCombatTimePerFight), percentFightsLength, percentOverallTime);
                    sb.Append("\n");
                }
            }

            sb.Append("\n\n");
        }

        private void ProcessDPM(KPDatabaseDataSet dataSet,
            EnumerableRowCollection<AttackGroup> attackSet,
            MobFilter mobFilter,
            ref StringBuilder sb,
            ref List<StringMods> strModList,
            Dictionary<string, TimeSpan> playerCombatTime)
        {
            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsDPSTitle.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(lsDPSTitle + "\n\n");

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsDPSHeader.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(lsDPSHeader + "\n");


            TimeSpan playerTimeFighting;

            foreach (var player in attackSet)
            {
                if (player.AnyAction.Count() > 0)
                {
                    playerTimeFighting = playerCombatTime[player.Name];

                    if (playerTimeFighting > TimeSpan.Zero)
                    {
                        int meleeDamage = player.MeleeDmg + player.MeleeEffectDmg;
                        int rangeDamage = player.RangeDmg + player.RangeEffectDmg;
                        int spellDamage = player.SpellDmg;
                        int wsDamage = player.WSkillDmg;

                        int otherDamage = player.AbilityDmg +
                            player.CounterDmg +
                            player.RetaliateDmg +
                            player.SpikesDmg;

                        int totalDamage = meleeDamage + rangeDamage + spellDamage + wsDamage + otherDamage;

                        double meleeDPS = meleeDamage / playerTimeFighting.TotalSeconds;
                        double rangeDPS = rangeDamage / playerTimeFighting.TotalSeconds;
                        double spellDPS = spellDamage / playerTimeFighting.TotalSeconds;
                        double wsDPS = wsDamage / playerTimeFighting.TotalSeconds;
                        double otherDPS = otherDamage / playerTimeFighting.TotalSeconds;
                        double totalDPS = totalDamage / playerTimeFighting.TotalSeconds;

                        if (totalDamage > 0)
                        {
                            sb.AppendFormat(lsDPSFormat,
                                player.Name,
                                meleeDPS,
                                rangeDPS,
                                wsDPS,
                                spellDPS,
                                otherDPS,
                                totalDPS);
                            sb.Append("\n");
                        }
                    }
                }
            }
        }


        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            return string.Format("{0,3}:{1:d2}:{2:d2}",
                (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
        }

        private string FormatSeconds(double seconds)
        {
            return string.Format("{0:f2} s", seconds);
        }
        #endregion

        #region Event Handlers
        private void DisableOptions(bool isParseRunning)
        {
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
            mobsLabel.Text = Resources.PublicResources.MobsLabel;

            optionsMenu.Text = Resources.PublicResources.Options;

            UpdatePlayerList();
            playersCombo.CBSelectIndex(0);

            UpdateMobList();
            //if (mobsCombo.Items[0] == Resources.PublicResources.All)
            //    mobsCombo.Items[0] = Resources.PublicResources.None;

            mobsCombo.CBSelectIndex(0);

            optionsMenu.Text = Resources.PublicResources.Options;
            groupMobsOption.Text = Resources.PublicResources.GroupMobs;
            exclude0XPOption.Text = Resources.PublicResources.Exclude0XPMobs;
            customMobSelectionOption.Text = Resources.PublicResources.CustomMobSelection;
            editCustomMobFilter.Text = Resources.PublicResources.EditMobFilter;

        }

        protected override void LoadResources()
        {
            this.tabName = Resources.Combat.PerformancePluginTabName;

            lsAll = Resources.PublicResources.All;
            lsNone = Resources.PublicResources.None;
            lsTotal = Resources.PublicResources.Total;

            lsOverallTitle = Resources.Combat.PerformancePluginOverallTitle;
            lsOverallHeader = Resources.Combat.PerformancePluginOverallHeader;
            lsOverallFormat = Resources.Combat.PerformancePluginOverallFormat;

            lsParticipateTitle = Resources.Combat.PerformancePluginParticipateTitle;
            lsParticipateFightsHeader = Resources.Combat.PerformancePluginParticipateFightsHeader;
            lsParticipateFightsFormat = Resources.Combat.PerformancePluginParticipateFightsFormat;
            lsParticipateTimeHeader = Resources.Combat.PerformancePluginParticipateTimeHeader;
            lsParticipateTimeFormat = Resources.Combat.PerformancePluginParticipateTimeFormat;

            lsDPSTitle = Resources.Combat.PerformancePluginDPSTitle;
            lsDPSHeader = Resources.Combat.PerformancePluginDPSHeader;
            lsDPSFormat = Resources.Combat.PerformancePluginDPSFormat;
        }
        #endregion

    }
}
