using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WaywardGamers.KParser;
using System.Diagnostics;

namespace WaywardGamers.KParser.Plugin
{
    public class BuffsPlugin : BasePluginControl
    {
        #region Member Variables
        bool processBuffsUsed;
        ToolStripLabel catLabel = new ToolStripLabel();
        ToolStripComboBox categoryCombo = new ToolStripComboBox();

        string lsBuffUsedHeader;
        string lsBuffRecHeader;
        string lsSelf;

        string lsNumTimesFormat;
        string lsIntervalsFormat;
        #endregion

        #region Constructor
        public BuffsPlugin()
        {
            LoadLocalizedUI();

            categoryCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            categoryCombo.SelectedIndex = 0;
            categoryCombo.SelectedIndexChanged += new EventHandler(this.categoryCombo_SelectedIndexChanged);

            toolStrip.Items.Add(catLabel);
            toolStrip.Items.Add(categoryCombo);
        }
        #endregion

        #region IPlugin Overrides
        public override void Reset()
        {
            ResetTextBox();
            processBuffsUsed = true;
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            if (e.DatasetChanges.Interactions != null)
            {
                if (e.DatasetChanges.Interactions.Count != 0)
                {
                    var enhancements = from i in e.DatasetChanges.Interactions
                                       where i.AidType == (byte)AidType.Enhance
                                       select i;

                    if (enhancements.Count() > 0)
                    {
                        HandleDataset(null);
                    }
                }
            }
        }
        #endregion

        #region Processing sections
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            ResetTextBox();

            if (processBuffsUsed == true)
                ProcessBuffsUsed(dataSet);
            else
                ProcessBuffsReceived(dataSet);

        }

        private void ProcessBuffsUsed(KPDatabaseDataSet dataSet)
        {
            var buffs = from c in dataSet.Combatants
                        where (((EntityType)c.CombatantType == EntityType.Player) ||
                              ((EntityType)c.CombatantType == EntityType.Pet) ||
                              ((EntityType)c.CombatantType == EntityType.CharmedMob) ||
                              ((EntityType)c.CombatantType == EntityType.Fellow))
                        orderby c.CombatantType, c.CombatantName
                        select new
                        {
                            Name = c.CombatantNameOrJobName,
                            Buffs = from b in c.GetInteractionsRowsByActorCombatantRelation()
                                    where (b.AidType == (byte)AidType.Enhance ||
                                           b.AidType == (byte)AidType.RemoveStatus ||
                                           b.AidType == (byte)AidType.RemoveEnmity) &&
                                          b.Preparing == false &&
                                          b.IsActionIDNull() == false
                                    group b by b.ActionsRow.ActionName into ba
                                    orderby ba.Key
                                    select new
                                    {
                                        BuffName = ba.Key,
                                        BuffTargets = from bt in ba
                                                      where bt.IsTargetIDNull() == false &&
                                                            bt.CombatantsRowByTargetCombatantRelation.CombatantName != c.CombatantName
                                                      group bt by bt.CombatantsRowByTargetCombatantRelation.CombatantName into btn
                                                      orderby btn.Key
                                                      select new
                                                      {
                                                          TargetName = btn.Key,
                                                          Buffs = btn.OrderBy(i => i.Timestamp)
                                                      },
                                        SelfTargeted = from bt in ba
                                                       where bt.IsTargetIDNull() == true ||
                                                             bt.CombatantsRowByTargetCombatantRelation.CombatantName == c.CombatantName
                                                       orderby bt.Timestamp
                                                       select bt
                                    }
                        };



            StringBuilder sb = new StringBuilder();
            List<StringMods> strModList = new List<StringMods>();

            int used;
            TimeSpan minInterval;
            TimeSpan maxInterval;
            TimeSpan avgInterval;
            TimeSpan thisInterval;
            DateTime thistime;
            DateTime lasttime;

            string buffName;
            bool firstEntryShown;

            foreach (var player in buffs)
            {
                if ((player.Buffs == null) || (player.Buffs.Count() == 0))
                    continue;

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = player.Name.Length,
                    Bold = true,
                    Color = Color.Blue
                });
                sb.AppendFormat("{0}\n", player.Name);

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsBuffUsedHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(lsBuffUsedHeader);
                sb.Append("\n");

