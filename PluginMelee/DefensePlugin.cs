using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;

namespace WaywardGamers.KParser.Plugin
{
    class DefensePlugin : BasePluginControlWithDropdown
    {
        #region IPlugin Overrides
        public override string TabName
        {
            get { return "Defense"; }
        }

        public override void Reset()
        {
            richTextBox.Clear();

            label1.Text = "Attack Type";
            comboBox1.Left = label1.Right + 10;
            comboBox1.Items.Clear();
            comboBox1.Items.Add("All");
            comboBox1.Items.Add("Recovery");
            comboBox1.Items.Add("Defense");
            comboBox1.Items.Add("Utsusemi");
            comboBox1.SelectedIndex = 0;

            label2.Left = comboBox1.Right + 20;
            label2.Text = "Mob Group";
            comboBox2.Left = label2.Right + 10;
            comboBox2.Items.Clear();
            comboBox2.Items.Add("All");
            comboBox2.SelectedIndex = 0;

            checkBox1.Left = comboBox2.Right + 20;
            checkBox1.Text = "Exclude 0 XP Mobs";
            checkBox1.Checked = false;

            //checkBox2.Left = checkBox1.Right + 10;
            //checkBox2.Text = "Exclude 0 Dmg Mobs";
            //checkBox2.Checked = false;
            checkBox2.Enabled = false;
            checkBox2.Visible = false;
        }

