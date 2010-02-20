using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using WaywardGamers.KParser;
using WaywardGamers.KParser.Database;

namespace WaywardGamers.KParser.Plugin
{
    public class CorsairPlugin : BasePluginControl
    {
        #region Member variables
        bool flagNoUpdate = false;

        // UI controls
        ToolStripLabel playersLabel = new ToolStripLabel();
        ToolStripComboBox playersCombo = new ToolStripComboBox();

        // Localized strings

        string lsAll;

        string lsTotals;
        string lsAllRolls;
        string lsRollFrequency;
        string lsDoubleUpFrequency;

        string lsFullRollHeader;
        string lsSingleRollValueHeader;
        string lsSingleRollInitialHeader;

        string lsLongFrequencyFormat;
        string lsLongPercentageFormat;
        string lsAverageFormat;
        string lsShortFormat;
        string lsShortPercentageFormat;
        string lsFrequency;
        #endregion

        #region Constructor
        public CorsairPlugin()
        {
            LoadLocalizedUI();

            playersCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            playersCombo.MaxDropDownItems = 10;
            playersCombo.SelectedIndexChanged += new EventHandler(this.playersCombo_SelectedIndexChanged);

            toolStrip.Items.Add(playersLabel);
            toolStrip.Items.Add(playersCombo);
        }
        #endregion

        #region IPlugin Overrides
        public override void Reset()
        {
            ResetTextBox();

            playersCombo.Items.Clear();
            playersCombo.Items.Add(lsAll);
            flagNoUpdate = true;
            playersCombo.SelectedIndex = 0;
        }

        public override void NotifyOfUpdate()
        {
            ResetTextBox();

            UpdatePlayerList();

            playersCombo.CBSelectIndex(0);
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            bool changesFound = false;
            string currentlySelectedPlayer = lsAll;

            if (playersCombo.CBSelectedIndex() > 0)
                currentlySelectedPlayer = playersCombo.CBSelectedItem();

            if (e.DatasetChanges.Combatants != null)
            {
                if (e.DatasetChanges.Combatants.Any(x => x.RowState == DataRowState.Added))
                {
                    UpdatePlayerList();
                    changesFound = true;

                    flagNoUpdate = true;
                    playersCombo.CBSelectIndex(0);
                }
            }

            if (e.DatasetChanges.Interactions != null)
            {
                if (e.DatasetChanges.Interactions.Any(x => x.RowState == DataRowState.Added))
                {
                    changesFound = true;
                }
            }

            if (currentlySelectedPlayer != playersCombo.CBSelectedItem())
            {
                flagNoUpdate = true;
                playersCombo.CBSelectItem(currentlySelectedPlayer);
            }

            if (changesFound == true)
            {
                HandleDataset(null);
            }
        }
        #endregion

        #region Private functions
        private void UpdatePlayerList()
        {
            playersCombo.CBReset();
            playersCombo.CBAddStrings(GetPlayerListing());
        }
        #endregion

        #region Processing and Display functions
        /// <summary>
        /// General branching for processing data
        /// </summary>
        /// <param name="dataSet"></param>
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            if (dataSet == null)
                return;

            // If we get here during initialization, skip.
            if (playersCombo.Items.Count == 0)
                return;

            ResetTextBox();

            string selectedPlayer = playersCombo.CBSelectedItem();

            List<string> playerList = new List<string>();

            if (selectedPlayer == lsAll)
            {
                foreach (var player in playersCombo.CBGetStrings())
                {
                    if (player != lsAll)
                        playerList.Add(player.ToString());
                }
            }
            else
            {
                playerList.Add(selectedPlayer);
            }

            if (playerList.Count == 0)
                return;

            string[] selectedPlayers = playerList.ToArray();

            IEnumerable<AttackGroup> rollSet;

            rollSet = from c in dataSet.Combatants
                        where (((EntityType)c.CombatantType == EntityType.Player) &&
                               (selectedPlayers.Contains(c.CombatantName)))
                        orderby c.CombatantName
                        select new AttackGroup
                        {
                            Name = c.CombatantNameOrJobName,
                            ComType = (EntityType)c.CombatantType,
                            Ability = from q in c.GetInteractionsRowsByActorCombatantRelation()
                                      where (q.IsActionIDNull() == false &&
                                             Regex.Match(q.ActionsRow.ActionName, Resources.ParsedStrings.AnyRoll).Success == true)
                                      select q,
                        };


            ProcessCombatSet(rollSet);
        }

