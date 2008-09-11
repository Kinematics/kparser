using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WaywardGamers.KParser;

namespace WaywardGamers.KParser.Plugin
{
    public class EnfeeblePlugin : BasePluginControl
    {
        #region Constructor
        string debuffHeader = "Debuff              Used on                 # Times   # Successful   # No Effect   % Successful\n";

        bool flagNoUpdate = false;
        ToolStripComboBox categoryCombo = new ToolStripComboBox();
        ToolStripComboBox mobsCombo = new ToolStripComboBox();

        public EnfeeblePlugin()
        {
            ToolStripLabel catLabel = new ToolStripLabel();
            catLabel.Text = "Category:";
            toolStrip.Items.Add(catLabel);

            categoryCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            categoryCombo.Items.Add("Debuff Mobs");
            categoryCombo.Items.Add("Debuff Players");
            categoryCombo.SelectedIndex = 0;
            categoryCombo.SelectedIndexChanged += new EventHandler(this.categoryCombo_SelectedIndexChanged);
            toolStrip.Items.Add(categoryCombo);


            ToolStripLabel mobLabel = new ToolStripLabel();
            mobLabel.Text = "Mobs:";
            toolStrip.Items.Add(mobLabel);

            mobsCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            mobsCombo.MaxDropDownItems = 10;
            mobsCombo.AutoSize = false;
            mobsCombo.Width = 175;
            mobsCombo.Items.Add("All");
            mobsCombo.SelectedIndex = 0;
            mobsCombo.SelectedIndexChanged += new EventHandler(this.mobsCombo_SelectedIndexChanged);
            toolStrip.Items.Add(mobsCombo);

        }
        #endregion

        #region IPlugin Overrides
        public override string TabName
        {
            get { return "Debuffs"; }
        }

        public override void Reset()
        {
            ResetTextBox();
        }

