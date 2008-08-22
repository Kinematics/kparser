using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;

namespace WaywardGamers.KParser.Plugin
{
    public class TreasurePlugin : BasePluginControlWithRadio
    {
        #region IPlugin overrides
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
            radioButton3.Left = radioButton2.Right + 30;
            radioButton3.Text = "Stealing";
            radioButton4.Left = radioButton3.Right + 30;
            radioButton4.Text = "HELM";

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

            if (radioButton4.Checked == true)
            {
                if (e.DatasetChanges.ChatMessages.Any())
                {
                    datasetToUse = e.Dataset;
                    return true;
                }
            }

            datasetToUse = null;
            return false;
        }
        #endregion

        #region Event Handlers
        protected override void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
            {
                richTextBox.Clear();
                HandleDataset(DatabaseManager.Instance.Database);
            }
        }

        protected override void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked == true)
            {
                richTextBox.Clear();
                HandleDataset(DatabaseManager.Instance.Database);
            }
        }

        protected override void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked == true)
            {
                richTextBox.Clear();
                HandleDataset(DatabaseManager.Instance.Database);
            }
        }

        protected override void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked == true)
            {
                richTextBox.Clear();
                HandleDataset(DatabaseManager.Instance.Database);
            }
        }
        #endregion

        #region Process display output
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            // For now, rebuild the entire page each time.
            richTextBox.Clear();

            if (radioButton1.Checked == true)
                ProcessSummary(dataSet);
            else if (radioButton2.Checked == true)
                ProcessDropRates(dataSet);
            else if (radioButton3.Checked == true)
                ProcessStealing(dataSet);
            else if (radioButton4.Checked == true)
                ProcessHELM(dataSet);
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
                if ((item.GetLootRows().Count() > 0) &&
                    (item.ItemName != "Gil"))
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

            var lootByMob = from c in dataSet.Combatants
                            where (c.CombatantType == (byte)EntityType.Mob)
                            orderby c.CombatantName
                            select new
                            {
                                Name = c.CombatantName,
                                Battles = from b in c.GetBattlesRowsByEnemyCombatantRelation()
                                          where b.Killed == true
                                          select b,
                                Loot = from l in dataSet.Loot
                                       where ((l.IsBattleIDNull() == false) &&
                                              (l.BattlesRow.CombatantsRowByEnemyCombatantRelation == c))
                                       group l by l.ItemsRow.ItemName into li
                                       orderby li.Key
                                       select new
                                       {
                                           LootName = li.Key,
                                           LootDrops = li
                                       }
                            };

            var lootByChest = from c in dataSet.Combatants
                              where (c.CombatantType == (byte)EntityType.TreasureChest)
                              orderby c.CombatantName
                              select new
                              {
                                  Name = c.CombatantName,
                                  Battles = from b in c.GetBattlesRowsByEnemyCombatantRelation()
                                            where b.Killed == true
                                            select b,
                                  Loot = from l in dataSet.Loot
                                         where ((l.IsBattleIDNull() == false) &&
                                                (l.BattlesRow.CombatantsRowByEnemyCombatantRelation == c))
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
                int mobKillCount = mob.Battles.Count();
                AppendBoldText(string.Format("\n{0} (Killed {1} times)\n", mob.Name, mobKillCount), Color.Black);

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

            if (lootByChest.Count() > 0)
                AppendBoldText("\n\nTreasure Chests\n", Color.Red);

            foreach (var chest in lootByChest)
            {
                int chestsOpened = chest.Battles.Count();
                AppendBoldText(string.Format("\n{0} (Opened {1} times)\n", chest.Name, chestsOpened), Color.Black);

                totalGil = 0;
                avgGil = 0;

                if (chest.Loot != null)
                {
                    if (chest.Loot.Count() == 0)
                    {
                        AppendNormalText("       No drops.\n");
                    }
                    else
                    {
                        var gilLoot = chest.Loot.FirstOrDefault(l => l.LootName == "Gil");

                        if (gilLoot != null)
                        {
                            // Gil among loot dropped
                            totalGil = gilLoot.LootDrops.Sum(l => l.GilDropped);

                            if (chestsOpened > 0)
                                avgGil = (double)totalGil / chestsOpened;

                            AppendNormalText(string.Format(dropGilFormat,
                                totalGil, "Gil", avgGil));
                        }

                        // Non-gil loot
                        foreach (var loot in chest.Loot)
                        {
                            avgLoot = 0;

                            if (loot.LootName != "Gil")
                            {
                                if (chestsOpened > 0)
                                    avgLoot = (double)loot.LootDrops.Count() / chestsOpened;

                                AppendNormalText(string.Format(dropItemFormat,
                                    loot.LootDrops.Count(), loot.LootName, avgLoot));
                            }
                        }
                    }
                }
            }
        }

        private void ProcessStealing(KPDatabaseDataSet dataSet)
        {
            AppendBoldText("Stealing\n\n", Color.Red);

            var stealByPlayer = from c in dataSet.Combatants
                                where (c.CombatantType == (byte)EntityType.Player)
                                orderby c.CombatantName
                                select new
                                {
                                    Name = c.CombatantName,
                                    StolenFrom = from i in c.GetInteractionsRowsByActorCombatantRelation()
                                                 where i.ActionType == (byte)ActionType.Steal
                                                 group i by i.CombatantsRowByTargetCombatantRelation.CombatantName into ci
                                                 orderby ci.Key
                                                 select new
                                                 {
                                                     TargetName = ci.Key,
                                                     Stolen = from cis in ci
                                                              where cis.IsItemIDNull() == false
                                                              group cis by cis.ItemsRow.ItemName into cisi
                                                              orderby cisi.Key
                                                              select new
                                                              {
                                                                  ItemName = cisi.Key,
                                                                  Items = cisi
                                                              },
                                                     Mugged = from cis in ci
                                                              where cis.Amount > 0
                                                              select cis,
                                                     FailedSteal = from cis in ci
                                                                   where (cis.ActionsRow.ActionName == "Steal" &&
                                                                          cis.FailedActionType != (byte)FailedActionType.None)
                                                                   select cis,
                                                     FailedMug = from cis in ci
                                                                 where (cis.ActionsRow.ActionName == "Mug" &&
                                                                        cis.FailedActionType != (byte)FailedActionType.None)
                                                                 select cis,
                                                 }
                                };

            var stealByPlayerActive = stealByPlayer.Where(s => s.StolenFrom.Count() > 0);

            foreach (var player in stealByPlayerActive)
            {
                AppendBoldText(player.Name + ":\n", Color.Black);
                foreach (var stoleFrom in player.StolenFrom)
                {
                    AppendBoldText(string.Format("  {0}:\n", stoleFrom.TargetName), Color.Black);

                    foreach (var stoleItem in stoleFrom.Stolen)
                    {
                        AppendNormalText(string.Format("    Stole {0} {1} time{2}.\n",
                            stoleItem.ItemName, stoleItem.Items.Count(), stoleItem.Items.Count() > 1 ? "s" : ""));
                    }

                    foreach (var mugged in stoleFrom.Mugged)
                    {
                        AppendNormalText(string.Format("    Mugged {0} gil.\n", mugged.Amount));
                    }


                    if ((stoleFrom.FailedSteal != null) && (stoleFrom.FailedSteal.Count() > 0))
                    {
                        string s = string.Format("    Failed to Steal {0} time{1}\n",
                            stoleFrom.FailedSteal.Count(), stoleFrom.FailedSteal.Count() > 1 ? "s" : "");
                        AppendNormalText(s);
                    }

                    if ((stoleFrom.FailedMug != null) && (stoleFrom.FailedMug.Count() > 0))
                    {
                        string s = string.Format("    Failed to Mug {0} time{1}\n",
                            stoleFrom.FailedMug.Count(), stoleFrom.FailedMug.Count() > 1 ? "s" : "");
                        AppendNormalText(s);
                    }
                }

                AppendNormalText("\n\n");
            }
        }

        #region HELM strings
        private static readonly string item = @"((a|an|the) )?(?<item>\w+( \w+)*)";

        // HELM messages:
        // You successfully harvest a clump of red moko grass!
        // You are unable to harvest anything.
        // You harvest a bunch of gysahl greens, but your sickle breaks.
        // Your sickle breaks!

        private static readonly Regex Harvest = new Regex(string.Format("You (successfully )?harvest {0}(, but your sickle breaks)?(.|!)$", item));
        private static readonly Regex Log     = new Regex(string.Format("You (successfully )?log {0}(, but your hatchet breaks)?(.|!)$", item));
        private static readonly Regex Mine    = new Regex(string.Format("You (successfully )?dig up {0}(, but your pickaxe breaks)?(.|!)$", item));

        private static readonly string harvestFail = "You are unable to harvest anything.";
        private static readonly string loggingFail = "You are unable to log anything.";
        private static readonly string exMineFail = "You are unable to dig up anything.";

        private static readonly string harvestToolBreak = "Your sickle breaks!";
        private static readonly string loggingToolBreak = "Your hatchet breaks!";
        private static readonly string exMineToolBreak = "Your pickaxe breaks!";

        private static readonly Regex harvestBreak = new Regex(@"sickle breaks( in the process)?(\.|!)$");
        private static readonly Regex loggingBreak = new Regex(@"hatchet breaks( in the process)?(\.|!)$");
        private static readonly Regex exMineBreak = new Regex(@"pickaxe breaks( in the process)?(\.|!)$");

        // Chocobo digging messages:
        // You dig and you dig, but find nothing.
        // Obtained: Lauan log.
        // Obtained: Pebble.

        string chocoDigFail = "You dig and you dig, but find nothing.";
        private static readonly Regex Dig = new Regex(string.Format("^Obtained: {0}{1}$", item, @"\."));

        #endregion

        private void ProcessHELM(KPDatabaseDataSet dataSet)
        {
            richTextBox.Clear();

            #region LINQ queries
            var arenaChat = dataSet.ChatMessages.Where(m => (ChatMessageType)m.ChatType == ChatMessageType.Arena);

            var harvestedItems = from ac in arenaChat
                                 where Harvest.Match(ac.Message).Success == true
                                 group ac by Harvest.Match(ac.Message).Groups["item"].Value into acn
                                 orderby acn.Key
                                 select new
                                 {
                                     Item = acn.Key,
                                     Count = acn.Count()
                                 };

            int harvestingBreaks = arenaChat.Count(ac => harvestBreak.Match(ac.Message).Success == true);

            int harvestingFailures = arenaChat.Count(a => a.Message == harvestFail ||
                a.Message == harvestToolBreak);


            var loggedItems = from ac in arenaChat
                              where Log.Match(ac.Message).Success == true
                              group ac by Log.Match(ac.Message).Groups["item"].Value into acn
                              orderby acn.Key
                              select new
                              {
                                  Item = acn.Key,
                                  Count = acn.Count()
                              };

            int loggingBreaks = arenaChat.Count(ac => loggingBreak.Match(ac.Message).Success == true);

            int loggingFailures = arenaChat.Count(a => a.Message == loggingFail ||
                a.Message == loggingToolBreak);


            var minedItems = from ac in arenaChat
                             where Mine.Match(ac.Message).Success == true
                             group ac by Mine.Match(ac.Message).Groups["item"].Value into acn
                             orderby acn.Key
                             select new
                             {
                                 Item = acn.Key,
                                 Count = acn.Count()
                             };


            int miningBreaks = arenaChat.Count(ac => exMineBreak.Match(ac.Message).Success == true);

            int miningFailures = arenaChat.Count(a => a.Message == exMineFail ||
                a.Message == exMineToolBreak);


            var chocoboItems = from ac in arenaChat
                               where Dig.Match(ac.Message).Success == true
                               group ac by Dig.Match(ac.Message).Groups["item"].Value into acn
                               orderby acn.Key
                               select new
                               {
                                   Item = acn.Key,
                                   Count = acn.Count()
                               };

            int chocoboDiggingFailures = arenaChat.Count(ac => ac.Message == chocoDigFail);
            #endregion

            int totalItems;
            int totalCount;
            double avgResult;
            string lineFormat = "  {0,-34} {1,5}  [{2,8:p2}]\n";

            // Harvesting results
            if (harvestedItems.Count() > 0 || harvestingBreaks > 0)
            {
                AppendBoldText("Harvesting:\n", Color.Red);

                totalItems = harvestedItems.Sum(a => a.Count);
                totalCount = totalItems + harvestingFailures;

                foreach (var item in harvestedItems)
                {
                    avgResult = (double)item.Count / totalCount;
                    AppendNormalText(string.Format(lineFormat,
                        item.Item, item.Count, avgResult));
                }

                avgResult = (double)harvestingFailures / totalCount;
                AppendNormalText(string.Format(lineFormat,
                    "Nothing", harvestingFailures, avgResult));

                if (harvestedItems.Count() > 0)
                    AppendNormalText(string.Format("\n  {0,-34} {1,5}\n",
                        "Total Items:", totalItems));

                AppendNormalText(string.Format("  {0,-34} {1,5}\n",
                    "Total Tries:", totalCount));

                avgResult = (double)harvestingBreaks / totalCount;
                AppendNormalText(string.Format("\n  {0,-34} {1,5}  [{2,8:p2}]\n",
                    "Breaks:", harvestingBreaks, avgResult));

                AppendNormalText("\n");
            }

            // Logging results
            if (loggedItems.Count() > 0 || loggingBreaks > 0)
            {
                AppendBoldText("Logging:\n", Color.Red);

                totalItems = loggedItems.Sum(a => a.Count);
                totalCount = totalItems + loggingFailures;

                foreach (var item in loggedItems)
                {
                    avgResult = (double)item.Count / totalCount;
                    AppendNormalText(string.Format(lineFormat,
                        item.Item, item.Count, avgResult));
                }

                avgResult = (double)loggingFailures / totalCount;
                AppendNormalText(string.Format(lineFormat,
                    "Nothing", loggingFailures, avgResult));

                if (loggedItems.Count() > 0)
                    AppendNormalText(string.Format("\n  {0,-34} {1,5}\n",
                        "Total Items:", totalItems));

                AppendNormalText(string.Format("  {0,-34} {1,5}\n",
                    "Total Tries:", totalCount));

                avgResult = (double)loggingBreaks / totalCount;
                AppendNormalText(string.Format("\n  {0,-34} {1,5}  [{2,8:p2}]\n",
                    "Breaks:", loggingBreaks, avgResult));
                
                AppendNormalText("\n");
            }

            // Mining results
            if (minedItems.Count() > 0 || miningBreaks > 0)
            {
                AppendBoldText("Mining/Excavation:\n", Color.Red);

                totalItems = minedItems.Sum(a => a.Count);
                totalCount = totalItems + miningFailures;

                foreach (var item in minedItems)
                {
                    avgResult = (double)item.Count / totalCount;
                    AppendNormalText(string.Format(lineFormat,
                        item.Item, item.Count, avgResult));
                }

                avgResult = (double)miningFailures / totalCount;
                AppendNormalText(string.Format(lineFormat,
                   "Nothing", miningFailures, avgResult));

                if (minedItems.Count() > 0)
                    AppendNormalText(string.Format("\n  {0,-34} {1,5}\n",
                        "Total Items:", totalItems));

                AppendNormalText(string.Format("  {0,-34} {1,5}\n",
                    "Total Tries:", totalCount));

                avgResult = (double)miningBreaks / totalCount;
                AppendNormalText(string.Format("\n  {0,-34} {1,5}  [{2,8:p2}]\n",
                    "Breaks:", miningBreaks, avgResult));
                
                AppendNormalText("\n");
            }

            // Chocobo results
            if (chocoboItems.Count() > 0 || chocoboDiggingFailures > 0)
            {
                AppendBoldText("Chocobo Digging:\n", Color.Red);

                totalCount = chocoboItems.Sum(a => a.Count) + chocoboDiggingFailures;

                foreach (var item in chocoboItems)
                {
                    avgResult = (double)item.Count / totalCount;
                    AppendNormalText(string.Format(lineFormat,
                        item.Item, item.Count, avgResult));
                }

                avgResult = (double)chocoboDiggingFailures / totalCount;
                AppendNormalText(string.Format(lineFormat,
                   "Nothing", chocoboDiggingFailures, avgResult));

                if (chocoboItems.Count() > 0)
                    AppendNormalText(string.Format("\n  {0,-34} {1,5}\n",
                        "Total Items:", chocoboItems.Sum(li => li.Count)));

                AppendNormalText(string.Format("  {0,-34} {1,5}\n",
                    "Total Tries:", totalCount));

                AppendNormalText("\n");
            }

        }

        #endregion

    }
}
