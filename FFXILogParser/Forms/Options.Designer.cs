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
            this.memoryOffsetAddress = new System.Windows.Forms.TextBox();
            this.logDirectory = new System.Windows.Forms.TextBox();
            this.memoryLabel = new System.Windows.Forms.Label();
            this.getMemoryAddress = new System.Windows.Forms.Button();
            this.directoryLabel = new System.Windows.Forms.Label();
            this.getLogDirectory = new System.Windows.Forms.Button();
            this.dataSourceRam = new System.Windows.Forms.RadioButton();
            this.dataSourceLogs = new System.Windows.Forms.RadioButton();
            this.ok = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.logFileGroup = new System.Windows.Forms.GroupBox();
            this.readExistingLogs = new System.Windows.Forms.CheckBox();
            this.reset = new System.Windows.Forms.Button();
            this.dataSourceGroup.SuspendLayout();
            this.logFileGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataSourceGroup
            // 
            this.dataSourceGroup.Controls.Add(this.memoryOffsetAddress);
            this.dataSourceGroup.Controls.Add(this.logDirectory);
            this.dataSourceGroup.Controls.Add(this.memoryLabel);
            this.dataSourceGroup.Controls.Add(this.getMemoryAddress);
            this.dataSourceGroup.Controls.Add(this.directoryLabel);
            this.dataSourceGroup.Controls.Add(this.getLogDirectory);
            this.dataSourceGroup.Controls.Add(this.dataSourceRam);
            this.dataSourceGroup.Controls.Add(this.dataSourceLogs);
            this.dataSourceGroup.Location = new System.Drawing.Point(12, 12);
            this.dataSourceGroup.Name = "dataSourceGroup";
            this.dataSourceGroup.Size = new System.Drawing.Size(305, 155);
            this.dataSourceGroup.TabIndex = 0;
            this.dataSourceGroup.TabStop = false;
            this.dataSourceGroup.Text = "Data Source";
            // 
            // memoryOffsetAddress
            // 
            this.memoryOffsetAddress.Location = new System.Drawing.Point(28, 117);
            this.memoryOffsetAddress.Name = "memoryOffsetAddress";
            this.memoryOffsetAddress.ReadOnly = true;
            this.memoryOffsetAddress.Size = new System.Drawing.Size(236, 20);
            this.memoryOffsetAddress.TabIndex = 5;
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
            this.memoryLabel.Location = new System.Drawing.Point(25, 101);
            this.memoryLabel.Name = "memoryLabel";
            this.memoryLabel.Size = new System.Drawing.Size(116, 13);
            this.memoryLabel.TabIndex = 7;
            this.memoryLabel.Text = "Memory offset address:";
            // 
            // getMemoryAddress
            // 
            this.getMemoryAddress.Location = new System.Drawing.Point(270, 117);
            this.getMemoryAddress.Name = "getMemoryAddress";
            this.getMemoryAddress.Size = new System.Drawing.Size(25, 20);
            this.getMemoryAddress.TabIndex = 6;
            this.getMemoryAddress.Text = "...";
            this.getMemoryAddress.UseVisualStyleBackColor = true;
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
            // 
            // dataSourceRam
            // 
            this.dataSourceRam.AutoSize = true;
            this.dataSourceRam.Location = new System.Drawing.Point(6, 81);
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
            this.ok.Location = new System.Drawing.Point(161, 241);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(75, 23);
            this.ok.TabIndex = 1;
            this.ok.Text = "OK";
            this.ok.UseVisualStyleBackColor = true;
            // 
            // cancel
            // 
            this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Location = new System.Drawing.Point(242, 241);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 23);
            this.cancel.TabIndex = 2;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            // 
            // logFileGroup
            // 
            this.logFileGroup.Controls.Add(this.readExistingLogs);
            this.logFileGroup.Location = new System.Drawing.Point(12, 173);
            this.logFileGroup.Name = "logFileGroup";
            this.logFileGroup.Size = new System.Drawing.Size(304, 52);
            this.logFileGroup.TabIndex = 3;
            this.logFileGroup.TabStop = false;
            this.logFileGroup.Text = "Log File Preferences";
            // 
            // readExistingLogs
            // 
            this.readExistingLogs.AutoSize = true;
            this.readExistingLogs.Location = new System.Drawing.Point(6, 19);
            this.readExistingLogs.Name = "readExistingLogs";
            this.readExistingLogs.Size = new System.Drawing.Size(128, 17);
            this.readExistingLogs.TabIndex = 0;
            this.readExistingLogs.Text = "Read existing log files";
            this.readExistingLogs.UseVisualStyleBackColor = true;
            // 
            // reset
            // 
            this.reset.Location = new System.Drawing.Point(18, 241);
            this.reset.Name = "reset";
            this.reset.Size = new System.Drawing.Size(75, 23);
            this.reset.TabIndex = 4;
            this.reset.Text = "Reset";
            this.reset.UseVisualStyleBackColor = true;
            this.reset.Click += new System.EventHandler(this.reset_Click);
            // 
            // Options
            // 
            this.AcceptButton = this.ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancel;
            this.ClientSize = new System.Drawing.Size(329, 276);
            this.Controls.Add(this.reset);
            this.Controls.Add(this.logFileGroup);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.ok);
            this.Controls.Add(this.dataSourceGroup);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Options";
            this.ShowIcon = false;
            this.Text = "Options";
            this.Load += new System.EventHandler(this.Options_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Options_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Options_FormClosing);
            this.dataSourceGroup.ResumeLayout(false);
            this.dataSourceGroup.PerformLayout();
            this.logFileGroup.ResumeLayout(false);
            this.logFileGroup.PerformLayout();
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
        private System.Windows.Forms.Button getMemoryAddress;
        private System.Windows.Forms.TextBox memoryOffsetAddress;
        private System.Windows.Forms.GroupBox logFileGroup;
        private System.Windows.Forms.CheckBox readExistingLogs;
        private System.Windows.Forms.Button reset;
    }
}