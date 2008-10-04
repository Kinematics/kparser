﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Windows.Forms;
using WaywardGamers.KParser.Interface;

namespace WaywardGamers.KParser.Plugin
{
    public class ExperiencePlugin : BasePluginControl
    {
        #region Constructor
        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();
        bool excludedPlayerInfo = true;

        public ExperiencePlugin()
        {
            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;
            optionsMenu.Text = "Options";

            ToolStripMenuItem excludedPlayerInfoOption = new ToolStripMenuItem();
            excludedPlayerInfoOption.Text = "Don't Count 'exclude'd Player Kills";
            excludedPlayerInfoOption.CheckOnClick = true;
            excludedPlayerInfoOption.Checked = true;
            excludedPlayerInfoOption.Click += new EventHandler(excludedPlayerInfoOption_Click);
            optionsMenu.DropDownItems.Add(excludedPlayerInfoOption);

            toolStrip.Items.Add(optionsMenu);



            //toolStrip.Enabled = false;
            //toolStrip.Visible = false;

            //richTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left |
            //    System.Windows.Forms.AnchorStyles.Right |
            //    System.Windows.Forms.AnchorStyles.Bottom;
            //richTextBox.Top -= toolStrip.Height;
            //richTextBox.Height += toolStrip.Height;
            //richTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top |
            //    System.Windows.Forms.AnchorStyles.Left |
            //    System.Windows.Forms.AnchorStyles.Right |
            //    System.Windows.Forms.AnchorStyles.Bottom;
        }
        #endregion

        #region IPlugin Overrides
        public override string TabName
        {
            get { return "Experience"; }
        }

        public override void Reset()
        {
            ResetTextBox();
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
        {
            if (e.DatasetChanges != null)
            {
                if (e.DatasetChanges.Battles.Any(b => (b.Killed == true) || (b.EndTime != MagicNumbers.MinSQLDateTime)))
                {
                    datasetToUse = e.Dataset;
                    return true;
                }

                if (e.DatasetChanges.Combatants.Count > 0)
                {
                    datasetToUse = e.Dataset;
                    return true;
                }
            }

            datasetToUse = null;
            return false;
        }

        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            ResetTextBox();
            ProcessExperience(dataSet);
            ProcessMobs(dataSet);
        }
        #endregion

        #region Processing functions
        private void ProcessExperience(KPDatabaseDataSet dataSet)
        {
            Regex excludePlayersRegex = new Regex("exclude");

            StringBuilder sb1 = new StringBuilder();
            StringBuilder sb2 = new StringBuilder();

            var completedFights = dataSet.Battles.Where(b =>
                b.Killed == true &&
                b.EndTime > b.StartTime);


            var completedFights2 = from b in dataSet.Battles
                                   where b.Killed == true &&
                                   (excludedPlayerInfo == false ||
                                    b.IsKillerIDNull() == true ||
                                    excludePlayersRegex.Match(b.CombatantsRowByBattleKillerRelation.PlayerInfo).Success == false)
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

                foreach (var fight in completedFights2)
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


                sb1.AppendFormat("Total Experience : {0}\n", totalXP);
                sb1.AppendFormat("Number of Fights : {0}\n", totalFights);
                sb1.AppendFormat("Start Time       : {0}\n", startTime.ToLongTimeString());
                sb1.AppendFormat("End Time         : {0}\n", endTime.ToLongTimeString());
                sb1.AppendFormat("Party Duration   : {0:d}:{1:d2}:{2:d2}\n",
                    partyDuration.Hours, partyDuration.Minutes, partyDuration.Seconds);
                sb1.AppendFormat("Total Fight Time : {0:d}:{1:d2}:{2:d2}\n",
                    totalFightsLength.Hours, totalFightsLength.Minutes, totalFightsLength.Seconds);
                sb1.AppendFormat("Avg Time/Fight   : {0:F2} seconds\n", timePerFight);
                sb1.AppendFormat("Avg Fight Length : {0:F2} seconds\n", avgFightLength);
                sb1.AppendFormat("XP/Fight         : {0:F2}\n", xpPerFight);
                sb1.AppendFormat("XP/Minute        : {0:F2}\n", xpPerMinute);
                sb1.AppendFormat("XP/Hour          : {0:F2}\n", xpPerHour);
                sb1.Append("\n\n");


                sb2.Append("Chain   Count   Total XP   Avg XP\n");

                for (int i = 0; i < 10; i++)
                {
                    if (chainCounts[i] > 0)
                        sb2.AppendFormat("{0,-5}{1,8}{2,11}{3,9:F2}\n", i, chainCounts[i], chainXPTotals[i],
                            (double)chainXPTotals[i] / chainCounts[i]);
                }

                if (chainCounts[10] > 0)
                {
                    sb2.AppendFormat("{0,-5}{1,8}{2,11}{3,9:F2}\n", "10+", chainCounts[10], chainXPTotals[10],
                        (double)chainXPTotals[10] / chainCounts[10]);
                }

                sb2.Append("\n");
                sb2.AppendFormat("Highest Chain:  {0}\n\n\n", maxChain);


                // Dump all the constructed text above into the window.
                AppendText("Experience Rates\n", Color.Black, true, false);
                AppendText(sb1.ToString());

                AppendText("Experience Chains\n", Color.Black, true, false);
                AppendText(sb2.ToString());
            }
        }

        private void ProcessMobs(KPDatabaseDataSet dataSet)
        {
            var mobSet = from c in dataSet.Combatants
                         where (c.CombatantType == (byte)EntityType.Mob)
                         orderby c.CombatantName
                         select new
                         {
                             Mob = c.CombatantName,
                             //Battles = from b in c.GetBattlesRowsByEnemyCombatantRelation()
                             //          group b by b.BaseExperience() into bx
                             //          orderby bx.Key
                             //          select bx,
                             Battles = from b in c.GetBattlesRowsByEnemyCombatantRelation()
                                       group b by b.MinBaseExperience() into bx
                                       orderby bx.Key
                                       select bx
                         };

            if ((mobSet == null) || (mobSet.Count() == 0))
                return;

            string mobSetHeader = "Mob                        Base XP   Number   Avg Fight Time\n";

            StringBuilder sb = new StringBuilder();
            bool headerDisplayed = false;

            double ttlMobFightTime;
            double avgMobFightTime;

            int mobCount;

            foreach (var mob in mobSet)
            {
                if (mob.Battles.Count() > 0)
                {
                    foreach (var mobBattle in mob.Battles)
                    {
                        mobCount = mobBattle.Count();

                        if (mobCount > 0)
                        {
                            if (headerDisplayed == false)
                            {
                                AppendText("Mob Listing\n", Color.Blue, true, false);
                                AppendText(mobSetHeader, Color.Black, true, true);

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
                            if (killedMobs.Count() > 0)
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

            if (headerDisplayed == true)
            {
                sb.Append("\n\n");
                AppendText(sb.ToString());
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

            HandleDataset(DatabaseManager.Instance.Database);
        }
        #endregion

    }
}
