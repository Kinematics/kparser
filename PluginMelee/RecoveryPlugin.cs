using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WaywardGamers.KParser;
using WaywardGamers.KParser.Database;

namespace WaywardGamers.KParser.Plugin
{
    public class RecoveryPlugin : BasePluginControl
    {
        #region Member Variables
        Dictionary<string, int> playerDamage = new Dictionary<string, int>();

        bool flagNoUpdate;
        bool groupMobs = true;
        bool exclude0XPMobs = false;
        bool customMobSelection = false;

        ToolStripLabel catLabel = new ToolStripLabel();
        ToolStripComboBox categoryCombo = new ToolStripComboBox();
        ToolStripLabel mobsLabel = new ToolStripLabel();
        ToolStripComboBox mobsCombo = new ToolStripComboBox();

        ToolStripDropDownButton optionsMenu = new ToolStripDropDownButton();
        ToolStripMenuItem groupMobsOption = new ToolStripMenuItem();
        ToolStripMenuItem exclude0XPOption = new ToolStripMenuItem();
        ToolStripMenuItem customMobSelectionOption = new ToolStripMenuItem();

        ToolStripButton editCustomMobFilter = new ToolStripButton();

        // Localized strings

        string lsTitleRecovery;
        string lsTitleCuring;
        string lsTitleAvgCuring;

        string lsHeaderRecovery;
        string lsHeaderCuring;
        string lsHeaderAvgCuring;

        string lsFormatRecovery;
        string lsFormatCuring;
        string lsFormatAvgCuring;

        string lsTotal;

        // Spell names

        string lsRegen1;
        string lsRegen2;
        string lsRegen3;
        string lsCure1;
        string lsCure2;
        string lsCure3;
        string lsCure4;
        string lsCure5;
        string lsCWaltz1;
        string lsCWaltz2;
        string lsCWaltz3;
        string lsCWaltz4;
        string lsHealingBreath1;
        string lsHealingBreath2;
        string lsHealingBreath3;
        string lsPollen;
        string lsWildCarrot;
        string lsMagicFruit;
        string lsDivineWaltz;
        string lsHealingBreeze;
        string lsChakra;

        string lsCuragaREString;
        Regex lsCuragaRegex;
        #endregion


        #region Constructor
        public RecoveryPlugin()
        {
            LoadLocalizedUI();

            categoryCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            categoryCombo.SelectedIndex = 0;
            categoryCombo.SelectedIndexChanged += new EventHandler(this.categoryCombo_SelectedIndexChanged);

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

            optionsMenu.DisplayStyle = ToolStripItemDisplayStyle.Text;
            optionsMenu.DropDownItems.Add(groupMobsOption);
            optionsMenu.DropDownItems.Add(exclude0XPOption);
            optionsMenu.DropDownItems.Add(customMobSelectionOption);


            editCustomMobFilter.Enabled = false;
            editCustomMobFilter.Click += new EventHandler(editCustomMobFilter_Click);


            ToolStripSeparator aSeparator = new ToolStripSeparator();

            toolStrip.Items.Add(catLabel);
            toolStrip.Items.Add(categoryCombo);
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

        #region Private Methods
        private void UpdateMobList()
        {
            UpdateMobList(false);
        }

        private void UpdateMobList(bool overrideGrouping)
        {
            mobsCombo.UpdateWithMobList(groupMobs, exclude0XPMobs);
        }
        #endregion

        #region Processing sections
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            if (dataSet == null)
                return;

            ResetTextBox();

            MobFilter mobFilter;
            if (customMobSelection)
                mobFilter = MobXPHandler.Instance.CustomMobFilter;
            else
                mobFilter = mobsCombo.CBGetMobFilter();

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

        private void ProcessDamage(KPDatabaseDataSet dataSet, MobFilter mobFilter)
        {
            var playerData = from c in dataSet.Combatants
                             where (((EntityType)c.CombatantType == EntityType.Player) ||
                                    ((EntityType)c.CombatantType == EntityType.Pet) ||
                                    ((EntityType)c.CombatantType == EntityType.CharmedMob) ||
                                    ((EntityType)c.CombatantType == EntityType.Fellow))
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
                                                 (cr.ActionsRow.ActionName == lsRegen1)) &&
                                                 mobFilter.CheckFilterMobBattle(cr)
                                          select cr,
                                 Regen2 = from cr in c.GetInteractionsRowsByTargetCombatantRelation()
                                          where ((cr.AidType == (byte)AidType.Enhance) &&
                                                 (cr.IsActionIDNull() == false) &&
                                                 (cr.ActionsRow.ActionName == lsRegen2)) &&
                                                 mobFilter.CheckFilterMobBattle(cr)
                                          select cr,
                                 Regen3 = from cr in c.GetInteractionsRowsByTargetCombatantRelation()
                                          where ((cr.AidType == (byte)AidType.Enhance) &&
                                                 (cr.IsActionIDNull() == false) &&
                                                 (cr.ActionsRow.ActionName == lsRegen3)) &&
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
                            AppendText(lsTitleRecovery, Color.Blue, true, false);
                            AppendText("\n");
                            AppendText(lsHeaderRecovery, Color.Black, true, true);
                            AppendText("\n");

                            placeHeader = true;
                        }

