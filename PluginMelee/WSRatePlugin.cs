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
    public class WSRatePlugin : BasePluginControl
    {
        #region Member Variables

        bool groupMobs = true;
        bool exclude0XPMobs = false;
        bool flagNoUpdate = false;
        bool customMobSelection = false;
        bool showDetails = false;

        // UI controls
        ToolStripLabel playersLabel = new ToolStripLabel();
        ToolStripComboBox playersCombo = new ToolStripComboBox();

        ToolStripLabel mobsLabel = new ToolStripLabel();
        ToolStripComboBox mobsCombo = new ToolStripComboBox();

        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();
        ToolStripMenuItem groupMobsOption = new ToolStripMenuItem();
        ToolStripMenuItem exclude0XPOption = new ToolStripMenuItem();
        ToolStripMenuItem customMobSelectionOption = new ToolStripMenuItem();
        ToolStripMenuItem showDetailOption = new ToolStripMenuItem();

        ToolStripButton editCustomMobFilter = new ToolStripButton();


        // Localized strings
        string lsAll;

        string lsTitle;
        string lsHeader;
        string lsFormat;

        string lsDetailsHeader;
        string lsDetailsFormat;

        #endregion

        #region Constructor
        public WSRatePlugin()
        {
            LoadLocalizedUI();

            playersCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            playersCombo.MaxDropDownItems = 10;
            playersCombo.SelectedIndexChanged += new EventHandler(this.playersCombo_SelectedIndexChanged);

            mobsCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            mobsCombo.AutoSize = false;
            mobsCombo.Width = 175;
            mobsCombo.MaxDropDownItems = 10;
            mobsCombo.SelectedIndexChanged += new EventHandler(this.mobsCombo_SelectedIndexChanged);


            groupMobsOption.CheckOnClick = true;
            groupMobsOption.Checked = true;
            groupMobsOption.Click += new EventHandler(groupMobs_Click);

            exclude0XPOption.CheckOnClick = true;
            exclude0XPOption.Checked = false;
            exclude0XPOption.Click += new EventHandler(exclude0XPMobs_Click);

            customMobSelectionOption.CheckOnClick = true;
            customMobSelectionOption.Checked = false;
            customMobSelectionOption.Click += new EventHandler(customMobSelection_Click);

            showDetailOption.CheckOnClick = true;
            showDetailOption.Checked = false;
            showDetailOption.Click += new EventHandler(showDetailOption_Click);

            ToolStripSeparator bSeparator = new ToolStripSeparator();

            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;
            optionsMenu.DropDownItems.Add(groupMobsOption);
            optionsMenu.DropDownItems.Add(exclude0XPOption);
            optionsMenu.DropDownItems.Add(customMobSelectionOption);
            optionsMenu.DropDownItems.Add(bSeparator);
            optionsMenu.DropDownItems.Add(showDetailOption);


            editCustomMobFilter.Enabled = false;
            editCustomMobFilter.Click += new EventHandler(editCustomMobFilter_Click);


            ToolStripSeparator aSeparator = new ToolStripSeparator();

            toolStrip.Items.Add(playersLabel);
            toolStrip.Items.Add(playersCombo);
            toolStrip.Items.Add(mobsLabel);
            toolStrip.Items.Add(mobsCombo);
            toolStrip.Items.Add(optionsMenu);
            toolStrip.Items.Add(aSeparator);
            toolStrip.Items.Add(editCustomMobFilter);
        }
        #endregion

        #region IPlugin Overrides
        public override void Reset()
        {
            ResetTextBox();
        }

        public override void NotifyOfUpdate()
        {
            ResetTextBox();

            UpdatePlayerList();
            UpdateMobList(false);

            try
            {
                // Don't generate an update on the first combo box change
                flagNoUpdate = true;
                playersCombo.CBSelectIndex(0);

                // Setting the second combo box will cause the display to load.
                mobsCombo.CBSelectIndex(0);

            }
            finally
            {
                flagNoUpdate = false;
            }

            HandleDataset(null);
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            bool changesFound = false;
            string currentlySelectedPlayer = lsAll;

            // Check for new mobs being fought.  If any exist, update the Mob Group dropdown list.
            if (e.DatasetChanges.Battles != null)
            {
                if (e.DatasetChanges.Battles.Any(x => x.RowState == DataRowState.Added))
                {
                    changesFound = true;
                }
            }

            if (e.DatasetChanges.Interactions != null)
            {
                if (e.DatasetChanges.Interactions.Any(x => x.RowState == DataRowState.Added))
                {
                    changesFound = true;
                }
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

        private void UpdateMobList()
        {
            UpdateMobList(false);
        }

        private void UpdateMobList(bool overrideGrouping)
        {
            if (overrideGrouping == true)
                mobsCombo.UpdateWithMobList(false, exclude0XPMobs);
            else
                mobsCombo.UpdateWithMobList(groupMobs, exclude0XPMobs);
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

            if (mobsCombo.Items.Count == 0)
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


            MobFilter mobFilter;
            if (customMobSelection)
                mobFilter = MobXPHandler.Instance.CustomMobFilter;
            else
                mobFilter = mobsCombo.CBGetMobFilter();


            IEnumerable<AttackGroup> attackSet;

            if (mobFilter.AllMobs == false)
            {
                // For single or grouped mobs

                // If we have any mob filter subset, get that data starting
                // with the battle table and working outwards.  Significantly
                // faster (eg: 5-25 ms instead of 400 ms on a 200 mob parse).

                var bSet = from b in dataSet.Battles
                           where (mobFilter.CheckFilterBattle(b) == true)
                           orderby b.BattleID
                           select b.GetInteractionsRows();

                if (bSet.Count() == 0)
                    return;


                IEnumerable<KPDatabaseDataSet.InteractionsRow> iRows = bSet.First();

                var bSetSkip = bSet.Skip(1);

                foreach (var b in bSetSkip)
                {
                    iRows = iRows.Concat(b);
                }

                if (iRows.Count() > 0)
                {
                    DateTime initialTime = iRows.First().Timestamp - TimeSpan.FromSeconds(70);
                    DateTime endTime = iRows.Last().Timestamp;

                    var dSet = dataSet.Battles.GetDefaultBattle().GetInteractionsRows()
                        .Where(i => i.Timestamp >= initialTime && i.Timestamp <= endTime);

                    iRows = iRows.Concat(dSet);
                }

                attackSet = from c in iRows
                            where (c.IsActorIDNull() == false) &&
                                  (selectedPlayers.Contains(c.CombatantsRowByActorCombatantRelation.CombatantName))
                            group c by c.CombatantsRowByActorCombatantRelation into ca
                            orderby ca.Key.CombatantType, ca.Key.CombatantName
                            select new AttackGroup
                            {
                                Name = ca.Key.CombatantNameOrJobName,
                                ComType = (EntityType)ca.Key.CombatantType,
                                Melee = from q in ca
                                        where ((ActionType)q.ActionType == ActionType.Melee &&
                                               ((HarmType)q.HarmType == HarmType.Damage ||
                                                (HarmType)q.HarmType == HarmType.Drain) &&
                                               ((DefenseType)q.DefenseType == DefenseType.None))
                                        orderby q.Timestamp
                                        select q,
                                Retaliate = from q in ca
                                            where ((ActionType)q.ActionType == ActionType.Retaliation &&
                                                   ((HarmType)q.HarmType == HarmType.Damage ||
                                                    (HarmType)q.HarmType == HarmType.Drain) &&
                                                   ((DefenseType)q.DefenseType == DefenseType.None))
                                            orderby q.Timestamp
                                            select q,
                                Ability = from q in ca
                                          where ((ActionType)q.ActionType == ActionType.Ability &&
                                                 (AidType)q.AidType == AidType.Enhance &&
                                                 q.Preparing == false)
                                          select q,
                                WSkill = from q in ca
                                         where ((ActionType)q.ActionType == ActionType.Weaponskill &&
                                               ((HarmType)q.HarmType == HarmType.Damage ||
                                                (HarmType)q.HarmType == HarmType.Drain) &&
                                                q.Preparing == false)
                                         orderby q.Timestamp
                                         select q,
                            };
            }
            else
            {
                // For all mobs

                attackSet = from c in dataSet.Combatants
                            where (selectedPlayers.Contains(c.CombatantName))
                            select new AttackGroup
                            {
                                Name = c.CombatantNameOrJobName,
                                Melee = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Melee &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None))
                                        select n,
                                Retaliate = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                            where ((ActionType)n.ActionType == ActionType.Retaliation &&
                                                   ((HarmType)n.HarmType == HarmType.Damage ||
                                                    (HarmType)n.HarmType == HarmType.Drain) &&
                                                   ((DefenseType)n.DefenseType == DefenseType.None))
                                            select n,
                                Ability = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                          where ((ActionType)n.ActionType == ActionType.Ability &&
                                                 (AidType)n.AidType == AidType.Enhance &&
                                                 n.Preparing == false)
                                          select n,
                                WSkill = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where ((ActionType)n.ActionType == ActionType.Weaponskill &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                                n.Preparing == false)
                                         select n,
                            };
            }


            List<StringMods> strModList = new List<StringMods>();
            StringBuilder sb = new StringBuilder();

            ProcessAttackSet(attackSet,  ref sb, ref strModList);

            PushStrings(sb, strModList);
        }

        private void ProcessAttackSet(IEnumerable<AttackGroup> attackSet,
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            if (attackSet.Count() == 0)
                return;

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsTitle.Length,
                Bold = true,
                Color = Color.Red
            });
            sb.Append(lsTitle + "\n\n");

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsHeader.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(lsHeader + "\n");

            StringBuilder sbDetails = new StringBuilder();
            List<StringMods> strDetailsModList = new List<StringMods>();

            // Per-player processing
            foreach (var player in attackSet)
            {
                // Get a weaponskill count
                int numWSs = player.WSkill.Count();

                if (numWSs == 0)
                    continue;

                // We'll create a couple arrays to store the database ID of
                // each weaponskill, and a count of the melee hits leading up to
                // that weaponskill
                int[] wsIDs = new int[numWSs];
                int[] hitBuckets = new int[numWSs];

                int totalMeleeHits = 0;
                int totalRetaliationHits = 0;

                // Get the IDs of the weaponskills
                for (int i = 0; i < numWSs; i++)
                {
                    wsIDs[i] = player.WSkill.ElementAt(i).InteractionID;
                }

                Array.Sort(wsIDs);

                // Find the nearest weaponskill table ID to match each
                // melee hit.
                foreach (var melee in player.Melee)
                {
                    // Should always return a negative bitwise complement value
                    int nearestWS_bwc = Array.BinarySearch(wsIDs, melee.InteractionID);

                    if (nearestWS_bwc < 0)
                    {
                        int nearestWS = ~nearestWS_bwc;

                        if (nearestWS < numWSs)
                        {
                            hitBuckets[nearestWS]++;
                            totalMeleeHits++;
                        }
                    }
                }

                // And the same for retaliations
                foreach (var retaliate in player.Retaliate)
                {
                    // Should always return a negative bitwise complement value
                    int nearestWS_bwc = Array.BinarySearch(wsIDs, retaliate.InteractionID);

                    if (nearestWS_bwc < 0)
                    {
                        int nearestWS = ~nearestWS_bwc;

                        if (nearestWS < numWSs)
                        {
                            hitBuckets[nearestWS]++;
                            totalRetaliationHits++;
                        }
                    }
                }

                // The hit buckets have now been filled.  Time for the math.

                // Simple computations
                int minHits = hitBuckets.Min();
                int maxHits = hitBuckets.Max();
                int totalHits = hitBuckets.Sum();
                double arithMean = (double)totalHits / numWSs;

                // Group sets for complex computations
                var groupedHits = from h in hitBuckets
                                  group h by h into hg
                                  orderby hg.Key
                                  select new
                                  {
                                      NumHits = hg.Key,
                                      Count = hg.Count()
                                  };

                // Get the mode
                int mode = groupedHits.OrderBy(a => a.Count).Last().NumHits;

                // Calculate the median
                int median = 0;
                int sum = 0;

                foreach (var hitCount in groupedHits)
                {
                    if ((sum + hitCount.Count) > (numWSs / 2))
                    {
                        median = hitCount.NumHits;
                        break;
                    }

                    sum += hitCount.Count;
                }

                // Calculate the harmonic mean
                double recipSum = hitBuckets.Sum(a => (a > 0) ? 1d / a : 0);

                double harmonicMean = (recipSum > 0) ? numWSs / recipSum : 0;


                // Print it out

                sb.AppendFormat(lsFormat,
                    player.Name,
                    totalMeleeHits,
                    totalRetaliationHits,
                    numWSs,
                    minHits,
                    maxHits,
                    arithMean,
                    harmonicMean,
                    median,
                    mode);

                sb.Append("\n");


                // Construct the details info
                if (showDetails)
                {
                    strDetailsModList.Add(new StringMods
                    {
                        Start = sbDetails.Length,
                        Length = player.Name.Length,
                        Bold = true,
                        Color = Color.Blue
                    });
                    sbDetails.Append(player.Name + "\n");

                    strDetailsModList.Add(new StringMods
                    {
                        Start = sbDetails.Length,
                        Length = lsDetailsHeader.Length,
                        Bold = true,
                        Underline = true,
                        Color = Color.Black
                    });
                    sbDetails.Append(lsDetailsHeader + "\n");

                    foreach (var hitGroup in groupedHits)
                    {
                        sbDetails.AppendFormat(lsDetailsFormat,
                            hitGroup.NumHits, hitGroup.Count);
                        sbDetails.Append("\n");
                    }

                    sbDetails.Append("\n");
                }
            }

            if (showDetails)
            {
                sb.Append("\n\n");

                foreach (var mod in strDetailsModList)
                {
                    mod.Start += sb.Length;
                    strModList.Add(mod);
                }

                sb.Append(sbDetails.ToString());
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

        protected void mobsCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(null);

            flagNoUpdate = false;
        }

        void groupMobs_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            groupMobs = sentBy.Checked;

            if (flagNoUpdate == false)
            {
                flagNoUpdate = true;
                UpdateMobList();

                HandleDataset(null);
            }

            flagNoUpdate = false;
        }

        protected void exclude0XPMobs_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            exclude0XPMobs = sentBy.Checked;

            if (flagNoUpdate == false)
            {
                flagNoUpdate = true;
                UpdateMobList();

                HandleDataset(null);
            }

            flagNoUpdate = false;
        }

        protected void customMobSelection_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            customMobSelection = sentBy.Checked;

            mobsCombo.Enabled = !customMobSelection;
            groupMobsOption.Enabled = !customMobSelection;
            exclude0XPOption.Enabled = !customMobSelection;

            editCustomMobFilter.Enabled = customMobSelection;

            if (flagNoUpdate == false)
            {
                HandleDataset(null);
            }

            flagNoUpdate = false;
        }

        protected void editCustomMobFilter_Click(object sender, EventArgs e)
        {
            MobXPHandler.Instance.ShowCustomMobFilter();
        }

        protected void showDetailOption_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem sentBy = sender as ToolStripMenuItem;
            if (sentBy == null)
                return;

            showDetails = sentBy.Checked;

            if (flagNoUpdate == false)
                HandleDataset(null);

            flagNoUpdate = false;
        }

        protected override void OnCustomMobFilterChanged()
        {
            HandleDataset(null);
        }

        #endregion

        #region Localization Overrides
        protected override void LoadLocalizedUI()
        {
            playersLabel.Text = Resources.PublicResources.PlayersLabel;
            mobsLabel.Text = Resources.PublicResources.MobsLabel;

            optionsMenu.Text = Resources.PublicResources.Options;
            groupMobsOption.Text = Resources.PublicResources.GroupMobs;
            exclude0XPOption.Text = Resources.PublicResources.Exclude0XPMobs;
            customMobSelectionOption.Text = Resources.PublicResources.CustomMobSelection;
            showDetailOption.Text = Resources.PublicResources.ShowDetail;
            editCustomMobFilter.Text = Resources.PublicResources.EditMobFilter;

            UpdatePlayerList();
            playersCombo.SelectedIndex = 0;

            UpdateMobList();
            mobsCombo.SelectedIndex = 0;
        }

        protected override void LoadResources()
        {
            this.tabName = Resources.Combat.WSRatePluginTabName;

            lsAll = Resources.PublicResources.All;

            lsTitle = Resources.Combat.WSRatePluginTitle;

            lsHeader = Resources.Combat.WSRatePluginMainHeader;
            lsFormat = Resources.Combat.WSRatePluginMainFormat;

            lsDetailsHeader = Resources.Combat.WSRatePluginDetailsHeader;
            lsDetailsFormat = Resources.Combat.WSRatePluginDetailsFormat;
        }
        #endregion

    }
}
