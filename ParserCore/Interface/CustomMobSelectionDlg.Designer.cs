using System.Windows.Forms;

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
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectAllButton = new System.Windows.Forms.Button();
            this.selectNoneButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.invertSelectionButton = new System.Windows.Forms.Button();
            this.uncheck0XPMobs = new System.Windows.Forms.Button();
            this.mobList = new System.Windows.Forms.ListBox();
            this.mobList2ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.numMobsSelected = new System.Windows.Forms.Label();
            this.mobListContextMenuStrip.SuspendLayout();
            this.mobList2ContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // mobListContextMenuStrip
            // 
            this.mobListContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem,
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem,
            this.toolStripSeparator2,
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem,
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem,
            this.toolStripSeparator1,
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem,
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem});
            this.mobListContextMenuStrip.Name = "mobListContextMenuStrip";
            resources.ApplyResources(this.mobListContextMenuStrip, "mobListContextMenuStrip");
            this.mobListContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.mobListContextMenuStrip_Opening);
            // 
            // checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem
            // 
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Name = "checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem";
            resources.ApplyResources(this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem, "checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem");
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Click += new System.EventHandler(this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem_Click);
            // 
            // uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem
            // 
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Name = "uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem";
            resources.ApplyResources(this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem, "uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem");
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Click += new System.EventHandler(this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem
            // 
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Name = "checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem";
            resources.ApplyResources(this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem, "checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem");
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Click += new System.EventHandler(this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem_Click);
            // 
            // uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem
            // 
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Name = "uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem";
            resources.ApplyResources(this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem, "uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem");
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Click += new System.EventHandler(this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // checkAllMobsBelowCurrentSelectionToolStripMenuItem
            // 
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem.Name = "checkAllMobsBelowCurrentSelectionToolStripMenuItem";
            resources.ApplyResources(this.checkAllMobsBelowCurrentSelectionToolStripMenuItem, "checkAllMobsBelowCurrentSelectionToolStripMenuItem");
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem.Click += new System.EventHandler(this.checkAllMobsBelowCurrentSelectionToolStripMenuItem_Click);
            // 
            // uncheckAllMobsBelowCurrentSelectionToolStripMenuItem
            // 
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem.Name = "uncheckAllMobsBelowCurrentSelectionToolStripMenuItem";
            resources.ApplyResources(this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem, "uncheckAllMobsBelowCurrentSelectionToolStripMenuItem");
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem.Click += new System.EventHandler(this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem_Click);
            // 
            // selectAllButton
            // 
            resources.ApplyResources(this.selectAllButton, "selectAllButton");
            this.selectAllButton.Name = "selectAllButton";
            this.selectAllButton.UseVisualStyleBackColor = true;
            this.selectAllButton.Click += new System.EventHandler(this.selectAllButton_Click);
            // 
            // selectNoneButton
            // 
            resources.ApplyResources(this.selectNoneButton, "selectNoneButton");
            this.selectNoneButton.Name = "selectNoneButton";
            this.selectNoneButton.UseVisualStyleBackColor = true;
            this.selectNoneButton.Click += new System.EventHandler(this.selectNoneButton_Click);
            // 
            // closeButton
            // 
            resources.ApplyResources(this.closeButton, "closeButton");
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.closeButton.Name = "closeButton";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // invertSelectionButton
            // 
            resources.ApplyResources(this.invertSelectionButton, "invertSelectionButton");
            this.invertSelectionButton.Name = "invertSelectionButton";
            this.invertSelectionButton.UseVisualStyleBackColor = true;
            this.invertSelectionButton.Click += new System.EventHandler(this.invertSelectionButton_Click);
            // 
            // uncheck0XPMobs
            // 
            resources.ApplyResources(this.uncheck0XPMobs, "uncheck0XPMobs");
            this.uncheck0XPMobs.Name = "uncheck0XPMobs";
            this.uncheck0XPMobs.UseVisualStyleBackColor = true;
            this.uncheck0XPMobs.Click += new System.EventHandler(this.uncheck0XPMobs_Click);
            // 
            // mobList
            // 
            resources.ApplyResources(this.mobList, "mobList");
            this.mobList.ContextMenuStrip = this.mobList2ContextMenuStrip;
            this.mobList.FormattingEnabled = true;
            this.mobList.Name = "mobList";
            this.mobList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.mobList.SelectedIndexChanged += new System.EventHandler(this.mobList_SelectedIndexChanged);
            this.mobList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mobList_MouseDown);
            // 
            // mobList2ContextMenuStrip
            // 
            this.mobList2ContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem1,
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem1,
            this.toolStripSeparator3,
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1,
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1,
            this.toolStripSeparator4,
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem1,
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem1});
            this.mobList2ContextMenuStrip.Name = "mobList2ContextMenuStrip";
            resources.ApplyResources(this.mobList2ContextMenuStrip, "mobList2ContextMenuStrip");
            // 
            // checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem1
            // 
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem1.Name = "checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem1";
            resources.ApplyResources(this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem1, "checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem1");
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem1.Click += new System.EventHandler(this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem_Click);
            // 
            // uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem1
            // 
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem1.Name = "uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem1";
            resources.ApplyResources(this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem1, "uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem1");
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem1.Click += new System.EventHandler(this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1
            // 
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1.Name = "checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1";
            resources.ApplyResources(this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1, "checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1");
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1.Click += new System.EventHandler(this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem_Click);
            // 
            // uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1
            // 
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1.Name = "uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1";
            resources.ApplyResources(this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1, "uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1");
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1.Click += new System.EventHandler(this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            // 
            // checkAllMobsBelowCurrentSelectionToolStripMenuItem1
            // 
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem1.Name = "checkAllMobsBelowCurrentSelectionToolStripMenuItem1";
            resources.ApplyResources(this.checkAllMobsBelowCurrentSelectionToolStripMenuItem1, "checkAllMobsBelowCurrentSelectionToolStripMenuItem1");
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem1.Click += new System.EventHandler(this.checkAllMobsBelowCurrentSelectionToolStripMenuItem_Click);
            // 
            // uncheckAllMobsBelowCurrentSelectionToolStripMenuItem1
            // 
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem1.Name = "uncheckAllMobsBelowCurrentSelectionToolStripMenuItem1";
            resources.ApplyResources(this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem1, "uncheckAllMobsBelowCurrentSelectionToolStripMenuItem1");
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem1.Click += new System.EventHandler(this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // numMobsSelected
            // 
            resources.ApplyResources(this.numMobsSelected, "numMobsSelected");
            this.numMobsSelected.Name = "numMobsSelected";
            // 
            // CustomMobSelectionDlg
            // 
            this.AcceptButton = this.closeButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mobList);
            this.Controls.Add(this.invertSelectionButton);
            this.Controls.Add(this.uncheck0XPMobs);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.numMobsSelected);
            this.Controls.Add(this.selectNoneButton);
            this.Controls.Add(this.selectAllButton);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CustomMobSelectionDlg";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CustomMobSelectionDlg_FormClosing);
            this.mobListContextMenuStrip.ResumeLayout(false);
            this.mobList2ContextMenuStrip.ResumeLayout(false);
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
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem;
        private ToolStripMenuItem uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem;
        private ListBox mobList;
        private ContextMenuStrip mobList2ContextMenuStrip;
        private ToolStripMenuItem checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem1;
        private ToolStripMenuItem uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem1;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1;
        private ToolStripMenuItem uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem checkAllMobsBelowCurrentSelectionToolStripMenuItem1;
        private ToolStripMenuItem uncheckAllMobsBelowCurrentSelectionToolStripMenuItem1;
        private Label label1;
        private Label numMobsSelected;
    }
}