using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using WaywardGamers.KParser;
using System.Diagnostics;

namespace WaywardGamers.KParser.Plugin
{
    public class RecoveryPlugin : BasePluginControlWithDropdown
    {
        #region IPlugin Overrides
        public override string TabName
        {
            get { return "Recovery"; }
        }

        public override void Reset()
        {
            richTextBox.Clear();
            richTextBox.WordWrap = false;

            label1.Text = "Category";
            comboBox1.Left = label1.Right + 10;
            comboBox1.Items.Clear();
            comboBox1.Items.Add("All");
            comboBox1.Items.Add("Recovery");
            comboBox1.Items.Add("Curing");
            comboBox1.Items.Add("Average Curing");
            comboBox1.SelectedIndex = 0;

            label2.Visible = false;
            label2.Enabled = false;
            comboBox2.Visible = false;
            comboBox2.Enabled = false;

            checkBox1.Enabled = false;
            checkBox1.Visible = false;
            checkBox2.Enabled = false;
            checkBox2.Visible = false;
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
        {
            if (e.DatasetChanges.Interactions.Count != 0)
            {
                datasetToUse = e.Dataset;
                return true;
            }

            datasetToUse = null;
            return false;
        }
        #endregion

        #region Member Variables
        Dictionary<string, int> playerDamage = new Dictionary<string, int>();

        string dmgRecoveryHeader = "Player           Dmg Taken   HP Drained   HP Cured   #Regen   #Regen 2   #Regen 3\n";
        string cureHeader        = "Player           Cured (Sp)  Cured (Ab)  C.1s  C.2s  C.3s  C.4s  C.5s  Curagas  Rg.1s  Rg.2s  Rg.3s\n";
        string avgCureHeader     = "Player           Avg Cure 1   Avg Cure 2   Avg Cure 3   Avg Cure 4   Avg Cure 5   Avg Curaga   Avg Ability\n";
        #endregion

        #region Processing sections
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            richTextBox.Clear();

            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    // All
                    ProcessDamage(dataSet);
                    ProcessCuring(dataSet, true, true);
                    break;
                case 1:
                    // Recovery
                    ProcessDamage(dataSet);
                    break;
                case 2:
                    // Curing
                    ProcessCuring(dataSet, true, false);
                    break;
                case 3:
                    // AverageCuring
                    ProcessCuring(dataSet, false, true);
                    break;
            }
        }
        #endregion

