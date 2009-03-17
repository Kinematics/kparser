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

namespace WaywardGamers.KParser.Interface
{
    public partial class CustomMobSelectionDlg : Form
    {
        #region Constructor
        MobFilter mobFilter;

        public CustomMobSelectionDlg(MobFilter mobFilter)
        {
            InitializeComponent();

            this.mobFilter = mobFilter;
            if (this.mobFilter.CustomSelection == false)
            {
                this.mobFilter.CustomSelection = true;
                if (this.mobFilter.CustomBattleIDs == null)
                    this.mobFilter.CustomBattleIDs = new HashSet<int>();
                else
                    this.mobFilter.CustomBattleIDs.Clear();
            }

            FillMobsList();
        }
        #endregion

        #region Event Handlers
        private void mobList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                mobFilter.CustomBattleIDs.Add(MobXPHandler.Instance.CompleteMobList.ElementAt(e.Index).BattleID);
            }
            else
            {
                mobFilter.CustomBattleIDs.Remove(MobXPHandler.Instance.CompleteMobList.ElementAt(e.Index).BattleID);
            }

            NotifyOfChangedMobList();
        }

        private void selectAllButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < mobList.Items.Count; i++)
            {
                mobList.SetItemChecked(i, true);
            }

            UpdateFilter();
            NotifyOfChangedMobList();
        }

        private void invertSelectionButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < mobList.Items.Count; i++)
            {
                mobList.SetItemChecked(i, !mobList.GetItemChecked(i));
            }

            UpdateFilter();
            NotifyOfChangedMobList();
        }

        private void selectNoneButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < mobList.Items.Count; i++)
            {
                mobList.SetItemChecked(i, false);
            }

            UpdateFilter();
            NotifyOfChangedMobList();
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
            NotifyOfChangedMobList();
        }

        private void allCurrentSelectionTypes_Click(object sender, EventArgs e)
        {
            string mobName = GetSelectedMobName();

            if (mobName != string.Empty)
            {
                var mobXPVals = MobXPHandler.Instance.CompleteMobList;

                for (int i = 0; i < mobList.Items.Count; i++)
                {
                    if (mobXPVals.ElementAt(i).Name == mobName)
                        mobList.SetItemChecked(i, true);
                }

                UpdateFilter();
                NotifyOfChangedMobList();
            }
        }

        private void noneOfCurrentSelectionTypes_Click(object sender, EventArgs e)
        {
            string mobName = GetSelectedMobName();

            if (mobName != string.Empty)
            {
                var mobXPVals = MobXPHandler.Instance.CompleteMobList;

                for (int i = 0; i < mobList.Items.Count; i++)
                {
                    if (mobXPVals.ElementAt(i).Name == mobName)
                        mobList.SetItemChecked(i, false);
                }

                UpdateFilter();
                NotifyOfChangedMobList();
            }
        }
        #endregion

        #region Utility Functions
        private void FillMobsList()
        {
            // Get data from Mob XP Handler to add mob entries to the checked item list.

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

        private string GetSelectedMobName()
        {
            string mobName = string.Empty;

            if (mobList.SelectedIndex >= 0)
            {
                mobName = MobXPHandler.Instance.CompleteMobList.ElementAt(mobList.SelectedIndex).Name;
            }

            return mobName;
        }

        private void UpdateFilter()
        {
            var mobXPVals = MobXPHandler.Instance.CompleteMobList;

            mobFilter.CustomBattleIDs.Clear();
            foreach (int index in mobList.CheckedIndices)
            {
                mobFilter.CustomBattleIDs.Add(mobXPVals.ElementAt(index).BattleID);
            }
        }

        private void NotifyOfChangedMobList()
        {
            // Update the mob filter and send event to controlling plugin to update

            // First, determine the plugin
            IPlugin parentPlugin = this.Parent as IPlugin;

            if (parentPlugin != null)
            {
                parentPlugin.UpdateUsingMobFilter(mobFilter);
            }
        }

        #endregion

    }
}
