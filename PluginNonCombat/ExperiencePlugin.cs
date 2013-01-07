using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Windows.Forms;
using WaywardGamers.KParser.Interface;
using WaywardGamers.KParser.Database;

namespace WaywardGamers.KParser.Plugin
{
    public class ExperiencePlugin : BasePluginControl
    {
        #region Member Variables
        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();
        ToolStripMenuItem excludedPlayerInfoOption = new ToolStripMenuItem();

        bool excludedPlayerInfo = true;

        // Localized strings

        // For formatting
        string lsXPListFormatNum;
        string lsXPListFormatTime;
        string lsXPListFormatSec;
        string lsXPListFormatDec;
        string lsChainFormat;

        // For text
        string lsMobListing;
        string lsMobListingHeader;
        string lsTotalExperience;
        string lsNumberOfFights;
        string lsDate;
        string lsStartTime;
        string lsEndTime;
        string lsPartyDuration;
        string lsTotalFightTime;
        string lsAverageTimePerFight;
        string lsAverageFightLength;
        string lsXPPerFight;
        string lsXPPerMinute;
        string lsXPPerHour;
        string lsChainHeader;
        string lsHighestChain;
        string lsExperienceRates;
        string lsExperienceChains;
        #endregion

        #region Constructor
        public ExperiencePlugin()
        {
            LoadLocalizedUI();

            excludedPlayerInfoOption.CheckOnClick = true;
            excludedPlayerInfoOption.Checked = true;
            excludedPlayerInfoOption.Click += new EventHandler(excludedPlayerInfoOption_Click);

            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;
            optionsMenu.DropDownItems.Add(excludedPlayerInfoOption);

            toolStrip.Items.Add(optionsMenu);
        }
        #endregion

        #region IPlugin Overrides
        public override void Reset()
        {
            ResetTextBox();
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            if (e.DatasetChanges != null)
            {
                if (e.DatasetChanges.Battles.Any(b => (b.Killed == true) || (b.EndTime != MagicNumbers.MinSQLDateTime)))
                {
                    HandleDataset(null);
                    return;
                }

                if (e.DatasetChanges.Combatants.Count > 0)
                {
                    HandleDataset(null);
                    return;
                }
            }
        }

        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            ResetTextBox();
            MobXPHandler.Instance.Update();

            StringBuilder sb = new StringBuilder();
            List<StringMods> strModList = new List<StringMods>();

            ProcessExperience(dataSet, ref sb, ref strModList);
            ProcessMobs(dataSet, ref sb, ref strModList);

            PushStrings(sb, strModList);
        }
        #endregion

        #region Processing functions
        private void ProcessExperience(KPDatabaseDataSet dataSet,
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            var completedFights = from b in dataSet.Battles
                                   where b.Killed == true &&
                                   (excludedPlayerInfo == false ||
                                    b.IsKillerIDNull() == true ||
                                    RegexUtility.ExcludedPlayer.Match(b.CombatantsRowByBattleKillerRelation.PlayerInfo).Success == false)
                                   select b;

            int totalFights = completedFights.Count();

            if (totalFights > 0)
            {
                DateTime startTime;
                DateTime endTime;
                TimeSpan partyDuration;
                TimeSpan totalFightsLength = new TimeSpan(); ;
                TimeSpan minTime = TimeSpan.FromSeconds(1);

                int totalXP = 0;
                double xpPerHour = 0;
                double xpPerMinute = 0;
                double xpPerFight = 0;

                double avgFightLength;
                double timePerFight;

                int[] chainXPTotals = new int[11];
                int[] chainCounts = new int[11];
                int maxChain = 0;

                int chainNum;

                foreach (var fight in completedFights)
                {
                    totalFightsLength += fight.FightLength();

                    chainNum = fight.ExperienceChain;

                    if (chainNum > maxChain)
                        maxChain = chainNum;

                    if (chainNum < 10)
                    {
                        chainCounts[chainNum]++;
                        chainXPTotals[chainNum] += fight.ExperiencePoints;
                    }
                    else
                    {
                        chainCounts[10]++;
                        chainXPTotals[10] += fight.ExperiencePoints;
                    }

                    totalXP += fight.ExperiencePoints;
                }


                startTime = completedFights.First(b => b.Killed == true).StartTime;
                endTime = completedFights.Last(b => b.Killed == true).EndTime;
                partyDuration = endTime - startTime;

                if (partyDuration > minTime)
                {
                    double totalXPDouble = (double)totalXP;
                    xpPerHour = totalXPDouble / partyDuration.TotalHours;
                    xpPerMinute = totalXPDouble / partyDuration.TotalMinutes;
                    xpPerFight = totalXPDouble / totalFights;
                }

                avgFightLength = totalFightsLength.TotalSeconds / totalFights;
                timePerFight = partyDuration.TotalSeconds / totalFights;

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsExperienceRates.Length,
                    Bold = true,
                    Color = Color.Black
                });
                sb.Append(lsExperienceRates + "\n");


