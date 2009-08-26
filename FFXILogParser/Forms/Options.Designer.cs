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
            resources.ApplyResources(this.dataSourceGroup, "dataSourceGroup");
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
            this.dataSourceGroup.Name = "dataSourceGroup";
            this.dataSourceGroup.TabStop = false;
            this.dataSourceGroup.UseWaitCursor = true;
            // 
            // specifyPID
            // 
            resources.ApplyResources(this.specifyPID, "specifyPID");
            this.specifyPID.Name = "specifyPID";
            this.specifyPID.UseVisualStyleBackColor = true;
            this.specifyPID.UseWaitCursor = true;
            // 
            // editMemoryAddress
            // 
            resources.ApplyResources(this.editMemoryAddress, "editMemoryAddress");
            this.editMemoryAddress.Name = "editMemoryAddress";
            this.editMemoryAddress.UseVisualStyleBackColor = true;
            this.editMemoryAddress.UseWaitCursor = true;
            this.editMemoryAddress.CheckedChanged += new System.EventHandler(this.editMemoryAddress_CheckedChanged);
            // 
            // readExistingLogs
            // 
            resources.ApplyResources(this.readExistingLogs, "readExistingLogs");
            this.readExistingLogs.Name = "readExistingLogs";
            this.readExistingLogs.UseVisualStyleBackColor = true;
            this.readExistingLogs.UseWaitCursor = true;
            // 
            // memoryOffsetAddress
            // 
            resources.ApplyResources(this.memoryOffsetAddress, "memoryOffsetAddress");
            this.memoryOffsetAddress.Name = "memoryOffsetAddress";
            this.memoryOffsetAddress.ReadOnly = true;
            this.memoryOffsetAddress.UseWaitCursor = true;
            this.memoryOffsetAddress.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.memoryOffsetAddress_KeyPress);
            // 
            // logDirectory
            // 
            resources.ApplyResources(this.logDirectory, "logDirectory");
            this.logDirectory.Name = "logDirectory";
            this.logDirectory.ReadOnly = true;
            this.logDirectory.UseWaitCursor = true;
            // 
            // memoryLabel
            // 
            resources.ApplyResources(this.memoryLabel, "memoryLabel");
            this.memoryLabel.Name = "memoryLabel";
            this.memoryLabel.UseWaitCursor = true;
            // 
            // directoryLabel
            // 
            resources.ApplyResources(this.directoryLabel, "directoryLabel");
            this.directoryLabel.Name = "directoryLabel";
            this.directoryLabel.UseWaitCursor = true;
            // 
            // getLogDirectory
            // 
            resources.ApplyResources(this.getLogDirectory, "getLogDirectory");
            this.getLogDirectory.Name = "getLogDirectory";
            this.getLogDirectory.UseVisualStyleBackColor = true;
            this.getLogDirectory.UseWaitCursor = true;
            this.getLogDirectory.Click += new System.EventHandler(this.getLogDirectory_Click);
            // 
            // dataSourceRam
            // 
            resources.ApplyResources(this.dataSourceRam, "dataSourceRam");
            this.dataSourceRam.Name = "dataSourceRam";
            this.dataSourceRam.TabStop = true;
            this.dataSourceRam.UseVisualStyleBackColor = true;
            this.dataSourceRam.UseWaitCursor = true;
            // 
            // dataSourceLogs
            // 
            resources.ApplyResources(this.dataSourceLogs, "dataSourceLogs");
            this.dataSourceLogs.Name = "dataSourceLogs";
            this.dataSourceLogs.TabStop = true;
            this.dataSourceLogs.UseVisualStyleBackColor = true;
            this.dataSourceLogs.UseWaitCursor = true;
            this.dataSourceLogs.CheckedChanged += new System.EventHandler(this.dataSourceLogs_CheckedChanged);
            // 
            // ok
            // 
            resources.ApplyResources(this.ok, "ok");
            this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ok.Name = "ok";
            this.ok.UseVisualStyleBackColor = true;
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // cancel
            // 
            resources.ApplyResources(this.cancel, "cancel");
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Name = "cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // reset
            // 
            resources.ApplyResources(this.reset, "reset");
            this.reset.Name = "reset";
            this.reset.UseVisualStyleBackColor = true;
            this.reset.Click += new System.EventHandler(this.reset_Click);
            // 
            // debugMode
            // 
            resources.ApplyResources(this.debugMode, "debugMode");
            this.debugMode.Name = "debugMode";
            this.debugMode.UseVisualStyleBackColor = true;
            // 
            // otherGroup
            // 
            resources.ApplyResources(this.otherGroup, "otherGroup");
            this.otherGroup.Controls.Add(this.label1);
            this.otherGroup.Controls.Add(this.numberOfRecentFilesUpDown);
            this.otherGroup.Controls.Add(this.defaultSaveDirectory);
            this.otherGroup.Controls.Add(this.saveDirectoryLabel);
            this.otherGroup.Controls.Add(this.getSaveDirectory);
            this.otherGroup.Controls.Add(this.debugMode);
            this.otherGroup.Name = "otherGroup";
            this.otherGroup.TabStop = false;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // numberOfRecentFilesUpDown
            // 
            resources.ApplyResources(this.numberOfRecentFilesUpDown, "numberOfRecentFilesUpDown");
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
            resources.ApplyResources(this.defaultSaveDirectory, "defaultSaveDirectory");
            this.defaultSaveDirectory.Name = "defaultSaveDirectory";
            this.defaultSaveDirectory.ReadOnly = true;
            // 
            // saveDirectoryLabel
            // 
            resources.ApplyResources(this.saveDirectoryLabel, "saveDirectoryLabel");
            this.saveDirectoryLabel.Name = "saveDirectoryLabel";
            // 
            // getSaveDirectory
            // 
            resources.ApplyResources(this.getSaveDirectory, "getSaveDirectory");
            this.getSaveDirectory.Name = "getSaveDirectory";
            this.getSaveDirectory.UseVisualStyleBackColor = true;
            this.getSaveDirectory.Click += new System.EventHandler(this.getSaveDirectory_Click);
            // 
            // Options
            // 
            this.AcceptButton = this.ok;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancel;
            this.Controls.Add(this.otherGroup);
            this.Controls.Add(this.reset);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.ok);
            this.Controls.Add(this.dataSourceGroup);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
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