        #region Recovery
        private void ProcessDamage(KPDatabaseDataSet dataSet)
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
                                                        (dm.HarmType == (byte)HarmType.Drain))
                                                 select dm.Amount,
                                 SecondDmgTaken = from dm in c.GetInteractionsRowsByTargetCombatantRelation()
                                                  where ((dm.SecondHarmType == (byte)HarmType.Damage) ||
                                                         (dm.SecondHarmType == (byte)HarmType.Drain))
                                                  select dm.SecondAmount,
                                 PrimeDrain = from dr in c.GetInteractionsRowsByActorCombatantRelation()
                                              where (dr.HarmType == (byte)HarmType.Drain)
                                              select dr.Amount,
                                 SecondDrain = from dr in c.GetInteractionsRowsByActorCombatantRelation()
                                               where ((dr.SecondHarmType == (byte)HarmType.Drain) ||
                                                      (dr.SecondAidType == (byte)AidType.Recovery))
                                               select dr.SecondAmount,
                                 Cured = from cr in c.GetInteractionsRowsByTargetCombatantRelation()
                                         where ((cr.AidType == (byte)AidType.Recovery) &&
                                                (cr.RecoveryType == (byte)RecoveryType.RecoverHP))
                                         select cr.Amount,
                                 Regen1 = from cr in c.GetInteractionsRowsByTargetCombatantRelation()
                                          where ((cr.AidType == (byte)AidType.Enhance) &&
                                          (cr.IsActionIDNull() == false) &&
                                          (cr.ActionsRow.ActionName == "Regen"))
                                          select cr,
                                 Regen2 = from cr in c.GetInteractionsRowsByTargetCombatantRelation()
                                          where ((cr.AidType == (byte)AidType.Enhance) &&
                                          (cr.IsActionIDNull() == false) &&
                                          (cr.ActionsRow.ActionName == "Regen II"))
                                          select cr,
                                 Regen3 = from cr in c.GetInteractionsRowsByTargetCombatantRelation()
                                          where ((cr.AidType == (byte)AidType.Enhance) &&
                                          (cr.IsActionIDNull() == false) &&
                                          (cr.ActionsRow.ActionName == "Regen III"))
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
                            AppendBoldText("Damage Recovery\n", Color.Blue);
                            AppendBoldUnderText(dmgRecoveryHeader, Color.Black);

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
                    AppendNormalText(sb.ToString());
                    string totalString = string.Format(
                        "{0,-17}{1,9}{2,13}{3,11}{4,9}{5,11}{6,11}\n\n\n", "Total",
                        ttlDmgTaken, ttlDrainAmt, ttlHealAmt, ttlNumR1, ttlNumR2, ttlNumR3);
                    AppendBoldText(totalString, Color.Black);
                }

            }
        }

        private void ProcessCuring(KPDatabaseDataSet dataSet, bool displayCures, bool displayAvgCures)
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
                                                   (cr.ActionsRow.ActionName == "Healing Breath")))
                                           select cr.Amount,
                                  Cure2s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                           where ((cr.AidType == (byte)AidType.Recovery) &&
                                                  (cr.IsActionIDNull() == false) &&
                                                  ((cr.ActionsRow.ActionName == "Cure II") ||
                                                   (cr.ActionsRow.ActionName == "Curing Waltz") ||
                                                   (cr.ActionsRow.ActionName == "Healing Breath II")))
                                           select cr.Amount,
                                  Cure3s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                           where ((cr.AidType == (byte)AidType.Recovery) &&
                                                  (cr.IsActionIDNull() == false) &&
                                                  ((cr.ActionsRow.ActionName == "Cure III") ||
                                                   (cr.ActionsRow.ActionName == "Curing Waltz II") ||
                                                   (cr.ActionsRow.ActionName == "Wild Carrot") ||
                                                   (cr.ActionsRow.ActionName == "Healing Breath III")))
                                           select cr.Amount,
                                  Cure4s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                           where ((cr.AidType == (byte)AidType.Recovery) &&
                                                  (cr.IsActionIDNull() == false) &&
                                                  ((cr.ActionsRow.ActionName == "Cure IV") ||
                                                   (cr.ActionsRow.ActionName == "Curing Waltz III") ||
                                                   (cr.ActionsRow.ActionName == "Magic Fruit")))
                                           select cr.Amount,
                                  Cure5s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                           where ((cr.AidType == (byte)AidType.Recovery) &&
                                                  (cr.IsActionIDNull() == false) &&
                                                  (cr.ActionsRow.ActionName == "Cure V"))
                                           select cr.Amount,
                                  Curagas = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                            where ((cr.AidType == (byte)AidType.Recovery) &&
                                                   (cr.IsActionIDNull() == false) &&
                                                   ((cr.ActionsRow.ActionName.StartsWith("Curaga")) ||
                                                    (cr.ActionsRow.ActionName == "Healing Breeze") ||
                                                    (cr.ActionsRow.ActionName == "Divine Waltz")))
                                            group cr by cr.Timestamp into crt
                                            select crt,
                                  OtherCures = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                               where ((cr.AidType == (byte)AidType.Recovery) &&
                                                      (cr.IsActionIDNull() == false) &&
                                                      (cr.ActionsRow.ActionName == "Chakra"))
                                               select cr.Amount,
                                  Reg1s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                          where ((cr.AidType == (byte)AidType.Enhance) &&
                                                 (cr.IsActionIDNull() == false) &&
                                                 (cr.ActionsRow.ActionName == "Regen"))
                                          select cr,
                                  Reg2s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                          where ((cr.AidType == (byte)AidType.Enhance) &&
                                                 (cr.IsActionIDNull() == false) &&
                                                 (cr.ActionsRow.ActionName == "Regen II"))
                                          select cr,
                                  Reg3s = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                          where ((cr.AidType == (byte)AidType.Enhance) &&
                                                 (cr.IsActionIDNull() == false) &&
                                                 (cr.ActionsRow.ActionName == "Regen III"))
                                          select cr,
                                  Spells = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                           where ((cr.ActionType == (byte)ActionType.Spell) &&
                                                  (cr.AidType == (byte)AidType.Recovery) &&
                                                  (cr.RecoveryType == (byte)RecoveryType.RecoverHP))
                                           select cr.Amount,
                                  Ability = from cr in c.GetInteractionsRowsByActorCombatantRelation()
                                            where ((cr.ActionType == (byte)ActionType.Ability) &&
                                                   (cr.AidType == (byte)AidType.Recovery) &&
                                                   (cr.RecoveryType == (byte)RecoveryType.RecoverHP))
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
                                AppendBoldText("Curing (Whm spells or equivalent)\n", Color.Blue);
                                AppendBoldUnderText(cureHeader, Color.Black);

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
                        AppendNormalText(sb.ToString());
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
                                AppendBoldText("Average Curing (Whm spells or equivalent)\n", Color.Blue);
                                AppendBoldUnderText(avgCureHeader, Color.Black);

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
                        AppendNormalText(sb.ToString());
                    }
                }
            }
        }
        #endregion

        #region Event Handlers
        protected override void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            HandleDataset(DatabaseManager.Instance.Database);
        }

        protected override void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            HandleDataset(DatabaseManager.Instance.Database);
        }

        protected override void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            HandleDataset(DatabaseManager.Instance.Database);
        }

        protected override void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            HandleDataset(DatabaseManager.Instance.Database);
        }
        #endregion

    }
}