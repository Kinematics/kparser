﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;

namespace WaywardGamers.KParser.Plugin
{
    /// <summary>
    /// Class to handle a LINQ query result so that it can be passed
    /// as a function argument.
    /// </summary>
    internal class AttackGroup
    {
        internal string Name { get; set; }
        internal EntityType ComType { get; set; }
        internal int BaseXP { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Melee { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Range { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Spell { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Ability { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> WSkill { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> SC { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Counter { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Retaliate { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Spikes { get; set; }

        internal int TotalActions
        {
            get
            {
                return Melee.Count() +
                    Range.Count() +
                    Spell.Count() +
                    Ability.Count() +
                    WSkill.Count() +
                    Counter.Count() +
                    Retaliate.Count() +
                    Spikes.Count();
            }
        }

        internal int MeleeDmg
        {
            get
            {
                return Melee.Sum(d => d.Amount);
            }
        }

        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> MeleeEffect
        {
            get
            {
                return Melee.Where(s =>
                           s.SecondHarmType == (byte)HarmType.Damage ||
                           s.SecondHarmType == (byte)HarmType.Drain);
            }
        }

        internal int RangeDmg
        {
            get
            {
                return Range.Sum(d => d.Amount);
            }
        }

        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> RangeEffect
        {
            get
            {
                return Range.Where(s =>
                           s.SecondHarmType == (byte)HarmType.Damage ||
                           s.SecondHarmType == (byte)HarmType.Drain);
            }
        }

        internal int SpellDmg
        {
            get
            {
                return Spell.Sum(d => d.Amount);
            }
        }

        internal int AbilityDmg
        {
            get
            {
                return Ability.Sum(d => d.Amount);
            }
        }

        internal int WSkillDmg
        {
            get
            {
                return WSkill.Sum(d => d.Amount);
            }
        }

        internal int SCDmg
        {
            get
            {
                return SC.Sum(d => d.Amount);
            }
        }

        internal int CounterDmg
        {
            get
            {
                return Counter.Sum(d => d.Amount);
            }
        }

        internal int RetaliateDmg
        {
            get
            {
                return Retaliate.Sum(d => d.Amount);
            }
        }

        internal int SpikesDmg
        {
            get
            {
                return Spikes.Sum(d => d.Amount);
            }
        }

        public AttackGroup()
        {
        }
    }

    internal class DefenseGroup
    {
        internal string Name { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> AllAttacks { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Melee { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Range { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Spell { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Abil { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Unknown { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Retaliations { get; set; }

        public DefenseGroup()
        {
        }
    }

    internal class MobGroup
    {
        internal string Mob { get; set; }
        internal IOrderedEnumerable<IGrouping<int, KPDatabaseDataSet.BattlesRow>> Battles { get; set; }

        public MobGroup()
        {
        }
    }

    internal class DebuffGroup
    {
        internal string DebufferName { get; set; }
        internal IEnumerable<Debuffs> Debuffs { get; set; }

        public DebuffGroup()
        {
        }
    }

    internal class Debuffs
    {
        internal string DebuffName { get; set; }
        internal IEnumerable<DebuffTargets> DebuffTargets { get; set; }

        public Debuffs()
        {
        }
    }

    internal class DebuffTargets
    {
        internal string TargetName { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> DebuffData { get; set; }

        public DebuffTargets()
        {
        }
    }
}
