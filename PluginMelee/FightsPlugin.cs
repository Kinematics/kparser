using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Drawing;

namespace WaywardGamers.KParser.Plugin
{
    public class FightsPlugin : BasePluginControl
    {
        public override string TabName
        {
            get { return "Fights"; }
        }

        public override void Reset()
        {
            richTextBox.Clear();
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
            }

            datasetToUse = null;
            return false;
        }

        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            richTextBox.Clear();

            var fights = from b in dataSet.Battles
                         where b.DefaultBattle == false &&
                               b.IsEnemyIDNull() == false &&
                               b.EndTime != MagicNumbers.MinSQLDateTime &&
                               b.CombatantsRowByEnemyCombatantRelation.CombatantType == (byte)EntityType.Mob
                         select b;

            if (fights.Count() == 0)
                return;

            string fightHeader = "Fight #   Enemy                   Killed?   Killed By           Start Time   End Time   Fight Length   Exp\n";

            AppendBoldText(fightHeader, Color.Black);

            int fightNum = 0;

            foreach (var fight in fights)
            {
                fightNum++;

                string killer = string.Empty;
                if (fight.IsKillerIDNull() == false)
                    killer = fight.CombatantsRowByBattleKillerRelation.CombatantName;

                TimeSpan fightLength = fight.FightLength();

                string fightLengthString = string.Empty;

                if ((fightLength.Days > 0) || (fightLength.TotalDays < 0))
                {
                    fightLengthString = string.Format("{0:f2} days",
                        fightLength.TotalDays);
                }
                else
                {
                    fightLengthString = string.Format("{0:d2}:{1:d2}:{2:d2}",
                        fightLength.Hours, fightLength.Minutes, fightLength.Seconds, fightLength.Days);
                }

                AppendNormalText(string.Format("{0,-10}{1,-24}{2,-10}{3,-20}{4,10}{5,11}{6,15}{7,6}\n",
                    fightNum, fight.CombatantsRowByEnemyCombatantRelation.CombatantName,
                    fight.Killed, killer, fight.StartTime.ToShortTimeString(),
                    fight.EndTime.ToShortTimeString(), fightLengthString, fight.ExperiencePoints));

            }
        }
    }
}
