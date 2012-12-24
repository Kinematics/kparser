using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Drawing;

namespace WaywardGamers.KParser.Plugin
{
    public class PlayerInfoPlugin : BasePluginControl
    {
        #region Constructor
        public PlayerInfoPlugin()
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
        public override void Reset()
        {
            ResetTextBox();
        }

        public override void NotifyOfUpdate()
        {
            HandleDataset(null);
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            if (e.DatasetChanges != null)
            {
                if ((e.DatasetChanges.Combatants != null) &&
                    (e.DatasetChanges.Combatants.Count > 0))
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

            var playerData = from c in dataSet.Combatants
                             where ((EntityType)c.CombatantType == EntityType.Player ||
                                    (EntityType)c.CombatantType == EntityType.Pet ||
                                    (EntityType)c.CombatantType == EntityType.CharmedMob ||
                                    (EntityType)c.CombatantType == EntityType.Fellow)
                             orderby c.CombatantName
                             select new
                             {
                                 Name = c.CombatantName,
                                 ComType = c.CombatantType,
                                 Description = c.PlayerInfo
                             };

            if (playerData.Count() == 0)
                return;

            string noInfoDescrip = string.Format("    {0}", Resources.NonCombat.PlayerInfoPluginNoInfo);

            foreach (var player in playerData)
            {
                if (player.Description != "")
                {
                    strModList.Add(new StringMods
                    {
                        Start = sb.Length,
                        Length = player.Name.Length,
                        Bold = true,
                        Color = Color.Blue
                    });
                    sb.Append(player.Name + "\n");

                    sb.AppendFormat("    {0}\n\n", player.Description);
                }
                else
                {
                    strModList.Add(new StringMods
                    {
                        Start = sb.Length,
                        Length = player.Name.Length,
                        Bold = true,
                        Color = Color.Blue
                    });
                    sb.Append(player.Name + "\n");

                    strModList.Add(new StringMods
                    {
                        Start = sb.Length,
                        Length = noInfoDescrip.Length,
                        Bold = true,
                        Color = Color.Red
                    });
                    sb.Append(noInfoDescrip + "\n\n");
                }
            }

            PushStrings(sb, strModList);
        }
        #endregion

        #region Localization Overrides
        protected override void LoadResources()
        {
            base.LoadResources();

            this.tabName = Resources.NonCombat.PlayerInfoPluginTabName;
        }
        #endregion

    }
}
