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
        #region Local data structure
        private class WSAggregates
        {
            internal string Name { get; set; }
            internal int WSCount { get; set; }
            internal TimeSpan WSInterval { get; set; }
            internal TimeSpan WSHarmonicInterval { get; set; }
            internal int MHits { get; set; }
            internal int MMin { get; set; }
            internal int MMax { get; set; }
            internal int MMode { get; set; }
            internal int MMedian { get; set; }
            internal double MMean { get; set; }
            internal int RHits { get; set; }
            internal int RMin { get; set; }
            internal int RMax { get; set; }
            internal int RMode { get; set; }
            internal int RMedian { get; set; }
            internal double RMean { get; set; }
            internal int SCast { get; set; }
            internal int SFail { get; set; }
            internal int STotal { get; set; }
            internal int SMin { get; set; }
            internal int SMax { get; set; }
            internal double SMean { get; set; }
            internal double SMeanF { get; set; }
        }
        #endregion

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


        Regex tpRegex = new Regex(@"KP(arser)?:\s*(TP return =)?\s*(?<tpReturn>\d+)%\W*((ws)?\s*tp)?",
            RegexOptions.IgnoreCase);
        

        // Localized strings
        string lsAll;

        string lsTitle;
        string lsDetailsTitle;

        string lsTotalWSs;
        string lsMeleeTitle;
        string lsRangeTitle;
        string lsAbsorbTitle;

        string lsWSHeader;
        string lsWSFormat;
        string lsPhysicalHeader;
        string lsPhysicalFormat;
        string lsSpellHeader;
        string lsSpellFormat;

        string lsDetailsMHeader;
        string lsDetailsRHeader;
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
                mobFilter = mobsCombo.CBGetMobFilter(exclude0XPMobs);


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
                                Range = from q in ca
                                        where ((ActionType)q.ActionType == ActionType.Ranged &&
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
                                Spell = from q in ca
                                        where ((ActionType)q.ActionType == ActionType.Spell &&
                                                (HarmType)q.HarmType == HarmType.Enfeeble &&
                                                q.Preparing == false &&
                                                q.IsActionIDNull() == false &&
                                                q.ActionsRow.ActionName == Resources.ParsedStrings.AbsorbTP)
                                        orderby q.Timestamp
                                        select q
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
                                Range = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Ranged &&
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
                                Spell = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where ((ActionType)n.ActionType == ActionType.Spell &&
                                                (HarmType)n.HarmType == HarmType.Enfeeble &&
                                                n.Preparing == false &&
                                                n.IsActionIDNull() == false &&
                                                n.ActionsRow.ActionName == Resources.ParsedStrings.AbsorbTP)
                                         select n,
                            };
            }


            List<StringMods> strModList = new List<StringMods>();
            StringBuilder sb = new StringBuilder();

            ProcessAttackSet(attackSet, ref sb, ref strModList);

            ProcessTPReturns(attackSet, dataSet, ref sb, ref strModList);

            PushStrings(sb, strModList);
        }

        private void ProcessTPReturns(IEnumerable<AttackGroup> attackSet, KPDatabaseDataSet dataSet,
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            var echoSpeaker = dataSet.ChatSpeakers.FirstOrDefault(s => s.SpeakerName == "-Echo-");

            if (echoSpeaker == null)
                return;

            var echos = echoSpeaker.GetChatMessagesRows();

            var tpEchoes = from e in echos
                           where tpRegex.Match(e.Message).Success == true
                           select e;

            var selfPlayer = attackSet.Where(a => a.WSkill.Count() > 0).Where(a =>
                a.WSkill.Any(w => (ActorPlayerType)w.ActorType == ActorPlayerType.Self));

            foreach (var player in selfPlayer.OrderBy(a => a.Name))
            {
                var combatant = dataSet.Combatants.FirstOrDefault(c => c.CombatantName == player.Name);

                if (combatant == null)
                    continue;

                // Ok, we've found the parsing player. Collect the TP returns for as many WSs as we can.

                var wsTP = from ws in player.WSkill
                           select new
                           {
                               WS = ws,
                               TP = GetTP(ws, tpEchoes)
                           };


                if (wsTP.Any(w => w.TP >= 0) == false)
                    return;

                var groupedByWS = wsTP.GroupBy(w => w.WS.ActionsRow.ActionName);

                string tmp = string.Format("TP Returns for {0}", player.Name);

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = tmp.Length,
                    Bold = true,
                    Color = Color.Red
                });
                sb.Append(tmp + "\n\n");

                sb.Append("Echo format: \"/echo KParser: TP return = <tp>\" or \"/echo KParser: <tp> WS TP\"\n\n");

                foreach (var byWS in groupedByWS)
                {
                    tmp = byWS.Key;
                    strModList.Add(new StringMods
                    {
                        Start = sb.Length,
                        Length = tmp.Length,
                        Bold = true,
                        Color = Color.Green
                    });
                    sb.Append(tmp + "\n\n");

                    int wsCount = byWS.Count();

                    var groupedWSTP = byWS.GroupBy(a => a.TP);

                    var over0TP = byWS.Where(a => a.TP >= 0);
                    int freqOver0TP = over0TP.Count();
                    int dmgOver0TP = over0TP.Sum(a => a.WS.Amount);

                    int sumTP = over0TP.Sum(a => a.TP);

                    groupedWSTP.Sum(a => a.Count());

                    double avgTP = 0;
                    if (freqOver0TP > 0)
                    {
                        avgTP = (double)sumTP / freqOver0TP;
                        sb.AppendFormat("Average recorded TP return: {0:f2}\n\n", avgTP);
                    }
                    else
                    {
                        sb.Append("Average recorded TP return: Unknown\n\n");
                    }

                    tmp = "Data by TP return (-1 is invalid or missing echo data):";

                    strModList.Add(new StringMods
                    {
                        Start = sb.Length,
                        Length = tmp.Length,
                        Bold = true,
                        Color = Color.Blue
                    });
                    sb.Append(tmp + "\n\n");

                    string tpHeader = "TP    Count      Freq %    Total WS Dmg    Min WS Dmg    Max WS Dmg    Avg WS Dmg";

                    strModList.Add(new StringMods
                    {
                        Start = sb.Length,
                        Length = tpHeader.Length,
                        Bold = true,
                        Underline = true,
                        Color = Color.Black
                    });
                    sb.Append(tpHeader + "\n");


                    foreach (var tp in groupedWSTP.OrderBy(a => a.Key))
                    {
                        sb.AppendFormat("{0,-3}{1,8}{2,12:p2}{3,16}{4,14}{5,14}{6,14:f2}",
                            tp.Key,
                            tp.Count(),
                            (double)tp.Count() / wsCount,
                            tp.Sum(a => a.WS.Amount),
                            tp.Min(a => a.WS.Amount),
                            tp.Max(a => a.WS.Amount),
                            tp.Average(a => a.WS.Amount));
                        sb.Append("\n");
                    }
                    sb.Append("\n");

                    if (showDetails)
                    {
                        tmp = "Details:";

                        strModList.Add(new StringMods
                        {
                            Start = sb.Length,
                            Length = tmp.Length,
                            Bold = true,
                            Color = Color.Blue
                        });
                        sb.Append(tmp + "\n\n");

                        foreach (var tp in groupedWSTP.OrderBy(a => a.Key))
                        {
                            sb.AppendFormat("{0} TP\n", tp.Key);

                            foreach (var ws in tp.OrderBy(a => a.WS.Timestamp))
                            {
                                sb.AppendFormat("   {0}\n", ws.WS.Amount);
                            }
                        }
                        sb.Append("\n\n");
                    }

                }
            }
        }

        private int GetTP(KPDatabaseDataSet.InteractionsRow ws,
            IEnumerable<KPDatabaseDataSet.ChatMessagesRow> tpEchoes)
        {
            if (ws == null)
                throw new ArgumentNullException("ws");
            if (tpEchoes == null)
                throw new ArgumentNullException("tpEchoes");


            // Find anything +/- 5 seconds from the ws
            var localTPSet = tpEchoes.Where(c => Math.Abs((c.Timestamp - ws.Timestamp).TotalSeconds) <= 5);

            // If none, no valid TP
            if (localTPSet.Count() == 0)
                return -1;

            // If only one, return that
            if (localTPSet.Count() == 1)
                return GetTPValue(localTPSet.First().Message);

            // If there's more than one, sort them by how close they are to the weaponskill,
            // picking only those with validly parsed results.
            var proximityTPSet = from e in localTPSet
                                 let tpAmount = GetTPValue(e.Message)
                                 where tpAmount >= 0 && tpAmount < 100
                                 orderby Math.Abs((e.Timestamp - ws.Timestamp).TotalSeconds)
                                 select e;

            // If no validly formatted results, return unknown.
            if (proximityTPSet.Count() == 0)
                return -1;

            // Otherwise take the first (closest to weaponskill) echo in the list.
            return GetTPValue(proximityTPSet.First().Message);
        }

        private int GetTPValue(string tpEcho)
        {
            if (string.IsNullOrEmpty(tpEcho))
                throw new ArgumentNullException();

            Match tpMatch = tpRegex.Match(tpEcho);

            if (tpMatch.Success)
            {
                string tpStr = tpMatch.Groups["tpReturn"].Value;
                int tp = 0;

                if (int.TryParse(tpStr, out tp))
                    return tp;
                else
                    return -1;
            }
            else
            {
                return -1;
            }
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


            StringBuilder sbDetails = new StringBuilder();
            List<StringMods> strDetailsModList = new List<StringMods>();

            List<WSAggregates> WSDataList = new List<WSAggregates>();

            // Per-player processing
            foreach (var player in attackSet.OrderBy(a => a.Name))
            {
                // Get a weaponskill count
                int numWSs = player.WSkill.Count();

                if (numWSs == 0)
                    continue;

                WSAggregates wsAgg = new WSAggregates();
                wsAgg.Name = player.Name;
                wsAgg.WSCount = numWSs;

                if (numWSs > 1)
                {
                    DateTime firstWS = player.WSkill.Min(a => a.Timestamp);
                    DateTime lastWS = player.WSkill.Max(a => a.Timestamp);

                    double interval = (lastWS - firstWS).TotalSeconds;
                    wsAgg.WSInterval = TimeSpan.FromSeconds(interval / (numWSs - 1));
                }


                WSDataList.Add(wsAgg);

                // We'll create a couple arrays to store the database ID of
                // each weaponskill, and a count of the melee hits leading up to
                // that weaponskill
                int[] wsIDs = new int[numWSs];
                int[] hitBuckets = new int[numWSs];
                int[] rangedBuckets = new int[numWSs];

                int totalMeleeHits = 0;
                int totalRetaliationHits = 0;
                int totalRangedHits = 0;

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

                // The same for retaliations
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

                // And the same for Ranged
                foreach (var ranged in player.Range)
                {
                    // Should always return a negative bitwise complement value
                    int nearestWS_bwc = Array.BinarySearch(wsIDs, ranged.InteractionID);

                    if (nearestWS_bwc < 0)
                    {
                        int nearestWS = ~nearestWS_bwc;

                        if (nearestWS < numWSs)
                        {
                            rangedBuckets[nearestWS]++;
                            totalRangedHits++;
                        }
                    }
                }

                // The hit buckets have now been filled.  Time for the math.

                // Simple computations
                wsAgg.MHits = totalMeleeHits + totalRetaliationHits;
                wsAgg.MMin = hitBuckets.Min();
                wsAgg.MMax = hitBuckets.Max();
                wsAgg.MMean = (double)wsAgg.MHits / numWSs;

                wsAgg.RHits = totalRangedHits;
                wsAgg.RMin = rangedBuckets.Min();
                wsAgg.RMax = rangedBuckets.Max();
                wsAgg.RMean = (double)totalRangedHits / numWSs;


                // Group sets for complex computations
                var groupedHits = from h in hitBuckets
                                  group h by h into hg
                                  orderby hg.Key
                                  select new
                                  {
                                      NumHits = hg.Key,
                                      Count = hg.Count()
                                  };

                // Group sets for complex computations
                var groupedRange = from h in rangedBuckets
                                   group h by h into hg
                                   orderby hg.Key
                                   select new
                                   {
                                       NumHits = hg.Key,
                                       Count = hg.Count()
                                   };


                int sum = 0;

                // For melee swings
                if (totalMeleeHits + totalRetaliationHits > 0)
                {
                    wsAgg.MMode = groupedHits.MaxEntry(compare => compare.Count, retVal => retVal.NumHits);

                    foreach (var hitCount in groupedHits)
                    {
                        if ((sum + hitCount.Count) >= (numWSs / 2))
                        {
                            wsAgg.MMedian = hitCount.NumHits;
                            break;
                        }

                        sum += hitCount.Count;
                    }
                }

                sum = 0;

                // For ranged shots
                if (totalRangedHits > 0)
                {
                    wsAgg.RMode = groupedRange.MaxEntry(compare => compare.Count, retVal => retVal.NumHits);

                    foreach (var hitCount in groupedRange)
                    {
                        if ((sum + hitCount.Count) >= (numWSs / 2))
                        {
                            wsAgg.RMedian = hitCount.NumHits;
                            break;
                        }

                        sum += hitCount.Count;
                    }
                }


                // Absorb-TP stuff
                if (player.Spell.Count() > 0)
                {
                    wsAgg.SCast = player.Spell.Count();
                    wsAgg.SFail = player.Spell.Count(s => (FailedActionType)s.FailedActionType != FailedActionType.None);

                    var successfulCast = player.Spell.Where(s => (FailedActionType)s.FailedActionType == FailedActionType.None);

                    if (successfulCast.Count() > 0)
                    {
                        wsAgg.STotal = successfulCast.Sum(s => s.Amount);

                        wsAgg.SMin = successfulCast.Min(s => s.Amount);
                        wsAgg.SMax = successfulCast.Max(s => s.Amount);
                        wsAgg.SMeanF = (double)wsAgg.STotal / wsAgg.SCast;

                        wsAgg.SMean = (double)wsAgg.STotal / successfulCast.Count();
                    }
                }


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

                    if (totalMeleeHits + totalRetaliationHits > 0)
                    {
                        strDetailsModList.Add(new StringMods
                        {
                            Start = sbDetails.Length,
                            Length = lsDetailsMHeader.Length,
                            Bold = true,
                            Underline = true,
                            Color = Color.Black
                        });
                        sbDetails.Append(lsDetailsMHeader + "\n");

                        foreach (var hitGroup in groupedHits)
                        {
                            sbDetails.AppendFormat(lsDetailsFormat,
                                hitGroup.NumHits, hitGroup.Count);
                            sbDetails.Append("\n");
                        }

                        sbDetails.Append("\n");
                    }

                    if (totalRangedHits > 0)
                    {
                        strDetailsModList.Add(new StringMods
                        {
                            Start = sbDetails.Length,
                            Length = lsDetailsRHeader.Length,
                            Bold = true,
                            Underline = true,
                            Color = Color.Black
                        });
                        sbDetails.Append(lsDetailsRHeader + "\n");

                        foreach (var hitGroup in groupedRange)
                        {
                            sbDetails.AppendFormat(lsDetailsFormat,
                                hitGroup.NumHits, hitGroup.Count);
                            sbDetails.Append("\n");
                        }

                        sbDetails.Append("\n");
                    }
                }
            }

            OutputWSTotals(WSDataList, sb, strModList);
            OutputMeleeTotals(WSDataList, sb, strModList);
            OutputRangeTotals(WSDataList, sb, strModList);
            OutputSpellTotals(WSDataList, sb, strModList);


            // Main loop done, show details if appropriate
            if (showDetails)
            {
                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = lsDetailsTitle.Length,
                    Bold = true,
                    Color = Color.Red
                });
                sb.Append(lsDetailsTitle + "\n\n");

                foreach (var mod in strDetailsModList)
                {
                    mod.Start += sb.Length;
                    strModList.Add(mod);
                }

                sb.Append(sbDetails.ToString());
            }
        }

        private void OutputWSTotals(List<WSAggregates> WSDataList, StringBuilder sb, List<StringMods> strModList)
        {
            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsTotalWSs.Length,
                Bold = true,
                Color = Color.Blue
            });
            sb.Append(lsTotalWSs + "\n\n");

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = lsWSHeader.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(lsWSHeader + "\n");

            foreach (var player in WSDataList.OrderBy(p => p.Name))
            {
                if (player.WSCount > 0)
                {
                    sb.AppendFormat(lsWSFormat,
                            player.Name,
                            player.WSCount,
                            player.WSInterval.FormattedShortTimeSpanString());

                    sb.Append("\n");
                }
            }

            sb.Append("\n\n");
        }

        private void OutputMeleeTotals(List<WSAggregates> WSDataList, StringBuilder sb, List<StringMods> strModList)
        {
            bool shownHeader = false;

            foreach (var player in WSDataList.OrderBy(p => p.Name))
            {
                if ((player.WSCount > 0) && (player.MHits > 0))
                {
                    if (shownHeader == false)
                    {
                        strModList.Add(new StringMods
                        {
                            Start = sb.Length,
                            Length = lsMeleeTitle.Length,
                            Bold = true,
                            Color = Color.Blue
                        });
                        sb.Append(lsMeleeTitle + "\n\n");

                        strModList.Add(new StringMods
                        {
                            Start = sb.Length,
                            Length = lsPhysicalHeader.Length,
                            Bold = true,
                            Underline = true,
                            Color = Color.Black
                        });
                        sb.Append(lsPhysicalHeader + "\n");

                        shownHeader = true;
                    }

                    sb.AppendFormat(lsPhysicalFormat,
                            player.Name,
                            player.MHits,
                            player.MMin,
                            player.MMax,
                            player.MMean,
                            player.MMedian,
                            player.MMode);

                    sb.Append("\n");
                }
            }

            if (shownHeader)
                sb.Append("\n\n");
        }

        private void OutputRangeTotals(List<WSAggregates> WSDataList, StringBuilder sb, List<StringMods> strModList)
        {
            bool shownHeader = false;

            foreach (var player in WSDataList.OrderBy(p => p.Name))
            {
                if ((player.WSCount > 0) && (player.RHits > 0))
                {
                    if (shownHeader == false)
                    {
                        strModList.Add(new StringMods
                        {
                            Start = sb.Length,
                            Length = lsRangeTitle.Length,
                            Bold = true,
                            Color = Color.Blue
                        });
                        sb.Append(lsRangeTitle + "\n\n");

                        strModList.Add(new StringMods
                        {
                            Start = sb.Length,
                            Length = lsPhysicalHeader.Length,
                            Bold = true,
                            Underline = true,
                            Color = Color.Black
                        });
                        sb.Append(lsPhysicalHeader + "\n");

                        shownHeader = true;
                    }

                    sb.AppendFormat(lsPhysicalFormat,
                            player.Name,
                            player.RHits,
                            player.RMin,
                            player.RMax,
                            player.RMean,
                            player.RMedian,
                            player.RMode);

                    sb.Append("\n");
                }
            }

            if (shownHeader)
                sb.Append("\n\n");
        }

        private void OutputSpellTotals(List<WSAggregates> WSDataList, StringBuilder sb, List<StringMods> strModList)
        {
            bool shownHeader = false;

            foreach (var player in WSDataList.OrderBy(p => p.Name))
            {
                if ((player.WSCount > 0) && (player.SCast > 0))
                {
                    if (shownHeader == false)
                    {
                        strModList.Add(new StringMods
                        {
                            Start = sb.Length,
                            Length = lsAbsorbTitle.Length,
                            Bold = true,
                            Color = Color.Blue
                        });
                        sb.Append(lsAbsorbTitle + "\n\n");

                        strModList.Add(new StringMods
                        {
                            Start = sb.Length,
                            Length = lsSpellHeader.Length,
                            Bold = true,
                            Underline = true,
                            Color = Color.Black
                        });
                        sb.Append(lsSpellHeader + "\n");
                        
                        shownHeader = true;
                    }

                    sb.AppendFormat(lsSpellFormat,
                            player.Name,
                            player.SCast,
                            player.SFail,
                            player.STotal,
                            player.SMin,
                            player.SMax,
                            player.SMean,
                            player.SMeanF);

                    sb.Append("\n");
                }
            }

            if (shownHeader)
                sb.Append("\n\n");
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
            lsDetailsTitle = Resources.Combat.WSRatePluginDetailsTitle;
            lsTotalWSs = Resources.Combat.WSRatePluginTotalWSs;
            lsMeleeTitle = Resources.Combat.WSRatePluginMeleeTitle;
            lsRangeTitle = Resources.Combat.WSRatePluginRangeTitle;
            lsAbsorbTitle = Resources.Combat.WSRatePluginAbsorbTitle;

            lsWSHeader = Resources.Combat.WSRatePluginWSHeader;
            lsPhysicalHeader = Resources.Combat.WSRatePluginPhysicalHeader;
            lsSpellHeader = Resources.Combat.WSRatePluginSpellHeader;

            lsWSFormat = Resources.Combat.WSRatePluginWSFormat;
            lsPhysicalFormat = Resources.Combat.WSRatePluginPhysicalFormat;
            lsSpellFormat = Resources.Combat.WSRatePluginSpellFormat;

            lsDetailsMHeader = Resources.Combat.WSRatePluginDetailsMHeader;
            lsDetailsRHeader = Resources.Combat.WSRatePluginDetailsRHeader;
            lsDetailsFormat = Resources.Combat.WSRatePluginDetailsFormat;
        }
        #endregion

    }
}