        public override void DatabaseOpened(KPDatabaseDataSet dataSet)
        {
            ResetTextBox();

            UpdateMobList(dataSet);

            flagNoUpdate = true;
            mobsCombo.CBSelectIndex(0);

            ProcessData(dataSet);
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
        {
            // Check for new mobs being fought.  If any exist, update the Mob Group dropdown list.
            if (e.DatasetChanges.Battles.Count > 0)
            {
                string mobSelected = mobsCombo.CBSelectedItem();
                flagNoUpdate = true;
                UpdateMobList(e.Dataset);

                flagNoUpdate = true;
                mobsCombo.CBSelectItem(mobSelected);
            }

            if (e.DatasetChanges.Interactions.Count != 0)
            {
                var enfeebles = from i in e.DatasetChanges.Interactions
                                where i.HarmType == (byte)HarmType.Enfeeble
                                select i;

                if (enfeebles.Count() > 0)
                {
                    datasetToUse = e.Dataset;
                    return true;
                }
            }

            datasetToUse = null;
            return false;
        }
        #endregion

        #region Private functions
        private void UpdateMobList(KPDatabaseDataSet dataSet)
        {
            mobsCombo.CBReset();

            // Enemy group listing

            var mobsKilled = from b in dataSet.Battles
                             where ((b.DefaultBattle == false) &&
                                    (b.IsEnemyIDNull() == false) &&
                                    (b.CombatantsRowByEnemyCombatantRelation.CombatantType == (byte)EntityType.Mob))
                             orderby b.CombatantsRowByEnemyCombatantRelation.CombatantName
                             group b by b.CombatantsRowByEnemyCombatantRelation.CombatantName into bn
                             select new
                             {
                                 Name = bn.Key,
                                 XP = from xb in bn
                                      group xb by xb.MinBaseExperience() into xbn
                                      orderby xbn.Key
                                      select new { BaseXP = xbn.Key }
                             };

            List<string> mobXPStrings = new List<string>();
            mobXPStrings.Add("All");

            foreach (var mob in mobsKilled)
            {
                mobXPStrings.Add(mob.Name);

                foreach (var xp in mob.XP)
                {
                    if (xp.BaseXP > 0)
                        mobXPStrings.Add(string.Format("{0} ({1})", mob.Name, xp.BaseXP));
                }
            }

            mobsCombo.CBAddStrings(mobXPStrings.ToArray());

        }
        #endregion

        #region Processing sections
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            ResetTextBox();
            MobFilter mobFilter = mobsCombo.CBGetMobFilter();

            #region LINQ group construction

            IEnumerable<DebuffGroup> debuffSet = null;
            bool processPlayerDebuffs = (categoryCombo.CBSelectedIndex() == 0);

            if (processPlayerDebuffs == true)
            {
                // Process debuffs used by players

                debuffSet = from c in dataSet.Combatants
                            where ((c.CombatantType == (byte)EntityType.Player) ||
                                  (c.CombatantType == (byte)EntityType.Pet) ||
                                  (c.CombatantType == (byte)EntityType.Fellow))
                            orderby c.CombatantType, c.CombatantName
                            select new DebuffGroup
                            {
                                DebufferName = c.CombatantName,
                                Debuffs = from b in c.GetInteractionsRowsByActorCombatantRelation()
                                          where (b.HarmType == (byte)HarmType.Enfeeble ||
                                                 b.HarmType == (byte)HarmType.Dispel ||
                                                 b.HarmType == (byte)HarmType.Unknown) &&
                                                b.Preparing == false &&
                                                b.IsActionIDNull() == false
                                          group b by b.ActionsRow.ActionName into ba
                                          orderby ba.Key
                                          select new Debuffs
                                          {
                                              DebuffName = ba.Key,
                                              DebuffTargets = from bt in ba
                                                              where (bt.IsTargetIDNull() == false &&
                                                                  // all mobs if mobFilter == All
                                                                     ((mobFilter.MobName == "All") ||
                                                                  // else make sure mob name matches and
                                                                      (bt.CombatantsRowByTargetCombatantRelation.CombatantName == mobFilter.MobName &&
                                                                  // either no xp requirement
                                                                       (mobFilter.MobXP == 0 ||
                                                                  // or there's a battle entry and it has the specified XP amount
                                                                        (bt.IsBattleIDNull() == false &&
                                                                         bt.BattlesRow.MinBaseExperience() == mobFilter.MobXP)
                                                                       )
                                                                      )
                                                                     )
                                                                    )
                                                              group bt by bt.CombatantsRowByTargetCombatantRelation.CombatantName into btn
                                                              orderby btn.Key
                                                              select new DebuffTargets
                                                              {
                                                                  TargetName = btn.Key,
                                                                  DebuffData = btn.OrderBy(i => i.Timestamp)
                                                              }
                                          }
                            };

            }
            else
            {
                // Process debuffs used by mobs

                debuffSet = from c in dataSet.Combatants
                            where (c.CombatantType == (byte)EntityType.Mob &&
                                // all mobs if mobFilter == All
                                   (mobFilter.MobName == "All" ||
                                // else make sure mob name matches and
                                    c.CombatantName == mobFilter.MobName)
                                  )
                            orderby c.CombatantType, c.CombatantName
                            select new DebuffGroup
                            {
                                DebufferName = c.CombatantName,
                                Debuffs = from b in c.GetInteractionsRowsByActorCombatantRelation()
                                          where ((b.HarmType == (byte)HarmType.Enfeeble ||
                                                 b.HarmType == (byte)HarmType.Dispel ||
                                                 b.HarmType == (byte)HarmType.Unknown) &&
                                                 b.Preparing == false &&
                                                 b.IsActionIDNull() == false) &&
                                              // either no xp requirement
                                                (mobFilter.MobXP == 0 ||
                                              // or there's a battle entry and it has the specified XP amount
                                                 (b.IsBattleIDNull() == false &&
                                                  b.BattlesRow.MinBaseExperience() == mobFilter.MobXP)
                                                )
                                          group b by b.ActionsRow.ActionName into ba
                                          orderby ba.Key
                                          select new Debuffs
                                          {
                                              DebuffName = ba.Key,
                                              DebuffTargets = from bt in ba
                                                              where (bt.IsTargetIDNull() == false)
                                                              group bt by bt.CombatantsRowByTargetCombatantRelation.CombatantName into btn
                                                              orderby btn.Key
                                                              select new DebuffTargets
                                                              {
                                                                  TargetName = btn.Key,
                                                                  DebuffData = btn.OrderBy(i => i.Timestamp)
                                                              }
                                          }
                            };

            }
            #endregion


            if (processPlayerDebuffs == true)
                ProcessPlayerDebuffs(debuffSet);
            else
                ProcessMobDebuffs(debuffSet);
        }

