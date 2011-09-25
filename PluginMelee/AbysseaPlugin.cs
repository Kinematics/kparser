using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WaywardGamers.KParser;
using WaywardGamers.KParser.Database;

namespace WaywardGamers.KParser.Plugin
{
    public class AbysseaPlugin : BasePluginControl
    {
        #region Member Variables
        bool flagNoUpdate;
        bool groupMobs = true;
        bool exclude0XPMobs = false;
        bool customMobSelection = false;

        ToolStripLabel catLabel = new ToolStripLabel();
        ToolStripComboBox categoryCombo = new ToolStripComboBox();
        ToolStripLabel mobsLabel = new ToolStripLabel();
        ToolStripComboBox mobsCombo = new ToolStripComboBox();

        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();
        ToolStripMenuItem groupMobsOption = new ToolStripMenuItem();
        ToolStripMenuItem exclude0XPOption = new ToolStripMenuItem();
        ToolStripMenuItem customMobSelectionOption = new ToolStripMenuItem();

        ToolStripButton editCustomMobFilter = new ToolStripButton();

        Regex colorLight = new Regex("(?<light>(?<color>pearlescent|azure|ruby|amber|ebon|golden|silvery) light)");

        // Localizable strings
        string lsCruor;
        string lsKey;
        string lsTreasureChest;
        string lsTimeExtension;

        #endregion

        #region Constructor
        public AbysseaPlugin()
        {
            LoadLocalizedUI();

            categoryCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            categoryCombo.SelectedIndex = 0;
            categoryCombo.SelectedIndexChanged += new EventHandler(this.categoryCombo_SelectedIndexChanged);

            mobsCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            mobsCombo.AutoSize = false;
            mobsCombo.Width = 175;
            mobsCombo.MaxDropDownItems = 10;
            mobsCombo.SelectedIndexChanged += new EventHandler(this.mobsCombo_SelectedIndexChanged);


            groupMobsOption.CheckOnClick = true;
            groupMobsOption.Checked = true;
            groupMobsOption.Click += new EventHandler(groupMobs_Click);

            exclude0XPOption.CheckOnClick = true;
            exclude0XPOption.Checked = false;
            exclude0XPOption.Click += new EventHandler(exclude0XPMobs_Click);

            customMobSelectionOption.CheckOnClick = true;
            customMobSelectionOption.Checked = false;
            customMobSelectionOption.Click += new EventHandler(customMobSelection_Click);

            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;
            optionsMenu.DropDownItems.Add(groupMobsOption);
            optionsMenu.DropDownItems.Add(exclude0XPOption);
            optionsMenu.DropDownItems.Add(customMobSelectionOption);


            editCustomMobFilter.Enabled = false;
            editCustomMobFilter.Click += new EventHandler(editCustomMobFilter_Click);


            ToolStripSeparator aSeparator = new ToolStripSeparator();

            toolStrip.Items.Add(catLabel);
            toolStrip.Items.Add(categoryCombo);
            toolStrip.Items.Add(mobsLabel);
            toolStrip.Items.Add(mobsCombo);
            toolStrip.Items.Add(optionsMenu);
            toolStrip.Items.Add(aSeparator);
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
            UpdateMobList(false);

            mobsCombo.CBSelectIndex(0);
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            // Check for new mobs being fought.  If any exist, update the Mob Group dropdown list.
            if (e.DatasetChanges.Battles.Count > 0)
            {
                string selectedItem = mobsCombo.CBSelectedItem();
                UpdateMobList(true);

                flagNoUpdate = true;
                mobsCombo.CBSelectItem(selectedItem);
            }

            if (e.DatasetChanges.Interactions.Count != 0)
            {
                HandleDataset(null);
            }
        }
        #endregion

        #region Private Methods
        private void UpdateMobList()
        {
            UpdateMobList(false);
        }

        private void UpdateMobList(bool overrideGrouping)
        {
            mobsCombo.UpdateWithMobList(groupMobs, exclude0XPMobs);
        }

        private KPDatabaseDataSet.InteractionsRow GetLastAction(KPDatabaseDataSet.BattlesRow b)
        {
            var killer = b.CombatantsRowByBattleKillerRelation;

            var killerActions = b.GetInteractionsRows()
                .Where(i =>
                    i.IsActorIDNull() == false &&
                    i.ActorID == killer.CombatantID);

            return killerActions.LastOrDefault(a =>
                (ActionType)a.ActionType != ActionType.Death);
        }
        #endregion

        #region Processing sections
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            if (dataSet == null)
                return;

            ResetTextBox();

            MobFilter mobFilter;
            if (customMobSelection)
                mobFilter = MobXPHandler.Instance.CustomMobFilter;
            else
                mobFilter = mobsCombo.CBGetMobFilter(exclude0XPMobs);

