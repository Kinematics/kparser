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
    public class EnhancePlugin : BasePluginControl
    {
        #region Constructor
        bool processBuffsUsed;
        ToolStripLabel catLabel = new ToolStripLabel();
        ToolStripComboBox categoryCombo = new ToolStripComboBox();

        public EnhancePlugin()
        {
            catLabel.Text = "Category:";
            toolStrip.Items.Add(catLabel);

            categoryCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            categoryCombo.Items.Add("Buffs Used");
            categoryCombo.Items.Add("Buffs Received");
            categoryCombo.SelectedIndex = 0;
            categoryCombo.SelectedIndexChanged += new EventHandler(this.categoryCombo_SelectedIndexChanged);
            toolStrip.Items.Add(categoryCombo);
        }
        #endregion

        #region IPlugin Overrides
        public override string TabName
        {
            get { return "Buffs"; }
        }

        public override void Reset()
        {
            ResetTextBox();
            processBuffsUsed = true;
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
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
                        datasetToUse = e.Dataset;
                        return true;
                    }
                }
            }

            datasetToUse = null;
            return false;
        }
        #endregion

        #region Member Variables
        string buffUsedHeader = "Buff                Used on             # Times   Min Interval   Max Interval   Avg Interval\n";
        string buffRecHeader = "Buff                Used by             # Times   Min Interval   Max Interval   Avg Interval\n";
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
                        where ((c.CombatantType == (byte)EntityType.Player) ||
                              (c.CombatantType == (byte)EntityType.Pet) ||
                              (c.CombatantType == (byte)EntityType.Fellow))
                        orderby c.CombatantType, c.CombatantName
                        select new
                        {
                            Name = c.CombatantName,
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



            int used;
            List<TimeSpan> intervalList = new List<TimeSpan>();
            TimeSpan minInterval;
            TimeSpan maxInterval;
            TimeSpan avgInterval;
            //double percActive;

            string buffName;

            //StringBuilder sb = new StringBuilder();

            foreach (var player in buffs)
            {
                if ((player.Buffs == null) || (player.Buffs.Count() == 0))
                    continue;

                AppendText(string.Format("{0}\n", player.Name), Color.Blue, true, false);
                AppendText(buffUsedHeader, Color.Black, true, true);

                foreach (var buff in player.Buffs)
                {
                    buffName = buff.BuffName;

                    if (buff.SelfTargeted.Count() > 0)
                    {
                        AppendText(buffName.PadRight(20));
                        AppendText("Self".PadRight(20));

                        var allDistinctBuffs = buff.SelfTargeted.Distinct(new KPDatabaseDataSet.InteractionTimestampComparer());
                        used = allDistinctBuffs.Count();

                        AppendText(used.ToString().PadLeft(7));

                        if (used > 1)
                        {
                            avgInterval = TimeSpan.FromSeconds(
                                (allDistinctBuffs.Last().Timestamp - allDistinctBuffs.First().Timestamp).TotalSeconds / (used - 1));

                            intervalList.Clear();

                            for (int i = 1; i < used; i++)
                            {
                                var curr = allDistinctBuffs.ElementAt(i);
                                var last = allDistinctBuffs.ElementAt(i - 1);

                                intervalList.Add(curr.Timestamp - last.Timestamp);
                            }

                            minInterval = intervalList.Min();
                            maxInterval = intervalList.Max();

                            AppendText(string.Format("{0,15}{1,15}{2,15}",
                                TimespanString(minInterval), TimespanString(maxInterval), TimespanString(avgInterval)));
                        }

                        AppendText("\n");
                    }
                    else
                    {
                        foreach (var target in buff.BuffTargets)
                        {
                            AppendText(buffName.PadRight(20));
                            buffName = "";

                            used = target.Buffs.Count();
                            AppendText(target.TargetName.PadRight(20));
                            AppendText(used.ToString().PadLeft(7));

                            if (used > 1)
                            {
                                avgInterval = TimeSpan.FromSeconds(
                                    (target.Buffs.Last().Timestamp - target.Buffs.First().Timestamp).TotalSeconds / (used - 1));

                                intervalList.Clear();

                                for (int i = 1; i < used; i++)
                                {
                                    var curr = target.Buffs.ElementAt(i);
                                    var last = target.Buffs.ElementAt(i - 1);

                                    intervalList.Add(curr.Timestamp - last.Timestamp);
                                }

                                minInterval = intervalList.Min();
                                maxInterval = intervalList.Max();

                                AppendText(string.Format("{0,15}{1,15}{2,15}",
                                    TimespanString(minInterval), TimespanString(maxInterval), TimespanString(avgInterval)));
                            }

                            AppendText("\n");
                        }
                    }
                }

                AppendText("\n");
            }
        }

        private void ProcessBuffsReceived(KPDatabaseDataSet dataSet)
        {
            var buffs = from c in dataSet.Combatants
                        where ((c.CombatantType == (byte)EntityType.Player) ||
                              (c.CombatantType == (byte)EntityType.Pet) ||
                              (c.CombatantType == (byte)EntityType.Fellow))
                        orderby c.CombatantType, c.CombatantName
                        select new
                        {
                            Name = c.CombatantName,
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
                                                          CasterName = btn.Key,
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



            int used;
            List<TimeSpan> intervalList = new List<TimeSpan>();
            TimeSpan minInterval;
            TimeSpan maxInterval;
            TimeSpan avgInterval;
            //double percActive;

            string buffName;

            //StringBuilder sb = new StringBuilder();

            foreach (var player in buffs)
            {
                if ((player.Buffs == null) || (player.Buffs.Count() == 0))
                    continue;

                AppendText(string.Format("{0}\n", player.Name), Color.Blue, true, false);
                AppendText(buffRecHeader, Color.Black, true, true);

                if (player.Buffs.Count() > 0)
                {
                    foreach (var buff in player.Buffs)
                    {
                        buffName = buff.BuffName;

                        foreach (var target in buff.BuffCasters)
                        {
                            AppendText(buffName.PadRight(20));
                            buffName = "";

                            used = target.Buffs.Count();
                            AppendText(target.CasterName.PadRight(20));
                            AppendText(used.ToString().PadLeft(7));

                            if (used > 1)
                            {
                                avgInterval = TimeSpan.FromSeconds(
                                    (target.Buffs.Last().Timestamp - target.Buffs.First().Timestamp).TotalSeconds / (used - 1));

                                intervalList.Clear();

                                for (int i = 1; i < used; i++)
                                {
                                    var curr = target.Buffs.ElementAt(i);
                                    var last = target.Buffs.ElementAt(i - 1);

                                    intervalList.Add(curr.Timestamp - last.Timestamp);
                                }

                                minInterval = intervalList.Min();
                                maxInterval = intervalList.Max();

                                AppendText(string.Format("{0,15}{1,15}{2,15}",
                                    TimespanString(minInterval), TimespanString(maxInterval), TimespanString(avgInterval)));
                            }


                            AppendText("\n");
                        }
                    }
                }

                if (player.SelfBuffs.Count() > 0)
                {
                    foreach (var buff in player.SelfBuffs)
                    {
                        buffName = buff.BuffName;

                        AppendText(buffName.PadRight(20));
                        AppendText("Self".PadRight(20));

                        var allDistinctBuffs = buff.Buffs.Distinct(new KPDatabaseDataSet.InteractionTimestampComparer());
                        used = allDistinctBuffs.Count();

                        AppendText(used.ToString().PadLeft(7));

                        if (used > 1)
                        {
                            avgInterval = TimeSpan.FromSeconds(
                                (allDistinctBuffs.Last().Timestamp - allDistinctBuffs.First().Timestamp).TotalSeconds / (used - 1));

                            intervalList.Clear();

                            for (int i = 1; i < used; i++)
                            {
                                var curr = allDistinctBuffs.ElementAt(i);
                                var last = allDistinctBuffs.ElementAt(i - 1);

                                intervalList.Add(curr.Timestamp - last.Timestamp);
                            }

                            minInterval = intervalList.Min();
                            maxInterval = intervalList.Max();

                            AppendText(string.Format("{0,15}{1,15}{2,15}",
                                TimespanString(minInterval), TimespanString(maxInterval), TimespanString(avgInterval)));
                        }

                        AppendText("\n");
                    }
                }


                AppendText("\n");
            }

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
                HandleDataset(DatabaseManager.Instance.Database);
            }
        }
        #endregion
    }
}
