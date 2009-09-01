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
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.mobListContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem,
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem,
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem,
            this.toolStripSeparator2,
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem,
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem,
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
            this.mobList.ContextMenuStrip = this.mobListContextMenuStrip;
            this.mobList.FormattingEnabled = true;
            this.mobList.Name = "mobList";
            this.mobList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.mobList.MouseHover += new System.EventHandler(this.mobList_MouseHover);
            this.mobList.SelectedIndexChanged += new System.EventHandler(this.mobList_SelectedIndexChanged);
            this.mobList.MouseDown += new System.Windows.Forms.MouseEventHandler(this.mobList_MouseDown);
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