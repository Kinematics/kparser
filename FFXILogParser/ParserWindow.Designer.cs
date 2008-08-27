namespace WaywardGamers.KParser
{
    partial class ParserWindow
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
            Shutdown();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ParserWindow));
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.pluginTabs = new System.Windows.Forms.TabControl();
            this.tabContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.closeTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeOtherTabsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.beginParseAndSaveDataMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.beginDefaultParseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitParsingMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.openSavedDataMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.continueParsingMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveCurrentDataAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveReportMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsTestFunctionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsReparseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.playerInformationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowsToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.statusStrip.SuspendLayout();
            this.tabContextMenuStrip.SuspendLayout();
            this.mainMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 435);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(992, 22);
            this.statusStrip.TabIndex = 2;
            this.statusStrip.Text = "statusStrip";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(38, 17);
            this.toolStripStatusLabel.Text = "Status";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.BackColor = System.Drawing.SystemColors.Control;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(66, 17);
            this.toolStripStatusLabel1.Text = "Status Label";
            // 
            // pluginTabs
            // 
            this.pluginTabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pluginTabs.ContextMenuStrip = this.tabContextMenuStrip;
            this.pluginTabs.ItemSize = new System.Drawing.Size(80, 21);
            this.pluginTabs.Location = new System.Drawing.Point(0, 24);
            this.pluginTabs.Name = "pluginTabs";
            this.pluginTabs.SelectedIndex = 0;
            this.pluginTabs.Size = new System.Drawing.Size(994, 412);
            this.pluginTabs.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.pluginTabs.TabIndex = 3;
            this.pluginTabs.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pluginTabs_MouseMove);
            // 
            // tabContextMenuStrip
            // 
            this.tabContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeTabToolStripMenuItem,
            this.closeOtherTabsToolStripMenuItem});
            this.tabContextMenuStrip.Name = "tabContextMenuStrip";
            this.tabContextMenuStrip.Size = new System.Drawing.Size(169, 48);
            // 
            // closeTabToolStripMenuItem
            // 
            this.closeTabToolStripMenuItem.Name = "closeTabToolStripMenuItem";
            this.closeTabToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.closeTabToolStripMenuItem.Text = "Close Tab";
            this.closeTabToolStripMenuItem.Click += new System.EventHandler(this.closeTabToolStripMenuItem_Click);
            // 
            // closeOtherTabsToolStripMenuItem
            // 
            this.closeOtherTabsToolStripMenuItem.Name = "closeOtherTabsToolStripMenuItem";
            this.closeOtherTabsToolStripMenuItem.Size = new System.Drawing.Size(168, 22);
            this.closeOtherTabsToolStripMenuItem.Text = "Close Other Tabs";
            this.closeOtherTabsToolStripMenuItem.Click += new System.EventHandler(this.closeOtherTabsToolStripMenuItem_Click);
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenu,
            this.toolsMenu,
            this.windowsMenu});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Size = new System.Drawing.Size(992, 24);
            this.mainMenuStrip.TabIndex = 5;
            this.mainMenuStrip.Text = "Main Menu Strip";
            // 
            // fileMenu
            // 
            this.fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.beginParseAndSaveDataMenuItem,
            this.beginDefaultParseMenuItem,
            this.quitParsingMenuItem,
            this.toolStripSeparator1,
            this.openSavedDataMenuItem,
            this.continueParsingMenuItem,
            this.saveCurrentDataAsMenuItem,
            this.saveReportMenuItem,
            this.toolStripSeparator2,
            this.importToolStripMenuItem,
            this.toolStripSeparator4,
            this.exitMenuItem});
            this.fileMenu.Name = "fileMenu";
            this.fileMenu.Size = new System.Drawing.Size(35, 20);
            this.fileMenu.Text = "File";
            this.fileMenu.DropDownOpening += new System.EventHandler(this.fileMenu_Popup);
            // 
            // beginParseAndSaveDataMenuItem
            // 
            this.beginParseAndSaveDataMenuItem.Name = "beginParseAndSaveDataMenuItem";
            this.beginParseAndSaveDataMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.beginParseAndSaveDataMenuItem.Size = new System.Drawing.Size(265, 22);
            this.beginParseAndSaveDataMenuItem.Text = "Begin &Parse and Save Data...";
            this.beginParseAndSaveDataMenuItem.Click += new System.EventHandler(this.menuBeginParseWithSave_Click);
            // 
            // beginDefaultParseMenuItem
            // 
            this.beginDefaultParseMenuItem.Name = "beginDefaultParseMenuItem";
            this.beginDefaultParseMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.beginDefaultParseMenuItem.Size = new System.Drawing.Size(265, 22);
            this.beginDefaultParseMenuItem.Text = "Begin &Default Parse";
            this.beginDefaultParseMenuItem.Click += new System.EventHandler(this.menuBeginDefaultParse_Click);
            // 
            // quitParsingMenuItem
            // 
            this.quitParsingMenuItem.Name = "quitParsingMenuItem";
            this.quitParsingMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.quitParsingMenuItem.Size = new System.Drawing.Size(265, 22);
            this.quitParsingMenuItem.Text = "&Quit Parsing";
            this.quitParsingMenuItem.Click += new System.EventHandler(this.menuStopParse_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(262, 6);
            // 
            // openSavedDataMenuItem
            // 
            this.openSavedDataMenuItem.Name = "openSavedDataMenuItem";
            this.openSavedDataMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openSavedDataMenuItem.Size = new System.Drawing.Size(265, 22);
            this.openSavedDataMenuItem.Text = "&Open Saved Data";
            this.openSavedDataMenuItem.Click += new System.EventHandler(this.menuOpenSavedData_Click);
            // 
            // continueParsingMenuItem
            // 
            this.continueParsingMenuItem.Name = "continueParsingMenuItem";
            this.continueParsingMenuItem.Size = new System.Drawing.Size(265, 22);
            this.continueParsingMenuItem.Text = "Continue Parsing";
            this.continueParsingMenuItem.Click += new System.EventHandler(this.menuContinueParse_Click);
            // 
            // saveCurrentDataAsMenuItem
            // 
            this.saveCurrentDataAsMenuItem.Name = "saveCurrentDataAsMenuItem";
            this.saveCurrentDataAsMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveCurrentDataAsMenuItem.Size = new System.Drawing.Size(265, 22);
            this.saveCurrentDataAsMenuItem.Text = "&Save Current Data As...";
            this.saveCurrentDataAsMenuItem.Click += new System.EventHandler(this.menuSaveDataAs_Click);
            // 
            // saveReportMenuItem
            // 
            this.saveReportMenuItem.Name = "saveReportMenuItem";
            this.saveReportMenuItem.Size = new System.Drawing.Size(265, 22);
            this.saveReportMenuItem.Text = "Save Report...";
            this.saveReportMenuItem.Click += new System.EventHandler(this.menuSaveReport_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(262, 6);
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(265, 22);
            this.importToolStripMenuItem.Text = "Import...";
            this.importToolStripMenuItem.Click += new System.EventHandler(this.importToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(262, 6);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.exitMenuItem.Size = new System.Drawing.Size(265, 22);
            this.exitMenuItem.Text = "E&xit";
            this.exitMenuItem.Click += new System.EventHandler(this.menuExit_Click);
            // 
            // toolsMenu
            // 
            this.toolsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolsTestFunctionMenuItem,
            this.toolsReparseMenuItem,
            this.toolStripSeparator3,
            this.playerInformationToolStripMenuItem,
            this.optionsToolStripMenuItem});
            this.toolsMenu.Name = "toolsMenu";
            this.toolsMenu.Size = new System.Drawing.Size(44, 20);
            this.toolsMenu.Text = "Tools";
            this.toolsMenu.DropDownOpening += new System.EventHandler(this.toolsMenu_Popup);
            // 
            // toolsTestFunctionMenuItem
            // 
            this.toolsTestFunctionMenuItem.Name = "toolsTestFunctionMenuItem";
            this.toolsTestFunctionMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.toolsTestFunctionMenuItem.Size = new System.Drawing.Size(258, 22);
            this.toolsTestFunctionMenuItem.Text = "Test Function";
            this.toolsTestFunctionMenuItem.Click += new System.EventHandler(this.menuTestItem_Click);
            // 
            // toolsReparseMenuItem
            // 
            this.toolsReparseMenuItem.Name = "toolsReparseMenuItem";
            this.toolsReparseMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.U)));
            this.toolsReparseMenuItem.Size = new System.Drawing.Size(258, 22);
            this.toolsReparseMenuItem.Text = "Reparse/&Upgrade Database";
            this.toolsReparseMenuItem.Click += new System.EventHandler(this.databaseReparse_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(255, 6);
            // 
            // playerInformationToolStripMenuItem
            // 
            this.playerInformationToolStripMenuItem.Name = "playerInformationToolStripMenuItem";
            this.playerInformationToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.playerInformationToolStripMenuItem.Size = new System.Drawing.Size(258, 22);
            this.playerInformationToolStripMenuItem.Text = "Player &Information";
            this.playerInformationToolStripMenuItem.Click += new System.EventHandler(this.playerInformationToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(258, 22);
            this.optionsToolStripMenuItem.Text = "Options...";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.menuOptions_Click);
            // 
            // windowsMenu
            // 
            this.windowsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem,
            this.windowsToolStripSeparator});
            this.windowsMenu.Name = "windowsMenu";
            this.windowsMenu.Size = new System.Drawing.Size(62, 20);
            this.windowsMenu.Text = "Windows";
            this.windowsMenu.DropDownOpening += new System.EventHandler(this.windowsMenu_Popup);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.menuAbout_Click);
            // 
            // windowsToolStripSeparator
            // 
            this.windowsToolStripSeparator.Name = "windowsToolStripSeparator";
            this.windowsToolStripSeparator.Size = new System.Drawing.Size(149, 6);
            // 
            // ParserWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(992, 457);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.mainMenuStrip);
            this.Controls.Add(this.pluginTabs);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mainMenuStrip;
            this.Name = "ParserWindow";
            this.Text = "K-Parser";
            this.Load += new System.EventHandler(this.ParserWindow_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ParserWindow_FormClosing);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.tabContextMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.TabControl pluginTabs;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ContextMenuStrip tabContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem closeTabToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeOtherTabsToolStripMenuItem;
        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileMenu;
        private System.Windows.Forms.ToolStripMenuItem toolsMenu;
        private System.Windows.Forms.ToolStripMenuItem toolsTestFunctionMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsReparseMenuItem;
        private System.Windows.Forms.ToolStripMenuItem windowsMenu;
        private System.Windows.Forms.ToolStripMenuItem beginParseAndSaveDataMenuItem;
        private System.Windows.Forms.ToolStripMenuItem beginDefaultParseMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitParsingMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem openSavedDataMenuItem;
        private System.Windows.Forms.ToolStripMenuItem continueParsingMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveCurrentDataAsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveReportMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator windowsToolStripSeparator;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem playerInformationToolStripMenuItem;
    }
}