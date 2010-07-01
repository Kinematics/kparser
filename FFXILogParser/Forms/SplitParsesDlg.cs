using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using WaywardGamers.KParser.Database;

namespace WaywardGamers.KParser.Forms
{
    public partial class SplitParsesDlg : Form
    {
        #region Constructor
        public SplitParsesDlg()
        {
            InitializeComponent();

            StartBoundary = DateTime.MinValue;
            EndBoundary = DateTime.MaxValue;
        }
        #endregion

        #region Member Variables
        // Boundaries for selected portions of the parse to extract
        DateTime firstTime = DateTime.MinValue;
        DateTime lastTime = DateTime.MinValue;

        Dictionary<int, DateTime> BattleStartTimes = new Dictionary<int, DateTime>();
        Dictionary<int, DateTime> BattleEndTimes = new Dictionary<int, DateTime>();
        #endregion

        #region Properties
        public DateTime StartBoundary { get; private set; }
        public DateTime EndBoundary { get; private set; }
        #endregion

        #region Event Handlers
        private void SplitParsesDlg_Load(object sender, EventArgs e)
        {
            ResetAndEnable();
        }

        /// <summary>
        /// Adjust which elements of the starting group are enabled based on
        /// which radio buttons are active.
        /// </summary>
        /// <param name="sender">The radio button that had its state changed.</param>
        /// <param name="e"></param>
        private void startPointType_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton == null)
                return;