                        ttlDmgTaken += dmgTaken;
                        ttlDrainAmt += drainAmt;
                        ttlHealAmt += healAmt;
                        ttlNumR1 += numR1;
                        ttlNumR2 += numR2;
                        ttlNumR3 += numR3;

                        sb.AppendFormat(lsFormatRecovery,
                            player.Player,
                            dmgTaken,
                            drainAmt,
                            healAmt,
                            numR1,
                            numR2,
                            numR3);

                        sb.Append("\n");
                    }
                }

                if (placeHeader == true)
                {
                    AppendText(sb.ToString());

                    string totalString = string.Format(lsFormatRecovery,
                        lsTotal,
                        ttlDmgTaken,
                        ttlDrainAmt,
                        ttlHealAmt,
                        ttlNumR1,
                        ttlNumR2,
                        ttlNumR3);

                    AppendText(totalString, Color.Black, true, false);
                    AppendText("\n\n\n");
                }
            }
        }

        private void ProcessCuring(KPDatabaseDataSet dataSet, MobFilter mobFilter,
            bool displayCures, bool displayAvgCures)
        {
            var uberHealing = from c in dataSet.Combatants
                              where (((EntityType)c.CombatantType == EntityType.Player) ||
                                     ((EntityType)c.CombatantType == EntityType.Pet) ||
                                     ((EntityType)c.CombatantType == EntityType.CharmedMob) ||
                                     ((EntityType)c.CombatantType == EntityType.Fellow))
                              orderby c.CombatantName
                              select new
                              {
                                  Player = c.CombatantName,
                                  Cure1s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                           where (((AidType)cr.AidType == AidType.Recovery) &&
                                                  (cr.IsActionIDNull() == false) &&
                                                  ((cr.ActionsRow.ActionName == lsCure1) ||
                                                   (cr.ActionsRow.ActionName == lsPollen) ||
                                                   (cr.ActionsRow.ActionName == lsHealingBreath1))) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                           select cr.Amount,
                                  Cure2s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                           where (((AidType)cr.AidType == AidType.Recovery) &&
                                                  (cr.IsActionIDNull() == false) &&
                                                  ((cr.ActionsRow.ActionName == lsCure2) ||
                                                   (cr.ActionsRow.ActionName == lsCWaltz1) ||
                                                   (cr.ActionsRow.ActionName == lsHealingBreath2))) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                           select cr.Amount,
                                  Cure3s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                           where (((AidType)cr.AidType == AidType.Recovery) &&
                                                  (cr.IsActionIDNull() == false) &&
                                                  ((cr.ActionsRow.ActionName == lsCure3) ||
                                                   (cr.ActionsRow.ActionName == lsCWaltz2) ||
                                                   (cr.ActionsRow.ActionName == lsWildCarrot) ||
                                                   (cr.ActionsRow.ActionName == lsHealingBreath3))) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                           select cr.Amount,
                                  Cure4s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                           where (((AidType)cr.AidType == AidType.Recovery) &&
                                                  (cr.IsActionIDNull() == false) &&
                                                  ((cr.ActionsRow.ActionName == lsCure4) ||
                                                   (cr.ActionsRow.ActionName == lsCWaltz3) ||
                                                   (cr.ActionsRow.ActionName == lsMagicFruit))) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                           select cr.Amount,
                                  Cure5s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                           where (((AidType)cr.AidType == AidType.Recovery) &&
                                                  (cr.IsActionIDNull() == false) &&
                                                  ((cr.ActionsRow.ActionName == lsCure5) ||
                                                  (cr.ActionsRow.ActionName == lsCWaltz4))) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                           select cr.Amount,
                                  Curagas = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                            where (((AidType)cr.AidType == AidType.Recovery) &&
                                                   (cr.IsActionIDNull() == false) &&
                                                   ((lsCuragaRegex.Match(cr.ActionsRow.ActionName).Success) ||
                                                    (cr.ActionsRow.ActionName == lsHealingBreeze) ||
                                                    (cr.ActionsRow.ActionName == lsDivineWaltz))) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                            group cr by cr.Timestamp into crt
                                            select crt,
                                  OtherCures = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                               where (((AidType)cr.AidType == AidType.Recovery) &&
                                                      (cr.IsActionIDNull() == false) &&
                                                      (cr.ActionsRow.ActionName == lsChakra)) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                               select cr.Amount,
                                  Reg1s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                          where (((AidType)cr.AidType == AidType.Enhance) &&
                                                 (cr.IsActionIDNull() == false) &&
                                                 (cr.ActionsRow.ActionName == lsRegen1)) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                          select cr,
                                  Reg2s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                          where (((AidType)cr.AidType == AidType.Enhance) &&
                                                 (cr.IsActionIDNull() == false) &&
                                                 (cr.ActionsRow.ActionName == lsRegen2)) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                          select cr,
                                  Reg3s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                          where (((AidType)cr.AidType == AidType.Enhance) &&
                                                 (cr.IsActionIDNull() == false) &&
                                                 (cr.ActionsRow.ActionName == lsRegen3)) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                          select cr,
                                  Spells = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                           where (((ActionType)cr.ActionType == ActionType.Spell) &&
                                                  ((AidType)cr.AidType == AidType.Recovery) &&
                                                  ((RecoveryType)cr.RecoveryType == RecoveryType.RecoverHP)) &&
                                                  mobFilter.CheckFilterMobBattle(cr)
                                           select cr.Amount,
                                  Ability = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                            where (((ActionType)cr.ActionType == ActionType.Ability) &&
                                                   ((AidType)cr.AidType == AidType.Recovery) &&
                                                   ((RecoveryType)cr.RecoveryType == RecoveryType.RecoverHP)) &&
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
                                AppendText(lsTitleCuring, Color.Blue, true, false);
                                AppendText("\n");
                                AppendText(lsHeaderCuring, Color.Black, true, true);
                                AppendText("\n");

                                placeHeader = true;
                            }

                            sb.AppendFormat(lsFormatCuring,
                                healer.Player,
                                cureSpell,
                                cureAbil,
                                numCure1,
                                numCure2,
                                numCure3,
                                numCure4,
                                numCure5,
                                numCuraga,
                                numRegen1,
                                numRegen2,
                                numRegen3);

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
                                AppendText(lsTitleAvgCuring, Color.Blue, true, false);
                                AppendText("\n");
                                AppendText(lsHeaderAvgCuring, Color.Black, true, true);
                                AppendText("\n");

                                placeHeader = true;
                            }


                            sb.AppendFormat(lsFormatAvgCuring,
                                healer.Player,
                                avgC1,
                                avgC2,
                                avgC3,
                                avgC4,
                                avgC5,
                                avgCg,
                                avgAb);

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

        protected override void OnCustomMobFilterChanged()
        {
            HandleDataset(null);
        }

        #endregion

        #region Localization Overrides
        protected override void LoadLocalizedUI()
        {
            catLabel.Text = Resources.PublicResources.CategoryLabel;
            mobsLabel.Text = Resources.PublicResources.MobsLabel;

            categoryCombo.Items.Clear();
            categoryCombo.Items.Add(Resources.PublicResources.All);
            categoryCombo.Items.Add(Resources.Combat.RecoveryPluginCategoryRecovery);
            categoryCombo.Items.Add(Resources.Combat.RecoveryPluginCategoryCuring);
            categoryCombo.Items.Add(Resources.Combat.RecoveryPluginCategoryAvgCuring);
            categoryCombo.SelectedIndex = 0;

            UpdateMobList();
            mobsCombo.SelectedIndex = 0;

            optionsMenu.Text = Resources.PublicResources.Options;
            groupMobsOption.Text = Resources.PublicResources.GroupMobs;
            exclude0XPOption.Text = Resources.PublicResources.Exclude0XPMobs;
            customMobSelectionOption.Text = Resources.PublicResources.CustomMobSelection;
            editCustomMobFilter.Text = Resources.PublicResources.EditMobFilter;

        }

        protected override void LoadResources()
        {
            this.tabName = Resources.Combat.RecoveryPluginTabName;

            // Titles

            lsTitleRecovery = Resources.Combat.RecoveryPluginTitleRecovery;
            lsTitleCuring = Resources.Combat.RecoveryPluginTitleCuring;
            lsTitleAvgCuring = Resources.Combat.RecoveryPluginTitleAvgCuring;


            // Headers

            lsHeaderRecovery = Resources.Combat.RecoveryPluginHeaderRecovery;
            lsHeaderCuring = Resources.Combat.RecoveryPluginHeaderCuring;
            lsHeaderAvgCuring = Resources.Combat.RecoveryPluginHeaderAvgCuring;

            // Formatters

            lsFormatRecovery = Resources.Combat.RecoveryPluginFormatRecovery;
            lsFormatCuring = Resources.Combat.RecoveryPluginFormatCuring;
            lsFormatAvgCuring = Resources.Combat.RecoveryPluginFormatAvgCuring;

            // Misc

            lsTotal = Resources.PublicResources.Total;

            // Spell names

            lsRegen1 = Resources.ParsedStrings.Regen1;
            lsRegen2 = Resources.ParsedStrings.Regen2;
            lsRegen3 = Resources.ParsedStrings.Regen3;
            lsCure1 = Resources.ParsedStrings.Cure1;
            lsCure2 = Resources.ParsedStrings.Cure2;
            lsCure3 = Resources.ParsedStrings.Cure3;
            lsCure4 = Resources.ParsedStrings.Cure4;
            lsCure5 = Resources.ParsedStrings.Cure5;
            lsCWaltz1 = Resources.ParsedStrings.CuringWaltz1;
            lsCWaltz2 = Resources.ParsedStrings.CuringWaltz2;
            lsCWaltz3 = Resources.ParsedStrings.CuringWaltz3;
            lsCWaltz4 = Resources.ParsedStrings.CuringWaltz4;
            lsHealingBreath1 = Resources.ParsedStrings.HealingBreath1;
            lsHealingBreath2 = Resources.ParsedStrings.HealingBreath2;
            lsHealingBreath3 = Resources.ParsedStrings.HealingBreath3;
            lsPollen = Resources.ParsedStrings.Pollen;
            lsWildCarrot = Resources.ParsedStrings.WildCarrot;
            lsMagicFruit = Resources.ParsedStrings.MagicFruit;
            lsDivineWaltz = Resources.ParsedStrings.DivineWaltz;
            lsHealingBreeze = Resources.ParsedStrings.HealingBreeze;
            lsCuragaREString = Resources.ParsedStrings.CuragaRegex;
            lsChakra = Resources.ParsedStrings.Chakra;

            lsCuragaRegex = new Regex(lsCuragaREString);
        }
        #endregion
    }
}