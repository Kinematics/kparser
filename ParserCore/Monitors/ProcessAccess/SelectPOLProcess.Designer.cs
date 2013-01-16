namespace WaywardGamers.KParser.Monitoring
{
    partial class SelectPOLProcess
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectPOLProcess));
            this.processList = new System.Windows.Forms.ListBox();
            this.ok = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.refresh = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // processList
            // 
            this.processList.AccessibleDescription = null;
            this.processList.AccessibleName = null;
            resources.ApplyResources(this.processList, "processList");
            this.processList.BackgroundImage = null;
            this.processList.Font = null;
            this.processList.FormattingEnabled = true;
            this.processList.Name = "processList";
            this.processList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.processList_MouseDoubleClick);
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
            this.ok.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ok_MouseClick);
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
            this.cancel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.cancel_MouseClick);
            // 
            // refresh
            // 
            this.refresh.AccessibleDescription = null;
            this.refresh.AccessibleName = null;
            resources.ApplyResources(this.refresh, "refresh");
            this.refresh.BackgroundImage = null;
            this.refresh.Font = null;
            this.refresh.Name = "refresh";
            this.refresh.UseVisualStyleBackColor = true;
            this.refresh.Click += new System.EventHandler(this.refresh_Click);
            // 
            // SelectPOLProcess
            // 
            this.AcceptButton = this.ok;
            this.AccessibleDescription = null;
            this.AccessibleName = null;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = null;
            this.CancelButton = this.cancel;
            this.Controls.Add(this.refresh);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.ok);
            this.Controls.Add(this.processList);
            this.Icon = null;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SelectPOLProcess";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.SelectPOLProcess_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox processList;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Button refresh;
    }
}