        private void ProcessCombatSet(IEnumerable<AttackGroup> rollSet)
        {
            StringBuilder sb = new StringBuilder();
            List<StringMods> strModList = new List<StringMods>();

            string localDoubleUp = Resources.ParsedStrings.DoubleUp;
            string localBust = Resources.ParsedStrings.Bust;

            foreach (var player in rollSet)
            {
                if (player.Ability.Count() > 0)
                {
                    strModList.Add(new StringMods
                    {
                        Start = sb.Length,
                        Length = player.Name.Length,
                        Bold = true,
                        Color = Color.Blue
                    });
                    sb.AppendFormat("{0}\n\n", player.Name);

                    int[] rollCounter = new int[13];

                    var groupedRolls = from r in player.Ability
                                       group r by r.Timestamp into gr
                                       select gr.First();

                    var groupedRollSets = groupedRolls.
                        GroupAdjacentWithComparer<KPDatabaseDataSet.InteractionsRow, string>
                        (i => i.ActionsRow.ActionName, d => d == localDoubleUp || d == localBust);


                    var finalRollValues = groupedRollSets.Select(r => r.Last());

                    var finalRollGroups = finalRollValues.GroupBy(r => r.Amount);

                    foreach (var rollGroup in finalRollGroups)
                    {
                        rollCounter[rollGroup.Key] = rollGroup.Count();
                    }

                    strModList.Add(new StringMods
                    {
                        Start = sb.Length,
                        Length = lsTotals.Length,
                        Bold = true,
                        Color = Color.Red
                    });
                    sb.AppendFormat("{0}\n\n", lsTotals);

                    OutputRollCounter(rollCounter, sb, strModList, lsAllRolls);
                    sb.Append("\n\n");


                    var finalRollValuesByRoll = finalRollValues
                        .GroupBy(r => r.IsSecondActionIDNull() == false ?
                            r.ActionsRowBySecondaryActionNameRelation.ActionName : 
                            r.ActionsRow.ActionName)
                        .OrderBy(r => r.Key);

                    foreach (var roll in finalRollValuesByRoll)
                    {
                        Array.Clear(rollCounter, 0, rollCounter.Length);
                        var rollGroups = roll.GroupBy(r => r.Amount);

                        foreach (var rollGroup in rollGroups)
                        {
                            rollCounter[rollGroup.Key] = rollGroup.Count();
                        }

                        OutputRollCounter(rollCounter, sb, strModList, roll.Key);

                        sb.Append("\n\n");
                    }


                    var individualRolls = groupedRollSets.Select(a =>
                        a.Separate(r => r.Amount, (b, c) => b.Amount - c));


                    int[] perRollCounter = new int[7];

                    foreach (var iRoll in individualRolls)
                    {
                        foreach (var iR in iRoll)
                        {
                            if ((iR < perRollCounter.Length) && (iR >= 0))
                                perRollCounter[iR]++;
                        }
                    }

                    strModList.Add(new StringMods
                    {
                        Start = sb.Length,
                        Length = lsRollFrequency.Length,
                        Bold = true,
                        Color = Color.Red
                    });
                    sb.AppendFormat("{0}\n\n", lsRollFrequency);


                    OutputPerRollCounter(perRollCounter, sb, strModList, lsAllRolls);

                    sb.Append("\n\n");

                    var followupRolls = individualRolls.Where(a => a.Count() > 1);

                    int[,] followupTable = new int[7, 7];

                    foreach (var fRoll in followupRolls)
                    {
                        int first = fRoll.First();
                        int second = fRoll.Skip(1).First();

                        // Busts are listed as 0, and will generate negative values
                        // when subtracted from previous roll
                        // The only way to get a bust on the second roll is to
                        // roll two sixes in a row, so setting to 6 here.
                        if (second <= 0)
                            second = 6;

                        followupTable[first, second]++;
                    }

                    OutputFollowupCounter(followupTable, sb, strModList, lsDoubleUpFrequency);

                }
            }

            sb.Append("\n");
            PushStrings(sb, strModList);

        }

