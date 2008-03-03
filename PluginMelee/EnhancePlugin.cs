using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using WaywardGamers.KParser;
using System.Diagnostics;

namespace WaywardGamers.KParser.Plugin
{
    public class EnhancePlugin : BasePluginControlWithDropdown
    {
        #region IPlugin Overrides
        public override string TabName
        {
            get { return "Buffs"; }
        }

        public override void Reset()
        {
            richTextBox.Clear();
            richTextBox.WordWrap = false;

            label1.Text = "Category";
            comboBox1.Left = label1.Right + 10;
            comboBox1.Items.Clear();
            comboBox1.Items.Add("Buffs Used");
            comboBox1.Items.Add("Buffs Received");
            comboBox1.SelectedIndex = 0;

            label2.Enabled = false;
            label2.Visible = false;
            comboBox2.Enabled = false;
            comboBox2.Visible = false;

            checkBox1.Enabled = false;
            checkBox1.Visible = false;
            checkBox2.Enabled = false;
            checkBox2.Visible = false;
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
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
            richTextBox.Clear();

            if (comboBox1.SelectedIndex == 0)
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
                                    where b.AidType == (byte)AidType.Enhance &&
                                          b.Preparing == false
                                    group b by b.ActionsRow.ActionName into ba
                                    orderby ba.Key
                                    select new
                                    {
                                        BuffName = ba.Key,
                                        BuffTargets = from bt in ba
                                                      where bt.IsTargetIDNull() == false
                                                      group bt by bt.CombatantsRowByTargetCombatantRelation.CombatantName into btn
                                                      orderby btn.Key
                                                      select new
                                                      {
                                                          TargetName = btn.Key,
                                                          Buffs = btn.OrderBy(i => i.Timestamp)
                                                      }
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

                AppendBoldText(string.Format("{0}\n", player.Name), Color.Blue);
                AppendBoldUnderText(buffUsedHeader, Color.Black);

                foreach (var buff in player.Buffs)
                {
                    buffName = buff.BuffName;

                    foreach (var target in buff.BuffTargets)
                    {
                        AppendNormalText(buffName.PadRight(20));
                        buffName = "";

                        used = target.Buffs.Count();
                        AppendNormalText(target.TargetName.PadRight(20));
                        AppendNormalText(used.ToString().PadLeft(7));

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

                            AppendNormalText(string.Format("{0,15}{1,15}{2,15}",
                                TimespanString(minInterval), TimespanString(maxInterval), TimespanString(avgInterval)));
                        }


                        AppendNormalText("\n");
                    }
                }

                AppendNormalText("\n");
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
                                    where b.AidType == (byte)AidType.Enhance &&
                                          b.Preparing == false
                                    group b by b.ActionsRow.ActionName into ba
                                    orderby ba.Key
                                    select new
                                    {
                                        BuffName = ba.Key,
                                        BuffCasters = from bt in ba
                                                      where bt.IsActorIDNull() == false
                                                      group bt by bt.CombatantsRowByActorCombatantRelation.CombatantName into btn
                                                      orderby btn.Key
                                                      select new
                                                      {
                                                          CasterName = btn.Key,
                                                          Buffs = btn.OrderBy(i => i.Timestamp)
                                                      }
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

                AppendBoldText(string.Format("{0}\n", player.Name), Color.Blue);
                AppendBoldUnderText(buffRecHeader, Color.Black);

                foreach (var buff in player.Buffs)
                {
                    buffName = buff.BuffName;

                    foreach (var target in buff.BuffCasters)
                    {
                        AppendNormalText(buffName.PadRight(20));
                        buffName = "";

                        used = target.Buffs.Count();
                        AppendNormalText(target.CasterName.PadRight(20));
                        AppendNormalText(used.ToString().PadLeft(7));

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

                            AppendNormalText(string.Format("{0,15}{1,15}{2,15}",
                                TimespanString(minInterval), TimespanString(maxInterval), TimespanString(avgInterval)));
                        }


                        AppendNormalText("\n");
                    }
                }

                AppendNormalText("\n");
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
        protected override void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            HandleDataset(DatabaseManager.Instance.Database);
        }
        #endregion
    }
}
