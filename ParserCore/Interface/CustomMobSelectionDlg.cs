using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WaywardGamers.KParser.Database;
using WaywardGamers.KParser.Plugin;

namespace WaywardGamers.KParser.Plugin
{
    public partial class CustomMobSelectionDlg : Form
    {
        #region Member Variables
        bool checkingMobList = false;
        List<int> oldIndices = new List<int>();

        // Can't depend on mobList.SelectedIndex to be accurate to the
        // item selected with a right-click, so storing in a local class
        // local variable.
        int rightClickIndex = -1;

        readonly string contextMenuRegexFormat = "(?<prefix>[^:]+):.*";
        Regex contextMenuRegex;
        #endregion

        #region Constructor
        public CustomMobSelectionDlg()
        {
            InitializeComponent();

            contextMenuRegex = new Regex(contextMenuRegexFormat);
        }
        #endregion

        #region Event Handlers

        // Window events

        private void CustomMobSelectionDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If the user closed the window, just hide it.
            // If CloseReason is something else (eg: application shutdown, windows shutdown),
            // then allow it to proceed normally.
            if ((e.CloseReason == CloseReason.None) ||
                (e.CloseReason == CloseReason.UserClosing))
                e.Cancel = true;

            this.Hide();
        }

        // List box event handlers

        private void mobList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Flag to prevent processing of this section of code if we're
            // using one of the predefined event handler functions below.
            if (checkingMobList)
                return;

            MobFilter mobFilter = MobXPHandler.Instance.CustomMobFilter;
            // List of which entries in the mob list are currently selected.
            var indices = mobList.SelectedIndices;

            foreach (int i in indices)
            {
                if (oldIndices.BinarySearch(i) >= 0)
                {
                    mobFilter.CustomBattleIDs.Add(MobXPHandler.Instance.CompleteMobList.ElementAt(i).BattleID);
                }
            }

            foreach (int i in oldIndices)
            {
                if (!indices.Contains(i))
                    mobFilter.CustomBattleIDs.Remove(MobXPHandler.Instance.CompleteMobList.ElementAt(i).BattleID);
            }

            oldIndices = indices.Cast<int>().ToList<int>();
            oldIndices.Sort();