            StringBuilder sb = new StringBuilder();
            List<StringMods> strModList = new List<StringMods>();

            switch (categoryCombo.CBSelectedIndex())
            {
                case 0:
                    // All
                    ProcessLights(dataSet, mobFilter, ref sb, strModList);
                    ProcessMobs(dataSet, mobFilter, ref sb, strModList);
                    ProcessChests(dataSet, mobFilter, ref sb, strModList);
                    break;
                case 1:
                    // Lights
                    ProcessLights(dataSet, mobFilter, ref sb, strModList);
                    break;
                case 2:
                    // Mobs
                    ProcessMobs(dataSet, mobFilter, ref sb, strModList);
                    break;
                case 3:
                    // Chests
                    ProcessChests(dataSet, mobFilter, ref sb, strModList);
                    break;
            }

            PushStrings(sb, strModList);
        }

        private void ProcessLights(KPDatabaseDataSet dataSet, MobFilter mobFilter,
            ref StringBuilder sb, List<StringMods> strModList)
        {
            string tmpStr = "Lights";

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = tmpStr.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(tmpStr + "\n\n");

            var allBattles = from b in dataSet.Battles
                             where mobFilter.CheckFilterBattle(b) == true ||
                                   (b.IsEnemyIDNull() == false &&
                                    (EntityType)b.CombatantsRowByEnemyCombatantRelation.CombatantType == EntityType.TreasureChest)
                             select b;

            var lightRewards = from b in allBattles
                               let loot = b.GetLootRows()
                               let light = loot.FirstOrDefault(l => colorLight.Match(l.ItemsRow.ItemName).Success == true)
                               where light != null
                               group b by light.ItemsRow.ItemName;

            // Header
            string formatString = "{0,-18} : {1,10}{2,14}{3,10}";
            tmpStr = string.Format(formatString,
                "Color Light",
                "From Mobs",
                "From Chests",
                "Total");

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = tmpStr.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(tmpStr + "\n");


            int totalMobCount = 0;
            int totalChestCount = 0;
            int totalCount = 0;

            foreach (var light in lightRewards.OrderBy(a => a.Key))
            {
                int lightCount = light.Count();
                int chestLightCount = light.Count(a =>
                    (EntityType)a.CombatantsRowByEnemyCombatantRelation.CombatantType == EntityType.TreasureChest);

                int mobLightCount = lightCount - chestLightCount;

                totalCount += lightCount;
                totalMobCount += mobLightCount;
                totalChestCount += chestLightCount;

                sb.AppendFormat(formatString,
                    light.Key,
                    mobLightCount,
                    chestLightCount,
                    lightCount);
                sb.Append("\n");
            }

            if (totalCount > 0)
            {
                sb.Append("\n");
                sb.AppendFormat(formatString,
                    "Total",
                    totalMobCount,
                    totalChestCount,
                    totalCount);
                sb.Append("\n");
            }


            sb.Append("\n\n");

        }

        private void ProcessMobs(KPDatabaseDataSet dataSet, MobFilter mobFilter,
            ref StringBuilder sb, List<StringMods> strModList)
        {
            var allBattles = from b in dataSet.Battles
                          where mobFilter.CheckFilterBattle(b) == true 
                          select b;

            // Actual killed mobs
            var battles = allBattles.Where(b => b.Killed == true);

            int battleCount = battles.Count();

            string tmpStr = "Mobs";

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = tmpStr.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(tmpStr + "\n\n");

            sb.AppendFormat("Total number of mobs: {0}\n\n", battles.Count());

            if (battleCount > 0)
            {
                // Types of kills
                var dropDead = battles.Where(b => b.IsKillerIDNull() == true);

                var lastActions = from b in battles
                                  where b.IsKillerIDNull() == false
                                  select new
                                  {
                                      Battle = b,
                                      LastAction = GetLastAction(b)
                                  };

                int meleeCount = 0;
                int magicCount = 0;
                int wsCount = 0;
                int jaCount = 0;
                int otherCount = 0;
                int dropDeadCount = 0;
                int unknownCount = 0;
                int petCount = 0;

                dropDeadCount = dropDead.Count();

                foreach (var action in lastActions)
                {
                    if (action.LastAction == null)
                    {
                        unknownCount++;
                    }
                    else
                    {
                        switch ((ActionType)action.LastAction.ActionType)
                        {
                            case ActionType.Ability:
                                jaCount++;
                                break;
                            case ActionType.Spell:
                                magicCount++;
                                break;
                            case ActionType.Weaponskill:
                            case ActionType.Skillchain:
                                wsCount++;
                                break;
                            case ActionType.Melee:
                            case ActionType.Ranged:
                            case ActionType.Counterattack:
                            case ActionType.Retaliation:
                                meleeCount++;
                                break;
                            default:
                                otherCount++;
                                break;
                        };

                        if ((EntityType)action.LastAction.CombatantsRowByActorCombatantRelation.CombatantType
                            == EntityType.Pet)
                        {
                            petCount++;
                        }
                    }
                }


                tmpStr = "Kill Types:";

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpStr.Length,
                    Bold = true,
                    Color = Color.Black
                });
                sb.Append(tmpStr + "\n\n");