                foreach (var buff in player.Buffs)
                {
                    firstEntryShown = false;
                    buffName = buff.BuffName;

                    if ((buff.SelfTargeted.Count() > 0) || (buff.BuffTargets.Count() > 0))
                    {
                        sb.AppendFormat("{0,-20}", buffName);
                    }

                    if (buff.SelfTargeted.Count() > 0)
                    {
                        sb.AppendFormat("{0,-20}", lsSelf);
                        firstEntryShown = true;

                        var allDistinctBuffs = buff.SelfTargeted.Distinct(new KPDatabaseDataSet.InteractionTimestampComparer());
                        used = allDistinctBuffs.Count();

                        sb.AppendFormat(lsNumTimesFormat, used);

                        if (used > 1)
                        {
                            avgInterval = TimeSpan.FromSeconds(
                                (allDistinctBuffs.Last().Timestamp - allDistinctBuffs.First().Timestamp).TotalSeconds / (used - 1));


                            thistime = allDistinctBuffs.First().Timestamp;
                            lasttime = allDistinctBuffs.First().Timestamp;
                            minInterval = TimeSpan.Zero;
                            maxInterval = TimeSpan.Zero;

                            foreach (var distinctBuff in allDistinctBuffs.Where((a, index) => index > 0))
                            {
                                lasttime = thistime;
                                thistime = distinctBuff.Timestamp;
                                thisInterval = thistime - lasttime;
                                if (thisInterval > maxInterval)
                                    maxInterval = thisInterval;
                                if ((thisInterval < minInterval) || (minInterval == TimeSpan.Zero))
                                    minInterval = thisInterval;
                            }

                            sb.AppendFormat(lsIntervalsFormat,
                                TimespanString(minInterval),
                                TimespanString(maxInterval),
                                TimespanString(avgInterval));
                        }

                        sb.Append("\n");
                    }
                    
                    if (buff.BuffTargets.Count() > 0)
                    {
                        foreach (var target in buff.BuffTargets)
                        {
                            if (firstEntryShown == true)
                                sb.AppendFormat("{0,-20}", string.Empty);

                            used = target.Buffs.Count();

                            sb.AppendFormat("{0,-20}", target.TargetName);
                            sb.AppendFormat(lsNumTimesFormat, used);

                            firstEntryShown = true;

                            if (used > 1)
                            {
                                avgInterval = TimeSpan.FromSeconds(
                                    (target.Buffs.Last().Timestamp - target.Buffs.First().Timestamp).TotalSeconds / (used - 1));

                                thistime = target.Buffs.First().Timestamp;
                                lasttime = target.Buffs.First().Timestamp;
                                minInterval = TimeSpan.Zero;
                                maxInterval = TimeSpan.Zero;

                                foreach (var distinctBuff in target.Buffs.Where((a, index) => index > 0))
                                {
                                    lasttime = thistime;
                                    thistime = distinctBuff.Timestamp;
                                    thisInterval = thistime - lasttime;
                                    if (thisInterval > maxInterval)
                                        maxInterval = thisInterval;
                                    if ((thisInterval < minInterval) || (minInterval == TimeSpan.Zero))
                                        minInterval = thisInterval;
                                }

                                sb.AppendFormat(lsIntervalsFormat,
                                    TimespanString(minInterval),
                                    TimespanString(maxInterval),
                                    TimespanString(avgInterval));
                            }

                            sb.Append("\n");
                        }
                    }
                }

