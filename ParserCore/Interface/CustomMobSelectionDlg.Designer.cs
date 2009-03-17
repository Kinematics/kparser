using System.Windows.Forms;

namespace WaywardGamers.KParser.Interface
{
    public partial class CustomMobSelectionDlg : Form
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
            this.mobList = new System.Windows.Forms.CheckedListBox();
            this.selectAllButton = new System.Windows.Forms.Button();
            this.selectNoneButton = new System.Windows.Forms.Button();
            this.ok = new System.Windows.Forms.Button();
            this.cancel = new System.Windows.Forms.Button();
            this.allCurrentSelectionTypes = new System.Windows.Forms.Button();
            this.invertSelectionButton = new System.Windows.Forms.Button();
            this.noneOfCurrentSelectionTypes = new System.Windows.Forms.Button();
            this.uncheck0XPMobs = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // mobList
            // 
            this.mobList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.mobList.CheckOnClick = true;
            this.mobList.FormattingEnabled = true;
            this.mobList.Location = new System.Drawing.Point(12, 12);
            this.mobList.Name = "mobList";
            this.mobList.Size = new System.Drawing.Size(236, 229);
            this.mobList.TabIndex = 0;
            this.mobList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.mobList_ItemCheck);
            // 
            // selectAllButton
            // 
            this.selectAllButton.Location = new System.Drawing.Point(12, 247);
            this.selectAllButton.Name = "selectAllButton";
            this.selectAllButton.Size = new System.Drawing.Size(75, 23);
            this.selectAllButton.TabIndex = 1;
            this.selectAllButton.Text = "All";
            this.selectAllButton.UseVisualStyleBackColor = true;
            this.selectAllButton.Click += new System.EventHandler(this.selectAllButton_Click);
            // 
            // selectNoneButton
            // 
            this.selectNoneButton.Location = new System.Drawing.Point(174, 247);
            this.selectNoneButton.Name = "selectNoneButton";
            this.selectNoneButton.Size = new System.Drawing.Size(75, 23);
            this.selectNoneButton.TabIndex = 3;
            this.selectNoneButton.Text = "None";
            this.selectNoneButton.UseVisualStyleBackColor = true;
            this.selectNoneButton.Click += new System.EventHandler(this.selectNoneButton_Click);
            // 
            // ok
            // 
            this.ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ok.Location = new System.Drawing.Point(92, 372);
            this.ok.Name = "ok";
            this.ok.Size = new System.Drawing.Size(75, 23);
            this.ok.TabIndex = 5;
            this.ok.Text = "OK";
            this.ok.UseVisualStyleBackColor = true;
            // 
            // cancel
            // 
            this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel.Location = new System.Drawing.Point(173, 372);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(75, 23);
            this.cancel.TabIndex = 6;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            // 
            // allCurrentSelectionTypes
            // 
            this.allCurrentSelectionTypes.Location = new System.Drawing.Point(12, 276);
            this.allCurrentSelectionTypes.Name = "allCurrentSelectionTypes";
            this.allCurrentSelectionTypes.Size = new System.Drawing.Size(236, 23);
            this.allCurrentSelectionTypes.TabIndex = 4;
            this.allCurrentSelectionTypes.Text = "All of Currently Selected Mob Types";
            this.allCurrentSelectionTypes.UseVisualStyleBackColor = true;
            this.allCurrentSelectionTypes.Click += new System.EventHandler(this.allCurrentSelectionTypes_Click);
            // 
            // invertSelectionButton
            // 
            this.invertSelectionButton.Location = new System.Drawing.Point(93, 247);
            this.invertSelectionButton.Name = "invertSelectionButton";
            this.invertSelectionButton.Size = new System.Drawing.Size(75, 23);
            this.invertSelectionButton.TabIndex = 2;
            this.invertSelectionButton.Text = "Invert";
            this.invertSelectionButton.UseVisualStyleBackColor = true;
            this.invertSelectionButton.Click += new System.EventHandler(this.invertSelectionButton_Click);
            // 
            // noneOfCurrentSelectionTypes
            // 
            this.noneOfCurrentSelectionTypes.Location = new System.Drawing.Point(12, 305);
            this.noneOfCurrentSelectionTypes.Name = "noneOfCurrentSelectionTypes";
            this.noneOfCurrentSelectionTypes.Size = new System.Drawing.Size(236, 23);
            this.noneOfCurrentSelectionTypes.TabIndex = 7;
            this.noneOfCurrentSelectionTypes.Text = "None of Currently Selected Mob Types";
            this.noneOfCurrentSelectionTypes.UseVisualStyleBackColor = true;
            this.noneOfCurrentSelectionTypes.Click += new System.EventHandler(this.noneOfCurrentSelectionTypes_Click);
            // 
            // uncheck0XPMobs
            // 
            this.uncheck0XPMobs.Location = new System.Drawing.Point(12, 334);
            this.uncheck0XPMobs.Name = "uncheck0XPMobs";
            this.uncheck0XPMobs.Size = new System.Drawing.Size(121, 23);
            this.uncheck0XPMobs.TabIndex = 8;
            this.uncheck0XPMobs.Text = "Uncheck 0 XP Mobs";
            this.uncheck0XPMobs.UseVisualStyleBackColor = true;
            this.uncheck0XPMobs.Click += new System.EventHandler(this.uncheck0XPMobs_Click);
            // 
            // CustomMobSelectionDlg
            // 
            this.AcceptButton = this.ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancel;
            this.ClientSize = new System.Drawing.Size(260, 407);
            this.Controls.Add(this.uncheck0XPMobs);
            this.Controls.Add(this.noneOfCurrentSelectionTypes);
            this.Controls.Add(this.invertSelectionButton);
            this.Controls.Add(this.allCurrentSelectionTypes);
            this.Controls.Add(this.cancel);
            this.Controls.Add(this.ok);
            this.Controls.Add(this.selectNoneButton);
            this.Controls.Add(this.selectAllButton);
            this.Controls.Add(this.mobList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CustomMobSelectionDlg";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Custom Mob Selection";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckedListBox mobList;
        private System.Windows.Forms.Button selectAllButton;
        private System.Windows.Forms.Button selectNoneButton;
        private System.Windows.Forms.Button ok;
        private System.Windows.Forms.Button cancel;
        private System.Windows.Forms.Button allCurrentSelectionTypes;
        private System.Windows.Forms.Button invertSelectionButton;
        private Button noneOfCurrentSelectionTypes;
        private Button uncheck0XPMobs;
    }
}