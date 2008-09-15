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
        string header1 = "Player               Total # Attacks   Total # Rounds   Min Attacks/Round   Total Extra Attacks\n";
        string header2 = "Player               Avg # Extra Attacks   % Rounds w/Extra Attacks   Avg # Attacks/Round\n";
        string header3 = "Player               # Rounds w/Extra Attacks   # +1 Rounds   # +2 Rounds   # >+2 Rounds\n";

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
            mobsCombo.Items.Add("All");
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

        public override void DatabaseOpened(KPDatabaseDataSet dataSet)
        {
            UpdatePlayerList(dataSet);
            UpdateMobList(dataSet);

            flagNoUpdate = true;
            mobsCombo.CBSelectIndex(0);

            flagNoUpdate = true;
            playersCombo.CBSelectIndex(0);

            ProcessData(dataSet);
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
                    UpdateMobList(e.Dataset);
                    flagNoUpdate = true;
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
            mobsCombo.CBAddStrings(GetMobListing(dataSet, false, false));
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
            var attackSet = from c in dataSet.Combatants
                            where (selectedPlayers.Contains(c.CombatantName))
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

            ProcessAttackSet(attackSet);
        }

        private void ProcessAttackSet(EnumerableRowCollection<AttackGroup> attackSet)
        {
            ResetTextBox();

            
        }

        private class AttackCalculations
        {
            internal string Name { get; set; }
            internal int Attacks { get; set; }
            internal int Rounds { get; set; }
            internal int AttacksPerRound { get; set; }
            internal int ExtraAttacks { get; set; }
            internal int RoundsWithExtraAttacks { get; set; }
            internal double FracRoundsWithExtraAttacks { get; set; }
            internal double AvgExtraAttacks { get; set; }
            internal double AvgAttacksPerRound { get; set; }
            internal int Plus1Rounds { get; set; }
            internal int Plus2Rounds { get; set; }
            internal int PlusNRounds { get; set; }
        }

        private void PrintOutput(List<AttackCalculations> attackCalcs)
        {
            if (attackCalcs.Count == 0)
                return;

            StringBuilder sb = new StringBuilder();
            List<StringMods> strModList = new List<StringMods>();

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = header1.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(header1);

            foreach (var attacker in attackCalcs)
            {
                sb.AppendFormat("{0,-20}{1,16}{2,17}{3,20}{4,22}\n",
                    attacker.Name,
                    attacker.Attacks,
                    attacker.Rounds,
                    attacker.AttacksPerRound,
                    attacker.ExtraAttacks);
            }
            sb.Append("\n");

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = header2.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(header2);

            foreach (var attacker in attackCalcs)
            {
                sb.AppendFormat("{0,-20}{1,20:f2}{2,27:p2}{3,22:f2}\n",
                    attacker.Name,
                    attacker.AvgExtraAttacks,
                    attacker.FracRoundsWithExtraAttacks,
                    attacker.AvgAttacksPerRound);
            }
            sb.Append("\n");

            strModList.Add(new StringMods
            {
                Start = sb.Length,
                Length = header3.Length,
                Bold = true,
                Underline = true,
                Color = Color.Black
            });
            sb.Append(header3);

            foreach (var attacker in attackCalcs)
            {
                sb.AppendFormat("{0,-20}{1,25}{2,14}{3,14}{4,15}\n",
                    attacker.Name,
                    attacker.RoundsWithExtraAttacks,
                    attacker.Plus1Rounds,
                    attacker.Plus2Rounds,
                    attacker.PlusNRounds);
            }
            sb.Append("\n");


            PushStrings(sb, strModList);
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
