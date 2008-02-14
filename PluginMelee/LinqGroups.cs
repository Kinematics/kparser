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
        internal string Player { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Melee { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Range { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Spell { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Ability { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> WSkill { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> SC { get; set; }
        internal int MeleeDmg
        {
            get
            {
                return Melee.Sum(d => d.Amount);
            }
        }

        internal int MeleeEffectDmg
        {
            get
            {
                return Melee.Where(s =>
                           s.SecondHarmType == (byte)HarmType.Damage ||
                           s.SecondHarmType == (byte)HarmType.Drain).Sum(d => d.SecondAmount);
            }
        }

        internal int RangeDmg
        {
            get
            {
                return Range.Sum(d => d.Amount);
            }
        }

        internal int RangeEffectDmg
        {
            get
            {
                return Range.Where(s =>
                           s.SecondHarmType == (byte)HarmType.Damage ||
                           s.SecondHarmType == (byte)HarmType.Drain).Sum(d => d.SecondAmount);
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

        public AttackGroup()
        {
        }
    }


    internal class DefenseGroup
    {
        internal string Player { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> AllAttacks { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Melee { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Range { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Spell { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Abil { get; set; }

        public DefenseGroup()
        {
        }

        public DefenseGroup(string player,
            IEnumerable<KPDatabaseDataSet.InteractionsRow> allAtt,
            IEnumerable<KPDatabaseDataSet.InteractionsRow> melee,
            IEnumerable<KPDatabaseDataSet.InteractionsRow> range,
            IEnumerable<KPDatabaseDataSet.InteractionsRow> spell,
            IEnumerable<KPDatabaseDataSet.InteractionsRow> abil)
        {
            Player = player;
            AllAttacks = allAtt;
            Melee = melee;
            Range = range;
            Spell = spell;
            Abil = abil;
        }
    }
}
