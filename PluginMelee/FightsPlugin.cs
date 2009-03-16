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
        #region Constructor
        public FightsPlugin()
        {
            toolStrip.Enabled = false;
            toolStrip.Visible = false;

            richTextBox.Anchor = System.Windows.Forms.AnchorStyles.Left |
                System.Windows.Forms.AnchorStyles.Right |
                System.Windows.Forms.AnchorStyles.Bottom;
            richTextBox.Top -= toolStrip.Height;
            richTextBox.Height += toolStrip.Height;
            richTextBox.Anchor = System.Windows.Forms.AnchorStyles.Top |
                System.Windows.Forms.AnchorStyles.Left |
                System.Windows.Forms.AnchorStyles.Right |
                System.Windows.Forms.AnchorStyles.Bottom;
        }
        #endregion

        #region IPlugin Overrides
        public override string TabName
        {
            get { return "Fights"; }
        }

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
                }
            }
        }

        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            ResetTextBox();
            List<StringMods> strModList = new List<StringMods>();
            StringBuilder sb = new StringBuilder();

            var fights = from b in dataSet.Battles
                         where b.DefaultBattle == false &&
                               b.IsEnemyIDNull() == false &&
                               b.EndTime != MagicNumbers.MinSQLDateTime &&
                               ((EntityType)b.CombatantsRowByEnemyCombatantRelation.CombatantType == EntityType.Mob ||
                                (EntityType)b.CombatantsRowByEnemyCombatantRelation.CombatantType == EntityType.CharmedPlayer)
                         select b;

            if (fights.Count() == 0)
                return;

            string fightHeader = "Fight #   Enemy                   Killed?   Killed By           Start Time   End Time   Fight Length   Exp\n";

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = fightHeader.Length,
                Bold = true,
                Color = Color.Black
            });
            sb.Append(fightHeader);


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

                
                sb.AppendFormat("{0,-10}{1,-24}{2,-10}{3,-20}{4,10}{5,11}{6,15}{7,6}\n",
                    fightNum, fight.CombatantsRowByEnemyCombatantRelation.CombatantName,
                    fight.Killed, killer, fight.StartTime.ToLocalTime().ToShortTimeString(),
                    fight.EndTime.ToLocalTime().ToShortTimeString(), fightLengthString, fight.ExperiencePoints);

            }

            PushStrings(sb, strModList);
        }
        #endregion
    }
}
