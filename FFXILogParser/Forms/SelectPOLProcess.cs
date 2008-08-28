using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace WaywardGamers.KParser.Forms
{
    public partial class SelectPOLProcess : Form
    {
        #region Construction
        public SelectPOLProcess()
        {
            InitializeComponent();
        }

        Process[] polProcesses;
        #endregion

        #region Event handlers - initializing
        private void SelectPOLProcess_Load(object sender, EventArgs e)
        {
            PopulateProcessList();
        }

        private void refresh_Click(object sender, EventArgs e)
        {
            PopulateProcessList();
        }

        private void PopulateProcessList()
        {
            processList.Items.Clear();

            polProcesses = Process.GetProcessesByName("pol");

            ok.Enabled = (polProcesses.Length > 0);

            if (ok.Enabled == true)
            {
                foreach (var proc in polProcesses)
                {
                    processList.Items.Add(
                        string.Format("{0} --- PID: {1}", proc.ProcessName, proc.Id));
                }

                processList.SelectedIndex = 0;
            }
        }
        #endregion

        #region Event handlers - closing
        private void processList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ok_MouseClick(object sender, MouseEventArgs e)
        {
            this.Close();
        }

        private void cancel_MouseClick(object sender, MouseEventArgs e)
        {
            this.Close();
        }

        private void SelectPOLProcess_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((e.CloseReason == CloseReason.UserClosing) ||
                (e.CloseReason == CloseReason.None))
            {
                if (this.DialogResult == DialogResult.OK)
                {
                    if (processList.SelectedItem == null)
                    {
                        MessageBox.Show("No process has been selected.",
                           "Process not selected.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        e.Cancel = true;
                    }

                    Properties.Settings settings = new WaywardGamers.KParser.Properties.Settings();

                    if (polProcesses.Length > 0)
                    {
                        Process selectedProcess = polProcesses[processList.SelectedIndex];

                        if (selectedProcess != null)
                            settings.RequestedPID = selectedProcess.Id;

                        settings.Save();
                    }
                }
            }
        }
        #endregion
    }
}
