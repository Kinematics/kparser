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
    public class OffensePlugin : BasePluginControlWithDropdown
    {
        #region IPlugin Overrides
        public override string TabName
        {
            get { return "Offense"; }
        }

        public override void Reset()
        {
            richTextBox.Clear();
            richTextBox.WordWrap = false;

            label1.Text = "Attack Type";
            comboBox1.Left = label1.Right + 10;
            comboBox1.Items.Clear();
            comboBox1.Items.Add("All");
            comboBox1.Items.Add("Summary");
            for (var action = ActionType.Melee; action <= ActionType.Weaponskill; action++)
            {
                comboBox1.Items.Add(action.ToString());
            }
            //comboBox1.Items.Add("Other");
            comboBox1.SelectedIndex = 0;

            label2.Left = comboBox1.Right + 20;
            label2.Text = "Mob Group";
            comboBox2.Left = label2.Right + 10;
            comboBox2.Width = 150;
            comboBox2.Items.Clear();
            comboBox2.Items.Add("All");
            comboBox2.SelectedIndex = 0;
            //comboBox2.Enabled = false;

            checkBox1.Left = comboBox2.Right + 20;
            checkBox1.Text = "Exclude 0 XP Mobs";
            checkBox1.Checked = false;

            //checkBox2.Left = checkBox1.Right + 10;
            //checkBox2.Text = "Exclude 0 Dmg Mobs";
            //checkBox2.Checked = false;
            checkBox2.Enabled = false;
            checkBox2.Visible = false;
        }

        public override void DatabaseOpened(KPDatabaseDataSet dataSet)
        {
            ResetComboBox2();
            AddToComboBox2("All");
            ResetTextBox();

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
                                          group xb by xb.MinBaseExperience() into xbn
                                          orderby xbn.Key
                                          select new { BaseXP = xbn.Key }
                                 };

                if (mobsKilled.Count() > 0)
                {
                    // Add to the Reset list

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

            InitComboBox2Selection();
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
                                          group xb by xb.MinBaseExperience() into xbn
                                          orderby xbn.Key
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

                                // Check for existing entry with higher min base xp
                                mobWithXP = string.Format("{0} ({1})", mob.Name, xp.BaseXP+1);

                                if (comboBox2.Items.Contains(mobWithXP))
                                    RemoveFromComboBox2(mobWithXP);
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
        int totalDamage;
        List<string> playerList = new List<string>();
        Dictionary<string, int> playerDamage = new Dictionary<string,int>();

        string mobSetHeader     = "Mob                        Base XP   Number\n";
        string summaryHeader    = "Player               Total Dmg   Damage %   Melee Dmg   Range Dmg   Spell Dmg   Abil. Dmg  WSkill Dmg\n";
        string meleeHeader      = "Player            Melee Dmg   Melee %   Hit/Miss   M.Acc %  M.Low/Hi    M.Avg  Effect  #Crit  C.Low/Hi   C.Avg     Crit%\n";
        string rangeHeader      = "Player            Range Dmg   Range %   Hit/Miss   R.Acc %  R.Low/Hi    R.Avg  Effect  #Crit  C.Low/Hi   C.Avg     Crit%\n";
        string spellHeader      = "Player            Spell Dmg   Spell %  #Spells  S.Low/Hi     S.Avg  #MagicBurst  MB.Low/Hi   MB.Avg\n";
        string abilHeader       = "Player            Abil. Dmg   Abil. %   Hit/Miss   A.Acc %  A.Low/Hi    A.Avg\n";
        string wskillHeader     = "Player            WSkill Dmg  WSkill %   Hit/Miss  WS.Acc %  WS.Low/Hi   WS.Avg\n";
        string skillchainHeader = "Skillchain          SC Dmg  # SC  SC.Low/Hi  SC.Avg\n";
        string otherHeader      = "Player\n";
        #endregion

        #region Processing sections
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            richTextBox.Clear();
            string actionSourceFilter = comboBox1.SelectedItem.ToString();

            #region Filtering
            string mobFilter;

            if (comboBox2.SelectedIndex >= 0)
                mobFilter = comboBox2.SelectedItem.ToString();
            else
                mobFilter = "All";

            string mobName = "All";
            int xp = 0;

            if (mobFilter != "All")
            {
                Regex mobAndXP = new Regex(@"((?<mobName>.*(?<! \())) \(((?<xp>\d+)\))|(?<mobName>.*)");
                Match mobAndXPMatch = mobAndXP.Match(mobFilter);

                if (mobAndXPMatch.Success == true)
                {
                    mobName = mobAndXPMatch.Groups["mobName"].Value;

                    if ((mobAndXPMatch.Groups["xp"] != null) && (mobAndXPMatch.Groups["xp"].Value != string.Empty))
                    {
                        xp = int.Parse(mobAndXPMatch.Groups["xp"].Value);
                    }
                }
            }
            #endregion

            IEnumerable<AttackGroup> attackSet = null;
            //IEnumerable<MobGroup> mobSet = null;


            //int minXP = 0;
            //if (checkBox1.Checked == true)
            //    minXP = 1;

            #region LINQ queries
            //mobSet = from c in dataSet.Combatants
            //         where ((c.CombatantName == mobName) ||
            //                ((mobName == "All") && (c.CombatantType == (byte)EntityType.Mob)))
            //         orderby c.CombatantName
            //         select new MobGroup
            //         {
            //             Mob = c.CombatantName,
            //             Battles = from b in c.GetBattlesRowsByEnemyCombatantRelation()
            //                       where ((b.Killed == false) ||
            //                              (xp == 0) ||
            //                              (b.BaseExperience() == xp))
            //                       group b by b.BaseExperience() into bx
            //                       orderby bx.Key
            //                       select bx
            //         };

            if (mobFilter == "All")
            {
                attackSet = from c in dataSet.Combatants
                            where ((c.CombatantType == (byte)EntityType.Player) ||
                                   (c.CombatantType == (byte)EntityType.Pet) ||
                                   (c.CombatantType == (byte)EntityType.Fellow) ||
                                   (c.CombatantType == (byte)EntityType.Skillchain))
                            orderby c.CombatantType, c.CombatantName
                            select new AttackGroup
                            {
                                Player = c.CombatantName,
                                Melee = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where (n.ActionType == (byte)ActionType.Melee &&
                                               (n.HarmType == (byte)HarmType.Damage || 
                                                n.HarmType == (byte)HarmType.Drain))
                                        select n,
                                Range = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where (n.ActionType == (byte)ActionType.Ranged &&
                                               (n.HarmType == (byte)HarmType.Damage || 
                                                n.HarmType == (byte)HarmType.Drain))
                                        select n,
                                Spell = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                        where (n.ActionType == (byte)ActionType.Spell &&
                                               (n.HarmType == (byte)HarmType.Damage ||
                                                n.HarmType == (byte)HarmType.Drain))
                                        select n,
                                Ability = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                          where (n.ActionType == (byte)ActionType.Ability &&
                                               (n.HarmType == (byte)HarmType.Damage ||
                                                n.HarmType == (byte)HarmType.Drain))
                                          select n,
                                WSkill = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where (n.ActionType == (byte)ActionType.Weaponskill &&
                                               (n.HarmType == (byte)HarmType.Damage ||
                                                n.HarmType == (byte)HarmType.Drain))
                                         select n,
                                SC = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                     where (n.ActionType == (byte)ActionType.Skillchain &&
                                               (n.HarmType == (byte)HarmType.Damage ||
                                                n.HarmType == (byte)HarmType.Drain))
                                     select n
                            };

            }
            else
            {
                if (xp > 0)
                {
                    // Attacks against a particular mob type of a given base xp

                    attackSet = from c in dataSet.Combatants
                                where ((c.CombatantType == (byte)EntityType.Player) ||
                                       (c.CombatantType == (byte)EntityType.Pet) ||
                                       (c.CombatantType == (byte)EntityType.Fellow) ||
                                       (c.CombatantType == (byte)EntityType.Skillchain))
                                orderby c.CombatantName
                                select new AttackGroup
                                {
                                    Player = c.CombatantName,
                                    Melee = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                            where (n.ActionType == (byte)ActionType.Melee &&
                                                   (n.HarmType == (byte)HarmType.Damage ||
                                                    n.HarmType == (byte)HarmType.Drain) &&
                                                   n.IsTargetIDNull() == false &&
                                                   n.CombatantsRowByTargetCombatantRelation.CombatantName == mobName &&
                                                   n.IsBattleIDNull() == false &&
                                                   n.BattlesRow.MinBaseExperience() == xp)
                                            select n,
                                    Range = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                            where (n.ActionType == (byte)ActionType.Ranged &&
                                                   (n.HarmType == (byte)HarmType.Damage ||
                                                    n.HarmType == (byte)HarmType.Drain) &&
                                                   n.IsTargetIDNull() == false &&
                                                   n.CombatantsRowByTargetCombatantRelation.CombatantName == mobName &&
                                                   n.IsBattleIDNull() == false &&
                                                   n.BattlesRow.MinBaseExperience() == xp)
                                            select n,
                                    Spell = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                            where (n.ActionType == (byte)ActionType.Spell &&
                                                   (n.HarmType == (byte)HarmType.Damage ||
                                                    n.HarmType == (byte)HarmType.Drain) &&
                                                   n.IsTargetIDNull() == false &&
                                                   n.CombatantsRowByTargetCombatantRelation.CombatantName == mobName &&
                                                   n.IsBattleIDNull() == false &&
                                                   n.BattlesRow.MinBaseExperience() == xp)
                                            select n,
                                    Ability = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                              where (n.ActionType == (byte)ActionType.Ability &&
                                                     (n.HarmType == (byte)HarmType.Damage ||
                                                      n.HarmType == (byte)HarmType.Drain) &&
                                                     n.IsTargetIDNull() == false &&
                                                     n.CombatantsRowByTargetCombatantRelation.CombatantName == mobName &&
                                                     n.IsBattleIDNull() == false &&
                                                     n.BattlesRow.MinBaseExperience() == xp)
                                              select n,
                                    WSkill = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                             where (n.ActionType == (byte)ActionType.Weaponskill &&
                                                    (n.HarmType == (byte)HarmType.Damage ||
                                                     n.HarmType == (byte)HarmType.Drain) &&
                                                    n.IsTargetIDNull() == false &&
                                                    n.CombatantsRowByTargetCombatantRelation.CombatantName == mobName &&
                                                    n.IsBattleIDNull() == false &&
                                                    n.BattlesRow.MinBaseExperience() == xp)
                                             select n,
                                    SC = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where (n.ActionType == (byte)ActionType.Skillchain &&
                                                (n.HarmType == (byte)HarmType.Damage ||
                                                 n.HarmType == (byte)HarmType.Drain) &&
                                                n.IsTargetIDNull() == false &&
                                                n.CombatantsRowByTargetCombatantRelation.CombatantName == mobName &&
                                                n.IsBattleIDNull() == false &&
                                                n.BattlesRow.MinBaseExperience() == xp)
                                         select n
                                };
                }
                else
                {
                    // Attacks against a particular mob type
                    attackSet = from c in dataSet.Combatants
                                where ((c.CombatantType == (byte)EntityType.Player) ||
                                       (c.CombatantType == (byte)EntityType.Pet) ||
                                       (c.CombatantType == (byte)EntityType.Fellow) ||
                                       (c.CombatantType == (byte)EntityType.Skillchain))
                                orderby c.CombatantName
                                select new AttackGroup
                                {
                                    Player = c.CombatantName,
                                    Melee = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                            where (n.ActionType == (byte)ActionType.Melee &&
                                                   (n.HarmType == (byte)HarmType.Damage ||
                                                    n.HarmType == (byte)HarmType.Drain) &&
                                                   n.IsTargetIDNull() == false &&
                                                   n.CombatantsRowByTargetCombatantRelation.CombatantName == mobName)
                                            select n,
                                    Range = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                            where (n.ActionType == (byte)ActionType.Ranged &&
                                                   (n.HarmType == (byte)HarmType.Damage ||
                                                    n.HarmType == (byte)HarmType.Drain) &&
                                                   n.IsTargetIDNull() == false &&
                                                   n.CombatantsRowByTargetCombatantRelation.CombatantName == mobName)
                                            select n,
                                    Spell = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                            where (n.ActionType == (byte)ActionType.Spell &&
                                                   (n.HarmType == (byte)HarmType.Damage ||
                                                    n.HarmType == (byte)HarmType.Drain) &&
                                                   n.IsTargetIDNull() == false &&
                                                   n.CombatantsRowByTargetCombatantRelation.CombatantName == mobName)
                                            select n,
                                    Ability = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                              where (n.ActionType == (byte)ActionType.Ability &&
                                                     (n.HarmType == (byte)HarmType.Damage ||
                                                      n.HarmType == (byte)HarmType.Drain) &&
                                                     n.IsTargetIDNull() == false &&
                                                     n.CombatantsRowByTargetCombatantRelation.CombatantName == mobName)
                                              select n,
                                    WSkill = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                             where (n.ActionType == (byte)ActionType.Weaponskill &&
                                                    (n.HarmType == (byte)HarmType.Damage ||
                                                     n.HarmType == (byte)HarmType.Drain) &&
                                                    n.IsTargetIDNull() == false &&
                                                    n.CombatantsRowByTargetCombatantRelation.CombatantName == mobName)
                                             select n,
                                    SC = from n in c.GetInteractionsRowsByActorCombatantRelation()
                                         where (n.ActionType == (byte)ActionType.Skillchain &&
                                                (n.HarmType == (byte)HarmType.Damage ||
                                                 n.HarmType == (byte)HarmType.Drain) &&
                                                n.IsTargetIDNull() == false &&
                                                n.CombatantsRowByTargetCombatantRelation.CombatantName == mobName)
                                         select n
                                };
                }
            }
            #endregion

            if ((attackSet == null) || (attackSet.Count() == 0))
                return;

            int localDamage = 0;
            totalDamage = 0;
            playerDamage.Clear();

            foreach (var player in attackSet)
            {
                playerDamage[player.Player] = 0;

                localDamage = player.MeleeDmg + player.MeleeEffectDmg +
                    player.RangeDmg + player.RangeEffectDmg + player.SpellDmg +
                    player.AbilityDmg + player.WSkillDmg + player.SCDmg;
                playerDamage[player.Player] = localDamage;
                totalDamage += localDamage;
            }

            //ProcessMobSummary(mobSet);

            switch (actionSourceFilter)
            {
                // Unknown == "All"
                case "All":
                    ProcessAttackSummary(attackSet);
                    ProcessMeleeAttacks(attackSet);
                    ProcessRangedAttacks(attackSet);
                    ProcessSpellsAttacks(attackSet);
                    ProcessAbilityAttacks(attackSet);
                    ProcessWeaponskillAttacks(attackSet);
                    ProcessSkillchains(attackSet);
                    //ProcessOtherAttacks(otherAttacks);
                    break;
                case "Summary":
                    ProcessAttackSummary(attackSet);
                    break;
                case "Melee":
                    ProcessMeleeAttacks(attackSet);
                    break;
                case "Ranged":
                    ProcessRangedAttacks(attackSet);
                    break;
                case "Spell":
                    ProcessSpellsAttacks(attackSet);
                    break;
                case "Ability":
                    ProcessAbilityAttacks(attackSet);
                    break;
                case "Weaponskill":
                    ProcessWeaponskillAttacks(attackSet);
                    break;
                case "Skillchain":
                    ProcessSkillchains(attackSet);
                    break;
                default:
                    //ProcessOtherAttacks(otherAttacks);
                    break;
            }
        }

        private void ProcessMobSummary(IEnumerable<MobGroup> mobSet)
        {
            if (mobSet == null)
                return;

            if (mobSet.Count() == 0)
                return;

            StringBuilder sb = new StringBuilder();
            bool headerDisplayed = false;

            int mobCount;

            foreach (var mob in mobSet)
            {
                if (mob.Battles.Count() > 0)
                {
                    foreach (var mobBattle in mob.Battles)
                    {
                        mobCount = mobBattle.Count();

                        if (mobCount > 0)
                        {
                            if (headerDisplayed == false)
                            {
                                AppendBoldText("Mob Listing\n", Color.Red);
                                AppendBoldUnderText(mobSetHeader, Color.Black);

                                headerDisplayed = true;
                            }

                            sb.Append(mob.Mob.PadRight(24));

                            if (mobBattle.Key > 0)
                            {
                                sb.Append(mobBattle.Key.ToString().PadLeft(10));
                            }
                            else
                            {
                                sb.Append("---".PadLeft(10));
                            }

                            sb.Append(mobCount.ToString().PadLeft(9));

                            sb.Append("\n");
                        }
                    }
                }
            }

            if (headerDisplayed == true)
            {
                sb.Append("\n\n");
                AppendNormalText(sb.ToString());
            }
        }

        private void ProcessAttackSummary(IEnumerable<AttackGroup> attacksByPlayer)
        {
            if (attacksByPlayer == null)
                return;

            if (attacksByPlayer.Count() == 0)
                return;

            AppendBoldText("Damage Summary\n", Color.Red);
            AppendBoldUnderText(summaryHeader, Color.Black);

            int playerDmg;
            double damageShare;
            int meleeDmg;
            int rangeDmg;
            int spellDmg;
            int abilDmg;
            int wskillDmg;

            int ttlPlayerDmg = 0;
            double ttlDamageShare = 0;
            int ttlMeleeDmg = 0;
            int ttlRangeDmg = 0;
            int ttlSpellDmg = 0;
            int ttlAbilDmg = 0;
            int ttlWskillDmg = 0;

            StringBuilder sb = new StringBuilder();

            foreach (var player in attacksByPlayer)
            {
                if (playerDamage[player.Player] > 0)
                {
                    // Player name
                    sb.Append(player.Player.PadRight(20));

                    playerDmg = playerDamage[player.Player];
                    damageShare = (double)playerDmg / totalDamage;

                    meleeDmg = player.MeleeDmg;
                    rangeDmg = player.RangeDmg;
                    spellDmg = player.SpellDmg;
                    abilDmg = player.AbilityDmg;
                    wskillDmg = player.WSkillDmg;

                    ttlPlayerDmg += playerDmg;
                    ttlDamageShare += damageShare;
                    ttlMeleeDmg += meleeDmg;
                    ttlRangeDmg += rangeDmg;
                    ttlSpellDmg += spellDmg;
                    ttlAbilDmg += abilDmg;
                    ttlWskillDmg += wskillDmg;

                    sb.Append(playerDmg.ToString().PadLeft(10));
                    sb.Append(damageShare.ToString("P2").PadLeft(11));

                    sb.Append(meleeDmg.ToString().PadLeft(12));
                    sb.Append(rangeDmg.ToString().PadLeft(12));
                    sb.Append(spellDmg.ToString().PadLeft(12));
                    sb.Append(abilDmg.ToString().PadLeft(12));
                    sb.Append(wskillDmg.ToString().PadLeft(12));

                    sb.Append("\n");
                }
            }

            AppendNormalText(sb.ToString());

            sb = new StringBuilder();

            if (ttlPlayerDmg > 0)
            {
                sb.Append("Total".PadRight(20));
                
                sb.Append(ttlPlayerDmg.ToString().PadLeft(10));
                sb.Append(ttlDamageShare.ToString("P2").PadLeft(11));

                sb.Append(ttlMeleeDmg.ToString().PadLeft(12));
                sb.Append(ttlRangeDmg.ToString().PadLeft(12));
                sb.Append(ttlSpellDmg.ToString().PadLeft(12));
                sb.Append(ttlAbilDmg.ToString().PadLeft(12));
                sb.Append(ttlWskillDmg.ToString().PadLeft(12));

                sb.Append("\n");
                AppendBoldText(sb.ToString(), Color.Black);
            }

            AppendNormalText("\n\n");
        }

        private void ProcessMeleeAttacks(IEnumerable<AttackGroup> attacksByPlayer)
        {
            if (attacksByPlayer == null)
                return;

            if (attacksByPlayer.Count() == 0)
                return;

            StringBuilder sb = new StringBuilder();
            bool headerDisplayed = false;

            int meleeDmg;
            double meleePerc;
            int meleeHits;
            int meleeMiss;
            double meleeAcc;
            int normHits;
            int critHits;
            int normLow;
            int normHi;
            double normAvg;
            int critLow;
            int critHi;
            double critAvg;
            double critPerc;
            int effectDmg;


            foreach (var player in attacksByPlayer)
            {
                if (player.Melee.Count() == 0)
                    continue;

                meleeDmg = 0;
                meleePerc = 0;
                meleeHits = 0;
                meleeMiss = 0;
                meleeAcc = 0;
                normHits = 0;
                critHits = 0;
                normLow = 0;
                normHi = 0;
                normAvg = 0;
                critLow = 0;
                critHi = 0;
                critAvg = 0;
                critPerc = 0;
                effectDmg = 0;


                meleeDmg = player.MeleeDmg;
                effectDmg = player.MeleeEffectDmg;

                if (playerDamage[player.Player] > 0)
                    meleePerc = (double)meleeDmg / playerDamage[player.Player];

                var successfulHits = player.Melee.Where(h => h.DefenseType == (byte)DefenseType.None);

                meleeHits = successfulHits.Count();
                meleeMiss = player.Melee.Count(b => b.DefenseType != (byte)DefenseType.None);

                meleeAcc = (double)meleeHits / (meleeHits + meleeMiss);

                var meleeNorm = successfulHits.Where(h => h.DamageModifier == (byte)DamageModifier.None);
                var meleeCrit = successfulHits.Where(h => h.DamageModifier == (byte)DamageModifier.Critical);

                normHits = meleeNorm.Count();
                critHits = meleeCrit.Count();

                if (normHits > 0)
                {
                    normLow = meleeNorm.Min(d => d.Amount);
                    normHi = meleeNorm.Max(d => d.Amount);
                    normAvg = meleeNorm.Average(d => d.Amount);
                }

                if (critHits > 0)
                {
                    critLow = meleeCrit.Min(d => d.Amount);
                    critHi = meleeCrit.Max(d => d.Amount);
                    critAvg = meleeCrit.Average(d => d.Amount);
                }

                if (meleeHits > 0)
                    critPerc = (double)critHits / meleeHits;


                if ((meleeHits + meleeMiss) > 0)
                {
                    if (headerDisplayed == false)
                    {
                        AppendBoldText("Melee Damage\n", Color.Red);
                        AppendBoldUnderText(meleeHeader, Color.Black);

                        headerDisplayed = true;
                    }

                    sb.Append(player.Player.PadRight(17));

                    sb.Append(meleeDmg.ToString().PadLeft(10));
                    sb.Append(meleePerc.ToString("P2").PadLeft(10));
                    sb.Append(string.Format("{0}/{1}", meleeHits, meleeMiss).PadLeft(11));
                    sb.Append(meleeAcc.ToString("P2").PadLeft(10));
                    sb.Append(string.Format("{0}/{1}", normLow, normHi).PadLeft(10));
                    sb.Append(normAvg.ToString("F2").PadLeft(9));
                    sb.Append(effectDmg.ToString().PadLeft(8));
                    sb.Append(critHits.ToString().PadLeft(7));
                    sb.Append(string.Format("{0}/{1}", critLow, critHi).PadLeft(10));
                    sb.Append(critAvg.ToString("F2").PadLeft(8));
                    sb.Append(critPerc.ToString("P2").PadLeft(10));

                    sb.Append("\n");
                }
            }

            if (headerDisplayed == true)
            {
                sb.Append("\n\n");
                AppendNormalText(sb.ToString());
            }
        }

        private void ProcessRangedAttacks(IEnumerable<AttackGroup> attacksByPlayer)
        {
            if (attacksByPlayer == null)
                return;

            if (attacksByPlayer.Count() == 0)
                return;

            StringBuilder sb = new StringBuilder();
            bool headerDisplayed = false;

            int rangeDmg = 0;
            double rangePerc = 0;
            int rangeHits = 0;
            int rangeMiss = 0;
            double rangeAcc = 0;
            int normHits = 0;
            int critHits = 0;
            int normLow = 0;
            int normHi = 0;
            double normAvg = 0;
            int critLow = 0;
            int critHi = 0;
            double critAvg = 0;
            double critPerc = 0;
            int effectDmg = 0;

            foreach (var player in attacksByPlayer)
            {
                if (player.Range.Count() == 0)
                    continue;

                rangeDmg = 0;
                rangePerc = 0;
                rangeHits = 0;
                rangeMiss = 0;
                rangeAcc = 0;
                normHits = 0;
                critHits = 0;
                normLow = 0;
                normHi = 0;
                normAvg = 0;
                critLow = 0;
                critHi = 0;
                critAvg = 0;
                critPerc = 0;
                effectDmg = 0;


                rangeDmg = player.RangeDmg;
                effectDmg = player.RangeEffectDmg;

                if (playerDamage[player.Player] > 0)
                    rangePerc = (double)rangeDmg / playerDamage[player.Player];

                var successfulHits = player.Range.Where(h => h.DefenseType == (byte)DefenseType.None);

                rangeHits = successfulHits.Count();
                rangeMiss = player.Range.Count(b => b.DefenseType != (byte)DefenseType.None);

                rangeAcc = (double)rangeHits / (rangeHits + rangeMiss);

                var rangeNorm = successfulHits.Where(h => h.DamageModifier == (byte)DamageModifier.None);
                var rangeCrit = successfulHits.Where(h => h.DamageModifier == (byte)DamageModifier.Critical);

                normHits = rangeNorm.Count();
                critHits = rangeCrit.Count();

                if (normHits > 0)
                {
                    normLow = rangeNorm.Min(d => d.Amount);
                    normHi = rangeNorm.Max(d => d.Amount);
                    normAvg = rangeNorm.Average(d => d.Amount);
                }

                if (critHits > 0)
                {
                    critLow = rangeCrit.Min(d => d.Amount);
                    critHi = rangeCrit.Max(d => d.Amount);
                    critAvg = rangeCrit.Average(d => d.Amount);
                }

                if (rangeHits > 0)
                    critPerc = (double)critHits / rangeHits;


                if ((rangeHits + rangeMiss) > 0)
                {
                    if (headerDisplayed == false)
                    {
                        AppendBoldText("Ranged Damage\n", Color.Red);
                        AppendBoldUnderText(rangeHeader, Color.Black);

                        headerDisplayed = true;
                    }

                    sb.Append(player.Player.PadRight(17));

                    sb.Append(rangeDmg.ToString().PadLeft(10));
                    sb.Append(rangePerc.ToString("P2").PadLeft(10));
                    sb.Append(string.Format("{0}/{1}", rangeHits, rangeMiss).PadLeft(11));
                    sb.Append(rangeAcc.ToString("P2").PadLeft(10));
                    sb.Append(string.Format("{0}/{1}", normLow, normHi).PadLeft(10));
                    sb.Append(normAvg.ToString("F2").PadLeft(9));
                    sb.Append(effectDmg.ToString().PadLeft(8));
                    sb.Append(critHits.ToString().PadLeft(7));
                    sb.Append(string.Format("{0}/{1}", critLow, critHi).PadLeft(10));
                    sb.Append(critAvg.ToString("F2").PadLeft(8));
                    sb.Append(critPerc.ToString("P2").PadLeft(10));


                    sb.Append("\n");
                }
            }

            if (headerDisplayed == true)
            {
                sb.Append("\n\n");
                AppendNormalText(sb.ToString());
            }
        }

        private void ProcessSpellsAttacks(IEnumerable<AttackGroup> attacksByPlayer)
        {
            if (attacksByPlayer == null)
                return;

            if (attacksByPlayer.Count() == 0)
                return;

            StringBuilder sb = new StringBuilder();
            bool headerDisplayed = false;

            int spellDamage;
            double spellPerc;
            int spellCasts;
            int spellLow;
            int spellHigh;
            double spellAvg;
            int mbCasts;
            int mbLow;
            int mbHigh;
            double mbAvg;
            int normSpellCount;
            int mbSpellCount;

            foreach (var player in attacksByPlayer)
            {
                if (player.Spell.Count() == 0)
                    continue;

                spellDamage = 0;
                spellPerc = 0;
                spellCasts = 0;
                spellLow = 0;
                spellHigh = 0;
                spellAvg = 0;
                mbCasts = 0;
                mbLow = 0;
                mbHigh = 0;
                mbAvg = 0;
                normSpellCount = 0;
                mbSpellCount = 0;

                // Spell damage
                spellDamage = player.SpellDmg;

                if (playerDamage[player.Player] > 0)
                    spellPerc = (double)spellDamage / playerDamage[player.Player];


                var spellsCast = player.Spell.Where(b => b.DefenseType == (byte)DefenseType.None);

                spellCasts = spellsCast.Count();

                var normSpells = spellsCast.Where(s => s.DamageModifier == (byte)DamageModifier.None);
                var mbSpells = spellsCast.Where(s => s.DamageModifier == (byte)DamageModifier.MagicBurst);

                normSpellCount = normSpells.Count();
                mbSpellCount = mbSpells.Count();

                if (normSpellCount > 0)
                {
                    spellLow = normSpells.Min(d => d.Amount);
                    spellHigh = normSpells.Max(d => d.Amount);
                    spellAvg = normSpells.Average(d => d.Amount);
                }

                if (mbSpellCount > 0)
                {
                    mbLow = mbSpells.Min(d => d.Amount);
                    mbHigh = mbSpells.Max(d => d.Amount);
                    mbAvg = mbSpells.Average(d => d.Amount);
                }

                if (spellCasts > 0)
                {
                    if (headerDisplayed == false)
                    {
                        AppendBoldText("Spell Damage\n", Color.Red);
                        AppendBoldUnderText(spellHeader, Color.Black);

                        headerDisplayed = true;
                    }

                    sb.Append(player.Player.PadRight(17));

                    sb.Append(spellDamage.ToString().PadLeft(10));
                    sb.Append(spellPerc.ToString("P2").PadLeft(10));
                    sb.Append(spellCasts.ToString().PadLeft(9));
                    sb.Append(string.Format("{0}/{1}", spellLow, spellHigh).PadLeft(10));
                    sb.Append(spellAvg.ToString("F2").PadLeft(10));
                    sb.Append(mbCasts.ToString().PadLeft(13));
                    sb.Append(string.Format("{0}/{1}", mbLow, mbHigh).PadLeft(11));
                    sb.Append(mbAvg.ToString("F2").PadLeft(9));

                    sb.Append("\n");
                }
            }

            if (headerDisplayed == true)
            {
                sb.Append("\n\n");
                AppendNormalText(sb.ToString());
            }
        }

        private void ProcessAbilityAttacks(IEnumerable<AttackGroup> attacksByPlayer)
        {
            if (attacksByPlayer == null)
                return;

            if (attacksByPlayer.Count() == 0)
                return;

            StringBuilder sb = new StringBuilder();
            bool headerDisplayed = false;

            int abilityDamage;
            double abilPerc;
            int abilUses;
            int abilHits;
            int abilMiss;
            double abilAcc;
            int abilLow;
            int abilHigh;
            double abilAvg;

            foreach (var player in attacksByPlayer)
            {
                if (player.Ability.Count() == 0)
                    continue;

                abilityDamage = 0;
                abilPerc = 0;
                abilUses = 0;
                abilHits = 0;
                abilMiss = 0;
                abilAcc = 0;
                abilLow = 0;
                abilHigh = 0;
                abilAvg = 0;

                // Spell damage
                abilityDamage = player.AbilityDmg;

                if (playerDamage[player.Player] > 0)
                    abilPerc = (double)abilityDamage / playerDamage[player.Player];

                var successfulHits = player.Ability.Where(h => h.DefenseType == (byte)DefenseType.None);

                abilHits = successfulHits.Count();
                abilMiss = player.Ability.Count(b => b.DefenseType != (byte)DefenseType.None);

                abilUses = abilHits + abilMiss;

                if (abilUses > 0)
                    abilAcc = (double)abilHits / abilUses;

                if (abilHits > 0)
                {
                    abilLow = successfulHits.Min(d => d.Amount);
                    abilHigh = successfulHits.Max(d => d.Amount);
                    abilAvg = successfulHits.Average(d => d.Amount);
                }

                if (abilUses > 0)
                {
                    if (headerDisplayed == false)
                    {
                        AppendBoldText("Ability Damage\n", Color.Red);
                        AppendBoldUnderText(abilHeader, Color.Black);

                        headerDisplayed = true;
                    }

                    sb.Append(player.Player.PadRight(17));

                    sb.Append(abilityDamage.ToString().PadLeft(10));
                    sb.Append(abilPerc.ToString("P2").PadLeft(10));
                    sb.Append(string.Format("{0}/{1}", abilHits, abilMiss).PadLeft(11));
                    sb.Append(abilAcc.ToString("P2").PadLeft(10));
                    sb.Append(string.Format("{0}/{1}", abilLow, abilHigh).PadLeft(10));
                    sb.Append(abilAvg.ToString("F2").PadLeft(9));

                    sb.Append("\n");
                }
            }

            if (headerDisplayed == true)
            {
                sb.Append("\n\n");
                AppendNormalText(sb.ToString());
            }
        }

        private void ProcessWeaponskillAttacks(IEnumerable<AttackGroup> attacksByPlayer)
        {
            if (attacksByPlayer == null)
                return;

            if (attacksByPlayer.Count() == 0)
                return;

            StringBuilder sb = new StringBuilder();
            bool headerDisplayed = false;

            int wskillDamage;
            double wskillPerc;
            int wskillUses;
            int wskillHits;
            int wskillMiss;
            double wskillAcc;
            int wskillLow;
            int wskillHigh;
            double wskillAvg;

            foreach (var player in attacksByPlayer)
            {
                if (player.WSkill.Count() == 0)
                    continue;

                wskillDamage = 0;
                wskillPerc = 0;
                wskillUses = 0;
                wskillHits = 0;
                wskillMiss = 0;
                wskillAcc = 0;
                wskillLow = 0;
                wskillHigh = 0;
                wskillAvg = 0;

                // Spell damage
                wskillDamage = player.WSkillDmg;

                if (playerDamage[player.Player] > 0)
                    wskillPerc = (double)wskillDamage / playerDamage[player.Player];

                var successfulHits = player.WSkill.Where(h => h.DefenseType == (byte)DefenseType.None);

                wskillHits = successfulHits.Count();
                wskillMiss = player.WSkill.Count(b => b.DefenseType != (byte)DefenseType.None);

                wskillUses = wskillHits + wskillMiss;

                if (wskillUses > 0)
                    wskillAcc = (double)wskillHits / wskillUses;

                if (wskillHits > 0)
                {
                    wskillLow = successfulHits.Min(d => d.Amount);
                    wskillHigh = successfulHits.Max(d => d.Amount);
                    wskillAvg = successfulHits.Average(d => d.Amount);
                }

                if (wskillUses > 0)
                {
                    if (headerDisplayed == false)
                    {
                        AppendBoldText("Weaponskill Damage\n", Color.Red);
                        AppendBoldUnderText(wskillHeader, Color.Black);

                        headerDisplayed = true;
                    }

                    sb.Append(player.Player.PadRight(17));

                    sb.Append(wskillDamage.ToString().PadLeft(10));
                    sb.Append(wskillPerc.ToString("P2").PadLeft(11));
                    sb.Append(string.Format("{0}/{1}", wskillHits, wskillMiss).PadLeft(11));
                    sb.Append(wskillAcc.ToString("P2").PadLeft(10));
                    sb.Append(string.Format("{0}/{1}", wskillLow, wskillHigh).PadLeft(10));
                    sb.Append(wskillAvg.ToString("F2").PadLeft(10));

                    sb.Append("\n");
                }
            }

            if (headerDisplayed == true)
            {
                sb.Append("\n\n");
                AppendNormalText(sb.ToString());
            }
        }

        private void ProcessSkillchains(IEnumerable<AttackGroup> attacksByPlayer)
        {
            if (attacksByPlayer == null)
                return;

            if (attacksByPlayer.Count() == 0)
                return;

            StringBuilder sb = new StringBuilder();
            bool headerDisplayed = false;

            int scDamage;
            double scPerc;
            int numSCs;
            int scLow;
            int scHigh;
            double scAvg;

            foreach (var player in attacksByPlayer)
            {
                if (player.SC.Count() == 0)
                    continue;

                scDamage = 0;
                scPerc = 0;
                numSCs = 0;
                scLow = 0;
                scHigh = 0;
                scAvg = 0;

                // Spell damage
                scDamage = player.SCDmg;

                if (playerDamage[player.Player] > 0)
                    scPerc = (double)scDamage / playerDamage[player.Player];

                numSCs = player.SC.Count();

                if (numSCs > 0)
                {
                    scLow = player.SC.Min(d => d.Amount);
                    scHigh = player.SC.Max(d => d.Amount);
                    scAvg = player.SC.Average(d => d.Amount);
                }

                if (numSCs > 0)
                {
                    if (headerDisplayed == false)
                    {
                        AppendBoldText("Skillchain Damage\n", Color.Red);
                        AppendBoldUnderText(skillchainHeader, Color.Black);

                        headerDisplayed = true;
                    }

                    sb.Append(player.Player.PadRight(20));

                    sb.Append(scDamage.ToString().PadLeft(6));
                    sb.Append(numSCs.ToString().PadLeft(6));
                    sb.Append(string.Format("{0}/{1}", scLow, scHigh).PadLeft(11));
                    sb.Append(scAvg.ToString("F2").PadLeft(8));

                    sb.Append("\n");
                }
            }

            if (headerDisplayed == true)
            {
                sb.Append("\n\n");
                AppendNormalText(sb.ToString());
            }
        }

        private void ProcessOtherAttacks(IEnumerable<AttackGroup> attacksByPlayer)
        {
            if (attacksByPlayer == null)
                return;

            if (attacksByPlayer.Count() == 0)
                return;

            AppendBoldText("Other Damage\n", Color.Red);

            if (otherHeader == null)
                otherHeader = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10}\n",
                "Player".PadRight(16), "Other Dmg".PadLeft(10), "Total Dmg".PadLeft(10), "Other %".PadLeft(9),
                "Dmg Share %".PadLeft(12), "# Counterattacks".PadLeft(18), "C.Dmg".PadLeft(8), "Avg C.Dmg".PadLeft(10),
                "# Spikes".PadLeft(9), "Spk.Dmg".PadLeft(9), "Avg Spk.Dmg".PadLeft(12));

            AppendBoldUnderText(otherHeader, Color.Black);

            //var counterAttacks = attacksByPlayer.FirstOrDefault(a => a.ActionSource == ActionType.Counterattack);
            //var spikesAttacks = otherAttacks.FirstOrDefault(a => a.ActionSource == ActionType.Spikes);
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
