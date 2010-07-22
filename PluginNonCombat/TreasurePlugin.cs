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

        internal class HELMList
        {
            internal string ItemName { get; set; }
            internal int ItemCount { get; set; }
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
        string lsGil;
        string lsCruor;
        string lsTreasureChest;
        string lsSalvageCell;
        string lsCrystalsAndSealsRegex;

        #endregion

        #region HELM strings

        // HELM parsing strings

        string lsItemRegex;
        string lsItemRegexReference;

        string lsHarvestFormat;
        string lsLoggingFormat;
        string lsMiningFormat;

        string lsHarvestFail;
        string lsLoggingFail;
        string lsMiningFail;

        string lsHarvestToolBreak;
        string lsLoggingToolBreak;
        string lsMiningToolBreak;

        string lsHarvestBreakRegex;
        string lsLoggingBreakRegex;
        string lsMiningBreakRegex;

        string lsChocoDiggingFail;
        string lsChocoDiggingFormat;
        string lsChocoDiggingFoundWithEase;

        Regex Harvest;
        Regex Logging;
        Regex Mining;

        Regex HarvestBreak;
        Regex LoggingBreak;
        Regex MiningBreak;

        Regex ChocoDigging;

        // HELM display strings

        string lsHELMHarvesting;
        string lsHELMLogging;
        string lsHELMMining;
        string lsHELMDigging;

        string lsHELMTotalItems;
        string lsHELMTotalTries;
        string lsHELMBreaks;
        string lsHELMNothing;
        string lsHELMFoundWithEase;
        string lsHELMTotalDigs;

        string lsHELMShortLineFormat;
        string lsHELMLongLineFormat;


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
            List<StringMods> strModList = new List<StringMods>();
            StringBuilder sb = new StringBuilder();

            // All items
            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsItemDrops.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(lsItemDrops);
            sb.Append("\n");

            string dropListFormat = "{0,9} {1}\n";

            int totalGil = 0;
            string gilPlayerName = string.Empty;

            var gilItem = dataSet.Items.SingleOrDefault(i => i.ItemName == lsGil);
            if (gilItem != null)
            {
                gilPlayerName = gilItem.GetLootRows().First().CombatantsRow.CombatantName;
                totalGil = gilItem.GetLootRows().Sum(l => l.GilDropped);
            }

            if (totalGil > 0)
            {
                sb.AppendFormat(dropListFormat, totalGil, lsGil);
            }

            int totalCruor = 0;
            string cruorPlayerName = string.Empty;

            var cruorItem = dataSet.Items.SingleOrDefault(i => i.ItemName == lsCruor);
            if (cruorItem != null)
            {
                cruorPlayerName = cruorItem.GetLootRows().First().CombatantsRow.CombatantName;
                totalCruor = cruorItem.GetLootRows().Sum(l => l.GilDropped);
            }

            if (totalCruor > 0)
            {
                sb.AppendFormat(dropListFormat, totalCruor, lsCruor);
            }

            var treasureChestItem = dataSet.Items.SingleOrDefault(i => i.ItemName == lsTreasureChest);
            if (treasureChestItem != null)
            {
                int treasureChestCount = treasureChestItem.GetLootRows().Count();
                sb.AppendFormat(dropListFormat, treasureChestCount, lsTreasureChest);
            }

            foreach (var item in dataSet.Items)
            {
                if ((item.GetLootRows().Count() > 0) &&
                    (item.ItemName != lsGil) &&
                    (item.ItemName != lsCruor) &&
                    (item.ItemName != lsTreasureChest))
                {
                    sb.AppendFormat(dropListFormat, item.GetLootRows().Count(), item.ItemName);
                }
            }

            // Items by player who got them
            var lootByPlayer = from c in dataSet.Combatants
                               where (((EntityType)c.CombatantType == EntityType.Player) &&
                                      (c.GetLootRows().Count() != 0))
                               orderby c.CombatantName
                               select new
                               {
                                   Name = c.CombatantNameOrJobName,
                                   LootItems = from l in c.GetLootRows()
                                               group l by l.ItemsRow.ItemName into li
                                               orderby li.Key
                                               select li
                               };


            if (lootByPlayer.Count() > 0)
            {
                sb.Append("\n\n");
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsDistribution.Length,
                    Bold = true,
                    Color = Color.Red
                });
                sb.Append(lsDistribution);
                sb.Append("\n");

                foreach (var loot in lootByPlayer)
                {
                    string tmp = string.Format("\n    {0}\n", loot.Name);
                    strModList.Add(new StringMods
                    {
                        Start = sb.Length,
                        Length = tmp.Length,
                        Bold = true,
                        Color = Color.Black
                    });
                    sb.Append(tmp);

                    if (totalGil > 0)
                    {
                        if (gilPlayerName == loot.Name)
                            sb.AppendFormat(dropListFormat, totalGil, lsGil);
                    }

                    foreach (var lootItem in loot.LootItems)
                    {
                        if ((lootItem.Key != lsGil) && (lootItem.Key != lsCruor))
                        {
                            sb.AppendFormat(dropListFormat,
                                lootItem.Count(), lootItem.Key);
                        }
                    }
                }
            }

            PushStrings(sb, strModList);
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
                excludeItemsRegex = new Regex(lsGil + "|" + lsCruor);
            }

            #region LINQ
            var lootByMob = from c in dataSet.Combatants
                            where ((EntityType)c.CombatantType == EntityType.Mob)
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
                                             (l.ItemsRow.ItemName == lsGil))
                                      select l,
                                Cruor = from l in dataSet.Loot
                                      where ((l.IsBattleIDNull() == false) &&
                                             (l.BattlesRow.CombatantsRowByEnemyCombatantRelation == c) &&
                                             (l.ItemsRow.ItemName == lsCruor))
                                      select l
                            };

            var lootByChest = from c in dataSet.Combatants
                              where ((EntityType)c.CombatantType == EntityType.TreasureChest)
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
                                               (l.ItemsRow.ItemName == lsGil))
                                        select l,
                                  Cruor = from l in dataSet.Loot
                                        where ((l.IsBattleIDNull() == false) &&
                                               (l.BattlesRow.CombatantsRowByEnemyCombatantRelation == c) &&
                                               (l.ItemsRow.ItemName == lsCruor))
                                        select l,
                              };
            #endregion

            int totalGil;
            double avgGil;
            int totalCruor;
            double avgCruor;
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

                if (mobKillCount > 0)
                {
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
                    totalCruor = 0;
                    avgCruor = 0;

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

                        if (mob.Cruor.Count() > 0)
                        {
                            // Gil among loot dropped
                            totalCruor = mob.Cruor.Sum(l => l.GilDropped);

                            if (mobKillCount > 0)
                                avgCruor = (double)totalCruor / mobKillCount;

                            sb.AppendFormat(lsDropGilFormat,
                                totalCruor, lsCruor, avgCruor);
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
                totalCruor = 0;
                avgCruor = 0;

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

                    if (chest.Cruor.Count() > 0)
                    {
                        // Cruor among loot dropped
                        totalCruor = chest.Cruor.Sum(l => l.GilDropped);

                        if (chestsOpened > 0)
                            avgCruor = (double)totalCruor / chestsOpened;

                        sb.AppendFormat(lsDropGilFormat,
                            totalCruor, lsCruor, avgCruor);
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
                                    Name = c.CombatantNameOrJobName,
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
                                 group ac by Harvest.Match(ac.Message).Groups[lsItemRegexReference].Value into acn
                                 orderby acn.Key
                                 select new HELMList
                                 {
                                     ItemName = acn.Key,
                                     ItemCount = acn.Count()
                                 };

            int harvestingBreaks = arenaChat.Count(ac => HarvestBreak.Match(ac.Message).Success == true);

            int harvestingFailures = arenaChat.Count(a => a.Message == lsHarvestFail ||
                a.Message == lsHarvestToolBreak);


            var loggedItems = from ac in arenaChat
                              where Logging.Match(ac.Message).Success == true
                              group ac by Logging.Match(ac.Message).Groups[lsItemRegexReference].Value into acn
                              orderby acn.Key
                              select new HELMList
                              {
                                  ItemName = acn.Key,
                                  ItemCount = acn.Count()
                              };

            int loggingBreaks = arenaChat.Count(ac => LoggingBreak.Match(ac.Message).Success == true);

            int loggingFailures = arenaChat.Count(a => a.Message == lsLoggingFail ||
                a.Message == lsLoggingToolBreak);


            var minedItems = from ac in arenaChat
                             where Mining.Match(ac.Message).Success == true
                             group ac by Mining.Match(ac.Message).Groups[lsItemRegexReference].Value into acn
                             orderby acn.Key
                             select new HELMList
                             {
                                 ItemName = acn.Key,
                                 ItemCount = acn.Count()
                             };


            int miningBreaks = arenaChat.Count(ac => MiningBreak.Match(ac.Message).Success == true);

            int miningFailures = arenaChat.Count(a => a.Message == lsMiningFail ||
                a.Message == lsMiningToolBreak);


            var chocoboItems = from ac in arenaChat
                               where ChocoDigging.Match(ac.Message).Success == true
                               group ac by ChocoDigging.Match(ac.Message).Groups[lsItemRegexReference].Value into acn
                               orderby acn.Key
                               select new HELMList
                               {
                                   ItemName = acn.Key,
                                   ItemCount = acn.Count()
                               };


            int chocoboDiggingFailures = arenaChat.Count(ac => ac.Message == lsChocoDiggingFail);
            int chocoboFoundWithEase = arenaChat.Count(ac => ac.Message == lsChocoDiggingFoundWithEase);
            #endregion

            // Consolidated build
            StringBuilder sb = new StringBuilder();
            List<StringMods> strModList = new List<StringMods>();

            // Build the full set of strings for all types of HELM
            BuildHELMResults(lsHELMHarvesting, harvestedItems, harvestingBreaks,
                harvestingFailures,     0, sb, strModList);
            BuildHELMResults(lsHELMLogging,    loggedItems,    loggingBreaks,
                loggingFailures,        0, sb, strModList);
            BuildHELMResults(lsHELMMining,     minedItems,     miningBreaks,
                miningFailures,         0, sb, strModList);
            BuildHELMResults(lsHELMDigging,    chocoboItems,   -1,
                chocoboDiggingFailures, chocoboFoundWithEase, sb, strModList);

            // Then display them
            PushStrings(sb, strModList);
        }

        /// <summary>
        /// Function to add on to the string builder with a constructed display
        /// for each section of HELM results.
        /// </summary>
        /// <param name="sectionTitle">The title of this section.</param>
        /// <param name="helmedItems">The IEnumerable HELMList of HELMed items.</param>
        /// <param name="helmedBreaks">A count of breaks for this HELM type.  -1 if not applicable (IE: Choco digging)</param>
        /// <param name="helmedFailures">A count of the number of failures for this HELM type.</param>
        /// <param name="sb">The StringBuilder that's being added to.</param>
        /// <param name="strModList">The list of string mods for color/bold/etc.</param>
        private void BuildHELMResults(string sectionTitle,
            IEnumerable<HELMList> helmedItems, int helmedBreaks, int helmedFailures, int chocoboFWE,
            StringBuilder sb, List<StringMods> strModList)
        {
            // Don't add anything if there aren't any of the provided type of results
            if (helmedItems.Count() == 0)
                return;

            // Add the section title first.
            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = sectionTitle.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.AppendFormat("{0}\n", sectionTitle);

            // Some local variables
            int totalItems;
            int totalCount;
            double avgResult;

            totalItems = helmedItems.Sum(a => a.ItemCount);
            totalCount = totalItems + helmedFailures;

            // List out each item HELMed and give its count and percentage.
            foreach (var item in helmedItems)
            {
                avgResult = (double)item.ItemCount / totalCount;

                sb.AppendFormat(lsHELMLongLineFormat,
                    item.ItemName, item.ItemCount, avgResult);
                sb.Append("\n");
            }

            // And then the number of times nothing was acquired (failures).
            avgResult = (double)helmedFailures / totalCount;
            sb.AppendFormat(lsHELMLongLineFormat,
                lsHELMNothing, helmedFailures, avgResult);
            sb.Append("\n\n");

            // Show total number of items
            sb.AppendFormat(lsHELMShortLineFormat,
                lsHELMTotalItems, totalItems);
            sb.Append("\n");

            // And total number of attempts
            sb.AppendFormat(lsHELMShortLineFormat,
                lsHELMTotalTries, totalCount);
            sb.Append("\n");

            // And finally, the number of breaks (if applicable)
            if (helmedBreaks >= 0)
            {
                avgResult = (double)helmedBreaks / totalCount;

                sb.Append("\n");
                sb.AppendFormat(lsHELMLongLineFormat,
                    lsHELMBreaks, helmedBreaks, avgResult);
                sb.Append("\n");
            }

            // "Found with ease" messages from choco racing silks.
            if (chocoboFWE > 0)
            {
                sb.AppendFormat(lsHELMShortLineFormat,
                    lsHELMFoundWithEase, chocoboFWE);
                sb.Append("\n");

                sb.AppendFormat(lsHELMShortLineFormat,
                    lsHELMTotalDigs, totalCount - chocoboFWE);
                sb.Append("\n");
            }

            sb.Append("\n");
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

            lsGil = Resources.ParsedStrings.Gil;
            lsCruor = Resources.ParsedStrings.Cruor;
            lsTreasureChest = "Treasure Chest";
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

            // HELM strings

            // HELM messages:
            // You successfully harvest a clump of red moko grass!
            // You are unable to harvest anything.
            // You harvest a bunch of gysahl greens, but your sickle breaks.
            // Your sickle breaks!

            lsItemRegex = Resources.NonCombat.TreasurePluginRegexItem;
            lsItemRegexReference = Resources.NonCombat.TreasurePluginRegexItemReference;

            lsHarvestFormat = Resources.NonCombat.TreasurePluginRegexHarvestFormat;
            lsLoggingFormat = Resources.NonCombat.TreasurePluginRegexLoggingFormat;
            lsMiningFormat = Resources.NonCombat.TreasurePluginRegexMiningFormat;

            Harvest = new Regex(string.Format(lsHarvestFormat, lsItemRegex));
            Logging = new Regex(string.Format(lsLoggingFormat, lsItemRegex));
            Mining = new Regex(string.Format(lsMiningFormat, lsItemRegex));

            lsHarvestFail = Resources.NonCombat.TreasurePluginHarvestFail;
            lsLoggingFail = Resources.NonCombat.TreasurePluginLoggingFail;
            lsMiningFail = Resources.NonCombat.TreasurePluginMiningFail;

            lsHarvestToolBreak = Resources.NonCombat.TreasurePluginHarvestToolBreak;
            lsLoggingToolBreak = Resources.NonCombat.TreasurePluginLoggingToolBreak;
            lsMiningToolBreak = Resources.NonCombat.TreasurePluginMiningToolBreak;

            lsHarvestBreakRegex = Resources.NonCombat.TreasurePluginRegexHarvestBreak;
            lsLoggingBreakRegex = Resources.NonCombat.TreasurePluginRegexLoggingBreak;
            lsMiningBreakRegex = Resources.NonCombat.TreasurePluginRegexMiningBreak;

            HarvestBreak = new Regex(lsHarvestBreakRegex);
            LoggingBreak = new Regex(lsLoggingBreakRegex);
            MiningBreak = new Regex(lsMiningBreakRegex);

            // Chocobo digging messages:
            // You dig and you dig, but find nothing.
            // Obtained: Lauan log.
            // Obtained: Pebble.
            // It appears your chocobo found this item with ease.

            lsChocoDiggingFail = Resources.NonCombat.TreasurePluginChocoDiggingFail;
            lsChocoDiggingFormat = Resources.NonCombat.TreasurePluginChocoDiggingFormat;
            lsChocoDiggingFoundWithEase = Resources.NonCombat.TreasurePluginChocoDiggingFWE;

            ChocoDigging = new Regex(string.Format(lsChocoDiggingFormat, lsItemRegex));

            // Display strings

            lsHELMHarvesting = Resources.NonCombat.TreasurePluginHELMHarvesting;
            lsHELMLogging = Resources.NonCombat.TreasurePluginHELMLogging;
            lsHELMMining = Resources.NonCombat.TreasurePluginHELMMining;
            lsHELMDigging = Resources.NonCombat.TreasurePluginHELMDigging;

            lsHELMTotalItems = Resources.NonCombat.TreasurePluginHELMTotalItems;
            lsHELMTotalTries = Resources.NonCombat.TreasurePluginHELMTotalTries;
            lsHELMBreaks = Resources.NonCombat.TreasurePluginHELMBreaks;
            lsHELMNothing = Resources.NonCombat.TreasurePluginHELMNothing;
            lsHELMFoundWithEase = Resources.NonCombat.TreasurePluginHELMFoundWithEase;
            lsHELMTotalDigs = Resources.NonCombat.TreasurePluginHELMNormalDigs;

            lsHELMShortLineFormat = Resources.NonCombat.TreasurePluginHELMShortLineFormat;
            lsHELMLongLineFormat = Resources.NonCombat.TreasurePluginHELMLongLineFormat;

        }
        #endregion

    }
}
