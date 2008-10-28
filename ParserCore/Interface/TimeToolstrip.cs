using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WaywardGamers.KParser.Plugin
{
    internal enum GameDay
    {
        Firesday,
        Earthsday,
        Watersday,
        Windsday,
        Iceday,
        Lightningday,
        Lightsday,
        Darksday
    }

    public class TimeToolstrip : ToolStrip
    {
        #region Member Variable - Components
        ToolStripLabel filterLabel;
        ToolStripLabel dayLabel;
        ToolStripLabel startLabel;
        ToolStripLabel endLabel;

        ToolStripDropDownButton filterTypeMenu;

        ToolStripComboBox gameDayCombo;

        #endregion

        #region Constructor
        public TimeToolstrip()
        {
            this.SuspendLayout();

            this.Name = "timeToolstrip";
            this.Size = new System.Drawing.Size(100, 25);
            this.TabIndex = 0;
            this.Text = "timeToolstrip";
            this.GripStyle = ToolStripGripStyle.Hidden;


            filterLabel = new ToolStripLabel();
            filterLabel.Text = "Filter Type";

            dayLabel = new ToolStripLabel();
            dayLabel.Text = "Day of Game Week";

            startLabel = new ToolStripLabel();
            startLabel.Text = "Start Time:";

            endLabel = new ToolStripLabel();
            endLabel.Text = "End Time:";

            filterTypeMenu = new ToolStripDropDownButton();
            filterTypeMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;
            filterTypeMenu.Text = "Time Filter Type";

            ToolStripMenuItem unfilteredOption = new ToolStripMenuItem();
            ToolStripMenuItem timeBasedOption = new ToolStripMenuItem();
            ToolStripMenuItem fightBasedOption = new ToolStripMenuItem();
            ToolStripMenuItem gameDayOption = new ToolStripMenuItem();
            //ToolStripMenuItem notesOption = new ToolStripMenuItem();
            unfilteredOption.Text = "None";
            timeBasedOption.Text = "Start/End Time";
            fightBasedOption.Text = "First/Last Fight";
            gameDayOption.Text = "By Game Day";

            unfilteredOption.Click += new EventHandler(unfilteredOption_Click);
            timeBasedOption.Click += new EventHandler(timeBasedOption_Click);
            fightBasedOption.Click += new EventHandler(fightBasedOption_Click);
            gameDayOption.Click += new EventHandler(gameDayOption_Click);
            unfilteredOption.Checked = true;

            filterTypeMenu.DropDownItems.Add(unfilteredOption);
            filterTypeMenu.DropDownItems.Add(timeBasedOption);
            filterTypeMenu.DropDownItems.Add(fightBasedOption);
            filterTypeMenu.DropDownItems.Add(gameDayOption);

            this.Items.Add(filterTypeMenu);


            gameDayCombo = new ToolStripComboBox();
            gameDayCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            gameDayCombo.MaxDropDownItems = 8;

            for (GameDay day = GameDay.Firesday; day <= GameDay.Darksday; day++)
            {
                gameDayCombo.Items.Add(day.ToString());
            }

            gameDayCombo.SelectedIndex = 0;

            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        #region Event Handlers
        protected void unfilteredOption_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            foreach (ToolStripMenuItem menuItem in filterTypeMenu.DropDownItems)
            {
                if (menuItem == sentBy)
                    menuItem.Checked = true;
                else
                    menuItem.Checked = false;
            }

            this.Items.Remove(gameDayCombo);
        }

        protected void timeBasedOption_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            foreach (ToolStripMenuItem menuItem in filterTypeMenu.DropDownItems)
            {
                if (menuItem == sentBy)
                    menuItem.Checked = true;
                else
                    menuItem.Checked = false;
            }

            this.Items.Remove(gameDayCombo);
        }

        protected void fightBasedOption_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            foreach (ToolStripMenuItem menuItem in filterTypeMenu.DropDownItems)
            {
                if (menuItem == sentBy)
                    menuItem.Checked = true;
                else
                    menuItem.Checked = false;
            }

            this.Items.Remove(gameDayCombo);
        }

        protected void gameDayOption_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            foreach (ToolStripMenuItem menuItem in filterTypeMenu.DropDownItems)
            {
                if (menuItem == sentBy)
                    menuItem.Checked = true;
                else
                    menuItem.Checked = false;
            }

            this.Items.Add(gameDayCombo);

        }
        #endregion
    }
}