                sb.AppendFormat("{0,-14}: {1,6}  ({2,8:p2})\n", "Melee",
                    meleeCount, (double)meleeCount / battleCount);
                sb.AppendFormat("{0,-14}: {1,6}  ({2,8:p2})\n", "Magic",
                    magicCount, (double)magicCount / battleCount);
                sb.AppendFormat("{0,-14}: {1,6}  ({2,8:p2})\n", "Weaponskill",
                    wsCount, (double)wsCount / battleCount);
                sb.AppendFormat("{0,-14}: {1,6}  ({2,8:p2})\n", "Ability",
                    jaCount, (double)jaCount / battleCount);
                sb.AppendFormat("{0,-14}: {1,6}  ({2,8:p2})\n", "Other",
                    otherCount, (double)otherCount / battleCount);
                sb.AppendFormat("{0,-14}: {1,6}  ({2,8:p2})\n", "Drop dead",
                    dropDeadCount, (double)dropDeadCount / battleCount);
                sb.AppendFormat("{0,-14}: {1,6}  ({2,8:p2})\n", "Unknown",
                    unknownCount, (double)unknownCount / battleCount);
                sb.Append("\n");
                sb.AppendFormat("{0,-14}: {1,6}  ({2,8:p2})\n", "Pets",
                    petCount, (double)petCount / battleCount);

                sb.Append("\n\n");

