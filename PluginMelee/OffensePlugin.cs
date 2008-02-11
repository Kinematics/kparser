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
            int allBattles = dataSet.Battles.Count(b => b.DefaultBattle == false);

            if (allBattles > 0)
            {
                var mobsKilled = from b in dataSet.Battles
                                 where ((b.DefaultBattle == false) &&
                                        (b.IsEnemyIDNull() == false) &&
                                        (b.CombatantsRowByEnemyCombatantRelation.CombatantType == (byte)EntityType.Mob))
                                 group b by b.CombatantsRowByEnemyCombatantRelation.CombatantName into bn
                                 orderby bn.Key
                                 select new
                                 {
                                     Name = bn.Key,
                                     XP = from xb in bn
                                          group xb by xb.BaseExperience() into xbn
                                          select new { BaseXP = xbn.Key }
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
                            mobWithXP = string.Format("{0} ({1})", mob.Name, xp.BaseXP);

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
                                     select new {
                                         Name = bn.Key,
                                         XP = from xb in bn
                                              group xb by xb.BaseExperience() into xbn
                                              select new { BXP = xbn.Key}
                                     };

                    if (mobsKilled != null)
                    {
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
        int totalDamage;
        List<string> playerList = new List<string>();
        Dictionary<string, int> playerDamage = new Dictionary<string,int>();

        string summaryHeader    = "Player            Total Dmg   Damage %   Melee Dmg   Range Dmg   Spell Dmg   Abil. Dmg  WSkill Dmg\n";
        string meleeHeader      = "Player            Melee Dmg   Melee %   Hit/Miss    M.Acc%  M.Low/Hi    M.Avg  Effect  #Crit  C.Low/Hi   C.Avg     Crit%\n";
        string rangeHeader      = "Player            Range Dmg   Range %   Hit/Miss    R.Acc%  R.Low/Hi    R.Avg  Effect  #Crit  C.Low/Hi   C.Avg     Crit%\n";
        string spellHeader      = "Player            Spell Dmg   Spell %  #Spells  S.Low/Hi   S.Avg  #MagicBurst  MB.Low/Hi   MB.Avg\n";
        string abilHeader       = "Player            Abil. Dmg   Abil. %   Hit/Miss      Acc%  A.Low/Hi    A.Avg\n";
        string wskillHeader     = "Player            WSkill Dmg  WSkill %   Hit/Miss      Acc%  WS.Low/Hi   WS.Avg\n";
        string skillchainHeader = "Skillchain        Skill Dmg   # SC   SC.Low/Hi  SC.Avg\n";
        string otherHeader      = "Player\n";
        #endregion

        #region Processing sections
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            richTextBox.Clear();
            string actionSourceFilter = comboBox1.SelectedItem.ToString();

            Stopwatch stopwatch = new Stopwatch();


            stopwatch.Reset();
            stopwatch.Start();

            string mobFilter = comboBox2.SelectedItem.ToString();
            IEnumerable<AttackGroup> attacksByType;
            IEnumerable<AttackGroup2> attacksByPlayer;

            int minXP = 0;
            if (checkBox1.Checked == true)
                minXP = 1;

            if (mobFilter == "All")
            {
                attacksByType = from cd in dataSet.Interactions
                             where ((cd.IsActorIDNull() == false) &&
                                    ((cd.HarmType == (byte)HarmType.Damage) ||
                                     (cd.HarmType == (byte)HarmType.Drain)) &&
                                    ((cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Player) ||
                                     (cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Pet) ||
                                     (cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Fellow) ||
                                     (cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Skillchain))
                                    ) &&
                                    ((cd.BattlesRow.ExperiencePoints >= minXP) || (cd.BattlesRow.Killed == false))
                             group cd by cd.ActionType into cda
                             select new AttackGroup
                                 {
                                     ActionSource = (ActionType) cda.Key,
                                     CombatGroup = from c in cda
                                                   orderby c.CombatantsRowByActorCombatantRelation.CombatantName
                                                   group c by c.CombatantsRowByActorCombatantRelation
                                 };


                attacksByPlayer = from cd in dataSet.Interactions
                                  where ((cd.IsActorIDNull() == false) &&
                                         ((cd.HarmType == (byte)HarmType.Damage) ||
                                          (cd.HarmType == (byte)HarmType.Drain)) &&
                                         ((cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Player) ||
                                          (cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Pet) ||
                                          (cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Fellow) ||
                                          (cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Skillchain))
                                         ) &&
                                         ((cd.BattlesRow.ExperiencePoints >= minXP) || (cd.BattlesRow.Killed == false))
                                  group cd by cd.CombatantsRowByActorCombatantRelation.CombatantName into cdp
                                  orderby cdp.Key
                                  select new AttackGroup2
                                  {
                                      Player = cdp.Key,
                                      Melee = from g in cdp
                                              where g.ActionType == (byte)ActionType.Melee
                                              select g,
                                      Range = from g in cdp
                                              where g.ActionType == (byte)ActionType.Ranged
                                              select g,
                                      Spell = from g in cdp
                                              where g.ActionType == (byte)ActionType.Spell
                                              select g,
                                      Ability = from g in cdp
                                                where g.ActionType == (byte)ActionType.Ability
                                                select g,
                                      WSkill = from g in cdp
                                               where g.ActionType == (byte)ActionType.Weaponskill
                                               select g,
                                      SC = from g in cdp
                                           where g.ActionType == (byte)ActionType.Skillchain
                                           select g,

                                      
                                     
                                  };


            }
            else
            {

                attacksByPlayer = from cd in dataSet.Interactions
                                  where ((cd.IsActorIDNull() == false) &&
                                         ((cd.HarmType == (byte)HarmType.Damage) ||
                                          (cd.HarmType == (byte)HarmType.Drain)) &&
                                         ((cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Player) ||
                                          (cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Pet) ||
                                          (cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Fellow) ||
                                          (cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Skillchain))
                                         ) &&
                                         ((cd.BattlesRow.ExperiencePoints >= minXP) || (cd.BattlesRow.Killed == false))
                                  group cd by cd.CombatantsRowByActorCombatantRelation.CombatantName into cdp
                                  orderby cdp.Key
                                  select new AttackGroup2
                                  {
                                      Player = cdp.Key,
                                      Melee = from g in cdp
                                              where g.ActionType == (byte)ActionType.Melee
                                              select g,
                                      Range = from g in cdp
                                              where g.ActionType == (byte)ActionType.Ranged
                                              select g,
                                      Spell = from g in cdp
                                              where g.ActionType == (byte)ActionType.Spell
                                              select g,
                                      Ability = from g in cdp
                                                where g.ActionType == (byte)ActionType.Ability
                                                select g,
                                      WSkill = from g in cdp
                                               where g.ActionType == (byte)ActionType.Weaponskill
                                               select g,
                                      SC = from g in cdp
                                           where g.ActionType == (byte)ActionType.Skillchain
                                           select g
                                  };

                Regex mobAndXP = new Regex(@"(?<mobName>\w+(['\- ](\d|\w)+)*)( \((?<xp>\d+)\))?");
                Match mobAndXPMatch = mobAndXP.Match(mobFilter);

                if (mobAndXPMatch.Success == true)
                {
                    if (mobAndXPMatch.Captures.Count == 1)
                    {
                        // Name only
                        attacksByType = from cd in dataSet.Interactions
                                     where ((cd.IsActorIDNull() == false) &&
                                            (cd.HarmType == (byte)HarmType.Damage) &&
                                            ((cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Player) ||
                                             (cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Pet) ||
                                             (cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Fellow) ||
                                             (cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Skillchain))
                                            ) &&
                                            (cd.BattlesRow.CombatantsRowByEnemyCombatantRelation.CombatantName == mobAndXPMatch.Groups["mobName"].Value) &&
                                            ((cd.BattlesRow.ExperiencePoints >= minXP) || (cd.BattlesRow.Killed == false))
                                     group cd by cd.ActionType into cda
                                     select new AttackGroup
                                         {
                                             ActionSource = (ActionType)cda.Key,
                                             CombatGroup = from c in cda
                                                           orderby c.CombatantsRowByActorCombatantRelation.CombatantName
                                                           group c by c.CombatantsRowByActorCombatantRelation
                                         };
                    }
                    else if (mobAndXPMatch.Captures.Count == 2)
                    {
                        // Name and XP
                        int xp = int.Parse(mobAndXPMatch.Groups["xp"].Value);

                        attacksByType = from cd in dataSet.Interactions
                                     where ((cd.IsActorIDNull() == false) &&
                                            (cd.HarmType == (byte)HarmType.Damage) &&
                                            ((cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Player) ||
                                             (cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Pet) ||
                                             (cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Fellow) ||
                                             (cd.CombatantsRowByActorCombatantRelation.CombatantType == (byte)EntityType.Skillchain))
                                            ) &&
                                            (cd.BattlesRow.CombatantsRowByEnemyCombatantRelation.CombatantName == mobAndXPMatch.Groups["mobName"].Value) &&
                                            (cd.BattlesRow.BaseExperience() == xp) &&
                                            ((cd.BattlesRow.ExperiencePoints >= minXP) || (cd.BattlesRow.Killed == false))
                                     group cd by cd.ActionType into cda
                                     select new AttackGroup
                                     {
                                         ActionSource = (ActionType)cda.Key,
                                         CombatGroup =
                                          from c in cda
                                          orderby c.CombatantsRowByActorCombatantRelation.CombatantName
                                          group c by c.CombatantsRowByActorCombatantRelation
                                     };
                    }
                    else
                    {
                        Logger.Instance.Log("OffensePlugin", "Failed in mob filtering.  Invalid number of captures.");
                        return;
                    }
                }
                else
                {
                    Logger.Instance.Log("OffensePlugin", "Failed in mob filtering.  Match failed.");
                    return;
                }
            }

            stopwatch.Stop();
            Debug.WriteLine(string.Format("Offense: Prep Linq section time: {0} ms", stopwatch.Elapsed.TotalMilliseconds));
            stopwatch.Reset();
            stopwatch.Start();

            int localDamage = 0;
            totalDamage = 0;
            playerDamage.Clear();

            foreach (var player in attacksByPlayer)
            {
                playerDamage[player.Player] = 0;

                localDamage = player.MeleeDmg + player.RangeDmg + player.SpellDmg
                    + player.AbilityDmg + player.WSkillDmg;
                playerDamage[player.Player] = localDamage;
                totalDamage += localDamage;
            }

            stopwatch.Stop();
            Debug.WriteLine(string.Format("Offense: Revised Setup player list/damage time: {0} ms", stopwatch.Elapsed.TotalMilliseconds));
            stopwatch.Reset();
            stopwatch.Start();


            AttackGroup meleeAttacks;
            AttackGroup rangeAttacks;
            AttackGroup spellAttacks;
            AttackGroup abilAttacks;
            AttackGroup wskillAttacks;
            AttackGroup skillchainAttacks;


            //var otherAttacks = allAttacks.Where(m =>
            //    (m.ActionSource != ActionSourceType.Melee) &&
            //    (m.ActionSource != ActionSourceType.Ranged) &&
            //    (m.ActionSource != ActionSourceType.Spell) &&
            //    (m.ActionSource != ActionSourceType.Ability) &&
            //    (m.ActionSource != ActionSourceType.Weaponskill) );

            switch (actionSourceFilter)
            {
                // Unknown == "All"
                case "All":
                    meleeAttacks = attacksByType.FirstOrDefault(m => m.ActionSource == ActionType.Melee);
                    rangeAttacks = attacksByType.FirstOrDefault(m => m.ActionSource == ActionType.Ranged);
                    spellAttacks = attacksByType.FirstOrDefault(m => m.ActionSource == ActionType.Spell);
                    abilAttacks = attacksByType.FirstOrDefault(m => m.ActionSource == ActionType.Ability);
                    wskillAttacks = attacksByType.FirstOrDefault(m => m.ActionSource == ActionType.Weaponskill);
                    skillchainAttacks = attacksByType.FirstOrDefault(m => m.ActionSource == ActionType.Skillchain);
                    stopwatch.Start();
                    ProcessAttackSummary(attacksByPlayer);
                    stopwatch.Stop();
                    Debug.WriteLine(string.Format("Offense: Process Summary time: {0} ms", stopwatch.Elapsed.TotalMilliseconds));
                    ProcessMeleeAttacks(meleeAttacks);
                    ProcessRangedAttacks(rangeAttacks);
                    ProcessSpellsAttacks(spellAttacks);
                    ProcessAbilityAttacks(abilAttacks);
                    ProcessWeaponskillAttacks(wskillAttacks);
                    //ProcessOtherAttacks(otherAttacks);
                    break;
                case "Summary":
                    ProcessAttackSummary(attacksByPlayer);
                    break;
                case "Melee":
                    meleeAttacks = attacksByType.FirstOrDefault(m => m.ActionSource == ActionType.Melee);
                    ProcessMeleeAttacks(meleeAttacks);
                    break;
                case "Ranged":
                    rangeAttacks = attacksByType.FirstOrDefault(m => m.ActionSource == ActionType.Ranged);
                    ProcessRangedAttacks(rangeAttacks);
                    break;
                case "Spell":
                    spellAttacks = attacksByType.FirstOrDefault(m => m.ActionSource == ActionType.Spell);
                    ProcessSpellsAttacks(spellAttacks);
                    break;
                case "Ability":
                    abilAttacks = attacksByType.FirstOrDefault(m => m.ActionSource == ActionType.Ability);
                    ProcessAbilityAttacks(abilAttacks);
                    break;
                case "Weaponskill":
                    wskillAttacks = attacksByType.FirstOrDefault(m => m.ActionSource == ActionType.Weaponskill);
                    ProcessWeaponskillAttacks(wskillAttacks);
                    break;
                case "Skillchain":
                    skillchainAttacks = attacksByType.FirstOrDefault(m => m.ActionSource == ActionType.Skillchain);
                    ProcessSkillchains(skillchainAttacks);
                    break;
                default:
                    //ProcessOtherAttacks(otherAttacks);
                    break;
            }
        }

        private void ProcessAttackSummary(IEnumerable<AttackGroup2> attacksByPlayer)
        {
            if (attacksByPlayer == null)
                return;

            if (attacksByPlayer.Count() == 0)
                return;

            AppendBoldText("Damage Summary\n", Color.Red);
            AppendBoldUnderText(summaryHeader, Color.Black);


            StringBuilder sb = new StringBuilder();

            foreach (var player in attacksByPlayer)
            {
                if (playerDamage[player.Player] > 0)
                {
                    // Player name
                    sb.Append(player.Player.PadRight(16));
                    sb.Append(" ");

                    int ttlPlayerDmg = playerDamage[player.Player];
                    double damageShare = (double)ttlPlayerDmg / totalDamage;

                    int meleeDmg = player.MeleeDmg;
                    int rangeDmg = player.RangeDmg;
                    int spellDmg = player.SpellDmg;
                    int abilDmg = player.AbilityDmg;
                    int wskillDmg = player.WSkillDmg;

                    sb.Append(ttlPlayerDmg.ToString().PadLeft(10));
                    sb.Append(damageShare.ToString("P2").PadLeft(11));

                    sb.Append(meleeDmg.ToString().PadLeft(12));
                    sb.Append(rangeDmg.ToString().PadLeft(12));
                    sb.Append(spellDmg.ToString().PadLeft(12));
                    sb.Append(abilDmg.ToString().PadLeft(12));
                    sb.Append(wskillDmg.ToString().PadLeft(12));

                    sb.Append("\n");
                }
            }

            sb.Append("\n\n");
            AppendNormalText(sb.ToString());
        }

        private void ProcessMeleeAttacks(AttackGroup meleeAttacks)
        {
            if (meleeAttacks == null)
                return;

            if (meleeAttacks.CombatGroup.Count() == 0)
                return;

            AppendBoldText("Melee Damage\n", Color.Red);
            AppendBoldUnderText(meleeHeader, Color.Black);

            StringBuilder sb = new StringBuilder();

            foreach (var player in meleeAttacks.CombatGroup)
            {

                sb.Append(player.Key.CombatantName.PadRight(16));
                sb.Append(" ");

                int meleeDmg = 0;
                double meleePerc = 0;
                int meleeHits = 0;
                int meleeMiss = 0;
                double meleeAcc = 0;
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


                meleeDmg = player.Sum(d => d.Amount);
                effectDmg = player.Where(s =>
                                s.SecondHarmType == (byte)HarmType.Damage ||
                                s.SecondHarmType == (byte)HarmType.Drain).
                                Sum(d => d.SecondAmount);

                if (playerDamage[player.Key.CombatantName] > 0)
                    meleePerc = (double)meleeDmg / playerDamage[player.Key.CombatantName];

                var successfulHits = player.Where(h => h.DefenseType == (byte)DefenseType.None);

                meleeHits = successfulHits.Count();
                meleeMiss = player.Count(b => b.DefenseType != (byte)DefenseType.None);

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

            sb.Append("\n\n");
            AppendNormalText(sb.ToString());
        }

        private void ProcessRangedAttacks(AttackGroup rangeAttacks)
        {
            if (rangeAttacks == null)
                return;

            if (rangeAttacks.CombatGroup.Count() == 0)
                return;


            AppendBoldText("Ranged Damage\n", Color.Red);
            AppendBoldUnderText(rangeHeader, Color.Black);

            StringBuilder sb = new StringBuilder();

            foreach (var player in rangeAttacks.CombatGroup)
            {
                sb.Append(player.Key.CombatantName.PadRight(16));
                sb.Append(" ");

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


                rangeDmg = player.Sum(d => d.Amount);
                effectDmg = player.Where(s =>
                                s.SecondHarmType == (byte)HarmType.Damage ||
                                s.SecondHarmType == (byte)HarmType.Drain).
                                Sum(d => d.SecondAmount);

                if (playerDamage[player.Key.CombatantName] > 0)
                    rangePerc = (double)rangeDmg / playerDamage[player.Key.CombatantName];

                var successfulHits = player.Where(h => h.DefenseType == (byte)DefenseType.None);

                rangeHits = successfulHits.Count();
                rangeMiss = player.Count(b => b.DefenseType != (byte)DefenseType.None);

                rangeAcc = (double)rangeHits / (rangeHits + rangeMiss);

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

                if (rangeHits > 0)
                    critPerc = (double)critHits / rangeHits;

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

            sb.Append("\n\n");
            AppendNormalText(sb.ToString());
        }

        private void ProcessSpellsAttacks(AttackGroup spellAttacks)
        {
            if (spellAttacks == null)
                return;

            if (spellAttacks.CombatGroup.Count() == 0)
                return;


            AppendBoldText("Spell Damage\n", Color.Red);
            AppendBoldUnderText(spellHeader, Color.Black);

            foreach (var player in spellAttacks.CombatGroup)
            {
                if (player.Count() > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append(player.Key.CombatantName.PadRight(16));
                    sb.Append(" ");

                    // Spell damage
                    int spellDamage = player.Sum(b => b.Amount);
                    sb.Append(spellDamage.ToString().PadLeft(10));
                    sb.Append(" ");

                    // Percent of player damage from Spells
                    int damageDone = playerDamage[player.Key.CombatantName];
                    sb.Append(((double)spellDamage / damageDone).ToString("P2").PadLeft(9));
                    sb.Append(" ");

                    var spellsCast = player.Where(b => b.DefenseType == (byte)DefenseType.None);

                    var normSpells = spellsCast.Where(s => s.DamageModifier == (byte)DamageModifier.None);
                    var mbSpells = spellsCast.Where(s => s.DamageModifier == (byte)DamageModifier.MagicBurst);

                    int casts = spellsCast.Count();

                    if (casts > 0)
                    {
                        // # cast
                        sb.Append(casts.ToString().PadLeft(8));
                        sb.Append(" ");

                        // M.Low/Hi
                        int low = normSpells.Min(b => b.Amount);
                        int high = normSpells.Max(b => b.Amount);

                        sb.Append(string.Format("{0}/{1}", low, high).PadLeft(9));
                        sb.Append(" ");

                        // Spell avg
                        sb.Append(((double)normSpells.Sum(s => s.Amount) / normSpells.Count()).ToString("F2").PadLeft(7));
                        sb.Append(" ");

                        int mbCount = mbSpells.Count();

                        if (mbCount > 0)
                        {
                            // # MBs
                            sb.Append(mbCount.ToString().PadLeft(12));
                            sb.Append(" ");

                            // M.Low/Hi
                            low = mbSpells.Min(b => b.Amount);
                            high = mbSpells.Max(b => b.Amount);

                            sb.Append(string.Format("{0}/{1}", low, high).PadLeft(10));
                            sb.Append(" ");

                            // MB avg
                            sb.Append(((double)mbSpells.Sum(s => s.Amount) / mbSpells.Count()).ToString("F2").PadLeft(8));
                            sb.Append(" ");
                        }
                        else
                        {
                            // MB Cast
                            sb.Append("0".PadLeft(12));
                            sb.Append(" ");

                            // MB Low/High
                            sb.Append("N/A".PadLeft(10));
                            sb.Append(" ");

                            // MB avg
                            sb.Append("N/A".PadLeft(8));
                            sb.Append(" ");
                        }
                    }
                    else
                    {
                        // # cast
                        sb.Append("N/A".PadLeft(8));
                        sb.Append(" ");

                        // M.Low/Hi
                        sb.Append("N/A".PadLeft(9));
                        sb.Append(" ");

                        // Spell avg
                        sb.Append("N/A".PadLeft(7));
                        sb.Append(" ");

                        // MB Cast
                        sb.Append("N/A".PadRight(12));
                        sb.Append(" ");

                        // MB Low/High
                        sb.Append("N/A".PadLeft(10));
                        sb.Append(" ");

                        // MB avg
                        sb.Append("N/A".PadLeft(8));
                        sb.Append(" ");
                    }

                    sb.Append("\n");
                    AppendNormalText(sb.ToString());
                }
            }

            AppendNormalText("\n\n");
        }

        private void ProcessAbilityAttacks(AttackGroup abilAttacks)
        {
            if (abilAttacks == null)
                return;

            if (abilAttacks.CombatGroup.Count() == 0)
                return;

            AppendBoldText("Ability Damage\n", Color.Red);
            AppendBoldUnderText(abilHeader, Color.Black);

            foreach (var player in abilAttacks.CombatGroup)
            {
                if (player.Count() > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append(player.Key.CombatantName.PadRight(16));
                    sb.Append(" ");

                    // Ability damage
                    int abilityDmg = player.Sum(b => b.Amount);
                    sb.Append(abilityDmg.ToString().PadLeft(10));
                    sb.Append(" ");

                    // Percent of player damage from Abilities
                    int damageDone = playerDamage[player.Key.CombatantName];
                    sb.Append(((double)abilityDmg / damageDone).ToString("P2").PadLeft(9));
                    sb.Append(" ");


                    int hits = player.Count(b => b.DefenseType == (byte)DefenseType.None);
                    int misses = player.Count(b => b.DefenseType != (byte)DefenseType.None);

                    // Hits/Misses
                    sb.Append(string.Format("{0}/{1}", hits, misses).PadLeft(10));
                    sb.Append(" ");

                    // Accuracy
                    sb.Append(((double)hits / (hits + misses)).ToString("P2").PadLeft(9));
                    sb.Append(" ");

                    // A.Low/Hi
                    var successfulHits = player.Where(h => (h.DefenseType == (byte)DefenseType.None) &&
                        (h.DamageModifier == (byte)DamageModifier.None));

                    int successfulHitCount = successfulHits.Count();

                    if (successfulHitCount > 0)
                    {
                        int low = successfulHits.Min(b => b.Amount);
                        int high = successfulHits.Max(b => b.Amount);

                        // Ability low/high
                        sb.Append(string.Format("{0}/{1}", low, high).PadLeft(9));
                        sb.Append(" ");

                        // Ability avg
                        sb.Append(((double)abilityDmg / successfulHitCount).ToString("F2").PadLeft(8));
                        sb.Append(" ");
                    }
                    else
                    {
                        // Ability low/high
                        sb.Append("N/A".PadLeft(9));
                        sb.Append(" ");

                        // Ability avg
                        sb.Append("N/A".PadLeft(8));
                        sb.Append(" ");
                    }

                    sb.Append("\n");
                    AppendNormalText(sb.ToString());
                }
            }

            AppendNormalText("\n\n");
        }

        private void ProcessWeaponskillAttacks(AttackGroup wskillAttacks)
        {
            if (wskillAttacks == null)
                return;

            if (wskillAttacks.CombatGroup.Count() == 0)
                return;

            AppendBoldText("Weaponskill Damage\n", Color.Red);
            AppendBoldUnderText(wskillHeader, Color.Black);

            StringBuilder sb = new StringBuilder();

            foreach (var player in wskillAttacks.CombatGroup)
            {
                if (player.Count() > 0)
                {
                    sb.Append(player.Key.CombatantName.PadRight(16));
                    sb.Append(" ");

                    int wsDamage = 0;
                    double wsPerc = 0;
                    int wsHit = 0;
                    int wsMiss = 0;
                    double wsAcc = 0;
                    int wsLow = 0;
                    int wsHi = 0;
                    double wsAvg = 0;


                    // Weaponskill damage
                    wsDamage = player.Sum(b => b.Amount);

                    // Percent of player damage from Weaponskills
                    if (playerDamage[player.Key.CombatantName] > 0)
                        wsPerc = (double)wsDamage / playerDamage[player.Key.CombatantName];

                    wsHit = player.Count(b => b.DefenseType == (byte)DefenseType.None);
                    wsMiss = player.Count(b => b.DefenseType != (byte)DefenseType.None);

                    wsAcc = (double)wsHit / (wsHit + wsMiss);

                    if (wsHit > 0)
                    {
                        var weaponskills = player.Where(h => h.DefenseType == (byte)DefenseType.None);
                        wsLow = weaponskills.Min(w => w.Amount);
                        wsHi = weaponskills.Max(w => w.Amount);
                        wsAvg = weaponskills.Average(w => w.Amount);
                    }

                    sb.Append(wsDamage.ToString().PadLeft(11));
                    sb.Append(wsPerc.ToString("P2").PadLeft(10));
                    sb.Append(string.Format("{0}/{1}", wsHit, wsMiss).PadLeft(11));
                    sb.Append(wsAcc.ToString("P2").PadLeft(10));
                    sb.Append(string.Format("{0}/{1}", wsLow, wsHi).PadLeft(11));
                    sb.Append(wsAvg.ToString("F2").PadLeft(9));

                    sb.Append("\n");
                }
            }

            sb.Append("\n\n");
            AppendNormalText(sb.ToString());
        }

        private void ProcessSkillchains(AttackGroup skillchainAttacks)
        {
            if (skillchainAttacks == null)
                return;

            if (skillchainAttacks.CombatGroup.Count() == 0)
                return;

            AppendBoldText("Skillchain Damage\n", Color.Red);
            AppendBoldUnderText(skillchainHeader, Color.Black);

            StringBuilder sb = new StringBuilder();

            foreach (var player in skillchainAttacks.CombatGroup)
            {
                if (player.Count() > 0)
                {
                    sb.Append(player.Key.CombatantName.PadRight(16));
                    sb.Append(" ");

                    int scDamage = 0;
                    int numSCs = 0;
                    int minSC = 0;
                    int maxSC = 0;
                    double avgSC = 0;


                    var scList = player.Where(b => b.DefenseType == (byte)DefenseType.None);

                    numSCs = scList.Count();
                    scDamage = scList.Sum(b => b.Amount);

                    minSC = scList.Min(b => b.Amount);
                    maxSC = scList.Max(b => b.Amount);

                    if (numSCs > 0)
                        avgSC = scList.Average(b => b.Amount);

                    //"Skillchain        Skill Dmg   # SC   SC.Low/Hi  SC.Avg"

                    sb.Append(scDamage.ToString().PadLeft(11));
                    sb.Append(numSCs.ToString().PadLeft(7));
                    sb.Append(string.Format("{0}/{1}", minSC, maxSC).PadLeft(12));
                    sb.Append(avgSC.ToString("F2").PadLeft(8));

                    sb.Append("\n");
                }
            }

            sb.Append("\n\n");
            AppendNormalText(sb.ToString());
        }

        private void ProcessOtherAttacks(IEnumerable<AttackGroup> otherAttacks)
        {
            if (otherAttacks == null)
                return;

            if (otherAttacks.Count() == 0)
                return;

            AppendBoldText("Other Damage\n", Color.Red);

            if (otherHeader == null)
                otherHeader = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10}\n",
                "Player".PadRight(16), "Other Dmg".PadLeft(10), "Total Dmg".PadLeft(10), "Other %".PadLeft(9),
                "Dmg Share %".PadLeft(12), "# Counterattacks".PadLeft(18), "C.Dmg".PadLeft(8), "Avg C.Dmg".PadLeft(10),
                "# Spikes".PadLeft(9), "Spk.Dmg".PadLeft(9), "Avg Spk.Dmg".PadLeft(12));

            AppendBoldUnderText(otherHeader, Color.Black);

            var counterAttacks = otherAttacks.FirstOrDefault(a => a.ActionSource == ActionType.Counterattack);
            var spikesAttacks = otherAttacks.FirstOrDefault(a => a.ActionSource == ActionType.Spikes);
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
