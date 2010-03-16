namespace WaywardGamers.KParser.Forms
{
    partial class SplitParsesDlg
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
            this.startGroup = new System.Windows.Forms.GroupBox();
            this.startDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.startFightCombo = new System.Windows.Forms.ComboBox();
            this.startAtTime = new System.Windows.Forms.RadioButton();
            this.startAtEndOfFight = new System.Windows.Forms.RadioButton();
            this.startAtStartOfFight = new System.Windows.Forms.RadioButton();
            this.startAtBeginningOfParse = new System.Windows.Forms.RadioButton();
            this.endGroup = new System.Windows.Forms.GroupBox();
            this.endDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.endFightCombo = new System.Windows.Forms.ComboBox();
            this.endAtTime = new System.Windows.Forms.RadioButton();
            this.endAtEndOfFight = new System.Windows.Forms.RadioButton();
            this.endAtStartOfFight = new System.Windows.Forms.RadioButton();
            this.endAtEndOfParse = new System.Windows.Forms.RadioButton();
            this.acceptButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.startGroup.SuspendLayout();
            this.endGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // startGroup
            // 
            this.startGroup.Controls.Add(this.startDateTimePicker);
            this.startGroup.Controls.Add(this.startFightCombo);
            this.startGroup.Controls.Add(this.startAtTime);
            this.startGroup.Controls.Add(this.startAtEndOfFight);
            this.startGroup.Controls.Add(this.startAtStartOfFight);
            this.startGroup.Controls.Add(this.startAtBeginningOfParse);
            this.startGroup.Location = new System.Drawing.Point(12, 12);
            this.startGroup.Name = "startGroup";
            this.startGroup.Size = new System.Drawing.Size(468, 121);
            this.startGroup.TabIndex = 0;
            this.startGroup.TabStop = false;
            this.startGroup.Text = "Start point";
            // 
            // startDateTimePicker
            // 
            this.startDateTimePicker.CustomFormat = "dddd MMMM dd, yyyy    hh:mm:ss ";
            this.startDateTimePicker.Enabled = false;
            this.startDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.startDateTimePicker.Location = new System.Drawing.Point(192, 88);
            this.startDateTimePicker.MinDate = new System.DateTime(2001, 1, 1, 0, 0, 0, 0);
            this.startDateTimePicker.Name = "startDateTimePicker";
            this.startDateTimePicker.Size = new System.Drawing.Size(270, 20);
            this.startDateTimePicker.TabIndex = 5;
            // 
            // startFightCombo
            // 
            this.startFightCombo.Enabled = false;
            this.startFightCombo.FormattingEnabled = true;
            this.startFightCombo.Location = new System.Drawing.Point(192, 38);
            this.startFightCombo.Name = "startFightCombo";
            this.startFightCombo.Size = new System.Drawing.Size(270, 21);
            this.startFightCombo.TabIndex = 4;
            // 
            // startAtTime
            // 
            this.startAtTime.AutoSize = true;
            this.startAtTime.Location = new System.Drawing.Point(6, 88);
            this.startAtTime.Name = "startAtTime";
            this.startAtTime.Size = new System.Drawing.Size(91, 17);
            this.startAtTime.TabIndex = 3;
            this.startAtTime.TabStop = true;
            this.startAtTime.Text = "Specified time";
            this.startAtTime.UseVisualStyleBackColor = true;
            this.startAtTime.CheckedChanged += new System.EventHandler(this.startPointType_CheckedChanged);
            // 
            // startAtEndOfFight
            // 
            this.startAtEndOfFight.AutoSize = true;
            this.startAtEndOfFight.Enabled = false;
            this.startAtEndOfFight.Location = new System.Drawing.Point(6, 65);
            this.startAtEndOfFight.Name = "startAtEndOfFight";
            this.startAtEndOfFight.Size = new System.Drawing.Size(146, 17);
            this.startAtEndOfFight.TabIndex = 2;
            this.startAtEndOfFight.TabStop = true;
            this.startAtEndOfFight.Text = "After end of selected fight";
            this.startAtEndOfFight.UseVisualStyleBackColor = true;
            this.startAtEndOfFight.CheckedChanged += new System.EventHandler(this.startPointType_CheckedChanged);
            // 
            // startAtStartOfFight
            // 
            this.startAtStartOfFight.AutoSize = true;
            this.startAtStartOfFight.Enabled = false;
            this.startAtStartOfFight.Location = new System.Drawing.Point(6, 42);
            this.startAtStartOfFight.Name = "startAtStartOfFight";
            this.startAtStartOfFight.Size = new System.Drawing.Size(136, 17);
            this.startAtStartOfFight.TabIndex = 1;
            this.startAtStartOfFight.TabStop = true;
            this.startAtStartOfFight.Text = "At start of selected fight";
            this.startAtStartOfFight.UseVisualStyleBackColor = true;
            this.startAtStartOfFight.CheckedChanged += new System.EventHandler(this.startPointType_CheckedChanged);
            // 
            // startAtBeginningOfParse
            // 
            this.startAtBeginningOfParse.AutoSize = true;
            this.startAtBeginningOfParse.Checked = true;
            this.startAtBeginningOfParse.Location = new System.Drawing.Point(6, 19);
            this.startAtBeginningOfParse.Name = "startAtBeginningOfParse";
            this.startAtBeginningOfParse.Size = new System.Drawing.Size(113, 17);
            this.startAtBeginningOfParse.TabIndex = 0;
            this.startAtBeginningOfParse.TabStop = true;
            this.startAtBeginningOfParse.Text = "Beginning of parse";
            this.startAtBeginningOfParse.UseVisualStyleBackColor = true;
            this.startAtBeginningOfParse.CheckedChanged += new System.EventHandler(this.startPointType_CheckedChanged);
            // 
            // endGroup
            // 
            this.endGroup.Controls.Add(this.endDateTimePicker);
            this.endGroup.Controls.Add(this.endFightCombo);
            this.endGroup.Controls.Add(this.endAtTime);
            this.endGroup.Controls.Add(this.endAtEndOfFight);
            this.endGroup.Controls.Add(this.endAtStartOfFight);
            this.endGroup.Controls.Add(this.endAtEndOfParse);
            this.endGroup.Location = new System.Drawing.Point(12, 139);
            this.endGroup.Name = "endGroup";
            this.endGroup.Size = new System.Drawing.Size(468, 122);
            this.endGroup.TabIndex = 1;
            this.endGroup.TabStop = false;
            this.endGroup.Text = "End point";
            // 
            // endDateTimePicker
            // 
            this.endDateTimePicker.CustomFormat = "dddd MMMM dd, yyyy    hh:mm:ss ";
            this.endDateTimePicker.Enabled = false;
            this.endDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.endDateTimePicker.Location = new System.Drawing.Point(192, 88);
            this.endDateTimePicker.Name = "endDateTimePicker";
            this.endDateTimePicker.Size = new System.Drawing.Size(270, 20);
            this.endDateTimePicker.TabIndex = 5;
            // 
            // endFightCombo
            // 
            this.endFightCombo.Enabled = false;
            this.endFightCombo.FormattingEnabled = true;
            this.endFightCombo.Location = new System.Drawing.Point(192, 38);
            this.endFightCombo.Name = "endFightCombo";
            this.endFightCombo.Size = new System.Drawing.Size(270, 21);
            this.endFightCombo.TabIndex = 4;
            // 
            // endAtTime
            // 
            this.endAtTime.AutoSize = true;
            this.endAtTime.Location = new System.Drawing.Point(6, 88);
            this.endAtTime.Name = "endAtTime";
            this.endAtTime.Size = new System.Drawing.Size(91, 17);
            this.endAtTime.TabIndex = 3;
            this.endAtTime.TabStop = true;
            this.endAtTime.Text = "Specified time";
            this.endAtTime.UseVisualStyleBackColor = true;
            this.endAtTime.CheckedChanged += new System.EventHandler(this.endPointType_CheckedChanged);
            // 
            // endAtEndOfFight
            // 
            this.endAtEndOfFight.AutoSize = true;
            this.endAtEndOfFight.Enabled = false;
            this.endAtEndOfFight.Location = new System.Drawing.Point(6, 42);
            this.endAtEndOfFight.Name = "endAtEndOfFight";
            this.endAtEndOfFight.Size = new System.Drawing.Size(134, 17);
            this.endAtEndOfFight.TabIndex = 2;
            this.endAtEndOfFight.TabStop = true;
            this.endAtEndOfFight.Text = "At end of selected fight";
            this.endAtEndOfFight.UseVisualStyleBackColor = true;
            this.endAtEndOfFight.CheckedChanged += new System.EventHandler(this.endPointType_CheckedChanged);
            // 
            // endAtStartOfFight
            // 
            this.endAtStartOfFight.AutoSize = true;
            this.endAtStartOfFight.Enabled = false;
            this.endAtStartOfFight.Location = new System.Drawing.Point(6, 65);
            this.endAtStartOfFight.Name = "endAtStartOfFight";
            this.endAtStartOfFight.Size = new System.Drawing.Size(157, 17);
            this.endAtStartOfFight.TabIndex = 1;
            this.endAtStartOfFight.TabStop = true;
            this.endAtStartOfFight.Text = "Before start of selected fight";
            this.endAtStartOfFight.UseVisualStyleBackColor = true;
            this.endAtStartOfFight.CheckedChanged += new System.EventHandler(this.endPointType_CheckedChanged);
            // 
            // endAtEndOfParse
            // 
            this.endAtEndOfParse.AutoSize = true;
            this.endAtEndOfParse.Checked = true;
            this.endAtEndOfParse.Location = new System.Drawing.Point(6, 19);
            this.endAtEndOfParse.Name = "endAtEndOfParse";
            this.endAtEndOfParse.Size = new System.Drawing.Size(85, 17);
            this.endAtEndOfParse.TabIndex = 0;
            this.endAtEndOfParse.TabStop = true;
            this.endAtEndOfParse.Text = "End of parse";
            this.endAtEndOfParse.UseVisualStyleBackColor = true;
            this.endAtEndOfParse.CheckedChanged += new System.EventHandler(this.endPointType_CheckedChanged);
            // 
            // acceptButton
            // 
            this.acceptButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.acceptButton.Location = new System.Drawing.Point(324, 274);
            this.acceptButton.Name = "acceptButton";
            this.acceptButton.Size = new System.Drawing.Size(75, 23);
            this.acceptButton.TabIndex = 2;
            this.acceptButton.Text = "Accept";
            this.acceptButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(405, 274);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // SplitParsesDlg
            // 
            this.AcceptButton = this.acceptButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(492, 309);
            this.ControlBox = false;
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.acceptButton);
            this.Controls.Add(this.endGroup);
            this.Controls.Add(this.startGroup);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "SplitParsesDlg";
            this.ShowInTaskbar = false;
            this.Text = "Split Parse File";
            this.Load += new System.EventHandler(this.SplitParsesDlg_Load);
            this.startGroup.ResumeLayout(false);
            this.startGroup.PerformLayout();
            this.endGroup.ResumeLayout(false);
            this.endGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox startGroup;
        private System.Windows.Forms.GroupBox endGroup;
        private System.Windows.Forms.Button acceptButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.RadioButton startAtTime;
        private System.Windows.Forms.RadioButton startAtEndOfFight;
        private System.Windows.Forms.RadioButton startAtStartOfFight;
        private System.Windows.Forms.RadioButton startAtBeginningOfParse;
        private System.Windows.Forms.RadioButton endAtTime;
        private System.Windows.Forms.RadioButton endAtEndOfFight;
        private System.Windows.Forms.RadioButton endAtStartOfFight;
        private System.Windows.Forms.RadioButton endAtEndOfParse;
        private System.Windows.Forms.DateTimePicker startDateTimePicker;
        private System.Windows.Forms.ComboBox startFightCombo;
        private System.Windows.Forms.DateTimePicker endDateTimePicker;
        private System.Windows.Forms.ComboBox endFightCombo;
    }
}