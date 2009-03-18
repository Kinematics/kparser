using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WaywardGamers.KParser;
using WaywardGamers.KParser.Interface;
using WaywardGamers.KParser.Database;

namespace WaywardGamers.KParser.Plugin
{
    public class EnfeeblePlugin : BasePluginControl
    {
        #region Member Variables
        string debuffHeader = "Debuff              Used on                 # Times   # Successful   # No Effect   % Successful\n";

        bool flagNoUpdate = false;
        bool groupMobs = true;
        bool exclude0XPMobs = false;
        bool customMobSelection = false;

        ToolStripComboBox categoryCombo = new ToolStripComboBox();
        ToolStripComboBox mobsCombo = new ToolStripComboBox();

        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();
        ToolStripMenuItem groupMobsOption = new ToolStripMenuItem();
        ToolStripMenuItem exclude0XPOption = new ToolStripMenuItem();
        ToolStripMenuItem customMobSelectionOption = new ToolStripMenuItem();

        ToolStripButton editCustomMobFilter = new ToolStripButton();
        #endregion

        #region Constructor
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

            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;
            optionsMenu.Text = "Options";

            groupMobsOption.Text = "Group Mobs";
            groupMobsOption.CheckOnClick = true;
            groupMobsOption.Checked = true;
            groupMobsOption.Click += new EventHandler(groupMobs_Click);
            optionsMenu.DropDownItems.Add(groupMobsOption);

            exclude0XPOption.Text = "Exclude 0 XP Mobs";
            exclude0XPOption.CheckOnClick = true;
            exclude0XPOption.Checked = false;
            exclude0XPOption.Click += new EventHandler(exclude0XPMobs_Click);
            optionsMenu.DropDownItems.Add(exclude0XPOption);

            customMobSelectionOption.Text = "Custom Mob Selection";
            customMobSelectionOption.CheckOnClick = true;
            customMobSelectionOption.Checked = false;
            customMobSelectionOption.Click += new EventHandler(customMobSelection_Click);
            optionsMenu.DropDownItems.Add(customMobSelectionOption);

            toolStrip.Items.Add(optionsMenu);

            ToolStripSeparator aSeparator = new ToolStripSeparator();
            toolStrip.Items.Add(aSeparator);

            editCustomMobFilter.Text = "Edit Mob Filter";
            editCustomMobFilter.Enabled = false;
            editCustomMobFilter.Click += new EventHandler(editCustomMobFilter_Click);

            toolStrip.Items.Add(editCustomMobFilter);

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

        public override void NotifyOfUpdate()
        {
            ResetTextBox();

            UpdateMobList();

            flagNoUpdate = true;
            mobsCombo.CBSelectIndex(0);

            HandleDataset(null);
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            // Check for new mobs being fought.  If any exist, update the Mob Group dropdown list.
            if (e.DatasetChanges.Battles.Count > 0)
            {
                string mobSelected = mobsCombo.CBSelectedItem();
                flagNoUpdate = true;
                UpdateMobList();

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
                    HandleDataset(null);
                }
            }
        }
        #endregion

        #region Private functions
        private void UpdateMobList()
        {
            mobsCombo.UpdateWithMobList(groupMobs, exclude0XPMobs);
        }
        #endregion

        #region Processing sections
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            if (dataSet == null)
                return;

            ResetTextBox();

            MobFilter mobFilter;
            if (customMobSelection)
                mobFilter = MobXPHandler.Instance.CustomMobFilter;
            else
                mobFilter = mobsCombo.CBGetMobFilter();


            #region LINQ group construction

            IEnumerable<DebuffGroup> debuffSet = null;
            bool processPlayerDebuffs = (categoryCombo.CBSelectedIndex() == 0);

            if (processPlayerDebuffs == true)
            {
                // Process debuffs used by players

                debuffSet = from c in dataSet.Combatants
                            where (((EntityType)c.CombatantType == EntityType.Player) ||
                                  ((EntityType)c.CombatantType == EntityType.Pet) ||
                                  ((EntityType)c.CombatantType == EntityType.CharmedMob) ||
                                  ((EntityType)c.CombatantType == EntityType.Fellow))
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
                                                                     mobFilter.CheckFilterMobTarget(bt))
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
                            where c.CombatantType == (byte)EntityType.Mob
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
                                                 mobFilter.CheckFilterMobActor(b)
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
                HandleDataset(null);

            flagNoUpdate = false;
        }

        protected void mobsCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(null);

            flagNoUpdate = false;
        }

        protected void groupMobs_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            groupMobs = sentBy.Checked;

            if (flagNoUpdate == false)
            {
                flagNoUpdate = true;
                UpdateMobList();

                HandleDataset(null);
            }

            flagNoUpdate = false;
        }

        protected void exclude0XPMobs_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            exclude0XPMobs = sentBy.Checked;

            if (flagNoUpdate == false)
            {
                flagNoUpdate = true;
                UpdateMobList();

                HandleDataset(null);
            }

            flagNoUpdate = false;
        }

        protected void customMobSelection_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            customMobSelection = sentBy.Checked;

            mobsCombo.Enabled = !customMobSelection;
            groupMobsOption.Enabled = !customMobSelection;
            exclude0XPOption.Enabled = !customMobSelection;

            editCustomMobFilter.Enabled = customMobSelection;

            if (flagNoUpdate == false)
            {
                HandleDataset(null);
            }

            flagNoUpdate = false;
        }

        protected void editCustomMobFilter_Click(object sender, EventArgs e)
        {
            MobXPHandler.Instance.ShowCustomMobFilter();
        }

        protected override void OnCustomMobFilterChanged()
        {
            HandleDataset(null);
        }

        #endregion
    }
}
