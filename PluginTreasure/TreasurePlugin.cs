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
        private static readonly string item = @"(([Aa]|[Aa]n|[Tt]he) )?(?<item>(\?\?\? )?.{3,})";

        private static readonly string harvestSuccess = "You successfully harvest ";
        private static readonly string loggingSuccess = "You successfully log ";
        private static readonly string exMineSuccess  = "You successfully dig up ";

        private static readonly string harvestToolBreak = "Your sickle breaks!";
        private static readonly string loggingToolBreak = "Your hatchet breaks!";
        private static readonly string exMineToolBreak  = "Your pickaxe breaks!";

        private static readonly string harvestToolBreakOnSuccess = ", but your sickle breaks in the process";
        private static readonly string loggingToolBreakOnSuccess = ", but your hatchet breaks in the process";
        private static readonly string exMineToolBreakOnSuccess  = ", but your pickaxe breaks in the process";

        private static readonly Regex Log     = new Regex(string.Format("^{0}{1}({2})?.$", loggingSuccess, item, loggingToolBreakOnSuccess));
        private static readonly Regex Harvest = new Regex(string.Format("^{0}{1}({2})?.$", harvestSuccess, item, harvestToolBreakOnSuccess));
        private static readonly Regex Mine    = new Regex(string.Format("^{0}{1}({2})?.$", exMineSuccess, item, exMineToolBreakOnSuccess));
        #endregion

        private void ProcessHELM(KPDatabaseDataSet dataSet)
        {
            richTextBox.Clear();

            #region LINQ queries
            var arenaChat = dataSet.ChatMessages.Where(m => (ChatMessageType)m.ChatType == ChatMessageType.Arena);

            var harvestedItems = from ac in arenaChat
                                 where Harvest.Match(ac.Message).Success == true
                                 group ac by Harvest.Match(ac.Message).Groups["item"].ToString() into acn
                                 orderby acn.Key
                                 select new
                                 {
                                     Item = acn.Key,
                                     Count = acn.Count()
                                 };

            var harvestingBreaks = from ac in arenaChat
                                   where (ac.Message == harvestToolBreak ||
                                   ac.Message.EndsWith(harvestToolBreakOnSuccess + "."))
                                   group ac by "Sickle";

            var loggedItems = from ac in arenaChat
                              where Log.Match(ac.Message).Success == true
                              group ac by Log.Match(ac.Message).Groups["item"].ToString() into acn
                              orderby acn.Key
                              select new
                              {
                                  Item = acn.Key,
                                  Count = acn.Count()
                              };

            var loggingBreaks = from ac in arenaChat
                                where (ac.Message == loggingToolBreak ||
                                   ac.Message.EndsWith(loggingToolBreakOnSuccess + "."))
                                group ac by "Hatchet";

            var minedItems = from ac in arenaChat
                             where Mine.Match(ac.Message).Success == true
                             group ac by Mine.Match(ac.Message).Groups["item"].ToString() into acn
                             orderby acn.Key
                             select new
                             {
                                 Item = acn.Key,
                                 Count = acn.Count()
                             };

            var miningBreaks = from ac in arenaChat
                               where (ac.Message == exMineToolBreak ||
                                   ac.Message.EndsWith(exMineToolBreakOnSuccess + "."))
                               group ac by "Pick";
            #endregion

            if (harvestedItems.Count() > 0 || harvestingBreaks.Count() > 0)
            {
                AppendBoldText("Harvesting:\n", Color.Red);

                foreach (var item in harvestedItems)
                {
                    AppendNormalText(string.Format("  {0,15} {1,-5}\n", item.Item, item.Count));
                }

                if (harvestedItems.Count() > 0)
                    AppendNormalText(string.Format("\n  Total: {0}\n",
                        harvestedItems.Sum(li => li.Count)));

                AppendNormalText(string.Format("\n  Breaks: {0}\n", harvestingBreaks.Count()));

                AppendNormalText("\n");
            }

            if (loggedItems.Count() > 0 || loggingBreaks.Count() > 0)
            {
                AppendBoldText("Logging:\n", Color.Red);

                foreach (var item in loggedItems)
                {
                    AppendNormalText(string.Format("  {0,15} {1,-5}\n", item.Item, item.Count));
                }

                if (loggedItems.Count() > 0)
                    AppendNormalText(string.Format("\n  Total: {0}\n",
                        loggedItems.Sum(li => li.Count)));

                AppendNormalText(string.Format("\n  Breaks: {0}\n", harvestingBreaks.Count()));
                
                AppendNormalText("\n");
            }

            if (minedItems.Count() > 0 || miningBreaks.Count() > 0)
            {
                AppendBoldText("Mining/Excavation:\n", Color.Red);

                foreach (var item in minedItems)
                {
                    AppendNormalText(string.Format("  {0,15} {1,-5}\n", item.Item, item.Count));
                }

                if (minedItems.Count() > 0)
                    AppendNormalText(string.Format("\n  Total: {0}\n",
                        minedItems.Sum(li => li.Count)));

                AppendNormalText(string.Format("\n  Breaks: {0}\n", harvestingBreaks.Count()));
                
                AppendNormalText("\n");
            }


        }

        #endregion

    }
}
