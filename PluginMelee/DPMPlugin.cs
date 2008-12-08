using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WaywardGamers.KParser;

namespace WaywardGamers.KParser.Plugin
{
    public class DPMPlugin : BasePluginControl
    {
        #region Constructor
        string header1 = "Mob                  Fight Start   Fight End   Duration\n";
        string header2 = "Player               Start Time   Melee DPM   Range DPM   Magic DPM   Abil DPM   WS DPM   Other DPM   Total DPM\n";

        bool flagNoUpdate;
        ToolStripComboBox playersCombo = new ToolStripComboBox();
        ToolStripComboBox mobsCombo = new ToolStripComboBox();
        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();

        public DPMPlugin()
        {
            ToolStripLabel playerLabel = new ToolStripLabel();
            playerLabel.Text = "Players:";
            toolStrip.Items.Add(playerLabel);

            playersCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            playersCombo.Items.Add("All");
            playersCombo.MaxDropDownItems = 10;
            playersCombo.SelectedIndex = 0;
            playersCombo.SelectedIndexChanged += new EventHandler(this.playersCombo_SelectedIndexChanged);
            toolStrip.Items.Add(playersCombo);

            
            ToolStripLabel mobsLabel = new ToolStripLabel();
            mobsLabel.Text = "Mobs:";
            toolStrip.Items.Add(mobsLabel);

            mobsCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            mobsCombo.AutoSize = false;
            mobsCombo.Width = 175;
            mobsCombo.Items.Add("None");
            mobsCombo.MaxDropDownItems = 10;
            mobsCombo.SelectedIndex = 0;
            mobsCombo.SelectedIndexChanged += new EventHandler(this.mobsCombo_SelectedIndexChanged);
            toolStrip.Items.Add(mobsCombo);

        }
        #endregion

        #region IPlugin Overrides
        public override string TabName
        {
            get { return "Damage/Minute"; }
        }

        public override void Reset()
        {
            ResetTextBox();
        }

        public override void NotifyOfUpdate(KPDatabaseDataSet dataSet)
        {
            UpdatePlayerList(dataSet);
            UpdateMobList(dataSet);

            flagNoUpdate = true;
            mobsCombo.CBSelectIndex(0);

            flagNoUpdate = true;
            playersCombo.CBSelectIndex(0);

            HandleDataset(dataSet);
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
        {
            if ((e.DatasetChanges.Combatants != null) &&
                (e.DatasetChanges.Combatants.Count > 0))
            {
                UpdatePlayerList(e.Dataset);

                flagNoUpdate = true;
                playersCombo.CBSelectIndex(0);
            }

            // Check for new mobs being fought.  If any exist, update the Mob Group dropdown list.
            if (e.DatasetChanges.Battles != null)
            {
                if (e.DatasetChanges.Battles.Count > 0)
                {
                    int mobIndex = mobsCombo.CBSelectedIndex();
                    var currentMob = mobsCombo.CBGetMobFilter();
                    var mobBattle = e.Dataset.Battles.FindByBattleID(currentMob.FightNumber);

                    UpdateMobList(e.Dataset);
                    flagNoUpdate = true;

                    if (mobBattle.Killed == true)
                        mobsCombo.CBSelectIndex(mobIndex);
                    else
                        mobsCombo.CBSelectIndex(-1);
                }
            }

            if (e.DatasetChanges.Interactions.Count != 0)
            {
                datasetToUse = e.Dataset;
                return true;
            }

            datasetToUse = null;
            return false;
        }
        #endregion

        #region Private Methods
        private void UpdatePlayerList(KPDatabaseDataSet dataSet)
        {
            playersCombo.CBReset();

            var playersFighting = from b in dataSet.Combatants
                                  where ((b.CombatantType == (byte)EntityType.Player ||
                                         b.CombatantType == (byte)EntityType.Pet ||
                                         b.CombatantType == (byte)EntityType.Fellow) &&
                                         b.GetInteractionsRowsByActorCombatantRelation().Any() == true)
                                  orderby b.CombatantName
                                  select new
                                  {
                                      Name = b.CombatantName
                                  };

            List<string> playerStrings = new List<string>();
            playerStrings.Add("All");

            foreach (var player in playersFighting)
                playerStrings.Add(player.Name);

            playersCombo.CBAddStrings(playerStrings.ToArray());
        }

        private void UpdateMobList()
        {
            UpdateMobList(DatabaseManager.Instance.Database);
            mobsCombo.CBSelectIndex(0);
        }

        private void UpdateMobList(KPDatabaseDataSet dataSet)
        {
            mobsCombo.CBReset();
            string[] tmpList = GetMobListing(dataSet, false, false);

            if (tmpList[0] == "All")
            {
                tmpList[0] = "None";
            }
            
            mobsCombo.CBAddStrings(tmpList);
        }
        #endregion

        #region Processing sections
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            // If we get here during initialization, skip.
            if (playersCombo.Items.Count == 0)
                return;

            if (mobsCombo.Items.Count == 0)
                return;


            string selectedPlayer = playersCombo.CBSelectedItem();
            MobFilter mobFilter = mobsCombo.CBGetMobFilter();

            List<string> playerList = new List<string>();

            if (selectedPlayer == "All")
            {
                foreach (string player in playersCombo.CBGetStrings())
                {
                    if (player != "All")
                        playerList.Add(player.ToString());
                }
            }
            else
            {
                playerList.Add(selectedPlayer);
            }

            if (playerList.Count == 0)
                return;

            if (mobFilter.AllMobs == true)
                ProcessAllMobs(dataSet, playerList);
            else
                ProcessFilteredMobs(dataSet, playerList.ToArray(), mobFilter);
        }