                sb.Append("\n");
            }

            PushStrings(sb, strModList);
        }

        private void ProcessBuffsReceived(KPDatabaseDataSet dataSet)
        {
            var buffs = from c in dataSet.Combatants
                        where (((EntityType)c.CombatantType == EntityType.Player) ||
                              ((EntityType)c.CombatantType == EntityType.Pet) ||
                              ((EntityType)c.CombatantType == EntityType.CharmedMob) ||
                              ((EntityType)c.CombatantType == EntityType.Fellow))
                        orderby c.CombatantType, c.CombatantName
                        select new
                        {
                            Name = c.CombatantNameOrJobName,
                            Buffs = from b in c.GetInteractionsRowsByTargetCombatantRelation()
                                    where (b.AidType == (byte)AidType.Enhance ||
                                           b.AidType == (byte)AidType.RemoveStatus ||
                                           b.AidType == (byte)AidType.RemoveEnmity) &&
                                          b.Preparing == false &&
                                          b.IsActionIDNull() == false
                                    group b by b.ActionsRow.ActionName into ba
                                    orderby ba.Key
                                    select new
                                    {
                                        BuffName = ba.Key,
                                        BuffCasters = from bt in ba
                                                      where bt.IsActorIDNull() == false &&
                                                            bt.CombatantsRowByActorCombatantRelation.CombatantName != c.CombatantName
                                                      group bt by bt.CombatantsRowByActorCombatantRelation.CombatantName into btn
                                                      orderby btn.Key
                                                      select new
                                                      {
                                                          CasterName = (btn.Count() > 0) ? btn.First().CombatantsRowByActorCombatantRelation.CombatantNameOrJobName : btn.Key,
                                                          Buffs = btn.OrderBy(i => i.Timestamp)
                                                      },
                                    },
                            SelfBuffs = from bt in c.GetInteractionsRowsByActorCombatantRelation()
                                        where (bt.AidType == (byte)AidType.Enhance ||
                                               bt.AidType == (byte)AidType.RemoveStatus) &&
                                              (bt.IsTargetIDNull() == true ||
                                               bt.CombatantsRowByTargetCombatantRelation.CombatantName == c.CombatantName) &&
                                              bt.Preparing == false &&
                                              bt.IsActionIDNull() == false
                                        group bt by bt.ActionsRow.ActionName into bu
                                        orderby bu.Key
                                        select new
                                        {
                                            BuffName = bu.Key,
                                            Buffs = bu,
                                        }
                        };


            StringBuilder sb = new StringBuilder();
            List<StringMods> strModList = new List<StringMods>();

            int used;
            TimeSpan minInterval;
            TimeSpan maxInterval;
            TimeSpan avgInterval;
            TimeSpan thisInterval;
            DateTime thistime;
            DateTime lasttime;

            string buffName;

            foreach (var player in buffs)
            {
                if ((player.Buffs == null) || (player.Buffs.Count() == 0))
                    continue;

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = player.Name.Length,
                    Bold = true,
                    Color = Color.Blue
                });
                sb.AppendFormat("{0}\n", player.Name);

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsBuffRecHeader.Length,
                    Bold = true,
                    Underline = true,
                    Color = Color.Black
                });
                sb.Append(lsBuffRecHeader);
                sb.Append("\n");

                if (player.Buffs.Count() > 0)
                {
                    foreach (var buff in player.Buffs)
                    {
                        buffName = buff.BuffName;

                        foreach (var target in buff.BuffCasters)
                        {
                            sb.AppendFormat("{0,-20}", buffName);
                            buffName = "";

                            used = target.Buffs.Count();
                            sb.AppendFormat("{0,-20}", target.CasterName);
                            sb.AppendFormat(lsNumTimesFormat, used);

                            if (used > 1)
                            {
                                avgInterval = TimeSpan.FromSeconds(
                                    (target.Buffs.Last().Timestamp - target.Buffs.First().Timestamp).TotalSeconds / (used - 1));

                                thistime = target.Buffs.First().Timestamp;
                                lasttime = target.Buffs.First().Timestamp;
                                minInterval = TimeSpan.Zero;
                                maxInterval = TimeSpan.Zero;

                                foreach (var distinctBuff in target.Buffs.Where((a, index) => index > 0))
                                {
                                    lasttime = thistime;
                                    thistime = distinctBuff.Timestamp;
                                    thisInterval = thistime - lasttime;
                                    if (thisInterval > maxInterval)
                                        maxInterval = thisInterval;
                                    if ((thisInterval < minInterval) || (minInterval == TimeSpan.Zero))
                                        minInterval = thisInterval;
                                }

                                sb.AppendFormat(lsIntervalsFormat,
                                    TimespanString(minInterval),
                                    TimespanString(maxInterval),
                                    TimespanString(avgInterval));
                            }

                            sb.Append("\n");
                        }
                    }
                }

                if (player.SelfBuffs.Count() > 0)
                {
                    foreach (var buff in player.SelfBuffs)
                    {
                        buffName = buff.BuffName;

                        sb.AppendFormat("{0,-20}", buffName);
                        sb.AppendFormat("{0,-20}", "Self");

                        var allDistinctBuffs = buff.Buffs.Distinct(new KPDatabaseDataSet.InteractionTimestampComparer());
                        used = allDistinctBuffs.Count();

                        sb.AppendFormat(lsNumTimesFormat, used);

                        if (used > 1)
                        {
                            avgInterval = TimeSpan.FromSeconds(
                                (allDistinctBuffs.Last().Timestamp - allDistinctBuffs.First().Timestamp).TotalSeconds / (used - 1));

                            thistime = allDistinctBuffs.First().Timestamp;
                            lasttime = allDistinctBuffs.First().Timestamp;
                            minInterval = TimeSpan.Zero;
                            maxInterval = TimeSpan.Zero;

                            foreach (var distinctBuff in allDistinctBuffs.Where((a, index) => index > 0))
                            {
                                lasttime = thistime;
                                thistime = distinctBuff.Timestamp;
                                thisInterval = thistime - lasttime;
                                if (thisInterval > maxInterval)
                                    maxInterval = thisInterval;
                                if ((thisInterval < minInterval) || (minInterval == TimeSpan.Zero))
                                    minInterval = thisInterval;
                            }

                            sb.AppendFormat(lsIntervalsFormat,
                                TimespanString(minInterval),
                                TimespanString(maxInterval),
                                TimespanString(avgInterval));
                        }

                        sb.Append("\n");
                    }
                }

                sb.Append("\n");
            }

            PushStrings(sb, strModList);
        }

        private string TimespanString(TimeSpan timeSpan)
        {
            string tsBlock;

            if (timeSpan.Hours > 0)
                tsBlock = string.Format("{0}:{1:d2}:{2:d2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            else
                tsBlock = string.Format("{0}:{1:d2}", timeSpan.Minutes, timeSpan.Seconds);


            return tsBlock;
        }
        #endregion

        #region Event Handlers
        private void categoryCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            ToolStripComboBox sentBy = sender as ToolStripComboBox;
            if (sentBy != null)
            {
                processBuffsUsed = (sentBy.SelectedIndex == 0);
                HandleDataset(null);
            }
        }
        #endregion

        #region Localization Overrides
        protected override void LoadLocalizedUI()
        {
            catLabel.Text = Resources.PublicResources.CategoryLabel;

            categoryCombo.Items.Clear();
            categoryCombo.Items.Add(Resources.Combat.BuffPluginBuffsUsedCategory);
            categoryCombo.Items.Add(Resources.Combat.BuffPluginBuffsReceivedCategory);
        }

        protected override void LoadResources()
        {
            this.tabName = Resources.Combat.BuffPluginTabName;

            lsBuffUsedHeader = Resources.Combat.BuffPluginUsedHeader;
            lsBuffRecHeader = Resources.Combat.BuffPluginReceivedHeader;
            lsNumTimesFormat = Resources.Combat.BuffPluginNumTimesFormat;
            lsIntervalsFormat = Resources.Combat.BuffPluginIntervalsFormat;

            lsSelf = Resources.Combat.BuffPluginSelf;
        }
        #endregion

    }
}
