﻿using System.Windows.Forms;

namespace WaywardGamers.KParser.Plugin
{
    public partial class CustomMobSelectionDlg : Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CustomMobSelectionDlg));
            this.mobListContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectAllButton = new System.Windows.Forms.Button();
            this.selectNoneButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.invertSelectionButton = new System.Windows.Forms.Button();
            this.uncheck0XPMobs = new System.Windows.Forms.Button();
            this.mobList = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numMobsSelected = new System.Windows.Forms.Label();
            this.mobListContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // mobListContextMenuStrip
            // 
            this.mobListContextMenuStrip.AccessibleDescription = null;
            this.mobListContextMenuStrip.AccessibleName = null;
            resources.ApplyResources(this.mobListContextMenuStrip, "mobListContextMenuStrip");
            this.mobListContextMenuStrip.BackgroundImage = null;
            this.mobListContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem,
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem,
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem,
            this.toolStripSeparator2,
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem,
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem,
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem});
            this.mobListContextMenuStrip.Name = "mobListContextMenuStrip";
            this.mobListContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.mobListContextMenuStrip_Opening);
            // 
            // checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem
            // 
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem.AccessibleDescription = null;
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem, "checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem");
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem.BackgroundImage = null;
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Name = "checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem";
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Click += new System.EventHandler(this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem_Click);
            // 
            // checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem
            // 
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.AccessibleDescription = null;
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem, "checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem");
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.BackgroundImage = null;
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Name = "checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem";
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Click += new System.EventHandler(this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem_Click);
            // 
            // checkAllMobsBelowCurrentSelectionToolStripMenuItem
            // 
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem.AccessibleDescription = null;
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.checkAllMobsBelowCurrentSelectionToolStripMenuItem, "checkAllMobsBelowCurrentSelectionToolStripMenuItem");
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem.BackgroundImage = null;
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem.Name = "checkAllMobsBelowCurrentSelectionToolStripMenuItem";
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem.Click += new System.EventHandler(this.checkAllMobsBelowCurrentSelectionToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.AccessibleDescription = null;
            this.toolStripSeparator2.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            // 
            // uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem
            // 
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem.AccessibleDescription = null;
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem, "uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem");
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem.BackgroundImage = null;
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Name = "uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem";
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Click += new System.EventHandler(this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem_Click);
            // 
            // uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem
            // 
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.AccessibleDescription = null;
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem, "uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem");
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.BackgroundImage = null;
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Name = "uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem";
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Click += new System.EventHandler(this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem_Click);
            // 
            // uncheckAllMobsBelowCurrentSelectionToolStripMenuItem
            // 
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem.AccessibleDescription = null;
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem, "uncheckAllMobsBelowCurrentSelectionToolStripMenuItem");
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem.BackgroundImage = null;
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem.Name = "uncheckAllMobsBelowCurrentSelectionToolStripMenuItem";
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem.Click += new System.EventHandler(this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem_Click);
            // 
            // selectAllButton
            // 
            this.selectAllButton.AccessibleDescription = null;
            this.selectAllButton.AccessibleName = null;
            resources.ApplyResources(this.selectAllButton, "selectAllButton");
            this.selectAllButton.BackgroundImage = null;
            this.selectAllButton.Font = null;
            this.selectAllButton.Name = "selectAllButton";
            this.selectAllButton.UseVisualStyleBackColor = true;
            this.selectAllButton.Click += new System.EventHandler(this.selectAllButton_Click);
            // 
            // selectNoneButton
            // 
            this.selectNoneButton.AccessibleDescription = null;
            this.selectNoneButton.AccessibleName = null;
            resources.ApplyResources(this.selectNoneButton, "selectNoneButton");
            this.selectNoneButton.BackgroundImage = null;
            this.selectNoneButton.Font = null;
            this.selectNoneButton.Name = "selectNoneButton";
            this.selectNoneButton.UseVisualStyleBackColor = true;
            this.selectNoneButton.Click += new System.EventHandler(this.selectNoneButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.AccessibleDescription = null;
            this.closeButton.AccessibleName = null;
            resources.ApplyResources(this.closeButton, "closeButton");
            this.closeButton.BackgroundImage = null;
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.closeButton.Font = null;
            this.closeButton.Name = "closeButton";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // invertSelectionButton
            // 
            this.invertSelectionButton.AccessibleDescription = null;
            this.invertSelectionButton.AccessibleName = null;
            resources.ApplyResources(this.invertSelectionButton, "invertSelectionButton");
            this.invertSelectionButton.BackgroundImage = null;
            this.invertSelectionButton.Font = null;
            this.invertSelectionButton.Name = "invertSelectionButton";
            this.invertSelectionButton.UseVisualStyleBackColor = true;
            this.invertSelectionButton.Click += new System.EventHandler(this.invertSelectionButton_Click);
            // 
            // uncheck0XPMobs
            // 
            this.uncheck0XPMobs.AccessibleDescription = null;
            this.uncheck0XPMobs.AccessibleName = null;
            resources.ApplyResources(this.uncheck0XPMobs, "uncheck0XPMobs");
            this.uncheck0XPMobs.BackgroundImage = null;
            this.uncheck0XPMobs.Font = null;
            this.uncheck0XPMobs.Name = "uncheck0XPMobs";
            this.uncheck0XPMobs.UseVisualStyleBackColor = true;
            this.uncheck0XPMobs.Click += new System.EventHandler(this.uncheck0XPMobs_Click);
            // 
            // mobList
            // 
            this.mobList.AccessibleDescription = null;
            this.mobList.AccessibleName = null;
            resources.ApplyResources(this.mobList, "mobList");
            this.mobList.BackgroundImage = null;
            this.mobList.ContextMenuStrip = this.mobListContextMenuStrip;
            this.mobList.Font = null;
            this.mobList.FormattingEnabled = true;
            this.mobList.Name = "mobList";
            this.mobList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.mobList.MouseHover += new System.EventHandler(this.mobList_MouseHover);
            this.mobList.SelectedIndexChanged += new System.EventHandler(this.mobList_SelectedIndexChanged);
            this.mobList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mobList_MouseDown);
            // 
            // label1
            // 
            this.label1.AccessibleDescription = null;
            this.label1.AccessibleName = null;
            resources.ApplyResources(this.label1, "label1");
            this.label1.Font = null;
            this.label1.Name = "label1";
            // 
            // numMobsSelected
            // 
            this.numMobsSelected.AccessibleDescription = null;
            this.numMobsSelected.AccessibleName = null;
            resources.ApplyResources(this.numMobsSelected, "numMobsSelected");
            this.numMobsSelected.Font = null;
            this.numMobsSelected.Name = "numMobsSelected";
            // 
            // CustomMobSelectionDlg
            // 
            this.AcceptButton = this.closeButton;
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.Controls.Add(this.mobList);
            this.Controls.Add(this.uncheck0XPMobs);
            this.Controls.Add(this.numMobsSelected);
            this.Controls.Add(this.invertSelectionButton);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.selectNoneButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.selectAllButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = null;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CustomMobSelectionDlg";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CustomMobSelectionDlg_FormClosing);
            this.mobListContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button selectAllButton;
        private System.Windows.Forms.Button selectNoneButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Button invertSelectionButton;
        private Button uncheck0XPMobs;
        private ContextMenuStrip mobListContextMenuStrip;
        private ToolStripMenuItem checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem;
        private ToolStripMenuItem uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem;
        private ToolStripMenuItem checkAllMobsBelowCurrentSelectionToolStripMenuItem;
        private ToolStripMenuItem uncheckAllMobsBelowCurrentSelectionToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem;
        private ToolStripMenuItem uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem;
        private ListBox mobList;
        private Label label1;
        private Label numMobsSelected;
    }
}