        private void ProcessAllMobs(KPDatabaseDataSet dataSet, List<string> playerList)
        {
            ResetTextBox();
            AppendText("Can only process single mobs at this time.\n", Color.Red, true, false);
        }

        private void ProcessFilteredMobs(KPDatabaseDataSet dataSet, string[] selectedPlayers, MobFilter mobFilter)
        {
            if ((mobFilter.AllMobs == true) ||
                (mobFilter.GroupMobs == true))
            {
                ResetTextBox();
                AppendText("Can only process single mobs at this time.\n", Color.Red, true, false);
                return;
            }

            var attackSet = from c in dataSet.Combatants
                            where (selectedPlayers.Contains(c.CombatantName))
                            orderby c.CombatantType, c.CombatantName
                            select new AttackGroup
                            {
                                Name = c.CombatantName,
                                Melee = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Melee &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                        select n,
                                Range = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Ranged &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                        select n,
                                Spell = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where ((ActionType)n.ActionType == ActionType.Spell &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                                n.Preparing == false &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                        select n,
                                Ability = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                          where ((ActionType)n.ActionType == ActionType.Ability &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain ||
                                                (HarmType)n.HarmType == HarmType.Unknown) &&
                                                n.Preparing == false &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                          select n,
                                WSkill = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where ((ActionType)n.ActionType == ActionType.Weaponskill &&
                                               ((HarmType)n.HarmType == HarmType.Damage ||
                                                (HarmType)n.HarmType == HarmType.Drain) &&
                                                n.Preparing == false &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                         select n,
                                SC = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                     where ((ActionType)n.ActionType == ActionType.Skillchain &&
                                            ((HarmType)n.HarmType == HarmType.Damage ||
                                             (HarmType)n.HarmType == HarmType.Drain) &&
                                               ((DefenseType)n.DefenseType == DefenseType.None)) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                     select n,
                                Counter = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                          where (ActionType)n.ActionType == ActionType.Counterattack &&
                                               ((DefenseType)n.DefenseType == DefenseType.None) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                          select n,
                                Retaliate = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                            where (ActionType)n.ActionType == ActionType.Retaliation &&
                                               ((DefenseType)n.DefenseType == DefenseType.None) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                            select n,
                                Spikes = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where (ActionType)n.ActionType == ActionType.Spikes &&
                                               ((DefenseType)n.DefenseType == DefenseType.None) &&
                                               mobFilter.CheckFilterMobTarget(n) == true
                                         select n
                            };

            KPDatabaseDataSet.BattlesRow battleRow = dataSet.Battles.FindByBattleID(mobFilter.FightNumber);

            if (battleRow == null)
            {
                Logger.Instance.Log("Error finding battle",
                    string.Format("Unable to locate battle #{0}\n", mobFilter.FightNumber));
                return;
            }

            ProcessAttackSetForBattle(attackSet, battleRow);
        }

        private void ProcessAttackSetForBattle(EnumerableRowCollection<AttackGroup> attackSet,
            KPDatabaseDataSet.BattlesRow battle)
        {
            ResetTextBox();

            StringBuilder sb = new StringBuilder();
            List<StringMods> strModList = new List<StringMods>();
            string headerTitle;

            DateTime startTimeFilter;
            DateTime endTimeFilter;

            ///////////////////////////////////////////////////////////////////

            headerTitle = "Fight Summary\n";
            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = headerTitle.Length,
                Bold = true,
                Color = Color.Blue
            });
            sb.Append(headerTitle);

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = header1.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(header1);

            TimeSpan fightLength = battle.FightLength();
            double fightMinutes = fightLength.TotalMinutes;

            sb.AppendFormat("{0,-20}{1,12}{2,12}{3,11}",
                battle.CombatantsRowByEnemyCombatantRelation.CombatantName,
                battle.StartTime.ToShortTimeString(),
                battle.EndTime > battle.StartTime ? battle.EndTime.ToShortTimeString() : "--:--:--",
                fightLength != TimeSpan.Zero
                    ? string.Format("{0:d2}:{1:d2}:{2:d2}",
                        fightLength.Hours, fightLength.Minutes, fightLength.Seconds)
                    : "--:--:--");

            sb.Append("\n\n");

            ///////////////////////////////////////////////////////////////////
            // Cumulative Damage Per Minute

            startTimeFilter = battle.StartTime;

            if (battle.EndTime > battle.StartTime)
                endTimeFilter = battle.EndTime;
            else
                endTimeFilter = DateTime.Now;

            CreateOuput("Cumulative Damage Per Minute\n",
                attackSet, battle, startTimeFilter, endTimeFilter, ref sb, ref strModList);


            ///////////////////////////////////////////////////////////////////
            // Damage in the last minute

            if (battle.EndTime > battle.StartTime)
                endTimeFilter = battle.EndTime;
            else
                endTimeFilter = DateTime.Now;

            startTimeFilter = endTimeFilter.AddMinutes(-1);

            if (startTimeFilter < battle.StartTime)
                startTimeFilter = battle.StartTime;

            CreateOuput("Damage in the last minute\n",
                attackSet, battle, startTimeFilter, endTimeFilter, ref sb, ref strModList);


            ///////////////////////////////////////////////////////////////////
            // Damage in the previous minute

            startTimeFilter = battle.StartTime;

            if (battle.EndTime > battle.StartTime)
                endTimeFilter = battle.EndTime.AddMinutes(-1);
            else
                endTimeFilter = DateTime.Now.AddMinutes(-1);

            startTimeFilter = endTimeFilter.AddMinutes(-1);

            if (endTimeFilter < battle.StartTime)
                endTimeFilter = battle.StartTime;

            if (startTimeFilter < battle.StartTime)
                startTimeFilter = battle.StartTime;

            CreateOuput("Damage in the previous minute\n",
                attackSet, battle, startTimeFilter, endTimeFilter, ref sb, ref strModList);


            ///////////////////////////////////////////////////////////////////
            // Damage in the first minute

            startTimeFilter = battle.StartTime;

            endTimeFilter = startTimeFilter.AddMinutes(1);

            if ((battle.EndTime > battle.StartTime) && (endTimeFilter > battle.EndTime))
                endTimeFilter = battle.EndTime;

            CreateOuput("Damage in the first minute\n",
                attackSet, battle, startTimeFilter, endTimeFilter, ref sb, ref strModList);


            ////////////////////////////////////////////////////////////////////
            // Done with calculations and string construction.  Dump to display.

            PushStrings(sb, strModList);
        }

