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
        #region Member Variables

        // Localized strings

        string lsFightHeader;
        string lsFightFormat;

        string lsDays;
        string lsUnknown;

        #endregion

        #region Constructor
        public FightsPlugin()
        {
            LoadLocalizedUI();

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
                               b.EndTime != MagicNumbers.MinSQLDateTime &&
                               (b.IsEnemyIDNull() == true ||
                               ((EntityType)b.CombatantsRowByEnemyCombatantRelation.CombatantType == EntityType.Mob ||
                                (EntityType)b.CombatantsRowByEnemyCombatantRelation.CombatantType == EntityType.CharmedPlayer))
                         select b;

            if (fights.Count() == 0)
                return;

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsFightHeader.Length,
                Bold = true,
                Color = Color.Black
            });
            sb.Append(lsFightHeader + "\n");


            int fightNum = 0;
            string enemy = string.Empty;

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
                    fightLengthString = string.Format("{0:f2} ",
                        fightLength.TotalDays) + lsDays;
                }
                else
                {
                    fightLengthString = string.Format("{0:d2}:{1:d2}:{2:d2}",
                        fightLength.Hours, fightLength.Minutes, fightLength.Seconds, fightLength.Days);
                }

                if (fight.IsEnemyIDNull())
                    enemy = lsUnknown;
                else
                    enemy = fight.CombatantsRowByEnemyCombatantRelation.CombatantName;
                
                sb.AppendFormat(lsFightFormat,
                    fightNum, enemy,
                    fight.Killed, killer,
                    fight.StartTime.ToLocalTime().ToShortTimeString(),
                    fight.EndTime.ToLocalTime().ToShortTimeString(),
                    fightLengthString,
                    fight.ExperiencePoints, fight.ExperienceChain);
                sb.Append("\n");
            }

            PushStrings(sb, strModList);
        }
        #endregion

        #region Localization Overrides
        protected override void LoadLocalizedUI()
        {
            // No UI to localize in this plugin.
        }

        protected override void LoadResources()
        {
            this.tabName = Resources.Combat.FightsPluginTabName;

            lsFightHeader = Resources.Combat.FightsPluginFightHeader;
            lsFightFormat = Resources.Combat.FightsPluginFightFormat;

            lsDays = Resources.PublicResources.Days;
            lsUnknown = Resources.Combat.FightsPluginUnknownEnemy;
        }
        #endregion

    }
}