                // Cruor
                tmpStr = "Cruor:";
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmpStr.Length,
                    Bold = true,
                    Color = Color.Black
                });
                sb.Append(tmpStr + "\n\n");

                var cruorBattles = battles.Where(b => b.GetLootRows().Any(l => l.ItemsRow.ItemName == lsCruor));

                sb.AppendFormat("Number of mobs that dropped Cruor : {0}\n", cruorBattles.Count());

                if (cruorBattles.Any())
                {
                    int totalCruor = cruorBattles.Sum(b =>
                        b.GetLootRows().First(l => l.ItemsRow.ItemName == lsCruor).GilDropped);
                    double avgCruor = (double)totalCruor / cruorBattles.Count();
                    sb.AppendFormat("Total Cruor drop (mobs) : {0}\n", totalCruor);
                    sb.AppendFormat("Average Cruor drop      : {0:f2}\n", avgCruor);
                }

                sb.Append("\n\n");

            }

        }

        private void ProcessChests(KPDatabaseDataSet dataSet, MobFilter mobFilter,
            ref StringBuilder sb, List<StringMods> strModList)
        {
            string tmpStr = "Chests";

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = tmpStr.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(tmpStr + "\n\n");

            var selectBattles = from b in dataSet.Battles
                                where mobFilter.CheckFilterBattle(b) == true
                                select b;

            var droppedChests = from b in selectBattles
                                where b.GetLootRows().Any() &&
                                      b.GetLootRows().Any(l => l.ItemsRow.ItemName == lsTreasureChest)
                                select b;

            int droppedChestCount = droppedChests.Count();

            var chestBattles = from b in dataSet.Battles
                               where b.IsEnemyIDNull() == false &&
                               (EntityType)b.CombatantsRowByEnemyCombatantRelation.CombatantType == EntityType.TreasureChest
                               select b;

            int failedChests = 0;
            int expiredChests = 0;
            int openedChests = 0;
            int usedKey = 0;
            int chestsWithCruor = 0;
            int cruorTotal = 0;
            int chestsWithExperience = 0;
            int experienceTotal = 0;
            int chestsWithTE = 0;

            foreach (var chestBattle in chestBattles)
            {
                if (chestBattle.Killed)
                {
                    openedChests++;

                    if (chestBattle.GetInteractionsRows()
                        .Any(i => i.IsItemIDNull() == false && i.ItemsRow.ItemName == lsKey))
                    {
                        usedKey++;
                    }

                    if (chestBattle.ExperiencePoints > 0)
                    {
                        chestsWithExperience++;
                        experienceTotal += chestBattle.ExperiencePoints;
                    }
                    else
                    {
                        var chestLoot = chestBattle.GetLootRows();

                        if (chestLoot.Any())
                        {
                            var cruorLoot = chestLoot.Where(l => l.ItemsRow.ItemName == lsCruor);

                            if ((cruorLoot != null) && (cruorLoot.Any()))
                            {
                                chestsWithCruor++;
                                cruorTotal += cruorLoot.Sum(l => l.GilDropped);
                            }

                            var teLoot = chestLoot.Where(l => l.ItemsRow.ItemName == lsTimeExtension);

                            if ((teLoot != null) && (teLoot.Any()))
                            {
                                chestsWithTE++;
                            }
                        }
                    }
                }
                else
                {
                    if (chestBattle.GetInteractionsRows()
                        .Any(i => (FailedActionType)i.FailedActionType == FailedActionType.FailedUnlock))
                    {
                        failedChests++;
                    }
                    else
                    {
                        expiredChests++;
                    }
                }
            }

            if (droppedChestCount + failedChests + openedChests > 0)
            {
                sb.AppendFormat("Dropped chests : {0}\n", droppedChestCount);
                sb.AppendFormat("Expired chests : {0}\n", expiredChests);
                sb.AppendFormat("Failed chests  : {0}\n", failedChests);
                sb.Append("\n");
                sb.AppendFormat("Opened chests  : {0}\n", openedChests);
                sb.AppendFormat(" - Using a key : {0}\n", usedKey);
                sb.Append("\n");

                if (chestsWithTE > 0)
                {
                    sb.AppendFormat("Chests that granted Time Extensions : {0}\n", chestsWithTE);
                    sb.AppendFormat(" - Total time gained  : {0}\n",
                        new TimeSpan(0, chestsWithTE * 10, 0).FormattedShortTimeSpanString());
                    sb.Append("\n");
                }

                if (chestsWithExperience > 0)
                {
                    sb.AppendFormat("Chests that granted experience : {0}\n", chestsWithExperience);
                    sb.AppendFormat(" - Total experience   : {0}\n", experienceTotal);
                    sb.AppendFormat(" - Average experience : {0:f2}\n", (double)experienceTotal / chestsWithExperience);
                    sb.Append("\n");
                }

                if (chestsWithCruor > 0)
                {
                    sb.AppendFormat("Chests that granted Cruor : {0}\n", chestsWithCruor);
                    sb.AppendFormat(" - Total Cruor        : {0}\n", cruorTotal);
                    sb.AppendFormat(" - Average Cruor      : {0:f2}\n", (double)cruorTotal / chestsWithCruor);
                }

                sb.Append("\n\n");
            }
        }

        #endregion

        #region Event Handlers
        protected void categoryCombo_SelectedIndexChanged(object sender, EventArgs e)
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

        protected void groupMobs_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            groupMobs = sentBy.Checked;

            if (flagNoUpdate == false)
            {
                flagNoUpdate = true;
                UpdateMobList();

                HandleDataset(null);
            }

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
                flagNoUpdate = true;
                UpdateMobList();

                HandleDataset(null);
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
                HandleDataset(null);
            }

            flagNoUpdate = false;
        }

        protected void editCustomMobFilter_Click(object sender, EventArgs e)
        {
            MobXPHandler.Instance.ShowCustomMobFilter();
        }

        protected override void OnCustomMobFilterChanged()
        {
            HandleDataset(null);
        }

        #endregion

        #region Localization Overrides
        protected override void LoadLocalizedUI()
        {
            catLabel.Text = Resources.PublicResources.CategoryLabel;
            mobsLabel.Text = Resources.PublicResources.MobsLabel;

            categoryCombo.Items.Clear();
            categoryCombo.Items.Add(Resources.PublicResources.All);
            categoryCombo.Items.Add("Lights");
            categoryCombo.Items.Add("Mobs");
            categoryCombo.Items.Add("Chests");
            categoryCombo.SelectedIndex = 0;

            UpdateMobList();
            mobsCombo.SelectedIndex = 0;

            optionsMenu.Text = Resources.PublicResources.Options;
            groupMobsOption.Text = Resources.PublicResources.GroupMobs;
            exclude0XPOption.Text = Resources.PublicResources.Exclude0XPMobs;
            customMobSelectionOption.Text = Resources.PublicResources.CustomMobSelection;
            editCustomMobFilter.Text = Resources.PublicResources.EditMobFilter;

        }

        protected override void LoadResources()
        {
            this.tabName = "Abyssea";

            lsCruor = Resources.ParsedStrings.Cruor;
            lsTreasureChest = "Treasure Chest";
            lsTimeExtension = Resources.PublicResources.TimeExtension;
            lsKey = "forbidden key";
        }
        #endregion
    }
}
