namespace WaywardGamers.KParser.Forms
{
    partial class PlayerInfo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlayerInfo));
            this.ok = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.combatantListBox = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.combatantType = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.combatantDescription = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // ok
            // 
            this.ok.AccessibleDescription = null;
            this.ok.AccessibleName = null;
            resources.ApplyResources(this.ok, "ok");
            this.ok.BackgroundImage = null;
            this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
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
            this.cancel.Name = "cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.cancel_Click);
            // 
            // combatantListBox
            // 
            this.combatantListBox.AccessibleDescription = null;
            this.combatantListBox.AccessibleName = null;
            resources.ApplyResources(this.combatantListBox, "combatantListBox");
            this.combatantListBox.BackgroundImage = null;
            this.combatantListBox.Font = null;
            this.combatantListBox.FormattingEnabled = true;
            this.combatantListBox.Name = "combatantListBox";
            this.combatantListBox.SelectedIndexChanged += new System.EventHandler(this.combatantListBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AccessibleDescription = null;
            this.label1.AccessibleName = null;
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // combatantType
            // 
            this.combatantType.AccessibleDescription = null;
            this.combatantType.AccessibleName = null;
            resources.ApplyResources(this.combatantType, "combatantType");
            this.combatantType.BackgroundImage = null;
            this.combatantType.Font = null;
            this.combatantType.Name = "combatantType";
            this.combatantType.ReadOnly = true;
            // 
            // label2
            // 
            this.label2.AccessibleDescription = null;
            this.label2.AccessibleName = null;
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label3
            // 
            this.label3.AccessibleDescription = null;
            this.label3.AccessibleName = null;
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // combatantDescription
            // 
            this.combatantDescription.AccessibleDescription = null;
            this.combatantDescription.AccessibleName = null;
            resources.ApplyResources(this.combatantDescription, "combatantDescription");
            this.combatantDescription.BackgroundImage = null;
            this.combatantDescription.Font = null;
            this.combatantDescription.Name = "combatantDescription";
            this.combatantDescription.TextChanged += new System.EventHandler(this.combatantDescription_TextChanged);
            // 
            // PlayerInfo
            // 
            this.AcceptButton = this.ok;
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.CancelButton = this.cancel;
            this.ControlBox = false;
            this.Controls.Add(this.combatantDescription);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.combatantType);
            this.Controls.Add(this.combatantListBox);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.ok);
            this.Controls.Add(this.label1);
            this.Font = null;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PlayerInfo";
            this.ShowInTaskbar = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PlayerInfo_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.ListBox combatantListBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox combatantType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox combatantDescription;
    }
}