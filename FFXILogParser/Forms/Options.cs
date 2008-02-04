using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace WaywardGamers.KParser
{
    public partial class Options : Form
    {
        #region Member Variables
        Properties.WindowSettings windowSettings;
        Properties.Settings coreSettings;
        #endregion

        #region Constructor
        /// <summary>
        /// Basic constructor.  Main window passes in whether a parse is
        /// running when we start.
        /// </summary>
        /// <param name="isParseRunning"></param>
        public Options(bool isParseRunning)
        {
            InitializeComponent();

            // Load a local copy of the app settings.
            windowSettings = new WaywardGamers.KParser.Properties.WindowSettings();
            windowSettings.Reload();
            coreSettings = new WaywardGamers.KParser.Properties.Settings();
            coreSettings.Reload();

            // Copy the settings into the form.
            LoadSettingsValues();

            // Disable changing most values if a parse is already running.
            if (isParseRunning == true)
            {
                dataSourceGroup.Enabled = false;
                dataSourceGroup.Text = "Data Source (Cannot change while parse is running)";
            }
            else
            {
                dataSourceGroup.Enabled = true;
                dataSourceGroup.Text = "Data Source";
            }
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets and sets the source to read data from.
        /// </summary>
        public DataSource DataSource
        {
            get
            {
                if (dataSourceLogs.Checked == true)
                    return DataSource.Log;
                else
                    return DataSource.Ram;
            }
            protected set
            {
                if (value == DataSource.Ram)
                    dataSourceRam.Checked = true;
                else
                    dataSourceLogs.Checked = true;
            }
        }

        /// <summary>
        /// Gets the default memory offset for chat log data within the
        /// FFXI process space.
        /// </summary>
        public uint MemoryOffset
        {
            get
            {
                // For reference: Default at the moment is 0x55AEC8

                uint result = 0;
                System.Globalization.NumberFormatInfo nfi = System.Globalization.CultureInfo.CurrentCulture.NumberFormat;

                if (uint.TryParse(memoryOffsetAddress.Text, System.Globalization.NumberStyles.HexNumber, nfi, out result) == true)
                    return result;
                else
                    return 0;

            }
        }

        /// <summary>
        /// If reading from the log directory, indicate whether we want to
        /// read the logs that are already there, or only read new ones as
        /// they come in.
        /// </summary>
        public bool ParseExistingLogs
        {
            get
            {
                return readExistingLogs.Checked;
            }
        }
        #endregion

        #region Event Handlers
        private void Options_Load(object sender, EventArgs e)
        {
            // Adjust the fields that are to be enabled when the form first loads.
            SetEnabledFields();
        }

        private void dataSourceLogs_CheckedChanged(object sender, EventArgs e)
        {
            // Adjust the fields that are to be enabled based on this setting.
            SetEnabledFields();
        }

        private void reset_Click(object sender, EventArgs e)
        {
            // Reset the app settings and refill the window data.
            windowSettings.Reset();
            LoadSettingsValues();
        }

        private void Options_FormClosing(object sender, FormClosingEventArgs e)
        {
            // If user is closing the window, save all data back to the program settings object.
            if ((e.CloseReason == CloseReason.UserClosing) ||
                (e.CloseReason == CloseReason.None))
            {
                if (this.DialogResult == DialogResult.OK)
                {
                    coreSettings.ParseMode = this.DataSource;

                    if (Directory.Exists(logDirectory.Text) == true)
                        coreSettings.FFXILogDirectory = logDirectory.Text;
                    else
                        e.Cancel = true;

                    if (this.MemoryOffset != 0)
                        coreSettings.MemoryOffset = this.MemoryOffset;
                    else
                        e.Cancel = true;

                    coreSettings.ParseExistingLogs = this.ParseExistingLogs;
                }
            }
        }

        private void Options_FormClosed(object sender, FormClosedEventArgs e)
        {
            // If form closed and user hit OK, save settings.
            if ((e.CloseReason == CloseReason.UserClosing) ||
                (e.CloseReason == CloseReason.None))
            {
                if (this.DialogResult == DialogResult.OK)
                {
                    windowSettings.Save();
                }
            }
        }
        #endregion

        #region Private methods
        private void LoadSettingsValues()
        {
            // Put the values from the app settings into the form.
            this.DataSource = coreSettings.ParseMode;
            logDirectory.Text = coreSettings.FFXILogDirectory;
            memoryOffsetAddress.Text = string.Format("{0:X8}", coreSettings.MemoryOffset);

            readExistingLogs.Checked = coreSettings.ParseExistingLogs;
        }

        private void SetEnabledFields()
        {
            // Enable/disable these controls based on whether the
            // option to read from logs is set.
            directoryLabel.Enabled = dataSourceLogs.Checked;
            logDirectory.Enabled = dataSourceLogs.Checked;
            getLogDirectory.Enabled = dataSourceLogs.Checked;
            logFileGroup.Enabled = dataSourceLogs.Checked;

            // Enable/disable these controls based on whether the
            // option to read from memory is set.
            memoryLabel.Enabled = dataSourceRam.Checked;
            memoryOffsetAddress.Enabled = dataSourceRam.Checked;
            getMemoryAddress.Enabled = dataSourceRam.Checked;
        }
        #endregion

    }
}