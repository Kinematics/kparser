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
            get { return "Enfeebling"; }
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

        #region Processing Functions
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
                                          where (((HarmType)b.HarmType == HarmType.Enfeeble ||
                                                  (HarmType)b.HarmType == HarmType.Dispel ||
                                                  (HarmType)b.HarmType == HarmType.Unknown) &&
                                                  b.Preparing == false && b.IsActionIDNull() == false)
                                                 ||
                                                 (b.Preparing == false &&
                                                  b.IsActionIDNull() == false &&
                                                  (b.ActionsRow.ActionName.StartsWith("Dia") ||
                                                    b.ActionsRow.ActionName.StartsWith("Bio")))
                                                 ||
                                                 (b.Preparing == false && b.IsActionIDNull() == false &&
                                                  b.ActionsRow.GetInteractionsRows()
                                                   .Any(q => (HarmType)q.SecondHarmType == HarmType.Enfeeble ||
                                                             (HarmType)q.SecondHarmType == HarmType.Dispel))
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
                                          where ((((HarmType)b.HarmType == HarmType.Enfeeble ||
                                                   (HarmType)b.HarmType == HarmType.Dispel ||
                                                   (HarmType)b.HarmType == HarmType.Unknown) &&
                                                   b.Preparing == false && b.IsActionIDNull() == false)
                                                 ||
                                                 (b.Preparing == false &&
                                                  b.IsActionIDNull() == false &&
                                                  (b.ActionsRow.ActionName.StartsWith("Dia") ||
                                                   b.ActionsRow.ActionName.StartsWith("Bio")))
                                                 || 
                                                 (b.Preparing == false && b.IsActionIDNull() == false &&
                                                  b.ActionsRow.GetInteractionsRows()
                                                   .Any(q => (HarmType)q.SecondHarmType == HarmType.Enfeeble ||
                                                             (HarmType)q.SecondHarmType == HarmType.Dispel))
                                                 )
                                                 &&
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



            ProcessDurations(debuffSet);

        }

        private void ProcessDurations(IEnumerable<DebuffGroup> debuffSet)
        {
            int used;
            int count;
            string debuffName;
            TimeSpan totalRemainingFight, avgRemainingFight;
            string totalDurationString, avgDurationString;

            foreach (var player in debuffSet)
            {
                if ((player.Debuffs == null) || (player.Debuffs.Count() == 0))
                    continue;

                if (player.Debuffs.Sum(d => d.DebuffTargets.Count()) == 0)
                    continue;

                AppendText(string.Format("{0}\n", player.DebufferName), Color.Blue, true, false);
                AppendText("Debuff               #Successful     Total Duration     Avg Duration\n", Color.Black, true, true);

                foreach (var debuff in player.Debuffs)
                {
                    debuffName = debuff.DebuffName;

                    totalRemainingFight = TimeSpan.FromMilliseconds(0);
                    avgRemainingFight = TimeSpan.FromMilliseconds(0);
                    count = 0;

                    foreach (var target in debuff.DebuffTargets)
                    {
                        used = target.DebuffData.Count();

                        if (used > 0)
                        {
                            var successDebuff = target.DebuffData.Where(d =>
                                (((HarmType)d.HarmType == HarmType.Dispel ||
                                 (HarmType)d.HarmType == HarmType.Enfeeble ||
                                 (HarmType)d.HarmType == HarmType.Unknown ||
                                 (d.IsActionIDNull() == false && (d.ActionsRow.ActionName.StartsWith("Dia") ||
                                 d.ActionsRow.ActionName.StartsWith("Bio")))) &&
                                 ((DefenseType)d.DefenseType == DefenseType.None &&
                                 (FailedActionType)d.FailedActionType == FailedActionType.None)) ||
                                ((HarmType)d.SecondHarmType == HarmType.Dispel ||
                                 (HarmType)d.SecondHarmType == HarmType.Enfeeble));


                            foreach (var sDebuff in successDebuff)
                            {
                                if (sDebuff.IsBattleIDNull() == false)
                                {
                                    if (sDebuff.BattlesRow.EndTime > sDebuff.Timestamp)
                                    {
                                        count++;
                                        totalRemainingFight += sDebuff.BattlesRow.EndTime - sDebuff.Timestamp;
                                    }
                                }
                            }
                        }
                    }

                    if (count > 0)
                    {
                        AppendText(debuffName.PadRight(20));

                        avgRemainingFight = TimeSpan.FromMilliseconds(
                            totalRemainingFight.TotalMilliseconds / count);

                        totalDurationString = string.Format("{0:d}:{1,2:d2}:{2,2:d2}",
                            (int)totalRemainingFight.TotalHours, totalRemainingFight.Minutes,
                            totalRemainingFight.Seconds);

                        avgDurationString = string.Format("{0:d}:{1,2:d2}",
                            (int)avgRemainingFight.TotalMinutes, avgRemainingFight.Seconds);

                        AppendText(string.Format("{0,12:d}{1,19}{2,17}\n",
                            count, totalDurationString, avgDurationString));
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
