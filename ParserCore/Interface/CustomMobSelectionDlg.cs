using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WaywardGamers.KParser.Database;
using WaywardGamers.KParser.Plugin;

namespace WaywardGamers.KParser.Plugin
{
    public partial class CustomMobSelectionDlg : Form
    {
        #region Constructor
        //static CustomMobSelectionDlg singleReference = null;

        public CustomMobSelectionDlg()
        {
            InitializeComponent();
        }
        #endregion

        #region Event Handlers

        // Window events

        private void CustomMobSelectionDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        // List box event handlers

        private void mobList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            MobFilter mobFilter = MobXPHandler.Instance.CustomMobFilter;

            if (e.NewValue == CheckState.Checked)
            {
                mobFilter.CustomBattleIDs.Add(MobXPHandler.Instance.CompleteMobList.ElementAt(e.Index).BattleID);
            }
            else
            {
                mobFilter.CustomBattleIDs.Remove(MobXPHandler.Instance.CompleteMobList.ElementAt(e.Index).BattleID);
            }

            NotifyMobXPHandlerOfChangedMobList();
        }

        // Button event handlers

        private void selectAllButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < mobList.Items.Count; i++)
            {
                mobList.SetItemChecked(i, true);
            }

            UpdateFilter();
        }

        private void invertSelectionButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < mobList.Items.Count; i++)
            {
                mobList.SetItemChecked(i, !mobList.GetItemChecked(i));
            }

            UpdateFilter();
        }

        private void selectNoneButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < mobList.Items.Count; i++)
            {
                mobList.SetItemChecked(i, false);
            }

            UpdateFilter();
        }

        private void uncheck0XPMobs_Click(object sender, EventArgs e)
        {
            var mobXPVals = MobXPHandler.Instance.CompleteMobList;

            for (int i = 0; i < mobList.Items.Count; i++)
            {
                if (mobXPVals.ElementAt(i).BaseXP == 0)
                    mobList.SetItemChecked(i, false);
            }

            UpdateFilter();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        // Context menu event handlers

        private void mobListContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            if ((mobList.Items.Count == 0) || (mobList.SelectedIndex < 0))
            {
                checkAllMobsBelowCurrentSelectionToolStripMenuItem.Enabled = false;
                uncheckAllMobsBelowCurrentSelectionToolStripMenuItem.Enabled = false;
                checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Enabled = false;
                uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Enabled = false;
                checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Enabled = false;
                uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Enabled = false;
            }
            else
            {
                checkAllMobsBelowCurrentSelectionToolStripMenuItem.Enabled = true;
                uncheckAllMobsBelowCurrentSelectionToolStripMenuItem.Enabled = true;
                checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Enabled = true;
                uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Enabled = true;
                checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Enabled = true;
                uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Enabled = true;
            }
        }

        private void checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MobXPValues mob = GetSelectedMob();

            if (mob != null)
            {
                var mobXPVals = MobXPHandler.Instance.CompleteMobList;

                for (int i = 0; i < mobList.Items.Count; i++)
                {
                    if (mobXPVals.ElementAt(i).Name == mob.Name)
                        mobList.SetItemChecked(i, true);
                }

                UpdateFilter();
            }
        }

        private void uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MobXPValues mob = GetSelectedMob();

            if (mob != null)
            {
                var mobXPVals = MobXPHandler.Instance.CompleteMobList;

                for (int i = 0; i < mobList.Items.Count; i++)
                {
                    if (mobXPVals.ElementAt(i).Name == mob.Name)
                        mobList.SetItemChecked(i, false);
                }

                UpdateFilter();
            }
        }

        private void checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MobXPValues selectedMob = GetSelectedMob();

            if (selectedMob != null)
            {
                var mobXPVals = MobXPHandler.Instance.CompleteMobList;

                for (int i = 0; i < mobList.Items.Count; i++)
                {
                    MobXPValues mob = mobXPVals.ElementAt(i);

                    if ((mob.Name == selectedMob.Name) && (mob.BaseXP == selectedMob.BaseXP))
                        mobList.SetItemChecked(i, true);
                }

                UpdateFilter();
            }
        }

        private void uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MobXPValues selectedMob = GetSelectedMob();

            if (selectedMob != null)
            {
                var mobXPVals = MobXPHandler.Instance.CompleteMobList;

                for (int i = 0; i < mobList.Items.Count; i++)
                {
                    MobXPValues mob = mobXPVals.ElementAt(i);

                    if ((mob.Name == selectedMob.Name) && (mob.BaseXP == selectedMob.BaseXP))
                        mobList.SetItemChecked(i, false);
                }

                UpdateFilter();
            }
        }

        private void checkAllMobsBelowCurrentSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int currentSelectedIndex = mobList.SelectedIndex;

            if (currentSelectedIndex >= 0)
            {
                for (int i = currentSelectedIndex+1; i < mobList.Items.Count; i++)
                {
                    mobList.SetItemChecked(i, true);
                }

                UpdateFilter();
            }
        }

        private void uncheckAllMobsBelowCurrentSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int currentSelectedIndex = mobList.SelectedIndex;

            if (currentSelectedIndex >= 0)
            {
                for (int i = currentSelectedIndex + 1; i < mobList.Items.Count; i++)
                {
                    mobList.SetItemChecked(i, false);
                }

                UpdateFilter();
            }
        }

        #endregion

        #region Public Methods
        public void UpdateMobsList()
        {
            FillMobsList();
        }

        public void ResetMobsList()
        {
            mobList.Items.Clear();
        }
        #endregion

        #region Utility Functions
        private void FillMobsList()
        {
            mobList.Items.Clear();

            // Get data from Mob XP Handler to add mob entries to the checked item list.
            MobFilter mobFilter = MobXPHandler.Instance.CustomMobFilter;

            string mobString;
            var mobXPVals = MobXPHandler.Instance.CompleteMobList;

            foreach (var mob in mobXPVals)
            {
                mobString = string.Format("{0}  ({1})", mob.Name, mob.BaseXP);
                mobList.Items.Add(mobString);
            }

            // If the filter has any data in it, check those mobs
            for (int i = 0; i < mobList.Items.Count; i++)
            {
                if (mobFilter.CustomBattleIDs.Contains(mobXPVals.ElementAt(i).BattleID))
                    mobList.SetItemChecked(i, true);
            }
        }

        private MobXPValues GetSelectedMob()
        {
            MobXPValues mob = null;
            
            if (mobList.SelectedIndex >= 0)
            {
                mob = MobXPHandler.Instance.CompleteMobList.ElementAt(mobList.SelectedIndex);
            }

            return mob;
        }

        private void UpdateFilter()
        {
            var mobXPVals = MobXPHandler.Instance.CompleteMobList;
            MobFilter mobFilter = MobXPHandler.Instance.CustomMobFilter;

            mobFilter.CustomBattleIDs.Clear();
            foreach (int index in mobList.CheckedIndices)
            {
                mobFilter.CustomBattleIDs.Add(mobXPVals.ElementAt(index).BattleID);
            }

            NotifyMobXPHandlerOfChangedMobList();
        }

        private void NotifyMobXPHandlerOfChangedMobList()
        {
            // Update the mob filter and send event to controlling plugin to update
            MobXPHandler.Instance.OnCustomMobFilterWasChanged();
        }

        #endregion

    }
}
