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
    public class DefensePlugin : BasePluginControlWithDropdown
    {
        #region IPlugin Overrides
        public override string TabName
        {
            get { return "Defense"; }
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
            comboBox1.Items.Add("Defense");
            comboBox1.Items.Add("Utsusemi");
            comboBox1.SelectedIndex = 0;

            label2.Left = comboBox1.Right + 20;
            label2.Text = "Mob Group";
            comboBox2.Left = label2.Right + 10;
            comboBox2.Items.Clear();
            comboBox2.Items.Add("All");
            comboBox2.SelectedIndex = 0;
            comboBox2.Enabled = false;

            checkBox1.Enabled = false;
            checkBox1.Visible = false;
            checkBox2.Enabled = false;
            checkBox2.Visible = false;
        }

        public override void DatabaseOpened(KPDatabaseDataSet dataSet)
        {
            if (dataSet.Battles.Count > 1)
            {
                var mobsKilled = from b in dataSet.Battles
                                 where ((b.DefaultBattle == false) &&
                                        (b.IsEnemyIDNull() == false))
                                 orderby b.CombatantsRowByEnemyCombatantRelation.CombatantName
                                 group b by b.CombatantsRowByEnemyCombatantRelation.CombatantName into bn
                                 select new
                                 {
                                     Name = bn.Key,
                                     XP = from xb in bn
                                          group xb by xb.BaseExperience() into xbn
                                          select new { BaseXP = xbn.Key }
                                 };

                if (mobsKilled.Count() > 0)
                {
                    comboBox2.Items.Clear();
                    AddToComboBox2("All");

                    string mobWithXP;

                    foreach (var mob in mobsKilled)
                    {
                        AddToComboBox2(mob.Name);

                        if (mob.XP.Count() > 1)
                        {
                            foreach (var xp in mob.XP)
                            {
                                mobWithXP = string.Format("{0} ({1})", mob.Name, xp.BaseXP);

                                AddToComboBox2(mobWithXP);
                            }
                        }
                    }
                }
            }

            base.DatabaseOpened(dataSet);
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
        {
            // Check for new mobs being fought.  If any exist, update the Mob Group dropdown list.
            if (e.DatasetChanges.Battles.Count > 0)
            {
                var mobsFought = from b in e.DatasetChanges.Battles
                                 where ((b.DefaultBattle == false) &&
                                        (b.IsEnemyIDNull() == false))
                                 group b by b.CombatantsRowByEnemyCombatantRelation.CombatantName into bn
                                 select new
                                 {
                                     Name = bn.Key,
                                     XP = from xb in bn
                                          group xb by xb.BaseExperience() into xbn
                                          select new { BaseXP = xbn.Key }
                                 };


                if (mobsFought.Count() > 0)
                {
                    string mobWithXP;

                    foreach (var mob in mobsFought)
                    {
                        if (comboBox2.Items.Contains(mob.Name) == false)
                            AddToComboBox2(mob.Name);

                        foreach (var xp in mob.XP)
                        {
                            if (xp.BaseXP > 0)
                            {
                                mobWithXP = string.Format("{0} ({1})", mob.Name, xp.BaseXP);

                                if (comboBox2.Items.Contains(mobWithXP) == false)
                                    AddToComboBox2(mobWithXP);
                            }
                        }
                    }
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

        #region Member Variables
        //int totalDamage;
        Dictionary<string, int> playerDamage = new Dictionary<string, int>();

        string dmgRecoveryHeader = "Player           Dmg Taken   HP Drained   HP Cured   #Regen   #Regen 2   #Regen 3\n";
        string cureHeader        = "Player           Cured (Sp)  Cured (Ab)  C.1s  C.2s  C.3s  C.4s  C.5s  Curagas  Rg.1s  Rg.2s  Rg.3s\n";
        string avgCureHeader     = "Player           Avg Cure 1   Avg Cure 2   Avg Cure 3   Avg Cure 4   Avg Cure 5   Avg Curaga   Avg Ability\n";

        string incAttacksHeader  = "Player           Melee   Range   Abil/Ws   Spells   Avoided   Avoid %   Attack# %\n";
        string incDamageHeader   = "Player           M.Dmg   Avg M.Dmg   R.Dmg  Avg R.Dmg   S.Dmg  Avg S.Dmg   A/WS.Dmg  Avg A/WS.Dmg   Damage %\n";
        string evasionHeader     = "Player           M.Evade   M.Evade %   R.Evade   R.Evade %\n";
        string otherDefHeader    = "Player           Parry   Parry %   Blink   Blink %   Anticipate  Anticipate %   Counter   Counter %\n";
        
        string utsuHeader        = "Player           Shadows Used   Ichi Cast  Ichi Fin  Ni Cast  Ni Fin  Shadows  Shadows(N)  Efficiency  Efficiency(N)\n";
        #endregion

        #region Processing sections
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            richTextBox.Clear();

            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    // All
                    ProcessRecovery(dataSet);
                    ProcessDefense(dataSet);
                    ProcessUtsusemi(dataSet);
                    break;
                case 1:
                    // Recovery
                    ProcessRecovery(dataSet);
                    break;
                case 2:
                    // Defense
                    ProcessDefense(dataSet);
                    break;
                case 3:
                    // Utsusemi
                    ProcessUtsusemi(dataSet);
                    break;
            }
        }

        #region Recovery
        private void ProcessRecovery(KPDatabaseDataSet dataSet)
        {
            AppendBoldText("Recovery\n\n", Color.Red);
            ProcessRecoveryDamage(dataSet);
            ProcessRecoveryCuring(dataSet);
            AppendNormalText("\n");
        }

        private void ProcessRecoveryDamage(KPDatabaseDataSet dataSet)
        {
            var playerData = from c in dataSet.Combatants
                             where ((c.CombatantType == (byte)EntityType.Player) ||
                                    (c.CombatantType == (byte)EntityType.Pet) ||
                                    (c.CombatantType == (byte)EntityType.Fellow))
                             orderby c.CombatantName
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
            StringBuilder sb = new StringBuilder();

            if (playerData.Count() > 0)
            {
                AppendBoldText("Damage Recovery\n", Color.Blue);
                AppendBoldUnderText(dmgRecoveryHeader, Color.Black);

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

                sb.Append("\n\n");
                AppendNormalText(sb.ToString());

            }
        }

        private void ProcessRecoveryCuring(KPDatabaseDataSet dataSet)
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

            StringBuilder sb = new StringBuilder();
            bool haveHealer = false;

            if (uberHealing.Count() > 0)
            {
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
                        if (haveHealer == false)
                        {
                            AppendBoldText("Curing\n", Color.Blue);
                            AppendBoldUnderText(cureHeader, Color.Black);

                            haveHealer = true;
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

                if (haveHealer == true)
                {
                    sb.Append("\n\n");
                    AppendNormalText(sb.ToString());
                    sb = new StringBuilder();
                }


                if (haveHealer == true)
                {
                    AppendBoldText("Average Curing (whm spells or equivalent)\n", Color.Blue);
                    AppendBoldUnderText(avgCureHeader, Color.Black);

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

                    sb.Append("\n\n");
                    AppendNormalText(sb.ToString());
                }
            }
        }
        #endregion

        #region Defense
        private void ProcessDefense(KPDatabaseDataSet dataSet)
        {
            IEnumerable<DefenseGroup> incAttacks;

            incAttacks = from cd in dataSet.Interactions
                         where ((cd.IsTargetIDNull() == false) &&
                                ((cd.CombatantsRowByTargetCombatantRelation.CombatantType == (byte)EntityType.Player) ||
                                 (cd.CombatantsRowByTargetCombatantRelation.CombatantType == (byte)EntityType.Pet) ||
                                 (cd.CombatantsRowByTargetCombatantRelation.CombatantType == (byte)EntityType.Fellow)) &&
                                ((cd.HarmType == (byte)HarmType.Damage) ||
                                 (cd.HarmType == (byte)HarmType.Drain)))
                         group cd by cd.CombatantsRowByTargetCombatantRelation.CombatantName into cdd
                         orderby cdd.Key
                         select new DefenseGroup
                         {
                             Player = cdd.Key,
                             AllAttacks = from pd in cdd
                                          select pd,
                             Melee = from pd in cdd
                                     where (pd.ActionType == (byte)ActionType.Melee)
                                     select pd,
                             Range = from pd in cdd
                                     where (pd.ActionType == (byte)ActionType.Ranged)
                                     select pd,
                             Spell = from pd in cdd
                                     where (pd.ActionType == (byte)ActionType.Spell)
                                     select pd,
                             Abil = from pd in cdd
                                    where ((pd.ActionType == (byte)ActionType.Ability) ||
                                            (pd.ActionType == (byte)ActionType.Weaponskill))
                                    select pd
                         };

            if ((incAttacks != null) && (incAttacks.Count() > 0))
            {
                AppendBoldText("Defense\n\n", Color.Red);

                ProcessDefenseAttacks(incAttacks);
                ProcessDefenseDamage(incAttacks);
                ProcessDefenseEvasion(incAttacks);
                ProcessDefenseOther(incAttacks);

                AppendNormalText("\n");
            }
        }

        private void ProcessDefenseAttacks(IEnumerable<DefenseGroup> incAttacks)
        {
            AppendBoldText("Incoming Attacks\n", Color.Blue);
            AppendBoldUnderText(incAttacksHeader, Color.Black);

            StringBuilder sb = new StringBuilder();

            //"Player           Melee   Range   Abil/Ws   Spells   Avoided   Avoid %   Attack# %"

            int totalAttacks = incAttacks.Sum(b =>
                b.Melee.Count() + b.Range.Count() + b.Abil.Count() + b.Spell.Count());

            foreach (var player in incAttacks)
            {
                sb.Append(player.Player.PadRight(16));
                sb.Append(" ");

                int mHits = 0;
                int rHits = 0;
                int sHits = 0;
                int aHits = 0;
                int incHits = 0;
                int avoidHits = 0;

                double avoidPerc = 0;
                double attackPerc = 0;

                if (player.Melee != null)
                    mHits = player.Melee.Count();
                if (player.Range != null)
                    rHits = player.Range.Count();
                if (player.Abil != null)
                    aHits = player.Abil.Count();
                if (player.Spell != null)
                    sHits = player.Spell.Count();

                incHits = mHits + rHits + aHits + sHits;

                avoidHits = player.AllAttacks.Count(h => h.DefenseType != (byte)DefenseType.None);

                avoidPerc = (double)avoidHits / incHits;

                attackPerc = (double)incHits / totalAttacks;


                sb.Append(mHits.ToString().PadLeft(5));
                sb.Append(rHits.ToString().PadLeft(8));
                sb.Append(aHits.ToString().PadLeft(10));
                sb.Append(sHits.ToString().PadLeft(9));
                sb.Append(avoidHits.ToString().PadLeft(10));
                sb.Append(avoidPerc.ToString("P2").PadLeft(10));
                sb.Append(attackPerc.ToString("P2").PadLeft(12));

                sb.Append("\n");
            }

            sb.Append("\n\n");
            AppendNormalText(sb.ToString());
        }

        private void ProcessDefenseDamage(IEnumerable<DefenseGroup> incAttacks)
        {
            //Player           M.Dmg   Avg M.Dmg   R.Dmg  Avg R.Dmg   S.Dmg  Avg S.Dmg   A/WS.Dmg  Avg A/WS.Dmg   Damage %

            int totalDmg = 0;
            playerDamage.Clear();
            foreach (var player in incAttacks)
            {
                playerDamage[player.Player] = player.Melee.Concat(player.Range.Concat(player.Spell.Concat(player.Abil))).
                    Sum(a => a.Amount);

                totalDmg += playerDamage[player.Player];
            }

            if (totalDmg > 0)
            {
                AppendBoldText("Incoming Damage\n", Color.Blue);
                AppendBoldUnderText(incDamageHeader, Color.Black);

                StringBuilder sb = new StringBuilder();

                foreach (var player in incAttacks)
                {
                    if (playerDamage[player.Player] > 0)
                    {
                        sb.Append(player.Player.PadRight(16));
                        sb.Append(" ");

                        int mDmg = 0;
                        double mAvg = 0;
                        int rDmg = 0;
                        double rAvg = 0;
                        int sDmg = 0;
                        double sAvg = 0;
                        int aDmg = 0;
                        double aAvg = 0;

                        int numHits;

                        if (player.Melee.Count() > 0)
                        {
                            mDmg = player.Melee.Sum(a => a.Amount);
                            numHits = player.Melee.Count(a => a.DefenseType == (byte)DefenseType.None);
                            if (numHits > 0)
                                mAvg = (double)mDmg / numHits;
                        }

                        if (player.Range.Count() > 0)
                        {
                            rDmg = player.Range.Sum(a => a.Amount);
                            numHits = player.Range.Count(a => a.DefenseType == (byte)DefenseType.None);
                            if (numHits > 0)
                                rAvg = (double)rDmg / numHits;
                        }

                        if (player.Spell.Count() > 0)
                        {
                            sDmg = player.Spell.Sum(a => a.Amount);
                            numHits = player.Spell.Count(a => a.DefenseType == (byte)DefenseType.None);
                            if (numHits > 0)
                                sAvg = (double)sDmg / numHits;
                        }

                        if (player.Abil.Count() > 0)
                        {
                            aDmg = player.Abil.Sum(a => a.Amount);
                            numHits = player.Abil.Count(a => a.DefenseType == (byte)DefenseType.None);
                            if (numHits > 0)
                                aAvg = (double)aDmg / numHits;
                        }

                        double dmgPerc = 0;
                        if (totalDmg > 0)
                            dmgPerc = (double)playerDamage[player.Player] / totalDmg;


                        sb.Append(mDmg.ToString().PadLeft(5));
                        sb.Append(mAvg.ToString("F2").PadLeft(12));
                        sb.Append(rDmg.ToString().PadLeft(8));
                        sb.Append(rAvg.ToString("F2").PadLeft(11));
                        sb.Append(sDmg.ToString().PadLeft(8));
                        sb.Append(sAvg.ToString("F2").PadLeft(11));
                        sb.Append(aDmg.ToString().PadLeft(11));
                        sb.Append(aAvg.ToString("F2").PadLeft(14));
                        sb.Append(dmgPerc.ToString("P2").PadLeft(11));

                        sb.Append("\n");
                    }
                }

                sb.Append("\n\n");
                AppendNormalText(sb.ToString());
            }
        }

        private void ProcessDefenseEvasion(IEnumerable<DefenseGroup> incAttacks)
        {
            bool headerPrinted = false;

            //Player           M.Evade   M.Evade %   R.Evade   R.Evade %

            StringBuilder sb = new StringBuilder();

            foreach (var player in incAttacks)
            {
                if ((player.Melee.Count() + player.Range.Count()) > 0)
                {
                    int mEvaded = 0;
                    double mEvadePerc = 0;
                    int rEvaded = 0;
                    double rEvadePerc = 0;

                    if (player.Melee.Count() > 0)
                    {
                        mEvaded = player.Melee.Count(h => h.DefenseType == (byte)DefenseType.Evasion);
                        mEvadePerc = (double)mEvaded / player.Melee.Count();
                    }

                    if (player.Range.Count() > 0)
                    {
                        rEvaded = player.Range.Count(h => h.DefenseType == (byte)DefenseType.Evasion);
                        rEvadePerc = (double)rEvaded / player.Range.Count();
                    }

                    if ((mEvaded + rEvaded) > 0)
                    {
                        if (headerPrinted == false)
                        {
                            AppendBoldText("Evasion\n", Color.Blue);
                            AppendBoldUnderText(evasionHeader, Color.Black);

                            headerPrinted = true;
                        }

                        sb.Append(player.Player.PadRight(16));
                        sb.Append(" ");

                        sb.Append(mEvaded.ToString().PadLeft(7));
                        sb.Append(mEvadePerc.ToString("P2").PadLeft(12));
                        sb.Append(rEvaded.ToString().PadLeft(10));
                        sb.Append(rEvadePerc.ToString("P2").PadLeft(12));

                        sb.Append("\n");
                    }
                }
            }

            if (headerPrinted == true)
            {
                sb.Append("\n\n");
                AppendNormalText(sb.ToString());
            }
        }

        private void ProcessDefenseOther(IEnumerable<DefenseGroup> incAttacks)
        {
            bool headerPrinted = false;

            //Player           Parry   Parry %   Blink   Blink %   Anticipate  Anticipate %   Counter   Counter %

            StringBuilder sb = new StringBuilder();

            foreach (var player in incAttacks)
            {
                var parryableAttacks = player.Melee.Where(a =>
                    a.DefenseType != (byte) DefenseType.Evasion);

                var blinkableAttacks = player.Melee.Concat(player.Range.Concat(player.Spell.Concat(player.Abil))).
                    Where(a =>
                        a.DefenseType != (byte)DefenseType.Evasion &&
                        a.DefenseType != (byte)DefenseType.Parry);

                var anticableAttacks = player.Melee.Concat(player.Abil).Where(a =>
                    a.DefenseType != (byte)DefenseType.Evasion &&
                    a.DefenseType != (byte)DefenseType.Parry &&
                    a.DefenseType != (byte)DefenseType.Blink);

                var counterableAttacks = player.Melee.Where(a =>
                    a.DefenseType != (byte)DefenseType.Evasion &&
                    a.DefenseType != (byte)DefenseType.Parry &&
                    a.DefenseType != (byte)DefenseType.Blink &&
                    a.DefenseType != (byte)DefenseType.Anticipate);


                int parryableCount = parryableAttacks.Count();
                int blinkableCount = blinkableAttacks.Count();
                int anticibleCount = anticableAttacks.Count();
                int counterableCount = counterableAttacks.Count();

                int parriedAttacks = 0;
                int blinkedAttacks = 0;
                int anticipatedAttacks = 0;
                int counteredAttacks = 0;

                double parryPerc = 0;
                double blinkPerc = 0;
                double antiPerc = 0;
                double counterPerc = 0;


                if ((parryableCount + blinkableCount + anticibleCount + counterableCount) > 0)
                {
                    if (parryableCount > 0)
                    {
                        parriedAttacks = parryableAttacks.Count(a => a.DefenseType == (byte)DefenseType.Parry);
                        parryPerc = (double)parriedAttacks / parryableCount;
                    }

                    if (blinkableCount > 0)
                    {
                        blinkedAttacks = blinkableAttacks.Count(a => a.DefenseType == (byte)DefenseType.Blink);
                        blinkPerc = (double)blinkedAttacks / blinkableCount;
                    }

                    if (anticibleCount > 0)
                    {
                        anticipatedAttacks = anticableAttacks.Count(a => a.DefenseType == (byte)DefenseType.Anticipate);
                        antiPerc = (double)anticipatedAttacks / anticibleCount;
                    }

                    if (counterableCount > 0)
                    {
                        counteredAttacks = counterableAttacks.Count(a => a.DefenseType == (byte)DefenseType.Counter);
                        counterPerc = (double)counteredAttacks / counterableAttacks.Count();
                    }


                    if ((parriedAttacks + blinkedAttacks + anticipatedAttacks + counteredAttacks) > 0)
                    {
                        if (headerPrinted == false)
                        {
                            AppendBoldText("Other Defenses\n", Color.Blue);
                            AppendBoldUnderText(otherDefHeader, Color.Black);
                            headerPrinted = true;
                        }

                        sb.Append(player.Player.PadRight(16));
                        sb.Append(" ");

                        sb.Append(parriedAttacks.ToString().PadLeft(5));
                        sb.Append(parryPerc.ToString("P2").PadLeft(10));
                        sb.Append(blinkedAttacks.ToString().PadLeft(8));
                        sb.Append(blinkPerc.ToString("P2").PadLeft(10));
                        sb.Append(anticipatedAttacks.ToString().PadLeft(13));
                        sb.Append(antiPerc.ToString("P2").PadLeft(14));
                        sb.Append(counteredAttacks.ToString().PadLeft(10));
                        sb.Append(counterPerc.ToString("P2").PadLeft(12));

                        sb.Append("\n");
                    }
                }
            }

            if (headerPrinted == true)
            {
                sb.Append("\n\n");
                AppendNormalText(sb.ToString());
            }
        }
        #endregion

        #region Utsu
        private void ProcessUtsusemi(KPDatabaseDataSet dataSet)
        {
            var utsu1 = dataSet.Actions.FirstOrDefault(a => a.ActionName == "Utsusemi: Ichi");
            var utsu2 = dataSet.Actions.FirstOrDefault(a => a.ActionName == "Utsusemi: Ni");

            var utsuByPlayer = from c in dataSet.Combatants
                               where c.CombatantType == (byte)EntityType.Player
                               orderby c.CombatantName
                               select new
                               {
                                   Player = c.CombatantName,
                                   ShadowsUsed = from uc in c.GetInteractionsRowsByTargetCombatantRelation()
                                                 where ((uc.DefenseType == (byte)DefenseType.Blink) &&
                                                        (uc.ShadowsUsed > 0))
                                                 select uc,
                                   UtsuIchi = from i in dataSet.Interactions
                                              where ((utsu1 != null) &&
                                                     (i.ActionsRow == utsu1) &&
                                                     (i.CombatantsRowByActorCombatantRelation == c))
                                              select i,
                                   UtsuNi = from i in dataSet.Interactions
                                            where ((utsu2 != null) &&
                                                   (i.ActionsRow == utsu2) &&
                                                   (i.CombatantsRowByActorCombatantRelation == c))
                                            select i,
                               };


            int shadsUsed = 0;
            int ichiCast = 0;
            int niCast = 0;
            int ichiFin = 0;
            int niFin = 0;
            int numShads = 0;
            int numShadsN = 0;
            double effNorm = 0;
            double effNin = 0;


            if (utsuByPlayer.Count() > 0)
            {
                AppendBoldText("Utsusemi\n\n", Color.Red);
                AppendBoldUnderText(utsuHeader, Color.Black);

                StringBuilder sb = new StringBuilder();

                foreach (var player in utsuByPlayer)
                {
                    shadsUsed = 0;
                    ichiCast = 0;
                    niCast = 0;
                    ichiFin = 0;
                    niFin = 0;
                    numShads = 0;
                    numShadsN = 0;
                    effNorm = 0;
                    effNin = 0;

                    shadsUsed = player.ShadowsUsed.Sum(u => u.ShadowsUsed);
                    ichiCast = player.UtsuIchi.Count(u => u.Preparing == true);
                    niCast = player.UtsuNi.Count(u => u.Preparing == true);
                    ichiFin = player.UtsuIchi.Count(u => u.Preparing == false);
                    niFin = player.UtsuNi.Count(u => u.Preparing == false);

                    numShads = ichiFin * 3 + niFin * 3;
                    numShadsN = ichiFin * 3 + niFin * 4;

                    if (numShads > 0)
                    {
                        effNorm = (double)shadsUsed / numShads;
                        effNin = (double)shadsUsed / numShadsN;
                    }

                    if ((numShads + shadsUsed + ichiCast + niCast) > 0)
                    {
                        sb.Append(player.Player.PadRight(16));
                        sb.Append(" ");

                        sb.Append(shadsUsed.ToString().PadLeft(12));
                        sb.Append(ichiCast.ToString().PadLeft(12));
                        sb.Append(ichiFin.ToString().PadLeft(10));
                        sb.Append(niCast.ToString().PadLeft(9));
                        sb.Append(niFin.ToString().PadLeft(8));
                        sb.Append(numShads.ToString().PadLeft(9));
                        sb.Append(numShadsN.ToString().PadLeft(11));
                        sb.Append(effNorm.ToString("P2").PadLeft(13));
                        sb.Append(effNin.ToString("P2").PadLeft(14));

                        sb.Append("\n");
                    }
                }

                sb.Append("\n");
                AppendNormalText(sb.ToString());
            }
        }
        #endregion
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