        private void CreateOuput(string headerTitle,
            EnumerableRowCollection<AttackGroup> attackSet, KPDatabaseDataSet.BattlesRow battle,
            DateTime startTimeFilter, DateTime endTimeFilter,
            ref StringBuilder sb, ref List<StringMods> strModList)
        {
            string totalsLine;

            double meleeDPM;
            double rangeDPM;
            double magicDPM;
            double abilDPM;
            double wsDPM;
            double otherDPM;
            double totalDPM;

            double meleeDPMTotal;
            double rangeDPMTotal;
            double magicDPMTotal;
            double abilDPMTotal;
            double wsDPMTotal;
            double otherDPMTotal;
            double totalDPMTotal;

            DateTime playerStart;
            DateTime firstStart;
            IEnumerable<KPDatabaseDataSet.InteractionsRow> attacks;

            double fightMinutes = (endTimeFilter - startTimeFilter).TotalMinutes;

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = headerTitle.Length,
                Bold = true,
                Color = Color.Blue
            });
            sb.Append(headerTitle);

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = header2.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(header2);

            meleeDPMTotal = 0;
            rangeDPMTotal = 0;
            magicDPMTotal = 0;
            abilDPMTotal = 0;
            wsDPMTotal = 0;
            otherDPMTotal = 0;
            totalDPMTotal = 0;
            firstStart = DateTime.MaxValue;

