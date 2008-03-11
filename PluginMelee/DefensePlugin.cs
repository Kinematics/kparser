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
            comboBox1.Items.Add("Attacks");
            comboBox1.Items.Add("Damage");
            comboBox1.Items.Add("Evasion");
            comboBox1.Items.Add("Other Defense");
            comboBox1.Items.Add("Utsusemi");
            comboBox1.SelectedIndex = 0;

            label2.Left = comboBox1.Right + 20;
            label2.Text = "Mob Group";
            comboBox2.Left = label2.Right + 10;
            comboBox2.Width = 150;
            comboBox2.Items.Clear();
            comboBox2.Items.Add("All");
            comboBox2.SelectedIndex = 0;

            checkBox1.Enabled = false;
            checkBox1.Visible = false;
            checkBox2.Enabled = false;
            checkBox2.Visible = false;
        }

        public override void DatabaseOpened(KPDatabaseDataSet dataSet)
        {
            ResetComboBox2();
            AddToComboBox2("All");
            ResetTextBox();

            if (dataSet.Battles.Count > 1)
            {
                var mobsKilled = from b in dataSet.Battles
                                 where ((b.DefaultBattle == false) &&
                                        (b.IsEnemyIDNull() == false) &&
                                        (b.CombatantsRowByEnemyCombatantRelation.CombatantType == (byte)EntityType.Mob))
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

                                // Check for existing entry with higher min base xp
                                mobWithXP = string.Format("{0} ({1})", mob.Name, xp.BaseXP + 1);

                                if (comboBox2.Items.Contains(mobWithXP))
                                    RemoveFromComboBox2(mobWithXP);
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
                                        (b.IsEnemyIDNull() == false) &&
                                        (b.CombatantsRowByEnemyCombatantRelation.CombatantType == (byte)EntityType.Mob))
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
                                mobWithXP = string.Format("{0} ({1})", mob.Name, xp.BaseXP + 1);

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
        Dictionary<string, int> playerDamage = new Dictionary<string, int>();

        string incAttacksHeader = "Player           Melee   Range   Abil/Ws   Spells   Unknown   Total   Attack# %   Avoided   Avoid %\n";
        string incDamageHeader  = "Player           M.Dmg   Avg M.Dmg   R.Dmg  Avg R.Dmg   S.Dmg  Avg S.Dmg   A/WS.Dmg  Avg A/WS.Dmg   Damage %\n";
        string evasionHeader    = "Player           M.Evade   M.Evade %   R.Evade   R.Evade %   Shadow   Shadow %\n";
        string otherDefHeader   = "Player           Parry   Parry %   Intimidate   Intimidate %   Anticipate  Anticipate %   Counter   Counter %\n";

        string utsuHeader       = "Player           Shadows Used   Ichi Cast  Ichi Fin  Ni Cast  Ni Fin   Count  Count(N)  Efficiency  Effic.(N)\n";
        #endregion

        #region Processing sections
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            richTextBox.Clear();

            IEnumerable<DefenseGroup> incAttacks;
            //IEnumerable<MobGroup> mobSet = null;

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
                // Attacks by all mobs

                incAttacks = from c in dataSet.Combatants
                             where ((c.CombatantType == (byte)EntityType.Player) ||
                                   (c.CombatantType == (byte)EntityType.Pet) ||
                                   (c.CombatantType == (byte)EntityType.Fellow))
                             orderby c.CombatantType, c.CombatantName
                             select new DefenseGroup
                             {
                                 Player = c.CombatantName,
                                 AllAttacks = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                              where ((da.HarmType == (byte)HarmType.Damage) ||
                                                     (da.HarmType == (byte)HarmType.Drain))
                                              select da,
                                 Melee = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                         where (((da.HarmType == (byte)HarmType.Damage) ||
                                                 (da.HarmType == (byte)HarmType.Drain)) &&
                                                ((da.ActionType == (byte)ActionType.Melee) ||
                                                 (da.ActionType == (byte)ActionType.Counterattack)))
                                         select da,
                                 Range = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                         where (((da.HarmType == (byte)HarmType.Damage) ||
                                                 (da.HarmType == (byte)HarmType.Drain)) &&
                                                (da.ActionType == (byte)ActionType.Ranged))
                                         select da,
                                 Abil = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                        where (((da.HarmType == (byte)HarmType.Damage) ||
                                                (da.HarmType == (byte)HarmType.Drain)) &&
                                               ((da.ActionType == (byte)ActionType.Ability) ||
                                                (da.ActionType == (byte)ActionType.Weaponskill)))
                                        select da,
                                 Spell = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                         where (((da.HarmType == (byte)HarmType.Damage) ||
                                                 (da.HarmType == (byte)HarmType.Drain)) &&
                                                (da.ActionType == (byte)ActionType.Spell))
                                         select da,
                                 Unknown = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                           where (((da.HarmType == (byte)HarmType.Damage) ||
                                                   (da.HarmType == (byte)HarmType.Drain)) &&
                                                  (da.ActionType == (byte)ActionType.Unknown))
                                           select da,
                             };


                /*
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
                                        select pd,
                                 Unknown = from pd in cdd
                                           where (pd.ActionType == (byte)ActionType.Unknown)
                                           select pd
                             };*/
            }
            else
            {
                if (xp > 0)
                {
                    // Attacks by a particular mob type of a given base xp

                    incAttacks = from c in dataSet.Combatants
                                 where ((c.CombatantType == (byte)EntityType.Player) ||
                                       (c.CombatantType == (byte)EntityType.Pet) ||
                                       (c.CombatantType == (byte)EntityType.Fellow))
                                 orderby c.CombatantType, c.CombatantName
                                 select new DefenseGroup
                                 {
                                     Player = c.CombatantName,
                                     AllAttacks = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                                  where ((da.HarmType == (byte)HarmType.Damage) ||
                                                         (da.HarmType == (byte)HarmType.Drain)) &&
                                                        (da.IsActorIDNull() == false &&
                                                         da.CombatantsRowByActorCombatantRelation.CombatantName == mobName &&
                                                         da.IsBattleIDNull() == false &&
                                                         da.BattlesRow.MinBaseExperience() == xp)
                                                  select da,
                                     Melee = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                             where (((da.HarmType == (byte)HarmType.Damage) ||
                                                     (da.HarmType == (byte)HarmType.Drain)) &&
                                                    ((da.ActionType == (byte)ActionType.Melee) ||
                                                     (da.ActionType == (byte)ActionType.Counterattack))) &&
                                                   (da.IsActorIDNull() == false &&
                                                    da.CombatantsRowByActorCombatantRelation.CombatantName == mobName &&
                                                    da.IsBattleIDNull() == false &&
                                                    da.BattlesRow.MinBaseExperience() == xp)
                                             select da,
                                     Range = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                             where (((da.HarmType == (byte)HarmType.Damage) ||
                                                     (da.HarmType == (byte)HarmType.Drain)) &&
                                                    (da.ActionType == (byte)ActionType.Ranged)) &&
                                                   (da.IsActorIDNull() == false &&
                                                    da.CombatantsRowByActorCombatantRelation.CombatantName == mobName &&
                                                    da.IsBattleIDNull() == false &&
                                                    da.BattlesRow.MinBaseExperience() == xp)
                                             select da,
                                     Abil = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                            where (((da.HarmType == (byte)HarmType.Damage) ||
                                                    (da.HarmType == (byte)HarmType.Drain)) &&
                                                   ((da.ActionType == (byte)ActionType.Ability) ||
                                                    (da.ActionType == (byte)ActionType.Weaponskill))) &&
                                                   (da.IsActorIDNull() == false &&
                                                    da.CombatantsRowByActorCombatantRelation.CombatantName == mobName &&
                                                    da.IsBattleIDNull() == false &&
                                                    da.BattlesRow.MinBaseExperience() == xp)
                                            select da,
                                     Spell = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                             where (((da.HarmType == (byte)HarmType.Damage) ||
                                                     (da.HarmType == (byte)HarmType.Drain)) &&
                                                    (da.ActionType == (byte)ActionType.Spell)) &&
                                                   (da.IsActorIDNull() == false &&
                                                    da.CombatantsRowByActorCombatantRelation.CombatantName == mobName &&
                                                    da.IsBattleIDNull() == false &&
                                                    da.BattlesRow.MinBaseExperience() == xp)
                                             select da,
                                     Unknown = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                               where (((da.HarmType == (byte)HarmType.Damage) ||
                                                       (da.HarmType == (byte)HarmType.Drain)) &&
                                                      (da.ActionType == (byte)ActionType.Unknown)) &&
                                                     (da.IsActorIDNull() == false &&
                                                      da.CombatantsRowByActorCombatantRelation.CombatantName == mobName &&
                                                      da.IsBattleIDNull() == false &&
                                                      da.BattlesRow.MinBaseExperience() == xp)
                                               select da,
                                 };



                    /*
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
                                                  where (pd.IsActorIDNull() == false &&
                                                        pd.CombatantsRowByActorCombatantRelation.CombatantName == mobName &&
                                                        pd.IsBattleIDNull() == false &&
                                                        pd.BattlesRow.MinBaseExperience() == xp)
                                                  select pd,
                                     Melee = from pd in cdd
                                             where (pd.ActionType == (byte)ActionType.Melee &&
                                                    pd.IsActorIDNull() == false &&
                                                    pd.CombatantsRowByActorCombatantRelation.CombatantName == mobName &&
                                                    pd.IsBattleIDNull() == false &&
                                                    pd.BattlesRow.MinBaseExperience() == xp)
                                             select pd,
                                     Range = from pd in cdd
                                             where (pd.ActionType == (byte)ActionType.Ranged &&
                                                    pd.IsActorIDNull() == false &&
                                                    pd.CombatantsRowByActorCombatantRelation.CombatantName == mobName &&
                                                    pd.IsBattleIDNull() == false &&
                                                    pd.BattlesRow.MinBaseExperience() == xp)
                                             select pd,
                                     Spell = from pd in cdd
                                             where (pd.ActionType == (byte)ActionType.Spell &&
                                                    pd.IsActorIDNull() == false &&
                                                    pd.CombatantsRowByActorCombatantRelation.CombatantName == mobName &&
                                                    pd.IsBattleIDNull() == false &&
                                                    pd.BattlesRow.MinBaseExperience() == xp)
                                             select pd,
                                     Abil = from pd in cdd
                                            where (((pd.ActionType == (byte)ActionType.Ability) ||
                                                    (pd.ActionType == (byte)ActionType.Weaponskill)) &&
                                                    pd.IsActorIDNull() == false &&
                                                    pd.CombatantsRowByActorCombatantRelation.CombatantName == mobName &&
                                                    pd.IsBattleIDNull() == false &&
                                                    pd.BattlesRow.MinBaseExperience() == xp)
                                            select pd,
                                     Unknown = from pd in cdd
                                               where (pd.ActionType == (byte)ActionType.Unknown &&
                                                    pd.IsActorIDNull() == false &&
                                                    pd.CombatantsRowByActorCombatantRelation.CombatantName == mobName &&
                                                    pd.IsBattleIDNull() == false &&
                                                    pd.BattlesRow.MinBaseExperience() == xp)
                                               select pd
                                 };*/
                }
                else
                {
                    // Attacks by a particular mob type


                    incAttacks = from c in dataSet.Combatants
                                 where ((c.CombatantType == (byte)EntityType.Player) ||
                                       (c.CombatantType == (byte)EntityType.Pet) ||
                                       (c.CombatantType == (byte)EntityType.Fellow))
                                 orderby c.CombatantType, c.CombatantName
                                 select new DefenseGroup
                                 {
                                     Player = c.CombatantName,
                                     AllAttacks = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                                  where ((da.HarmType == (byte)HarmType.Damage) ||
                                                         (da.HarmType == (byte)HarmType.Drain)) &&
                                                         (da.IsActorIDNull() == false &&
                                                          da.CombatantsRowByActorCombatantRelation.CombatantName == mobName)
                                                  select da,
                                     Melee = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                             where (((da.HarmType == (byte)HarmType.Damage) ||
                                                     (da.HarmType == (byte)HarmType.Drain)) &&
                                                    ((da.ActionType == (byte)ActionType.Melee) ||
                                                     (da.ActionType == (byte)ActionType.Counterattack))) &&
                                                   (da.IsActorIDNull() == false &&
                                                    da.CombatantsRowByActorCombatantRelation.CombatantName == mobName)
                                             select da,
                                     Range = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                             where (((da.HarmType == (byte)HarmType.Damage) ||
                                                     (da.HarmType == (byte)HarmType.Drain)) &&
                                                    (da.ActionType == (byte)ActionType.Ranged)) &&
                                                   (da.IsActorIDNull() == false &&
                                                    da.CombatantsRowByActorCombatantRelation.CombatantName == mobName)
                                             select da,
                                     Abil = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                            where (((da.HarmType == (byte)HarmType.Damage) ||
                                                    (da.HarmType == (byte)HarmType.Drain)) &&
                                                   ((da.ActionType == (byte)ActionType.Ability) ||
                                                    (da.ActionType == (byte)ActionType.Weaponskill))) &&
                                                   (da.IsActorIDNull() == false &&
                                                    da.CombatantsRowByActorCombatantRelation.CombatantName == mobName)
                                            select da,
                                     Spell = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                             where (((da.HarmType == (byte)HarmType.Damage) ||
                                                     (da.HarmType == (byte)HarmType.Drain)) &&
                                                    (da.ActionType == (byte)ActionType.Spell)) &&
                                                   (da.IsActorIDNull() == false &&
                                                    da.CombatantsRowByActorCombatantRelation.CombatantName == mobName)
                                             select da,
                                     Unknown = from da in c.GetInteractionsRowsByTargetCombatantRelation()
                                               where (((da.HarmType == (byte)HarmType.Damage) ||
                                                       (da.HarmType == (byte)HarmType.Drain)) &&
                                                      (da.ActionType == (byte)ActionType.Unknown)) &&
                                                     (da.IsActorIDNull() == false &&
                                                      da.CombatantsRowByActorCombatantRelation.CombatantName == mobName)
                                               select da,
                                 };


                    /*
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
                                                  where (pd.IsActorIDNull() == false &&
                                                        pd.CombatantsRowByActorCombatantRelation.CombatantName == mobName)
                                                  select pd,
                                     Melee = from pd in cdd
                                             where (pd.ActionType == (byte)ActionType.Melee &&
                                                    pd.IsActorIDNull() == false &&
                                                    pd.CombatantsRowByActorCombatantRelation.CombatantName == mobName)
                                             select pd,
                                     Range = from pd in cdd
                                             where (pd.ActionType == (byte)ActionType.Ranged &&
                                                    pd.IsActorIDNull() == false &&
                                                    pd.CombatantsRowByActorCombatantRelation.CombatantName == mobName)
                                             select pd,
                                     Spell = from pd in cdd
                                             where (pd.ActionType == (byte)ActionType.Spell &&
                                                    pd.IsActorIDNull() == false &&
                                                    pd.CombatantsRowByActorCombatantRelation.CombatantName == mobName)
                                             select pd,
                                     Abil = from pd in cdd
                                            where (((pd.ActionType == (byte)ActionType.Ability) ||
                                                    (pd.ActionType == (byte)ActionType.Weaponskill)) &&
                                                    pd.IsActorIDNull() == false &&
                                                    pd.CombatantsRowByActorCombatantRelation.CombatantName == mobName)
                                            select pd,
                                     Unknown = from pd in cdd
                                               where (pd.ActionType == (byte)ActionType.Unknown &&
                                                    pd.IsActorIDNull() == false &&
                                                    pd.CombatantsRowByActorCombatantRelation.CombatantName == mobName)
                                               select pd
                                 };*/
                }
            }
            #endregion

            if ((incAttacks != null) && (incAttacks.Count() > 0))
            {
                AppendBoldText("Defense\n\n", Color.Red);

                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        // All
                        ProcessDefenseAttacks(incAttacks);
                        ProcessDefenseDamage(incAttacks);
                        ProcessDefenseEvasion(incAttacks);
                        ProcessDefenseOther(incAttacks);
                        ProcessUtsusemi(dataSet);
                        break;
                    case 1:
                        // Attacks
                        ProcessDefenseAttacks(incAttacks);
                        break;
                    case 2:
                        // Damage
                        ProcessDefenseDamage(incAttacks);
                        break;
                    case 3:
                        // Evasion
                        ProcessDefenseEvasion(incAttacks);
                        break;
                    case 4:
                        // Other
                        ProcessDefenseOther(incAttacks);
                        break;
                    case 5:
                        // Utsusemi
                        ProcessUtsusemi(dataSet);
                        break;
                }

                AppendNormalText("\n");
            }
        }

        private void ProcessDefenseAttacks(IEnumerable<DefenseGroup> incAttacks)
        {
            if (incAttacks.Count() == 0)
                return;

            AppendBoldText("Attacks Against:\n", Color.Blue);
            AppendBoldUnderText(incAttacksHeader, Color.Black);

            StringBuilder sb = new StringBuilder();

            //"Player           Melee   Range   Abil/Ws   Spells   Unknown   Total   Attack# %   Avoided   Avoid %"

            int totalAttacks = incAttacks.Sum(b =>
                b.Melee.Count() + b.Range.Count() + b.Abil.Count() + b.Spell.Count() + b.Unknown.Count());

            foreach (var player in incAttacks)
            {
                int mHits = 0;
                int rHits = 0;
                int sHits = 0;
                int aHits = 0;
                int uHits = 0;
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
                if (player.Unknown != null)
                    uHits = player.Unknown.Count();

                incHits = mHits + rHits + aHits + sHits + uHits;

                avoidHits = player.AllAttacks.Count(h => h.DefenseType != (byte)DefenseType.None);

                if (incHits > 0)
                {
                    if (incHits > 0)
                        avoidPerc = (double)avoidHits / incHits;

                    if (totalAttacks > 0)
                        attackPerc = (double)incHits / totalAttacks;


                    sb.Append(player.Player.PadRight(17));

                    sb.AppendFormat("{0,5}{1,8}{2,10}{3,9}{4,10}{5,8}{6,12:p2}{7,10}{8,10:p2}\n",
                        mHits, rHits, aHits, sHits, uHits, incHits, attackPerc, avoidHits, avoidPerc);
                }
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
                AppendBoldText("Damage Against:\n", Color.Blue);
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

                        sb.AppendFormat("{0,5}{1,12:f2}{2,8}{3,11:f2}{4,8}{5,11:f2}{6,11}{7,14:f2}{8,11:p2}\n",
                            mDmg, mAvg, rDmg, rAvg, sDmg, sAvg, aDmg, aAvg, dmgPerc);
                    }
                }

                sb.Append("\n\n");
                AppendNormalText(sb.ToString());
            }
        }

        private void ProcessDefenseEvasion(IEnumerable<DefenseGroup> incAttacks)
        {
            bool headerPrinted = false;

            StringBuilder sb = new StringBuilder();

            foreach (var player in incAttacks)
            {
                if ((player.Melee.Count() + player.Range.Count()) > 0)
                {
                    int mEvaded = 0;
                    int rEvaded = 0;
                    int blinkedAttacks = 0;
                    double mEvadePerc = 0;
                    double rEvadePerc = 0;
                    double blinkPerc = 0;
                    
                    int mEvadableAttacks = player.Melee.Count() + player.Unknown.Count();
                    int rEvadableAttacks = player.Range.Count();

                    var blinkableAttacks = player.Melee.Concat(
                                           player.Range.Concat(
                                           player.Spell.Concat(
                                           player.Abil.Concat(
                                           player.Unknown)))).Where(a =>
                            a.DefenseType != (byte)DefenseType.Evasion &&
                            a.DefenseType != (byte)DefenseType.Parry &&
                            a.DefenseType != (byte)DefenseType.Intimidate);

                    int blinkableCount = blinkableAttacks.Count();


                    if (player.Melee.Count() > 0)
                    {
                        mEvaded = player.Melee.Count(h => h.DefenseType == (byte)DefenseType.Evasion);
                        mEvadePerc = (double)mEvaded / mEvadableAttacks;
                    }

                    if (player.Range.Count() > 0)
                    {
                        rEvaded = player.Range.Count(h => h.DefenseType == (byte)DefenseType.Evasion);
                        rEvadePerc = (double)rEvaded / rEvadableAttacks;
                    }

                    if (blinkableCount > 0)
                    {
                        blinkedAttacks = blinkableAttacks.Count(a => a.DefenseType == (byte)DefenseType.Shadow);
                        blinkPerc = (double)blinkedAttacks / blinkableCount;
                    }


                    if ((mEvaded + rEvaded + blinkedAttacks) > 0)
                    {
                        if (headerPrinted == false)
                        {
                            AppendBoldText("Evasion & Shadows\n", Color.Blue);
                            AppendBoldUnderText(evasionHeader, Color.Black);

                            headerPrinted = true;
                        }

                        sb.AppendFormat("{0,-17}{1,7}{2,12:p2}{3,10}{4,12:p2}{5,9}{6,11:p2}\n",
                            player.Player, mEvaded, mEvadePerc, rEvaded, rEvadePerc, blinkedAttacks, blinkPerc);
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

            StringBuilder sb = new StringBuilder();

            foreach (var player in incAttacks)
            {
                var parryableAttacks = player.Melee.Concat(
                                       player.Unknown).Where(a =>
                    a.DefenseType != (byte)DefenseType.Evasion);

                var anticableAttacks = player.Melee.Concat(
                                       player.Abil.Concat(
                                       player.Unknown)).Where(a =>
                    a.DefenseType != (byte)DefenseType.Evasion &&
                    a.DefenseType != (byte)DefenseType.Parry &&
                    a.DefenseType != (byte)DefenseType.Shadow &&
                    a.DefenseType != (byte)DefenseType.Intimidate);

                var counterableAttacks = player.Melee.Where(a =>
                    a.DefenseType != (byte)DefenseType.Evasion &&
                    a.DefenseType != (byte)DefenseType.Parry &&
                    a.DefenseType != (byte)DefenseType.Shadow &&
                    a.DefenseType != (byte)DefenseType.Intimidate).Concat(
                                         player.Unknown.Where(a =>
                                             a.DefenseType == (byte)DefenseType.Anticipate));

                var intimidateableAttacks = player.Melee.Concat(player.Unknown);

                int parryableCount = parryableAttacks.Count();
                int anticibleCount = anticableAttacks.Count();
                int counterableCount = counterableAttacks.Count();
                int intimidatableCount = intimidateableAttacks.Count();

                int parriedAttacks = 0;
                int anticipatedAttacks = 0;
                int counteredAttacks = 0;
                int intimidatedAttacks = 0;

                double parryPerc = 0;
                double antiPerc = 0;
                double counterPerc = 0;
                double intimidatedPerc = 0;


                if ((parryableCount + intimidatableCount + anticibleCount + counterableCount) > 0)
                {
                    if (parryableCount > 0)
                    {
                        parriedAttacks = parryableAttacks.Count(a => a.DefenseType == (byte)DefenseType.Parry);
                        parryPerc = (double)parriedAttacks / parryableCount;
                    }

                    if (intimidatableCount > 0)
                    {
                        intimidatedAttacks = intimidateableAttacks.Count(a => a.DefenseType == (byte)DefenseType.Intimidate);
                        intimidatedPerc = (double)intimidatedAttacks / intimidatableCount;
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


                    if ((parriedAttacks + intimidatedAttacks + anticipatedAttacks + counteredAttacks) > 0)
                    {
                        if (headerPrinted == false)
                        {
                            AppendBoldText("Other Defenses\n", Color.Blue);
                            AppendBoldUnderText(otherDefHeader, Color.Black);
                            headerPrinted = true;
                        }

                        sb.Append(player.Player.PadRight(17));

                        sb.Append(parriedAttacks.ToString().PadLeft(5));
                        sb.Append(parryPerc.ToString("P2").PadLeft(10));
                        sb.Append(intimidatedAttacks.ToString().PadLeft(13));
                        sb.Append(intimidatedPerc.ToString("P2").PadLeft(15));
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

        private void ProcessUtsusemi(KPDatabaseDataSet dataSet)
        {
            var utsu1 = dataSet.Actions.FirstOrDefault(a => a.ActionName == "Utsusemi: Ichi");
            var utsu2 = dataSet.Actions.FirstOrDefault(a => a.ActionName == "Utsusemi: Ni");

            if ((utsu1 == null) && (utsu2 == null))
                return;

            KPDatabaseDataSet.InteractionsRow[] utsu1Rows;
            KPDatabaseDataSet.InteractionsRow[] utsu2Rows;

            if (utsu1 != null)
                utsu1Rows = utsu1.GetInteractionsRows();
            else
                utsu1Rows = new KPDatabaseDataSet.InteractionsRow[0];

            if (utsu2 != null)
                utsu2Rows = utsu2.GetInteractionsRows();
            else
                utsu2Rows = new KPDatabaseDataSet.InteractionsRow[0];

            var utsuByPlayer = from c in dataSet.Combatants
                               where c.CombatantType == (byte)EntityType.Player
                               orderby c.CombatantName
                               select new
                               {
                                   Player = c.CombatantName,
                                   ShadowsUsed = from uc in c.GetInteractionsRowsByTargetCombatantRelation()
                                                 where ((uc.DefenseType == (byte)DefenseType.Shadow) &&
                                                        (uc.ShadowsUsed > 0))
                                                 select uc,
                                   UtsuIchi = from i in utsu1Rows
                                              where (i.CombatantsRowByActorCombatantRelation == c)
                                              select i,
                                   UtsuNi = from i in utsu2Rows
                                            where (i.CombatantsRowByActorCombatantRelation == c)
                                            select i,
                               };


            int shadsUsed;
            int ichiCast;
            int niCast;
            int ichiFin;
            int niFin;
            int numShads;
            int numShadsN;
            double effNorm;
            double effNin;


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
                        sb.Append(numShads.ToString().PadLeft(8));
                        sb.Append(numShadsN.ToString().PadLeft(10));
                        sb.Append(effNorm.ToString("P2").PadLeft(12));
                        sb.Append(effNin.ToString("P2").PadLeft(11));

                        sb.Append("\n");
                    }
                }

                sb.Append("\n");
                AppendNormalText(sb.ToString());
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
