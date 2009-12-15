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


            List<PlayerTimeIntervalSets> intervalSets = GetTimeIntervals(dataSet, playerList);

            if (intervalSets == null)
                return;

            StringBuilder sb = new StringBuilder();
            List<StringMods> strModList = new List<StringMods>();

            if (processAccuracy)
                ProcessAccuracy(dataSet, intervalSets, playerList, ref sb, strModList);

            if (processAttack)
                ProcessAttack(dataSet, intervalSets, playerList, ref sb, strModList);

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

            List<string> accBuffNames = new List<string>() { "Focus", "Aggressor", "Sharpshot", "Souleater",
                "Diabolic Eye", "Hasso", "Yonin", "Innin", "Sword Madrigal", "Blade Madrigal" };


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

            string accHeader = "Buff                     M.Hit/Miss     MHit%    R.Hit/Miss     RHit%     Buff (In)Active%";
            string tmpString = "Accuracy";

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = tmpString.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(tmpString + "\n");


            foreach (var playerInterval in intervalSets)
            {
                if (playerInterval.TimeIntervalSets.Count > 0)
                {
                    var playerActions = attackSet.FirstOrDefault(a => a.Name == playerInterval.PlayerName);

                    if ((playerActions != null) &&
                        ((playerActions.Melee.Count() > 0) || (playerActions.Range.Count() > 0)) &&
                        (playerInterval.TimeIntervalSets.Any(s => accBuffNames.Contains(s.SetName))))
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
                            Length = accHeader.Length,
                            Bold = true,
                            Underline = true
                        });
                        sb.Append(accHeader + "\n");


                        foreach (var intervalSet in playerInterval.TimeIntervalSets)
                        {
                            if (accBuffNames.Contains(intervalSet.SetName))
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

                                    
                                    double inSetRate = 0;
                                    double notInSetRate = 0;

                                    if (mInSetCount + mNotInSetCount + rInSetCount + rNisHitCount > 0)
                                    {
                                        inSetRate = (double)(mInSetCount + rInSetCount) / (mInSetCount + mNotInSetCount + rInSetCount + rNisHitCount);
                                        notInSetRate = (double)(mNotInSetCount + rNisHitCount) / (mInSetCount + mNotInSetCount + rInSetCount + rNisHitCount);
                                    }

                                    sb.AppendFormat("+{0,20}", intervalSet.SetName);

                                    sb.AppendFormat("{0,14}  {1,8:p2}  {2,12}  {3,8:p2}  {4,19:p2}\n",
                                        string.Format("{0}/{1}", mHitCount, mMissCount), mHitRate,
                                        string.Format("{0}/{1}", rHitCount, rMissCount), rHitRate, inSetRate);

                                    sb.AppendFormat("-{0,20}", intervalSet.SetName);

                                    sb.AppendFormat("{0,14}  {1,8:p2}  {2,12}  {3,8:p2}  {4,19:p2}\n",
                                        string.Format("{0}/{1}", mNisHitCount, mNisMissCount), mNisHitRate,
                                        string.Format("{0}/{1}", rNisHitCount, rNisMissCount), rNisHitRate, notInSetRate);

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

            List<string> attBuffNames = new List<string>()
                {   "Valor Minuet", "Valor Minuet II", "Valor Minuet III", "Valor Minuet IV", "Chaos Roll",
                    "Berserk", "Warcry", "Last Resort", "Souleater", "Hasso", "Defender",
                    "Dia", "Dia II", "Dia III" };


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

            int mNotInMin = 0;
            int mNotInMax = 0;
            double mNotInAvg = 0;

            int rInMin = 0;
            int rInMax = 0;
            double rInAvg = 0;

            int rNotInMin = 0;
            int rNotInMax = 0;
            double rNotInAvg = 0;

            int mInCritMin = 0;
            int mInCritMax = 0;
            double mInCritAvg = 0;

            int mNotInCritMin = 0;
            int mNotInCritMax = 0;
            double mNotInCritAvg = 0;

            int rInCritMin = 0;
            int rInCritMax = 0;
            double rInCritAvg = 0;

            int rNotInCritMin = 0;
            int rNotInCritMax = 0;
            double rNotInCritAvg = 0;

            int wInMin = 0;
            int wInMax = 0;
            double wInAvg = 0;

            int wNotInMin = 0;
            int wNotInMax = 0;
            double wNotInAvg = 0;

            double mInSetRate = 0;
            double rInSetRate = 0;
            double wInSetRate = 0;



            string attHeader = "Buff                      Type      Min/Max    Average   Cr.Min/Max     Cr.Avg     Buff (In)Active%";

            string tmpString = "Attack";

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = tmpString.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(tmpString + "\n");


            foreach (var playerInterval in intervalSets)
            {
                if (playerInterval.TimeIntervalSets.Count > 0)
                {
                    var playerActions = attackSet.FirstOrDefault(a => a.Name == playerInterval.PlayerName);

                    if ((playerActions != null) &&
                        ((playerActions.Melee.Count() > 0) || (playerActions.Range.Count() > 0)) &&
                        (playerInterval.TimeIntervalSets.Any(s => attBuffNames.Contains(s.SetName))))
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
                            Length = attHeader.Length,
                            Bold = true,
                            Underline = true
                        });
                        sb.Append(attHeader + "\n");

                        foreach (var intervalSet in playerInterval.TimeIntervalSets)
                        {
                            if (attBuffNames.Contains(intervalSet.SetName))
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


                                    if (mInSet.Count() > 0)
                                    {
                                        mInMin = mInSet.Min(a => a.Amount);
                                        mInMax = mInSet.Max(a => a.Amount);
                                        mInAvg = mInSet.Average(a => a.Amount);
                                    }
                                    else
                                    {
                                        mInMin = 0;
                                        mInMax = 0;
                                        mInAvg = 0;
                                    }

                                    if (mNotInSet.Count() > 0)
                                    {
                                        mNotInMin = mNotInSet.Min(a => a.Amount);
                                        mNotInMax = mNotInSet.Max(a => a.Amount);
                                        mNotInAvg = mNotInSet.Average(a => a.Amount);
                                    }
                                    else
                                    {
                                        mNotInMin = 0;
                                        mNotInMax = 0;
                                        mNotInAvg = 0;
                                    }

                                    if (rInSet.Count() > 0)
                                    {
                                        rInMin = rInSet.Min(a => a.Amount);
                                        rInMax = rInSet.Max(a => a.Amount);
                                        rInAvg = rInSet.Average(a => a.Amount);
                                    }
                                    else
                                    {
                                        rInMin = 0;
                                        rInMax = 0;
                                        rInAvg = 0;
                                    }

                                    if (rNotInSet.Count() > 0)
                                    {
                                        rNotInMin = rNotInSet.Min(a => a.Amount);
                                        rNotInMax = rNotInSet.Max(a => a.Amount);
                                        rNotInAvg = rNotInSet.Average(a => a.Amount);
                                    }
                                    else
                                    {
                                        rNotInMin = 0;
                                        rNotInMax = 0;
                                        rNotInAvg = 0;
                                    }

                                    if (mInCritSet.Count() > 0)
                                    {
                                        mInCritMin = mInCritSet.Min(a => a.Amount);
                                        mInCritMax = mInCritSet.Max(a => a.Amount);
                                        mInCritAvg = mInCritSet.Average(a => a.Amount);
                                    }
                                    else
                                    {
                                        mInCritMin = 0;
                                        mInCritMax = 0;
                                        mInCritAvg = 0;
                                    }

                                    if (mNotInCritSet.Count() > 0)
                                    {
                                        mNotInCritMin = mNotInCritSet.Min(a => a.Amount);
                                        mNotInCritMax = mNotInCritSet.Max(a => a.Amount);
                                        mNotInCritAvg = mNotInCritSet.Average(a => a.Amount);
                                    }
                                    else
                                    {
                                        mNotInCritMin = 0;
                                        mNotInCritMax = 0;
                                        mNotInCritAvg = 0;
                                    }

                                    if (rInCritSet.Count() > 0)
                                    {
                                        rInCritMin = rInCritSet.Min(a => a.Amount);
                                        rInCritMax = rInCritSet.Max(a => a.Amount);
                                        rInCritAvg = rInCritSet.Average(a => a.Amount);
                                    }
                                    else
                                    {
                                        rInCritMin = 0;
                                        rInCritMax = 0;
                                        rInCritAvg = 0;
                                    }

                                    if (rNotInCritSet.Count() > 0)
                                    {
                                        rNotInCritMin = rNotInCritSet.Min(a => a.Amount);
                                        rNotInCritMax = rNotInCritSet.Max(a => a.Amount);
                                        rNotInCritAvg = rNotInCritSet.Average(a => a.Amount);
                                    }
                                    else
                                    {
                                        rNotInCritMin = 0;
                                        rNotInCritMax = 0;
                                        rNotInCritAvg = 0;
                                    }

                                    if (wInSet.Count() > 0)
                                    {
                                        wInMin = wInSet.Min(a => a.Amount);
                                        wInMax = wInSet.Max(a => a.Amount);
                                        wInAvg = wInSet.Average(a => a.Amount);
                                    }
                                    else
                                    {
                                        wInMin = 0;
                                        wInMax = 0;
                                        wInAvg = 0;
                                    }

                                    if (wNotInSet.Count() > 0)
                                    {
                                        wNotInMin = wNotInSet.Min(a => a.Amount);
                                        wNotInMax = wNotInSet.Max(a => a.Amount);
                                        wNotInAvg = wNotInSet.Average(a => a.Amount);
                                    }
                                    else
                                    {
                                        wNotInMin = 0;
                                        wNotInMax = 0;
                                        wNotInAvg = 0;
                                    }


                                    int mInSetCount = mInSet.Count() + mInCritSet.Count();
                                    int mNotInSetCount = mNotInSet.Count() + mNotInCritSet.Count();
                                    int mCount = mInSetCount + mNotInSetCount;

                                    if (mCount > 0)
                                        mInSetRate = (double) mInSetCount / mCount;
                                    else
                                        mInSetRate = 0;

                                    int rInSetCount = rInSet.Count() + rInCritSet.Count();
                                    int rNotInSetCount = rNotInSet.Count() + rNotInCritSet.Count();
                                    int rCount = rInSetCount + rNotInSetCount;

                                    if (rCount > 0)
                                        rInSetRate = (double) rInSetCount / rCount;
                                    else
                                        rInSetRate = 0;

                                    int wInSetCount = wInSet.Count();
                                    int wNotInSetCount = wNotInSet.Count();
                                    int wCount = wInSetCount + wNotInSetCount;

                                    if (wCount > 0)
                                        wInSetRate = (double) wInSetCount / wCount;
                                    else
                                        wInSetRate = 0;

                                    // In sets
                                    string plusString = string.Format("+{0,20}", intervalSet.SetName);
                                    string minusString = string.Format("-{0,20}", intervalSet.SetName);

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
                                            1-rInSetRate);
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
                                            1-wInSetRate);
                                       //tmpString = string.Empty;
                                    }


                                }
                            }
                        }

                        sb.Append("\n\n");
                    }
                }
            }
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

            List<string> hasteBuffNames = new List<string>() { "Haste",
                "Advancing March", "Victory March",
                "Hasso", "Haste Samba" };


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

            string hasteHeader = "Buff                    Melee w/Buff   Melee wo/Buff    % w/Buff    WS w/Buff   WS wo/Buff   % w/Buff";
            string tmpString = "Haste";

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = tmpString.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(tmpString + "\n");


            foreach (var playerInterval in intervalSets)
            {
                if (playerInterval.TimeIntervalSets.Count > 0)
                {
                    var playerActions = attackSet.FirstOrDefault(a => a.Name == playerInterval.PlayerName);

                    if ((playerActions != null) &&
                        ((playerActions.Melee.Count() > 0) || (playerActions.WSkill.Count() > 0)) &&
                        (playerInterval.TimeIntervalSets.Any(s => hasteBuffNames.Contains(s.SetName))))
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
                            Length = hasteHeader.Length,
                            Bold = true,
                            Underline = true
                        });
                        sb.Append(hasteHeader + "\n");


                        foreach (var intervalSet in playerInterval.TimeIntervalSets)
                        {
                            if (hasteBuffNames.Contains(intervalSet.SetName))
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

            sb.Append("\n\n");
        }

        #endregion

        #region Get Time-based info collected
        private List<PlayerTimeIntervalSets> GetTimeIntervals(KPDatabaseDataSet dataSet, List<string> playerList)
        {
            List<PlayerTimeIntervalSets> playerIntervals = new List<PlayerTimeIntervalSets>();

            foreach (var player in playerList)
            {
                playerIntervals.Add(new PlayerTimeIntervalSets(player));
            }

            CompileFixedLengthBuffs("Focus", TimeSpan.FromMinutes(2), playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs("Aggressor", TimeSpan.FromMinutes(3), playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs("Sharpshot", TimeSpan.FromMinutes(1), playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs("Souleater", TimeSpan.FromMinutes(1), playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs("Diabolic Eye", TimeSpan.FromMinutes(3), playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs("Berserk", TimeSpan.FromMinutes(3), playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs("Defender", TimeSpan.FromMinutes(3), playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs("Warcry", TimeSpan.FromSeconds(30), playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs("Last Resort", TimeSpan.FromSeconds(30), playerList, playerIntervals, dataSet);
            CompileFixedLengthBuffs("Haste", TimeSpan.FromMinutes(3), playerList, playerIntervals, dataSet);

            CompileStanceBuffs("Hasso", "Seigan", TimeSpan.FromMinutes(5), playerList, playerIntervals, dataSet);
            CompileStanceBuffs("Yonin", "Innin", TimeSpan.FromMinutes(5), playerList, playerIntervals, dataSet);
            CompileStanceBuffs("Innin", "Yonin", TimeSpan.FromMinutes(5), playerList, playerIntervals, dataSet);


            string altSongsRegex = "Minuet|Madrigal|March|Mambo|Prelude|Ballad";

            CompileSongBuffs("Valor Minuet", altSongsRegex, TimeSpan.FromSeconds(144), playerList, playerIntervals, dataSet);
            CompileSongBuffs("Valor Minuet II", altSongsRegex, TimeSpan.FromSeconds(144), playerList, playerIntervals, dataSet);
            CompileSongBuffs("Valor Minuet III", altSongsRegex, TimeSpan.FromSeconds(144), playerList, playerIntervals, dataSet);
            CompileSongBuffs("Valor Minuet IV", altSongsRegex, TimeSpan.FromSeconds(144), playerList, playerIntervals, dataSet);
            CompileSongBuffs("Sword Madrigal", altSongsRegex, TimeSpan.FromSeconds(144), playerList, playerIntervals, dataSet);
            CompileSongBuffs("Blade Madrigal", altSongsRegex, TimeSpan.FromSeconds(144), playerList, playerIntervals, dataSet);
            CompileSongBuffs("Hunter's Prelude", altSongsRegex, TimeSpan.FromSeconds(144), playerList, playerIntervals, dataSet);
            CompileSongBuffs("Archer's Prelude", altSongsRegex, TimeSpan.FromSeconds(144), playerList, playerIntervals, dataSet);
            CompileSongBuffs("Advancing March", altSongsRegex, TimeSpan.FromSeconds(144), playerList, playerIntervals, dataSet);
            CompileSongBuffs("Victory March", altSongsRegex, TimeSpan.FromSeconds(144), playerList, playerIntervals, dataSet);

            string altRollsRegex = "Roll";

            CompileRollBuffs("Hunter's Roll", altRollsRegex, TimeSpan.FromMinutes(5), playerList, playerIntervals, dataSet);
            CompileRollBuffs("Chaos Roll", altRollsRegex, TimeSpan.FromMinutes(5), playerList, playerIntervals, dataSet);


            //CompileDebuffs("Gravity", TimeSpan.FromMinutes(1), playerList, playerIntervals, dataSet);

            CompileSambaBuffs("Haste Samba", "Samba", TimeSpan.FromMinutes(2), playerList, playerIntervals, dataSet);

            CompileDebuffsWithOR("Dia", "Bio", TimeSpan.FromMinutes(1), playerList, playerIntervals, dataSet);
            CompileDebuffsWithOR("Dia II", "Bio II", TimeSpan.FromMinutes(1), playerList, playerIntervals, dataSet);
            CompileDebuffsWithOR("Dia III", "Bio III", TimeSpan.FromMinutes(1), playerList, playerIntervals, dataSet);


            return playerIntervals;
        }

        private void CompileFixedLengthBuffs(string buffName, TimeSpan duration,
           List<string> playerList, List<PlayerTimeIntervalSets> playerIntervals,
           KPDatabaseDataSet dataSet)
        {
            var allBuffActions = from a in dataSet.Actions
                                 where a.ActionName == buffName
                                 select a.GetInteractionsRows();

            List<KPDatabaseDataSet.InteractionsRow> iBuffList = new List<KPDatabaseDataSet.InteractionsRow>();

            foreach (var actionSet in allBuffActions)
            {
                iBuffList.AddRange(actionSet);
            }

            var buffsByTarget = from i in iBuffList
                                where i.IsActorIDNull() == false &&
                                      i.Preparing == false
                                let targetName = (i.IsTargetIDNull() == true) ? i.CombatantsRowByActorCombatantRelation.CombatantName : i.CombatantsRowByTargetCombatantRelation.CombatantName
                                where playerList.Contains(targetName)
                                group i by targetName into sn
                                select sn;


            if ((buffsByTarget != null) && (buffsByTarget.Count() > 0))
            {
                foreach (var targetBuffActions in buffsByTarget)
                {
                    if (targetBuffActions.Count() > 0)
                    {
                        var playerIntervalSet = playerIntervals.First(s => s.PlayerName == targetBuffActions.Key);
                        TimeIntervalSet intervalSet = new TimeIntervalSet(buffName);

                        foreach (var buff in targetBuffActions)
                        {
                            intervalSet.Add(new TimeInterval(buff.Timestamp, duration));
                        }

                        playerIntervalSet.AddIntervalSet(intervalSet);
                    }
                }
            }
        }

        private void CompileStanceBuffs(string stanceBuffName, string oppositeBuffName, TimeSpan duration,
           List<string> playerList, List<PlayerTimeIntervalSets> playerIntervals, KPDatabaseDataSet dataSet)
        {
            var buffUses = from a in dataSet.Actions
                           where Regex.Match(a.ActionName, stanceBuffName).Success ||
                                 Regex.Match(a.ActionName, oppositeBuffName).Success
                           select new
                           {
                               BuffName = a.ActionName,
                               PlayerUses = from i in a.GetInteractionsRows()
                                            where (i.IsTargetIDNull() == true ||
                                                  i.IsActorIDNull() == false) &&
                                                  (AidType)i.AidType == AidType.Enhance // redundant
                                            let userName = (i.IsTargetIDNull() == true) ? i.CombatantsRowByActorCombatantRelation.CombatantName : i.CombatantsRowByTargetCombatantRelation.CombatantName
                                            where playerList.Contains(userName)
                                            group i by userName into iu
                                            select new
                                            {
                                                UserName = iu.Key,
                                                Buffs = iu
                                            }

                           };

            var buffUse = buffUses.FirstOrDefault(b => b.BuffName == stanceBuffName);

            if ((buffUse != null) && (buffUse.PlayerUses.Count() > 0))
            {
                foreach (var baseStance in buffUse.PlayerUses)
                {
                    if (baseStance.Buffs.Count() > 0)
                    {
                        var playerIntervalSet = playerIntervals.First(s => s.PlayerName == baseStance.UserName);
                        TimeIntervalSet intervalSet = new TimeIntervalSet(buffUse.BuffName);

                        var oppositeBuffUse = buffUses.FirstOrDefault(b => b.BuffName == oppositeBuffName);
                        var oppositeStances = (oppositeBuffUse != null) ? oppositeBuffUse.PlayerUses.FirstOrDefault(b => b.UserName == baseStance.UserName) : null;
                        KPDatabaseDataSet.InteractionsRow oppositeStance;

                        foreach (var buff in baseStance.Buffs)
                        {
                            oppositeStance = null;
                            DateTime endTime = buff.Timestamp + duration;

                            if (oppositeStances != null)
                            {
                                oppositeStance = oppositeStances.Buffs.FirstOrDefault(b => b.Timestamp > buff.Timestamp
                                    && b.Timestamp < endTime);

                                if (oppositeStance != null)
                                {
                                    var hassoCheck = baseStance.Buffs.FirstOrDefault(b => b.Timestamp > buff.Timestamp
                                        && b.Timestamp < oppositeStance.Timestamp);

                                    if (hassoCheck != null)
                                    {
                                        endTime = hassoCheck.Timestamp;
                                    }
                                    else
                                    {
                                        endTime = oppositeStance.Timestamp;
                                    }
                                }
                            }

                            intervalSet.Add(new TimeInterval(buff.Timestamp, endTime));
                        }

                        playerIntervalSet.AddIntervalSet(intervalSet);
                    }
                }
            }
        }

        private void CompileSongBuffs(string songBuffName, string alternatesRegex, TimeSpan duration,
           List<string> playerList, List<PlayerTimeIntervalSets> playerIntervals,
           KPDatabaseDataSet dataSet)
        {
            var allSongActions = from a in dataSet.Actions
                                 where Regex.Match(a.ActionName, alternatesRegex).Success
                                 select a.GetInteractionsRows();

            List<KPDatabaseDataSet.InteractionsRow> iSongList = new List<KPDatabaseDataSet.InteractionsRow>();

            foreach (var actionSet in allSongActions)
            {
                iSongList.AddRange(actionSet);
            }

            var songsByTarget = from i in iSongList
                                where i.IsActorIDNull() == false &&
                                      i.IsTargetIDNull() == false &&
                                      i.Preparing == false
                                 let targetName = i.CombatantsRowByTargetCombatantRelation.CombatantName
                                 where playerList.Contains(targetName)
                                 group i by targetName into sn
                                 select sn;


            if ((songsByTarget != null)  && (songsByTarget.Count() > 0))
            {
                foreach (var targetSongActions in songsByTarget)
                {
                    if (targetSongActions.Count() > 0)
                    {
                        var orderedTargetSongActions = targetSongActions.OrderBy(a => a.Timestamp);

                        var playerIntervalSet = playerIntervals.First(s => s.PlayerName == targetSongActions.Key);
                        TimeIntervalSet intervalSet = new TimeIntervalSet(songBuffName);

                        foreach (var targettedSong in orderedTargetSongActions.Where(s => s.ActionsRow.ActionName == songBuffName))
                        {
                            DateTime endTime = targettedSong.Timestamp + duration;

                            var checkSongs = orderedTargetSongActions.Where(s => 
                                (s.CombatantsRowByActorCombatantRelation.CombatantName ==
                                 targettedSong.CombatantsRowByActorCombatantRelation.CombatantName) &&
                                (s.Timestamp > targettedSong.Timestamp) &&
                                (s.Timestamp < endTime));

                            if (checkSongs.Count() > 1)
                            {
                                endTime = checkSongs.Skip(1).First().Timestamp;
                            }

                            intervalSet.Add(new TimeInterval(targettedSong.Timestamp, endTime));
                        }

                        playerIntervalSet.AddIntervalSet(intervalSet);
                    }
                }
            }
        }

        private void CompileRollBuffs(string songBuffName, string alternatesRegex, TimeSpan duration,
           List<string> playerList, List<PlayerTimeIntervalSets> playerIntervals,
           KPDatabaseDataSet dataSet)
        {
            var allSongActions = from a in dataSet.Actions
                                 where Regex.Match(a.ActionName, alternatesRegex).Success
                                 select a.GetInteractionsRows();

            List<KPDatabaseDataSet.InteractionsRow> iSongList = new List<KPDatabaseDataSet.InteractionsRow>();

            foreach (var actionSet in allSongActions)
            {
                iSongList.AddRange(actionSet);
            }

            var songsByTarget = from i in iSongList
                                where i.IsActorIDNull() == false &&
                                      i.IsTargetIDNull() == false &&
                                      i.Preparing == false
                                let targetName = i.CombatantsRowByTargetCombatantRelation.CombatantName
                                where playerList.Contains(targetName)
                                group i by targetName into sn
                                select sn;


            if ((songsByTarget != null) && (songsByTarget.Count() > 0))
            {
                foreach (var targetSongActions in songsByTarget)
                {
                    if (targetSongActions.Count() > 0)
                    {
                        var orderedTargetSongActions = targetSongActions.OrderBy(a => a.Timestamp);

                        var groupOrderedTargetRolls = orderedTargetSongActions.
                            GroupAdjacentByTimeLimit<KPDatabaseDataSet.InteractionsRow, DateTime>(
                            i => i.Timestamp, TimeSpan.FromSeconds(45));

                        var playerIntervalSet = playerIntervals.First(s => s.PlayerName == targetSongActions.Key);
                        TimeIntervalSet intervalSet = new TimeIntervalSet(songBuffName);


                        foreach (var targettedSong in groupOrderedTargetRolls.Where(s => s.First().ActionsRow.ActionName == songBuffName))
                        {
                            DateTime endTime = targettedSong.First().Timestamp + duration;

                            var checkSongs = groupOrderedTargetRolls.Where(s =>
                                (s.First().CombatantsRowByActorCombatantRelation.CombatantName ==
                                 targettedSong.First().CombatantsRowByActorCombatantRelation.CombatantName) &&
                                (s.Key > targettedSong.Last().Timestamp) &&
                                (s.Key < endTime));

                            if (checkSongs.Count() > 1)
                            {
                                endTime = checkSongs.Skip(1).First().Key;
                            }

                            intervalSet.Add(new TimeInterval(targettedSong.Key, endTime));
                        }

                        playerIntervalSet.AddIntervalSet(intervalSet);
                    }
                }
            }
        }

        private void CompileDebuffs(string debuffName, TimeSpan duration,
           List<string> playerList, List<PlayerTimeIntervalSets> playerIntervals,
           KPDatabaseDataSet dataSet)
        {
        }

        private void CompileDebuffsWithOR(string debuffName, string overrideDebuffName, TimeSpan duration,
           List<string> playerList, List<PlayerTimeIntervalSets> playerIntervals,
           KPDatabaseDataSet dataSet)
        {
            var allDebuffActions = from a in dataSet.Actions
                                 where a.ActionName == debuffName ||
                                       Regex.Match(a.ActionName, overrideDebuffName).Success
                                 select a.GetInteractionsRows();

            List<KPDatabaseDataSet.InteractionsRow> iDebuffList = new List<KPDatabaseDataSet.InteractionsRow>();

            foreach (var actionSet in allDebuffActions)
            {
                iDebuffList.AddRange(actionSet);
            }

            var debuffsByBattle = from d in iDebuffList
                                  where d.IsBattleIDNull() == false
                                  group d by d.BattleID into db
                                  select db;

            TimeIntervalSet intervalSet = new TimeIntervalSet(debuffName);
            DateTime prevDebuffUseTime;

            if ((debuffsByBattle != null) && (debuffsByBattle.Count() > 0))
            {
                foreach (var battleSet in debuffsByBattle)
                {
                    prevDebuffUseTime = DateTime.MinValue;

                    foreach (var debuff in battleSet.Where(d => d.ActionsRow.ActionName == debuffName))
                    {
                        if (prevDebuffUseTime == DateTime.MinValue)
                            prevDebuffUseTime = debuff.Timestamp;

                        DateTime endTime = debuff.Timestamp + duration;

                        var overrideDebuff = battleSet.FirstOrDefault(d => Regex.Match(d.ActionsRow.ActionName, overrideDebuffName).Success);

                        if (overrideDebuff != null)
                        {
                            if (overrideDebuff.Timestamp < endTime)
                                endTime = overrideDebuff.Timestamp;
                        }

                        intervalSet.Add(new TimeInterval(debuff.Timestamp, endTime));

                    }
                }
            }

            if (intervalSet.TimeIntervals.Count > 0)
            {
                foreach (var player in playerList)
                {
                    var playerIntervalSet = playerIntervals.First(s => s.PlayerName == player);

                    playerIntervalSet.AddIntervalSet(intervalSet);
                }
            }
        }

        private void CompileSambaBuffs(string buffName, string overrideBuffs, TimeSpan duration,
           List<string> playerList, List<PlayerTimeIntervalSets> playerIntervals,
           KPDatabaseDataSet dataSet)
        {
            var allBuffActions = from a in dataSet.Actions
                                 where Regex.Match(a.ActionName, overrideBuffs).Success
                                 select a.GetInteractionsRows();

            List<KPDatabaseDataSet.InteractionsRow> iBuffList = new List<KPDatabaseDataSet.InteractionsRow>();

            foreach (var actionSet in allBuffActions)
            {
                iBuffList.AddRange(actionSet);
            }

            if (iBuffList.Count == 0)
                return;

            var sambaBuffList = iBuffList.OrderBy(a => a.Timestamp).ToList<KPDatabaseDataSet.InteractionsRow>();

            TimeIntervalSet intervalSet = new TimeIntervalSet(buffName);

            var sambaBuffs = from i in sambaBuffList
                             where i.ActionsRow.ActionName == buffName
                             select i;

            foreach (var samba in sambaBuffs)
            {
                DateTime endTime = samba.Timestamp + duration;

                var limitSamba = sambaBuffList.Find(b => b.Timestamp > samba.Timestamp && b.Timestamp < endTime);

                if (limitSamba != null)
                    endTime = limitSamba.Timestamp;

                intervalSet.Add(new TimeInterval(samba.Timestamp, endTime));
            }

            if (intervalSet.TimeIntervals.Count > 0)
            {
                foreach (var player in playerList)
                {
                    var playerIntervalSet = playerIntervals.First(s => s.PlayerName == player);

                    playerIntervalSet.AddIntervalSet(intervalSet);
                }
            }
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
                    processHaste = ((sentBy.SelectedIndex == 0) || (sentBy.SelectedIndex == 3));
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
            categoryCombo.Items.Add("Haste");
            categoryCombo.SelectedIndex = 0;
            processAccuracy = true;
            processAttack = true;
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
            this.tabName = "Buffs by Time";

            lsAll = Resources.PublicResources.All;

            lsBuffUsedHeader = Resources.Combat.BuffPluginUsedHeader;
            lsBuffRecHeader = Resources.Combat.BuffPluginReceivedHeader;
            lsNumTimesFormat = Resources.Combat.BuffPluginNumTimesFormat;
            lsIntervalsFormat = Resources.Combat.BuffPluginIntervalsFormat;

            lsSelf = Resources.Combat.BuffPluginSelf;
        }
        #endregion

    }
}