                sb.AppendFormat(lsXPListFormatNum + "\n", lsTotalExperience, totalXP);
                sb.AppendFormat(lsXPListFormatNum + "\n", lsNumberOfFights, totalFights);
                sb.AppendFormat(lsXPListFormatNum + "\n", lsDate, startTime.ToLocalTime().ToShortDateString());
                sb.AppendFormat(lsXPListFormatNum + "\n", lsStartTime, startTime.ToLocalTime().ToLongTimeString());
                sb.AppendFormat(lsXPListFormatNum + "\n", lsEndTime, endTime.ToLocalTime().ToLongTimeString());
                sb.AppendFormat(lsXPListFormatTime + "\n",
                    lsPartyDuration, partyDuration.Hours, partyDuration.Minutes, partyDuration.Seconds);
                sb.AppendFormat(lsXPListFormatTime + "\n",
                    lsTotalFightTime, totalFightsLength.Hours, totalFightsLength.Minutes, totalFightsLength.Seconds);
                sb.AppendFormat(lsXPListFormatSec + "\n", lsAverageTimePerFight, timePerFight);
                sb.AppendFormat(lsXPListFormatSec + "\n", lsAverageFightLength, avgFightLength);
                sb.AppendFormat(lsXPListFormatDec + "\n", lsXPPerFight, xpPerFight);
                sb.AppendFormat(lsXPListFormatDec + "\n", lsXPPerMinute, xpPerMinute);
                sb.AppendFormat(lsXPListFormatDec + "\n", lsXPPerHour, xpPerHour);
                sb.Append("\n\n");


                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsExperienceChains.Length,
                    Bold = true,
                    Color = Color.Black
                });
                sb.Append(lsExperienceChains + "\n");


                sb.Append(lsChainHeader);
                sb.Append("\n");

                for (int i = 0; i < 10; i++)
                {
                    if (chainCounts[i] > 0)
                        sb.AppendFormat(lsChainFormat + "\n", i, chainCounts[i], chainXPTotals[i],
                            (double)chainXPTotals[i] / chainCounts[i]);
                }

                if (chainCounts[10] > 0)
                {
                    sb.AppendFormat(lsChainFormat + "\n", "10+", chainCounts[10], chainXPTotals[10],
                        (double)chainXPTotals[10] / chainCounts[10]);
                }

                sb.Append("\n");
                sb.AppendFormat("{0}:  {1}\n\n\n", lsHighestChain, maxChain);
            }
        }

        private void ProcessMobs(KPDatabaseDataSet dataSet,
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            var mobSet = from c in dataSet.Combatants
                         where ((EntityType)c.CombatantType == EntityType.Mob)
                         orderby c.CombatantName
                         select new
                         {
                             Mob = c.CombatantName,
                             Battles = from b in c.GetBattlesRowsByEnemyCombatantRelation()
                                       where b.Killed == true &&
                                            (excludedPlayerInfo == false ||
                                             b.IsKillerIDNull() == true ||
                                             RegexUtility.ExcludedPlayer.Match(b.CombatantsRowByBattleKillerRelation.PlayerInfo).Success == false)
                                       //group b by b.MinBaseExperience() into bx
                                       group b by MobXPHandler.Instance.GetBaseXP(b.BattleID) into bx
                                       orderby bx.Key
                                       select bx
                         };

            var chestSet = from c in dataSet.Combatants
                         where ((EntityType)c.CombatantType == EntityType.TreasureChest)
                         orderby c.CombatantName
                         select new
                         {
                             Mob = c.CombatantName,
                             Battles = from b in c.GetBattlesRowsByEnemyCombatantRelation()
                                       where b.Killed == true &&
                                             b.ExperiencePoints != 0
                                       group b by b.ExperiencePoints into bx
                                       orderby bx.Key
                                       select bx
                         };

            if (((mobSet == null) || (mobSet.Count() == 0)) &&
                ((chestSet == null) || (chestSet.Count() == 0)))
                return;

            bool headerDisplayed = false;

            double ttlMobFightTime;
            double avgMobFightTime;

            int mobCount;
            int chestCount;

            foreach (var mob in mobSet)
            {
                if (mob.Battles.Any())
                {
                    foreach (var mobBattle in mob.Battles)
                    {
                        mobCount = mobBattle.Count();

                        if (mobCount > 0)
                        {
                            if (headerDisplayed == false)
                            {
                                strModList.Add(new StringMods
                                {
                                    Start = sb.Length,
                                    Length = lsMobListing.Length,
                                    Bold = true,
                                    Color = Color.Blue
                                });
                                sb.Append(lsMobListing + "\n");

                                strModList.Add(new StringMods
                                {
                                    Start = sb.Length,
                                    Length = lsMobListingHeader.Length,
                                    Bold = true,
                                    Underline = true,
                                    Color = Color.Black
                                });
                                sb.Append(lsMobListingHeader + "\n");

                                headerDisplayed = true;
                            }

                            sb.Append(mob.Mob.PadRight(24));

                            if (mobBattle.Key > 0)
                            {
                                sb.Append(mobBattle.Key.ToString().PadLeft(10));
                            }
                            else
                            {
                                sb.Append("---".PadLeft(10));
                            }

                            sb.Append(mobCount.ToString().PadLeft(9));

                            // Avg fight time per level
                            ttlMobFightTime = 0;
                            avgMobFightTime = 0;

                            var killedMobs = mobBattle.Where(m => m.Killed == true);
                            if (killedMobs.Any())
                            {
                                ttlMobFightTime = killedMobs.Sum(m => m.FightLength().TotalSeconds);
                                avgMobFightTime = ttlMobFightTime / mobCount;
                            }

                            TimeSpan tsAvgFight = TimeSpan.FromSeconds(avgMobFightTime);

                            string fightLengthString;

                            if (avgMobFightTime > 60)
                            {
                                fightLengthString = string.Format("{0}:{1:d2}.{2:d2}",
                                    tsAvgFight.Minutes, tsAvgFight.Seconds, tsAvgFight.Milliseconds / 10);
                            }
                            else
                            {
                                fightLengthString = string.Format("{0}.{1:d2}",
                                    tsAvgFight.Seconds, tsAvgFight.Milliseconds / 10);
                            }

                            sb.Append(fightLengthString.PadLeft(17));

                            sb.Append("\n");
                        }
                    }
                }
            }

            foreach (var chest in chestSet)
            {
                if (chest.Battles.Any())
                {
                    foreach (var openChest in chest.Battles)
                    {
                        chestCount = openChest.Count();

                        if (chestCount > 0)
                        {
                            if (headerDisplayed == false)
                            {
                                strModList.Add(new StringMods
                                {
                                    Start = sb.Length,
                                    Length = lsMobListing.Length,
                                    Bold = true,
                                    Color = Color.Blue
                                });
                                sb.Append(lsMobListing + "\n");

                                strModList.Add(new StringMods
                                {
                                    Start = sb.Length,
                                    Length = lsMobListingHeader.Length,
                                    Bold = true,
                                    Underline = true,
                                    Color = Color.Black
                                });
                                sb.Append(lsMobListingHeader + "\n");

                                headerDisplayed = true;
                            }

                            sb.Append(chest.Mob.PadRight(24));

                            if (openChest.Key > 0)
                            {
                                sb.Append(openChest.Key.ToString().PadLeft(10));
                            }
                            else
                            {
                                sb.Append("---".PadLeft(10));
                            }

                            sb.Append(chestCount.ToString().PadLeft(9));

                            sb.Append("\n");
                        }
                    }
                }
            }

            if (headerDisplayed == true)
            {
                sb.Append("\n\n");
            }
        }
        #endregion

        #region Event Handlers
        protected void excludedPlayerInfoOption_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            excludedPlayerInfo = sentBy.Checked;

            HandleDataset(null);
        }
        #endregion

        #region Localization Overrides
        protected override void LoadLocalizedUI()
        {
            optionsMenu.Text = Resources.PublicResources.Options;
            excludedPlayerInfoOption.Text = Resources.NonCombat.ExcludedPlayerOption;
        }

        protected override void LoadResources()
        {
            base.LoadResources();

            this.tabName = Resources.NonCombat.ExperiencePluginTabName;

            lsMobListing = Resources.NonCombat.ExperiencePluginMobListing;
            lsMobListingHeader = Resources.NonCombat.ExperiencePluginMobListingHeader;
            lsTotalExperience = Resources.NonCombat.ExperiencePluginTotalExperience;
            lsNumberOfFights = Resources.NonCombat.ExperiencePluginNumberOfFights;
            lsDate = Resources.NonCombat.ExperiencePluginDate;
            lsStartTime = Resources.NonCombat.ExperiencePluginStartTime;
            lsEndTime = Resources.NonCombat.ExperiencePluginEndTime;
            lsPartyDuration = Resources.NonCombat.ExperiencePluginPartyDuration;
            lsTotalFightTime = Resources.NonCombat.ExperiencePluginTotalFightTime;
            lsAverageTimePerFight = Resources.NonCombat.ExperiencePluginAverageTimePerFight;
            lsAverageFightLength = Resources.NonCombat.ExperiencePluginAverageFightLength;
            lsXPPerFight = Resources.NonCombat.ExperiencePluginXPPerFight;
            lsXPPerMinute = Resources.NonCombat.ExperiencePluginXPPerMinute;
            lsXPPerHour = Resources.NonCombat.ExperiencePluginXPPerHour;
            lsChainHeader = Resources.NonCombat.ExperiencePluginChainHeader;
            lsHighestChain = Resources.NonCombat.ExperiencePluginHighestChain;
            lsExperienceRates = Resources.NonCombat.ExperiencePluginExperienceRates;
            lsExperienceChains = Resources.NonCombat.ExperiencePluginExperienceChains;

            lsChainFormat = Resources.NonCombat.ExperiencePluginChainFormat;
            lsXPListFormatNum = Resources.NonCombat.ExperiencePluginXPListFormatNum;
            lsXPListFormatTime = Resources.NonCombat.ExperiencePluginXPListFormatTime;
            lsXPListFormatSec = Resources.NonCombat.ExperiencePluginXPListFormatSec;
            lsXPListFormatDec = Resources.NonCombat.ExperiencePluginXPListFormatDec;

        }
        #endregion

    }
}