        public override void DatabaseOpened(KPDatabaseDataSet dataSet)
        {
            int allBattles = dataSet.Battles.Count(b => b.DefaultBattle == false);

            if (allBattles > 0)
            {
                var mobsKilled = from b in dataSet.Battles
                                 where b.DefaultBattle == false
                                 orderby b.CombatantsRowByEnemyCombatantRelation.CombatantName
                                 group b by b.CombatantsRowByEnemyCombatantRelation.CombatantName into bn
                                 select new
                                 {
                                     Name = bn.Key,
                                     XP = from xb in bn
                                          group xb by xb.BaseExperience() into xbn
                                          select new { BXP = xbn.Key }
                                 };

                foreach (var mob in mobsKilled)
                {
                    if (this.comboBox2.Items.Contains(mob.Name) == false)
                    {
                        AddToComboBox2(mob.Name);
                    }

                    if (mob.XP.Count() > 1)
                    {
                        string mobWithXP;

                        foreach (var xp in mob.XP)
                        {
                            mobWithXP = string.Format("{0} ({1})", mob.Name, xp.BXP);

                            if (this.comboBox2.Items.Contains(mobWithXP) == false)
                            {
                                AddToComboBox2(mobWithXP);
                            }
                        }
                    }
                }
            }

            base.DatabaseOpened(dataSet);
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
        {
            if (e.DatasetChanges.Battles.Count != 0)
            {
                // Check for new kills.  If any exist, update the Mob Group dropdown list.
                int allBattles = e.DatasetChanges.Battles.Count(b => b.DefaultBattle == false);

                if (allBattles != 0)
                {
                    var mobsKilled = from b in e.FullDataset.Battles
                                     where b.Killed == true
                                     orderby b.CombatantsRowByEnemyCombatantRelation.CombatantName
                                     group b by b.CombatantsRowByEnemyCombatantRelation.CombatantName into bn
                                     select new
                                     {
                                         Name = bn.Key,
                                         XP = from xb in bn
                                              group xb by xb.BaseExperience() into xbn
                                              select new { BXP = xbn.Key }
                                     };

                    foreach (var mob in mobsKilled)
                    {
                        if (this.comboBox2.Items.Contains(mob.Name) == false)
                        {
                            AddToComboBox2(mob.Name);
                        }

                        if (mob.XP.Count() > 1)
                        {
                            string mobWithXP;

                            foreach (var xp in mob.XP)
                            {
                                mobWithXP = string.Format("{0} ({1})", mob.Name, xp.BXP);

                                if (this.comboBox2.Items.Contains(mobWithXP) == false)
                                {
                                    AddToComboBox2(mobWithXP);
                                }
                            }
                        }
                    }
                }
            }

            if (e.DatasetChanges.CombatDetails.Count != 0)
            {
                datasetToUse = e.FullDataset;
                return true;
            }

            datasetToUse = null;
            return false;
        }

        private void AddToComboBox2(string p)
        {
            if (this.InvokeRequired)
            {
                Action<string> thisFunc = AddToComboBox2;
                Invoke(thisFunc, new object[] { p });
                return;
            }

            this.comboBox2.Items.Add(p);
        }
        #endregion

        #region Processing sections
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            richTextBox.Clear();

            string mobFilter = comboBox2.SelectedItem.ToString();
            IEnumerable<AttackGroup> allAttacks;

            int minXP = 0;
            if (checkBox1.Checked == true)
                minXP = 1;

            if (mobFilter == "All")
            {
                allAttacks = from cd in dataSet.CombatDetails
                             where ((cd.IsActorIDNull() == false) &&
                                    (cd.AttackType == (byte)AttackType.Damage) &&
                                    ((cd.CombatantsRowByCombatActorRelation.CombatantType == (byte)EntityType.Player) ||
                                     (cd.CombatantsRowByCombatActorRelation.CombatantType == (byte)EntityType.Pet) ||
                                     (cd.CombatantsRowByCombatActorRelation.CombatantType == (byte)EntityType.Fellow) ||
                                     (cd.CombatantsRowByCombatActorRelation.CombatantType == (byte)EntityType.Skillchain))
                                    ) &&
                                    ((cd.BattlesRow.ExperiencePoints >= minXP) || (cd.BattlesRow.Killed == false))
                             group cd by cd.ActionSource into cda
                             select new AttackGroup(
                                 (ActionSourceType)cda.Key,
                                  from c in cda
                                  orderby c.CombatantsRowByCombatActorRelation.CombatantName
                                  group c by c.CombatantsRowByCombatActorRelation);

            }
            else
            {
                #region subset
                Regex mobAndXP = new Regex(@"(?<mobName>\w+(['\- ](\d|\w)+)*)( \((?<xp>\d+)\))?");
                Match mobAndXPMatch = mobAndXP.Match(mobFilter);

                if (mobAndXPMatch.Success == true)
                {
                    if (mobAndXPMatch.Captures.Count == 1)
                    {
                        // Name only
                        allAttacks = from cd in dataSet.CombatDetails
                                     where ((cd.IsActorIDNull() == false) &&
                                            (cd.AttackType == (byte)AttackType.Damage) &&
                                            ((cd.CombatantsRowByCombatActorRelation.CombatantType == (byte)EntityType.Player) ||
                                             (cd.CombatantsRowByCombatActorRelation.CombatantType == (byte)EntityType.Pet) ||
                                             (cd.CombatantsRowByCombatActorRelation.CombatantType == (byte)EntityType.Fellow) ||
                                             (cd.CombatantsRowByCombatActorRelation.CombatantType == (byte)EntityType.Skillchain))
                                            ) &&
                                            (cd.BattlesRow.CombatantsRowByEnemyCombatantRelation.CombatantName == mobAndXPMatch.Groups["mobName"].Value) &&
                                            ((cd.BattlesRow.ExperiencePoints >= minXP) || (cd.BattlesRow.Killed == false))
                                     group cd by cd.ActionSource into cda
                                     select new AttackGroup(
                                         (ActionSourceType)cda.Key,
                                          from c in cda
                                          orderby c.CombatantsRowByCombatActorRelation.CombatantName
                                          group c by c.CombatantsRowByCombatActorRelation);
                    }
                    else if (mobAndXPMatch.Captures.Count == 2)
                    {
                        // Name and XP
                        int xp = int.Parse(mobAndXPMatch.Groups["xp"].Value);

                        allAttacks = from cd in dataSet.CombatDetails
                                     where ((cd.IsActorIDNull() == false) &&
                                            (cd.AttackType == (byte)AttackType.Damage) &&
                                            ((cd.CombatantsRowByCombatActorRelation.CombatantType == (byte)EntityType.Player) ||
                                             (cd.CombatantsRowByCombatActorRelation.CombatantType == (byte)EntityType.Pet) ||
                                             (cd.CombatantsRowByCombatActorRelation.CombatantType == (byte)EntityType.Fellow) ||
                                             (cd.CombatantsRowByCombatActorRelation.CombatantType == (byte)EntityType.Skillchain))
                                            ) &&
                                            (cd.BattlesRow.CombatantsRowByEnemyCombatantRelation.CombatantName == mobAndXPMatch.Groups["mobName"].Value) &&
                                            (cd.BattlesRow.BaseExperience() == xp) &&
                                            ((cd.BattlesRow.ExperiencePoints >= minXP) || (cd.BattlesRow.Killed == false))
                                     group cd by cd.ActionSource into cda
                                     select new AttackGroup(
                                         (ActionSourceType)cda.Key,
                                          from c in cda
                                          orderby c.CombatantsRowByCombatActorRelation.CombatantName
                                          group c by c.CombatantsRowByCombatActorRelation);
                    }
                    else
                    {
                        Logger.Instance.Log("DefensePlugin", "Failed in mob filtering.  Invalid number of captures.");
                        return;
                    }
                }
                else
                {
                    Logger.Instance.Log("DefensePlugin", "Failed in mob filtering.  Match failed.");
                    return;
                }
                #endregion
            }

            return;

            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    // all
                    ProcessRecovery();
                    ProcessDefense();
                    ProcessUtsusemi();
                    break;
                case 1:
                    // Recovery
                    ProcessRecovery();
                    break;
                case 2:
                    // Defense
                    ProcessDefense();
                    break;
                case 3:
                    // Utsusemi
                    ProcessUtsusemi();
                    break;
            }
        }

        private void ProcessRecovery()
        {
            throw new NotImplementedException();
        }

        private void ProcessDefense()
        {
            throw new NotImplementedException();
        }

        private void ProcessUtsusemi()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Event Handlers
        protected override void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            HandleDataset(DatabaseManager.Instance.Database);
        }

        protected override void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            HandleDataset(DatabaseManager.Instance.Database);
        }

        protected override void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            HandleDataset(DatabaseManager.Instance.Database);
        }

        protected override void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            HandleDataset(DatabaseManager.Instance.Database);
        }
        #endregion

    }
}