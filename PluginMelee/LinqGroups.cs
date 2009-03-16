using System;
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

    /// <summary>
    /// Class to handle a LINQ query result so that it can be passed
    /// as a function argument.
    /// </summary>
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

    /// <summary>
    /// Class to handle a LINQ query result so that it can be passed
    /// as a function argument.
    /// </summary>
    internal class DefenseGroup2
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
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Countered { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Retaliate { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Retaliated { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Spikes { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> Unknown { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> UtsuIchiCast { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> UtsuIchiFinish { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> UtsuNiCast { get; set; }
        internal IEnumerable<KPDatabaseDataSet.InteractionsRow> UtsuNiFinish { get; set; }

        public DefenseGroup2()
        {
        }

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
                    Spikes.Count() +
                    Unknown.Count();
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

    #region Accumulator classes
    class MainAccumulator
    {
        internal MainAccumulator()
        {
            Abilities = new List<AbilAccum>();
            Weaponskills = new List<WSAccum>();
            Spells = new List<SpellAccum>();
        }

        internal List<AbilAccum> Abilities { get; private set; }
        internal List<WSAccum> Weaponskills { get; private set; }
        internal List<SpellAccum> Spells { get; private set; }

        internal string Name { get; set; }
        internal EntityType CType { get; set; }

        internal int TDmg { get; set; }
        internal int TMDmg { get; set; }
        internal int TRDmg { get; set; }
        internal int TADmg { get; set; }
        internal int TWDmg { get; set; }
        internal int TSDmg { get; set; }
        internal int TODmg { get; set; }
        internal int TSCDmg { get; set; }
        internal int MHits { get; set; }
        internal int MMiss { get; set; }
        internal int MNonCritDmg { get; set; }
        internal int MLow { get; set; }
        internal int MHi { get; set; }
        internal int MCritHits { get; set; }
        internal int MCritDmg { get; set; }
        internal int MCritLow { get; set; }
        internal int MCritHi { get; set; }
        internal int RHits { get; set; }
        internal int RMiss { get; set; }
        internal int RNonCritDmg { get; set; }
        internal int RLow { get; set; }
        internal int RHi { get; set; }
        internal int RCritHits { get; set; }
        internal int RCritDmg { get; set; }
        internal int RCritLow { get; set; }
        internal int RCritHi { get; set; }
        internal int SCNum { get; set; }
        internal int SCLow { get; set; }
        internal int SCHi { get; set; }
        internal int MAEDmg { get; set; }
        internal int MAENum { get; set; }
        internal int RAEDmg { get; set; }
        internal int RAENum { get; set; }
        internal int SpkDmg { get; set; }
        internal int SpkNum { get; set; }
        internal int CADmg { get; set; }
        internal int CAHits { get; set; }
        internal int CAMiss { get; set; }
        internal int CALow { get; set; }
        internal int CAHi { get; set; }
        internal int RTDmg { get; set; }
        internal int RTHits { get; set; }
        internal int RTMiss { get; set; }
        internal int RTLow { get; set; }
        internal int RTHi { get; set; }
        internal int DefEvasion { get; set; }
        internal int DefParry { get; set; }
        internal int DefShadow { get; set; }
        internal int DefAnticipate { get; set; }
        internal int DefIntimidate { get; set; }
        internal int DefCounter { get; set; }
        internal int DefRetaliate { get; set; }
        internal int DefResist { get; set; }
        internal int UtsuUsed { get; set; }
        internal int UtsuICast { get; set; }
        internal int UtsuIFin { get; set; }
        internal int UtsuNCast { get; set; }
        internal int UtsuNFin { get; set; }
    }

    class AbilAccum
    {
        internal string AName { get; set; }

        internal int ADmg { get; set; }
        internal int AHit { get; set; }
        internal int AMiss { get; set; }
        internal int ALow { get; set; }
        internal int AHi { get; set; }
    }

    class WSAccum
    {
        internal string WName { get; set; }

        internal int WDmg { get; set; }
        internal int WHit { get; set; }
        internal int WMiss { get; set; }
        internal int WLow { get; set; }
        internal int WHi { get; set; }
    }

    class SpellAccum
    {
        internal string SName { get; set; }

        internal int SDmg { get; set; }
        internal int SNum { get; set; }
        internal int SNumMB { get; set; }
        internal int SFail { get; set; }
        internal int SLow { get; set; }
        internal int SHi { get; set; }
        internal int SMBDmg { get; set; }
        internal int SMBLow { get; set; }
        internal int SMBHi { get; set; }
    }
    #endregion

}