        private void OutputRollCounter(int[] rollCounter, StringBuilder sb,
            List<StringMods> strModList, string leadText)
        {
            int totalNumberOfRolls = rollCounter.Sum();

            if (totalNumberOfRolls == 0)
                return;

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = leadText.Length,
                Bold = true,
                Color = Color.Blue
            });
            sb.Append(leadText);
            sb.Append("\n");
            

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsFullRollHeader.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(lsFullRollHeader);
            sb.Append("\n");

            sb.AppendFormat(lsLongFrequencyFormat,
                rollCounter[1],
                rollCounter[2],
                rollCounter[3],
                rollCounter[4],
                rollCounter[5],
                rollCounter[6],
                rollCounter[7],
                rollCounter[8],
                rollCounter[9],
                rollCounter[10],
                rollCounter[11],
                rollCounter[0]);
            sb.Append("\n");

            sb.AppendFormat(lsLongPercentageFormat,
                (double)rollCounter[1] * 100 / totalNumberOfRolls,
                (double)rollCounter[2] * 100 / totalNumberOfRolls,
                (double)rollCounter[3] * 100 / totalNumberOfRolls,
                (double)rollCounter[4] * 100 / totalNumberOfRolls,
                (double)rollCounter[5] * 100 / totalNumberOfRolls,
                (double)rollCounter[6] * 100 / totalNumberOfRolls,
                (double)rollCounter[7] * 100 / totalNumberOfRolls,
                (double)rollCounter[8] * 100 / totalNumberOfRolls,
                (double)rollCounter[9] * 100 / totalNumberOfRolls,
                (double)rollCounter[10] * 100 / totalNumberOfRolls,
                (double)rollCounter[11] * 100 / totalNumberOfRolls,
                (double)rollCounter[0] * 100 / totalNumberOfRolls);
            sb.Append("\n");

            int weightedSum = 0;

            for (int i = 0; i < rollCounter.Length; i++)
            {
                weightedSum += i * rollCounter[i];
            }

            sb.AppendFormat(lsAverageFormat, (double)weightedSum / totalNumberOfRolls);
            sb.Append("\n");

        }

        private void OutputPerRollCounter(int[] perRollCounter, StringBuilder sb,
            List<StringMods> strModList, string leadText)
        {
            int totalNumberOfRolls = perRollCounter.Sum();

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = leadText.Length,
                Bold = true,
                Color = Color.Blue
            });
            sb.Append(leadText);
            sb.Append("\n");

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsSingleRollValueHeader.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(lsSingleRollValueHeader);
            sb.Append("\n");

            sb.AppendFormat(lsShortFormat,
                lsFrequency,
                perRollCounter[1],
                perRollCounter[2],
                perRollCounter[3],
                perRollCounter[4],
                perRollCounter[5],
                perRollCounter[6]);
            sb.Append("\n");

            sb.AppendFormat(lsShortPercentageFormat,
                (double)perRollCounter[1] * 100 / totalNumberOfRolls,
                (double)perRollCounter[2] * 100 / totalNumberOfRolls,
                (double)perRollCounter[3] * 100 / totalNumberOfRolls,
                (double)perRollCounter[4] * 100 / totalNumberOfRolls,
                (double)perRollCounter[5] * 100 / totalNumberOfRolls,
                (double)perRollCounter[6] * 100 / totalNumberOfRolls);
            sb.Append("\n");

        }

        private void OutputFollowupCounter(int[,] followupCounter, StringBuilder sb,
            List<StringMods> strModList, string leadText)
        {
            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = leadText.Length,
                Bold = true,
                Color = Color.Blue
            });
            sb.Append(leadText);
            sb.Append("\n");

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsSingleRollInitialHeader.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(lsSingleRollInitialHeader);
            sb.Append("\n");

            for (int i = 1; i <= 6; i++)
            {
                sb.AppendFormat(lsShortFormat,
                    i,
                    followupCounter[i, 1],
                    followupCounter[i, 2],
                    followupCounter[i, 3],
                    followupCounter[i, 4],
                    followupCounter[i, 5],
                    followupCounter[i, 6]);
                sb.Append("\n");
            }
        }

        #endregion


        #region Event Handlers
        protected void playersCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(null);

            flagNoUpdate = false;
        }
        #endregion


        #region Localization Overrides
        protected override void LoadLocalizedUI()
        {
            playersLabel.Text = Resources.PublicResources.PlayersLabel;

            UpdatePlayerList();
            playersCombo.SelectedIndex = 0;
        }

        protected override void LoadResources()
        {
            this.tabName = Resources.Combat.CorsairPluginTabName;

            lsAll = Resources.PublicResources.All;

            lsTotals = Resources.Combat.CorsairPluginTotals;
            lsAllRolls = Resources.Combat.CorsairPluginAllRolls;
            lsRollFrequency = Resources.Combat.CorsairPluginRollFrequency;
            lsDoubleUpFrequency = Resources.Combat.CorsairPluginDoubleUpFrequency;

            lsFullRollHeader = Resources.Combat.CorsairPluginFullRollHeader;
            lsSingleRollValueHeader = Resources.Combat.CorsairPluginSingleRollValueHeader;
            lsSingleRollInitialHeader = Resources.Combat.CorsairPluginSingleRollInitialHeader;

            lsLongFrequencyFormat = Resources.Combat.CorsairPluginLongFrequencyFormat;
            lsLongPercentageFormat = Resources.Combat.CorsairPluginLongFloatFormat;
            lsAverageFormat = Resources.Combat.CorsairPluginAverageFormat;
            lsShortPercentageFormat = Resources.Combat.CorsairPluginShortPercentageFormat;
            lsShortFormat = Resources.Combat.CorsairPluginShortFormat;
            lsFrequency = Resources.Combat.CorsairPluginFrequency;
        }
        #endregion

    }
}