        private void ProcessPlayerDebuffs(IEnumerable<DebuffGroup> debuffSet)
        {
            int used;
            int successful;
            int noEffect;
            double percSuccess;
            string debuffName;

            foreach (var player in debuffSet)
            {
                if ((player.Debuffs == null) || (player.Debuffs.Count() == 0))
                    continue;

                if (player.Debuffs.Sum(d => d.DebuffTargets.Count()) == 0)
                    continue;

                AppendText(string.Format("{0}\n", player.DebufferName), Color.Blue, true, false);
                AppendText(debuffHeader, Color.Black, true, true);

                foreach (var debuff in player.Debuffs)
                {
                    debuffName = debuff.DebuffName;

                    foreach (var target in debuff.DebuffTargets)
                    {
                        used = target.DebuffData.Count();

                        if (used > 0)
                        {
                            AppendText(debuffName.PadRight(20));
                            debuffName = "";

                            AppendText(target.TargetName.PadRight(24));

                            successful = target.DebuffData.Count(d =>
                                (d.DefenseType == (byte)DefenseType.None) &&
                                (d.FailedActionType == (byte)FailedActionType.None));

                            noEffect = target.DebuffData.Count(d => d.FailedActionType == (byte)FailedActionType.NoEffect);

                            percSuccess = (double)successful / used;

                            AppendText(string.Format("{0,7:d}{1,15:d}{2,14:d}{3,15:p2}\n",
                                used, successful, noEffect, percSuccess));
                        }
                    }
                }

                AppendText("\n");
            }
        }

        private void ProcessMobDebuffs(IEnumerable<DebuffGroup> debuffSet)
        {
            int used;
            int successful;
            int noEffect;
            double percSuccess;
            string debuffName;

            foreach (var mob in debuffSet)
            {
                if ((mob.Debuffs == null) || (mob.Debuffs.Count() == 0))
                    continue;

                if (mob.Debuffs.Sum(d => d.DebuffTargets.Count()) == 0)
                    continue;

                AppendText(string.Format("{0}\n", mob.DebufferName), Color.Blue, true, false);
                AppendText(debuffHeader, Color.Black, true, true);

                foreach (var debuff in mob.Debuffs)
                {
                    debuffName = debuff.DebuffName;

                    foreach (var target in debuff.DebuffTargets)
                    {
                        used = target.DebuffData.Count();

                        if (used > 0)
                        {
                            AppendText(debuffName.PadRight(20));
                            debuffName = "";


                            AppendText(target.TargetName.PadRight(24));

                            successful = target.DebuffData.Count(d =>
                                (d.DefenseType == (byte)DefenseType.None) &&
                                (d.FailedActionType == (byte)FailedActionType.None));

                            noEffect = target.DebuffData.Count(d => d.FailedActionType == (byte)FailedActionType.NoEffect);

                            percSuccess = (double)successful / used;

                            AppendText(string.Format("{0,7:d}{1,15:d}{2,14:d}{3,15:p2}\n",
                                used, successful, noEffect, percSuccess));
                        }
                    }
                }

                AppendText("\n");
            }
        }
        #endregion

        #region Event Handlers
        protected void categoryCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(DatabaseManager.Instance.Database);

            flagNoUpdate = false;
        }

        protected void mobsCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(DatabaseManager.Instance.Database);

            flagNoUpdate = false;
        }
        #endregion
    }
}
