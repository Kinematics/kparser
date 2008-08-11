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
            this.dataSourceGroup = new System.Windows.Forms.GroupBox();
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
            this.dataSourceGroup.SuspendLayout();
            this.otherGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataSourceGroup
            // 
            this.dataSourceGroup.Controls.Add(this.editMemoryAddress);
            this.dataSourceGroup.Controls.Add(this.readExistingLogs);
            this.dataSourceGroup.Controls.Add(this.memoryOffsetAddress);
            this.dataSourceGroup.Controls.Add(this.logDirectory);
            this.dataSourceGroup.Controls.Add(this.memoryLabel);
            this.dataSourceGroup.Controls.Add(this.directoryLabel);
            this.dataSourceGroup.Controls.Add(this.getLogDirectory);
            this.dataSourceGroup.Controls.Add(this.dataSourceRam);
            this.dataSourceGroup.Controls.Add(this.dataSourceLogs);
            this.dataSourceGroup.Location = new System.Drawing.Point(12, 12);
            this.dataSourceGroup.Name = "dataSourceGroup";
            this.dataSourceGroup.Size = new System.Drawing.Size(305, 181);
            this.dataSourceGroup.TabIndex = 0;
            this.dataSourceGroup.TabStop = false;
            this.dataSourceGroup.Text = "Data Source";
            // 
            // editMemoryAddress
            // 
            this.editMemoryAddress.Appearance = System.Windows.Forms.Appearance.Button;
            this.editMemoryAddress.AutoSize = true;
            this.editMemoryAddress.Location = new System.Drawing.Point(260, 146);
            this.editMemoryAddress.Name = "editMemoryAddress";
            this.editMemoryAddress.Size = new System.Drawing.Size(35, 23);
            this.editMemoryAddress.TabIndex = 6;
            this.editMemoryAddress.Text = "Edit";
            this.editMemoryAddress.UseVisualStyleBackColor = true;
            this.editMemoryAddress.CheckedChanged += new System.EventHandler(this.editMemoryAddress_CheckedChanged);
            // 
            // readExistingLogs
            // 
            this.readExistingLogs.AutoSize = true;
            this.readExistingLogs.Location = new System.Drawing.Point(28, 81);
            this.readExistingLogs.Name = "readExistingLogs";
            this.readExistingLogs.Size = new System.Drawing.Size(128, 17);
            this.readExistingLogs.TabIndex = 0;
            this.readExistingLogs.Text = "Read existing log files";
            this.readExistingLogs.UseVisualStyleBackColor = true;
            // 
            // memoryOffsetAddress
            // 
            this.memoryOffsetAddress.Location = new System.Drawing.Point(28, 149);
            this.memoryOffsetAddress.Name = "memoryOffsetAddress";
            this.memoryOffsetAddress.ReadOnly = true;
            this.memoryOffsetAddress.Size = new System.Drawing.Size(226, 20);
            this.memoryOffsetAddress.TabIndex = 5;
            this.memoryOffsetAddress.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.memoryOffsetAddress_KeyPress);
            // 
            // logDirectory
            // 
            this.logDirectory.Location = new System.Drawing.Point(28, 55);
            this.logDirectory.Name = "logDirectory";
            this.logDirectory.ReadOnly = true;
            this.logDirectory.Size = new System.Drawing.Size(236, 20);
            this.logDirectory.TabIndex = 2;
            // 
            // memoryLabel
            // 
            this.memoryLabel.AutoSize = true;
            this.memoryLabel.Location = new System.Drawing.Point(25, 133);
            this.memoryLabel.Name = "memoryLabel";
            this.memoryLabel.Size = new System.Drawing.Size(184, 13);
            this.memoryLabel.TabIndex = 7;
            this.memoryLabel.Text = "Memory offset address (hexadecimal):";
            // 
            // directoryLabel
            // 
            this.directoryLabel.AutoSize = true;
            this.directoryLabel.Location = new System.Drawing.Point(25, 39);
            this.directoryLabel.Name = "directoryLabel";
            this.directoryLabel.Size = new System.Drawing.Size(98, 13);
            this.directoryLabel.TabIndex = 4;
            this.directoryLabel.Text = "FFXI Log Directory:";
            // 
            // getLogDirectory
            // 
            this.getLogDirectory.Location = new System.Drawing.Point(270, 55);
            this.getLogDirectory.Name = "getLogDirectory";
            this.getLogDirectory.Size = new System.Drawing.Size(25, 20);
            this.getLogDirectory.TabIndex = 3;
            this.getLogDirectory.Text = "...";
            this.getLogDirectory.UseVisualStyleBackColor = true;
            this.getLogDirectory.Click += new System.EventHandler(this.getLogDirectory_Click);
            // 
            // dataSourceRam
            // 
            this.dataSourceRam.AutoSize = true;
            this.dataSourceRam.Location = new System.Drawing.Point(6, 113);
            this.dataSourceRam.Name = "dataSourceRam";
            this.dataSourceRam.Size = new System.Drawing.Size(85, 17);
            this.dataSourceRam.TabIndex = 1;
            this.dataSourceRam.TabStop = true;
            this.dataSourceRam.Text = "Live Memory";
            this.dataSourceRam.UseVisualStyleBackColor = true;
            // 
            // dataSourceLogs
            // 
            this.dataSourceLogs.AutoSize = true;
            this.dataSourceLogs.Location = new System.Drawing.Point(6, 19);
            this.dataSourceLogs.Name = "dataSourceLogs";
            this.dataSourceLogs.Size = new System.Drawing.Size(67, 17);
            this.dataSourceLogs.TabIndex = 0;
            this.dataSourceLogs.TabStop = true;
            this.dataSourceLogs.Text = "Log Files";
            this.dataSourceLogs.UseVisualStyleBackColor = true;
            this.dataSourceLogs.CheckedChanged += new System.EventHandler(this.dataSourceLogs_CheckedChanged);
            // 
            // ok
            // 
            this.ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ok.Location = new System.Drawing.Point(161, 265);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(75, 23);
            this.ok.TabIndex = 1;
            this.ok.Text = "OK";
            this.ok.UseVisualStyleBackColor = true;
            this.ok.Click += new System.EventHandler(this.ok_Click);
            // 
            // cancel
            // 
            this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Location = new System.Drawing.Point(242, 265);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 23);
            this.cancel.TabIndex = 2;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // reset
            // 
            this.reset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.reset.Location = new System.Drawing.Point(18, 265);
            this.reset.Name = "reset";
            this.reset.Size = new System.Drawing.Size(75, 23);
            this.reset.TabIndex = 4;
            this.reset.Text = "Reset";
            this.reset.UseVisualStyleBackColor = true;
            this.reset.Click += new System.EventHandler(this.reset_Click);
            // 
            // debugMode
            // 
            this.debugMode.AutoSize = true;
            this.debugMode.Location = new System.Drawing.Point(15, 19);
            this.debugMode.Name = "debugMode";
            this.debugMode.Size = new System.Drawing.Size(88, 17);
            this.debugMode.TabIndex = 5;
            this.debugMode.Text = "Debug Mode";
            this.debugMode.UseVisualStyleBackColor = true;
            // 
            // otherGroup
            // 
            this.otherGroup.Controls.Add(this.debugMode);
            this.otherGroup.Location = new System.Drawing.Point(12, 199);
            this.otherGroup.Name = "otherGroup";
            this.otherGroup.Size = new System.Drawing.Size(305, 52);
            this.otherGroup.TabIndex = 6;
            this.otherGroup.TabStop = false;
            this.otherGroup.Text = "Other";
            // 
            // Options
            // 
            this.AcceptButton = this.ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancel;
            this.ClientSize = new System.Drawing.Size(329, 300);
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
            this.Text = "Options";
            this.Load += new System.EventHandler(this.Options_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Options_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Options_FormClosing);
            this.dataSourceGroup.ResumeLayout(false);
            this.dataSourceGroup.PerformLayout();
            this.otherGroup.ResumeLayout(false);
            this.otherGroup.PerformLayout();
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
    }
}