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
            this.quitParsingMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.openSavedDataMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.continueParsingMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.statusStrip.SuspendLayout();
            this.tabContextMenuStrip.SuspendLayout();
            this.mainMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip
            // 
            this.statusStrip.AccessibleDescription = null;
            this.statusStrip.AccessibleName = null;
            resources.ApplyResources(this.statusStrip, "statusStrip");
            this.statusStrip.BackgroundImage = null;
            this.statusStrip.Font = null;
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip.Name = "statusStrip";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.AccessibleDescription = null;
            this.toolStripStatusLabel.AccessibleName = null;
            resources.ApplyResources(this.toolStripStatusLabel, "toolStripStatusLabel");
            this.toolStripStatusLabel.BackgroundImage = null;
            this.toolStripStatusLabel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            // 
            // pluginTabs
            // 
            this.pluginTabs.AccessibleDescription = null;
            this.pluginTabs.AccessibleName = null;
            resources.ApplyResources(this.pluginTabs, "pluginTabs");
            this.pluginTabs.BackgroundImage = null;
            this.pluginTabs.ContextMenuStrip = this.tabContextMenuStrip;
            this.pluginTabs.Font = null;
            this.pluginTabs.Name = "pluginTabs";
            this.pluginTabs.SelectedIndex = 0;
            this.pluginTabs.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.pluginTabs.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pluginTabs_MouseMove);
            // 
            // tabContextMenuStrip
            // 
            this.tabContextMenuStrip.AccessibleDescription = null;
            this.tabContextMenuStrip.AccessibleName = null;
            resources.ApplyResources(this.tabContextMenuStrip, "tabContextMenuStrip");
            this.tabContextMenuStrip.BackgroundImage = null;
            this.tabContextMenuStrip.Font = null;
            this.tabContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeTabToolStripMenuItem,
            this.closeOtherTabsToolStripMenuItem});
            this.tabContextMenuStrip.Name = "tabContextMenuStrip";
            // 
            // closeTabToolStripMenuItem
            // 
            this.closeTabToolStripMenuItem.AccessibleDescription = null;
            this.closeTabToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.closeTabToolStripMenuItem, "closeTabToolStripMenuItem");
            this.closeTabToolStripMenuItem.BackgroundImage = null;
            this.closeTabToolStripMenuItem.Name = "closeTabToolStripMenuItem";
            this.closeTabToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.closeTabToolStripMenuItem.Click += new System.EventHandler(this.closeTabToolStripMenuItem_Click);
            // 
            // closeOtherTabsToolStripMenuItem
            // 
            this.closeOtherTabsToolStripMenuItem.AccessibleDescription = null;
            this.closeOtherTabsToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.closeOtherTabsToolStripMenuItem, "closeOtherTabsToolStripMenuItem");
            this.closeOtherTabsToolStripMenuItem.BackgroundImage = null;
            this.closeOtherTabsToolStripMenuItem.Name = "closeOtherTabsToolStripMenuItem";
            this.closeOtherTabsToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.closeOtherTabsToolStripMenuItem.Click += new System.EventHandler(this.closeOtherTabsToolStripMenuItem_Click);
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.AccessibleDescription = null;
            this.mainMenuStrip.AccessibleName = null;
            resources.ApplyResources(this.mainMenuStrip, "mainMenuStrip");
            this.mainMenuStrip.BackgroundImage = null;
            this.mainMenuStrip.Font = null;
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenu,
            this.editToolStripMenuItem,
            this.toolsMenu,
            this.windowsMenu});
            this.mainMenuStrip.Name = "mainMenuStrip";
            // 
            // fileMenu
            // 
            this.fileMenu.AccessibleDescription = null;
            this.fileMenu.AccessibleName = null;
            resources.ApplyResources(this.fileMenu, "fileMenu");
            this.fileMenu.BackgroundImage = null;
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
            this.recentFilesToolStripMenuItem,
            this.toolStripSeparator7,
            this.exitMenuItem});
            this.fileMenu.Name = "fileMenu";
            this.fileMenu.ShortcutKeyDisplayString = null;
            this.fileMenu.DropDownOpening += new System.EventHandler(this.fileMenu_Popup);
            // 
            // beginParseAndSaveDataMenuItem
            // 
            this.beginParseAndSaveDataMenuItem.AccessibleDescription = null;
            this.beginParseAndSaveDataMenuItem.AccessibleName = null;
            resources.ApplyResources(this.beginParseAndSaveDataMenuItem, "beginParseAndSaveDataMenuItem");
            this.beginParseAndSaveDataMenuItem.BackgroundImage = null;
            this.beginParseAndSaveDataMenuItem.Name = "beginParseAndSaveDataMenuItem";
            this.beginParseAndSaveDataMenuItem.ShortcutKeyDisplayString = null;
            this.beginParseAndSaveDataMenuItem.Click += new System.EventHandler(this.menuBeginParseWithSave_Click);
            // 
            // beginDefaultParseMenuItem
            // 
            this.beginDefaultParseMenuItem.AccessibleDescription = null;
            this.beginDefaultParseMenuItem.AccessibleName = null;
            resources.ApplyResources(this.beginDefaultParseMenuItem, "beginDefaultParseMenuItem");
            this.beginDefaultParseMenuItem.BackgroundImage = null;
            this.beginDefaultParseMenuItem.Name = "beginDefaultParseMenuItem";
            this.beginDefaultParseMenuItem.ShortcutKeyDisplayString = null;
            this.beginDefaultParseMenuItem.Click += new System.EventHandler(this.menuBeginDefaultParse_Click);
            // 
            // quitParsingMenuItem
            // 
            this.quitParsingMenuItem.AccessibleDescription = null;
            this.quitParsingMenuItem.AccessibleName = null;
            resources.ApplyResources(this.quitParsingMenuItem, "quitParsingMenuItem");
            this.quitParsingMenuItem.BackgroundImage = null;
            this.quitParsingMenuItem.Name = "quitParsingMenuItem";
            this.quitParsingMenuItem.ShortcutKeyDisplayString = null;
            this.quitParsingMenuItem.Click += new System.EventHandler(this.menuStopParse_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.AccessibleDescription = null;
            this.toolStripSeparator1.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            // 
            // openSavedDataMenuItem
            // 
            this.openSavedDataMenuItem.AccessibleDescription = null;
            this.openSavedDataMenuItem.AccessibleName = null;
            resources.ApplyResources(this.openSavedDataMenuItem, "openSavedDataMenuItem");
            this.openSavedDataMenuItem.BackgroundImage = null;
            this.openSavedDataMenuItem.Name = "openSavedDataMenuItem";
            this.openSavedDataMenuItem.ShortcutKeyDisplayString = null;
            this.openSavedDataMenuItem.Click += new System.EventHandler(this.menuOpenSavedData_Click);
            // 
            // continueParsingMenuItem
            // 
            this.continueParsingMenuItem.AccessibleDescription = null;
            this.continueParsingMenuItem.AccessibleName = null;
            resources.ApplyResources(this.continueParsingMenuItem, "continueParsingMenuItem");
            this.continueParsingMenuItem.BackgroundImage = null;
            this.continueParsingMenuItem.Name = "continueParsingMenuItem";
            this.continueParsingMenuItem.ShortcutKeyDisplayString = null;
            this.continueParsingMenuItem.Click += new System.EventHandler(this.menuContinueParse_Click);
            // 
            // saveCurrentDataAsMenuItem
            // 
            this.saveCurrentDataAsMenuItem.AccessibleDescription = null;
            this.saveCurrentDataAsMenuItem.AccessibleName = null;
            resources.ApplyResources(this.saveCurrentDataAsMenuItem, "saveCurrentDataAsMenuItem");
            this.saveCurrentDataAsMenuItem.BackgroundImage = null;
            this.saveCurrentDataAsMenuItem.Name = "saveCurrentDataAsMenuItem";
            this.saveCurrentDataAsMenuItem.ShortcutKeyDisplayString = null;
            this.saveCurrentDataAsMenuItem.Click += new System.EventHandler(this.menuSaveDataAs_Click);
            // 
            // saveReportMenuItem
            // 
            this.saveReportMenuItem.AccessibleDescription = null;
            this.saveReportMenuItem.AccessibleName = null;
            resources.ApplyResources(this.saveReportMenuItem, "saveReportMenuItem");
            this.saveReportMenuItem.BackgroundImage = null;
            this.saveReportMenuItem.Name = "saveReportMenuItem";
            this.saveReportMenuItem.ShortcutKeyDisplayString = null;
            this.saveReportMenuItem.Click += new System.EventHandler(this.menuSaveReport_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.AccessibleDescription = null;
            this.toolStripSeparator2.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator2, "toolStripSeparator2");
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.AccessibleDescription = null;
            this.importToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.importToolStripMenuItem, "importToolStripMenuItem");
            this.importToolStripMenuItem.BackgroundImage = null;
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.importToolStripMenuItem.Click += new System.EventHandler(this.importDatabaseToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.AccessibleDescription = null;
            this.toolStripSeparator4.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator4, "toolStripSeparator4");
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            // 
            // recentFilesToolStripMenuItem
            // 
            this.recentFilesToolStripMenuItem.AccessibleDescription = null;
            this.recentFilesToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.recentFilesToolStripMenuItem, "recentFilesToolStripMenuItem");
            this.recentFilesToolStripMenuItem.BackgroundImage = null;
            this.recentFilesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.noneToolStripMenuItem});
            this.recentFilesToolStripMenuItem.Name = "recentFilesToolStripMenuItem";
            this.recentFilesToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.recentFilesToolStripMenuItem.DropDownOpening += new System.EventHandler(this.recentFilesToolStripMenuItem_DropDownOpening);
            // 
            // noneToolStripMenuItem
            // 
            this.noneToolStripMenuItem.AccessibleDescription = null;
            this.noneToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.noneToolStripMenuItem, "noneToolStripMenuItem");
            this.noneToolStripMenuItem.BackgroundImage = null;
            this.noneToolStripMenuItem.Name = "noneToolStripMenuItem";
            this.noneToolStripMenuItem.ShortcutKeyDisplayString = null;
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.AccessibleDescription = null;
            this.toolStripSeparator7.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator7, "toolStripSeparator7");
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.AccessibleDescription = null;
            this.exitMenuItem.AccessibleName = null;
            resources.ApplyResources(this.exitMenuItem, "exitMenuItem");
            this.exitMenuItem.BackgroundImage = null;
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.ShortcutKeyDisplayString = null;
            this.exitMenuItem.Click += new System.EventHandler(this.menuExit_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.AccessibleDescription = null;
            this.editToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.editToolStripMenuItem, "editToolStripMenuItem");
            this.editToolStripMenuItem.BackgroundImage = null;
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyasTextToolStripMenuItem,
            this.copyasHTMLToolStripMenuItem,
            this.copyTabInfoAsBBCodeToolStripMenuItem,
            this.copyTabasRTFToolStripMenuItem,
            this.toolStripSeparator5,
            this.languageToolStripMenuItem,
            this.toolStripSeparator6,
            this.playerInformationToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.ShortcutKeyDisplayString = null;
            // 
            // copyasTextToolStripMenuItem
            // 
            this.copyasTextToolStripMenuItem.AccessibleDescription = null;
            this.copyasTextToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.copyasTextToolStripMenuItem, "copyasTextToolStripMenuItem");
            this.copyasTextToolStripMenuItem.BackgroundImage = null;
            this.copyasTextToolStripMenuItem.Name = "copyasTextToolStripMenuItem";
            this.copyasTextToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.copyasTextToolStripMenuItem.Click += new System.EventHandler(this.copyTabInfoAsTextToolStripMenuItem_Click);
            // 
            // copyasHTMLToolStripMenuItem
            // 
            this.copyasHTMLToolStripMenuItem.AccessibleDescription = null;
            this.copyasHTMLToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.copyasHTMLToolStripMenuItem, "copyasHTMLToolStripMenuItem");
            this.copyasHTMLToolStripMenuItem.BackgroundImage = null;
            this.copyasHTMLToolStripMenuItem.Name = "copyasHTMLToolStripMenuItem";
            this.copyasHTMLToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.copyasHTMLToolStripMenuItem.Click += new System.EventHandler(this.copyTabInfoAsHTMLToolStripMenuItem_Click);
            // 
            // copyTabInfoAsBBCodeToolStripMenuItem
            // 
            this.copyTabInfoAsBBCodeToolStripMenuItem.AccessibleDescription = null;
            this.copyTabInfoAsBBCodeToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.copyTabInfoAsBBCodeToolStripMenuItem, "copyTabInfoAsBBCodeToolStripMenuItem");
            this.copyTabInfoAsBBCodeToolStripMenuItem.BackgroundImage = null;
            this.copyTabInfoAsBBCodeToolStripMenuItem.Name = "copyTabInfoAsBBCodeToolStripMenuItem";
            this.copyTabInfoAsBBCodeToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.copyTabInfoAsBBCodeToolStripMenuItem.Click += new System.EventHandler(this.copyTabInfoAsBBCodeToolStripMenuItem_Click);
            // 
            // copyTabasRTFToolStripMenuItem
            // 
            this.copyTabasRTFToolStripMenuItem.AccessibleDescription = null;
            this.copyTabasRTFToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.copyTabasRTFToolStripMenuItem, "copyTabasRTFToolStripMenuItem");
            this.copyTabasRTFToolStripMenuItem.BackgroundImage = null;
            this.copyTabasRTFToolStripMenuItem.Name = "copyTabasRTFToolStripMenuItem";
            this.copyTabasRTFToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.copyTabasRTFToolStripMenuItem.Click += new System.EventHandler(this.copyTabInfoAsRTFToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.AccessibleDescription = null;
            this.toolStripSeparator5.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator5, "toolStripSeparator5");
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            // 
            // languageToolStripMenuItem
            // 
            this.languageToolStripMenuItem.AccessibleDescription = null;
            this.languageToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.languageToolStripMenuItem, "languageToolStripMenuItem");
            this.languageToolStripMenuItem.BackgroundImage = null;
            this.languageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.englishToolStripMenuItem,
            this.frenchToolStripMenuItem,
            this.germanToolStripMenuItem,
            this.japaneseToolStripMenuItem});
            this.languageToolStripMenuItem.Name = "languageToolStripMenuItem";
            this.languageToolStripMenuItem.ShortcutKeyDisplayString = null;
            // 
            // englishToolStripMenuItem
            // 
            this.englishToolStripMenuItem.AccessibleDescription = null;
            this.englishToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.englishToolStripMenuItem, "englishToolStripMenuItem");
            this.englishToolStripMenuItem.BackgroundImage = null;
            this.englishToolStripMenuItem.Name = "englishToolStripMenuItem";
            this.englishToolStripMenuItem.ShortcutKeyDisplayString = null;
            // 
            // frenchToolStripMenuItem
            // 
            this.frenchToolStripMenuItem.AccessibleDescription = null;
            this.frenchToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.frenchToolStripMenuItem, "frenchToolStripMenuItem");
            this.frenchToolStripMenuItem.BackgroundImage = null;
            this.frenchToolStripMenuItem.Name = "frenchToolStripMenuItem";
            this.frenchToolStripMenuItem.ShortcutKeyDisplayString = null;
            // 
            // germanToolStripMenuItem
            // 
            this.germanToolStripMenuItem.AccessibleDescription = null;
            this.germanToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.germanToolStripMenuItem, "germanToolStripMenuItem");
            this.germanToolStripMenuItem.BackgroundImage = null;
            this.germanToolStripMenuItem.Name = "germanToolStripMenuItem";
            this.germanToolStripMenuItem.ShortcutKeyDisplayString = null;
            // 
            // japaneseToolStripMenuItem
            // 
            this.japaneseToolStripMenuItem.AccessibleDescription = null;
            this.japaneseToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.japaneseToolStripMenuItem, "japaneseToolStripMenuItem");
            this.japaneseToolStripMenuItem.BackgroundImage = null;
            this.japaneseToolStripMenuItem.Name = "japaneseToolStripMenuItem";
            this.japaneseToolStripMenuItem.ShortcutKeyDisplayString = null;
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.AccessibleDescription = null;
            this.toolStripSeparator6.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator6, "toolStripSeparator6");
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            // 
            // playerInformationToolStripMenuItem
            // 
            this.playerInformationToolStripMenuItem.AccessibleDescription = null;
            this.playerInformationToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.playerInformationToolStripMenuItem, "playerInformationToolStripMenuItem");
            this.playerInformationToolStripMenuItem.BackgroundImage = null;
            this.playerInformationToolStripMenuItem.Name = "playerInformationToolStripMenuItem";
            this.playerInformationToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.playerInformationToolStripMenuItem.Click += new System.EventHandler(this.playerInformationToolStripMenuItem_Click);
            // 
            // toolsMenu
            // 
            this.toolsMenu.AccessibleDescription = null;
            this.toolsMenu.AccessibleName = null;
            resources.ApplyResources(this.toolsMenu, "toolsMenu");
            this.toolsMenu.BackgroundImage = null;
            this.toolsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolsTestFunctionMenuItem,
            this.toolsReparseMenuItem,
            this.upgradeDatabaseTimestampToolStripMenuItem,
            this.toolStripSeparator3,
            this.optionsToolStripMenuItem});
            this.toolsMenu.Name = "toolsMenu";
            this.toolsMenu.ShortcutKeyDisplayString = null;
            this.toolsMenu.DropDownOpening += new System.EventHandler(this.toolsMenu_Popup);
            // 
            // toolsTestFunctionMenuItem
            // 
            this.toolsTestFunctionMenuItem.AccessibleDescription = null;
            this.toolsTestFunctionMenuItem.AccessibleName = null;
            resources.ApplyResources(this.toolsTestFunctionMenuItem, "toolsTestFunctionMenuItem");
            this.toolsTestFunctionMenuItem.BackgroundImage = null;
            this.toolsTestFunctionMenuItem.Name = "toolsTestFunctionMenuItem";
            this.toolsTestFunctionMenuItem.ShortcutKeyDisplayString = null;
            this.toolsTestFunctionMenuItem.Click += new System.EventHandler(this.menuTestItem_Click);
            // 
            // toolsReparseMenuItem
            // 
            this.toolsReparseMenuItem.AccessibleDescription = null;
            this.toolsReparseMenuItem.AccessibleName = null;
            resources.ApplyResources(this.toolsReparseMenuItem, "toolsReparseMenuItem");
            this.toolsReparseMenuItem.BackgroundImage = null;
            this.toolsReparseMenuItem.Name = "toolsReparseMenuItem";
            this.toolsReparseMenuItem.ShortcutKeyDisplayString = null;
            this.toolsReparseMenuItem.Click += new System.EventHandler(this.reparseDatabaseToolStripMenuItem_Click);
            // 
            // upgradeDatabaseTimestampToolStripMenuItem
            // 
            this.upgradeDatabaseTimestampToolStripMenuItem.AccessibleDescription = null;
            this.upgradeDatabaseTimestampToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.upgradeDatabaseTimestampToolStripMenuItem, "upgradeDatabaseTimestampToolStripMenuItem");
            this.upgradeDatabaseTimestampToolStripMenuItem.BackgroundImage = null;
            this.upgradeDatabaseTimestampToolStripMenuItem.Name = "upgradeDatabaseTimestampToolStripMenuItem";
            this.upgradeDatabaseTimestampToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.upgradeDatabaseTimestampToolStripMenuItem.Click += new System.EventHandler(this.upgradeDatabaseTimestampToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.AccessibleDescription = null;
            this.toolStripSeparator3.AccessibleName = null;
            resources.ApplyResources(this.toolStripSeparator3, "toolStripSeparator3");
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.AccessibleDescription = null;
            this.optionsToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.optionsToolStripMenuItem, "optionsToolStripMenuItem");
            this.optionsToolStripMenuItem.BackgroundImage = null;
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.menuOptions_Click);
            // 
            // windowsMenu
            // 
            this.windowsMenu.AccessibleDescription = null;
            this.windowsMenu.AccessibleName = null;
            resources.ApplyResources(this.windowsMenu, "windowsMenu");
            this.windowsMenu.BackgroundImage = null;
            this.windowsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem,
            this.windowsToolStripSeparator,
            this.closeAllTabsToolStripMenuItem});
            this.windowsMenu.Name = "windowsMenu";
            this.windowsMenu.ShortcutKeyDisplayString = null;
            this.windowsMenu.DropDownOpening += new System.EventHandler(this.windowsMenu_Popup);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.AccessibleDescription = null;
            this.aboutToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.aboutToolStripMenuItem, "aboutToolStripMenuItem");
            this.aboutToolStripMenuItem.BackgroundImage = null;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.menuAbout_Click);
            // 
            // windowsToolStripSeparator
            // 
            this.windowsToolStripSeparator.AccessibleDescription = null;
            this.windowsToolStripSeparator.AccessibleName = null;
            resources.ApplyResources(this.windowsToolStripSeparator, "windowsToolStripSeparator");
            this.windowsToolStripSeparator.Name = "windowsToolStripSeparator";
            // 
            // closeAllTabsToolStripMenuItem
            // 
            this.closeAllTabsToolStripMenuItem.AccessibleDescription = null;
            this.closeAllTabsToolStripMenuItem.AccessibleName = null;
            resources.ApplyResources(this.closeAllTabsToolStripMenuItem, "closeAllTabsToolStripMenuItem");
            this.closeAllTabsToolStripMenuItem.BackgroundImage = null;
            this.closeAllTabsToolStripMenuItem.Name = "closeAllTabsToolStripMenuItem";
            this.closeAllTabsToolStripMenuItem.ShortcutKeyDisplayString = null;
            this.closeAllTabsToolStripMenuItem.Click += new System.EventHandler(this.closeAllTabsToolStripMenuItem_Click);
            // 
            // ParserWindow
            // 
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImage = null;
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.mainMenuStrip);
            this.Controls.Add(this.pluginTabs);
            this.Font = null;
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
    }
}