            foreach (var attacker in attackSet)
            {
                if (attacker.TotalActions > 0)
                {
                    playerStart = DateTime.MaxValue;

                    attacks = attacker.Melee
                        .Where(d => d.Timestamp >= startTimeFilter && d.Timestamp <= endTimeFilter);
                    meleeDPM = attacks != null ? attacks.Sum(d => d.Amount) : 0;
                    if (attacks.Count() > 0)
                        if (attacks.First().Timestamp < playerStart)
                            playerStart = attacks.First().Timestamp;
                    attacks = attacker.MeleeEffect
                        .Where(d => d.Timestamp >= startTimeFilter && d.Timestamp <= endTimeFilter);
                    meleeDPM += attacks != null ? attacks.Sum(d => d.SecondAmount) : 0;
                    meleeDPM /= fightMinutes;

                    attacks = attacker.Range
                        .Where(d => d.Timestamp >= startTimeFilter && d.Timestamp <= endTimeFilter);
                    rangeDPM = attacks != null ? attacks.Sum(d => d.Amount) : 0;
                    if (attacks.Count() > 0)
                        if (attacks.First().Timestamp < playerStart)
                            playerStart = attacks.First().Timestamp;
                    attacks = attacker.RangeEffect
                        .Where(d => d.Timestamp >= startTimeFilter && d.Timestamp <= endTimeFilter);
                    rangeDPM += attacks != null ? attacks.Sum(d => d.SecondAmount) : 0;
                    rangeDPM /= fightMinutes;

                    attacks = attacker.Spell
                        .Where(d => d.Timestamp >= startTimeFilter && d.Timestamp <= endTimeFilter);
                    magicDPM = attacks != null ? attacks.Sum(d => d.Amount) : 0;
                    if (attacks.Count() > 0)
                        if (attacks.First().Timestamp < playerStart)
                            playerStart = attacks.First().Timestamp;
                    magicDPM /= fightMinutes;

                    attacks = attacker.Ability
                        .Where(d => d.Timestamp >= startTimeFilter && d.Timestamp <= endTimeFilter);
                    abilDPM = attacks != null ? attacks.Sum(d => d.Amount) : 0;
                    if (attacks.Count() > 0)
                        if (attacks.First().Timestamp < playerStart)
                            playerStart = attacks.First().Timestamp;
                    abilDPM /= fightMinutes;

                    attacks = attacker.WSkill
                        .Where(d => d.Timestamp >= startTimeFilter && d.Timestamp <= endTimeFilter);
                    wsDPM = attacks != null ? attacks.Sum(d => d.Amount) : 0;
                    if (attacks.Count() > 0)
                        if (attacks.First().Timestamp < playerStart)
                            playerStart = attacks.First().Timestamp;
                    wsDPM /= fightMinutes;

                    attacks = attacker.Spikes
                        .Where(d => d.Timestamp >= startTimeFilter && d.Timestamp <= endTimeFilter)
                        .Concat(attacker.Counter.Where(d => d.Timestamp >= startTimeFilter && d.Timestamp <= endTimeFilter))
                        .Concat(attacker.Retaliate.Where(d => d.Timestamp >= startTimeFilter && d.Timestamp <= endTimeFilter));
                    otherDPM = attacks != null ? attacks.Sum(d => d.Amount) : 0;
                    if (attacks.Count() > 0)
                        if (attacks.First().Timestamp < playerStart)
                            playerStart = attacks.First().Timestamp;
                    otherDPM /= fightMinutes;


                    totalDPM = meleeDPM + rangeDPM + magicDPM + abilDPM + wsDPM + otherDPM;

                    if (totalDPM > 0)
                    {
                        if (playerStart < firstStart)
                            firstStart = playerStart;

                        meleeDPMTotal += meleeDPM;
                        rangeDPMTotal += rangeDPM;
                        magicDPMTotal += magicDPM;
                        abilDPMTotal += abilDPM;
                        wsDPMTotal += wsDPM;
                        otherDPMTotal += otherDPM;
                        totalDPMTotal += totalDPM;

                        sb.AppendFormat("{0,-20}{1,11}{2,12:f2}{3,12:f2}{4,12:f2}{5,11:f2}{6,9:f2}{7,12:f2}{8,12:f2}\n",
                            attacker.Name,
                            playerStart.ToShortTimeString(),
                            meleeDPM,
                            rangeDPM,
                            magicDPM,
                            abilDPM,
                            wsDPM,
                            otherDPM,
                            totalDPM);
                    }
                }
            }

            if (firstStart != DateTime.MaxValue)
            {
                totalsLine = string.Format("{0,-20}{1,11}{2,12:f2}{3,12:f2}{4,12:f2}{5,11:f2}{6,9:f2}{7,12:f2}{8,12:f2}\n",
                    "Totals",
                    firstStart.ToShortTimeString(),
                    meleeDPMTotal,
                    rangeDPMTotal,
                    magicDPMTotal,
                    abilDPMTotal,
                    wsDPMTotal,
                    otherDPMTotal,
                    totalDPMTotal);

                strModList.Add(new StringMods
                {
                    Start = sb.Length,
                    Length = totalsLine.Length,
                    Bold = true,
                    Color = Color.Black
                });
                sb.Append(totalsLine);
            }

            sb.Append("\n\n");
        }
        #endregion

        #region Event Handlers
        protected void playersCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(DatabaseManager.Instance.Database);

            flagNoUpdate = false;
        }

        protected void mobsCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (flagNoUpdate == false)
                HandleDataset(DatabaseManager.Instance.Database);

            flagNoUpdate = false;
        }
        #endregion

    }
}
