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
            this.mobListContextMenuStrip.Size = new System.Drawing.Size(336, 148);
            this.mobListContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.mobListContextMenuStrip_Opening);
            // 
            // checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem
            // 
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Name = "checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem";
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Size = new System.Drawing.Size(335, 22);
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Text = "Check All Mobs of Currently Selected Type";
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Click += new System.EventHandler(this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem_Click);
            // 
            // uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem
            // 
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Name = "uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem";
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Size = new System.Drawing.Size(335, 22);
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Text = "Uncheck All Mobs of Currently Selected Type";
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem.Click += new System.EventHandler(this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(332, 6);
            // 
            // checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem
            // 
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Name = "checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem";
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Size = new System.Drawing.Size(335, 22);
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Text = "Check All Mobs of Currently Selected Type and XP";
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Click += new System.EventHandler(this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem_Click);
            // 
            // uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem
            // 
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Name = "uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem";
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Size = new System.Drawing.Size(335, 22);
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Text = "Uncheck All Mobs of Currently Selected Type and XP";
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem.Click += new System.EventHandler(this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(332, 6);
            // 
            // checkAllMobsBelowCurrentSelectionToolStripMenuItem
            // 
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem.Name = "checkAllMobsBelowCurrentSelectionToolStripMenuItem";
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem.Size = new System.Drawing.Size(335, 22);
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem.Text = "Check All Mobs Below Current Selection";
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem.Click += new System.EventHandler(this.checkAllMobsBelowCurrentSelectionToolStripMenuItem_Click);
            // 
            // uncheckAllMobsBelowCurrentSelectionToolStripMenuItem
            // 
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem.Name = "uncheckAllMobsBelowCurrentSelectionToolStripMenuItem";
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem.Size = new System.Drawing.Size(335, 22);
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem.Text = "Uncheck All Mobs Below Current Selection";
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem.Click += new System.EventHandler(this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem_Click);
            // 
            // selectAllButton
            // 
            this.selectAllButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.selectAllButton.Location = new System.Drawing.Point(28, 326);
            this.selectAllButton.Name = "selectAllButton";
            this.selectAllButton.Size = new System.Drawing.Size(75, 23);
            this.selectAllButton.TabIndex = 1;
            this.selectAllButton.Text = "All";
            this.selectAllButton.UseVisualStyleBackColor = true;
            this.selectAllButton.Click += new System.EventHandler(this.selectAllButton_Click);
            // 
            // selectNoneButton
            // 
            this.selectNoneButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.selectNoneButton.Location = new System.Drawing.Point(189, 326);
            this.selectNoneButton.Name = "selectNoneButton";
            this.selectNoneButton.Size = new System.Drawing.Size(75, 23);
            this.selectNoneButton.TabIndex = 3;
            this.selectNoneButton.Text = "None";
            this.selectNoneButton.UseVisualStyleBackColor = true;
            this.selectNoneButton.Click += new System.EventHandler(this.selectNoneButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.closeButton.Location = new System.Drawing.Point(189, 356);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 5;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // invertSelectionButton
            // 
            this.invertSelectionButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.invertSelectionButton.Location = new System.Drawing.Point(109, 326);
            this.invertSelectionButton.Name = "invertSelectionButton";
            this.invertSelectionButton.Size = new System.Drawing.Size(75, 23);
            this.invertSelectionButton.TabIndex = 2;
            this.invertSelectionButton.Text = "Invert";
            this.invertSelectionButton.UseVisualStyleBackColor = true;
            this.invertSelectionButton.Click += new System.EventHandler(this.invertSelectionButton_Click);
            // 
            // uncheck0XPMobs
            // 
            this.uncheck0XPMobs.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.uncheck0XPMobs.Location = new System.Drawing.Point(28, 355);
            this.uncheck0XPMobs.Name = "uncheck0XPMobs";
            this.uncheck0XPMobs.Size = new System.Drawing.Size(121, 23);
            this.uncheck0XPMobs.TabIndex = 8;
            this.uncheck0XPMobs.Text = "Uncheck 0 XP Mobs";
            this.uncheck0XPMobs.UseVisualStyleBackColor = true;
            this.uncheck0XPMobs.Click += new System.EventHandler(this.uncheck0XPMobs_Click);
            // 
            // mobList
            // 
            this.mobList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mobList.ContextMenuStrip = this.mobList2ContextMenuStrip;
            this.mobList.FormattingEnabled = true;
            this.mobList.Location = new System.Drawing.Point(12, 12);
            this.mobList.Name = "mobList";
            this.mobList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.mobList.Size = new System.Drawing.Size(268, 290);
            this.mobList.TabIndex = 9;
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
            this.mobList2ContextMenuStrip.Size = new System.Drawing.Size(336, 148);
            // 
            // checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem1
            // 
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem1.Name = "checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem1";
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem1.Size = new System.Drawing.Size(335, 22);
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem1.Text = "Check All Mobs of Currently Selected Type";
            this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem1.Click += new System.EventHandler(this.checkAllMobsOfCurrentlySelectedTypeToolStripMenuItem_Click);
            // 
            // uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem1
            // 
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem1.Name = "uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem1";
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem1.Size = new System.Drawing.Size(335, 22);
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem1.Text = "Uncheck All Mobs of Currently Selected Type";
            this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem1.Click += new System.EventHandler(this.uncheckAllMobsOfCurrentlySelectedTypeToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(332, 6);
            // 
            // checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1
            // 
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1.Name = "checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1";
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1.Size = new System.Drawing.Size(335, 22);
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1.Text = "Check All Mobs of Currently Selected Type and XP";
            this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1.Click += new System.EventHandler(this.checkAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem_Click);
            // 
            // uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1
            // 
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1.Name = "uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1";
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1.Size = new System.Drawing.Size(335, 22);
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1.Text = "Uncheck All Mobs of Currently Selected Type and XP";
            this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem1.Click += new System.EventHandler(this.uncheckAllMobsOfCurrentlySelectedTypeAndXPToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(332, 6);
            // 
            // checkAllMobsBelowCurrentSelectionToolStripMenuItem1
            // 
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem1.Name = "checkAllMobsBelowCurrentSelectionToolStripMenuItem1";
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem1.Size = new System.Drawing.Size(335, 22);
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem1.Text = "Check All Mobs Below Current Selection";
            this.checkAllMobsBelowCurrentSelectionToolStripMenuItem1.Click += new System.EventHandler(this.checkAllMobsBelowCurrentSelectionToolStripMenuItem_Click);
            // 
            // uncheckAllMobsBelowCurrentSelectionToolStripMenuItem1
            // 
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem1.Name = "uncheckAllMobsBelowCurrentSelectionToolStripMenuItem1";
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem1.Size = new System.Drawing.Size(335, 22);
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem1.Text = "Uncheck All Mobs Below Current Selection";
            this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem1.Click += new System.EventHandler(this.uncheckAllMobsBelowCurrentSelectionToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(43, 305);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(170, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Current Number of Selected Mobs:";
            // 
            // numMobsSelected
            // 
            this.numMobsSelected.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.numMobsSelected.AutoSize = true;
            this.numMobsSelected.Location = new System.Drawing.Point(219, 305);
            this.numMobsSelected.Name = "numMobsSelected";
            this.numMobsSelected.Size = new System.Drawing.Size(13, 13);
            this.numMobsSelected.TabIndex = 11;
            this.numMobsSelected.Text = "0";
            // 
            // CustomMobSelectionDlg
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 391);
            this.Controls.Add(this.numMobsSelected);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.mobList);
            this.Controls.Add(this.invertSelectionButton);
            this.Controls.Add(this.uncheck0XPMobs);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.selectNoneButton);
            this.Controls.Add(this.selectAllButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CustomMobSelectionDlg";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Custom Mob Selection";
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