using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WaywardGamers.KParser;
using WaywardGamers.KParser.Database;
using WaywardGamers.KParser.Utility;

namespace WaywardGamers.KParser.Plugin
{
    public class BuffsByTimePlugin : BasePluginControl
    {
        #region Member Variables
        bool processAccuracy;
        bool processAttack;
        bool processCritRate;
        bool processHaste;

        ToolStripLabel catLabel = new ToolStripLabel();
        ToolStripComboBox categoryCombo = new ToolStripComboBox();
        ToolStripLabel playersLabel = new ToolStripLabel();
        ToolStripComboBox playersCombo = new ToolStripComboBox();
        ToolStripLabel mobsLabel = new ToolStripLabel();
        ToolStripComboBox mobsCombo = new ToolStripComboBox();

        ToolStripMenuItem groupMobsOption = new ToolStripMenuItem();
        ToolStripMenuItem exclude0XPOption = new ToolStripMenuItem();
        ToolStripMenuItem customMobSelectionOption = new ToolStripMenuItem();

        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();

        ToolStripButton editCustomMobFilter = new ToolStripButton();

        bool flagNoUpdate;
        bool groupMobs = true;
        bool exclude0XPMobs = false;
        bool customMobSelection = false;

        // Localized strings
        string lsAll;

        string lsBuffUsedHeader;
        string lsBuffRecHeader;
        string lsSelf;

        string lsNumTimesFormat;
        string lsIntervalsFormat;

        string lsAttack;
        string lsAttackHeader;
        string lsAccuracy;
        string lsAccuracyHeader;
        string lsCritRate;
        string lsCritRateHeader;
        string lsCritRateFormat;
        string lsHaste;
        string lsHasteHeader;
        #endregion

        #region Constructor
        public BuffsByTimePlugin()
        {
            LoadLocalizedUI();

            categoryCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            categoryCombo.SelectedIndex = 0;
            categoryCombo.SelectedIndexChanged += new EventHandler(this.categoryCombo_SelectedIndexChanged);

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


            toolStrip.Items.Add(catLabel);
            toolStrip.Items.Add(categoryCombo);
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
            ResetTextBox();

            UpdateMobList();

            flagNoUpdate = true;
            mobsCombo.CBSelectIndex(0);

            UpdatePlayerList();

            flagNoUpdate = true;
            playersCombo.CBSelectIndex(0);

            HandleDataset(null);
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            if (e.DatasetChanges.Interactions != null)
            {
                if (e.DatasetChanges.Interactions.Count != 0)
                {
                    var enhancements = from i in e.DatasetChanges.Interactions
                                       where (AidType)i.AidType == AidType.Enhance
                                       select i;

                    if (enhancements.Count() > 0)
                    {
                        HandleDataset(null);
                    }
                }
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
            mobsCombo.UpdateWithMobList(groupMobs, exclude0XPMobs);
        }
        #endregion

        #region Processing/display sections
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            ResetTextBox();

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

            List<PlayerTimeIntervalSets> intervalSets = CollectTimeIntervals.GetTimeIntervals(dataSet, playerList);

            if (intervalSets == null)
                return;

            StringBuilder sb = new StringBuilder();
            List<StringMods> strModList = new List<StringMods>();

            if (processAccuracy)
                ProcessAccuracy(dataSet, intervalSets, playerList, ref sb, strModList);

            if (processAttack)
                ProcessAttack(dataSet, intervalSets, playerList, ref sb, strModList);

            if (processCritRate)
                ProcessCritRate(dataSet, intervalSets, playerList, ref sb, strModList);

            if (processHaste)
                ProcessHaste(dataSet, intervalSets, playerList, ref sb, strModList);

            PushStrings(sb, strModList);
        }

