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
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.mainMenu = new System.Windows.Forms.MainMenu(this.components);
            this.fileMenu = new System.Windows.Forms.MenuItem();
            this.menuBeginParseWithSave = new System.Windows.Forms.MenuItem();
            this.menuBeginDefaultParse = new System.Windows.Forms.MenuItem();
            this.menuStopParse = new System.Windows.Forms.MenuItem();
            this.menuSeparator1 = new System.Windows.Forms.MenuItem();
            this.menuOpenSavedData = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuSaveDataAs = new System.Windows.Forms.MenuItem();
            this.menuSaveReport = new System.Windows.Forms.MenuItem();
            this.menuSeparator2 = new System.Windows.Forms.MenuItem();
            this.menuExit = new System.Windows.Forms.MenuItem();
            this.toolsMenu = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.databaseToolsMenu = new System.Windows.Forms.MenuItem();
            this.databaseClearData = new System.Windows.Forms.MenuItem();
            this.databaseUpgrade = new System.Windows.Forms.MenuItem();
            this.databaseReparse = new System.Windows.Forms.MenuItem();
            this.menuSeparator3 = new System.Windows.Forms.MenuItem();
            this.menuOptions = new System.Windows.Forms.MenuItem();
            this.windowMenu = new System.Windows.Forms.MenuItem();
            this.menuAbout = new System.Windows.Forms.MenuItem();
            this.pluginTabs = new System.Windows.Forms.TabControl();
            this.statusStrip.SuspendLayout();
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
            // mainMenu
            // 
            this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.fileMenu,
            this.toolsMenu,
            this.windowMenu});
            // 
            // fileMenu
            // 
            this.fileMenu.Index = 0;
            this.fileMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuBeginParseWithSave,
            this.menuBeginDefaultParse,
            this.menuStopParse,
            this.menuSeparator1,
            this.menuOpenSavedData,
            this.menuItem3,
            this.menuSaveDataAs,
            this.menuSaveReport,
            this.menuSeparator2,
            this.menuExit});
            this.fileMenu.Text = "&File";
            // 
            // menuBeginParseWithSave
            // 
            this.menuBeginParseWithSave.Index = 0;
            this.menuBeginParseWithSave.Shortcut = System.Windows.Forms.Shortcut.CtrlP;
            this.menuBeginParseWithSave.Text = "Begin &Parse and Save Data ...";
            this.menuBeginParseWithSave.Click += new System.EventHandler(this.menuBeginParseWithSave_Click);
            // 
            // menuBeginDefaultParse
            // 
            this.menuBeginDefaultParse.Index = 1;
            this.menuBeginDefaultParse.Shortcut = System.Windows.Forms.Shortcut.CtrlD;
            this.menuBeginDefaultParse.Text = "Begin &Default Parse";
            this.menuBeginDefaultParse.Click += new System.EventHandler(this.menuBeginDefaultParse_Click);
            // 
            // menuStopParse
            // 
            this.menuStopParse.Enabled = false;
            this.menuStopParse.Index = 2;
            this.menuStopParse.Shortcut = System.Windows.Forms.Shortcut.CtrlQ;
            this.menuStopParse.Text = "&Quit Parsing";
            this.menuStopParse.Click += new System.EventHandler(this.menuStopParse_Click);
            // 
            // menuSeparator1
            // 
            this.menuSeparator1.Index = 3;
            this.menuSeparator1.Text = "-";
            // 
            // menuOpenSavedData
            // 
            this.menuOpenSavedData.Index = 4;
            this.menuOpenSavedData.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
            this.menuOpenSavedData.Text = "&Open Saved Data";
            this.menuOpenSavedData.Click += new System.EventHandler(this.menuOpenSavedData_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Enabled = false;
            this.menuItem3.Index = 5;
            this.menuItem3.Text = "Continue Parsing";
            // 
            // menuSaveDataAs
            // 
            this.menuSaveDataAs.Enabled = false;
            this.menuSaveDataAs.Index = 6;
            this.menuSaveDataAs.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
            this.menuSaveDataAs.Text = "&Save Current Data As ...";
            this.menuSaveDataAs.Click += new System.EventHandler(this.menuSaveDataAs_Click);
            // 
            // menuSaveReport
            // 
            this.menuSaveReport.Enabled = false;
            this.menuSaveReport.Index = 7;
            this.menuSaveReport.Shortcut = System.Windows.Forms.Shortcut.CtrlR;
            this.menuSaveReport.Text = "Save &Report ...";
            this.menuSaveReport.Click += new System.EventHandler(this.menuSaveReport_Click);
            // 
            // menuSeparator2
            // 
            this.menuSeparator2.Index = 8;
            this.menuSeparator2.Text = "-";
            // 
            // menuExit
            // 
            this.menuExit.Index = 9;
            this.menuExit.Shortcut = System.Windows.Forms.Shortcut.CtrlX;
            this.menuExit.Text = "E&xit";
            this.menuExit.Click += new System.EventHandler(this.menuExit_Click);
            // 
            // toolsMenu
            // 
            this.toolsMenu.Index = 1;
            this.toolsMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.databaseToolsMenu,
            this.menuSeparator3,
            this.menuOptions});
            this.toolsMenu.Text = "&Tools";
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.Shortcut = System.Windows.Forms.Shortcut.CtrlT;
            this.menuItem1.Text = "&Test Function";
            this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
            // 
            // databaseToolsMenu
            // 
            this.databaseToolsMenu.Index = 1;
            this.databaseToolsMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.databaseClearData,
            this.databaseUpgrade,
            this.databaseReparse});
            this.databaseToolsMenu.Text = "Database";
            this.databaseToolsMenu.Popup += new System.EventHandler(this.databaseToolsMenu_Popup);
            // 
            // databaseClearData
            // 
            this.databaseClearData.Enabled = false;
            this.databaseClearData.Index = 0;
            this.databaseClearData.Text = "Clear Data";
            // 
            // databaseUpgrade
            // 
            this.databaseUpgrade.Enabled = false;
            this.databaseUpgrade.Index = 1;
            this.databaseUpgrade.Text = "Upgrade";
            // 
            // databaseReparse
            // 
            this.databaseReparse.Enabled = false;
            this.databaseReparse.Index = 2;
            this.databaseReparse.Text = "Reparse";
            this.databaseReparse.Click += new System.EventHandler(this.databaseReparse_Click);
            // 
            // menuSeparator3
            // 
            this.menuSeparator3.Index = 2;
            this.menuSeparator3.Text = "-";
            // 
            // menuOptions
            // 
            this.menuOptions.Index = 3;
            this.menuOptions.Text = "&Options...";
            this.menuOptions.Click += new System.EventHandler(this.menuOptions_Click);
            // 
            // windowMenu
            // 
            this.windowMenu.Index = 2;
            this.windowMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuAbout});
            this.windowMenu.Text = "Windows";
            // 
            // menuAbout
            // 
            this.menuAbout.Index = 0;
            this.menuAbout.Text = "&About";
            this.menuAbout.Click += new System.EventHandler(this.menuAbout_Click);
            // 
            // pluginTabs
            // 
            this.pluginTabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pluginTabs.ItemSize = new System.Drawing.Size(80, 21);
            this.pluginTabs.Location = new System.Drawing.Point(0, 0);
            this.pluginTabs.Name = "pluginTabs";
            this.pluginTabs.SelectedIndex = 0;
            this.pluginTabs.Size = new System.Drawing.Size(992, 436);
            this.pluginTabs.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.pluginTabs.TabIndex = 3;
            // 
            // ParserWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(992, 457);
            this.Controls.Add(this.pluginTabs);
            this.Controls.Add(this.statusStrip);
            this.Menu = this.mainMenu;
            this.Name = "ParserWindow";
            this.Text = "K-Parser";
            this.Load += new System.EventHandler(this.ParserWindow_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ParserWindow_FormClosing);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.MainMenu mainMenu;
        private System.Windows.Forms.MenuItem fileMenu;
        private System.Windows.Forms.MenuItem menuBeginDefaultParse;
        private System.Windows.Forms.MenuItem menuBeginParseWithSave;
        private System.Windows.Forms.MenuItem menuStopParse;
        private System.Windows.Forms.MenuItem menuSeparator1;
        private System.Windows.Forms.MenuItem menuOpenSavedData;
        private System.Windows.Forms.MenuItem menuSaveDataAs;
        private System.Windows.Forms.MenuItem menuSaveReport;
        private System.Windows.Forms.MenuItem menuSeparator2;
        private System.Windows.Forms.MenuItem menuExit;
        private System.Windows.Forms.MenuItem toolsMenu;
        private System.Windows.Forms.MenuItem menuSeparator3;
        private System.Windows.Forms.MenuItem menuOptions;
        private System.Windows.Forms.MenuItem menuAbout;
        private System.Windows.Forms.TabControl pluginTabs;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem windowMenu;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.MenuItem databaseToolsMenu;
        private System.Windows.Forms.MenuItem databaseClearData;
        private System.Windows.Forms.MenuItem databaseUpgrade;
        private System.Windows.Forms.MenuItem databaseReparse;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
    }
}