            if (radioButton == startAtBeginningOfParse)
            {
                if (radioButton.Checked)
                    StartBoundary = firstTime;
            }
            else if (radioButton == startAtStartOfFight)
            {
                startFightCombo.Enabled = radioButton.Checked;

                if (radioButton.Checked)
                    SetStartBoundaryForStartCombo();
            }
            else if (radioButton == startAtEndOfFight)
            {
                startFightCombo.Enabled = radioButton.Checked;

                if (radioButton.Checked)
                    SetStartBoundaryForStartCombo();
            }
            else if (radioButton == startAtTime)
            {
                startDateTimePicker.Enabled = radioButton.Checked;

                if (radioButton.Checked)
                    StartBoundary = startDateTimePicker.Value.ToUniversalTime();
            }
        }

        /// <summary>
        /// Adjust which elements of the ending group are enabled based on
        /// which radio buttons are active.
        /// </summary>
        /// <param name="sender">The radio button that had its state changed.</param>
        /// <param name="e"></param>
        private void endPointType_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton == null)
                return;

            if (radioButton == endAtEndOfParse)
            {
                if (radioButton.Checked)
                    EndBoundary = lastTime;
            }
            else if (radioButton == endAtStartOfFight)
            {
                endFightCombo.Enabled = radioButton.Checked;

                if (radioButton.Checked)
                    SetEndBoundaryForEndCombo();
            }
            else if (radioButton == endAtEndOfFight)
            {
                endFightCombo.Enabled = radioButton.Checked;

                if (radioButton.Checked)
                    SetEndBoundaryForEndCombo();
            }
            else if (radioButton == endAtTime)
            {
                endDateTimePicker.Enabled = radioButton.Checked;

                if (radioButton.Checked)
                    EndBoundary = endDateTimePicker.Value.ToUniversalTime();
            }
        }

        private void startFightCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetStartBoundaryForStartCombo();
        }

        private void endFightCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetEndBoundaryForEndCombo();
        }

        private void startDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            if (startAtTime.Checked)
                StartBoundary = startDateTimePicker.Value.ToUniversalTime();
        }

        private void endDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            if (endAtTime.Checked)
                EndBoundary = endDateTimePicker.Value.ToUniversalTime();
        }

        /// <summary>
        /// Force a reset of the values of the dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void resetButton_Click(object sender, EventArgs e)
        {
            ResetAndEnable();
        }

        private void SplitParsesDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((e.CloseReason == CloseReason.UserClosing) ||
                (e.CloseReason == CloseReason.None))
            {
                if (this.DialogResult == DialogResult.OK)
                {
                    // validate boundaries
                    if (StartBoundary > EndBoundary)
                    {
                        MessageBox.Show("Start time cannot be after end time.", "Invalid limits", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        e.Cancel = true;
                    }
                }
            }
        }
        #endregion

        #region Private Methods
        private void ResetAndEnable()
        {
            // Only attempt to load info if a parse is open but not running.
            if ((DatabaseManager.Instance.IsDatabaseOpen == true) &&
                (Monitoring.Monitor.Instance.IsRunning == false))
            {
                startGroup.Enabled = true;
                endGroup.Enabled = true;
                acceptButton.Enabled = true;

                using (Database.AccessToTheDatabase dbAccess = new AccessToTheDatabase())
                {
                    if (dbAccess.HasAccess)
                    {
                        // Determine the first and last timestamp of the database
                        var firstLine = dbAccess.Database.RecordLog.FirstOrDefault();
                        bool hasData = (firstLine != null);

                        startAtBeginningOfParse.Enabled = hasData;
                        endAtEndOfParse.Enabled = hasData;
                        startAtTime.Enabled = hasData;
                        endAtTime.Enabled = hasData;

                        if (hasData == true)
                        {
                            firstTime = firstLine.Timestamp;

                            var lastLine = dbAccess.Database.RecordLog.Last();
                            lastTime = lastLine.Timestamp;

                            // Set these values as the min/max value for the datetimepickers.
                            startDateTimePicker.MinDate = firstTime.ToLocalTime();
                            startDateTimePicker.MaxDate = lastTime.ToLocalTime();
                            startDateTimePicker.Value = firstTime.ToLocalTime();
                            endDateTimePicker.MinDate = firstTime.ToLocalTime();
                            endDateTimePicker.MaxDate = lastTime.ToLocalTime();
                            endDateTimePicker.Value = lastTime.ToLocalTime();

                            // And update the default choices and boundaries with these values.
                            startAtBeginningOfParse.Checked = true;
                            endAtEndOfParse.Checked = true;
                            StartBoundary = firstTime;
                            EndBoundary = lastTime;
                        }
                        else
                        {
                            // If there's nothing in the database, default the values and return.
                            firstTime = DateTime.MinValue;
                            lastTime = DateTime.MinValue;

                            // Disable everything if there's nothing in the database.
                            startGroup.Enabled = false;
                            endGroup.Enabled = false;
                            acceptButton.Enabled = false;

                            return;
                        }

                        // If there are any fights in the database, enable the options
                        // to use those as demarcations, and load the battle list.
                        bool hasBattles = dbAccess.Database.Battles.Any(b => b.DefaultBattle == false);

                        startAtStartOfFight.Enabled = hasBattles;
                        startAtEndOfFight.Enabled = hasBattles;
                        endAtStartOfFight.Enabled = hasBattles;
                        endAtEndOfFight.Enabled = hasBattles;

                        if (hasBattles == true)
                        {
                            LoadBattleLists(dbAccess);
                        }
                    }
                }
            }
            else
            {
                // Disable everything if basic conditions aren't met.
                startGroup.Enabled = false;
                endGroup.Enabled = false;
                acceptButton.Enabled = false;
            }
        }

        private void LoadBattleLists(Database.AccessToTheDatabase dbAccess)
        {
            BattleStartTimes.Clear();
            BattleEndTimes.Clear();
            startFightCombo.Items.Clear();
            endFightCombo.Items.Clear();

            var battleTimes = from b in dbAccess.Database.Battles
                              let actions = b.GetInteractionsRows()
                              where b.DefaultBattle == false &&
                                    actions.Count() > 0
                              orderby b.BattleID
                              select new
                              {
                                  BattleID = b.BattleID,
                                  EnemyName = b.CombatantsRowByEnemyCombatantRelation.CombatantName,
                                  FirstActionTime = actions.First().Timestamp,
                                  LastActionTime = actions.Last().Timestamp
                              };

            foreach (var b in battleTimes)
            {
                BattleStartTimes.Add(b.BattleID, b.FirstActionTime);
                BattleEndTimes.Add(b.BattleID, b.LastActionTime);
            }


            var battleStringList = from b in battleTimes
                                   orderby b.BattleID
                                   select string.Format("{0,3}: {1}  [{2} - {3}]",
                                        b.BattleID,
                                        b.EnemyName,
                                        b.FirstActionTime.ToLocalTime().ToShortTimeString(),
                                        b.LastActionTime.ToLocalTime().ToShortTimeString());


            startFightCombo.Items.AddRange(battleStringList.ToArray<string>());
            endFightCombo.Items.AddRange(battleStringList.ToArray<string>());

            startFightCombo.SelectedIndex = 0;
            endFightCombo.SelectedIndex = endFightCombo.Items.Count - 1;
        }

        private int GetCurrentBattleID(ComboBox comboBox)
        {
            if (comboBox == null)
                throw new ArgumentNullException();

            if (comboBox.Items.Count == 0)
                throw new ArgumentOutOfRangeException();

            if (comboBox.SelectedIndex < 0)
                throw new ArgumentOutOfRangeException();

            Regex bidRegex = new Regex(@"(?<battleID>\d+): .*");

            Match bidMatch = bidRegex.Match(comboBox.SelectedItem.ToString());

            if (bidMatch.Success)
            {
                int battleID = int.Parse(bidMatch.Groups["battleID"].Value);
                return battleID;
            }
            else
            {
                throw new ArgumentException("Invalid text data");
            }
        }

        private void SetStartBoundaryForStartCombo()
        {
            if (startFightCombo.Items.Count == 0)
                return;

            if (startFightCombo.SelectedIndex < 0)
                return;

            int battleID = GetCurrentBattleID(startFightCombo);

            if (startAtStartOfFight.Checked == true)
            {
                StartBoundary = BattleStartTimes[battleID];
            }
            else if (startAtEndOfFight.Checked == true)
            {
                StartBoundary = BattleEndTimes[battleID].AddMilliseconds(1);
            }
        }

        private void SetEndBoundaryForEndCombo()
        {
            if (endFightCombo.Items.Count == 0)
                return;

            if (endFightCombo.SelectedIndex < 0)
                return;

            int battleID = GetCurrentBattleID(endFightCombo);

            if (endAtStartOfFight.Checked == true)
            {
                EndBoundary = BattleStartTimes[battleID].AddMilliseconds(-1);
            }
            else if (endAtEndOfFight.Checked == true)
            {
                EndBoundary = BattleEndTimes[battleID];
            }
        }
        #endregion
    }
}
