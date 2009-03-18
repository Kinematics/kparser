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
            this.mobList = new System.Windows.Forms.CheckedListBox();
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
            this.mobListContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // mobList
            // 
            this.mobList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mobList.CheckOnClick = true;
            this.mobList.ContextMenuStrip = this.mobListContextMenuStrip;
            this.mobList.FormattingEnabled = true;
            this.mobList.Location = new System.Drawing.Point(12, 12);
            this.mobList.Name = "mobList";
            this.mobList.Size = new System.Drawing.Size(237, 289);
            this.mobList.TabIndex = 0;
            this.mobList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.mobList_ItemCheck);
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
            this.selectAllButton.Location = new System.Drawing.Point(13, 307);
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
            this.selectNoneButton.Location = new System.Drawing.Point(174, 307);
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
            this.closeButton.Location = new System.Drawing.Point(174, 337);
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
            this.invertSelectionButton.Location = new System.Drawing.Point(94, 307);
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
            this.uncheck0XPMobs.Location = new System.Drawing.Point(13, 336);
            this.uncheck0XPMobs.Name = "uncheck0XPMobs";
            this.uncheck0XPMobs.Size = new System.Drawing.Size(121, 23);
            this.uncheck0XPMobs.TabIndex = 8;
            this.uncheck0XPMobs.Text = "Uncheck 0 XP Mobs";
            this.uncheck0XPMobs.UseVisualStyleBackColor = true;
            this.uncheck0XPMobs.Click += new System.EventHandler(this.uncheck0XPMobs_Click);
            // 
            // CustomMobSelectionDlg
            // 
            this.AcceptButton = this.closeButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(262, 372);
            this.Controls.Add(this.invertSelectionButton);
            this.Controls.Add(this.uncheck0XPMobs);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.selectNoneButton);
            this.Controls.Add(this.selectAllButton);
            this.Controls.Add(this.mobList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CustomMobSelectionDlg";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Custom Mob Selection";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CustomMobSelectionDlg_FormClosing);
            this.mobListContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckedListBox mobList;
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
    }
}