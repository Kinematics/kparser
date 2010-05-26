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
        string lsTitleStatusCuring;
        string lsTitleStatusCured;
        string lsTitleCuringCost;

        string lsHeaderRecovery;
        string lsHeaderCuring;
        string lsHeaderAvgCuring;
        string lsHeaderStatusCuring;
        string lsHeaderCuringCost;

        string lsFormatRecovery;
        string lsFormatCuring;
        string lsFormatAvgCuring;
        string lsFormatStatusCuring;
        string lsFormatStatusCuringSub;
        string lsFormatCuringCost;

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
        string lsCura;
        string lsCuraga1;
        string lsCuraga2;
        string lsCuraga3;
        string lsCuraga4;

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
                mobFilter = mobsCombo.CBGetMobFilter(exclude0XPMobs);

            StringBuilder sb = new StringBuilder();
            List<StringMods> strModList = new List<StringMods>();

            switch (categoryCombo.CBSelectedIndex())
            {
                case 0:
                    // All
                    ProcessDamage(dataSet, mobFilter, ref sb, strModList);
                    ProcessCuring(dataSet, mobFilter, true, true, ref sb, strModList);
                    ProcessStatusCuring(dataSet, mobFilter, ref sb, strModList);
                    ProcessStatusCured(dataSet, mobFilter, ref sb, strModList);
                    break;
                case 1:
                    // Recovery
                    ProcessDamage(dataSet, mobFilter, ref sb, strModList);
                    break;
                case 2:
                    // Curing
                    ProcessCuring(dataSet, mobFilter, true, false, ref sb, strModList);
                    break;
                case 3:
                    // AverageCuring
                    ProcessCuring(dataSet, mobFilter, false, true, ref sb, strModList);
                    break;
                case 4:
                    // Status healing
                    ProcessStatusCuring(dataSet, mobFilter, ref sb, strModList);
                    break;
                case 5:
                    // Statuses healed
                    ProcessStatusCured(dataSet, mobFilter, ref sb, strModList);
                    break;
            }

            PushStrings(sb, strModList);

        }

        private void ProcessDamage(KPDatabaseDataSet dataSet, MobFilter mobFilter,
            ref StringBuilder sb, List<StringMods> strModList)
        {
            var playerData = from c in dataSet.Combatants
                             where (((EntityType)c.CombatantType == EntityType.Player) ||
                                    ((EntityType)c.CombatantType == EntityType.Pet) ||
                                    ((EntityType)c.CombatantType == EntityType.CharmedMob) ||
                                    ((EntityType)c.CombatantType == EntityType.Fellow))
                             orderby c.CombatantType, c.CombatantName
                              let targetInteractions = c.GetInteractionsRowsByTargetCombatantRelation().Where(a => mobFilter.CheckFilterMobBattle(a))
                              let actorInteractions = c.GetInteractionsRowsByActorCombatantRelation().Where(a => mobFilter.CheckFilterMobBattle(a))
                              select new
                             {
                                 Player = c.CombatantNameOrJobName,
                                 PrimeDmgTaken = from dm in targetInteractions
                                                 where ((dm.HarmType == (byte)HarmType.Damage) ||
                                                        (dm.HarmType == (byte)HarmType.Drain))
                                                 select dm.Amount,
                                 SecondDmgTaken = from dm in targetInteractions
                                                  where ((dm.SecondHarmType == (byte)HarmType.Damage) ||
                                                         (dm.SecondHarmType == (byte)HarmType.Drain))
                                                  select dm.SecondAmount,
                                 PrimeDrain = from dr in actorInteractions
                                              where (dr.HarmType == (byte)HarmType.Drain) &&
                                                    mobFilter.CheckFilterMobBattle(dr)
                                              select dr.Amount,
                                 SecondDrain = from dr in actorInteractions
                                               where ((dr.SecondHarmType == (byte)HarmType.Drain) ||
                                                      (dr.SecondAidType == (byte)AidType.Recovery))
                                               select dr.SecondAmount,
                                 Cured = from cr in targetInteractions
                                         where ((cr.AidType == (byte)AidType.Recovery) &&
                                                (cr.RecoveryType == (byte)RecoveryType.RecoverHP))
                                         select cr.Amount,
                                 Regen1 = from cr in targetInteractions
                                          where ((cr.AidType == (byte)AidType.Enhance) &&
                                                 (cr.IsActionIDNull() == false) &&
                                                 (cr.ActionsRow.ActionName == lsRegen1))
                                          select cr,
                                 Regen2 = from cr in targetInteractions
                                          where ((cr.AidType == (byte)AidType.Enhance) &&
                                                 (cr.IsActionIDNull() == false) &&
                                                 (cr.ActionsRow.ActionName == lsRegen2))
                                          select cr,
                                 Regen3 = from cr in targetInteractions
                                          where ((cr.AidType == (byte)AidType.Enhance) &&
                                                 (cr.IsActionIDNull() == false) &&
                                                 (cr.ActionsRow.ActionName == lsRegen3))
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
                            strModList.Add(new StringMods
                            {
                                Start = sb.Length,
                                Length = lsTitleRecovery.Length,
                                Bold = true,
                                Color = Color.Red
                            });
                            sb.Append(lsTitleRecovery + "\n\n");

                            strModList.Add(new StringMods
                            {
                                Start = sb.Length,
                                Length = lsHeaderRecovery.Length,
                                Bold = true,
                                Underline = true,
                                Color = Color.Black
                            });
                            sb.Append(lsHeaderRecovery + "\n");

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
                    string totalString = string.Format(lsFormatRecovery,
                        lsTotal,
                        ttlDmgTaken,
                        ttlDrainAmt,
                        ttlHealAmt,
                        ttlNumR1,
                        ttlNumR2,
                        ttlNumR3);

                    strModList.Add(new StringMods
                    {
                        Start = sb.Length,
                        Length = totalString.Length,
                        Bold = true,
                        Color = Color.Black
                    });
                    sb.Append(totalString + "\n\n\n");
                }
            }
        }

        private void ProcessCuring(KPDatabaseDataSet dataSet, MobFilter mobFilter,
            bool displayCures, bool displayAvgCures,
            ref StringBuilder sb, List<StringMods> strModList)
        {
            var uberHealing = from c in dataSet.Combatants
                              where (((EntityType)c.CombatantType == EntityType.Player) ||
                                     ((EntityType)c.CombatantType == EntityType.Pet) ||
                                     ((EntityType)c.CombatantType == EntityType.CharmedMob) ||
                                     ((EntityType)c.CombatantType == EntityType.Fellow))
                              orderby c.CombatantName
                              let actorInteractions = c.GetInteractionsRowsByActorCombatantRelation()
                                        .Where(a => mobFilter.CheckFilterMobBattle(a) &&
                                                    a.IsActionIDNull() == false &&
                                                    ((AidType)a.AidType == AidType.Recovery ||
                                                     (AidType)a.AidType == AidType.Enhance))
                              select new
                              {
                                  Player = c.CombatantNameOrJobName,
                                  Cure1 = from cr in actorInteractions
                                          where cr.ActionsRow.ActionName == lsCure1
                                          select cr.Amount,
                                  Cure2 = from cr in actorInteractions
                                          where cr.ActionsRow.ActionName == lsCure2
                                          select cr.Amount,
                                  Cure3 = from cr in actorInteractions
                                          where cr.ActionsRow.ActionName == lsCure3
                                          select cr.Amount,
                                  Cure4 = from cr in actorInteractions
                                          where cr.ActionsRow.ActionName == lsCure4
                                          select cr.Amount,
                                  Cure5 = from cr in actorInteractions
                                          where cr.ActionsRow.ActionName == lsCure5
                                          select cr.Amount,
                                  Cura = from cr in actorInteractions
                                         where cr.ActionsRow.ActionName == lsCura
                                         group cr by cr.Timestamp into crt
                                         select crt,
                                  Curaga1 = from cr in actorInteractions
                                            where cr.ActionsRow.ActionName == lsCuraga1
                                            group cr by cr.Timestamp into crt
                                            select crt,
                                  Curaga2 = from cr in actorInteractions
                                            where cr.ActionsRow.ActionName == lsCuraga2
                                            group cr by cr.Timestamp into crt
                                            select crt,
                                  Curaga3 = from cr in actorInteractions
                                            where cr.ActionsRow.ActionName == lsCuraga3
                                            group cr by cr.Timestamp into crt
                                            select crt,
                                  Curaga4 = from cr in actorInteractions
                                            where cr.ActionsRow.ActionName == lsCuraga4
                                            group cr by cr.Timestamp into crt
                                            select crt,
                                  Pollen = from cr in actorInteractions
                                           where cr.ActionsRow.ActionName == lsPollen
                                           select cr.Amount,
                                  Carrot = from cr in actorInteractions
                                           where cr.ActionsRow.ActionName == lsWildCarrot
                                           select cr.Amount,
                                  Fruit = from cr in actorInteractions
                                          where cr.ActionsRow.ActionName == lsMagicFruit
                                          select cr.Amount,
                                  Breeze = from cr in actorInteractions
                                           where cr.ActionsRow.ActionName == lsHealingBreeze
                                           group cr by cr.Timestamp into crt
                                           select crt,
                                  Breath1 = from cr in actorInteractions
                                            where cr.ActionsRow.ActionName == lsHealingBreath1
                                            select cr.Amount,
                                  Breath2 = from cr in actorInteractions
                                            where cr.ActionsRow.ActionName == lsHealingBreath2
                                            select cr.Amount,
                                  Breath3 = from cr in actorInteractions
                                            where cr.ActionsRow.ActionName == lsHealingBreath3
                                            select cr.Amount,
                                  Waltz1 = from cr in actorInteractions
                                           where cr.ActionsRow.ActionName == lsCWaltz1
                                           select cr.Amount,
                                  Waltz2 = from cr in actorInteractions
                                           where cr.ActionsRow.ActionName == lsCWaltz2
                                           select cr.Amount,
                                  Waltz3 = from cr in actorInteractions
                                           where cr.ActionsRow.ActionName == lsCWaltz3
                                           select cr.Amount,
                                  Waltz4 = from cr in actorInteractions
                                           where cr.ActionsRow.ActionName == lsCWaltz4
                                           select cr.Amount,
                                  DivineWaltz = from cr in actorInteractions
                                                where cr.ActionsRow.ActionName == lsDivineWaltz
                                                group cr by cr.Timestamp into crt
                                                select crt,
                                  Chakra = from cr in actorInteractions
                                           where cr.ActionsRow.ActionName == lsChakra
                                           select cr.Amount,
                                  Regen1 = from cr in actorInteractions
                                           where cr.ActionsRow.ActionName == lsRegen1
                                           select cr.Amount,
                                  Regen2 = from cr in actorInteractions
                                           where cr.ActionsRow.ActionName == lsRegen2
                                           select cr.Amount,
                                  Regen3 = from cr in actorInteractions
                                           where cr.ActionsRow.ActionName == lsRegen3
                                           select cr.Amount,
                                  Curagas = from cr in actorInteractions
                                            where (((AidType)cr.AidType == AidType.Recovery) &&
                                                   (cr.IsActionIDNull() == false) &&
                                                   ((lsCuragaRegex.Match(cr.ActionsRow.ActionName).Success) ||
                                                    (cr.ActionsRow.ActionName == lsHealingBreeze) ||
                                                    (cr.ActionsRow.ActionName == lsDivineWaltz)))
                                            group cr by cr.Timestamp into crt
                                            select crt,
                                  Spells = from cr in actorInteractions
                                           where (((ActionType)cr.ActionType == ActionType.Spell) &&
                                                  ((AidType)cr.AidType == AidType.Recovery) &&
                                                  ((RecoveryType)cr.RecoveryType == RecoveryType.RecoverHP))
                                           select cr.Amount,
                                  Ability = from cr in actorInteractions
                                            where (((ActionType)cr.ActionType == ActionType.Ability) &&
                                                   ((AidType)cr.AidType == AidType.Recovery) &&
                                                   ((RecoveryType)cr.RecoveryType == RecoveryType.RecoverHP))
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

            bool placeHeader = false;

            if (uberHealing.Count() > 0)
            {
                if (displayCures == true)
                {
                    foreach (var healer in uberHealing)
                    {
                        cureSpell = healer.Spells.Sum();
                        cureAbil = healer.Ability.Sum();
                        numCure1 = healer.Cure1.Count() + healer.Pollen.Count() + healer.Breath1.Count();
                        numCure2 = healer.Cure2.Count() + healer.Waltz1.Count() + healer.Breath2.Count();
                        numCure3 = healer.Cure3.Count() + healer.Waltz2.Count() + healer.Breath3.Count() + healer.Carrot.Count();
                        numCure4 = healer.Cure4.Count() + healer.Waltz3.Count() + healer.Fruit.Count();
                        numCure5 = healer.Cure5.Count() + healer.Waltz4.Count();
                        numCuraga = healer.Breeze.Count() + healer.DivineWaltz.Count() +
                            healer.Cura.Count() + healer.Curaga1.Count() + healer.Curaga2.Count() +
                            healer.Curaga3.Count() + healer.Curaga4.Count();
                        numRegen1 = healer.Regen1.Count();
                        numRegen2 = healer.Regen2.Count();
                        numRegen3 = healer.Regen3.Count();


                        if ((cureSpell + cureAbil + numCure1 + numCure2 + numCure3 + numCure4 + numCure5 +
                            numRegen1 + numRegen2 + numRegen3 + numCuraga) > 0)
                        {
                            if (placeHeader == false)
                            {
                                strModList.Add(new StringMods
                                {
                                    Start = sb.Length,
                                    Length = lsTitleCuring.Length,
                                    Bold = true,
                                    Color = Color.Red
                                });
                                sb.Append(lsTitleCuring + "\n\n");

                                strModList.Add(new StringMods
                                {
                                    Start = sb.Length,
                                    Length = lsHeaderCuring.Length,
                                    Bold = true,
                                    Underline = true,
                                    Color = Color.Black
                                });
                                sb.Append(lsHeaderCuring + "\n");

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

                    if (placeHeader)
                    {
                        sb.Append("\n\n");

                        strModList.Add(new StringMods
                        {
                            Start = sb.Length,
                            Length = lsTitleCuringCost.Length,
                            Bold = true,
                            Color = Color.Blue
                        });
                        sb.Append(lsTitleCuringCost + "\n\n");

                        strModList.Add(new StringMods
                        {
                            Start = sb.Length,
                            Length = lsHeaderCuringCost.Length,
                            Bold = true,
                            Underline = true,
                            Color = Color.Black
                        });
                        sb.Append(lsHeaderCuringCost + "\n");


                        // Estimate costs
                        foreach (var healer in uberHealing)
                        {
                            int totalMP = 0;

                            totalMP += healer.Cure1.Count() * 8;
                            totalMP += healer.Cure2.Count() * 24;
                            totalMP += healer.Cure3.Count() * 46;
                            totalMP += healer.Cure4.Count() * 88;
                            totalMP += healer.Cure5.Count() * 135;
                            totalMP += healer.Regen1.Count() * 15;
                            totalMP += healer.Regen2.Count() * 36;
                            totalMP += healer.Regen3.Count() * 64;
                            totalMP += healer.Pollen.Count() * 8;
                            totalMP += healer.Carrot.Count() * 37;
                            totalMP += healer.Fruit.Count() * 72;
                            totalMP += healer.Breeze.Count() * 55;
                            totalMP += healer.Cura.Count() * 30;
                            totalMP += healer.Curaga1.Count() * 60;
                            totalMP += healer.Curaga2.Count() * 120;
                            totalMP += healer.Curaga3.Count() * 180;
                            totalMP += healer.Curaga4.Count() * 260;

                            int totalTP = 0;
                            totalTP += healer.Waltz1.Count() * 20;
                            totalTP += healer.Waltz2.Count() * 35;
                            totalTP += healer.Waltz3.Count() * 50;
                            totalTP += healer.Waltz4.Count() * 65;
                            totalTP += healer.DivineWaltz.Count() * 40;


                            if ((totalMP > 0) || (totalTP > 0))
                            {
                                sb.AppendFormat(lsFormatCuringCost,
                                    healer.Player,
                                    totalMP,
                                    totalTP);

                                sb.Append("\n");
                            }
                        }

                        sb.Append("\n\n");
                    }
                }

                if (displayAvgCures == true)
                {
                    placeHeader = false;

                    foreach (var healer in uberHealing)
                    {
                        avgC1 = 0;
                        avgC2 = 0;
                        avgC3 = 0;
                        avgC4 = 0;
                        avgC5 = 0;
                        avgCg = 0;
                        avgAb = 0;

                        var cure1s = healer.Cure1.Concat(healer.Pollen).Concat(healer.Breath1);
                        if (cure1s.Count() > 0)
                            avgC1 = cure1s.Average();

                        var cure2s = healer.Cure2.Concat(healer.Waltz1).Concat(healer.Breath2);
                        if (cure2s.Count() > 0)
                            avgC2 = cure2s.Average();

                        var cure3s = healer.Cure3.Concat(healer.Waltz2).Concat(healer.Breath3).Concat(healer.Carrot);
                        if (cure3s.Count() > 0)
                            avgC3 = cure3s.Average();

                        var cure4s = healer.Cure4.Concat(healer.Waltz3).Concat(healer.Fruit);
                        if (cure4s.Count() > 0)
                            avgC4 = cure4s.Average();

                        var cure5s = healer.Cure5.Concat(healer.Waltz4);
                        if (cure5s.Count() > 0)
                            avgC5 = cure5s.Average();

                        var curagas = healer.Cura.Concat(healer.Curaga1).Concat(healer.Curaga2)
                            .Concat(healer.Curaga3).Concat(healer.Curaga4).Concat(healer.Breeze)
                            .Concat(healer.DivineWaltz);
                        if (healer.Curagas.Count() > 0)
                            avgCg = healer.Curagas.Average(c => c.Sum(i => i.Amount));

                        var otherCures = healer.Chakra;
                        if (otherCures.Count() > 0)
                            avgAb = otherCures.Average();


                        if ((avgAb + avgC1 + avgC2 + avgC3 + avgC4 + avgC5 + avgCg) > 0)
                        {
                            if (placeHeader == false)
                            {
                                strModList.Add(new StringMods
                                {
                                    Start = sb.Length,
                                    Length = lsTitleAvgCuring.Length,
                                    Bold = true,
                                    Color = Color.Red
                                });
                                sb.Append(lsTitleAvgCuring + "\n\n");

                                strModList.Add(new StringMods
                                {
                                    Start = sb.Length,
                                    Length = lsHeaderAvgCuring.Length,
                                    Bold = true,
                                    Underline = true,
                                    Color = Color.Black
                                });
                                sb.Append(lsHeaderAvgCuring + "\n");

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

                    if (placeHeader)
                        sb.Append("\n\n");
                }
            }
        }

        private void ProcessStatusCuring(KPDatabaseDataSet dataSet, MobFilter mobFilter,
            ref StringBuilder sb, List<StringMods> strModList)
        {
            var statusHealing = from c in dataSet.Combatants
                                where (((EntityType)c.CombatantType == EntityType.Player) ||
                                       ((EntityType)c.CombatantType == EntityType.Pet) ||
                                       ((EntityType)c.CombatantType == EntityType.Fellow))
                                orderby c.CombatantType, c.CombatantName
                                let actorInteractions = c.GetInteractionsRowsByActorCombatantRelation().Where(a => mobFilter.CheckFilterMobBattle(a))
                                select new
                                {
                                    Player = c.CombatantNameOrJobName,
                                    StatusRemovals = from cr in actorInteractions
                                             where ((AidType)cr.AidType == AidType.RemoveStatus) &&
                                                   (cr.IsActionIDNull() == false)
                                             group cr by cr.ActionsRow.ActionName
                                };

            bool placeHeader = false;

            foreach (var player in statusHealing)
            {
                if (player.StatusRemovals.Count() > 0)
                {
                    if (placeHeader == false)
                    {
                        strModList.Add(new StringMods
                        {
                            Start = sb.Length,
                            Length = lsTitleStatusCuring.Length,
                            Bold = true,
                            Color = Color.Red
                        });
                        sb.Append(lsTitleStatusCuring + "\n\n");

                        placeHeader = true;
                    }

                    strModList.Add(new StringMods
                    {
                        Start = sb.Length,
                        Length = player.Player.Length,
                        Bold = true,
                        Color = Color.Blue
                    });
                    sb.Append(player.Player + "\n");

                    strModList.Add(new StringMods
                    {
                        Start = sb.Length,
                        Length = lsHeaderStatusCuring.Length,
                        Bold = true,
                        Underline = true,
                        Color = Color.Black
                    });
                    sb.Append(lsHeaderStatusCuring + "\n");


                    foreach (var statusSpell in player.StatusRemovals)
                    {
                        int spellUsed = statusSpell.Count();
                        int spellNoEffect = statusSpell.Count(a => (FailedActionType)a.FailedActionType
                            == FailedActionType.NoEffect);

                        sb.AppendFormat(lsFormatStatusCuring,
                            statusSpell.Key,
                            spellUsed,
                            spellNoEffect);
                        sb.Append("\n");

                        var effects = statusSpell.GroupBy(a =>
                            a.IsSecondActionIDNull() ? "-unknown-" :
                            a.ActionsRowBySecondaryActionNameRelation.ActionName);

                        foreach (var effect in effects)
                        {
                            if (effect.Key != "-unknown-")
                            {
                                sb.AppendFormat(lsFormatStatusCuringSub,
                                    effect.Key,
                                    effect.Count());
                                sb.Append("\n");
                            }
                        }
                    }

                    sb.Append("\n\n");
                }
            }
        }

        private void ProcessStatusCured(KPDatabaseDataSet dataSet, MobFilter mobFilter,
            ref StringBuilder sb, List<StringMods> strModList)
        {
            var statusHealing = from c in dataSet.Combatants
                                where (((EntityType)c.CombatantType == EntityType.Player) ||
                                       ((EntityType)c.CombatantType == EntityType.Pet) ||
                                       ((EntityType)c.CombatantType == EntityType.Fellow))
                                orderby c.CombatantType, c.CombatantName
                                let targetInteractions = c.GetInteractionsRowsByTargetCombatantRelation().Where(a => mobFilter.CheckFilterMobBattle(a))
                                select new
                                {
                                    Player = c.CombatantNameOrJobName,
                                    StatusRemovals = from cr in targetInteractions
                                                     where ((AidType)cr.AidType == AidType.RemoveStatus) &&
                                                           (cr.IsActionIDNull() == false)
                                                     group cr by cr.ActionsRow.ActionName
                                };

            bool placeHeader = false;

            foreach (var player in statusHealing)
            {
                if (player.StatusRemovals.Count() > 0)
                {
                    if (placeHeader == false)
                    {
                        strModList.Add(new StringMods
                        {
                            Start = sb.Length,
                            Length = lsTitleStatusCured.Length,
                            Bold = true,
                            Color = Color.Red
                        });
                        sb.Append(lsTitleStatusCured + "\n\n");

                        placeHeader = true;
                    }

                    strModList.Add(new StringMods
                    {
                        Start = sb.Length,
                        Length = player.Player.Length,
                        Bold = true,
                        Color = Color.Blue
                    });
                    sb.Append(player.Player + "\n");

                    strModList.Add(new StringMods
                    {
                        Start = sb.Length,
                        Length = lsHeaderStatusCuring.Length,
                        Bold = true,
                        Underline = true,
                        Color = Color.Black
                    });
                    sb.Append(lsHeaderStatusCuring + "\n");


                    foreach (var statusSpell in player.StatusRemovals)
                    {
                        int spellUsed = statusSpell.Count();
                        int spellNoEffect = statusSpell.Count(a => (FailedActionType)a.FailedActionType
                            == FailedActionType.NoEffect);

                        sb.AppendFormat(lsFormatStatusCuring,
                            statusSpell.Key,
                            spellUsed,
                            spellNoEffect);
                        sb.Append("\n");

                        var effects = statusSpell.GroupBy(a =>
                            a.IsSecondActionIDNull() ? "-unknown-" :
                            a.ActionsRowBySecondaryActionNameRelation.ActionName);

                        foreach (var effect in effects)
                        {
                            if (effect.Key != "-unknown-")
                            {
                                sb.AppendFormat(lsFormatStatusCuringSub,
                                    effect.Key,
                                    effect.Count());
                                sb.Append("\n");
                            }
                        }
                    }

                    sb.Append("\n");

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
            categoryCombo.Items.Add(Resources.Combat.RecoveryPluginCategoryStatusCuring);
            categoryCombo.Items.Add(Resources.Combat.RecoveryPluginCategoryStatusCured);
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
            lsTitleStatusCuring = Resources.Combat.RecoveryPluginTitleStatusCuring;
            lsTitleStatusCured = Resources.Combat.RecoveryPluginTitleStatusCured;
            lsTitleCuringCost = Resources.Combat.RecoveryPluginTitleCuringCost;


            // Headers

            lsHeaderRecovery = Resources.Combat.RecoveryPluginHeaderRecovery;
            lsHeaderCuring = Resources.Combat.RecoveryPluginHeaderCuring;
            lsHeaderAvgCuring = Resources.Combat.RecoveryPluginHeaderAvgCuring;
            lsHeaderStatusCuring = Resources.Combat.RecoveryPluginHeaderStatusCuring;
            lsHeaderCuringCost = Resources.Combat.RecoveryPluginHeaderCuringCost;

            // Formatters

            lsFormatRecovery = Resources.Combat.RecoveryPluginFormatRecovery;
            lsFormatCuring = Resources.Combat.RecoveryPluginFormatCuring;
            lsFormatAvgCuring = Resources.Combat.RecoveryPluginFormatAvgCuring;
            lsFormatStatusCuring = Resources.Combat.RecoveryPluginFormatStatusCures;
            lsFormatStatusCuringSub = Resources.Combat.RecoveryPluginFormatStatusCuresSub;
            lsFormatCuringCost = Resources.Combat.RecoveryPluginFormatCuringCost;

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
            lsCura = Resources.ParsedStrings.Cura;
            lsCuraga1 = Resources.ParsedStrings.Curaga1;
            lsCuraga2 = Resources.ParsedStrings.Curaga2;
            lsCuraga3 = Resources.ParsedStrings.Curaga3;
            lsCuraga4 = Resources.ParsedStrings.Curaga4;

            lsCuragaRegex = new Regex(lsCuragaREString);
        }
        #endregion
    }
}