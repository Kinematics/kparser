namespace WaywardGamers.KParser.Plugin
{
    partial class BaseGraphPluginControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.zedGraphControl = new ZedGraph.ZedGraphControl();
            this.SuspendLayout();
            // 
            // richTextBox
            // 
            this.richTextBox.Size = new System.Drawing.Size(600, 348);
            this.richTextBox.Visible = false;
            // 
            // zedGraphControl
            // 
            this.zedGraphControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.zedGraphControl.Location = new System.Drawing.Point(3, 28);
            this.zedGraphControl.Name = "zedGraphControl";
            this.zedGraphControl.ScrollGrace = 0;
            this.zedGraphControl.ScrollMaxX = 0;
            this.zedGraphControl.ScrollMaxY = 0;
            this.zedGraphControl.ScrollMaxY2 = 0;
            this.zedGraphControl.ScrollMinX = 0;
            this.zedGraphControl.ScrollMinY = 0;
            this.zedGraphControl.ScrollMinY2 = 0;
            this.zedGraphControl.Size = new System.Drawing.Size(600, 348);
            this.zedGraphControl.TabIndex = 2;
            // 
            // BaseGraphPluginControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.zedGraphControl);
            this.Name = "BaseGraphPluginControl";
            this.Size = new System.Drawing.Size(606, 379);
            this.Controls.SetChildIndex(this.richTextBox, 0);
            this.Controls.SetChildIndex(this.zedGraphControl, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ZedGraph.ZedGraphControl zedGraphControl;
    }
}
