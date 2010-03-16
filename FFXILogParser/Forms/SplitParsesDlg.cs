using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WaywardGamers.KParser.Database;

namespace WaywardGamers.KParser.Forms
{
    public partial class SplitParsesDlg : Form
    {
        public SplitParsesDlg()
        {
            InitializeComponent();
        }

        DateTime firstTime = DateTime.MinValue;
        DateTime lastTime = DateTime.MinValue;

        private void SplitParsesDlg_Load(object sender, EventArgs e)
        {
            if (DatabaseManager.Instance.IsDatabaseOpen)
            {
                using (Database.AccessToTheDatabase dbAccess = new AccessToTheDatabase())
                {
                    var firstLine = dbAccess.Database.RecordLog.FirstOrDefault();
                    if (firstLine != null)
                    {
                        firstTime = firstLine.Timestamp;

                        var lastLine = dbAccess.Database.RecordLog.Last();
                        lastTime = lastLine.Timestamp;
                    }

                    if (dbAccess.Database.Battles.Any(b => b.DefaultBattle == false) == true)
                    {
                        startAtStartOfFight.Enabled = true;
                        startAtEndOfFight.Enabled = true;
                        endAtStartOfFight.Enabled = true;
                        endAtEndOfFight.Enabled = true;

                        LoadBattleLists(dbAccess);
                    }
                }
            }

            ResetFirstTimePicker();
            ResetLastTimePicker();
        }

        private void LoadBattleLists(Database.AccessToTheDatabase dbAccess)
        {
            var mobsKilledStringList = from b in dbAccess.Database.Battles
                                       where b.DefaultBattle == false
                                       orderby b.BattleID
                                       let actions = b.GetInteractionsRows()
                                       select string.Format("{0,3}: {1}  [{2} - {3}]",
                                            b.BattleID,
                                            b.CombatantsRowByEnemyCombatantRelation.CombatantName,
                                            actions.First().Timestamp.ToLocalTime().ToShortTimeString(),
                                            actions.Last().Timestamp.ToLocalTime().ToShortTimeString());

            startFightCombo.Items.Clear();
            endFightCombo.Items.Clear();
            startFightCombo.Items.AddRange(mobsKilledStringList.ToArray<string>());
            endFightCombo.Items.AddRange(mobsKilledStringList.ToArray<string>());

            startFightCombo.SelectedIndex = 0;
            endFightCombo.SelectedIndex = endFightCombo.Items.Count - 1;
        }

        private void ResetFirstTimePicker()
        {
            if (firstTime > DateTime.MinValue)
                startDateTimePicker.Value = firstTime;
            else
                startAtTime.Enabled = false;
        }

        private void ResetLastTimePicker()
        {
            if (lastTime > DateTime.MinValue)
                endDateTimePicker.Value = lastTime;
            else
                endAtTime.Enabled = false;
        }

        private void startPointType_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton == null)
                return;

            if (radioButton == startAtBeginningOfParse)
            {
            }
            else if (radioButton == startAtStartOfFight)
            {
                startFightCombo.Enabled = radioButton.Checked;
            }
            else if (radioButton == startAtEndOfFight)
            {
                startFightCombo.Enabled = radioButton.Checked;
            }
            else if (radioButton == startAtTime)
            {
                startDateTimePicker.Enabled = radioButton.Checked;
            }
        }

        private void endPointType_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton == null)
                return;

            if (radioButton == endAtEndOfParse)
            {
            }
            else if (radioButton == endAtStartOfFight)
            {
                endFightCombo.Enabled = radioButton.Checked;
            }
            else if (radioButton == endAtEndOfFight)
            {
                endFightCombo.Enabled = radioButton.Checked;
            }
            else if (radioButton == endAtTime)
            {
                endDateTimePicker.Enabled = radioButton.Checked;
            }
        }
    }
}
