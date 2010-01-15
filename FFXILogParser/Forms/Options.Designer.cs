namespace WaywardGamers.KParser
{
    partial class Options
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Options));
            this.dataSourceGroup = new System.Windows.Forms.GroupBox();
            this.specifyPID = new System.Windows.Forms.CheckBox();
            this.editMemoryAddress = new System.Windows.Forms.CheckBox();
            this.readExistingLogs = new System.Windows.Forms.CheckBox();
            this.memoryOffsetAddress = new System.Windows.Forms.TextBox();
            this.logDirectory = new System.Windows.Forms.TextBox();
            this.memoryLabel = new System.Windows.Forms.Label();
            this.directoryLabel = new System.Windows.Forms.Label();
            this.getLogDirectory = new System.Windows.Forms.Button();
            this.dataSourceRam = new System.Windows.Forms.RadioButton();
            this.dataSourceLogs = new System.Windows.Forms.RadioButton();
            this.ok = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.reset = new System.Windows.Forms.Button();
            this.debugMode = new System.Windows.Forms.CheckBox();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.otherGroup = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numberOfRecentFilesUpDown = new System.Windows.Forms.NumericUpDown();
            this.defaultSaveDirectory = new System.Windows.Forms.TextBox();
            this.saveDirectoryLabel = new System.Windows.Forms.Label();
            this.getSaveDirectory = new System.Windows.Forms.Button();
            this.dataSourceGroup.SuspendLayout();
            this.otherGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numberOfRecentFilesUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // dataSourceGroup
            // 
            this.dataSourceGroup.AccessibleDescription = null;
            this.dataSourceGroup.AccessibleName = null;
            resources.ApplyResources(this.dataSourceGroup, "dataSourceGroup");
            this.dataSourceGroup.BackgroundImage = null;
            this.dataSourceGroup.Controls.Add(this.specifyPID);
            this.dataSourceGroup.Controls.Add(this.editMemoryAddress);
            this.dataSourceGroup.Controls.Add(this.readExistingLogs);
            this.dataSourceGroup.Controls.Add(this.memoryOffsetAddress);
            this.dataSourceGroup.Controls.Add(this.logDirectory);
            this.dataSourceGroup.Controls.Add(this.memoryLabel);
            this.dataSourceGroup.Controls.Add(this.directoryLabel);
            this.dataSourceGroup.Controls.Add(this.getLogDirectory);
            this.dataSourceGroup.Controls.Add(this.dataSourceRam);
            this.dataSourceGroup.Controls.Add(this.dataSourceLogs);
            this.dataSourceGroup.Font = null;
            this.dataSourceGroup.Name = "dataSourceGroup";
            this.dataSourceGroup.TabStop = false;
            // 
            // specifyPID
            // 
            this.specifyPID.AccessibleDescription = null;
            this.specifyPID.AccessibleName = null;
            resources.ApplyResources(this.specifyPID, "specifyPID");
            this.specifyPID.BackgroundImage = null;
            this.specifyPID.Font = null;
            this.specifyPID.Name = "specifyPID";
            this.specifyPID.UseVisualStyleBackColor = true;
            // 
            // editMemoryAddress
            // 
            this.editMemoryAddress.AccessibleDescription = null;
            this.editMemoryAddress.AccessibleName = null;
            resources.ApplyResources(this.editMemoryAddress, "editMemoryAddress");
            this.editMemoryAddress.BackgroundImage = null;
            this.editMemoryAddress.Font = null;
            this.editMemoryAddress.Name = "editMemoryAddress";
            this.editMemoryAddress.UseVisualStyleBackColor = true;
            this.editMemoryAddress.CheckedChanged += new System.EventHandler(this.editMemoryAddress_CheckedChanged);
            // 
            // readExistingLogs
            // 
            this.readExistingLogs.AccessibleDescription = null;
            this.readExistingLogs.AccessibleName = null;
            resources.ApplyResources(this.readExistingLogs, "readExistingLogs");
            this.readExistingLogs.BackgroundImage = null;
            this.readExistingLogs.Font = null;
            this.readExistingLogs.Name = "readExistingLogs";
            this.readExistingLogs.UseVisualStyleBackColor = true;
            // 
            // memoryOffsetAddress
            // 
            this.memoryOffsetAddress.AccessibleDescription = null;
            this.memoryOffsetAddress.AccessibleName = null;
            resources.ApplyResources(this.memoryOffsetAddress, "memoryOffsetAddress");
            this.memoryOffsetAddress.BackgroundImage = null;
            this.memoryOffsetAddress.Font = null;
            this.memoryOffsetAddress.Name = "memoryOffsetAddress";
            this.memoryOffsetAddress.ReadOnly = true;
            this.memoryOffsetAddress.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.memoryOffsetAddress_KeyPress);
            // 
            // logDirectory
            // 
            this.logDirectory.AccessibleDescription = null;
            this.logDirectory.AccessibleName = null;
            resources.ApplyResources(this.logDirectory, "logDirectory");
            this.logDirectory.BackgroundImage = null;
            this.logDirectory.Font = null;
            this.logDirectory.Name = "logDirectory";
            this.logDirectory.ReadOnly = true;
            // 
            // memoryLabel
            // 
            this.memoryLabel.AccessibleDescription = null;
            this.memoryLabel.AccessibleName = null;
            resources.ApplyResources(this.memoryLabel, "memoryLabel");
            this.memoryLabel.Font = null;
            this.memoryLabel.Name = "memoryLabel";
            // 
            // directoryLabel
            // 
            this.directoryLabel.AccessibleDescription = null;
            this.directoryLabel.AccessibleName = null;
            resources.ApplyResources(this.directoryLabel, "directoryLabel");
            this.directoryLabel.Font = null;
            this.directoryLabel.Name = "directoryLabel";
            // 
            // getLogDirectory
            // 
            this.getLogDirectory.AccessibleDescription = null;
            this.getLogDirectory.AccessibleName = null;
            resources.ApplyResources(this.getLogDirectory, "getLogDirectory");
            this.getLogDirectory.BackgroundImage = null;
            this.getLogDirectory.Font = null;
            this.getLogDirectory.Name = "getLogDirectory";
            this.getLogDirectory.UseVisualStyleBackColor = true;
            this.getLogDirectory.Click += new System.EventHandler(this.getLogDirectory_Click);
            // 
            // dataSourceRam
            // 
            this.dataSourceRam.AccessibleDescription = null;
            this.dataSourceRam.AccessibleName = null;
            resources.ApplyResources(this.dataSourceRam, "dataSourceRam");
            this.dataSourceRam.BackgroundImage = null;
            this.dataSourceRam.Font = null;
            this.dataSourceRam.Name = "dataSourceRam";
            this.dataSourceRam.TabStop = true;
            this.dataSourceRam.UseVisualStyleBackColor = true;
            // 
            // dataSourceLogs
            // 
            this.dataSourceLogs.AccessibleDescription = null;
            this.dataSourceLogs.AccessibleName = null;
            resources.ApplyResources(this.dataSourceLogs, "dataSourceLogs");
            this.dataSourceLogs.BackgroundImage = null;
            this.dataSourceLogs.Font = null;
            this.dataSourceLogs.Name = "dataSourceLogs";
            this.dataSourceLogs.TabStop = true;
            this.dataSourceLogs.UseVisualStyleBackColor = true;
            this.dataSourceLogs.CheckedChanged += new System.EventHandler(this.dataSourceLogs_CheckedChanged);
            // 
            // ok
            // 
            this.ok.AccessibleDescription = null;
            this.ok.AccessibleName = null;
            resources.ApplyResources(this.ok, "ok");
            this.ok.BackgroundImage = null;
            this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ok.Font = null;
            this.ok.Name = "ok";
            this.ok.UseVisualStyleBackColor = true;
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // cancel
            // 
            this.cancel.AccessibleDescription = null;
            this.cancel.AccessibleName = null;
            resources.ApplyResources(this.cancel, "cancel");
            this.cancel.BackgroundImage = null;
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Font = null;
            this.cancel.Name = "cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // reset
            // 
            this.reset.AccessibleDescription = null;
            this.reset.AccessibleName = null;
            resources.ApplyResources(this.reset, "reset");
            this.reset.BackgroundImage = null;
            this.reset.Font = null;
            this.reset.Name = "reset";
            this.reset.UseVisualStyleBackColor = true;
            this.reset.Click += new System.EventHandler(this.reset_Click);
            // 
            // debugMode
            // 
            this.debugMode.AccessibleDescription = null;
            this.debugMode.AccessibleName = null;
            resources.ApplyResources(this.debugMode, "debugMode");
            this.debugMode.BackgroundImage = null;
            this.debugMode.Font = null;
            this.debugMode.Name = "debugMode";
            this.debugMode.UseVisualStyleBackColor = true;
            // 
            // folderBrowserDialog
            // 
            resources.ApplyResources(this.folderBrowserDialog, "folderBrowserDialog");
            // 
            // otherGroup
            // 
            this.otherGroup.AccessibleDescription = null;
            this.otherGroup.AccessibleName = null;
            resources.ApplyResources(this.otherGroup, "otherGroup");
            this.otherGroup.BackgroundImage = null;
            this.otherGroup.Controls.Add(this.label1);
            this.otherGroup.Controls.Add(this.numberOfRecentFilesUpDown);
            this.otherGroup.Controls.Add(this.defaultSaveDirectory);
            this.otherGroup.Controls.Add(this.saveDirectoryLabel);
            this.otherGroup.Controls.Add(this.getSaveDirectory);
            this.otherGroup.Controls.Add(this.debugMode);
            this.otherGroup.Font = null;
            this.otherGroup.Name = "otherGroup";
            this.otherGroup.TabStop = false;
            // 
            // label1
            // 
            this.label1.AccessibleDescription = null;
            this.label1.AccessibleName = null;
            resources.ApplyResources(this.label1, "label1");
            this.label1.Font = null;
            this.label1.Name = "label1";
            // 
            // numberOfRecentFilesUpDown
            // 
            this.numberOfRecentFilesUpDown.AccessibleDescription = null;
            this.numberOfRecentFilesUpDown.AccessibleName = null;
            resources.ApplyResources(this.numberOfRecentFilesUpDown, "numberOfRecentFilesUpDown");
            this.numberOfRecentFilesUpDown.Font = null;
            this.numberOfRecentFilesUpDown.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numberOfRecentFilesUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numberOfRecentFilesUpDown.Name = "numberOfRecentFilesUpDown";
            this.numberOfRecentFilesUpDown.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // defaultSaveDirectory
            // 
            this.defaultSaveDirectory.AccessibleDescription = null;
            this.defaultSaveDirectory.AccessibleName = null;
            resources.ApplyResources(this.defaultSaveDirectory, "defaultSaveDirectory");
            this.defaultSaveDirectory.BackgroundImage = null;
            this.defaultSaveDirectory.Font = null;
            this.defaultSaveDirectory.Name = "defaultSaveDirectory";
            this.defaultSaveDirectory.ReadOnly = true;
            // 
            // saveDirectoryLabel
            // 
            this.saveDirectoryLabel.AccessibleDescription = null;
            this.saveDirectoryLabel.AccessibleName = null;
            resources.ApplyResources(this.saveDirectoryLabel, "saveDirectoryLabel");
            this.saveDirectoryLabel.Font = null;
            this.saveDirectoryLabel.Name = "saveDirectoryLabel";
            // 
            // getSaveDirectory
            // 
            this.getSaveDirectory.AccessibleDescription = null;
            this.getSaveDirectory.AccessibleName = null;
            resources.ApplyResources(this.getSaveDirectory, "getSaveDirectory");
            this.getSaveDirectory.BackgroundImage = null;
            this.getSaveDirectory.Font = null;
            this.getSaveDirectory.Name = "getSaveDirectory";
            this.getSaveDirectory.UseVisualStyleBackColor = true;
            this.getSaveDirectory.Click += new System.EventHandler(this.getSaveDirectory_Click);
            // 
            // Options
            // 
            this.AcceptButton = this.ok;
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.CancelButton = this.cancel;
            this.Controls.Add(this.otherGroup);
            this.Controls.Add(this.reset);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.ok);
            this.Controls.Add(this.dataSourceGroup);
            this.Font = null;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = null;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Options";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.Options_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Options_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Options_FormClosing);
            this.dataSourceGroup.ResumeLayout(false);
            this.dataSourceGroup.PerformLayout();
            this.otherGroup.ResumeLayout(false);
            this.otherGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numberOfRecentFilesUpDown)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox dataSourceGroup;
        private System.Windows.Forms.RadioButton dataSourceRam;
        private System.Windows.Forms.RadioButton dataSourceLogs;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Label directoryLabel;
        private System.Windows.Forms.Button getLogDirectory;
        private System.Windows.Forms.TextBox logDirectory;
        private System.Windows.Forms.Label memoryLabel;
        private System.Windows.Forms.TextBox memoryOffsetAddress;
        private System.Windows.Forms.CheckBox readExistingLogs;
        private System.Windows.Forms.Button reset;
        private System.Windows.Forms.CheckBox debugMode;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.GroupBox otherGroup;
        private System.Windows.Forms.CheckBox editMemoryAddress;
        private System.Windows.Forms.CheckBox specifyPID;
        private System.Windows.Forms.Label saveDirectoryLabel;
        private System.Windows.Forms.Button getSaveDirectory;
        private System.Windows.Forms.TextBox defaultSaveDirectory;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numberOfRecentFilesUpDown;
    }
}