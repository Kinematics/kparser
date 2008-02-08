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

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseODataSet datasetToUse)
        {
            // Only update when something in the loot table changes, or if
            // the battle tables change (ie: mob was killed)
            if ((e.DatasetChanges.Loot.Count != 0) ||
                (e.DatasetChanges.Battles.Any(b => b.Killed == true)))
            {
                datasetToUse = e.FullDataset;
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

        protected override void ProcessData(KPDatabaseODataSet dataSet)
        {
            // For now, rebuild the entire page each time.
            richTextBox.Clear();

            if (radioButton1.Checked == true)
            {
                // Summary section

                // All items
                AppendBoldText("Item Drops\n", Color.Red);
                string dropListFormat = "{0} {1}\n";

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
                    AppendNormalText(string.Format(dropListFormat, totalGil.ToString().PadLeft(9), "Gil"));
                }

                foreach (var item in dataSet.Items)
                {
                    if (item.ItemName != "Gil")
                    {
                        AppendNormalText(string.Format(dropListFormat,
                            item.GetLootRows().Count().ToString().PadLeft(9), item.ItemName));
                    }
                }

                // Items by player who got them
                var lootByPlayer = from c in dataSet.Combatants
                                   where ((c.CombatantType == (byte) EntityType.Player) &&
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
                                AppendNormalText(string.Format(dropListFormat, totalGil.ToString().PadLeft(9), "Gil"));
                        }

                        foreach (var lootItem in loot.LootItems)
                        {
                            if (lootItem.Key != "Gil")
                            {
                                AppendNormalText(string.Format(dropListFormat,
                                    lootItem.Count().ToString().PadLeft(9), lootItem.Key));
                            }
                        }
                    }
                }
            }
            else
            {
                // Drop rate section
                AppendBoldText("Drop Rates\n", Color.Red);
                string dropListFormat = "{0} {1} [Drop Rate: {2:p2}]\n";
                int mobKillCount;

                var lootByMob = from b in dataSet.Battles
                                where (b.Killed == true)
                                group b by b.CombatantsRowByEnemyCombatantRelation.CombatantName into bm
                                orderby bm.Key
                                select new
                                {
                                    Name = bm.Key,
                                    Battles = bm,
                                    Loot = from l in dataSet.Loot
                                           where (l.BattlesRow.CombatantsRowByEnemyCombatantRelation.CombatantName == bm.Key)
                                           group l by l.ItemsRow.ItemName into li
                                           orderby li.Key
                                           select li
                                };


                foreach (var mob in lootByMob)
                {
                    mobKillCount = mob.Battles.Count();
                    AppendBoldText(string.Format("\n{0} (Killed {1} times)\n", mob.Name, mobKillCount), Color.Black);

                    if (mob.Loot.Count() == 0)
                    {
                        AppendNormalText("       No drops.\n");
                    }
                    else
                    {
                        var gilLoot = mob.Loot.FirstOrDefault(l => l.Key == "Gil");

                        if (gilLoot != null)
                        {
                            // Gil among loot dropped
                            int totalGil = gilLoot.Sum(l => l.GilDropped);

                            AppendNormalText(string.Format("{0} {1} [Average: {2:f2}]\n",
                                totalGil.ToString().PadLeft(9), "Gil".PadRight(24),
                                (double)totalGil / mobKillCount));
                        }

                        // Non-gil loot
                        foreach (var loot in mob.Loot)
                        {
                            if (loot.Key != "Gil")
                            {
                                AppendNormalText(string.Format(dropListFormat,
                                    loot.Count().ToString().PadLeft(9), loot.Key.PadRight(24),
                                    (double)loot.Count() / mobKillCount));
                            }
                        }
                    }
                }
            }
        }
    }
}
