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

        // Roll data
        string[] rollNames;
        bool[] hasValuesWithJobInParty = new bool[26];
        bool[] hasValuesWithoutJobInParty = new bool[26];
        int[,] valuesWithJobInParty = new int[26, 12];
        int[,] valuesWithoutJobInParty = new int[26, 12];

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

        string lsAvgRollHeader;
        string lsAvgRollAsMainFormat;
        string lsAvgRollAsSubFormat;
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

            InitializeRollData();
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
                if (player.Ability.Any())
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

                        sb.Append("\n");

                        OutputRollAverages(rollCounter, sb, strModList, roll.Key);

                        sb.Append("\n");
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

                        // In some cases intermediary rolls may have been missed,
                        // in which case the followup roll may appear to be
                        // greater than 6.  We can't adjust for that, so skip.
                        if (second > 6)
                            continue;

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

        private void OutputRollAverages(int[] rollCounter, StringBuilder sb, List<StringMods> strModList, string rollName)
        {
            int jobIndex = Array.IndexOf(rollNames, rollName);
            
            if (jobIndex < 0)
                return;

            if ((hasValuesWithJobInParty[jobIndex] == false) && (hasValuesWithoutJobInParty[jobIndex] == false))
                return;

            int totalNumberOfRolls = rollCounter.Sum();

            if (totalNumberOfRolls == 0)
                return;

            int sumWithMain = 0;
            int sumWithSub = 0;
            int sumWithoutMain = 0;
            int sumWithoutSub = 0;

            int valWithJob;
            int valWithoutJob;

            for (int i = 1; i <= 11; i++)
            {
                GetRollValue(rollName, i, out valWithJob, out valWithoutJob);

                sumWithMain += valWithJob * rollCounter[i];
                sumWithoutMain += valWithoutJob * rollCounter[i];
                sumWithSub += (valWithJob / 2) * rollCounter[i];
                sumWithoutSub += (valWithoutJob / 2) * rollCounter[i];
            }

            // Only rolls up to lvl 37 can be subbed.
            if (jobIndex > 10)
            {
                sumWithSub = 0;
                sumWithoutSub = 0;
            }


            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsAvgRollHeader.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(lsAvgRollHeader);
            sb.Append("\n");


            sb.AppendFormat(lsAvgRollAsMainFormat,
                (double)sumWithMain / totalNumberOfRolls,
                (double)sumWithoutMain / totalNumberOfRolls
                );
            sb.Append("\n");

            if (jobIndex < 11)
            {
                sb.AppendFormat(lsAvgRollAsSubFormat,
                    (double)sumWithSub / totalNumberOfRolls,
                    (double)sumWithoutSub / totalNumberOfRolls
                    );
                sb.Append("\n");
            }

            sb.Append("\n");

        }

        #endregion

        #region Roll Data
        /// <summary>
        /// Set roll names to localized versions of the roll names so 
        /// we can find them on lookup.
        /// </summary>
        private void InitializeRollNames()
        {
            rollNames = new string[26] {
                Resources.ParsedStrings.CorRoll,
                Resources.ParsedStrings.NinRoll,
                Resources.ParsedStrings.RngRoll,
                Resources.ParsedStrings.DrkRoll,
                Resources.ParsedStrings.BluRoll,
                Resources.ParsedStrings.WhmRoll,
                Resources.ParsedStrings.PupRoll,
                Resources.ParsedStrings.BrdRoll,
                Resources.ParsedStrings.MnkRoll,
                Resources.ParsedStrings.BstRoll,
                Resources.ParsedStrings.SamRoll,
                Resources.ParsedStrings.SmnRoll,
                Resources.ParsedStrings.ThfRoll,
                Resources.ParsedStrings.RdmRoll,
                Resources.ParsedStrings.WarRoll,
                Resources.ParsedStrings.DrgRoll,
                Resources.ParsedStrings.PldRoll,
                Resources.ParsedStrings.BlmRoll,
                Resources.ParsedStrings.DncRoll,
                Resources.ParsedStrings.SchRoll,
                Resources.ParsedStrings.BolterRoll,
                Resources.ParsedStrings.CasterRoll,
                Resources.ParsedStrings.BlitzerRoll,
                Resources.ParsedStrings.CourserRoll,
                Resources.ParsedStrings.TacticianRoll,
                Resources.ParsedStrings.AlliesRoll,
            };
        }

        private void InitializeRollData()
        {
            //bool[] hasValuesWithJobInParty = new bool[20];
            //bool[] hasValuesWithoutJobInParty = new bool[20];
            //int[,] valuesWithJobInParty = new int[20,12];
            //int[,] valuesWithoutJobInParty = new int[20,12];

            //Resources.ParsedStrings.CorRoll
            hasValuesWithJobInParty[0] = true;
            valuesWithJobInParty[0, 0] = -6;
            valuesWithJobInParty[0, 1] = 10;
            valuesWithJobInParty[0, 2] = 11;
            valuesWithJobInParty[0, 3] = 11;
            valuesWithJobInParty[0, 4] = 12;
            valuesWithJobInParty[0, 5] = 20;
            valuesWithJobInParty[0, 6] = 13;
            valuesWithJobInParty[0, 7] = 15;
            valuesWithJobInParty[0, 8] = 16;
            valuesWithJobInParty[0, 9] = 8;
            valuesWithJobInParty[0, 10] = 17;
            valuesWithJobInParty[0, 11] = 24;

            hasValuesWithoutJobInParty[0] = false;

            //Resources.ParsedStrings.NinRoll
            hasValuesWithJobInParty[1] = false;
            hasValuesWithoutJobInParty[1] = false;

            //Resources.ParsedStrings.RngRoll
            hasValuesWithJobInParty[2] = true;
            valuesWithJobInParty[2, 0] = -5;
            valuesWithJobInParty[2, 1] = 25;
            valuesWithJobInParty[2, 2] = 28;
            valuesWithJobInParty[2, 3] = 30;
            valuesWithJobInParty[2, 4] = 55;
            valuesWithJobInParty[2, 5] = 33;
            valuesWithJobInParty[2, 6] = 35;
            valuesWithJobInParty[2, 7] = 40;
            valuesWithJobInParty[2, 8] = 20;
            valuesWithJobInParty[2, 9] = 43;
            valuesWithJobInParty[2, 10] = 45;
            valuesWithJobInParty[2, 11] = 65;

            hasValuesWithoutJobInParty[2] = true;
            valuesWithoutJobInParty[2, 0] = -5;
            valuesWithoutJobInParty[2, 1] = 10;
            valuesWithoutJobInParty[2, 2] = 13;
            valuesWithoutJobInParty[2, 3] = 15;
            valuesWithoutJobInParty[2, 4] = 40;
            valuesWithoutJobInParty[2, 5] = 18;
            valuesWithoutJobInParty[2, 6] = 20;
            valuesWithoutJobInParty[2, 7] = 25;
            valuesWithoutJobInParty[2, 8] = 5;
            valuesWithoutJobInParty[2, 9] = 27;
            valuesWithoutJobInParty[2, 10] = 30;
            valuesWithoutJobInParty[2, 11] = 50;
            
            //Resources.ParsedStrings.DrkRoll
            hasValuesWithJobInParty[3] = true;
            valuesWithJobInParty[3, 0] = -10;
            valuesWithJobInParty[3, 1] = 16;
            valuesWithJobInParty[3, 2] = 18;
            valuesWithJobInParty[3, 3] = 19;
            valuesWithJobInParty[3, 4] = 35;
            valuesWithJobInParty[3, 5] = 21;
            valuesWithJobInParty[3, 6] = 22;
            valuesWithJobInParty[3, 7] = 25;
            valuesWithJobInParty[3, 8] = 13;
            valuesWithJobInParty[3, 9] = 27;
            valuesWithJobInParty[3, 10] = 29;
            valuesWithJobInParty[3, 11] = 41;

            hasValuesWithoutJobInParty[3] = true;
            valuesWithoutJobInParty[3, 0] = -10;
            valuesWithoutJobInParty[3, 1] = 6;
            valuesWithoutJobInParty[3, 2] = 8;
            valuesWithoutJobInParty[3, 3] = 9;
            valuesWithoutJobInParty[3, 4] = 25;
            valuesWithoutJobInParty[3, 5] = 11;
            valuesWithoutJobInParty[3, 6] = 13;
            valuesWithoutJobInParty[3, 7] = 16;
            valuesWithoutJobInParty[3, 8] = 3;
            valuesWithoutJobInParty[3, 9] = 17;
            valuesWithoutJobInParty[3, 10] = 19;
            valuesWithoutJobInParty[3, 11] = 31;
            
            //Resources.ParsedStrings.BluRoll
            hasValuesWithJobInParty[4] = true;
            valuesWithJobInParty[4, 0] = -5;
            valuesWithJobInParty[4, 1] = 13;
            valuesWithJobInParty[4, 2] = 28;
            valuesWithJobInParty[4, 3] = 14;
            valuesWithJobInParty[4, 4] = 16;
            valuesWithJobInParty[4, 5] = 17;
            valuesWithJobInParty[4, 6] = 11;
            valuesWithJobInParty[4, 7] = 18;
            valuesWithJobInParty[4, 8] = 21;
            valuesWithJobInParty[4, 9] = 22;
            valuesWithJobInParty[4, 10] = 23;
            valuesWithJobInParty[4, 11] = 33;

            hasValuesWithoutJobInParty[4] = true;
            valuesWithoutJobInParty[4, 0] = -5;
            valuesWithoutJobInParty[4, 1] = 5;
            valuesWithoutJobInParty[4, 2] = 20;
            valuesWithoutJobInParty[4, 3] = 6;
            valuesWithoutJobInParty[4, 4] = 8;
            valuesWithoutJobInParty[4, 5] = 9;
            valuesWithoutJobInParty[4, 6] = 3;
            valuesWithoutJobInParty[4, 7] = 10;
            valuesWithoutJobInParty[4, 8] = 13;
            valuesWithoutJobInParty[4, 9] = 14;
            valuesWithoutJobInParty[4, 10] = 15;
            valuesWithoutJobInParty[4, 11] = 25;

            //Resources.ParsedStrings.WhmRoll
            hasValuesWithJobInParty[5] = true;
            valuesWithJobInParty[5, 0] = -3;
            valuesWithJobInParty[5, 1] = 5;
            valuesWithJobInParty[5, 2] = 6;
            valuesWithJobInParty[5, 3] = 13;
            valuesWithJobInParty[5, 4] = 7;
            valuesWithJobInParty[5, 5] = 7;
            valuesWithJobInParty[5, 6] = 8;
            valuesWithJobInParty[5, 7] = 4;
            valuesWithJobInParty[5, 8] = 9;
            valuesWithJobInParty[5, 9] = 10;
            valuesWithJobInParty[5, 10] = 10;
            valuesWithJobInParty[5, 11] = 15;

            hasValuesWithoutJobInParty[5] = true;
            valuesWithoutJobInParty[5, 0] = -3;
            valuesWithoutJobInParty[5, 1] = 2;
            valuesWithoutJobInParty[5, 2] = 3;
            valuesWithoutJobInParty[5, 3] = 10;
            valuesWithoutJobInParty[5, 4] = 4;
            valuesWithoutJobInParty[5, 5] = 4;
            valuesWithoutJobInParty[5, 6] = 5;
            valuesWithoutJobInParty[5, 7] = 1;
            valuesWithoutJobInParty[5, 8] = 6;
            valuesWithoutJobInParty[5, 9] = 7;
            valuesWithoutJobInParty[5, 10] = 7;
            valuesWithoutJobInParty[5, 11] = 12;

            //Resources.ParsedStrings.PupRoll
            hasValuesWithJobInParty[6] = false;
            hasValuesWithoutJobInParty[6] = false;

            //Resources.ParsedStrings.BrdRoll
            hasValuesWithJobInParty[7] = false;
            hasValuesWithoutJobInParty[7] = false;

            //Resources.ParsedStrings.MnkRoll
            hasValuesWithJobInParty[8] = true;
            valuesWithJobInParty[8, 0] = -11;
            valuesWithJobInParty[8, 1] = 18;
            valuesWithJobInParty[8, 2] = 20;
            valuesWithJobInParty[8, 3] = 42;
            valuesWithJobInParty[8, 4] = 22;
            valuesWithJobInParty[8, 5] = 24;
            valuesWithJobInParty[8, 6] = 26;
            valuesWithJobInParty[8, 7] = 14;
            valuesWithJobInParty[8, 8] = 30;
            valuesWithJobInParty[8, 9] = 32;
            valuesWithJobInParty[8, 10] = 34;
            valuesWithJobInParty[8, 11] = 50;

            hasValuesWithoutJobInParty[8] = true;
            valuesWithoutJobInParty[8, 0] = -11;
            valuesWithoutJobInParty[8, 1] = 8;
            valuesWithoutJobInParty[8, 2] = 10;
            valuesWithoutJobInParty[8, 3] = 32;
            valuesWithoutJobInParty[8, 4] = 12;
            valuesWithoutJobInParty[8, 5] = 14;
            valuesWithoutJobInParty[8, 6] = 16;
            valuesWithoutJobInParty[8, 7] = 4;
            valuesWithoutJobInParty[8, 8] = 20;
            valuesWithoutJobInParty[8, 9] = 22;
            valuesWithoutJobInParty[8, 10] = 24;
            valuesWithoutJobInParty[8, 11] = 40;

            //Resources.ParsedStrings.BstRoll
            hasValuesWithJobInParty[9] = false;
            hasValuesWithoutJobInParty[9] = false;

            //Resources.ParsedStrings.SamRoll
            hasValuesWithJobInParty[10] = true;
            valuesWithJobInParty[10, 0] = -5;
            valuesWithJobInParty[10, 1] = 18;
            valuesWithJobInParty[10, 2] = 42;
            valuesWithJobInParty[10, 3] = 20;
            valuesWithJobInParty[10, 4] = 22;
            valuesWithJobInParty[10, 5] = 24;
            valuesWithJobInParty[10, 6] = 14;
            valuesWithJobInParty[10, 7] = 26;
            valuesWithJobInParty[10, 8] = 30;
            valuesWithJobInParty[10, 9] = 32;
            valuesWithJobInParty[10, 10] = 34;
            valuesWithJobInParty[10, 11] = 50;

            hasValuesWithoutJobInParty[10] = true;
            valuesWithoutJobInParty[10, 0] = -5;
            valuesWithoutJobInParty[10, 1] = 8;
            valuesWithoutJobInParty[10, 2] = 32;
            valuesWithoutJobInParty[10, 3] = 10;
            valuesWithoutJobInParty[10, 4] = 12;
            valuesWithoutJobInParty[10, 5] = 14;
            valuesWithoutJobInParty[10, 6] = 4;
            valuesWithoutJobInParty[10, 7] = 16;
            valuesWithoutJobInParty[10, 8] = 20;
            valuesWithoutJobInParty[10, 9] = 22;
            valuesWithoutJobInParty[10, 10] = 24;
            valuesWithoutJobInParty[10, 11] = 40;

            //Resources.ParsedStrings.SmnRoll
            hasValuesWithJobInParty[11] = true;
            valuesWithJobInParty[11, 0] = -1;
            valuesWithJobInParty[11, 1] = 2;
            valuesWithJobInParty[11, 2] = 2;
            valuesWithJobInParty[11, 3] = 2;
            valuesWithJobInParty[11, 4] = 2;
            valuesWithJobInParty[11, 5] = 4;
            valuesWithJobInParty[11, 6] = 3;
            valuesWithJobInParty[11, 7] = 3;
            valuesWithJobInParty[11, 8] = 3;
            valuesWithJobInParty[11, 9] = 2;
            valuesWithJobInParty[11, 10] = 4;
            valuesWithJobInParty[11, 11] = 5;

            hasValuesWithoutJobInParty[11] = true;
            valuesWithoutJobInParty[11, 0] = -1;
            valuesWithoutJobInParty[11, 1] = 1;
            valuesWithoutJobInParty[11, 2] = 1;
            valuesWithoutJobInParty[11, 3] = 1;
            valuesWithoutJobInParty[11, 4] = 1;
            valuesWithoutJobInParty[11, 5] = 3;
            valuesWithoutJobInParty[11, 6] = 2;
            valuesWithoutJobInParty[11, 7] = 2;
            valuesWithoutJobInParty[11, 8] = 2;
            valuesWithoutJobInParty[11, 9] = 1;
            valuesWithoutJobInParty[11, 10] = 3;
            valuesWithoutJobInParty[11, 11] = 4;

            //Resources.ParsedStrings.ThfRoll
            hasValuesWithJobInParty[12] = true;
            valuesWithJobInParty[12, 0] = -6;
            valuesWithJobInParty[12, 1] = 8;
            valuesWithJobInParty[12, 2] = 8;
            valuesWithJobInParty[12, 3] = 9;
            valuesWithJobInParty[12, 4] = 10;
            valuesWithJobInParty[12, 5] = 18;
            valuesWithJobInParty[12, 6] = 11;
            valuesWithJobInParty[12, 7] = 12;
            valuesWithJobInParty[12, 8] = 12;
            valuesWithJobInParty[12, 9] = 7;
            valuesWithJobInParty[12, 10] = 14;
            valuesWithJobInParty[12, 11] = 24;

            hasValuesWithoutJobInParty[12] = true;
            valuesWithoutJobInParty[12, 0] = -6;
            valuesWithoutJobInParty[12, 1] = 2;
            valuesWithoutJobInParty[12, 2] = 2;
            valuesWithoutJobInParty[12, 3] = 3;
            valuesWithoutJobInParty[12, 4] = 4;
            valuesWithoutJobInParty[12, 5] = 12;
            valuesWithoutJobInParty[12, 6] = 5;
            valuesWithoutJobInParty[12, 7] = 6;
            valuesWithoutJobInParty[12, 8] = 6;
            valuesWithoutJobInParty[12, 9] = 1;
            valuesWithoutJobInParty[12, 10] = 8;
            valuesWithoutJobInParty[12, 11] = 18;

            //Resources.ParsedStrings.RdmRoll
            hasValuesWithJobInParty[13] = false;
            hasValuesWithoutJobInParty[13] = false;

            //Resources.ParsedStrings.WarRoll
            hasValuesWithJobInParty[14] = true;
            valuesWithJobInParty[14, 0] = -6;
            valuesWithJobInParty[14, 1] = 8;
            valuesWithJobInParty[14, 2] = 8;
            valuesWithJobInParty[14, 3] = 9;
            valuesWithJobInParty[14, 4] = 10;
            valuesWithJobInParty[14, 5] = 18;
            valuesWithJobInParty[14, 6] = 11;
            valuesWithJobInParty[14, 7] = 12;
            valuesWithJobInParty[14, 8] = 13;
            valuesWithJobInParty[14, 9] = 7;
            valuesWithJobInParty[14, 10] = 15;
            valuesWithJobInParty[14, 11] = 24;

            hasValuesWithoutJobInParty[14] = true;
            valuesWithoutJobInParty[14, 0] = -6;
            valuesWithoutJobInParty[14, 1] = 2;
            valuesWithoutJobInParty[14, 2] = 2;
            valuesWithoutJobInParty[14, 3] = 3;
            valuesWithoutJobInParty[14, 4] = 4;
            valuesWithoutJobInParty[14, 5] = 12;
            valuesWithoutJobInParty[14, 6] = 5;
            valuesWithoutJobInParty[14, 7] = 6;
            valuesWithoutJobInParty[14, 8] = 7;
            valuesWithoutJobInParty[14, 9] = 1;
            valuesWithoutJobInParty[14, 10] = 9;
            valuesWithoutJobInParty[14, 11] = 18;

            //Resources.ParsedStrings.DrgRoll
            hasValuesWithJobInParty[15] = true;
            valuesWithJobInParty[15, 0] = -1;
            valuesWithJobInParty[15, 1] = 6;
            valuesWithJobInParty[15, 2] = 6;
            valuesWithJobInParty[15, 3] = 13;
            valuesWithJobInParty[15, 4] = 7;
            valuesWithJobInParty[15, 5] = 8;
            valuesWithJobInParty[15, 6] = 8;
            valuesWithJobInParty[15, 7] = 5;
            valuesWithJobInParty[15, 8] = 9;
            valuesWithJobInParty[15, 9] = 9;
            valuesWithJobInParty[15, 10] = 10;
            valuesWithJobInParty[15, 11] = 16;

            hasValuesWithoutJobInParty[15] = true;
            valuesWithoutJobInParty[15, 0] = -1;
            valuesWithoutJobInParty[15, 1] = 2;
            valuesWithoutJobInParty[15, 2] = 2;
            valuesWithoutJobInParty[15, 3] = 9;
            valuesWithoutJobInParty[15, 4] = 3;
            valuesWithoutJobInParty[15, 5] = 4;
            valuesWithoutJobInParty[15, 6] = 4;
            valuesWithoutJobInParty[15, 7] = 1;
            valuesWithoutJobInParty[15, 8] = 5;
            valuesWithoutJobInParty[15, 9] = 5;
            valuesWithoutJobInParty[15, 10] = 6;
            valuesWithoutJobInParty[15, 11] = 12;

            //Resources.ParsedStrings.PldRoll
            hasValuesWithJobInParty[16] = true;
            valuesWithJobInParty[16, 0] = 0;
            valuesWithJobInParty[16, 1] = 15;
            valuesWithJobInParty[16, 2] = 18;
            valuesWithJobInParty[16, 3] = 34;
            valuesWithJobInParty[16, 4] = 19;
            valuesWithJobInParty[16, 5] = 21;
            valuesWithJobInParty[16, 6] = 22;
            valuesWithJobInParty[16, 7] = 13;
            valuesWithJobInParty[16, 8] = 25;
            valuesWithJobInParty[16, 9] = 27;
            valuesWithJobInParty[16, 10] = 28;
            valuesWithJobInParty[16, 11] = 40;

            hasValuesWithoutJobInParty[16] = true;
            valuesWithoutJobInParty[16, 0] = 0;
            valuesWithoutJobInParty[16, 1] = 5;
            valuesWithoutJobInParty[16, 2] = 8;
            valuesWithoutJobInParty[16, 3] = 24;
            valuesWithoutJobInParty[16, 4] = 9;
            valuesWithoutJobInParty[16, 5] = 11;
            valuesWithoutJobInParty[16, 6] = 12;
            valuesWithoutJobInParty[16, 7] = 3;
            valuesWithoutJobInParty[16, 8] = 15;
            valuesWithoutJobInParty[16, 9] = 17;
            valuesWithoutJobInParty[16, 10] = 18;
            valuesWithoutJobInParty[16, 11] = 30;

            //Resources.ParsedStrings.BlmRoll
            hasValuesWithJobInParty[17] = true;
            valuesWithJobInParty[17, 0] = -4;
            valuesWithJobInParty[17, 1] = 6;
            valuesWithJobInParty[17, 2] = 6;
            valuesWithJobInParty[17, 3] = 7;
            valuesWithJobInParty[17, 4] = 7;
            valuesWithJobInParty[17, 5] = 13;
            valuesWithJobInParty[17, 6] = 8;
            valuesWithJobInParty[17, 7] = 9;
            valuesWithJobInParty[17, 8] = 9;
            valuesWithJobInParty[17, 9] = 5;
            valuesWithJobInParty[17, 10] = 10;
            valuesWithJobInParty[17, 11] = 16;

            hasValuesWithoutJobInParty[17] = true;
            valuesWithoutJobInParty[17, 0] = -4;
            valuesWithoutJobInParty[17, 1] = 2;
            valuesWithoutJobInParty[17, 2] = 3;
            valuesWithoutJobInParty[17, 3] = 4;
            valuesWithoutJobInParty[17, 4] = 4;
            valuesWithoutJobInParty[17, 5] = 10;
            valuesWithoutJobInParty[17, 6] = 5;
            valuesWithoutJobInParty[17, 7] = 6;
            valuesWithoutJobInParty[17, 8] = 7;
            valuesWithoutJobInParty[17, 9] = 1;
            valuesWithoutJobInParty[17, 10] = 7;
            valuesWithoutJobInParty[17, 11] = 12;

            //Resources.ParsedStrings.DncRoll
            hasValuesWithJobInParty[18] = true;
            valuesWithJobInParty[18, 0] = -3;
            valuesWithJobInParty[18, 1] = 6;
            valuesWithJobInParty[18, 2] = 7;
            valuesWithJobInParty[18, 3] = 14;
            valuesWithJobInParty[18, 4] = 7;
            valuesWithJobInParty[18, 5] = 8;
            valuesWithJobInParty[18, 6] = 9;
            valuesWithJobInParty[18, 7] = 4;
            valuesWithJobInParty[18, 8] = 10;
            valuesWithJobInParty[18, 9] = 11;
            valuesWithJobInParty[18, 10] = 11;
            valuesWithJobInParty[18, 11] = 17;

            hasValuesWithoutJobInParty[18] = true;
            valuesWithoutJobInParty[18, 0] = -3;
            valuesWithoutJobInParty[18, 1] = 3;
            valuesWithoutJobInParty[18, 2] = 4;
            valuesWithoutJobInParty[18, 3] = 11;
            valuesWithoutJobInParty[18, 4] = 4;
            valuesWithoutJobInParty[18, 5] = 5;
            valuesWithoutJobInParty[18, 6] = 6;
            valuesWithoutJobInParty[18, 7] = 1;
            valuesWithoutJobInParty[18, 8] = 7;
            valuesWithoutJobInParty[18, 9] = 8;
            valuesWithoutJobInParty[18, 10] = 8;
            valuesWithoutJobInParty[18, 11] = 14;

            //Resources.ParsedStrings.SchRoll
            hasValuesWithJobInParty[19] = false;
            hasValuesWithoutJobInParty[19] = false;

            //Resources.ParsedStrings.BolterRoll
            hasValuesWithJobInParty[20] = false;
            hasValuesWithoutJobInParty[20] = false;

            //Resources.ParsedStrings.CasterRoll
            hasValuesWithJobInParty[21] = false;
            hasValuesWithoutJobInParty[21] = false;

            //Resources.ParsedStrings.BlitzerRoll
            hasValuesWithJobInParty[22] = false;
            hasValuesWithoutJobInParty[22] = false;

            //Resources.ParsedStrings.CourserRoll
            hasValuesWithJobInParty[23] = false;
            hasValuesWithoutJobInParty[23] = false;

            //Resources.ParsedStrings.TacticianRoll
            hasValuesWithJobInParty[24] = false;
            hasValuesWithoutJobInParty[24] = false;

            //Resources.ParsedStrings.AlliesRoll
            hasValuesWithJobInParty[25] = false;
            hasValuesWithoutJobInParty[25] = false;

        }

        /// <summary>
        /// Get the value of a particular roll bonus, either with or without job in party.
        /// </summary>
        /// <param name="rollName">The name of the roll to look up.</param>
        /// <param name="roll">The numeric roll.</param>
        /// <param name="withJob"></param>
        /// <param name="withoutJob"></param>
        /// <returns>Returns true if at least one of the out parameters is set to a valid value.
        /// Returns false if no valid value can be found.</returns>
        private void GetRollValue(string rollName, int roll, out int withJob, out int withoutJob)
        {
            // Default return values
            withJob = 0;
            withoutJob = 0;

            // Always return 0 on Busts
            if (roll == 0)
                return;

            int jobIndex = Array.IndexOf(rollNames, rollName);

            if (jobIndex < 0)
                return;

            if (hasValuesWithJobInParty[jobIndex])
            {
                withJob = valuesWithJobInParty[jobIndex, roll];
            }

            if (hasValuesWithoutJobInParty[jobIndex])
            {
                withoutJob = valuesWithoutJobInParty[jobIndex, roll];
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

            lsAvgRollHeader = Resources.Combat.CorsairPluginAvgBonusHeader;
            lsAvgRollAsMainFormat = Resources.Combat.CorsairPluginAvgBonusAsMainFormat;
            lsAvgRollAsSubFormat = Resources.Combat.CorsairPluginAvgBonusAsSubFormat;

            InitializeRollNames();

        }
        #endregion

    }
}
