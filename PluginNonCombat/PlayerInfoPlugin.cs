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

            string playerDescrip;

            foreach (var player in playerData)
            {
                if (player.Description != "")
                {
                    playerDescrip = string.Format("\n    {0}\n\n", player.Description);
                    AppendText(player.Name, Color.Blue, true, false);
                    AppendText(playerDescrip);
                }
                else
                {
                    playerDescrip = string.Format("\n    {0}\n\n", Resources.NonCombat.PlayerInfoPluginNoInfo);
                    AppendText(player.Name, Color.Blue, true, false);
                    AppendText(playerDescrip, Color.Red, true, false);
                }
            }

            AppendText("\n");
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
