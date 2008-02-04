using System;

namespace WaywardGamers.KParser
{
    #region Error Information
    public enum ErrorLevel
    {
        None = 0,		// To turn off error logging.
        Notify = 1,		// Exception is used for program flow control.
        Debug = 2,		// Exception provides debugging information.
        Info = 3,		// General info, but not an actual program error.
        Warning = 4,	// Possible problem, but could be worked around.
        Error = 5		// Program cannot proceed because of error.
    }
    #endregion

    public enum DataSource
    {
        Log,
        Ram
    }

    #region General message types
    public enum MessageCategoryType : byte
	{
		System,
		Chat,
		Action
	}

    public enum SystemMessageType : byte
    {
        Unknown,
        ZoneChange,
        Echo,
        EffectWearsOff,
        OutOfRange,
        Examine,
    }

    public enum ChatMessageType : byte
	{
		Unknown,
		Say,
		Shout,
        Party,
		Linkshell,
        Tell,
        Emote,
        NPC
	}

    public enum ActionMessageType : byte
    {
        Unknown,
        Combat,
        Experience,
        Loot,
        Fishing,
        Crafting,
        Other
    }
    #endregion

    #region Combat detail enumerations
    public enum CombatActionType : byte
    {
        Unknown,
        Attack,
        Buff,
        Death,
        Experience,
    }
    
    public enum AttackType : byte
    {
        Unknown,
        Damage,
        Enfeeble,
        Drain,
        Aspir,
    }

    public enum BuffType : byte
    {
        Unknown,
        Enhance,
        Recovery,
    }

    public enum RecoveryType : byte
    {
        None,
        RecoverHP,
        RecoverMP,
    }

    public enum ActionSourceType : byte
    {
        Unknown,
        Melee,
        Ranged,
        Spell,
        Ability,
        Weaponskill,
        AdditionalEffect,
        Counterattack,
        Spikes,
    }


    public enum FailedActionType : byte
    {
        None,
        Paralyzed,
        Interrupted,
        Intimidated,
        NotEnoughMP,
        NotEnoughTP,
        TooFarAway,
        OutOfRange,
        CannotSee,
        UnableToCast,
        UnableToUse,
    }

    public enum DamageModifier : byte
    {
        None,
        Critical,
        MagicBurst,
        Skillchain,
    }

    public enum DefenseType : byte
    {
        None,
        Evasion,
        Evade,
        Blink,
        Parry,
        Anticipate,
        Counter,
        Block,
        Guard,
        Resist,
        Intimidate,
    }

    public enum SuccessType : byte
    {
        Unknown,
        Failed,
        Unsuccessful,
        Successful,
    }

    public enum MobDifficulty : byte
    {
        Unknown,
        TooWeakToBeWorthwhile,
        EasyPrey,
        DecentChallenge,
        EvenMatch,
        Tough,
        VeryTough,
        IncrediblyTough,
        ImpossibleToGauge,
    }
    #endregion

    #region Player/Enemy enumerations
    public enum EntityType : byte
    {
        Unknown,
        Player,
        Pet,
        Mob,
        NPC,
        Fellow,
        Skillchain,
    }

    public enum SpeakerType : byte
    {
        Unknown,
        Self,
        Player,
        NPC,
        Other
    }
    #endregion
}
