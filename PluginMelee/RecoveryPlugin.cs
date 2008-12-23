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
    public class RecoveryPlugin : BasePluginControl
    {
        #region Constructor
        bool flagNoUpdate;
        bool groupMobs = true;
        bool exclude0XPMobs = false;
        ToolStripComboBox categoryCombo = new ToolStripComboBox();
        ToolStripComboBox mobsCombo = new ToolStripComboBox();
        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();

        public RecoveryPlugin()
        {
            ToolStripLabel catLabel = new ToolStripLabel();
            catLabel.Text = "Category:";
            toolStrip.Items.Add(catLabel);

            categoryCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            categoryCombo.Items.Add("All");
            categoryCombo.Items.Add("Recovery");
            categoryCombo.Items.Add("Curing");
            categoryCombo.Items.Add("Average Curing");
            categoryCombo.SelectedIndex = 0;
            categoryCombo.SelectedIndexChanged += new EventHandler(this.categoryCombo_SelectedIndexChanged);
            toolStrip.Items.Add(categoryCombo);

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


            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;
            optionsMenu.Text = "Options";

            ToolStripMenuItem groupMobsOption = new ToolStripMenuItem();
            groupMobsOption.Text = "Group Mobs";
            groupMobsOption.CheckOnClick = true;
            groupMobsOption.Checked = true;
            groupMobsOption.Click += new EventHandler(groupMobs_Click);
            optionsMenu.DropDownItems.Add(groupMobsOption);

            ToolStripMenuItem exclude0XPOption = new ToolStripMenuItem();
            exclude0XPOption.Text = "Exclude 0 XP Mobs";
            exclude0XPOption.CheckOnClick = true;
            exclude0XPOption.Checked = false;
            exclude0XPOption.Click += new EventHandler(exclude0XPMobs_Click);
            optionsMenu.DropDownItems.Add(exclude0XPOption);

            toolStrip.Items.Add(optionsMenu);
        }
        #endregion

        #region IPlugin Overrides
        public override string TabName
        {
            get { return "Recovery"; }
        }

        public override void Reset()
        {
            ResetTextBox();
        }

        public override void NotifyOfUpdate()
        {
            UpdateMobList(false);

            mobsCombo.CBSelectIndex(0);
        }

        public override void WatchDatabaseChanging(object sender, DatabaseWatchEventArgs e)
        {
            // Check for new mobs being fought.  If any exist, update the Mob Group dropdown list.
            if (e.DatasetChanges.Battles.Count > 0)
            {
                string selectedItem = mobsCombo.CBSelectedItem();
                UpdateMobList(true);

                flagNoUpdate = true;
                mobsCombo.CBSelectItem(selectedItem);
            }

            if (e.DatasetChanges.Interactions.Count != 0)
            {
                HandleDataset(null);
            }
        }
        #endregion

        #region Member Variables
        Dictionary<string, int> playerDamage = new Dictionary<string, int>();

        string dmgRecoveryHeader = "Player           Dmg Taken   HP Drained   HP Cured   #Regen   #Regen 2   #Regen 3\n";
        string cureHeader = "Player           Cured (Sp)  Cured (Ab)  C.1s  C.2s  C.3s  C.4s  C.5s  Curagas  Rg.1s  Rg.2s  Rg.3s\n";
        string avgCureHeader = "Player           Avg Cure 1   Avg Cure 2   Avg Cure 3   Avg Cure 4   Avg Cure 5   Avg Curaga   Avg Ability\n";
        #endregion

        #region Private Methods
        private void UpdateMobList()
        {
            UpdateMobList(false);
            mobsCombo.CBSelectIndex(0);
        }

        private void UpdateMobList(bool overrideGrouping)
        {
            mobsCombo.CBReset();
            mobsCombo.CBAddStrings(GetMobListing(groupMobs, exclude0XPMobs));
        }
        #endregion

        #region Processing sections
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            ResetTextBox();

            MobFilter mobFilter = mobsCombo.CBGetMobFilter();

            switch (categoryCombo.CBSelectedIndex())
            {
                case 0:
                    // All
                    ProcessDamage(dataSet, mobFilter);
                    ProcessCuring(dataSet, mobFilter, true, true);
                    break;
                case 1:
                    // Recovery
                    ProcessDamage(dataSet, mobFilter);
                    break;
                case 2:
                    // Curing
                    ProcessCuring(dataSet, mobFilter, true, false);
                    break;
                case 3:
                    // AverageCuring
                    ProcessCuring(dataSet, mobFilter, false, true);
                    break;
            }
        }
        #endregion

        #region Recovery
        private void ProcessDamage(KPDatabaseDataSet dataSet, MobFilter mobFilter)
        {
            var playerData = from c in dataSet.Combatants
                             where ((c.CombatantType == (byte)EntityType.Player) ||
                                    (c.CombatantType == (byte)EntityType.Pet) ||
                                    (c.CombatantType == (byte)EntityType.Fellow))
                             orderby c.CombatantType, c.CombatantName
                             select new
                             {
                                 Player = c.CombatantName,
                                 PrimeDmgTaken = from dm in c.GetInteractionsRowsByTargetCombatantRelation()
                                                 where ((dm.HarmType == (byte)HarmType.Damage) ||
                                                        (dm.HarmType == (byte)HarmType.Drain)) &&
                                                        mobFilter.CheckFilterMobBattle(dm)
                                                 select dm.Amount,
                                 SecondDmgTaken = from dm in c.GetInteractionsRowsByTargetCombatantRelation()
                                                  where ((dm.SecondHarmType == (byte)HarmType.Damage) ||
                                                         (dm.SecondHarmType == (byte)HarmType.Drain)) &&
                                                        mobFilter.CheckFilterMobBattle(dm)
                                                  select dm.SecondAmount,
                                 PrimeDrain = from dr in c.GetInteractionsRowsByActorCombatantRelation()
                                              where (dr.HarmType == (byte)HarmType.Drain) &&
                                                    mobFilter.CheckFilterMobBattle(dr)
                                              select dr.Amount,
                                 SecondDrain = from dr in c.GetInteractionsRowsByActorCombatantRelation()
                                               where ((dr.SecondHarmType == (byte)HarmType.Drain) ||
                                                      (dr.SecondAidType == (byte)AidType.Recovery)) &&
                                                      mobFilter.CheckFilterMobBattle(dr)
                                               select dr.SecondAmount,
                                 Cured = from cr in c.GetInteractionsRowsByTargetCombatantRelation()
                                         where ((cr.AidType == (byte)AidType.Recovery) &&
                                                (cr.RecoveryType == (byte)RecoveryType.RecoverHP)) &&
                                                mobFilter.CheckFilterMobBattle(cr)
                                         select cr.Amount,
                                 Regen1 = from cr in c.GetInteractionsRowsByTargetCombatantRelation()
                                          where ((cr.AidType == (byte)AidType.Enhance) &&
                                                 (cr.IsActionIDNull() == false) &&
                                                 (cr.ActionsRow.ActionName == "Regen")) &&
                                                 mobFilter.CheckFilterMobBattle(cr)
                                          select cr,
                                 Regen2 = from cr in c.GetInteractionsRowsByTargetCombatantRelation()
                                          where ((cr.AidType == (byte)AidType.Enhance) &&
                                                 (cr.IsActionIDNull() == false) &&
                                                 (cr.ActionsRow.ActionName == "Regen II")) &&
                                                 mobFilter.CheckFilterMobBattle(cr)
                                          select cr,
                                 Regen3 = from cr in c.GetInteractionsRowsByTargetCombatantRelation()
                                          where ((cr.AidType == (byte)AidType.Enhance) &&
                                                 (cr.IsActionIDNull() == false) &&
                                                 (cr.ActionsRow.ActionName == "Regen III")) &&
                                                 mobFilter.CheckFilterMobBattle(cr)
                                          select cr,
                             };

            int dmgTaken = 0;
            int drainAmt = 0;
            int healAmt = 0;
            int numR1 = 0;
            int numR2 = 0;
            int numR3 = 0;

            int ttlDmgTaken = 0;
            int ttlDrainAmt = 0;
            int ttlHealAmt = 0;
            int ttlNumR1 = 0;
            int ttlNumR2 = 0;
            int ttlNumR3 = 0;

            bool placeHeader = false;

            StringBuilder sb = new StringBuilder();

            if (playerData.Count() > 0)
            {
                foreach (var player in playerData)
                {
                    dmgTaken = player.PrimeDmgTaken.Sum() + player.SecondDmgTaken.Sum();
                    drainAmt = player.PrimeDrain.Sum() + player.SecondDrain.Sum();
                    healAmt = player.Cured.Sum();
                    numR1 = player.Regen1.Count();
                    numR2 = player.Regen2.Count();
                    numR3 = player.Regen3.Count();

                    if ((dmgTaken + drainAmt + healAmt + numR1 + numR2 + numR3) > 0)
                    {
                        if (placeHeader == false)
                        {
                            AppendText("Damage Recovery\n", Color.Blue, true, false);
                            AppendText(dmgRecoveryHeader, Color.Black, true, true);

                            placeHeader = true;
                        }

                        ttlDmgTaken += dmgTaken;
                        ttlDrainAmt += drainAmt;
                        ttlHealAmt += healAmt;
                        ttlNumR1 += numR1;
                        ttlNumR2 += numR2;
                        ttlNumR3 += numR3;

                        sb.Append(player.Player.PadRight(16));
                        sb.Append(" ");

                        sb.Append(dmgTaken.ToString().PadLeft(9));
                        sb.Append(drainAmt.ToString().PadLeft(13));
                        sb.Append(healAmt.ToString().PadLeft(11));

                        sb.Append(numR1.ToString().PadLeft(9));
                        sb.Append(numR2.ToString().PadLeft(11));
                        sb.Append(numR3.ToString().PadLeft(11));

                        sb.Append("\n");
                    }
                }

                if (placeHeader == true)
                {
                    AppendText(sb.ToString());
                    string totalString = string.Format(
                        "{0,-17}{1,9}{2,13}{3,11}{4,9}{5,11}{6,11}\n\n\n", "Total",
                        ttlDmgTaken, ttlDrainAmt, ttlHealAmt, ttlNumR1, ttlNumR2, ttlNumR3);
                    AppendText(totalString, Color.Black, true, false);
                }

            }
        }

        private void ProcessCuring(KPDatabaseDataSet dataSet, MobFilter mobFilter,
            bool displayCures, bool displayAvgCures)
        {
            var uberHealing = from c in dataSet.Combatants
                              where ((c.CombatantType == (byte)EntityType.Player) ||
                                     (c.CombatantType == (byte)EntityType.Pet) ||
                                     (c.CombatantType == (byte)EntityType.Fellow))
                              orderby c.CombatantName
                              select new
                              {
                                  Player = c.CombatantName,
                                  Cure1s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                           where ((cr.AidType == (byte)AidType.Recovery) &&
                                                  (cr.IsActionIDNull() == false) &&
                                                  ((cr.ActionsRow.ActionName == "Cure") ||
                                                   (cr.ActionsRow.ActionName == "Pollen") ||
                                                   (cr.ActionsRow.ActionName == "Healing Breath"))) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                           select cr.Amount,
                                  Cure2s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                           where ((cr.AidType == (byte)AidType.Recovery) &&
                                                  (cr.IsActionIDNull() == false) &&
                                                  ((cr.ActionsRow.ActionName == "Cure II") ||
                                                   (cr.ActionsRow.ActionName == "Curing Waltz") ||
                                                   (cr.ActionsRow.ActionName == "Healing Breath II"))) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                           select cr.Amount,
                                  Cure3s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                           where ((cr.AidType == (byte)AidType.Recovery) &&
                                                  (cr.IsActionIDNull() == false) &&
                                                  ((cr.ActionsRow.ActionName == "Cure III") ||
                                                   (cr.ActionsRow.ActionName == "Curing Waltz II") ||
                                                   (cr.ActionsRow.ActionName == "Wild Carrot") ||
                                                   (cr.ActionsRow.ActionName == "Healing Breath III"))) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                           select cr.Amount,
                                  Cure4s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                           where ((cr.AidType == (byte)AidType.Recovery) &&
                                                  (cr.IsActionIDNull() == false) &&
                                                  ((cr.ActionsRow.ActionName == "Cure IV") ||
                                                   (cr.ActionsRow.ActionName == "Curing Waltz III") ||
                                                   (cr.ActionsRow.ActionName == "Magic Fruit"))) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                           select cr.Amount,
                                  Cure5s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                           where ((cr.AidType == (byte)AidType.Recovery) &&
                                                  (cr.IsActionIDNull() == false) &&
                                                  (cr.ActionsRow.ActionName == "Cure V")) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                           select cr.Amount,
                                  Curagas = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                            where ((cr.AidType == (byte)AidType.Recovery) &&
                                                   (cr.IsActionIDNull() == false) &&
                                                   ((cr.ActionsRow.ActionName.StartsWith("Curaga")) ||
                                                    (cr.ActionsRow.ActionName == "Healing Breeze") ||
                                                    (cr.ActionsRow.ActionName == "Divine Waltz"))) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                            group cr by cr.Timestamp into crt
                                            select crt,
                                  OtherCures = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                               where ((cr.AidType == (byte)AidType.Recovery) &&
                                                      (cr.IsActionIDNull() == false) &&
                                                      (cr.ActionsRow.ActionName == "Chakra")) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                               select cr.Amount,
                                  Reg1s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                          where ((cr.AidType == (byte)AidType.Enhance) &&
                                                 (cr.IsActionIDNull() == false) &&
                                                 (cr.ActionsRow.ActionName == "Regen")) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                          select cr,
                                  Reg2s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                          where ((cr.AidType == (byte)AidType.Enhance) &&
                                                 (cr.IsActionIDNull() == false) &&
                                                 (cr.ActionsRow.ActionName == "Regen II")) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                          select cr,
                                  Reg3s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                          where ((cr.AidType == (byte)AidType.Enhance) &&
                                                 (cr.IsActionIDNull() == false) &&
                                                 (cr.ActionsRow.ActionName == "Regen III")) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                          select cr,
                                  Spells = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                           where ((cr.ActionType == (byte)ActionType.Spell) &&
                                                  (cr.AidType == (byte)AidType.Recovery) &&
                                                  (cr.RecoveryType == (byte)RecoveryType.RecoverHP)) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                           select cr.Amount,
                                  Ability = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                            where ((cr.ActionType == (byte)ActionType.Ability) &&
                                                   (cr.AidType == (byte)AidType.Recovery) &&
                                                   (cr.RecoveryType == (byte)RecoveryType.RecoverHP)) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                            select cr.Amount
                              };


            int cureSpell = 0;
            int cureAbil = 0;
            int numCure1 = 0;
            int numCure2 = 0;
            int numCure3 = 0;
            int numCure4 = 0;
            int numCure5 = 0;
            int numCuraga = 0;
            int numRegen1 = 0;
            int numRegen2 = 0;
            int numRegen3 = 0;

            double avgC1 = 0;
            double avgC2 = 0;
            double avgC3 = 0;
            double avgC4 = 0;
            double avgC5 = 0;
            double avgCg = 0;
            double avgAb = 0;

            StringBuilder sb;
            bool placeHeader = false;

            if (uberHealing.Count() > 0)
            {
                if (displayCures == true)
                {
                    sb = new StringBuilder();

                    foreach (var healer in uberHealing)
                    {
                        cureSpell = healer.Spells.Sum();
                        cureAbil = healer.Ability.Sum();
                        numCure1 = healer.Cure1s.Count();
                        numCure2 = healer.Cure2s.Count();
                        numCure3 = healer.Cure3s.Count();
                        numCure4 = healer.Cure4s.Count();
                        numCure5 = healer.Cure5s.Count();
                        numCuraga = healer.Curagas.Count();
                        numRegen1 = healer.Reg1s.Count();
                        numRegen2 = healer.Reg2s.Count();
                        numRegen3 = healer.Reg3s.Count();


                        if ((cureSpell + cureAbil + numCure1 + numCure2 + numCure3 + numCure4 + numCure5 +
                            numRegen1 + numRegen2 + numRegen3 + numCuraga) > 0)
                        {
                            if (placeHeader == false)
                            {
                                AppendText("Curing (Whm spells or equivalent)\n", Color.Blue, true, false);
                                AppendText(cureHeader, Color.Black, true, true);

                                placeHeader = true;
                            }

                            sb.Append(healer.Player.PadRight(16));
                            sb.Append(" ");

                            sb.Append(cureSpell.ToString().PadLeft(9));
                            sb.Append(cureAbil.ToString().PadLeft(12));

                            sb.Append(numCure1.ToString().PadLeft(7));
                            sb.Append(numCure2.ToString().PadLeft(6));
                            sb.Append(numCure3.ToString().PadLeft(6));
                            sb.Append(numCure4.ToString().PadLeft(6));
                            sb.Append(numCure5.ToString().PadLeft(6));

                            sb.Append(numCuraga.ToString().PadLeft(9));

                            sb.Append(numRegen1.ToString().PadLeft(7));
                            sb.Append(numRegen2.ToString().PadLeft(7));
                            sb.Append(numRegen3.ToString().PadLeft(7));

                            sb.Append("\n");
                        }
                    }

                    if (placeHeader == true)
                    {
                        sb.Append("\n\n");
                        AppendText(sb.ToString());
                    }
                }

                if (displayAvgCures == true)
                {
                    placeHeader = false;
                    sb = new StringBuilder();

                    foreach (var healer in uberHealing)
                    {
                        avgC1 = 0;
                        avgC2 = 0;
                        avgC3 = 0;
                        avgC4 = 0;
                        avgC5 = 0;
                        avgCg = 0;
                        avgAb = 0;

                        if (healer.Cure1s.Count() > 0)
                            avgC1 = healer.Cure1s.Average();
                        if (healer.Cure2s.Count() > 0)
                            avgC2 = healer.Cure2s.Average();
                        if (healer.Cure3s.Count() > 0)
                            avgC3 = healer.Cure3s.Average();
                        if (healer.Cure4s.Count() > 0)
                            avgC4 = healer.Cure4s.Average();
                        if (healer.Cure5s.Count() > 0)
                            avgC5 = healer.Cure5s.Average();

                        if (healer.Curagas.Count() > 0)
                            avgCg = healer.Curagas.Average(c => c.Sum(i => i.Amount));

                        if (healer.OtherCures.Count() > 0)
                            avgAb = healer.OtherCures.Average();


                        if ((avgAb + avgC1 + avgC2 + avgC3 + avgC4 + avgC5 + avgCg) > 0)
                        {
                            if (placeHeader == false)
                            {
                                AppendText("Average Curing (Whm spells or equivalent)\n", Color.Blue, true, false);
                                AppendText(avgCureHeader, Color.Black, true, true);

                                placeHeader = true;
                            }

                            sb.Append(healer.Player.PadRight(16));
                            sb.Append(" ");

                            sb.Append(avgC1.ToString("F2").PadLeft(10));
                            sb.Append(avgC2.ToString("F2").PadLeft(13));
                            sb.Append(avgC3.ToString("F2").PadLeft(13));
                            sb.Append(avgC4.ToString("F2").PadLeft(13));
                            sb.Append(avgC5.ToString("F2").PadLeft(13));
                            sb.Append(avgCg.ToString("F2").PadLeft(13));
                            sb.Append(avgAb.ToString("F2").PadLeft(14));

                            sb.Append("\n");
                        }
                    }

                    if (placeHeader == true)
                    {
                        sb.Append("\n\n");
                        AppendText(sb.ToString());
                    }
                }
            }
        }
        #endregion

        #region Event Handlers
        protected void categoryCombo_SelectedIndexChanged(object sender, EventArgs e)
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

        protected void groupMobs_Click(object sender, EventArgs e)
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

        #endregion

    }
}