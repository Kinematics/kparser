using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Drawing;


namespace WaywardGamers.KParser.Plugin
{
    public class ExperiencePlugin : BasePluginControl
    {
        public override string TabName
        {
            get { return "Experience"; }
        }

        public override void Reset()
        {
            richTextBox.Clear();
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
        {
            if (e.DatasetChanges.Battles.Any(b => b.Killed == true))
            {
                datasetToUse = e.FullDataset;
                return true;
            }

            datasetToUse = null;
            return false;
        }

        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            richTextBox.Clear();

            var completedFights = dataSet.Battles.Where(b => b.Killed == true);
            int totalFights = completedFights.Count();

            if (totalFights > 0)
            {
                AppendBoldText("Experience Rates\n", Color.Black);

                int totalXP = completedFights.Sum(b => b.ExperiencePoints);

                DateTime startTime = completedFights.First(b => b.Killed == true).StartTime;
                DateTime endTime = completedFights.Last(b => b.Killed == true).EndTime;

                TimeSpan partyDuration = endTime - startTime;
                Double totalFightLengths = completedFights.Sum(b => b.FightLength().TotalSeconds);

                string expFormat = "{0}: {1}\n";
                string expDecFormat = "{0}: {1:f2}\n";
                string expDurFormat = "{0}: {1:f2} seconds\n";


                AppendNormalText(string.Format(expFormat, "Total Experience".PadRight(17), totalXP));
                AppendNormalText(string.Format(expFormat, "Total Fights".PadRight(17), totalFights));

                AppendNormalText(string.Format(expFormat, "Start Time".PadRight(17), startTime.ToLongTimeString()));
                AppendNormalText(string.Format(expFormat, "End Time".PadRight(17), endTime.ToLongTimeString()));

                if (partyDuration > TimeSpan.FromSeconds(1))
                {
                    AppendNormalText(string.Format(expDecFormat, "XP/Hour".PadRight(17), totalXP / partyDuration.TotalHours));
                    AppendNormalText(string.Format(expDecFormat, "XP/Minute".PadRight(17), totalXP / partyDuration.TotalMinutes));
                    AppendNormalText(string.Format(expDecFormat, "XP/Fight".PadRight(17), (double)totalXP / totalFights));

                    AppendNormalText(string.Format(expDurFormat, "Avg Fight Length".PadRight(17),
                        totalFightLengths / totalFights));
                    AppendNormalText(string.Format(expDurFormat, "Avg Time/Fight".PadRight(17),
                        partyDuration.TotalSeconds / totalFights));
                }

                AppendBoldText("\n\nExperience Chains\n", Color.Black);

                string chainFormat = "{0} {1} {2} {3}\n";
                EnumerableRowCollection<KPDatabaseDataSet.BattlesRow> chain;
                int chainCount;
                int chainXP;
                double avgChainXP;

                AppendNormalText(string.Format(chainFormat, "Chain".PadRight(6), "Count".PadRight(6),
                    "Total XP".PadRight(9), "Average XP"));

                for (int i = 0; i < 10; i++)
                {
                    chain = completedFights.Where(b => b.ExperienceChain == i);
                    chainCount = chain.Count();
                    if (chainCount > 0)
                    {
                        chainXP = chain.Sum(b => b.ExperiencePoints);
                        avgChainXP = (double)chainXP / chainCount;

                        AppendNormalText(string.Format(chainFormat, i.ToString().PadRight(6),
                            chainCount.ToString().PadRight(6), chainXP.ToString().PadRight(9),
                            avgChainXP.ToString("F2")));
                    }
                }

                chain = completedFights.Where(b => b.ExperienceChain >= 10);
                chainCount = chain.Count();
                if (chainCount > 0)
                {
                    chainXP = chain.Sum(b => b.ExperiencePoints);
                    avgChainXP = (double)chainXP / chainCount;

                    AppendNormalText(string.Format(chainFormat, "10+", chainCount, chainXP, avgChainXP.ToString("F2")));
                }

                int maxChain = dataSet.Battles.Max(b => b.ExperienceChain);

                AppendNormalText(string.Format("\nHighest Chain: {0}\n", maxChain));
            }
        }
    }
}
