using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Windows.Forms;
using WaywardGamers.KParser.Interface;

namespace WaywardGamers.KParser.Plugin
{
    public class TreasurePlugin : BasePluginControl
    {
        internal enum LootType
        {
            Summary,
            DropRates,
            Stealing,
            HELM,
            Salvage
        }

        #region Member Variables
        ToolStripDropDownButton lootTypeMenu = new ToolStripDropDownButton();
        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();
        LootType currentLootType = LootType.Summary;

        ToolStripMenuItem summaryOption = new ToolStripMenuItem();
        ToolStripMenuItem dropRatesOption = new ToolStripMenuItem();
        ToolStripMenuItem stealingOption = new ToolStripMenuItem();
        ToolStripMenuItem helmOption = new ToolStripMenuItem();
        ToolStripMenuItem salvageOption = new ToolStripMenuItem();

        ToolStripMenuItem groupMobsOption = new ToolStripMenuItem();
        ToolStripMenuItem excludeCrystalsOption = new ToolStripMenuItem();
        ToolStripMenuItem excludedPlayerInfoOption = new ToolStripMenuItem();

        bool showGroupDetails = false;
        bool excludeCrystalsAndSeals = false;
        bool excludedPlayerInfo = true;


        // Localizable strings

        // Display strings
        string lsGil;
        string lsItemDrops;
        string lsDistribution;
        string lsCellDrops;
        string lsDropRates;
        string lsNoDrops;
        string lsTreasureChests;
        string lsNumberOfTimesDropped;
        string lsNothing;

        // Format strings
        string lsDropItemFormat;
        string lsDropGilFormat;
        string lsTimesKilledFormat;
        string lsDroppedItemNTimesFormat;
        string lsOpenedNTimesFormat;
        string lsDropRateFormat;

        // Parse strings
        string lsParsedGil;
        string lsSalvageCell;
        string lsCrystalsAndSealsRegex;

        #endregion

        #region HELM strings
        private static readonly string item = @"((a|an|the) )?(?<item>\w+( \w+)*)";

        // HELM messages:
        // You successfully harvest a clump of red moko grass!
        // You are unable to harvest anything.
        // You harvest a bunch of gysahl greens, but your sickle breaks.
        // Your sickle breaks!

        private static readonly Regex Harvest = new Regex(string.Format("You (successfully )?harvest {0}(, but your sickle breaks( in the process)?)?(.|!)$", item));
        private static readonly Regex Log = new Regex(string.Format("You (successfully )?cut off {0}(, but your hatchet breaks( in the process)?)?(.|!)$", item));
        private static readonly Regex Mine = new Regex(string.Format("You (successfully )?dig up {0}(, but your pickaxe breaks( in the process)?)?(.|!)$", item));

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


        #region Constructor
        public TreasurePlugin()
        {
            LoadLocalizedUI();


            lootTypeMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;

            summaryOption.Click += new EventHandler(summaryOption_Click);
            dropRatesOption.Click += new EventHandler(dropRatesOption_Click);
            stealingOption.Click += new EventHandler(stealingOption_Click);
            helmOption.Click += new EventHandler(helmOption_Click);
            salvageOption.Click += new EventHandler(salvageOption_Click);
            summaryOption.Checked = true;

            lootTypeMenu.DropDownItems.Add(summaryOption);
            lootTypeMenu.DropDownItems.Add(dropRatesOption);
            lootTypeMenu.DropDownItems.Add(stealingOption);
            lootTypeMenu.DropDownItems.Add(helmOption);
            lootTypeMenu.DropDownItems.Add(salvageOption);


            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;

            groupMobsOption.CheckOnClick = true;
            groupMobsOption.Checked = false;
            groupMobsOption.Click += new EventHandler(groupDetails_Click);

            excludeCrystalsOption.CheckOnClick = true;
            excludeCrystalsOption.Checked = false;
            excludeCrystalsOption.Click += new EventHandler(excludeCrystalsOption_Click);

            excludedPlayerInfoOption.CheckOnClick = true;
            excludedPlayerInfoOption.Checked = true;
            excludedPlayerInfoOption.Click += new EventHandler(excludedPlayerInfoOption_Click);

            optionsMenu.DropDownItems.Add(groupMobsOption);
            optionsMenu.DropDownItems.Add(excludeCrystalsOption);
            optionsMenu.DropDownItems.Add(excludedPlayerInfoOption);

            optionsMenu.Enabled = false;

            toolStrip.Items.Add(lootTypeMenu);
            toolStrip.Items.Add(optionsMenu);
        }
        #endregion

