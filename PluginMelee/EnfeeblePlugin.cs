using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using WaywardGamers.KParser;
using System.Diagnostics;

namespace WaywardGamers.KParser.Plugin
{
    public class EnfeeblePlugin : BasePluginControlWithDropdown
    {
        #region IPlugin Overrides
        public override string TabName
        {
            get { return "Debuffs"; }
        }

        public override void Reset()
        {
            richTextBox.Clear();
            richTextBox.WordWrap = false;

            label1.Text = "Category";
            comboBox1.Left = label1.Right + 10;
            comboBox1.Items.Clear();
            comboBox1.Items.Add("Debuff Mobs");
            comboBox1.Items.Add("Debuff Players");
            flagNoUpdate = true;
            comboBox1.SelectedIndex = 0;

            label2.Left = comboBox1.Right + 20;
            label2.Text = "Mob Group";
            comboBox2.Left = label2.Right + 10;
            comboBox2.Width = 150;
            comboBox2.Items.Clear();
            comboBox2.Items.Add("All");
            flagNoUpdate = true;
            comboBox2.SelectedIndex = 0;

            checkBox1.Enabled = false;
            checkBox1.Visible = false;
            checkBox2.Enabled = false;
            checkBox2.Visible = false;
        }

        public override void DatabaseOpened(KPDatabaseDataSet dataSet)
        {
            ResetTextBox();

            UpdateMobList(dataSet);

            flagNoUpdate = true;
            InitComboBox2Selection();

            ProcessData(dataSet);
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
        {
            // Check for new mobs being fought.  If any exist, update the Mob Group dropdown list.
            if (e.DatasetChanges.Battles.Count > 0)
            {
                var mobsFought = from b in e.DatasetChanges.Battles
                                 where ((b.DefaultBattle == false) &&
                                        (b.IsEnemyIDNull() == false) &&
                                        (b.CombatantsRowByEnemyCombatantRelation.CombatantType == (byte)EntityType.Mob))
                                 group b by b.CombatantsRowByEnemyCombatantRelation.CombatantName into bn
                                 select new
                                 {
                                     Name = bn.Key,
                                     XP = from xb in bn
                                          group xb by xb.MinBaseExperience() into xbn
                                          orderby xbn.Key
                                          select new { BaseXP = xbn.Key }
                                 };


                if (mobsFought.Count() > 0)
                {
                    string mobWithXP;

                    foreach (var mob in mobsFought)
                    {
                        if (comboBox2.Items.Contains(mob.Name) == false)
                            AddStringToComboBox2(mob.Name);

                        foreach (var xp in mob.XP)
                        {
                            if (xp.BaseXP > 0)
                            {
                                mobWithXP = string.Format("{0} ({1})", mob.Name, xp.BaseXP);

                                if (comboBox2.Items.Contains(mobWithXP) == false)
                                    AddStringToComboBox2(mobWithXP);

                                // Check for existing entry with higher min base xp
                                mobWithXP = string.Format("{0} ({1})", mob.Name, xp.BaseXP + 1);

                                if (comboBox2.Items.Contains(mobWithXP))
                                    RemoveFromComboBox2(mobWithXP);
                            }
                        }
                    }
                }
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
            ResetComboBox2();

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

            AddArrayToComboBox2(mobXPStrings.ToArray());

        }
        #endregion


        #region Member Variables
        bool flagNoUpdate = false;

        string debuffMobHeader = "Debuff              Used on                 # Times   # Successful   # No Effect   % Successful\n";
        string debuffPlayerHeader = "Debuff              Used on                 # Times   # Successful   # No Effect   % Successful\n";
        #endregion

        #region Processing sections
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            richTextBox.Clear();

            #region Filtering
            string mobFilter;
            string mobName;
            int mobXP;

            GetMobFilter(comboBox2, out mobFilter, out mobName, out mobXP);
            #endregion

            #region LINQ group construction

            IEnumerable<DebuffGroup> debuffSet = null;
            bool processPlayerDebuffs = (comboBox1.SelectedIndex == 0);

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
                                                                     ((mobFilter == "All") ||
                                                                      // else make sure mob name matches and
                                                                      (bt.CombatantsRowByTargetCombatantRelation.CombatantName == mobName &&
                                                                        // either no xp requirement
                                                                       (mobXP == 0 ||
                                                                        // or there's a battle entry and it has the specified XP amount
                                                                        (bt.IsBattleIDNull() == false &&
                                                                         bt.BattlesRow.MinBaseExperience() == mobXP)
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
                                   (mobFilter == "All" ||
                                    // else make sure mob name matches and
                                    c.CombatantName == mobName)
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
                                                (mobXP == 0 ||
                                                 // or there's a battle entry and it has the specified XP amount
                                                 (b.IsBattleIDNull() == false &&
                                                  b.BattlesRow.MinBaseExperience() == mobXP)
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

                AppendBoldText(string.Format("{0}\n", player.DebufferName), Color.Blue);
                AppendBoldUnderText(debuffMobHeader, Color.Black);

                foreach (var debuff in player.Debuffs)
                {
                    debuffName = debuff.DebuffName;

                    foreach (var target in debuff.DebuffTargets)
                    {
                        used = target.DebuffData.Count();

                        if (used > 0)
                        {
                            AppendNormalText(debuffName.PadRight(20));
                            debuffName = "";

                            AppendNormalText(target.TargetName.PadRight(24));

                            successful = target.DebuffData.Count(d =>
                                (d.DefenseType == (byte)DefenseType.None) &&
                                (d.FailedActionType == (byte)FailedActionType.None));

                            noEffect = target.DebuffData.Count(d => d.FailedActionType == (byte)FailedActionType.NoEffect);

                            percSuccess = (double)successful / used;

                            AppendNormalText(string.Format("{0,7:d}{1,15:d}{2,14:d}{3,15:p2}\n",
                                used, successful, noEffect, percSuccess));
                        }
                    }
                }

                AppendNormalText("\n");
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

                AppendBoldText(string.Format("{0}\n", mob.DebufferName), Color.Blue);
                AppendBoldUnderText(debuffPlayerHeader, Color.Black);

                foreach (var debuff in mob.Debuffs)
                {
                    debuffName = debuff.DebuffName;

                    foreach (var target in debuff.DebuffTargets)
                    {
                        used = target.DebuffData.Count();

                        if (used > 0)
                        {
                            AppendNormalText(debuffName.PadRight(20));
                            debuffName = "";


                            AppendNormalText(target.TargetName.PadRight(24));

                            successful = target.DebuffData.Count(d =>
                                (d.DefenseType == (byte)DefenseType.None) &&
                                (d.FailedActionType == (byte)FailedActionType.None));

                            noEffect = target.DebuffData.Count(d => d.FailedActionType == (byte)FailedActionType.NoEffect);

                            percSuccess = (double)successful / used;

                            AppendNormalText(string.Format("{0,7:d}{1,15:d}{2,14:d}{3,15:p2}\n",
                                used, successful, noEffect, percSuccess));
                        }
                    }
                }

                AppendNormalText("\n");
            }
        }
        #endregion

        #region Event Handlers
        protected override void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(DatabaseManager.Instance.Database);

            flagNoUpdate = false;
        }

        protected override void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(DatabaseManager.Instance.Database);

            flagNoUpdate = false;
        }
        #endregion
    }
}
