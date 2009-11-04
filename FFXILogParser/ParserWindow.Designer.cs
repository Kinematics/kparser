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
            this.pluginTabs = new System.Windows.Forms.TabControl();
            this.tabContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.closeTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeOtherTabsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.beginParseAndSaveDataMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.beginDefaultParseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.continueParsingMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitParsingMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.openSavedDataMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveCurrentDataAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveReportMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.recentFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.noneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyasTextToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyasHTMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyTabInfoAsBBCodeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyTabasRTFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.languageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.englishToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.frenchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.germanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.japaneseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.playerInformationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsTestFunctionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsReparseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.upgradeDatabaseTimestampToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowsToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.closeAllTabsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showJobInsteadOfNameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip.SuspendLayout();
            this.tabContextMenuStrip.SuspendLayout();
            this.mainMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            resources.ApplyResources(this.statusStrip, "statusStrip");
            this.statusStrip.Name = "statusStrip";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            resources.ApplyResources(this.toolStripStatusLabel, "toolStripStatusLabel");
            // 
            // pluginTabs
            // 
            resources.ApplyResources(this.pluginTabs, "pluginTabs");
            this.pluginTabs.ContextMenuStrip = this.tabContextMenuStrip;
            this.pluginTabs.Name = "pluginTabs";
            this.pluginTabs.SelectedIndex = 0;
            this.pluginTabs.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.pluginTabs.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pluginTabs_MouseMove);
            // 
            // tabContextMenuStrip
            // 
            this.tabContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeTabToolStripMenuItem,
            this.closeOtherTabsToolStripMenuItem});
            this.tabContextMenuStrip.Name = "tabContextMenuStrip";
            resources.ApplyResources(this.tabContextMenuStrip, "tabContextMenuStrip");
            // 
            // closeTabToolStripMenuItem
            // 
            this.closeTabToolStripMenuItem.Name = "closeTabToolStripMenuItem";
            resources.ApplyResources(this.closeTabToolStripMenuItem, "closeTabToolStripMenuItem");
            this.closeTabToolStripMenuItem.Click += new System.EventHandler(this.closeTabToolStripMenuItem_Click);
            // 
            // closeOtherTabsToolStripMenuItem
            // 
            this.closeOtherTabsToolStripMenuItem.Name = "closeOtherTabsToolStripMenuItem";
            resources.ApplyResources(this.closeOtherTabsToolStripMenuItem, "closeOtherTabsToolStripMenuItem");
            this.closeOtherTabsToolStripMenuItem.Click += new System.EventHandler(this.closeOtherTabsToolStripMenuItem_Click);
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenu,
            this.editToolStripMenuItem,
            this.toolsMenu,
            this.windowsMenu});
            resources.ApplyResources(this.mainMenuStrip, "mainMenuStrip");
            this.mainMenuStrip.Name = "mainMenuStrip";
            // 
            // fileMenu
            // 
            this.fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.beginParseAndSaveDataMenuItem,
            this.beginDefaultParseMenuItem,
            this.continueParsingMenuItem,
            this.quitParsingMenuItem,
            this.toolStripSeparator1,
            this.openSavedDataMenuItem,
            this.saveCurrentDataAsMenuItem,
            this.saveReportMenuItem,
            this.toolStripSeparator2,
            this.importToolStripMenuItem,
            this.toolStripSeparator4,
            this.recentFilesToolStripMenuItem,
            this.toolStripSeparator7,
            this.exitMenuItem});
            this.fileMenu.Name = "fileMenu";
            resources.ApplyResources(this.fileMenu, "fileMenu");
            this.fileMenu.DropDownOpening += new System.EventHandler(this.fileMenu_Popup);
            // 
            // beginParseAndSaveDataMenuItem
            // 
            this.beginParseAndSaveDataMenuItem.Name = "beginParseAndSaveDataMenuItem";
            resources.ApplyResources(this.beginParseAndSaveDataMenuItem, "beginParseAndSaveDataMenuItem");
            this.beginParseAndSaveDataMenuItem.Click += new System.EventHandler(this.menuBeginParseWithSave_Click);
            // 
            // beginDefaultParseMenuItem
            // 
            this.beginDefaultParseMenuItem.Name = "beginDefaultParseMenuItem";
            resources.ApplyResources(this.beginDefaultParseMenuItem, "beginDefaultParseMenuItem");
            this.beginDefaultParseMenuItem.Click += new System.EventHandler(this.menuBeginDefaultParse_Click);
            // 
            // continueParsingMenuItem
            // 
            this.continueParsingMenuItem.Name = "continueParsingMenuItem";
            resources.ApplyResources(this.continueParsingMenuItem, "continueParsingMenuItem");
            this.continueParsingMenuItem.Click += new System.EventHandler(this.menuContinueParse_Click);
            // 
            // quitParsingMenuItem
            // 
            this.quitParsingMenuItem.Name = "quitParsingMenuItem";
            resources.ApplyResources(this.quitParsingMenuItem, "quitParsingMenuItem");
            this.quitParsingMenuItem.Click += new System.EventHandler(this.menuStopParse_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            // 
            // openSavedDataMenuItem
            // 
            this.openSavedDataMenuItem.Name = "openSavedDataMenuItem";
            resources.ApplyResources(this.openSavedDataMenuItem, "openSavedDataMenuItem");
            this.openSavedDataMenuItem.Click += new System.EventHandler(this.menuOpenSavedData_Click);
            // 
            // saveCurrentDataAsMenuItem
            // 
            this.saveCurrentDataAsMenuItem.Name = "saveCurrentDataAsMenuItem";
            resources.ApplyResources(this.saveCurrentDataAsMenuItem, "saveCurrentDataAsMenuItem");
            this.saveCurrentDataAsMenuItem.Click += new System.EventHandler(this.menuSaveDataAs_Click);
            // 
            // saveReportMenuItem
            // 
            this.saveReportMenuItem.Name = "saveReportMenuItem";
            resources.ApplyResources(this.saveReportMenuItem, "saveReportMenuItem");
            this.saveReportMenuItem.Click += new System.EventHandler(this.menuSaveReport_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            resources.ApplyResources(this.importToolStripMenuItem, "importToolStripMenuItem");
            this.importToolStripMenuItem.Click += new System.EventHandler(this.importDatabaseToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            // 
            // recentFilesToolStripMenuItem
            // 
            this.recentFilesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.noneToolStripMenuItem});
            this.recentFilesToolStripMenuItem.Name = "recentFilesToolStripMenuItem";
            resources.ApplyResources(this.recentFilesToolStripMenuItem, "recentFilesToolStripMenuItem");
            this.recentFilesToolStripMenuItem.DropDownOpening += new System.EventHandler(this.recentFilesToolStripMenuItem_DropDownOpening);
            // 
            // noneToolStripMenuItem
            // 
            resources.ApplyResources(this.noneToolStripMenuItem, "noneToolStripMenuItem");
            this.noneToolStripMenuItem.Name = "noneToolStripMenuItem";
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            resources.ApplyResources(this.toolStripSeparator7, "toolStripSeparator7");
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            resources.ApplyResources(this.exitMenuItem, "exitMenuItem");
            this.exitMenuItem.Click += new System.EventHandler(this.menuExit_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyasTextToolStripMenuItem,
            this.copyasHTMLToolStripMenuItem,
            this.copyTabInfoAsBBCodeToolStripMenuItem,
            this.copyTabasRTFToolStripMenuItem,
            this.toolStripSeparator5,
            this.languageToolStripMenuItem,
            this.toolStripSeparator6,
            this.playerInformationToolStripMenuItem,
            this.showJobInsteadOfNameToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            resources.ApplyResources(this.editToolStripMenuItem, "editToolStripMenuItem");
            this.editToolStripMenuItem.DropDownOpening += new System.EventHandler(this.editToolStripMenuItem_DropDownOpening);
            // 
            // copyasTextToolStripMenuItem
            // 
            this.copyasTextToolStripMenuItem.Name = "copyasTextToolStripMenuItem";
            resources.ApplyResources(this.copyasTextToolStripMenuItem, "copyasTextToolStripMenuItem");
            this.copyasTextToolStripMenuItem.Click += new System.EventHandler(this.copyTabInfoAsTextToolStripMenuItem_Click);
            // 
            // copyasHTMLToolStripMenuItem
            // 
            this.copyasHTMLToolStripMenuItem.Name = "copyasHTMLToolStripMenuItem";
            resources.ApplyResources(this.copyasHTMLToolStripMenuItem, "copyasHTMLToolStripMenuItem");
            this.copyasHTMLToolStripMenuItem.Click += new System.EventHandler(this.copyTabInfoAsHTMLToolStripMenuItem_Click);
            // 
            // copyTabInfoAsBBCodeToolStripMenuItem
            // 
            this.copyTabInfoAsBBCodeToolStripMenuItem.Name = "copyTabInfoAsBBCodeToolStripMenuItem";
            resources.ApplyResources(this.copyTabInfoAsBBCodeToolStripMenuItem, "copyTabInfoAsBBCodeToolStripMenuItem");
            this.copyTabInfoAsBBCodeToolStripMenuItem.Click += new System.EventHandler(this.copyTabInfoAsBBCodeToolStripMenuItem_Click);
            // 
            // copyTabasRTFToolStripMenuItem
            // 
            this.copyTabasRTFToolStripMenuItem.Name = "copyTabasRTFToolStripMenuItem";
            resources.ApplyResources(this.copyTabasRTFToolStripMenuItem, "copyTabasRTFToolStripMenuItem");
            this.copyTabasRTFToolStripMenuItem.Click += new System.EventHandler(this.copyTabInfoAsRTFToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
            // 
            // languageToolStripMenuItem
            // 
            this.languageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.englishToolStripMenuItem,
            this.frenchToolStripMenuItem,
            this.germanToolStripMenuItem,
            this.japaneseToolStripMenuItem});
            this.languageToolStripMenuItem.Name = "languageToolStripMenuItem";
            resources.ApplyResources(this.languageToolStripMenuItem, "languageToolStripMenuItem");
            // 
            // englishToolStripMenuItem
            // 
            this.englishToolStripMenuItem.Name = "englishToolStripMenuItem";
            resources.ApplyResources(this.englishToolStripMenuItem, "englishToolStripMenuItem");
            this.englishToolStripMenuItem.Click += new System.EventHandler(this.englishToolStripMenuItem_Click);
            // 
            // frenchToolStripMenuItem
            // 
            this.frenchToolStripMenuItem.Name = "frenchToolStripMenuItem";
            resources.ApplyResources(this.frenchToolStripMenuItem, "frenchToolStripMenuItem");
            this.frenchToolStripMenuItem.Click += new System.EventHandler(this.frenchToolStripMenuItem_Click);
            // 
            // germanToolStripMenuItem
            // 
            this.germanToolStripMenuItem.Name = "germanToolStripMenuItem";
            resources.ApplyResources(this.germanToolStripMenuItem, "germanToolStripMenuItem");
            this.germanToolStripMenuItem.Click += new System.EventHandler(this.germanToolStripMenuItem_Click);
            // 
            // japaneseToolStripMenuItem
            // 
            this.japaneseToolStripMenuItem.Name = "japaneseToolStripMenuItem";
            resources.ApplyResources(this.japaneseToolStripMenuItem, "japaneseToolStripMenuItem");
            this.japaneseToolStripMenuItem.Click += new System.EventHandler(this.japaneseToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            resources.ApplyResources(this.toolStripSeparator6, "toolStripSeparator6");
            // 
            // playerInformationToolStripMenuItem
            // 
            this.playerInformationToolStripMenuItem.Name = "playerInformationToolStripMenuItem";
            resources.ApplyResources(this.playerInformationToolStripMenuItem, "playerInformationToolStripMenuItem");
            this.playerInformationToolStripMenuItem.Click += new System.EventHandler(this.playerInformationToolStripMenuItem_Click);
            // 
            // toolsMenu
            // 
            this.toolsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolsTestFunctionMenuItem,
            this.toolsReparseMenuItem,
            this.upgradeDatabaseTimestampToolStripMenuItem,
            this.toolStripSeparator3,
            this.optionsToolStripMenuItem});
            this.toolsMenu.Name = "toolsMenu";
            resources.ApplyResources(this.toolsMenu, "toolsMenu");
            this.toolsMenu.DropDownOpening += new System.EventHandler(this.toolsMenu_Popup);
            // 
            // toolsTestFunctionMenuItem
            // 
            this.toolsTestFunctionMenuItem.Name = "toolsTestFunctionMenuItem";
            resources.ApplyResources(this.toolsTestFunctionMenuItem, "toolsTestFunctionMenuItem");
            this.toolsTestFunctionMenuItem.Click += new System.EventHandler(this.menuTestItem_Click);
            // 
            // toolsReparseMenuItem
            // 
            this.toolsReparseMenuItem.Name = "toolsReparseMenuItem";
            resources.ApplyResources(this.toolsReparseMenuItem, "toolsReparseMenuItem");
            this.toolsReparseMenuItem.Click += new System.EventHandler(this.reparseDatabaseToolStripMenuItem_Click);
            // 
            // upgradeDatabaseTimestampToolStripMenuItem
            // 
            this.upgradeDatabaseTimestampToolStripMenuItem.Name = "upgradeDatabaseTimestampToolStripMenuItem";
            resources.ApplyResources(this.upgradeDatabaseTimestampToolStripMenuItem, "upgradeDatabaseTimestampToolStripMenuItem");
            this.upgradeDatabaseTimestampToolStripMenuItem.Click += new System.EventHandler(this.upgradeDatabaseTimestampToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            resources.ApplyResources(this.optionsToolStripMenuItem, "optionsToolStripMenuItem");
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.menuOptions_Click);
            // 
            // windowsMenu
            // 
            this.windowsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem,
            this.windowsToolStripSeparator,
            this.closeAllTabsToolStripMenuItem});
            this.windowsMenu.Name = "windowsMenu";
            resources.ApplyResources(this.windowsMenu, "windowsMenu");
            this.windowsMenu.DropDownOpening += new System.EventHandler(this.windowsMenu_Popup);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            resources.ApplyResources(this.aboutToolStripMenuItem, "aboutToolStripMenuItem");
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.menuAbout_Click);
            // 
            // windowsToolStripSeparator
            // 
            this.windowsToolStripSeparator.Name = "windowsToolStripSeparator";
            resources.ApplyResources(this.windowsToolStripSeparator, "windowsToolStripSeparator");
            // 
            // closeAllTabsToolStripMenuItem
            // 
            this.closeAllTabsToolStripMenuItem.Name = "closeAllTabsToolStripMenuItem";
            resources.ApplyResources(this.closeAllTabsToolStripMenuItem, "closeAllTabsToolStripMenuItem");
            this.closeAllTabsToolStripMenuItem.Click += new System.EventHandler(this.closeAllTabsToolStripMenuItem_Click);
            // 
            // showJobInsteadOfNameToolStripMenuItem
            // 
            this.showJobInsteadOfNameToolStripMenuItem.CheckOnClick = true;
            this.showJobInsteadOfNameToolStripMenuItem.Name = "showJobInsteadOfNameToolStripMenuItem";
            resources.ApplyResources(this.showJobInsteadOfNameToolStripMenuItem, "showJobInsteadOfNameToolStripMenuItem");
            this.showJobInsteadOfNameToolStripMenuItem.CheckedChanged += new System.EventHandler(this.showJobInsteadOfNameToolStripMenuItem_CheckedChanged);
            // 
            // ParserWindow
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.mainMenuStrip);
            this.Controls.Add(this.pluginTabs);
            this.MainMenuStrip = this.mainMenuStrip;
            this.Name = "ParserWindow";
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
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyasTextToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyasHTMLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem playerInformationToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem copyTabasRTFToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyTabInfoAsBBCodeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem upgradeDatabaseTimestampToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeAllTabsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem recentFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem noneToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem languageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem englishToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem frenchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem germanToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem japaneseToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem showJobInsteadOfNameToolStripMenuItem;
    }
}