using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace WaywardGamers.KParser.Monitoring
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

        #region Properties
        public int SelectedPID
        {
            get
            {
                if (polProcesses.Length == 0)
                    return 0;

                if (processList.SelectedIndex < 0)
                    return 0;

                Process selectedProcess = polProcesses[processList.SelectedIndex];

                if (selectedProcess != null)
                    return selectedProcess.Id;

                return 0;
            }
        }
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
                        string.Format("{0} --- PID: {1}", proc.MainWindowTitle, proc.Id));
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
        #endregion
    }
}
