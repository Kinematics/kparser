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
    public class DebuffPlugin : BasePluginControl
    {
        #region Member Variables
        bool flagNoUpdate = false;
        bool groupMobs = true;
        bool exclude0XPMobs = false;
        bool customMobSelection = false;

        ToolStripLabel catLabel = new ToolStripLabel();
        ToolStripComboBox categoryCombo = new ToolStripComboBox();
        ToolStripLabel mobLabel = new ToolStripLabel();
        ToolStripComboBox mobsCombo = new ToolStripComboBox();

        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();
        ToolStripMenuItem groupMobsOption = new ToolStripMenuItem();
        ToolStripMenuItem exclude0XPOption = new ToolStripMenuItem();
        ToolStripMenuItem customMobSelectionOption = new ToolStripMenuItem();

        ToolStripButton editCustomMobFilter = new ToolStripButton();

        string lsDebuffHeader;
        string lsDebuffHeaderWithTargets;
        string lsPlayerDebuffFormat;
        string lsMobDebuffFormat;
        #endregion

        #region Constructor
        public DebuffPlugin()
        {
            LoadLocalizedUI();

            categoryCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            categoryCombo.SelectedIndex = 0;
            categoryCombo.SelectedIndexChanged += new EventHandler(this.categoryCombo_SelectedIndexChanged);

            mobsCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            mobsCombo.MaxDropDownItems = 10;
            mobsCombo.AutoSize = false;
            mobsCombo.Width = 175;
            mobsCombo.SelectedIndex = 0;
            mobsCombo.SelectedIndexChanged += new EventHandler(this.mobsCombo_SelectedIndexChanged);

            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;

            groupMobsOption.CheckOnClick = true;
            groupMobsOption.Checked = true;
            groupMobsOption.Click += new EventHandler(groupMobs_Click);
            optionsMenu.DropDownItems.Add(groupMobsOption);

            exclude0XPOption.CheckOnClick = true;
            exclude0XPOption.Checked = false;
            exclude0XPOption.Click += new EventHandler(exclude0XPMobs_Click);
            optionsMenu.DropDownItems.Add(exclude0XPOption);

            customMobSelectionOption.CheckOnClick = true;
            customMobSelectionOption.Checked = false;
            customMobSelectionOption.Click += new EventHandler(customMobSelection_Click);
            optionsMenu.DropDownItems.Add(customMobSelectionOption);

            editCustomMobFilter.Enabled = false;
            editCustomMobFilter.Click += new EventHandler(editCustomMobFilter_Click);

            ToolStripSeparator aSeparator = new ToolStripSeparator();

            toolStrip.Items.Add(catLabel);
            toolStrip.Items.Add(categoryCombo);
            toolStrip.Items.Add(mobLabel);
            toolStrip.Items.Add(mobsCombo);
            toolStrip.Items.Add(optionsMenu);
            toolStrip.Items.Add(aSeparator);
            toolStrip.Items.Add(editCustomMobFilter);

        }
        #endregion

        #region IPlugin Overrides
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
                                DebufferName = c.CombatantNameOrJobName,
                                Debuffs = from b in c.GetInteractionsRowsByActorCombatantRelation()
                                          where (((HarmType)b.HarmType == HarmType.Enfeeble ||
                                                  (HarmType)b.HarmType == HarmType.Dispel ||
                                                  (HarmType)b.HarmType == HarmType.Unknown) &&
                                                  b.Preparing == false && b.IsActionIDNull() == false)
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
                                                   b.Preparing == false && 
                                                   b.IsActionIDNull() == false)
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
                                                                  TargetName = (btn.Count() > 0) ? btn.First().CombatantsRowByTargetCombatantRelation.CombatantNameOrJobName : btn.Key,
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
            int usedCount;
            int successfulCount;
            int noEffectCount;
            double percSuccess;
            string debuffName;

            foreach (var player in debuffSet)
            {
                if ((player.Debuffs == null) || (player.Debuffs.Count() == 0))
                    continue;

                if (player.Debuffs.Sum(d => d.DebuffTargets.Count()) == 0)
                    continue;

                AppendText(string.Format("{0}\n", player.DebufferName), Color.Blue, true, false);
                AppendText(lsDebuffHeader, Color.Black, true, true);
                AppendText("\n");

                foreach (var debuff in player.Debuffs)
                {
                    debuffName = debuff.DebuffName;

                    usedCount = 0;
                    successfulCount = 0;
                    noEffectCount = 0;

                    foreach (var target in debuff.DebuffTargets)
                    {
                        used = target.DebuffData.Count();
                        usedCount += used;

                        if (used > 0)
                        {
                            successfulCount += target.DebuffData.Count(d =>
                                (((HarmType)d.HarmType == HarmType.Dispel ||
                                 (HarmType)d.HarmType == HarmType.Enfeeble ||
                                 (HarmType)d.HarmType == HarmType.Unknown) &&
                                 ((DefenseType)d.DefenseType == DefenseType.None &&
                                 (FailedActionType)d.FailedActionType == FailedActionType.None)) ||
                                ((HarmType)d.SecondHarmType == HarmType.Dispel ||
                                 (HarmType)d.SecondHarmType == HarmType.Enfeeble));

                            noEffectCount += target.DebuffData.Count(d => d.FailedActionType == (byte)FailedActionType.NoEffect);
                        }
                    }

                    if (usedCount > 0)
                    {
                        AppendText(debuffName.PadRight(20));

                        percSuccess = (double)successfulCount / usedCount;

                        AppendText(string.Format(lsPlayerDebuffFormat,
                            usedCount, successfulCount, noEffectCount, percSuccess));
                        AppendText("\n");
                    }
                }

                AppendText("\n");
            }
        }

        private void ProcessMobDebuffs(IEnumerable<DebuffGroup> debuffSet)
        {
            int used;
            int usedCount;
            int successfulCount;
            int noEffectCount;
            double percSuccess;
            string debuffName;

            foreach (var mob in debuffSet)
            {
                if ((mob.Debuffs == null) || (mob.Debuffs.Count() == 0))
                    continue;

                if (mob.Debuffs.Sum(d => d.DebuffTargets.Count()) == 0)
                    continue;

                AppendText(string.Format("{0}\n", mob.DebufferName), Color.Blue, true, false);
                AppendText(lsDebuffHeaderWithTargets, Color.Black, true, true);
                AppendText("\n");

                foreach (var debuff in mob.Debuffs)
                {
                    debuffName = debuff.DebuffName;

                    usedCount = 0;
                    successfulCount = 0;
                    noEffectCount = 0;

                    foreach (var target in debuff.DebuffTargets)
                    {
                        used = target.DebuffData.Count();
                        usedCount += used;

                        if (used > 0)
                        {
                            AppendText(debuffName.PadRight(20));
                            debuffName = "";

                            successfulCount += target.DebuffData.Count(d =>
                                (((HarmType)d.HarmType == HarmType.Dispel ||
                                 (HarmType)d.HarmType == HarmType.Enfeeble ||
                                 (HarmType)d.HarmType == HarmType.Unknown) &&
                                 ((DefenseType)d.DefenseType == DefenseType.None &&
                                 (FailedActionType)d.FailedActionType == FailedActionType.None)) ||
                                ((HarmType)d.SecondHarmType == HarmType.Dispel ||
                                 (HarmType)d.SecondHarmType == HarmType.Enfeeble));

                            //successfulCount += target.DebuffData.Count(d =>
                            //    (((HarmType)d.HarmType == HarmType.Dispel ||
                            //     (HarmType)d.HarmType == HarmType.Enfeeble ||
                            //     (HarmType)d.HarmType == HarmType.Unknown) &&
                            //     ((DefenseType)d.DefenseType == DefenseType.None &&
                            //     (FailedActionType)d.FailedActionType == FailedActionType.None) &&
                            //     d.Preparing == true) ||
                            //    ((HarmType)d.SecondHarmType == HarmType.Dispel ||
                            //     (HarmType)d.SecondHarmType == HarmType.Enfeeble));

                            noEffectCount += target.DebuffData.Count(d => d.FailedActionType == (byte)FailedActionType.NoEffect);

                            AppendText(target.TargetName.PadRight(20));

                            percSuccess = (double)successfulCount / usedCount;

                            AppendText(string.Format(lsMobDebuffFormat,
                                usedCount, successfulCount, noEffectCount, percSuccess));
                            AppendText("\n");

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

        #region Localization Overrides
        protected override void LoadLocalizedUI()
        {
            catLabel.Text = Resources.PublicResources.CategoryLabel;
            mobLabel.Text = Resources.PublicResources.MobsLabel;
            optionsMenu.Text = Resources.PublicResources.Options;
            groupMobsOption.Text = Resources.PublicResources.GroupMobs;
            exclude0XPOption.Text = Resources.PublicResources.Exclude0XPMobs;
            customMobSelectionOption.Text = Resources.PublicResources.CustomMobSelection;
            editCustomMobFilter.Text = Resources.PublicResources.EditMobFilter;

            categoryCombo.Items.Clear();
            categoryCombo.Items.Add(Resources.Combat.DebuffPluginDebuffMobsCategory);
            categoryCombo.Items.Add(Resources.Combat.DebuffPluginDebuffPlayersCategory);

            UpdateMobList();
        }

        protected override void LoadResources()
        {
            this.tabName = Resources.Combat.DebuffPluginTabName;

            lsDebuffHeader = Resources.Combat.DebuffPluginDebuffHeader;
            lsDebuffHeaderWithTargets = Resources.Combat.DebuffPluginDebuffWithTargetsHeader;
            lsPlayerDebuffFormat = Resources.Combat.DebuffPluginPlayerDebuffFormat;
            lsMobDebuffFormat = Resources.Combat.DebuffPluginMobDebuffFormat;
        }
        #endregion

    }
}
