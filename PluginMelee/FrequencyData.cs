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
    public class FrequencyDataPlugin : BasePluginControlWithDropdown
    {
        #region IPlugin Overrides
        public override string TabName
        {
            get { return "Detail Data"; }
        }

        public override void Reset()
        {
            richTextBox.Clear();
            richTextBox.WordWrap = false;

            label1.Text = "Players";
            comboBox1.Left = label1.Right + 10;
            comboBox1.MaxDropDownItems = 9;
            comboBox1.Items.Clear();

            label2.Left = comboBox1.Right + 20;
            label2.Text = "Enemies";
            comboBox2.Left = label2.Right + 10;
            comboBox2.Width = 150;
            comboBox2.Items.Clear();
            comboBox2.Items.Add("All");
            comboBox2.SelectedIndex = 0;

            checkBox1.Left = comboBox2.Right + 20;
            checkBox1.Text = "Group Enemies";
            checkBox1.Checked = false;

            checkBox2.Left = checkBox1.Right + 10;
            checkBox2.Text = "Show Detail";
            checkBox2.Checked = false;
        }

        public override void DatabaseOpened(KPDatabaseDataSet dataSet)
        {
            ResetComboBox2();
            AddToComboBox2("All");
            ResetTextBox();

            if (dataSet.Battles.Count() > 1)
            {
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

                if (mobsKilled.Count() > 0)
                {
                    // Add to the Reset list

                    string mobWithXP;

                    foreach (var mob in mobsKilled)
                    {
                        AddToComboBox2(mob.Name);

                        if (mob.XP.Count() > 1)
                        {
                            foreach (var xp in mob.XP)
                            {
                                mobWithXP = string.Format("{0} ({1})", mob.Name, xp.BaseXP);

                                AddToComboBox2(mobWithXP);
                            }
                        }
                    }
                }
            }

            InitComboBox2Selection();
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
        {
            // Check for new mobs being fought.  If any exist, update the Mob Group dropdown list.
            if (e.DatasetChanges.Battles != null)
            {
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
                                AddToComboBox2(mob.Name);

                            foreach (var xp in mob.XP)
                            {
                                if (xp.BaseXP > 0)
                                {
                                    mobWithXP = string.Format("{0} ({1})", mob.Name, xp.BaseXP);

                                    if (comboBox2.Items.Contains(mobWithXP) == false)
                                        AddToComboBox2(mobWithXP);

                                    // Check for existing entry with higher min base xp
                                    mobWithXP = string.Format("{0} ({1})", mob.Name, xp.BaseXP + 1);

                                    if (comboBox2.Items.Contains(mobWithXP))
                                        RemoveFromComboBox2(mobWithXP);
                                }
                            }
                        }
                    }
                }
            }

            if (e.DatasetChanges.Interactions.Count != 0)
            {
                datasetToUse = e.Dataset;
                return true;
            }

            datasetToUse = null;
            return false;
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