        /// <summary>
        /// Process/display info for accuracy-based buffs
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="intervalSets"></param>
        /// <param name="playerList"></param>
        /// <param name="sb"></param>
        /// <param name="strModList"></param>
        private void ProcessAccuracy(KPDatabaseDataSet dataSet,
            List<PlayerTimeIntervalSets> intervalSets, List<string> playerList,
            ref StringBuilder sb, List<StringMods> strModList)
        {
            MobFilter mobFilter;
            if (customMobSelection)
                mobFilter = MobXPHandler.Instance.CustomMobFilter;
            else
                mobFilter = mobsCombo.CBGetMobFilter(exclude0XPMobs);

            int mobCount = mobFilter.Count;

            if (mobCount == 0)
                return;

            var attackSet = from c in dataSet.Combatants
                            where (playerList.Contains(c.CombatantName) &&
                                   RegexUtility.ExcludedPlayer.Match(c.PlayerInfo).Success == false)
                            select new AttackGroup
                            {
                                Name = c.CombatantName,
                                DisplayName = c.CombatantNameOrJobName,
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
                            };

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsAccuracy.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(lsAccuracy + "\n");


            foreach (var playerInterval in intervalSets)
            {
                if (playerInterval.TimeIntervalSets.Count > 0)
                {
                    var playerActions = attackSet.FirstOrDefault(a => a.Name == playerInterval.PlayerName);

                    if ((playerActions != null) &&
                        ((playerActions.Melee.Count() > 0) || (playerActions.Range.Count() > 0)) &&
                        (playerInterval.TimeIntervalSets.Any(s =>
                            CollectTimeIntervals.AccuracyBuffNames.Contains(s.SetName))))
                    {
                        sb.Append("\n");
                        strModList.Add(new StringMods
                        {
                            Start = sb.Length,
                            Length = playerActions.DisplayName.Length,
                            Bold = true,
                            Color = Color.Blue
                        });
                        sb.Append(playerActions.DisplayName + "\n");

                        strModList.Add(new StringMods
                        {
                            Start = sb.Length,
                            Length = lsAccuracyHeader.Length,
                            Bold = true,
                            Underline = true
                        });
                        sb.Append(lsAccuracyHeader + "\n");


                        foreach (var intervalSet in playerInterval.TimeIntervalSets)
                        {
                            if (CollectTimeIntervals.AccuracyBuffNames.Contains(intervalSet.SetName))
                            {
                                if (intervalSet.TimeIntervals.Count > 0)
                                {
                                    var mInSet = from n in playerActions.Melee
                                                 where intervalSet.Contains(n.Timestamp)
                                                 select n;

                                    var mNotInSet = from n in playerActions.Melee
                                                    where intervalSet.Contains(n.Timestamp) == false
                                                    select n;

                                    var rInSet = from n in playerActions.Range
                                                 where intervalSet.Contains(n.Timestamp)
                                                 select n;

                                    var rNotInSet = from n in playerActions.Range
                                                    where intervalSet.Contains(n.Timestamp) == false
                                                    select n;


                                    int mHitCount = mInSet.Count(a => (DefenseType)a.DefenseType == DefenseType.None);
                                    int mMissCount = mInSet.Count(a => (DefenseType)a.DefenseType == DefenseType.Evasion);
                                    int mInSetCount = mHitCount + mMissCount;
                                    double mHitRate = 0;

                                    if (mInSetCount > 0)
                                        mHitRate = (double)mHitCount / mInSetCount;

                                    int mNisHitCount = mNotInSet.Count(a => (DefenseType)a.DefenseType == DefenseType.None);
                                    int mNisMissCount = mNotInSet.Count(a => (DefenseType)a.DefenseType == DefenseType.Evasion);
                                    int mNotInSetCount = mNisHitCount + mNisMissCount;
                                    double mNisHitRate = 0;

                                    if (mNotInSetCount > 0)
                                        mNisHitRate = (double)mNisHitCount / mNotInSetCount;


                                    int rHitCount = rInSet.Count(a => (DefenseType)a.DefenseType == DefenseType.None);
                                    int rMissCount = rInSet.Count(a => (DefenseType)a.DefenseType == DefenseType.Evasion);
                                    int rInSetCount = rHitCount + rMissCount;
                                    double rHitRate = 0;

                                    if (rInSetCount > 0)
                                        rHitRate = (double)rHitCount / rInSetCount;

                                    int rNisHitCount = rNotInSet.Count(a => (DefenseType)a.DefenseType == DefenseType.None);
                                    int rNisMissCount = rNotInSet.Count(a => (DefenseType)a.DefenseType == DefenseType.Evasion);
                                    int rNotInSetCount = rNisHitCount + rNisMissCount;
                                    double rNisHitRate = 0;

                                    if (rNotInSetCount > 0)
                                        rNisHitRate = (double)rNisHitCount / rNotInSetCount;


                                    int inCount = mInSetCount + rInSetCount;

                                    if (inCount > 0)
                                    {
                                        double inSetRate = 0;
                                        double notInSetRate = 0;
                                        double inCritRate = 0;
                                        double ninCritRate = 0;

                                        if (mInSetCount + mNotInSetCount + rInSetCount + rNisHitCount > 0)
                                        {
                                            inSetRate = (double)(mInSetCount + rInSetCount) / (mInSetCount + mNotInSetCount + rInSetCount + rNisHitCount);
                                            notInSetRate = (double)(mNotInSetCount + rNisHitCount) / (mInSetCount + mNotInSetCount + rInSetCount + rNisHitCount);
                                        }

                                        var inSetHits = mInSet.Where(a => (DefenseType)a.DefenseType == DefenseType.None);
                                        var outSetHits = mNotInSet.Where(a => (DefenseType)a.DefenseType == DefenseType.None);

                                        if (inSetHits.Count() > 0)
                                        {
                                            inCritRate = (double) inSetHits.Count(c => (DamageModifier)c.DamageModifier == DamageModifier.Critical) /
                                                inSetHits.Count();
                                        }

                                        if (outSetHits.Count() > 0)
                                        {
                                            ninCritRate = (double)outSetHits.Count(c => (DamageModifier)c.DamageModifier == DamageModifier.Critical) /
                                                outSetHits.Count();
                                        }

                                        sb.AppendFormat("+{0,20}", intervalSet.SetName);

                                        sb.AppendFormat("{0,14}  {1,8:p2}  {2,8:p2}  {3,11}  {4,7:p2}  {5,17:p2}\n",
                                            string.Format("{0}/{1}", mHitCount, mMissCount), mHitRate, inCritRate,
                                            string.Format("{0}/{1}", rHitCount, rMissCount), rHitRate, inSetRate);

                                        sb.AppendFormat("-{0,20}", intervalSet.SetName);

                                        sb.AppendFormat("{0,14}  {1,8:p2}  {2,8:p2}  {3,11}  {4,7:p2}  {5,17:p2}\n",
                                            string.Format("{0}/{1}", mNisHitCount, mNisMissCount), mNisHitRate, ninCritRate,
                                            string.Format("{0}/{1}", rNisHitCount, rNisMissCount), rNisHitRate, notInSetRate);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            sb.Append("\n\n");
        }

        /// <summary>
        /// Process/display attack based results
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="intervalSets"></param>
        /// <param name="playerList"></param>
        /// <param name="sb"></param>
        /// <param name="strModList"></param>
        private void ProcessAttack(KPDatabaseDataSet dataSet,
            List<PlayerTimeIntervalSets> intervalSets, List<string> playerList,
            ref StringBuilder sb, List<StringMods> strModList)
        {
            MobFilter mobFilter;
            if (customMobSelection)
                mobFilter = MobXPHandler.Instance.CustomMobFilter;
            else
                mobFilter = mobsCombo.CBGetMobFilter(exclude0XPMobs);

            int mobCount = mobFilter.Count;

            if (mobCount == 0)
                return;

            var attackSet = from c in dataSet.Combatants
                            where (playerList.Contains(c.CombatantName) &&
                                   RegexUtility.ExcludedPlayer.Match(c.PlayerInfo).Success == false)
                            select new AttackGroup
                            {
                                Name = c.CombatantName,
                                DisplayName = c.CombatantNameOrJobName,
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
                            };


            int mInMin = 0;
            int mInMax = 0;
            double mInAvg = 0;
            int mInCount = 0;

            int mNotInMin = 0;
            int mNotInMax = 0;
            double mNotInAvg = 0;
            int mNotInCount = 0;

            int rInMin = 0;
            int rInMax = 0;
            double rInAvg = 0;
            int rInCount = 0;

            int rNotInMin = 0;
            int rNotInMax = 0;
            double rNotInAvg = 0;
            int rNotInCount = 0;

            int mInCritMin = 0;
            int mInCritMax = 0;
            double mInCritAvg = 0;
            int mInCritCount = 0;

            int mNotInCritMin = 0;
            int mNotInCritMax = 0;
            double mNotInCritAvg = 0;
            int mNotInCritCount = 0;

            int rInCritMin = 0;
            int rInCritMax = 0;
            double rInCritAvg = 0;
            int rInCritCount = 0;

            int rNotInCritMin = 0;
            int rNotInCritMax = 0;
            double rNotInCritAvg = 0;
            int rNotInCritCount = 0;

            int wInMin = 0;
            int wInMax = 0;
            double wInAvg = 0;
            int wInCount = 0;

            int wNotInMin = 0;
            int wNotInMax = 0;
            double wNotInAvg = 0;
            int wNotInCount = 0;

            double mInSetRate = 0;
            double rInSetRate = 0;
            double wInSetRate = 0;


            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsAttack.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(lsAttack + "\n");


            foreach (var playerInterval in intervalSets)
            {
                if (playerInterval.TimeIntervalSets.Count > 0)
                {
                    var playerActions = attackSet.FirstOrDefault(a => a.Name == playerInterval.PlayerName);

                    if ((playerActions != null) &&
                        ((playerActions.Melee.Count() > 0) || (playerActions.Range.Count() > 0)) &&
                        (playerInterval.TimeIntervalSets.Any(s =>
                        CollectTimeIntervals.AttackBuffNames.Contains(s.SetName))))
                    {
                        sb.Append("\n");
                        strModList.Add(new StringMods
                        {
                            Start = sb.Length,
                            Length = playerActions.DisplayName.Length,
                            Bold = true,
                            Color = Color.Blue
                        });
                        sb.Append(playerActions.DisplayName + "\n");

                        strModList.Add(new StringMods
                        {
                            Start = sb.Length,
                            Length = lsAttackHeader.Length,
                            Bold = true,
                            Underline = true
                        });
                        sb.Append(lsAttackHeader + "\n");

                        foreach (var intervalSet in playerInterval.TimeIntervalSets)
                        {
                            if (CollectTimeIntervals.AttackBuffNames.Contains(intervalSet.SetName))
                            {
                                if (intervalSet.TimeIntervals.Count > 0)
                                {
                                    var meleeSet = playerActions.Melee.Where(m =>
                                        (DefenseType)m.DefenseType == DefenseType.None &&
                                        (DamageModifier)m.DamageModifier == DamageModifier.None);
                                    var rangeSet = playerActions.Range.Where(m =>
                                        (DefenseType)m.DefenseType == DefenseType.None &&
                                        (DamageModifier)m.DamageModifier == DamageModifier.None);
                                    var meleeCritSet = playerActions.Melee.Where(m =>
                                        (DefenseType)m.DefenseType == DefenseType.None &&
                                        (DamageModifier)m.DamageModifier == DamageModifier.Critical);
                                    var rangeCritSet = playerActions.Range.Where(m =>
                                        (DefenseType)m.DefenseType == DefenseType.None &&
                                        (DamageModifier)m.DamageModifier == DamageModifier.Critical);

                                    var wsSet = playerActions.WSkill.Where(m => (DefenseType)m.DefenseType == DefenseType.None);

                                    var mInSet = from n in meleeSet
                                                where intervalSet.Contains(n.Timestamp)
                                                select n;

                                    var mNotInSet = from n in meleeSet
                                                   where intervalSet.Contains(n.Timestamp) == false
                                                   select n;

                                    var rInSet = from n in rangeSet
                                                 where intervalSet.Contains(n.Timestamp)
                                                 select n;

                                    var rNotInSet = from n in rangeSet
                                                    where intervalSet.Contains(n.Timestamp) == false
                                                    select n;

                                    var mInCritSet = from n in meleeCritSet
                                                 where intervalSet.Contains(n.Timestamp)
                                                 select n;

                                    var mNotInCritSet = from n in meleeCritSet
                                                    where intervalSet.Contains(n.Timestamp) == false
                                                    select n;

                                    var rInCritSet = from n in rangeCritSet
                                                 where intervalSet.Contains(n.Timestamp)
                                                 select n;

                                    var rNotInCritSet = from n in rangeCritSet
                                                    where intervalSet.Contains(n.Timestamp) == false
                                                    select n;


                                    var wInSet = from n in wsSet
                                                 where intervalSet.Contains(n.Timestamp)
                                                 select n;

                                    var wNotInSet = from n in wsSet
                                                    where intervalSet.Contains(n.Timestamp) == false
                                                    select n;


                                    CalcMinMaxAvgAmount(mInSet, out mInMin, out mInMax, out mInAvg, out mInCount);

                                    CalcMinMaxAvgAmount(mNotInSet, out mNotInMin, out mNotInMax, out mNotInAvg, out mNotInCount);

                                    CalcMinMaxAvgAmount(rInSet, out rInMin, out rInMax, out rInAvg, out rInCount);

                                    CalcMinMaxAvgAmount(rNotInSet, out rNotInMin, out rNotInMax, out rNotInAvg, out rNotInCount);

                                    CalcMinMaxAvgAmount(mInCritSet, out mInCritMin, out mInCritMax, out mInCritAvg, out mInCritCount);

                                    CalcMinMaxAvgAmount(mNotInCritSet, out mNotInCritMin, out mNotInCritMax, out mNotInCritAvg, out mNotInCritCount);

                                    CalcMinMaxAvgAmount(rInCritSet, out rInCritMin, out rInCritMax, out rInCritAvg, out rInCritCount);

                                    CalcMinMaxAvgAmount(rNotInCritSet, out rNotInCritMin, out rNotInCritMax, out rNotInCritAvg, out rNotInCritCount);

                                    CalcMinMaxAvgAmount(wInSet, out wInMin, out wInMax, out wInAvg, out wInCount);

                                    CalcMinMaxAvgAmount(wNotInSet, out wNotInMin, out wNotInMax, out wNotInAvg, out wNotInCount);


                                    int mInSetCount = mInCount + mInCritCount;
                                    int mNotInSetCount = mNotInCount + mNotInCritCount;
                                    int mCount = mInSetCount + mNotInSetCount;

                                    if (mCount > 0)
                                        mInSetRate = (double) mInSetCount / mCount;
                                    else
                                        mInSetRate = 0;

                                    int rInSetCount = rInCount + rInCritCount;
                                    int rNotInSetCount = rNotInCount + rNotInCritCount;
                                    int rCount = rInSetCount + rNotInSetCount;

                                    if (rCount > 0)
                                        rInSetRate = (double) rInSetCount / rCount;
                                    else
                                        rInSetRate = 0;

                                    int wCount = wInCount + wNotInCount;

                                    if (wCount > 0)
                                        wInSetRate = (double) wInCount / wCount;
                                    else
                                        wInSetRate = 0;

                                    int inCount = mInSetCount + rInSetCount + wInCount;

                                    // In sets
                                    string plusString = string.Format("+{0,20}", intervalSet.SetName);
                                    string minusString = string.Format("-{0,20}", intervalSet.SetName);

                                    if (inCount > 0)
                                    {
                                        if (mCount > 0)
                                        {
                                            sb.AppendFormat("{0,21}   {1,6} {2,12} {3,10:f2} {4,12} {5,10:f2}  {6,19:p2}\n",
                                                plusString,
                                                "Melee",
                                                string.Format("{0}/{1}", mInMin, mInMax),
                                                mInAvg,
                                                string.Format("{0}/{1}", mInCritMin, mInCritMax),
                                                mInCritAvg,
                                                mInSetRate);
                                            //tmpString = string.Empty;
                                        }

                                        if (mCount > 0)
                                        {
                                            sb.AppendFormat("{0,21}   {1,6} {2,12} {3,10:f2} {4,12} {5,10:f2}  {6,19:p2}\n",
                                                minusString,
                                                "Melee",
                                                string.Format("{0}/{1}", mNotInMin, mNotInMax),
                                                mNotInAvg,
                                                string.Format("{0}/{1}", mNotInCritMin, mNotInCritMax),
                                                mNotInCritAvg,
                                                1 - mInSetRate);
                                            //tmpString = string.Empty;
                                        }


                                        if (rCount > 0)
                                        {
                                            sb.AppendFormat("{0,21}   {1,6} {2,12} {3,10:f2} {4,12} {5,10:f2}  {6,19:p2}\n",
                                                plusString,
                                                "Ranged",
                                                string.Format("{0}/{1}", rInMin, rInMax),
                                                rInAvg,
                                                string.Format("{0}/{1}", rInCritMin, rInCritMax),
                                                rInCritAvg,
                                                rInSetRate);
                                            //tmpString = string.Empty;
                                        }

                                        if (rCount > 0)
                                        {
                                            sb.AppendFormat("{0,21}   {1,6} {2,12} {3,10:f2} {4,12} {5,10:f2}  {6,19:p2}\n",
                                                minusString,
                                                "Ranged",
                                                string.Format("{0}/{1}", rNotInMin, rNotInMax),
                                                rNotInAvg,
                                                string.Format("{0}/{1}", rNotInCritMin, rNotInCritMax),
                                                rNotInCritAvg,
                                                1 - rInSetRate);
                                            //tmpString = string.Empty;
                                        }


                                        if (wCount > 0)
                                        {
                                            sb.AppendFormat("{0,21}   {1,6} {2,12} {3,10:f2} {4,24} {5,19:p2}\n",
                                                plusString,
                                                "WSkill",
                                                string.Format("{0}/{1}", wInMin, wInMax),
                                                wInAvg,
                                                string.Empty,
                                                wInSetRate);
                                            //tmpString = string.Empty;
                                        }

                                        if (wCount > 0)
                                        {
                                            sb.AppendFormat("{0,21}   {1,6} {2,12} {3,10:f2} {4,24} {5,19:p2}\n",
                                                minusString,
                                                "WSkill",
                                                string.Format("{0}/{1}", wNotInMin, wNotInMax),
                                                wNotInAvg,
                                                string.Empty,
                                                1 - wInSetRate);
                                            //tmpString = string.Empty;
                                        }
                                    }

                                }
                            }
                        }

                        sb.Append("\n\n");
                    }
                }
            }
        }

        private void CalcMinMaxAvgAmount(IEnumerable<KPDatabaseDataSet.InteractionsRow> inSet,
            out int inMin, out int inMax, out double inAvg, out int inCount)
        {
            if (inSet == null)
                throw new ArgumentNullException();

            inMin = 0;
            inMax = 0;
            inAvg = 0;

            int sum = 0;
            inCount = 0;

            var firstRow = inSet.FirstOrDefault();
            if (firstRow == null)
                return;

            inMin = firstRow.Amount;
            inMax = firstRow.Amount;

            foreach (var row in inSet)
            {
                inCount++;
                sum += row.Amount;

                if (row.Amount > inMax)
                    inMax = row.Amount;

                if (row.Amount < inMin)
                    inMin = row.Amount;
            }

            inAvg = (double)sum / inCount;
        }

        /// <summary>
        /// Process/display haste-based attack info.
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="intervalSets"></param>
        /// <param name="playerList"></param>
        /// <param name="sb"></param>
        /// <param name="strModList"></param>
        private void ProcessHaste(KPDatabaseDataSet dataSet,
            List<PlayerTimeIntervalSets> intervalSets, List<string> playerList,
            ref StringBuilder sb, List<StringMods> strModList)
        {
            MobFilter mobFilter;
            if (customMobSelection)
                mobFilter = MobXPHandler.Instance.CustomMobFilter;
            else
                mobFilter = mobsCombo.CBGetMobFilter(exclude0XPMobs);

            int mobCount = mobFilter.Count;

            if (mobCount == 0)
                return;

            var attackSet = from c in dataSet.Combatants
                            where (playerList.Contains(c.CombatantName) &&
                                   RegexUtility.ExcludedPlayer.Match(c.PlayerInfo).Success == false)
                            select new AttackGroup
                            {
                                Name = c.CombatantName,
                                DisplayName = c.CombatantNameOrJobName,
                                Melee = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Melee &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain)) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                        select n,
                                WSkill = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where ((ActionType)n.ActionType == ActionType.Weaponskill &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                                n.Preparing == false) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                         select n,
                            };

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsHaste.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(lsHaste + "\n");


            foreach (var playerInterval in intervalSets)
            {
                if (playerInterval.TimeIntervalSets.Count > 0)
                {
                    var playerActions = attackSet.FirstOrDefault(a => a.Name == playerInterval.PlayerName);

                    if ((playerActions != null) &&
                        ((playerActions.Melee.Count() > 0) || (playerActions.WSkill.Count() > 0)) &&
                        (playerInterval.TimeIntervalSets.Any(s =>
                        CollectTimeIntervals.HasteBuffNames.Contains(s.SetName))))
                    {
                        sb.Append("\n");
                        strModList.Add(new StringMods
                        {
                            Start = sb.Length,
                            Length = playerActions.DisplayName.Length,
                            Bold = true,
                            Color = Color.Blue
                        });
                        sb.Append(playerActions.DisplayName + "\n");

                        strModList.Add(new StringMods
                        {
                            Start = sb.Length,
                            Length = lsHasteHeader.Length,
                            Bold = true,
                            Underline = true
                        });
                        sb.Append(lsHasteHeader + "\n");


                        foreach (var intervalSet in playerInterval.TimeIntervalSets)
                        {
                            if (CollectTimeIntervals.HasteBuffNames.Contains(intervalSet.SetName))
                            {
                                if (intervalSet.TimeIntervals.Count > 0)
                                {
                                    var mInSet = from n in playerActions.Melee
                                                 where intervalSet.Contains(n.Timestamp)
                                                 select n;

                                    var mNotInSet = from n in playerActions.Melee
                                                    where intervalSet.Contains(n.Timestamp) == false
                                                    select n;

                                    var wInSet = from n in playerActions.WSkill
                                                 where intervalSet.Contains(n.Timestamp)
                                                 select n;

                                    var wNotInSet = from n in playerActions.WSkill
                                                    where intervalSet.Contains(n.Timestamp) == false
                                                    select n;

                                    int mCount = mInSet.Count();
                                    int mNotCount = mNotInSet.Count();
                                    int mTotalCount = mCount + mNotCount;

                                    double mInRate = 0;

                                    if (mTotalCount > 0)
                                        mInRate = (double)mCount / mTotalCount;


                                    int wCount = wInSet.Count();
                                    int wNotCount = wNotInSet.Count();
                                    int wTotalCount = wCount + wNotCount;

                                    double wInRate = 0;

                                    if (wTotalCount > 0)
                                        wInRate = (double)wCount / wTotalCount;

                                    int inCount = mCount + wCount;

                                    if (inCount > 0)
                                    {
                                        sb.AppendFormat(" {0,20}   {1,12}   {2,13}   {3,9:p2}   {4,10}   {5,10}   {6,8:p2}\n",
                                            intervalSet.SetName,
                                            mCount,
                                            mNotCount,
                                            mInRate,
                                            wCount,
                                            wNotCount,
                                            wInRate);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            sb.Append("\n\n");
        }

        private void ProcessCritRate(KPDatabaseDataSet dataSet,
            List<PlayerTimeIntervalSets> intervalSets, List<string> playerList,
            ref StringBuilder sb, List<StringMods> strModList)
        {
            MobFilter mobFilter;
            if (customMobSelection)
                mobFilter = MobXPHandler.Instance.CustomMobFilter;
            else
                mobFilter = mobsCombo.CBGetMobFilter(exclude0XPMobs);

            int mobCount = mobFilter.Count;

            if (mobCount == 0)
                return;

            var attackSet = from c in dataSet.Combatants
                            where (playerList.Contains(c.CombatantName) &&
                                   RegexUtility.ExcludedPlayer.Match(c.PlayerInfo).Success == false)
                            select new AttackGroup
                            {
                                Name = c.CombatantName,
                                DisplayName = c.CombatantNameOrJobName,
                                Melee = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Melee &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain)) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                        select n,
                                WSkill = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where ((ActionType)n.ActionType == ActionType.Weaponskill &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                                n.Preparing == false) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                         select n,
                            };

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsCritRate.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(lsCritRate + "\n");


            foreach (var playerInterval in intervalSets)
            {
                if (playerInterval.TimeIntervalSets.Count > 0)
                {
                    var playerActions = attackSet.FirstOrDefault(a => a.Name == playerInterval.PlayerName);

                    if ((playerActions != null) &&
                        ((playerActions.Melee.Count() > 0) || (playerActions.WSkill.Count() > 0)) &&
                        (playerInterval.TimeIntervalSets.Any(s =>
                        CollectTimeIntervals.CritBuffNames.Contains(s.SetName))))
                    {
                        sb.Append("\n");

                        strModList.AddModsAndAppendLineToSB(sb, playerActions.DisplayName,
                            new StringMods { Bold = true, Color = Color.Blue });

                        strModList.AddModsAndAppendLineToSB(sb, lsCritRateHeader,
                            new StringMods { Bold = true, Underline = true });


                        foreach (var intervalSet in playerInterval.TimeIntervalSets)
                        {
                            if (CollectTimeIntervals.CritBuffNames.Contains(intervalSet.SetName))
                            {
                                if (intervalSet.TimeIntervals.Count > 0)
                                {
                                    var mInSet = from n in playerActions.Melee
                                                 where intervalSet.Contains(n.Timestamp)
                                                 select n;

                                    var mNotInSet = from n in playerActions.Melee
                                                    where intervalSet.Contains(n.Timestamp) == false
                                                    select n;

                                    var wInSet = from n in playerActions.WSkill
                                                 where intervalSet.Contains(n.Timestamp)
                                                 select n;

                                    var wNotInSet = from n in playerActions.WSkill
                                                    where intervalSet.Contains(n.Timestamp) == false
                                                    select n;


                                    var mInHit = mInSet.Where(a => (DefenseType)a.DefenseType == DefenseType.None);
                                    var mNisHit = mNotInSet.Where(a => (DefenseType)a.DefenseType == DefenseType.None);

                                    var mInCrit = mInHit.Where(a => (DamageModifier)a.DamageModifier == DamageModifier.Critical);
                                    var mNisCrit = mNisHit.Where(a => (DamageModifier)a.DamageModifier == DamageModifier.Critical);


                                    int mInCritCount = mInCrit.Count();
                                    int mInNonCritCount = mInHit.Count() - mInCritCount;

                                    int mNisCritCount = mNisCrit.Count();
                                    int mNisNonCritCount = mNisHit.Count() - mNisCritCount;

                                    double mInCritRate = 0;
                                    double mNisCritRate = 0;

                                    if (mInHit.Count() > 0)
                                        mInCritRate = (double)mInCritCount / mInHit.Count();

                                    if (mNisHit.Count() > 0)
                                        mNisCritRate = (double)mNisCritCount / mNisHit.Count();

                                    double mAvgInCrit = 0;
                                    double mAvgNisCrit = 0;

                                    if (mInCrit.Count() > 0)
                                        mAvgInCrit = mInCrit.Average(a => a.Amount);

                                    if (mNisCrit.Count() > 0)
                                        mAvgNisCrit = mNisCrit.Average(a => a.Amount);


                                    double avgInWS = 0;
                                    double avgNisWS = 0;

                                    if (wInSet.Count() > 0)
                                        avgInWS = wInSet.Average(a => a.Amount);

                                    if (wNotInSet.Count() > 0)
                                        avgNisWS = wNotInSet.Average(a => a.Amount);


                                    int tInCount = mInSet.Count() + wInSet.Count();
                                    int tNisCount = mNotInSet.Count() + wNotInSet.Count();

                                    int tCount = tInCount + tNisCount;

                                    double tInRate = 0;
                                    double tNisRate = 0;

                                    if (tCount > 0)
                                    {
                                        tInRate = (double)tInCount / (tCount);
                                        tNisRate = (double)tNisCount / (tCount);
                                    }

                                    if (tCount > 0)
                                    {
                                        string plusString = string.Format("+{0,20}", intervalSet.SetName);
                                        string minusString = string.Format("-{0,20}", intervalSet.SetName);

                                        string inCritNonCrit = string.Format("{0}/{1}", mInCritCount, mInNonCritCount);
                                        string nisCritNonCrit = string.Format("{0}/{1}", mNisCritCount, mNisNonCritCount);

                                        sb.AppendFormat(lsCritRateFormat,
                                            plusString,
                                            inCritNonCrit,
                                            mInCritRate,
                                            mAvgInCrit,
                                            avgInWS,
                                            tInRate
                                        );
                                        sb.Append("\n");

                                        sb.AppendFormat(lsCritRateFormat,
                                            minusString,
                                            nisCritNonCrit,
                                            mNisCritRate,
                                            mAvgNisCrit,
                                            avgNisWS,
                                            tNisRate
                                        );
                                        sb.Append("\n");
                                    }
                                }
                            }
                        }
                    }
                }
            }

            sb.Append("\n\n");
        }
        #endregion

        #region Event Handlers
        private void categoryCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            ToolStripComboBox sentBy = sender as ToolStripComboBox;
            if (sentBy != null)
            {
                if (flagNoUpdate == false)
                {
                    processAccuracy = ((sentBy.SelectedIndex == 0) || (sentBy.SelectedIndex == 1));
                    processAttack = ((sentBy.SelectedIndex == 0) || (sentBy.SelectedIndex == 2));
                    processCritRate = ((sentBy.SelectedIndex == 0) || (sentBy.SelectedIndex == 3));
                    processHaste = ((sentBy.SelectedIndex == 0) || (sentBy.SelectedIndex == 4));
                    HandleDataset(null);
                }

                flagNoUpdate = false;
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
            catLabel.Text = Resources.PublicResources.CategoryLabel;

            categoryCombo.Items.Clear();
            categoryCombo.Items.Add(Resources.PublicResources.All);
            categoryCombo.Items.Add("Accuracy");
            categoryCombo.Items.Add("Attack");
            categoryCombo.Items.Add("Crit Rate");
            categoryCombo.Items.Add("Haste");
            categoryCombo.SelectedIndex = 0;
            processAccuracy = true;
            processAttack = true;
            processCritRate = true;
            processHaste = true;

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
        }

        protected override void LoadResources()
        {
            this.tabName = Resources.Combat.BuffsByTimePluginTabName;

            lsAll = Resources.PublicResources.All;

            lsBuffUsedHeader = Resources.Combat.BuffPluginUsedHeader;
            lsBuffRecHeader = Resources.Combat.BuffPluginReceivedHeader;
            lsNumTimesFormat = Resources.Combat.BuffPluginNumTimesFormat;
            lsIntervalsFormat = Resources.Combat.BuffPluginIntervalsFormat;

            lsSelf = Resources.Combat.BuffPluginSelf;

            lsAccuracy = Resources.Combat.BuffsByTimePluginAccuracy;
            lsAccuracyHeader = Resources.Combat.BuffsByTimePluginAccuracyHeader;
            lsAttack = Resources.Combat.BuffsByTimePluginAttack;
            lsAttackHeader = Resources.Combat.BuffsByTimePluginAttackHeader;
            lsCritRate = "Crit Rate";
            lsCritRateHeader = "Buff                      Crit/Non-crit       Crit%     Avg Crit     Avg WS     Buff (In)Active%";
            lsCritRateFormat = "{0,21}   {1,15}   {2,9:p2}   {3,10:f2}   {4,8:f2}   {5,18:p2}";
            lsHaste = Resources.Combat.BuffsByTimePluginHaste;
            lsHasteHeader = Resources.Combat.BuffsByTimePluginHasteHeader;
        }
        #endregion

    }
}