        #region IPlugin overrides
        public override void Reset()
        {
            ResetTextBox();
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            // Only update when something in the loot table changes, or if
            // the battle tables change (ie: mob was killed)
            if ((e.DatasetChanges.Loot.Count != 0) ||
                (e.DatasetChanges.Battles.Any(b => b.Killed == true)))
            {
                HandleDataset(null);
                return;
            }

            if (currentLootType == LootType.HELM)
            {
                if (e.DatasetChanges.ChatMessages.Count > 0)
                {
                    HandleDataset(null);
                    return;
                }
            }
        }

        public override void WatchDatabaseChanged(object sender, DatabaseWatchEventArgs e)
        {
            //base.WatchDatabaseChanged(sender, e);
        }
        #endregion

        #region Event Handlers
        protected void summaryOption_Click(object sender, EventArgs e)
        {
            currentLootType = LootType.Summary;
            optionsMenu.Enabled = false;

            HandleDataset(null);

            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            foreach (ToolStripMenuItem menuItem in lootTypeMenu.DropDownItems)
            {
                if (menuItem == sentBy)
                    menuItem.Checked = true;
                else
                    menuItem.Checked = false;
            }
        }

        protected void dropRatesOption_Click(object sender, EventArgs e)
        {
            currentLootType = LootType.DropRates;
            optionsMenu.Enabled = true;

            HandleDataset(null);

            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            foreach (ToolStripMenuItem menuItem in lootTypeMenu.DropDownItems)
            {
                if (menuItem == sentBy)
                    menuItem.Checked = true;
                else
                    menuItem.Checked = false;
            }
        }

        protected void stealingOption_Click(object sender, EventArgs e)
        {
            currentLootType = LootType.Stealing;
            optionsMenu.Enabled = false;

            HandleDataset(null);

            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            foreach (ToolStripMenuItem menuItem in lootTypeMenu.DropDownItems)
            {
                if (menuItem == sentBy)
                    menuItem.Checked = true;
                else
                    menuItem.Checked = false;
            }
        }

        protected void helmOption_Click(object sender, EventArgs e)
        {
            currentLootType = LootType.HELM;
            optionsMenu.Enabled = false;

            HandleDataset(null);

            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            foreach (ToolStripMenuItem menuItem in lootTypeMenu.DropDownItems)
            {
                if (menuItem == sentBy)
                    menuItem.Checked = true;
                else
                    menuItem.Checked = false;
            }
        }

        protected void salvageOption_Click(object sender, EventArgs e)
        {
            currentLootType = LootType.Salvage;
            optionsMenu.Enabled = false;

            HandleDataset(null);

            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            foreach (ToolStripMenuItem menuItem in lootTypeMenu.DropDownItems)
            {
                if (menuItem == sentBy)
                    menuItem.Checked = true;
                else
                    menuItem.Checked = false;
            }
        }

        protected void groupDetails_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            showGroupDetails = sentBy.Checked;

            HandleDataset(null);
        }

        protected void excludeCrystalsOption_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            excludeCrystalsAndSeals = sentBy.Checked;

