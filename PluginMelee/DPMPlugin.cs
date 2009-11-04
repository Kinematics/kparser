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
    public class DPMPlugin : BasePluginControl
    {
        #region Member Variables

        ToolStripLabel playersLabel = new ToolStripLabel();
        ToolStripComboBox playersCombo = new ToolStripComboBox();
        ToolStripLabel mobsLabel = new ToolStripLabel();
        ToolStripComboBox mobsCombo = new ToolStripComboBox();
        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();

        bool flagNoUpdate;

        // Localized strings

        string lsAll;
        string lsNone;
        string lsTotal;

        string lsOnlySingleMobsWarning;

        string lsSummaryTitle;
        string lsBattleHeader;
        string lsBattleFormat;
        string lsDataHeader;
        string lsDataFormat;

        string lsCumulativeDamage;
        string lsDamageInLastMinute;
        string lsDamageInPrevMinute;
        string lsDamageInFirstMinute;
        #endregion

        #region Constructor
        public DPMPlugin()
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

            toolStrip.Items.Add(playersLabel);
            toolStrip.Items.Add(playersCombo);
            toolStrip.Items.Add(mobsLabel);
            toolStrip.Items.Add(mobsCombo);
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

            flagNoUpdate = true;
            mobsCombo.CBSelectIndex(0);

            flagNoUpdate = true;
            playersCombo.CBSelectIndex(0);

            HandleDataset(null);
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
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
                    int selectedIndex = mobsCombo.CBSelectedIndex();

                    var mobBattleNumber = e.DatasetChanges.Battles.Last().BattleID;

                    if (mobBattleNumber > (mobsCombo.Items.Count + 1))
                    {
                        UpdateMobList();
                    }

                    if (selectedIndex < 1)
                    {
                        flagNoUpdate = true;
                        mobsCombo.CBSelectIndex(mobBattleNumber-1);
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

        private void UpdateMobList()
        {
            mobsCombo.UpdateWithMobList(false, false);
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


            string selectedPlayer = playersCombo.CBSelectedItem();
            MobFilter mobFilter = mobsCombo.CBGetMobFilter();

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

            if (mobFilter.AllMobs == true)
                ProcessAllMobs(dataSet, playerList);
            else
                ProcessFilteredMobs(dataSet, playerList.ToArray(), mobFilter);
        }

        private void ProcessAllMobs(KPDatabaseDataSet dataSet, List<string> playerList)
        {
            ResetTextBox();
            AppendText(lsOnlySingleMobsWarning + "\n", Color.Red, true, false);
        }

        private void ProcessFilteredMobs(KPDatabaseDataSet dataSet, string[] selectedPlayers, MobFilter mobFilter)
        {
            if ((mobFilter.AllMobs == true) ||
                (mobFilter.GroupMobs == true))
            {
                ResetTextBox();
                AppendText(lsOnlySingleMobsWarning + "\n", Color.Red, true, false);
                return;
            }

            var attackSet = from c in dataSet.Combatants
                            where (selectedPlayers.Contains(c.CombatantName))
                            orderby c.CombatantType, c.CombatantName
                            select new AttackGroup
                            {
                                Name = c.CombatantNameOrJobName,
                                Melee = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Melee &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                        select n,
                                Range = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Ranged &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                        select n,
                                Spell = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Spell &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                                n.Preparing == false &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                        select n,
                                Ability = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                          where ((ActionType)n.ActionType == ActionType.Ability &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain ||
                                                (HarmType)n.HarmType == HarmType.Unknown) &&
                                                n.Preparing == false &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                          select n,
                                WSkill = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where ((ActionType)n.ActionType == ActionType.Weaponskill &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                                n.Preparing == false &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                         select n,
                                SC = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                     where ((ActionType)n.ActionType == ActionType.Skillchain &&
                                            ((HarmType)n.HarmType == HarmType.Damage ||
                                             (HarmType)n.HarmType == HarmType.Drain) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                     select n,
                                Counter = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                          where (ActionType)n.ActionType == ActionType.Counterattack &&
                                               ((DefenseType)n.DefenseType == DefenseType.None) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                          select n,
                                Retaliate = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                            where (ActionType)n.ActionType == ActionType.Retaliation &&
                                               ((DefenseType)n.DefenseType == DefenseType.None) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                            select n,
                                Spikes = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where (ActionType)n.ActionType == ActionType.Spikes &&
                                               ((DefenseType)n.DefenseType == DefenseType.None) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                         select n
                            };

            KPDatabaseDataSet.BattlesRow battleRow = dataSet.Battles.FindByBattleID(mobFilter.FightNumber);

            if (battleRow == null)
            {
                Logger.Instance.Log("Error finding battle",
                    string.Format("Unable to locate battle #{0}\n", mobFilter.FightNumber));
                return;
            }

            ProcessAttackSetForBattle(attackSet, battleRow);
        }

        private void ProcessAttackSetForBattle(EnumerableRowCollection<AttackGroup> attackSet,
            KPDatabaseDataSet.BattlesRow battle)
        {
            ResetTextBox();

            StringBuilder sb = new StringBuilder();
            List<StringMods> strModList = new List<StringMods>();

            DateTime startTimeFilter;
            DateTime endTimeFilter;

            ///////////////////////////////////////////////////////////////////

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsSummaryTitle.Length,
                Bold = true,
                Color = Color.Blue
            });
            sb.Append(lsSummaryTitle + "\n");

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsBattleHeader.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(lsBattleHeader + "\n");

            TimeSpan fightLength = battle.FightLength();
            double fightMinutes = fightLength.TotalMinutes;

            sb.AppendFormat(lsBattleFormat,
                battle.CombatantsRowByEnemyCombatantRelation.CombatantName,
                battle.StartTime.ToLocalTime().ToShortTimeString(),
                battle.EndTime > battle.StartTime ? battle.EndTime.ToLocalTime().ToShortTimeString() : "--:--:--",
                fightLength != TimeSpan.Zero
                    ? string.Format("{0:d2}:{1:d2}:{2:d2}",
                        fightLength.Hours, fightLength.Minutes, fightLength.Seconds)
                    : "--:--:--");

            sb.Append("\n\n");

            ///////////////////////////////////////////////////////////////////
            // Cumulative Damage Per Minute

            startTimeFilter = battle.StartTime;

            if (battle.EndTime > battle.StartTime)
                endTimeFilter = battle.EndTime;
            else
                endTimeFilter = DateTime.Now.ToUniversalTime();

            CreateOuput(lsCumulativeDamage,
                attackSet, battle, startTimeFilter, endTimeFilter, ref sb, ref strModList);


            ///////////////////////////////////////////////////////////////////
            // Damage in the last minute

            if (battle.EndTime > battle.StartTime)
                endTimeFilter = battle.EndTime;
            else
                endTimeFilter = DateTime.Now.ToUniversalTime();

            startTimeFilter = endTimeFilter.AddMinutes(-1);

            if (startTimeFilter < battle.StartTime)
                startTimeFilter = battle.StartTime;

            CreateOuput(lsDamageInLastMinute,
                attackSet, battle, startTimeFilter, endTimeFilter, ref sb, ref strModList);


            ///////////////////////////////////////////////////////////////////
            // Damage in the previous minute

            startTimeFilter = battle.StartTime;

            if (battle.EndTime > battle.StartTime)
                endTimeFilter = battle.EndTime.AddMinutes(-1);
            else
                endTimeFilter = DateTime.Now.AddMinutes(-1);

            startTimeFilter = endTimeFilter.AddMinutes(-1);

            if (endTimeFilter < battle.StartTime)
                endTimeFilter = battle.StartTime;

            if (startTimeFilter < battle.StartTime)
                startTimeFilter = battle.StartTime;

            CreateOuput(lsDamageInPrevMinute,
                attackSet, battle, startTimeFilter, endTimeFilter, ref sb, ref strModList);


            ///////////////////////////////////////////////////////////////////
            // Damage in the first minute

            startTimeFilter = battle.StartTime;

            endTimeFilter = startTimeFilter.AddMinutes(1);

            if ((battle.EndTime > battle.StartTime)
                && (endTimeFilter > battle.EndTime))
                endTimeFilter = battle.EndTime;

            CreateOuput(lsDamageInFirstMinute,
                attackSet, battle, startTimeFilter, endTimeFilter, ref sb, ref strModList);


            ////////////////////////////////////////////////////////////////////
            // Done with calculations and string construction.  Dump to display.

            PushStrings(sb, strModList);
        }

        private void CreateOuput(string headerTitle,
            EnumerableRowCollection<AttackGroup> attackSet, KPDatabaseDataSet.BattlesRow battle,
            DateTime startTimeFilter, DateTime endTimeFilter,
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            string totalsLine;

            double meleeDPM;
            double rangeDPM;
            double magicDPM;
            double abilDPM;
            double wsDPM;
            double otherDPM;
            double totalDPM;

            double meleeDPMTotal;
            double rangeDPMTotal;
            double magicDPMTotal;
            double abilDPMTotal;
            double wsDPMTotal;
            double otherDPMTotal;
            double totalDPMTotal;

            DateTime playerStart;
            DateTime firstStart;
            IEnumerable<KPDatabaseDataSet.InteractionsRow> attacks;

            double fightMinutes = (endTimeFilter - startTimeFilter).TotalMinutes;

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = headerTitle.Length,
                Bold = true,
                Color = Color.Blue
            });
            sb.Append(headerTitle + "\n");

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsDataHeader.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(lsDataHeader + "\n");

            meleeDPMTotal = 0;
            rangeDPMTotal = 0;
            magicDPMTotal = 0;
            abilDPMTotal = 0;
            wsDPMTotal = 0;
            otherDPMTotal = 0;
            totalDPMTotal = 0;
            firstStart = DateTime.MaxValue;

            foreach (var attacker in attackSet)
            {
                if (attacker.TotalActions > 0)
                {
                    playerStart = DateTime.MaxValue;

                    attacks = attacker.Melee
                        .Where(d => d.Timestamp >= startTimeFilter && d.Timestamp <= endTimeFilter);
                    meleeDPM = attacks != null ? attacks.Sum(d => d.Amount) : 0;
                    if (attacks.Count() > 0)
                        if (attacks.First().Timestamp < playerStart)
                            playerStart = attacks.First().Timestamp;
                    attacks = attacker.MeleeEffect
                        .Where(d => d.Timestamp >= startTimeFilter && d.Timestamp <= endTimeFilter);
                    meleeDPM += attacks != null ? attacks.Sum(d => d.SecondAmount) : 0;
                    meleeDPM /= fightMinutes;

                    attacks = attacker.Range
                        .Where(d => d.Timestamp >= startTimeFilter && d.Timestamp <= endTimeFilter);
                    rangeDPM = attacks != null ? attacks.Sum(d => d.Amount) : 0;
                    if (attacks.Count() > 0)
                        if (attacks.First().Timestamp < playerStart)
                            playerStart = attacks.First().Timestamp;
                    attacks = attacker.RangeEffect
                        .Where(d => d.Timestamp >= startTimeFilter && d.Timestamp <= endTimeFilter);
                    rangeDPM += attacks != null ? attacks.Sum(d => d.SecondAmount) : 0;
                    rangeDPM /= fightMinutes;

                    attacks = attacker.Spell
                        .Where(d => d.Timestamp >= startTimeFilter && d.Timestamp <= endTimeFilter);
                    magicDPM = attacks != null ? attacks.Sum(d => d.Amount) : 0;
                    if (attacks.Count() > 0)
                        if (attacks.First().Timestamp < playerStart)
                            playerStart = attacks.First().Timestamp;
                    magicDPM /= fightMinutes;

                    attacks = attacker.Ability
                        .Where(d => d.Timestamp >= startTimeFilter && d.Timestamp <= endTimeFilter);
                    abilDPM = attacks != null ? attacks.Sum(d => d.Amount) : 0;
                    if (attacks.Count() > 0)
                        if (attacks.First().Timestamp < playerStart)
                            playerStart = attacks.First().Timestamp;
                    abilDPM /= fightMinutes;

                    attacks = attacker.WSkill
                        .Where(d => d.Timestamp >= startTimeFilter && d.Timestamp <= endTimeFilter);
                    wsDPM = attacks != null ? attacks.Sum(d => d.Amount) : 0;
                    if (attacks.Count() > 0)
                        if (attacks.First().Timestamp < playerStart)
                            playerStart = attacks.First().Timestamp;
                    wsDPM /= fightMinutes;

                    attacks = attacker.Spikes
                        .Where(d => d.Timestamp >= startTimeFilter && d.Timestamp <= endTimeFilter)
                        .Concat(attacker.Counter.Where(d => d.Timestamp >= startTimeFilter && d.Timestamp <= endTimeFilter))
                        .Concat(attacker.Retaliate.Where(d => d.Timestamp >= startTimeFilter && d.Timestamp <= endTimeFilter));
                    otherDPM = attacks != null ? attacks.Sum(d => d.Amount) : 0;
                    if (attacks.Count() > 0)
                        if (attacks.First().Timestamp < playerStart)
                            playerStart = attacks.First().Timestamp;
                    otherDPM /= fightMinutes;


                    totalDPM = meleeDPM + rangeDPM + magicDPM + abilDPM + wsDPM + otherDPM;

                    if (totalDPM > 0)
                    {
                        if (playerStart < firstStart)
                            firstStart = playerStart;

                        meleeDPMTotal += meleeDPM;
                        rangeDPMTotal += rangeDPM;
                        magicDPMTotal += magicDPM;
                        abilDPMTotal += abilDPM;
                        wsDPMTotal += wsDPM;
                        otherDPMTotal += otherDPM;
                        totalDPMTotal += totalDPM;

                        sb.AppendFormat(lsDataFormat,
                            attacker.Name,
                            playerStart.ToLocalTime().ToShortTimeString(),
                            meleeDPM,
                            rangeDPM,
                            magicDPM,
                            abilDPM,
                            wsDPM,
                            otherDPM,
                            totalDPM);
                        sb.Append("\n");
                    }
                }
            }

            if (firstStart != DateTime.MaxValue)
            {
                totalsLine = string.Format(lsDataFormat,
                    lsTotal,
                    firstStart.ToLocalTime().ToShortTimeString(),
                    meleeDPMTotal,
                    rangeDPMTotal,
                    magicDPMTotal,
                    abilDPMTotal,
                    wsDPMTotal,
                    otherDPMTotal,
                    totalDPMTotal);

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = totalsLine.Length,
                    Bold = true,
                    Color = Color.Black
                });
                sb.Append(totalsLine + "\n");
            }

            sb.Append("\n\n");
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
        }

        protected override void LoadResources()
        {
            this.tabName = Resources.Combat.DPMPluginTabName;

            lsAll = Resources.PublicResources.All;
            lsNone = Resources.PublicResources.None;
            lsTotal = Resources.PublicResources.Total;

            lsOnlySingleMobsWarning = Resources.Combat.DPMPluginOnlySingleMobs;

            lsSummaryTitle = Resources.Combat.DPMPluginSummaryTitle;
            lsBattleHeader = Resources.Combat.DPMPluginBattleHeader;
            lsBattleFormat = Resources.Combat.DPMPluginBattleFormat;
            lsDataHeader = Resources.Combat.DPMPluginDataHeader;
            lsDataFormat = Resources.Combat.DPMPluginDataFormat;

            lsCumulativeDamage = Resources.Combat.DPMPluginCumulatePerMinute;
            lsDamageInLastMinute = Resources.Combat.DPMPluginDamageInLastMinute;
            lsDamageInPrevMinute = Resources.Combat.DPMPluginDamageInPrevMinute;
            lsDamageInFirstMinute = Resources.Combat.DPMPluginDamageInFirstMinute;

        }
        #endregion

    }
}
