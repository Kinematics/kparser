using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using WaywardGamers.KParser;

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
            if (dataSet.Battles.Count() > 1)
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
                                          select new { BXP = xbn.Key }
                                 };

                foreach (var mob in mobsKilled)
                {
                    if (this.comboBox2.Items.Contains(mob.Name) == false)
                    {
                        AddToComboBox2(mob.Name);
                    }

                    if (mob.XP.Count() > 1)
                    {
                        string mobWithXP;

                        foreach (var xp in mob.XP)
                        {
                            mobWithXP = string.Format("{0} ({1})", mob.Name, xp.BXP);

                            if (this.comboBox2.Items.Contains(mobWithXP) == false)
                            {
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
            if (e.DatasetChanges.Battles.Count != 0)
            {
                // Check for new kills.  If any exist, update the Mob Group dropdown list.
                int allBattles = e.DatasetChanges.Battles.Count(b => b.DefaultBattle == false);

                if (allBattles != 0)
                {
                    var mobsKilled = from b in e.FullDataset.Battles
                                     where b.Killed == true
                                     orderby b.CombatantsRowByEnemyCombatantRelation.CombatantName
                                     group b by b.CombatantsRowByEnemyCombatantRelation.CombatantName into bn
                                     select new
                                     {
                                         Name = bn.Key,
                                         XP = from xb in bn
                                              group xb by xb.BaseExperience() into xbn
                                              select new { BXP = xbn.Key }
                                     };

                    foreach (var mob in mobsKilled)
                    {
                        if (this.comboBox2.Items.Contains(mob.Name) == false)
                        {
                            AddToComboBox2(mob.Name);
                        }

                        if (mob.XP.Count() > 1)
                        {
                            string mobWithXP;

                            foreach (var xp in mob.XP)
                            {
                                mobWithXP = string.Format("{0} ({1})", mob.Name, xp.BXP);

                                if (this.comboBox2.Items.Contains(mobWithXP) == false)
                                {
                                    AddToComboBox2(mobWithXP);
                                }
                            }
                        }
                    }
                }
            }

            if (e.DatasetChanges.Interactions.Count != 0)
            {
                datasetToUse = e.FullDataset;
                return true;
            }

            datasetToUse = null;
            return false;
        }

        private void AddToComboBox2(string p)
        {
            if (this.InvokeRequired)
            {
                Action<string> thisFunc = AddToComboBox2;
                Invoke(thisFunc, new object[] { p });
                return;
            }

            this.comboBox2.Items.Add(p);
        }
        #endregion

        #region Member Variables
        //int totalDamage;
        Dictionary<string, int> playerDamage = new Dictionary<string, int>();

        string dmgRecoveryHeader = "Player           Dmg Taken   HP Drained   HP Cured   #Regen   #Regen 2   #Regen 3\n";
        string cureHeader        = "Player           Cured (Sp)  Cured (Ab)  C.1s  C.2s  C.3s  C.4s  C.5s  Curagas  Rg.1s  Rg.2s  Rg.3s\n";
        string avgCureHeader     = "Player           Avg Cure 1   Avg Cure 2   Avg Cure 3   Avg Cure 4   Avg Cure 5   Avg Ability\n";

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
                    ProcessDefenseUtsusemi(dataSet);
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
                    ProcessDefenseUtsusemi(dataSet);
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
            playerDamage.Clear();

            var dmgTaken = from cd in dataSet.Interactions
                           where ((cd.IsTargetIDNull() == false) &&
                                  ((cd.CombatantsRowByTargetCombatantRelation.CombatantType == (byte)EntityType.Player) ||
                                   (cd.CombatantsRowByTargetCombatantRelation.CombatantType == (byte)EntityType.Pet) ||
                                   (cd.CombatantsRowByTargetCombatantRelation.CombatantType == (byte)EntityType.Fellow)) &&
                                  ((cd.HarmType == (byte)HarmType.Damage) ||
                                   (cd.HarmType == (byte)HarmType.Drain) ||
                                   (cd.SecondHarmType == (byte)HarmType.Damage) ||
                                   (cd.SecondHarmType == (byte)HarmType.Drain)))
                           group cd by cd.CombatantsRowByTargetCombatantRelation.CombatantName into cdd
                           orderby cdd.Key
                           select new
                           {
                               Player = cdd.Key,
                               PrimaryDamage = from pd in cdd
                                               where ((pd.HarmType == (byte)HarmType.Damage) ||
                                                      (pd.HarmType == (byte)HarmType.Drain))
                                               select pd,
                               SecondaryDamage = from pd in cdd
                                                 where ((pd.SecondHarmType == (byte)HarmType.Damage) ||
                                                        (pd.SecondHarmType == (byte)HarmType.Drain))
                                                 select pd,
                           };

            var hpDrained = from cd in dataSet.Interactions
                            where ((cd.IsActorIDNull() == false) &&
                                  ((cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Player) ||
                                   (cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Pet) ||
                                   (cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Fellow)) &&
                                  ((cd.HarmType == (byte)HarmType.Drain) ||
                                   (cd.SecondHarmType == (byte)HarmType.Drain) ||
                                   (cd.SecondAidType == (byte)AidType.Recovery)))
                            group cd by cd.CombatantsRowByActorCombatantRelation.CombatantName into cdd
                            orderby cdd.Key
                            select new
                            {
                                Player = cdd.Key,
                                Drains = from dd in cdd
                                         where (dd.HarmType == (byte)HarmType.Drain)
                                         select dd,
                                AddDrains = from dd in cdd
                                            where ((dd.SecondHarmType == (byte)HarmType.Drain) ||
                                                   (dd.SecondAidType == (byte)AidType.Recovery))
                                            select dd
                            };

            var allHealing = from cd in dataSet.Interactions
                             where ((cd.IsTargetIDNull() == false) &&
                                    ((cd.CombatantsRowByTargetCombatantRelation.CombatantType == (byte)EntityType.Player) ||
                                     (cd.CombatantsRowByTargetCombatantRelation.CombatantType == (byte)EntityType.Pet) ||
                                     (cd.CombatantsRowByTargetCombatantRelation.CombatantType == (byte)EntityType.Fellow)) &&
                                    (((cd.AidType == (byte)AidType.Recovery) &&
                                      (cd.RecoveryType == (byte)RecoveryType.RecoverHP)) ||
                                     ((cd.SecondAidType == (byte)AidType.Recovery) &&
                                      (cd.SecondRecoveryType == (byte)RecoveryType.RecoverHP)) ||
                                     ((cd.AidType == (byte)AidType.Enhance) &&
                                      (cd.Preparing == false) &&
                                      (cd.IsActionIDNull() == false) &&
                                      (cd.ActionsRow.ActionName.StartsWith("Regen")))))
                             group cd by cd.CombatantsRowByTargetCombatantRelation.CombatantName into cdr
                             orderby cdr.Key
                             select new
                             {
                                 Player = cdr.Key,
                                 PrimaryHealed = from hr in cdr
                                                 where hr.AidType == (byte)AidType.Recovery
                                                 select hr,
                                 SecondaryHealed = from hr in cdr
                                                   where hr.SecondAidType == (byte)AidType.Recovery
                                                   select hr,
                                 Regens = from hr in cdr
                                          where hr.AidType == (byte)AidType.Enhance
                                          select hr
                             };


            List<string> playerList = new List<string>();
            foreach (var dmg in dmgTaken)
            {
                if (playerList.Contains(dmg.Player) == false)
                    playerList.Add(dmg.Player);

                playerDamage[dmg.Player] = 0;
                if (dmg.PrimaryDamage.Count() > 0)
                    playerDamage[dmg.Player] += dmg.PrimaryDamage.Sum(d => d.Amount);
                if (dmg.SecondaryDamage.Count() > 0)
                    playerDamage[dmg.Player] += dmg.SecondaryDamage.Sum(d => d.SecondAmount);
            }

            foreach (var drain in hpDrained)
            {
                if (playerList.Contains(drain.Player) == false)
                    playerList.Add(drain.Player);
            }

            foreach (var heal in allHealing)
            {
                if (playerList.Contains(heal.Player) == false)
                    playerList.Add(heal.Player);
            }

            if (playerList.Count > 0)
            {
                playerList.Sort();

                AppendBoldText("Damage Recovery\n", Color.Blue);
                AppendBoldUnderText(dmgRecoveryHeader, Color.Black);
                //"Player           Dmg Taken   HP Drained   HP Cured   #Regen  #Regen 2  #Regen 3"

                StringBuilder sb = new StringBuilder();

                foreach (string player in playerList)
                {
                    sb.Append(player.PadRight(16));
                    sb.Append(" ");

                    if (playerDamage.ContainsKey(player))
                        sb.Append(playerDamage[player].ToString().PadLeft(9));
                    else
                        sb.Append("0".PadLeft(9));

                    sb.Append(" ");

                    var drain = hpDrained.FirstOrDefault(d => d.Player == player);

                    if (drain != null)
                    {
                        int amtDrained = drain.Drains.Sum(d => d.Amount) + drain.AddDrains.Sum(d => d.SecondAmount);
                        sb.Append(amtDrained.ToString().PadLeft(12));
                    }
                    else
                    {
                        sb.Append("0".PadLeft(12));
                    }


                    var heal = allHealing.FirstOrDefault(d => d.Player == player);

                    if (heal != null)
                    {
                        int amtHealed = 0;
                        if (heal.PrimaryHealed != null)
                            amtHealed += heal.PrimaryHealed.Sum(h => h.Amount);
                        if (heal.SecondaryHealed != null)
                            amtHealed += heal.SecondaryHealed.Sum(h => h.SecondAmount);

                        sb.Append(amtHealed.ToString().PadLeft(11));

                        int numR1 = 0;
                        int numR2 = 0;
                        int numR3 = 0;

                        if (heal.Regens != null)
                        {
                            numR1 = heal.Regens.Count(r => r.ActionsRow.ActionName == "Regen");
                            numR2 = heal.Regens.Count(r => r.ActionsRow.ActionName == "Regen II");
                            numR3 = heal.Regens.Count(r => r.ActionsRow.ActionName == "Regen III");
                        }

                        sb.Append(numR1.ToString().PadLeft(9));
                        sb.Append(numR2.ToString().PadLeft(11));
                        sb.Append(numR3.ToString().PadLeft(11));

                    }
                    else
                    {
                        sb.Append("0".PadLeft(11));
                        sb.Append("0".PadLeft(9));
                        sb.Append("0".PadLeft(11));
                        sb.Append("0".PadLeft(11));
                    }

                    sb.Append("\n");
                }

                sb.Append("\n");
                AppendNormalText(sb.ToString());
            }
        }

        private void ProcessRecoveryCuring(KPDatabaseDataSet dataSet)
        {
            var allHealing = from cd in dataSet.Interactions
                             where ((cd.IsActorIDNull() == false) &&
                                    ((cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Player) ||
                                     (cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Pet) ||
                                     (cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Fellow)) &&
                                    (((cd.AidType == (byte)AidType.Recovery) &&
                                      (cd.RecoveryType == (byte)RecoveryType.RecoverHP)) ||
                                     ((cd.AidType == (byte)AidType.Enhance) &&
                                      (cd.Preparing == false) &&
                                      (cd.IsActionIDNull() == false) &&
                                      (cd.ActionsRow.ActionName.StartsWith("Regen")))))
                             group cd by cd.CombatantsRowByTargetCombatantRelation.CombatantName into cdr
                             orderby cdr.Key
                             select new
                             {
                                 Player = cdr.Key,
                                 Cures = from hr in cdr
                                                 where hr.AidType == (byte)AidType.Recovery
                                                 select hr,
                                 Regens = from hr in cdr
                                          where hr.AidType == (byte)AidType.Enhance
                                          select hr
                             };

            if ((allHealing != null) && (allHealing.Count() > 0))
            {
                AppendBoldText("Curing\n", Color.Blue);
                AppendBoldUnderText(cureHeader, Color.Black);
                //"Player           Cured (Sp)  Cured (Ab)  C.1s  C.2s  C.3s  C.4s  C.5s  Curagas  Rg.1s  Rg.2s  Rg.3s"

                StringBuilder sb = new StringBuilder();

                foreach (var healer in allHealing)
                {
                    sb.Append(healer.Player.PadRight(16));
                    sb.Append(" ");

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


                    if (healer.Cures != null)
                    {
                        cureSpell = healer.Cures.Where(c => c.ActionType == (byte)ActionType.Spell).Sum(c => c.Amount);
                        cureAbil = healer.Cures.Where(c => c.ActionType == (byte)ActionType.Ability).Sum(c => c.Amount);

                        numCure1 = healer.Cures.Count(r => r.ActionsRow.ActionName == "Cure");
                        numCure2 = healer.Cures.Count(r => r.ActionsRow.ActionName == "Cure II" ||
                            r.ActionsRow.ActionName == "Curing Waltz");
                        numCure3 = healer.Cures.Count(r => r.ActionsRow.ActionName == "Cure III" ||
                            r.ActionsRow.ActionName == "Curing Waltz II");
                        numCure4 = healer.Cures.Count(r => r.ActionsRow.ActionName == "Cure IV" ||
                            r.ActionsRow.ActionName == "Curing Waltz III");
                        numCure5 = healer.Cures.Count(r => r.ActionsRow.ActionName == "Cure V");

                        numCuraga = healer.Cures.Count(r => r.ActionsRow.ActionName.StartsWith("Curaga") ||
                            r.ActionsRow.ActionName == "Divine Waltz");
                    }

                    if (healer.Regens != null)
                    {
                        numRegen1 = healer.Regens.Count(r => r.ActionsRow.ActionName == "Regen");
                        numRegen2 = healer.Regens.Count(r => r.ActionsRow.ActionName == "Regen II");
                        numRegen3 = healer.Regens.Count(r => r.ActionsRow.ActionName == "Regen III");
                    }


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

                sb.Append("\n");
                AppendNormalText(sb.ToString());


                //-- Second section (Average curing) uses same dataset results as this
                //-- one, so combining within the same function.

                AppendBoldText("Average Curing\n", Color.Blue);
                AppendBoldUnderText(avgCureHeader, Color.Black);

                sb = new StringBuilder();

                foreach (var healer in allHealing)
                {
                    sb.Append(healer.Player.PadRight(16));
                    sb.Append(" ");
                    
                    var cureSet = healer.Cures.Where(c => c.ActionType == (byte)ActionType.Spell);
                    var abilSet = healer.Cures.Where(c => c.ActionType == (byte)ActionType.Ability);

                    var cure1Set = cureSet.Where(c => c.ActionsRow.ActionName == "Cure");
                    var cure2Set = cureSet.Where(c => c.ActionsRow.ActionName == "Cure II");
                    var cure3Set = cureSet.Where(c => c.ActionsRow.ActionName == "Cure III");
                    var cure4Set = cureSet.Where(c => c.ActionsRow.ActionName == "Cure IV");
                    var cure5Set = cureSet.Where(c => c.ActionsRow.ActionName == "Cure IV");

                    double avgC1 = 0;
                    double avgC2 = 0;
                    double avgC3 = 0;
                    double avgC4 = 0;
                    double avgC5 = 0;
                    double avgAb = 0;

                    if ((cure1Set != null) && (cure1Set.Count() > 0))
                        avgC1 = cure1Set.Average(h => h.Amount);
                    if ((cure2Set != null) && (cure2Set.Count() > 0))
                        avgC2 = cure2Set.Average(h => h.Amount);
                    if ((cure3Set != null) && (cure3Set.Count() > 0))
                        avgC3 = cure3Set.Average(h => h.Amount);
                    if ((cure4Set != null) && (cure4Set.Count() > 0))
                        avgC4 = cure4Set.Average(h => h.Amount);
                    if ((cure5Set != null) && (cure5Set.Count() > 0))
                        avgC5 = cure5Set.Average(h => h.Amount);

                    if ((abilSet != null) && (abilSet.Count() > 0))
                        avgAb = abilSet.Average(h => h.Amount);


                    sb.Append(avgC1.ToString("F2").PadLeft(10));
                    sb.Append(avgC2.ToString("F2").PadLeft(13));
                    sb.Append(avgC3.ToString("F2").PadLeft(13));
                    sb.Append(avgC4.ToString("F2").PadLeft(13));
                    sb.Append(avgC5.ToString("F2").PadLeft(13));
                    sb.Append(avgAb.ToString("F2").PadLeft(14));

                    sb.Append("\n");
                }


                sb.Append("\n");
                AppendNormalText(sb.ToString());

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

            sb.Append("\n");
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

            // force display for test
            if (totalDmg > 0)
            {
                AppendBoldText("Incoming Damage\n", Color.Blue);
                AppendBoldUnderText(incDamageHeader, Color.Black);

                StringBuilder sb = new StringBuilder();

                foreach (var player in incAttacks)
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

                sb.Append("\n");
                AppendNormalText(sb.ToString());
            }
        }

        private void ProcessDefenseEvasion(IEnumerable<DefenseGroup> incAttacks)
        {
            AppendBoldText("Evasion\n", Color.Blue);
            AppendBoldUnderText(evasionHeader, Color.Black);

            //Player           M.Evade   M.Evade %   R.Evade   R.Evade %

            StringBuilder sb = new StringBuilder();

            foreach (var player in incAttacks)
            {
                if ((player.Melee.Count() + player.Range.Count()) > 0)
                {
                    sb.Append(player.Player.PadRight(16));
                    sb.Append(" ");

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

                    sb.Append(mEvaded.ToString().PadLeft(7));
                    sb.Append(mEvadePerc.ToString("P2").PadLeft(12));
                    sb.Append(rEvaded.ToString().PadLeft(10));
                    sb.Append(rEvadePerc.ToString("P2").PadLeft(12));

                    sb.Append("\n");
                }
            }

            sb.Append("\n");
            AppendNormalText(sb.ToString());
        }

        private void ProcessDefenseOther(IEnumerable<DefenseGroup> incAttacks)
        {
            bool placedHeader = false;

            //Player           Parry   Parry %   Blink   Blink %   Anticipate  Anticipate %   Counter   Counter %

            StringBuilder sb = new StringBuilder();

            foreach (var player in incAttacks)
            {
                sb.Append(player.Player.PadRight(16));
                sb.Append(" ");

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
                    if (placedHeader == false)
                    {
                        AppendBoldText("Other Defenses\n", Color.Blue);
                        AppendBoldUnderText(otherDefHeader, Color.Black);
                        placedHeader = true;
                    }

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


            sb.Append("\n");
            AppendNormalText(sb.ToString());
        }
        #endregion

        #region Utsu
        private void ProcessDefenseUtsusemi(KPDatabaseDataSet dataSet)
        {
            var utsuUsed = from cd in dataSet.Interactions
                           where ((cd.IsTargetIDNull() == false) &&
                                  (cd.CombatantsRowByTargetCombatantRelation.CombatantType == (byte)EntityType.Player) &&
                                  (cd.ShadowsUsed > 0))
                           group cd by cd.CombatantsRowByTargetCombatantRelation.CombatantName into cdu
                           orderby cdu.Key
                           select new
                           {
                               Player = cdu.Key,
                               Shadows = cdu.Sum(s => s.ShadowsUsed)
                           };


            var utsuCasts = from cd in dataSet.Interactions
                            where ((cd.IsActorIDNull() == false) &&
                                   (cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Player) &&
                                   (cd.ActionType == (byte)ActionType.Spell) &&
                                   (cd.ActionsRow.ActionName.StartsWith("Utsusemi")))
                            group cd by cd.CombatantsRowByActorCombatantRelation.CombatantName into cdu
                            orderby cdu.Key
                            select new
                            {
                                Player = cdu.Key,
                                UtsuIchi = from uc in cdu
                                           where uc.ActionsRow.ActionName == "Utsusemi: Ichi"
                                           select uc,
                                UtsuNi = from uc in cdu
                                         where uc.ActionsRow.ActionName == "Utsusemi: Ni"
                                         select uc
                            };

            if ((utsuUsed.Count() > 0) || (utsuCasts.Count() > 0))
            {
                AppendBoldText("Utsusemi\n\n", Color.Red);
                AppendBoldUnderText(utsuHeader, Color.Black);

                StringBuilder sb = new StringBuilder();

                var playerList = utsuUsed.Select(n => n.Player).
                    Concat(utsuCasts.Select(n => n.Player)).
                    Distinct().OrderBy(n => n);


                foreach (var playerName in playerList)
                {
                    sb.Append(playerName.PadRight(16));
                    sb.Append(" ");

                    int shadsUsed = 0;
                    int ichiCast = 0;
                    int niCast = 0;
                    int ichiFin = 0;
                    int niFin = 0;
                    int numShads = 0;
                    int numShadsN = 0;
                    double effNorm = 0;
                    double effNin = 0;

                    var used = utsuUsed.FirstOrDefault(p => p.Player == playerName);

                    if (used != null)
                    {
                        shadsUsed = used.Shadows;
                    }

                    var cast = utsuCasts.FirstOrDefault(p => p.Player == playerName);

                    if (cast != null)
                    {
                        ichiCast = cast.UtsuIchi.Count(u => u.Preparing == true);
                        ichiFin = cast.UtsuIchi.Count(u => u.Preparing == false);
                        niCast = cast.UtsuNi.Count(u => u.Preparing == true);
                        niFin = cast.UtsuNi.Count(u => u.Preparing = false);

                        numShads = ichiFin * 3 + niFin * 3;
                        numShadsN = ichiFin * 3 + niFin * 4;

                        if (numShads > 0)
                        {
                            effNorm = (double)shadsUsed / numShads;
                            effNin = (double)shadsUsed / numShadsN;
                        }
                    }

                    //Player           Shadows Used   Ichi Cast  Ichi Fin  Ni Cast  Ni Fin  Shadows  Shadows(N)  Efficiency  Efficiency(N)

                    sb.Append(shadsUsed.ToString().PadLeft(12));
                    sb.Append(ichiCast.ToString().PadLeft(12));
                    sb.Append(ichiFin.ToString().PadLeft(10));
                    sb.Append(niCast.ToString().PadLeft(9));
                    sb.Append(niFin.ToString().PadLeft(8));
                    sb.Append(numShads.ToString().PadLeft(9));
                    sb.Append(numShadsN.ToString().PadLeft(11));
                    sb.Append(effNorm.ToString("F2").PadLeft(13));
                    sb.Append(effNin.ToString("F2").PadLeft(14));

                    sb.Append("\n");
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