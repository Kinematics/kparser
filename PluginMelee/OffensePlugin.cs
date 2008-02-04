using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Drawing;

namespace WaywardGamers.KParser.Plugin
{
    internal class AttackGroup
    {
        internal ActionSourceType ActionSource { get; set; }
        internal IEnumerable<IGrouping<KPDatabaseDataSet.CombatantsRow, KPDatabaseDataSet.CombatDetailsRow>>
            CombatGroup { get; set; }

        public AttackGroup(ActionSourceType key,
            IEnumerable<IGrouping<KPDatabaseDataSet.CombatantsRow, KPDatabaseDataSet.CombatDetailsRow>> grouping)
        {
            ActionSource = key;
            CombatGroup = grouping;
        }
    }

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

            label1.Text = "Attack Type";
            comboBox1.Left = label1.Right + 10;
            comboBox1.Items.Clear();
            comboBox1.Items.Add("All");
            for (var action = ActionSourceType.Melee; action <= ActionSourceType.Weaponskill; action++)
            {
                comboBox1.Items.Add(action.ToString());
            }
            comboBox1.SelectedIndex = 0;

            label2.Enabled = false;
            comboBox2.Enabled = false;
            label2.Visible = false;
            comboBox2.Visible = false;
        }

        protected override bool FilterOnDatabaseChanging(DatabaseWatchEventArgs e, out KPDatabaseDataSet datasetToUse)
        {
            if (e.DatasetChanges.CombatDetails.Count != 0)
            {
                datasetToUse = e.FullDataset;
                return true;
            }

            datasetToUse = null;
            return false;
        }
        #endregion

        #region Member Variables
        //EnumerableRowCollection<KPDatabaseDataSet.CombatDetailsRow> Damage;
        int totalDamage;
        Dictionary<string, int> playerDamage = new Dictionary<string,int>();

        string meleeHeader;
        string rangeHeader;
        string spellHeader;
        string abilHeader;
        string wskillHeader;
        #endregion

        #region Processing sections
        protected override void ProcessData(KPDatabaseDataSet dataSet)
        {
            richTextBox.Clear();
            ActionSourceType actionSourceFilter = (ActionSourceType)comboBox1.SelectedIndex;

            var allAttacks = from cd in dataSet.CombatDetails
                             where ((cd.IsActorIDNull() == false) &&
                                    (cd.AttackType == (byte)AttackType.Damage) &&
                                    ((cd.CombatantsRowByCombatActorRelation.CombatantType == (byte)EntityType.Player) ||
                                     (cd.CombatantsRowByCombatActorRelation.CombatantType == (byte)EntityType.Pet) ||
                                     (cd.CombatantsRowByCombatActorRelation.CombatantType == (byte)EntityType.Fellow) ||
                                     (cd.CombatantsRowByCombatActorRelation.CombatantType == (byte)EntityType.Skillchain))
                                    )
                             group cd by cd.ActionSource into cda
                             select new AttackGroup(
                                 (ActionSourceType)cda.Key,
                                  from c in cda
                                  orderby c.CombatantsRowByCombatActorRelation.CombatantName
                                  group c by c.CombatantsRowByCombatActorRelation);


            totalDamage = 0;
            playerDamage.Clear();
            foreach (var attackTypes in allAttacks)
            {
                foreach (var player in attackTypes.CombatGroup)
                {
                    if (playerDamage.Keys.Contains(player.Key.CombatantName))
                        playerDamage[player.Key.CombatantName] += player.Sum(d => d.Damage);
                    else
                        playerDamage[player.Key.CombatantName] = player.Sum(d => d.Damage);
                }
            }

            foreach (var player in playerDamage)
                totalDamage += player.Value;


            var meleeAttacks = allAttacks.FirstOrDefault(m => m.ActionSource == ActionSourceType.Melee);
            var rangeAttacks = allAttacks.FirstOrDefault(m => m.ActionSource == ActionSourceType.Ranged);
            var spellAttacks = allAttacks.FirstOrDefault(m => m.ActionSource == ActionSourceType.Spell);
            var abilAttacks = allAttacks.FirstOrDefault(m => m.ActionSource == ActionSourceType.Ability);
            var wskillAttacks = allAttacks.FirstOrDefault(m => m.ActionSource == ActionSourceType.Weaponskill);

            switch (actionSourceFilter)
            {
                case ActionSourceType.Melee:
                    ProcessMeleeAttacks(meleeAttacks);
                    break;
                case ActionSourceType.Ranged:
                    ProcessRangedAttacks(rangeAttacks);
                    break;
                case ActionSourceType.Spell:
                    ProcessSpellsAttacks(spellAttacks);
                    break;
                case ActionSourceType.Ability:
                    ProcessAbilityAttacks(abilAttacks);
                    break;
                case ActionSourceType.Weaponskill:
                    ProcessWeaponskillAttackss(wskillAttacks);
                    break;
                default:
                    ProcessMeleeAttacks(meleeAttacks);
                    ProcessRangedAttacks(rangeAttacks);
                    ProcessSpellsAttacks(spellAttacks);
                    ProcessAbilityAttacks(abilAttacks);
                    ProcessWeaponskillAttackss(wskillAttacks);
                    break;
            }
        }

        private void ProcessMeleeAttacks(AttackGroup meleeAttacks)
        {
            if (meleeAttacks == null)
                return;

            if (meleeAttacks.CombatGroup.Count() == 0)
                return;

            AppendBoldText("Melee Damage\n", Color.Red);

            if (meleeHeader == null)
                meleeHeader = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14}\n",
                "Player".PadRight(16), "Melee Dmg".PadLeft(10), "Total Dmg".PadLeft(10), "Melee %".PadLeft(9),
                "Dmg Share %".PadLeft(12), "Hit/Miss".PadLeft(10), "Acc%".PadLeft(9), "M.Low/Hi".PadLeft(9),
                "M.Avg".PadLeft(8), "M.Avg0".PadLeft(8), "Effect".PadLeft(7), "#Crit".PadLeft(6),
                "C.Low/Hi".PadLeft(9), "C.Avg".PadLeft(7), "Crit%".PadLeft(9));

            AppendBoldUnderText(meleeHeader, Color.Black);


            foreach (var player in meleeAttacks.CombatGroup)
            {
                if (player.Count() > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append(player.Key.CombatantName.PadRight(16));
                    sb.Append(" ");

                    // Melee damage
                    int totalMelee = player.Sum(b => b.Damage);
                    sb.Append(totalMelee.ToString().PadLeft(10));
                    sb.Append(" ");

                    // Total damage
                    int damageDone = playerDamage[player.Key.CombatantName];
                    sb.Append(damageDone.ToString().PadLeft(10));
                    sb.Append(" ");

                    // Melee % of total player damage
                    sb.Append(((double)totalMelee / damageDone).ToString("P2").PadLeft(9));
                    sb.Append(" ");

                    // Damage share
                    sb.Append(((double)damageDone / totalDamage).ToString("P2").PadLeft(12));
                    sb.Append(" ");

                    var successfulHits = player.Where(h => h.SuccessLevel == (byte)SuccessType.Successful);
                    int hits = successfulHits.Count();
                    int misses = player.Count(b => b.SuccessLevel == (byte)SuccessType.Unsuccessful);

                    // Hits/Misses
                    sb.Append(string.Format("{0}/{1}", hits, misses).PadLeft(10));
                    sb.Append(" ");

                    // Accuracy
                    sb.Append(((double)hits / (hits + misses)).ToString("P2").PadLeft(9));
                    sb.Append(" ");

                    var normalHits = successfulHits.Where(h => h.DamageModifier == (byte)DamageModifier.None);
                    var critHits = successfulHits.Where(h => h.DamageModifier == (byte)DamageModifier.Critical);

                    if (hits > 0)
                    {
                        int low = 0;
                        int high = 0;

                        if (normalHits.Count() > 0)
                        {
                            // M.Low/Hi
                            low = normalHits.Min(b => b.Damage);
                            high = normalHits.Max(b => b.Damage);

                            sb.Append(string.Format("{0}/{1}", low, high).PadLeft(9));
                            sb.Append(" ");

                            int normDamage = normalHits.Sum(h => h.Damage);
                            // Melee avg
                            sb.Append(((double)normDamage / normalHits.Count()).ToString("F2").PadLeft(8));
                            sb.Append(" ");

                            var non0Hits = successfulHits.Where(m => m.Damage > 0);

                            // Melee non-0 avg
                            sb.Append(((double)totalMelee / non0Hits.Count()).ToString("F2").PadLeft(8));
                            sb.Append(" ");

                            int addDamageHits = player.Where(h => (h.SuccessLevel == (byte)SuccessType.Successful) &&
                                (h.ActionSource == (byte)ActionSourceType.AdditionalEffect)).Sum(b => b.Damage);

                            // Add. Effect damage
                            sb.Append(addDamageHits.ToString().PadLeft(7));
                            sb.Append(" ");
                        }
                        else
                        {
                            // Low/Hi
                            sb.Append("N/A".PadLeft(10));
                            sb.Append(" ");

                            // Melee avg
                            sb.Append("N/A".PadLeft(8));
                            sb.Append(" ");

                            // Melee non-0 avg
                            sb.Append("N/A".PadLeft(8));
                            sb.Append(" ");

                            // Add effect
                            sb.Append("N/A".PadLeft(7));
                            sb.Append(" ");
                        }

                        // Crit hits
                        int critCount = critHits.Count();
                        sb.Append(critCount.ToString().PadLeft(6));
                        sb.Append(" ");

                        // Crit low/high
                        if (critCount > 0)
                        {
                            // Crit low/high
                            low = critHits.Min(m => m.Damage);
                            high = critHits.Max(m => m.Damage);

                            sb.Append(string.Format("{0}/{1}", low, high).PadLeft(9));
                            sb.Append(" ");

                            // Crit avg
                            int critDamage = critHits.Sum(m => m.Damage);
                            sb.Append(((double)critDamage / critCount).ToString("F2").PadLeft(7));
                            sb.Append(" ");

                            // Crit %
                            sb.Append(((double)critCount / hits).ToString("P2").PadLeft(9));
                            sb.Append(" ");
                        }
                        else
                        {
                            // Crit low/high
                            sb.Append("N/A".PadLeft(9));
                            sb.Append(" ");

                            // Crit avg
                            sb.Append("N/A".PadLeft(7));
                            sb.Append(" ");

                            // Crit %
                            sb.Append(0.ToString("P2").PadLeft(10));
                            sb.Append(" ");
                        }
                    }
                    else
                    {
                        // Melee low/high
                        sb.Append("N/A".PadLeft(9));
                        sb.Append(" ");

                        // Melee avg
                        sb.Append("N/A".PadLeft(8));
                        sb.Append(" ");

                        // Melee non-0 avg
                        sb.Append("N/A".PadLeft(8));
                        sb.Append(" ");

                        // Add. Effect damage
                        sb.Append("N/A".PadLeft(7));
                        sb.Append(" ");

                        // Crit hits
                        sb.Append("N/A".PadLeft(6));
                        sb.Append(" ");

                        // Crit low/high
                        sb.Append("N/A".PadLeft(9));
                        sb.Append(" ");

                        // Crit avg
                        sb.Append("N/A".PadLeft(7));
                        sb.Append(" ");

                        // Crit %
                        sb.Append("N/A".PadLeft(9));
                        sb.Append(" ");
                    }

                    sb.Append("\n");
                    AppendNormalText(sb.ToString());
                }
            }

            AppendNormalText("\n\n");
        }

        private void ProcessRangedAttacks(AttackGroup rangeAttacks)
        {
            if (rangeAttacks == null)
                return;

            if (rangeAttacks.CombatGroup.Count() == 0)
                return;


            AppendBoldText("Ranged Damage\n", Color.Red);

            if (rangeHeader == null)
                rangeHeader = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14}\n",
                "Player".PadRight(16), "Range Dmg".PadLeft(10), "Total Dmg".PadLeft(10), "Range %".PadLeft(9),
                "Dmg Share %".PadLeft(12), "Hit/Miss".PadLeft(10), "Acc%".PadLeft(9), "R.Low/Hi".PadLeft(9),
                "R.Avg".PadLeft(8), "R.Avg0".PadLeft(8), "Effect".PadLeft(7),
                "#Crit".PadLeft(6), "C.Low/Hi".PadLeft(9), "C.Avg".PadLeft(7), "Crit%".PadLeft(9));

            AppendBoldUnderText(rangeHeader, Color.Black);


            foreach (var player in rangeAttacks.CombatGroup)
            {
                if (player.Count() > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append(player.Key.CombatantName.PadRight(16));
                    sb.Append(" ");

                    int damageDone = playerDamage[player.Key.CombatantName];
                    // Total damage
                    sb.Append(damageDone.ToString().PadRight(10));
                    sb.Append(" ");
                    // Damage share
                    sb.Append(((double)damageDone / totalDamage).ToString("P2").PadRight(9));
                    sb.Append(" ");
                    // Range damage
                    int totalRange = player.Sum(b => b.Damage);
                    sb.Append(totalRange.ToString().PadRight(8));

                    // Percent of player damage from ranged attacks
                    sb.Append(((double)totalRange / damageDone).ToString("P2").PadRight(8));

                    var successfulHits = player.Where(h => (h.SuccessLevel == (byte)SuccessType.Successful));
                    int hits = successfulHits.Count();
                    int misses = player.Count(b => b.SuccessLevel == (byte)SuccessType.Unsuccessful);

                    // Hits/Misses
                    sb.Append(string.Format("{0}/{1}", hits.ToString(), misses.ToString()).PadRight(9));
                    sb.Append(" ");

                    // Accuracy
                    sb.Append(((double)hits / (hits + misses)).ToString("P2").PadRight(8));
                    sb.Append(" ");

                    // R.Low/Hi

                    var normalHits = successfulHits.Where(h => h.DamageModifier == (byte)DamageModifier.None);
                    var critHits = successfulHits.Where(h => h.DamageModifier == (byte)DamageModifier.Critical);

                    if (hits > 0)
                    {
                        int low = 0;
                        int high = 0;

                        if (normalHits.Count() > 0)
                        {
                            low = normalHits.Min(b => b.Damage);
                            high = normalHits.Max(b => b.Damage);

                            // Range low/high
                            sb.Append(string.Format("{0}/{1}", low, high).PadRight(9));
                            sb.Append(" ");

                            // Range avg
                            int normDamage = normalHits.Sum(h => h.Damage);
                            sb.Append(((double)normDamage / normalHits.Count()).ToString("F2").PadRight(7));
                            sb.Append(" ");

                            var non0Hits = normalHits.Where(m => m.Damage > 0);

                            // Range non-0 avg
                            sb.Append(((double)totalRange / non0Hits.Count()).ToString("F2").PadRight(7));
                            sb.Append(" ");

                            int addDamageHits = player.Where(h => (h.SuccessLevel == (byte)SuccessType.Successful) &&
                                (h.ActionSource == (byte)ActionSourceType.AdditionalEffect)).Sum(b => b.Damage);

                            sb.Append(addDamageHits.ToString().PadRight(7));
                            sb.Append(" ");
                        }
                        else
                        {
                            // Low/Hi
                            sb.Append("N/A".PadRight(10));
                            sb.Append(" ");

                            // Melee avg
                            sb.Append("N/A".PadRight(8));
                            sb.Append(" ");

                            // Melee non-0 avg
                            sb.Append("N/A".PadRight(8));
                            sb.Append(" ");

                            // Add effect
                            sb.Append("N/A".PadRight(7));
                            sb.Append(" ");
                        }

                        // Crit hits
                        int critCount = critHits.Count();
                        sb.Append(critCount.ToString().PadRight(6));
                        sb.Append(" ");

                        if (critCount > 0)
                        {
                            // Crit low/high
                            low = critHits.Min(m => m.Damage);
                            high = critHits.Max(m => m.Damage);

                            sb.Append(string.Format("{0}/{1}", low, high).PadRight(9));
                            sb.Append(" ");

                            // Crit avg
                            int critDamage = critHits.Sum(m => m.Damage);
                            sb.Append(((double)critDamage / critCount).ToString("F2").PadRight(7));
                            sb.Append(" ");

                            // Crit %
                            sb.Append(((double)critCount / hits).ToString("P2").PadRight(10));
                            sb.Append(" ");
                        }
                        else
                        {
                            // Crit low/high
                            sb.Append("N/A".PadRight(9));
                            sb.Append(" ");

                            // Crit avg
                            sb.Append("N/A".PadRight(7));
                            sb.Append(" ");

                            // Crit %
                            sb.Append(0.ToString("P2").PadRight(10));
                            sb.Append(" ");
                        }
                    }
                    else
                    {
                        // Range low/high
                        sb.Append("N/A".PadRight(9));
                        sb.Append(" ");

                        // Range avg
                        sb.Append("N/A".PadRight(7));
                        sb.Append(" ");

                        // Range non-0 avg
                        sb.Append("N/A".PadRight(7));
                        sb.Append(" ");

                        sb.Append("N/A".PadRight(7));
                        sb.Append(" ");

                        // Crit hits
                        sb.Append("N/A".PadRight(6));
                        sb.Append(" ");

                        // Crit low/high
                        sb.Append("N/A".PadRight(9));
                        sb.Append(" ");

                        // Crit avg
                        sb.Append("N/A".PadRight(7));
                        sb.Append(" ");

                        // Crit %
                        sb.Append("N/A".PadRight(10));
                        sb.Append(" ");
                    }

                    sb.Append("\n");

                    AppendNormalText(sb.ToString());
                }
            }


            AppendNormalText("\n\n");
        }

        private void ProcessSpellsAttacks(AttackGroup spellAttacks)
        {
            if (spellAttacks == null)
                return;

            if (spellAttacks.CombatGroup.Count() == 0)
                return;


            AppendBoldText("Spell Damage\n", Color.Red);

            if (spellHeader == null)
                spellHeader = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10}\n",
                "Player".PadRight(16), "Spell Dmg".PadLeft(10), "Total Dmg".PadLeft(10), "Spell %".PadLeft(9),
                "Dmg Share %".PadLeft(11), "#Spells".PadLeft(8), "S.Low/Hi".PadLeft(9), "S.Avg".PadLeft(7),
                "#MagicBurst".PadLeft(12), "MB.Low/Hi".PadLeft(10), "MB.Avg".PadLeft(8));

            AppendBoldUnderText(spellHeader, Color.Black);

            foreach (var player in spellAttacks.CombatGroup)
            {
                if (player.Count() > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append(player.Key.CombatantName.PadRight(16));
                    sb.Append(" ");

                    // Spell damage
                    int spellDamage = player.Sum(b => b.Damage);
                    sb.Append(spellDamage.ToString().PadLeft(10));
                    sb.Append(" ");

                    // Total damage
                    int damageDone = playerDamage[player.Key.CombatantName];
                    sb.Append(damageDone.ToString().PadLeft(10));
                    sb.Append(" ");

                    // Percent of player damage from Spells
                    sb.Append(((double)spellDamage / damageDone).ToString("P2").PadLeft(9));
                    sb.Append(" ");

                    // Damage share
                    sb.Append(((double)damageDone / totalDamage).ToString("P2").PadLeft(12));
                    sb.Append(" ");

                    var spellsCast = player.Where(b => b.SuccessLevel == (byte)SuccessType.Successful);

                    var normSpells = spellsCast.Where(s => s.DamageModifier == (byte)DamageModifier.None);
                    var mbSpells = spellsCast.Where(s => s.DamageModifier == (byte)DamageModifier.MagicBurst);

                    int casts = spellsCast.Count();

                    if (casts > 0)
                    {
                        // # cast
                        sb.Append(casts.ToString().PadLeft(8));
                        sb.Append(" ");

                        // M.Low/Hi
                        int low = normSpells.Min(b => b.Damage);
                        int high = normSpells.Max(b => b.Damage);

                        sb.Append(string.Format("{0}/{1}", low, high).PadLeft(9));
                        sb.Append(" ");

                        // Spell avg
                        sb.Append(((double)normSpells.Sum(s => s.Damage) / normSpells.Count()).ToString("F2").PadLeft(7));
                        sb.Append(" ");

                        int mbCount = mbSpells.Count();

                        if (mbCount > 0)
                        {
                            // # MBs
                            sb.Append(mbCount.ToString().PadLeft(12));
                            sb.Append(" ");

                            // M.Low/Hi
                            low = mbSpells.Min(b => b.Damage);
                            high = mbSpells.Max(b => b.Damage);

                            sb.Append(string.Format("{0}/{1}", low, high).PadLeft(10));
                            sb.Append(" ");

                            // MB avg
                            sb.Append(((double)mbSpells.Sum(s => s.Damage) / mbSpells.Count()).ToString("F2").PadLeft(8));
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

            if (abilHeader == null)
                abilHeader = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}\n",
                "Player".PadRight(16), "Abil. Dmg".PadLeft(10), "Total Dmg".PadLeft(10), "Abil. %".PadLeft(9),
                "Dmg Share %".PadLeft(12), "Hit/Miss".PadLeft(10), "Acc%".PadLeft(9), "A.Low/Hi".PadLeft(9),
                "A.Avg".PadLeft(8), "A.Avg0".PadLeft(8));

            AppendBoldUnderText(abilHeader, Color.Black);

            foreach (var player in abilAttacks.CombatGroup)
            {
                if (player.Count() > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append(player.Key.CombatantName.PadRight(16));
                    sb.Append(" ");

                    // Ability damage
                    int abilityDmg = player.Sum(b => b.Damage);
                    sb.Append(abilityDmg.ToString().PadLeft(10));
                    sb.Append(" ");

                    // Total damage
                    int damageDone = playerDamage[player.Key.CombatantName];
                    sb.Append(damageDone.ToString().PadLeft(10));
                    sb.Append(" ");

                    // Percent of player damage from Abilities
                    sb.Append(((double)abilityDmg / damageDone).ToString("P2").PadLeft(9));
                    sb.Append(" ");

                    // Damage share
                    sb.Append(((double)damageDone / totalDamage).ToString("P2").PadLeft(12));
                    sb.Append(" ");


                    int hits = player.Count(b => b.SuccessLevel == (byte)SuccessType.Successful);
                    int misses = player.Count(b => b.SuccessLevel == (byte)SuccessType.Unsuccessful);

                    // Hits/Misses
                    sb.Append(string.Format("{0}/{1}", hits, misses).PadLeft(10));
                    sb.Append(" ");

                    // Accuracy
                    sb.Append(((double)hits / (hits + misses)).ToString("P2").PadLeft(9));
                    sb.Append(" ");

                    // A.Low/Hi
                    var successfulHits = player.Where(h => (h.SuccessLevel == (byte)SuccessType.Successful) &&
                        (h.DamageModifier == (byte)DamageModifier.None));

                    int successfulHitCount = successfulHits.Count();

                    if (successfulHitCount > 0)
                    {
                        int low = successfulHits.Min(b => b.Damage);
                        int high = successfulHits.Max(b => b.Damage);

                        // Ability low/high
                        sb.Append(string.Format("{0}/{1}", low, high).PadLeft(9));
                        sb.Append(" ");

                        // Ability avg
                        sb.Append(((double)abilityDmg / successfulHitCount).ToString("F2").PadLeft(8));
                        sb.Append(" ");

                        var non0Hits = successfulHits.Where(m => m.Damage > 0);
                        int non0HitCount = non0Hits.Count();

                        // Ability non-0 avg
                        if (non0HitCount > 0)
                            sb.Append(((double)abilityDmg / non0Hits.Count()).ToString("F2").PadLeft(8));
                        else
                            sb.Append("N/A".PadLeft(8));

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

                        // Ability non-0 avg
                        sb.Append("N/A".PadLeft(8));
                        sb.Append(" ");
                    }

                    sb.Append("\n");
                    AppendNormalText(sb.ToString());
                }
            }

            AppendNormalText("\n\n");
        }

        private void ProcessWeaponskillAttackss(AttackGroup wskillAttacks)
        {
            if (wskillAttacks == null)
                return;

            if (wskillAttacks.CombatGroup.Count() == 0)
                return;

            AppendBoldText("Weaponskill Damage\n", Color.Red);

            if (wskillHeader == null)
                wskillHeader = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}\n",
                "Player".PadRight(16), "WSkill Dmg".PadLeft(11), "Total Dmg".PadLeft(10), "WSkill %".PadLeft(9),
                "Dmg Share %".PadLeft(12), "Hit/Miss".PadLeft(10), "Acc%".PadLeft(9), "WS.Low/Hi".PadLeft(10),
                "WS.Avg".PadLeft(8), "WS.Avg0".PadLeft(8));

            AppendBoldUnderText(wskillHeader, Color.Black);

            foreach (var player in wskillAttacks.CombatGroup)
            {
                if (player.Count() > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.Append(player.Key.CombatantName.PadRight(16));
                    sb.Append(" ");

                    // Weaponskill damage
                    int wsDamage = player.Sum(b => b.Damage);
                    sb.Append(wsDamage.ToString().PadLeft(11));
                    sb.Append(" ");

                    // Total damage
                    int damageDone = playerDamage[player.Key.CombatantName];
                    sb.Append(damageDone.ToString().PadLeft(10));
                    sb.Append(" ");

                    // Percent of player damage from Weaponskills
                    sb.Append(((double)wsDamage / damageDone).ToString("P2").PadLeft(9));
                    sb.Append(" ");

                    // Damage share
                    sb.Append(((double)damageDone / totalDamage).ToString("P2").PadLeft(12));
                    sb.Append(" ");


                    int hits = player.Count(b => b.SuccessLevel == (byte)SuccessType.Successful);
                    int misses = player.Count(b => b.SuccessLevel == (byte)SuccessType.Unsuccessful);

                    // Hits/Misses
                    sb.Append(string.Format("{0}/{1}", hits, misses).PadLeft(10));
                    sb.Append(" ");

                    // Accuracy
                    sb.Append(((double)hits / (hits + misses)).ToString("P2").PadLeft(10));
                    sb.Append(" ");

                    // WS.Low/Hi
                    var successfulHits = player.Where(h => (h.SuccessLevel == (byte)SuccessType.Successful) &&
                        (h.DamageModifier == (byte)DamageModifier.None));

                    int successfulHitCount = successfulHits.Count();

                    if (successfulHitCount > 0)
                    {
                        int low = successfulHits.Min(b => b.Damage);
                        int high = successfulHits.Max(b => b.Damage);

                        // Weaponskill low/high
                        sb.Append(string.Format("{0}/{1}", low, high).PadLeft(9));
                        sb.Append(" ");

                        // Weaponskill avg
                        sb.Append(((double)wsDamage / successfulHitCount).ToString("F2").PadLeft(8));
                        sb.Append(" ");

                        var non0Hits = successfulHits.Where(m => m.Damage > 0);

                        // Weaponskill non-0 avg
                        sb.Append(((double)wsDamage / non0Hits.Count()).ToString("F2").PadLeft(8));
                        sb.Append(" ");
                    }
                    else
                    {
                        // Weaponskill low/high
                        sb.Append("N/A".PadLeft(9));
                        sb.Append(" ");

                        // Weaponskill avg
                        sb.Append("N/A".PadLeft(8));
                        sb.Append(" ");

                        // Weaponskill non-0 avg
                        sb.Append("N/A".PadLeft(8));
                        sb.Append(" ");
                    }

                    sb.Append("\n");
                    AppendNormalText(sb.ToString());
                }
            }

            AppendNormalText("\n\n");
        }

        #endregion

        #region Event Handlers
        protected override void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            HandleDataset(DatabaseManager.Instance.Database);
        }
        #endregion
    }
}
