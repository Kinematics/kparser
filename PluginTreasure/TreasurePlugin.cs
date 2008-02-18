using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Drawing;

namespace WaywardGamers.KParser.Plugin
{
    public class TreasurePlugin : BasePluginControlWithRadio
    {
        public override string TabName
        {
            get { return "Loot"; }
        }

        public override void Reset()
        {
            richTextBox.Clear();
            radioButton1.Text = "Summary";
            radioButton2.Left = radioButton1.Right + 30;
            radioButton2.Text = "Drop Rates";
            radioButton3.Enabled = false;
            radioButton3.Visible = false;

            radioButton1.Checked = true;
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
        {
            // Only update when something in the loot table changes, or if
            // the battle tables change (ie: mob was killed)
            if ((e.DatasetChanges.Loot.Count != 0) ||
                (e.DatasetChanges.Battles.Any(b => b.Killed == true)))
            {
                datasetToUse = e.Dataset;
                return true;
            }

            datasetToUse = null;
            return false;
        }

        protected override void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            richTextBox.Clear();
            HandleDataset(DatabaseManager.Instance.Database);
        }

        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            // For now, rebuild the entire page each time.
            richTextBox.Clear();

            if (radioButton1.Checked == true)
                ProcessSummary(dataSet);
            else
                ProcessDropRates(dataSet);
        }

        private void ProcessSummary(KPDatabaseDataSet dataSet)
        {
            // All items
            AppendBoldText("Item Drops\n", Color.Red);
            string dropListFormat = "{0,9} {1}\n";

            int totalGil = 0;
            string gilPlayerName = string.Empty;

            var gilItem = dataSet.Items.SingleOrDefault(i => i.ItemName == "Gil");
            if (gilItem != null)
            {
                gilPlayerName = gilItem.GetLootRows().First().CombatantsRow.CombatantName;
                totalGil = gilItem.GetLootRows().Sum(l => l.GilDropped);
            }

            if (totalGil > 0)
            {
                AppendNormalText(string.Format(dropListFormat, totalGil, "Gil"));
            }

            foreach (var item in dataSet.Items)
            {
                if (item.ItemName != "Gil")
                {
                    AppendNormalText(string.Format(dropListFormat,
                        item.GetLootRows().Count(), item.ItemName));
                }
            }

            // Items by player who got them
            var lootByPlayer = from c in dataSet.Combatants
                               where ((c.CombatantType == (byte)EntityType.Player) &&
                                      (c.GetLootRows().Count() != 0))
                               orderby c.CombatantName
                               select new
                               {
                                   Name = c.CombatantName,
                                   LootItems = from l in c.GetLootRows()
                                               group l by l.ItemsRow.ItemName into li
                                               orderby li.Key
                                               select li
                               };


            if (lootByPlayer.Count() > 0)
            {
                AppendBoldText("\n\nDistribution\n", Color.Red);

                foreach (var loot in lootByPlayer)
                {
                    AppendBoldText(string.Format("\n    {0}\n", loot.Name), Color.Black);

                    if (totalGil > 0)
                    {
                        if (gilPlayerName == loot.Name)
                            AppendNormalText(string.Format(dropListFormat, totalGil, "Gil"));
                    }

                    foreach (var lootItem in loot.LootItems)
                    {
                        if (lootItem.Key != "Gil")
                        {
                            AppendNormalText(string.Format(dropListFormat,
                                lootItem.Count(), lootItem.Key));
                        }
                    }
                }
            }
        }

        private void ProcessDropRates(KPDatabaseDataSet dataSet)
        {
            // Drop rate section
            AppendBoldText("Drop Rates\n", Color.Red);
            string dropItemFormat = "{0,9} {1,-28} [Drop Rate: {2,8:p2}]\n";
            string dropGilFormat  = "{0,9} {1,-28} [Average:   {2,8:f2}]\n";
            int mobKillCount;

            var lootByMob = from c in dataSet.Combatants
                            where (c.CombatantType == (byte)EntityType.Mob)
                            orderby c.CombatantName
                            select new
                            {
                                MobName = c.CombatantName,
                                Battles = from b in c.GetBattlesRowsByEnemyCombatantRelation()
                                          where b.Killed == true
                                          select b,
                                Loot = from l in dataSet.Loot
                                       where ((l.IsBattleIDNull() == false) &&
                                              (l.BattlesRow.CombatantsRowByEnemyCombatantRelation.CombatantName == c.CombatantName))
                                       group l by l.ItemsRow.ItemName into li
                                       orderby li.Key
                                       select new
                                       {
                                           LootName = li.Key,
                                           LootDrops = li
                                       }
                            };


            int totalGil;
            double avgGil;
            double avgLoot;

            foreach (var mob in lootByMob)
            {
                mobKillCount = mob.Battles.Count();
                AppendBoldText(string.Format("\n{0} (Killed {1} times)\n", mob.MobName, mobKillCount), Color.Black);

                totalGil = 0;
                avgGil = 0;

                if (mob.Loot != null)
                {
                    if (mob.Loot.Count() == 0)
                    {
                        AppendNormalText("       No drops.\n");
                    }
                    else
                    {
                        var gilLoot = mob.Loot.FirstOrDefault(l => l.LootName == "Gil");

                        if (gilLoot != null)
                        {
                            // Gil among loot dropped
                            totalGil = gilLoot.LootDrops.Sum(l => l.GilDropped);

                            if (mobKillCount > 0)
                                avgGil = (double)totalGil / mobKillCount;

                            AppendNormalText(string.Format(dropGilFormat,
                                totalGil, "Gil", avgGil));
                        }

                        // Non-gil loot
                        foreach (var loot in mob.Loot)
                        {
                            avgLoot = 0;

                            if (loot.LootName != "Gil")
                            {
                                if (mobKillCount > 0)
                                    avgLoot = (double)loot.LootDrops.Count() / mobKillCount;

                                AppendNormalText(string.Format(dropItemFormat,
                                    loot.LootDrops.Count(), loot.LootName, avgLoot));
                            }
                        }
                    }
                }
            }
        }

    }
}
