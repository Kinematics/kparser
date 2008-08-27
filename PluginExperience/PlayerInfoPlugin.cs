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
        public override string TabName
        {
            get { return "Player Info"; }
        }

        public override void Reset()
        {
            ResetTextBox();
        }

        public override void DatabaseOpened(KPDatabaseDataSet dataSet)
        {
            ProcessData(dataSet);
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
        {
            if (e.DatasetChanges != null)
            {
                if ((e.DatasetChanges.Combatants != null) &&
                    (e.DatasetChanges.Combatants.Count > 0))
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

            var playerData = from c in dataSet.Combatants
                             where ((EntityType)c.CombatantType == EntityType.Player ||
                                     (EntityType)c.CombatantType == EntityType.Pet ||
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

            foreach (var player in playerData)
            {
                if (player.Description != "")
                {
                    AppendBoldText(player.Name, Color.Red);
                    AppendNormalText(string.Format("\n    {0}\n\n", player.Description));
                }
            }

            AppendNormalText("\n");
        }
    }
}
