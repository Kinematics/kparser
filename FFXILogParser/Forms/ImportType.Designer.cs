namespace WaywardGamers.KParser.Forms
{
    partial class ImportType
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportType));
            this.ok = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.optionDVSParse = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
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
            // 
            // optionDVSParse
            // 
            this.optionDVSParse.AccessibleDescription = null;
            this.optionDVSParse.AccessibleName = null;
            resources.ApplyResources(this.optionDVSParse, "optionDVSParse");
            this.optionDVSParse.BackgroundImage = null;
            this.optionDVSParse.Font = null;
            this.optionDVSParse.Name = "optionDVSParse";
            this.optionDVSParse.TabStop = true;
            this.optionDVSParse.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.AccessibleDescription = null;
            this.groupBox1.AccessibleName = null;
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.BackgroundImage = null;
            this.groupBox1.Controls.Add(this.optionDVSParse);
            this.groupBox1.Font = null;
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // ImportType
            // 
            this.AcceptButton = this.ok;
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.CancelButton = this.cancel;
            this.ControlBox = false;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.ok);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = null;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportType";
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.ImportType_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.RadioButton optionDVSParse;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}