            UpdateFilter();
        }

        private void mobList_MouseHover(object sender, EventArgs e)
        {
            // Give the list focus on hover so that the mouse wheel works.
            mobList.Focus();
        }

        // Button event handlers

        private void selectAllButton_Click(object sender, EventArgs e)
        {
            try
            {
                checkingMobList = true;

                for (int i = 0; i < mobList.Items.Count; i++)
                {
                    mobList.SetSelected(i, true);
                }
            }
            finally
            {
                checkingMobList = false;
            }

            UpdateFilter();
        }

        private void invertSelectionButton_Click(object sender, EventArgs e)
        {
            try
            {
                checkingMobList = true;

                for (int i = 0; i < mobList.Items.Count; i++)
                {
                    mobList.SetSelected(i, !mobList.GetSelected(i));
                }
            }
            finally
            {
                checkingMobList = false;
            }

            UpdateFilter();
        }

        private void selectNoneButton_Click(object sender, EventArgs e)
        {
            try
            {
                checkingMobList = true;

                for (int i = 0; i < mobList.Items.Count; i++)
                {
                    mobList.SetSelected(i, false);
                }
            }
            finally
            {
                checkingMobList = false;
            }

            UpdateFilter();
        }

        private void uncheck0XPMobs_Click(object sender, EventArgs e)
        {
            var mobXPVals = MobXPHandler.Instance.CompleteMobList;

            try
            {
                checkingMobList = true;

                for (int i = 0; i < mobList.Items.Count; i++)
                {
                    if (mobXPVals.ElementAt(i).BaseXP == 0)
                    {
                        mobList.SetSelected(i, false);
                    }
                }
            }
            finally
            {
                checkingMobList = false;
            }

            UpdateFilter();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        // Context menu event handlers

        private void mobList_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                rightClickIndex = mobList.IndexFromPoint(e.X, e.Y);

                //if (rightClickIndex >= 0)
                //{
                //    mobList.SetSelected(rightClickIndex, true);
                //}
            }
        }

        private void mobListContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            MobXPValues mob = GetRCSelectedMob();

            if ((mob == null) || (mobList.Items.Count == 0))
            {
                checkAllMobsBelowCurrentSelectionToolStripMenuItem.Enabled = false;
                uncheckAllMobsBelowCurrentSelectionToolStripMenuItem.Enabled = false;
                checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Enabled = false;
                uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Enabled = false;
                checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Enabled = false;
                uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Enabled = false;
                return;
            }

            checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Text =
                string.Format("{0}:  {1}",
                GetBaseToolStripContextMenuString(checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem),
                mob.Name);
            checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Text =
                string.Format("{0}:  {1} ({2})",
                GetBaseToolStripContextMenuString(checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem),
                mob.Name, mob.BaseXP);
            checkAllMobsBelowCurrentSelectionToolStripMenuItem.Text =
                string.Format("{0}:  {1} (#{2})",
                GetBaseToolStripContextMenuString(checkAllMobsBelowCurrentSelectionToolStripMenuItem),
                mob.Name, mob.BattleID);
            uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Text =
                string.Format("{0}:  {1}",
                GetBaseToolStripContextMenuString(uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem),
                mob.Name);
            uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Text =
                string.Format("{0}:  {1} ({2})",
                GetBaseToolStripContextMenuString(uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem),
                mob.Name, mob.BaseXP);
            uncheckAllMobsBelowCurrentSelectionToolStripMenuItem.Text =
                string.Format("{0}:  {1} (#{2})",
                GetBaseToolStripContextMenuString(uncheckAllMobsBelowCurrentSelectionToolStripMenuItem),
                mob.Name, mob.BattleID);


            checkAllMobsBelowCurrentSelectionToolStripMenuItem.Enabled = true;
            uncheckAllMobsBelowCurrentSelectionToolStripMenuItem.Enabled = true;
            checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Enabled = true;
            uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Enabled = true;
            checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Enabled = true;
            uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Enabled = true;
        }

        private void checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MobXPValues mob = GetRCSelectedMob();

            if (mob != null)
            {
                var mobXPVals = MobXPHandler.Instance.CompleteMobList;

                try
                {
                    checkingMobList = true;

                    for (int i = 0; i < mobList.Items.Count; i++)
                    {
                        if (mobXPVals.ElementAt(i).Name == mob.Name)
                        {
                            mobList.SetSelected(i, true);
                        }
                    }
                }
                finally
                {
                    checkingMobList = false;
                }

                UpdateFilter();
            }
        }

        private void uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MobXPValues mob = GetRCSelectedMob();

            if (mob != null)
            {
                var mobXPVals = MobXPHandler.Instance.CompleteMobList;

                try
                {
                    checkingMobList = true;

                    for (int i = 0; i < mobList.Items.Count; i++)
                    {
                        if (mobXPVals.ElementAt(i).Name == mob.Name)
                        {
                            mobList.SetSelected(i, false);
                        }
                    }
                }
                finally
                {
                    checkingMobList = false;
                }

                UpdateFilter();
            }
        }

        private void checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MobXPValues selectedMob = GetRCSelectedMob();

            if (selectedMob != null)
            {
                var mobXPVals = MobXPHandler.Instance.CompleteMobList;

                try
                {
                    checkingMobList = true;

                    for (int i = 0; i < mobList.Items.Count; i++)
                    {
                        MobXPValues mob = mobXPVals.ElementAt(i);

                        if ((mob.Name == selectedMob.Name) && (mob.BaseXP == selectedMob.BaseXP))
                        {
                            mobList.SetSelected(i, true);
                        }
                    }
                }
                finally
                {
                    checkingMobList = false;
                }

                UpdateFilter();
            }
        }

        private void uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MobXPValues selectedMob = GetRCSelectedMob();

            if (selectedMob != null)
            {
                var mobXPVals = MobXPHandler.Instance.CompleteMobList;

                try
                {
                    checkingMobList = true;

                    for (int i = 0; i < mobList.Items.Count; i++)
                    {
                        MobXPValues mob = mobXPVals.ElementAt(i);

                        if ((mob.Name == selectedMob.Name) && (mob.BaseXP == selectedMob.BaseXP))
                        {
                            mobList.SetSelected(i, false);
                        }
                    }
                }
                finally
                {
                    checkingMobList = false;
                }

                UpdateFilter();
            }
        }

        private void checkAllMobsBelowCurrentSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rightClickIndex >= 0)
            {
                try
                {
                    checkingMobList = true;

                    for (int i = rightClickIndex + 1; i < mobList.Items.Count; i++)
                    {
                        mobList.SetSelected(i, true);
                    }
                }
                finally
                {
                    checkingMobList = false;
                }

                UpdateFilter();
            }
        }

        private void uncheckAllMobsBelowCurrentSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (rightClickIndex >= 0)
            {
                try
                {
                    checkingMobList = true;

                    for (int i = rightClickIndex + 1; i < mobList.Items.Count; i++)
                    {
                        mobList.SetSelected(i, false);
                    }
                }
                finally
                {
                    checkingMobList = false;
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
            oldIndices.Clear();
        }

        internal void NotifyOfCultureChange()
        {
            UpdateUI();
        }
        #endregion

        #region Utility Functions
        private void FillMobsList()
        {
            mobList.Items.Clear();
            oldIndices.Clear();

            // Get data from Mob XP Handler to add mob entries to the checked item list.
            MobFilter mobFilter = MobXPHandler.Instance.CustomMobFilter;

            string mobString;
            var mobXPVals = MobXPHandler.Instance.CompleteMobList;

            foreach (var mob in mobXPVals)
            {
                mobString = string.Format("{2} - {0}  ({1})", mob.Name, mob.BaseXP, mob.BattleID);
                mobList.Items.Add(mobString);
            }

            // If the filter has any data in it, check those mobs
            for (int i = 0; i < mobList.Items.Count; i++)
            {
                if (mobFilter.CustomBattleIDs.Contains(mobXPVals.ElementAt(i).BattleID))
                    mobList.SetSelected(i, true);
            }
        }

        private MobXPValues GetRCSelectedMob()
        {
            // Can't depend on mobList.SelectedIndex to be accurate to the
            // item selected with a right-click, so storing in a local class
            // variable.

            MobXPValues mob = null;

            try
            {
                if (rightClickIndex >= 0)
                {
                    mob = MobXPHandler.Instance.CompleteMobList.ElementAt(rightClickIndex);
                }
            }
            catch (Exception)
            {
            }

            return mob;
        }

        private void UpdateFilter()
        {
            var mobXPVals = MobXPHandler.Instance.CompleteMobList;
            MobFilter mobFilter = MobXPHandler.Instance.CustomMobFilter;

            mobFilter.CustomBattleIDs.Clear();
            foreach (int index in mobList.SelectedIndices)
            {
                mobFilter.CustomBattleIDs.Add(mobXPVals.ElementAt(index).BattleID);
            }

            NotifyMobXPHandlerOfChangedMobList();

            numMobsSelected.Text = mobList.SelectedIndices.Count.ToString();
        }

        private void NotifyMobXPHandlerOfChangedMobList()
        {
            // Update the mob filter and send event to controlling plugin to update
            MobXPHandler.Instance.OnCustomMobFilterWasChanged();
        }

        private string GetBaseToolStripContextMenuString(ToolStripMenuItem menuItem)
        {
            if (menuItem == null)
                throw new ArgumentNullException();

            Match contextMenuMatch = contextMenuRegex.Match(menuItem.Text);

            if (contextMenuMatch.Success)
            {
                return contextMenuMatch.Groups["prefix"].ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        private void UpdateUI()
        {
            System.ComponentModel.ComponentResourceManager resources =
                new System.ComponentModel.ComponentResourceManager(typeof(CustomMobSelectionDlg));

            resources.ApplyResources(this.mobListContextMenuStrip, "mobListContextMenuStrip");
            resources.ApplyResources(this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem, "checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem");
            resources.ApplyResources(this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem, "checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem");
            resources.ApplyResources(this.checkAllMobsBelowCurrentSelectionToolStripMenuItem, "checkAllMobsBelowCurrentSelectionToolStripMenuItem");
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            resources.ApplyResources(this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem, "uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem");
            resources.ApplyResources(this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem, "uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem");
            resources.ApplyResources(this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem, "uncheckAllMobsBelowCurrentSelectionToolStripMenuItem");
            resources.ApplyResources(this.selectAllButton, "selectAllButton");
            resources.ApplyResources(this.selectNoneButton, "selectNoneButton");
            resources.ApplyResources(this.closeButton, "closeButton");
            resources.ApplyResources(this.invertSelectionButton, "invertSelectionButton");
            resources.ApplyResources(this.uncheck0XPMobs, "uncheck0XPMobs");
            resources.ApplyResources(this.mobList, "mobList");
            resources.ApplyResources(this.label1, "label1");
            resources.ApplyResources(this.numMobsSelected, "numMobsSelected");
            resources.ApplyResources(this, "$this");
        }
        #endregion
    }
}