            HandleDataset(null);
        }

        protected void excludedPlayerInfoOption_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            excludedPlayerInfo = sentBy.Checked;

            HandleDataset(null);
        }
        #endregion

        #region Process display output
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            if (dataSet == null)
                return;

            // For now, rebuild the entire page each time.
            ResetTextBox();

            switch (currentLootType)
            {
                case LootType.DropRates:
                    ProcessDropRates(dataSet);
                    break;
                case LootType.HELM:
                    ProcessHELM(dataSet);
                    break;
                case LootType.Stealing:
                    ProcessStealing(dataSet);
                    break;
                case LootType.Salvage:
                    ProcessSalvage(dataSet);
                    break;
                case LootType.Summary:
                default:
                    ProcessSummary(dataSet);
                    break;
            }
        }

        private void ProcessSummary(KPDatabaseDataSet dataSet)
        {
            // All items
            AppendText(lsItemDrops, Color.Red, true, false);
            AppendText("\n");

            string dropListFormat = "{0,9} {1}\n";

            int totalGil = 0;
            string gilPlayerName = string.Empty;

            var gilItem = dataSet.Items.SingleOrDefault(i => i.ItemName == lsParsedGil);
            if (gilItem != null)
            {
                gilPlayerName = gilItem.GetLootRows().First().CombatantsRow.CombatantName;
                totalGil = gilItem.GetLootRows().Sum(l => l.GilDropped);
            }

            if (totalGil > 0)
            {
                AppendText(string.Format(dropListFormat, totalGil, lsGil));
            }

            foreach (var item in dataSet.Items)
            {
                if ((item.GetLootRows().Count() > 0) &&
                    (item.ItemName != lsParsedGil))
                {
                    AppendText(string.Format(dropListFormat,
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
                AppendText("\n\n");
                AppendText(lsDistribution, Color.Red, true, false);
                AppendText("\n");

                foreach (var loot in lootByPlayer)
                {
                    AppendText(string.Format("\n    {0}\n", loot.Name), Color.Black, true, false);

                    if (totalGil > 0)
                    {
                        if (gilPlayerName == loot.Name)
                            AppendText(string.Format(dropListFormat, totalGil, lsGil));
                    }

                    foreach (var lootItem in loot.LootItems)
                    {
                        if (lootItem.Key != lsParsedGil)
                        {
                            AppendText(string.Format(dropListFormat,
                                lootItem.Count(), lootItem.Key));
                        }
                    }
                }
            }
        }

        private void ProcessSalvage(KPDatabaseDataSet dataSet)
        {
            AppendText(lsCellDrops, Color.Red, true, false);
            AppendText("\n");

            Regex reSalvageCell = new Regex(lsSalvageCell);

            var cells = from c in dataSet.Items
                        where reSalvageCell.Match(c.ItemName).Success
                        orderby c.ItemName
                        select new
                        {
                            Name = c.ItemName,
                            Count = c.GetLootRows().Count(),
                            Drops = from l in c.GetLootRows()
                                    where l.CombatantsRow != null
                                    group l by l.CombatantsRow.CombatantName into li
                                    orderby li.Key
                                    select li
                        };

            foreach (var item in cells)
            {
                Match m = reSalvageCell.Match(item.Name);
                AppendText(string.Format("\n{0,13}({1:00}): ", m.Groups[1].Value, item.Count), Color.Black, true, false);

                foreach (var drop in item.Drops)
                {
                    AppendText(drop.Key + " ");
                }
            }
        }

        private void ProcessDropRates(KPDatabaseDataSet dataSet)
        {
            List<StringMods> strModList = new List<StringMods>();
            StringBuilder sb = new StringBuilder();

            // Drop rate section
            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsDropRates.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(lsDropRates);

            sb.Append("\n");

            Regex excludeItemsRegex;

            if (excludeCrystalsAndSeals == true)
            {
                excludeItemsRegex = new Regex(lsCrystalsAndSealsRegex);
            }
            else
            {
                excludeItemsRegex = new Regex(lsParsedGil);
            }

            #region LINQ
            var lootByMob = from c in dataSet.Combatants
                            where (c.CombatantType == (byte)EntityType.Mob)
                            orderby c.CombatantName
                            select new
                            {
                                Name = c.CombatantName,
                                Battles = from b in c.GetBattlesRowsByEnemyCombatantRelation()
                                          where b.Killed == true &&
                                            (excludedPlayerInfo == false ||
                                             b.IsKillerIDNull() == true ||
                                             RegexUtility.ExcludedPlayer.Match(b.CombatantsRowByBattleKillerRelation.PlayerInfo).Success == false)
                                          select new
                                          {
                                              Battle = b,
                                              DropsPerBattle = b.GetLootRows()
                                                .Where(a => excludeItemsRegex.Match(a.ItemsRow.ItemName).Success == false)
                                          },
                                Loot = from l in dataSet.Loot
                                       where ((l.IsBattleIDNull() == false) &&
                                              (l.BattlesRow.CombatantsRowByEnemyCombatantRelation == c) &&
                                              (excludeItemsRegex.Match(l.ItemsRow.ItemName).Success == false))
                                       group l by l.ItemsRow.ItemName into li
                                       orderby li.Key
                                       select new
                                       {
                                           LootName = li.Key,
                                           LootDrops = li
                                       },
                                Gil = from l in dataSet.Loot
                                      where ((l.IsBattleIDNull() == false) &&
                                             (l.BattlesRow.CombatantsRowByEnemyCombatantRelation == c) &&
                                             (l.ItemsRow.ItemName == "Gil"))
                                      select l
                            };

            var lootByChest = from c in dataSet.Combatants
                              where (c.CombatantType == (byte)EntityType.TreasureChest)
                              orderby c.CombatantName
                              select new
                              {
                                  Name = c.CombatantName,
                                  Battles = from b in c.GetBattlesRowsByEnemyCombatantRelation()
                                            where b.Killed == true
                                            select new
                                            {
                                                Battle = b,
                                                DropsPerBattle = b.GetLootRows()
                                                  .Where(a => excludeItemsRegex.Match(a.ItemsRow.ItemName).Success == false)
                                            },
                                  Loot = from l in dataSet.Loot
                                         where ((l.IsBattleIDNull() == false) &&
                                                (l.BattlesRow.CombatantsRowByEnemyCombatantRelation == c) &&
                                                (excludeItemsRegex.Match(l.ItemsRow.ItemName).Success == false))
                                         group l by l.ItemsRow.ItemName into li
                                         orderby li.Key
                                         select new
                                         {
                                             LootName = li.Key,
                                             LootDrops = li
                                         },
                                  Gil = from l in dataSet.Loot
                                        where ((l.IsBattleIDNull() == false) &&
                                               (l.BattlesRow.CombatantsRowByEnemyCombatantRelation == c) &&
                                               (l.ItemsRow.ItemName == "Gil"))
                                        select l
                              };
            #endregion

            int totalGil;
            double avgGil;
            double avgLoot;
            int anyDropped;
            double dropRate;
            int lootDropCount;
            int totalDrops;
            double percentDrop;
            int maxDrops;

            string tmpString;

            foreach (var mob in lootByMob)
            {
                int mobKillCount = mob.Battles.Count();

                sb.Append("\n");
                tmpString = string.Format(lsTimesKilledFormat, mob.Name, mobKillCount);
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpString.Length,
                    Bold = true,
                    Color = Color.Black
                });
                sb.Append(tmpString);
                sb.Append("\n");

                totalGil = 0;
                avgGil = 0;

                if (mob.Loot != null)
                {
                    if (mob.Gil.Count() > 0)
                    {
                        // Gil among loot dropped
                        totalGil = mob.Gil.Sum(l => l.GilDropped);

                        if (mobKillCount > 0)
                            avgGil = (double)totalGil / mobKillCount;

                        sb.AppendFormat(lsDropGilFormat,
                            totalGil, lsGil, avgGil);
                        sb.Append("\n");
                    }

                    if (mob.Loot.Count() == 0)
                    {
                        sb.Append("       " + lsNoDrops + "\n");
                    }
                    else
                    {
                        totalDrops = mob.Loot.Sum(a => a.LootDrops.Count());

                        // Non-gil loot
                        foreach (var loot in mob.Loot)
                        {
                            avgLoot = 0;

                            lootDropCount = loot.LootDrops.Count();

                            if (mobKillCount > 0)
                                avgLoot = (double)lootDropCount / mobKillCount;

                            maxDrops = mob.Battles.Max(a =>
                                a.DropsPerBattle.Count(b =>
                                    b.ItemsRow.ItemName == loot.LootName));

                            anyDropped = mob.Battles.Count(a =>
                                a.DropsPerBattle.Any(b =>
                                     b.ItemsRow.ItemName == loot.LootName));

                            dropRate = (double)anyDropped / mobKillCount;

                            percentDrop = totalDrops > 0 ? (double)lootDropCount / totalDrops : 0;

                            sb.AppendFormat(lsDropItemFormat,
                                loot.LootDrops.Count(),
                                loot.LootName,
                                maxDrops,
                                avgLoot,
                                dropRate,
                                percentDrop);
                            sb.Append("\n");
                        }
                    }
                }

                if (showGroupDetails == true)
                {
                    #region Count drops per kill
                    int[] dropCount = new int[11];
                    int maxDropCount = 0;
                    int totalDropCount = 0;

                    for (int i = 0; i < 11; i++)
                    {
                        dropCount[i] = mob.Battles.Count(b => b.DropsPerBattle.Count() == i);
                        totalDropCount += dropCount[i];
                        if (dropCount[i] > 0)
                            maxDropCount = i;
                    }

                    sb.Append("\n");

                    if (totalDropCount > 0)
                    {
                        for (int i = 0; i <= maxDropCount; i++)
                        {
                            sb.Append("       ");
                            sb.AppendFormat(lsDroppedItemNTimesFormat,
                                i, dropCount[i], (double)dropCount[i] / totalDropCount);
                            sb.Append("\n");
                        }
                    }
                    #endregion

                    #region Group drops per kill
                    Dictionary<string, int> strDict = new Dictionary<string, int>();

                    foreach (var battle in mob.Battles)
                    {
                        List<string> strList = new List<string>();

                        foreach (var loot in battle.DropsPerBattle)
                        {
                            if (excludeItemsRegex.Match(loot.ItemsRow.ItemName).Success == false)
                                strList.Add(loot.ItemsRow.ItemName);
                        }

                        string strAgg = string.Empty;

                        if (strList.Count > 0)
                        {
                            strList.Sort();
                            strAgg = strList.Aggregate((itemColl, nextItem) => itemColl + ", " + nextItem);
                        }

                        if (strDict.Any(a => a.Key == strAgg) == true)
                        {
                            strDict[strAgg]++;
                        }
                        else
                        {
                            strDict.Add(strAgg, 1);
                        }
                    }

                    if (strDict.Count > 0)
                    {
                        sb.Append("\n    ");
                        strModList.Add(new StringMods
                        {
                            Start = sb.Length,
                            Length = lsNumberOfTimesDropped.Length,
                            Bold = true,
                            Color = Color.Black
                        });
                        sb.Append(lsNumberOfTimesDropped);
                        sb.Append("\n\n");

                        var sortedStrDict = strDict.OrderByDescending(a => a.Value);
                        int denominator = strDict.Sum(a => a.Value);

                        string setString;

                        foreach (var listSet in sortedStrDict)
                        {
                            if (listSet.Key == string.Empty)
                            {
                                setString = lsNothing;
                            }
                            else
                            {
                                setString = listSet.Key;
                            }

                            sb.AppendFormat(lsDropRateFormat,
                                listSet.Value,
                                (double)listSet.Value / denominator,
                                setString);
                            sb.Append("\n");
                        }
                    }
                    #endregion
                }
            }

            if (lootByChest.Count() > 0)
            {
                sb.Append("\n\n");
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsTreasureChests.Length,
                    Bold = true,
                    Color = Color.Red
                });
                sb.Append(lsTreasureChests);
                sb.Append("\n");
            }

            foreach (var chest in lootByChest)
            {
                int chestsOpened = chest.Battles.Count();

                sb.Append("\n");
                tmpString = string.Format(lsOpenedNTimesFormat, chest.Name, chestsOpened);
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpString.Length,
                    Bold = true,
                    Color = Color.Black
                });
                sb.Append(tmpString);
                sb.Append("\n");

                totalGil = 0;
                avgGil = 0;

                if (chest.Loot != null)
                {
                    if (chest.Gil.Count() > 0)
                    {
                        // Gil among loot dropped
                        totalGil = chest.Gil.Sum(l => l.GilDropped);

                        if (chestsOpened > 0)
                            avgGil = (double)totalGil / chestsOpened;

                        sb.AppendFormat(lsDropGilFormat,
                            totalGil, lsGil, avgGil);
                        sb.Append("\n");
                    }

                    if (chest.Loot.Count() == 0)
                    {
                        sb.Append("       " + lsNoDrops + "\n");
                    }
                    else
                    {
                        totalDrops = chest.Loot.Sum(a => a.LootDrops.Count());

                        // Non-gil loot
                        foreach (var loot in chest.Loot)
                        {
                            avgLoot = 0;

                            lootDropCount = loot.LootDrops.Count();

                            if (chestsOpened > 0)
                                avgLoot = (double)lootDropCount / chestsOpened;

                            maxDrops = chest.Battles.Max(a =>
                                a.DropsPerBattle.Count(b =>
                                    b.ItemsRow.ItemName == loot.LootName));

                            anyDropped = chest.Battles.Count(a =>
                                a.DropsPerBattle.Any(b =>
                                     b.ItemsRow.ItemName == loot.LootName));

                            dropRate = (double)anyDropped / chestsOpened;

                            percentDrop = totalDrops > 0 ? (double)lootDropCount / totalDrops : 0;

                            sb.AppendFormat(lsDropItemFormat,
                                loot.LootDrops.Count(),
                                loot.LootName,
                                maxDrops,
                                avgLoot,
                                dropRate,
                                percentDrop);
                            sb.Append("\n");
                        }
                    }
                }
            }

            PushStrings(sb, strModList);
        }

        private void ProcessStealing(KPDatabaseDataSet dataSet)
        {
            AppendText("Stealing\n\n", Color.Red, true, false);

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
                AppendText(player.Name + ":\n", Color.Black, true, false);
                foreach (var stoleFrom in player.StolenFrom)
                {
                    AppendText(string.Format("  {0}:\n", stoleFrom.TargetName), Color.Black, true, false);

                    foreach (var stoleItem in stoleFrom.Stolen)
                    {
                        AppendText(string.Format("    Stole {0} {1} time{2}.\n",
                            stoleItem.ItemName, stoleItem.Items.Count(), stoleItem.Items.Count() > 1 ? "s" : ""));
                    }

                    foreach (var mugged in stoleFrom.Mugged)
                    {
                        AppendText(string.Format("    Mugged {0} gil.\n", mugged.Amount));
                    }


                    if ((stoleFrom.FailedSteal != null) && (stoleFrom.FailedSteal.Count() > 0))
                    {
                        string s = string.Format("    Failed to Steal {0} time{1}\n",
                            stoleFrom.FailedSteal.Count(), stoleFrom.FailedSteal.Count() > 1 ? "s" : "");
                        AppendText(s);
                    }

                    if ((stoleFrom.FailedMug != null) && (stoleFrom.FailedMug.Count() > 0))
                    {
                        string s = string.Format("    Failed to Mug {0} time{1}\n",
                            stoleFrom.FailedMug.Count(), stoleFrom.FailedMug.Count() > 1 ? "s" : "");
                        AppendText(s);
                    }
                }

                AppendText("\n\n");
            }
        }

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
                AppendText("Harvesting:\n", Color.Red, true, false);

                totalItems = harvestedItems.Sum(a => a.Count);
                totalCount = totalItems + harvestingFailures;

                foreach (var item in harvestedItems)
                {
                    avgResult = (double)item.Count / totalCount;
                    AppendText(string.Format(lineFormat,
                        item.Item, item.Count, avgResult));
                }

                avgResult = (double)harvestingFailures / totalCount;
                AppendText(string.Format(lineFormat,
                    "Nothing", harvestingFailures, avgResult));

                if (harvestedItems.Count() > 0)
                    AppendText(string.Format("\n  {0,-34} {1,5}\n",
                        "Total Items:", totalItems));

                AppendText(string.Format("  {0,-34} {1,5}\n",
                    "Total Tries:", totalCount));

                avgResult = (double)harvestingBreaks / totalCount;
                AppendText(string.Format("\n  {0,-34} {1,5}  [{2,8:p2}]\n",
                    "Breaks:", harvestingBreaks, avgResult));

                AppendText("\n");
            }

            // Logging results
            if (loggedItems.Count() > 0 || loggingBreaks > 0)
            {
                AppendText("Logging:\n", Color.Red, true, false);

                totalItems = loggedItems.Sum(a => a.Count);
                totalCount = totalItems + loggingFailures;

                foreach (var item in loggedItems)
                {
                    avgResult = (double)item.Count / totalCount;
                    AppendText(string.Format(lineFormat,
                        item.Item, item.Count, avgResult));
                }

                avgResult = (double)loggingFailures / totalCount;
                AppendText(string.Format(lineFormat,
                    "Nothing", loggingFailures, avgResult));

                if (loggedItems.Count() > 0)
                    AppendText(string.Format("\n  {0,-34} {1,5}\n",
                        "Total Items:", totalItems));

                AppendText(string.Format("  {0,-34} {1,5}\n",
                    "Total Tries:", totalCount));

                avgResult = (double)loggingBreaks / totalCount;
                AppendText(string.Format("\n  {0,-34} {1,5}  [{2,8:p2}]\n",
                    "Breaks:", loggingBreaks, avgResult));

                AppendText("\n");
            }

            // Mining results
            if (minedItems.Count() > 0 || miningBreaks > 0)
            {
                AppendText("Mining/Excavation:\n", Color.Red, true, false);

                totalItems = minedItems.Sum(a => a.Count);
                totalCount = totalItems + miningFailures;

                foreach (var item in minedItems)
                {
                    avgResult = (double)item.Count / totalCount;
                    AppendText(string.Format(lineFormat,
                        item.Item, item.Count, avgResult));
                }

                avgResult = (double)miningFailures / totalCount;
                AppendText(string.Format(lineFormat,
                   "Nothing", miningFailures, avgResult));

                if (minedItems.Count() > 0)
                    AppendText(string.Format("\n  {0,-34} {1,5}\n",
                        "Total Items:", totalItems));

                AppendText(string.Format("  {0,-34} {1,5}\n",
                    "Total Tries:", totalCount));

                avgResult = (double)miningBreaks / totalCount;
                AppendText(string.Format("\n  {0,-34} {1,5}  [{2,8:p2}]\n",
                    "Breaks:", miningBreaks, avgResult));

                AppendText("\n");
            }

            // Chocobo results
            if (chocoboItems.Count() > 0 || chocoboDiggingFailures > 0)
            {
                AppendText("Chocobo Digging:\n", Color.Red, true, false);

                totalCount = chocoboItems.Sum(a => a.Count) + chocoboDiggingFailures;

                foreach (var item in chocoboItems)
                {
                    avgResult = (double)item.Count / totalCount;
                    AppendText(string.Format(lineFormat,
                        item.Item, item.Count, avgResult));
                }

                avgResult = (double)chocoboDiggingFailures / totalCount;
                AppendText(string.Format(lineFormat,
                   "Nothing", chocoboDiggingFailures, avgResult));

                if (chocoboItems.Count() > 0)
                    AppendText(string.Format("\n  {0,-34} {1,5}\n",
                        "Total Items:", chocoboItems.Sum(li => li.Count)));

                AppendText(string.Format("  {0,-34} {1,5}\n",
                    "Total Tries:", totalCount));

                AppendText("\n");
            }

        }

        #endregion

        #region Localization Overrides
        protected override void LoadLocalizedUI()
        {
            lootTypeMenu.Text = Resources.NonCombat.TreasurePluginLootTypeMenu;
            summaryOption.Text = Resources.NonCombat.TreasurePluginLootTypeSummary;
            dropRatesOption.Text = Resources.NonCombat.TreasurePluginLootTypeDropRates;
            stealingOption.Text = Resources.NonCombat.TreasurePluginLootTypeStealing;
            helmOption.Text = Resources.NonCombat.TreasurePluginLootTypeHELM;
            salvageOption.Text = Resources.NonCombat.TreasurePluginLootTypeSalvage;

            optionsMenu.Text = Resources.PublicResources.Options;
            groupMobsOption.Text = Resources.NonCombat.TreasurePluginOptionGroupMobs;
            excludeCrystalsOption.Text = Resources.NonCombat.TreasurePluginOptionExcludeCrystals;
            excludedPlayerInfoOption.Text = Resources.NonCombat.ExcludedPlayerOption;
        }

        protected override void LoadResources()
        {
            this.tabName = Resources.NonCombat.TreasurePluginTabName;

            // Hard coded at the moment; will adjust later to adjust it based
            // on info in the database when the database is loaded/initialized.
            Resources.ParsedStrings.Culture = new System.Globalization.CultureInfo("en-US");

            lsGil = Resources.NonCombat.TreasurePluginGil;
            lsParsedGil = Resources.ParsedStrings.Gil;
            lsItemDrops = Resources.NonCombat.TreasurePluginItemDrops;
            lsDistribution = Resources.NonCombat.TreasurePluginDistribution;
            lsNoDrops = Resources.NonCombat.TreasurePluginNoDrops;
            lsTreasureChests = Resources.NonCombat.TreasurePluginTreasureChests;
            lsNumberOfTimesDropped = Resources.NonCombat.TreasurePluginNumberOfTimesDropped;
            lsNothing = Resources.NonCombat.TreasurePluginNothing;

            lsCellDrops = Resources.NonCombat.TreasurePluginCellDrops;
            lsSalvageCell = Resources.ParsedStrings.SalvageCellRegex;

            lsDropRates = Resources.NonCombat.TreasurePluginDropRates;
            lsDropItemFormat = Resources.NonCombat.TreasurePluginDropItemFormat;
            lsDropGilFormat = Resources.NonCombat.TreasurePluginDropGilFormat;
            lsCrystalsAndSealsRegex = Resources.ParsedStrings.CrystalsAndSealsRegex;
            lsTimesKilledFormat = Resources.NonCombat.TreasurePluginTimesKilledFormat;
            lsDroppedItemNTimesFormat = Resources.NonCombat.TreasurePluginDroppedItemNTimesFormat;
            lsOpenedNTimesFormat = Resources.NonCombat.TreasurePluginOpenedNTimesFormat;
            lsDropRateFormat = Resources.NonCombat.TreasurePluginDropRateFormat;
        }
        #endregion

    }
}
