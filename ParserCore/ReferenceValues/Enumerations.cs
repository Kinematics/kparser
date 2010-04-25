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
        Ram,
        Database,
    }

    public enum ImportSourceType
    {
        KParser,
        DVSParse,
        DirectParse,
    }

    public enum ImportMode
    {
        Import,
        Reparse,
    }

    #region General message types
    public enum MessageCategoryType : byte
	{
		System,
		Chat,
		Event
	}

    public enum SystemMessageType : byte
    {
        Unknown,
        ZoneChange,
        Echo,
        EffectWearsOff,
        OutOfRange,
        Examine,
        SearchComment,
        ReuseTime,
        ConquestUpdate,
        CommandError,
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
        NPC,
        Arena,
        Echo,
    }

    public enum EventMessageType : byte
    {
        Unknown,
        Interaction,
        EndBattle,  // chain #, xp, loot, gains a level
        Experience,
        Loot,
        Steal, // Interaction + Loot
        Fishing,
        Crafting,
        Other
    }

    public enum LootType : byte
    {
        Unknown,
        Drop,
        Steal,
        Chest,
    }
    #endregion

    #region Combat detail enumerations
    public enum InteractionType : byte
    {
        Unknown,
        Aid,
        Harm,
        Death,
    }

    public enum SuccessType : byte
    {
        None,
        Unknown,
        Failed,
        Unsuccessful,
        Successful,
    }

    public enum HarmType : byte
    {
        None,
        Unknown,
        Damage,
        Enfeeble,
        Drain,
        Aspir,
        Dispel,
    }

    public enum AidType : byte
    {
        None,
        Unknown,
        Enhance,
        Recovery,
        Item,
        RemoveStatus,
        RemoveEnmity,
    }

    public enum RecoveryType : byte
    {
        None,
        RecoverHP,
        RecoverMP,
        RecoverTP,
        Life,
    }

    public enum DamageType : byte
    {
        Unknown,
        Physical,
        Magical,
        AdditionalEffect,
        Spikes,
        Counterattack,
    }

    public enum ActionType : byte
    {
        Unknown,
        Melee,
        Ranged,
        Spell,
        Ability,
        Weaponskill,
        Skillchain,
        AdditionalEffect,
        Counterattack,
        Spikes,
        Steal,
        Retaliation,
        Death
    }


    public enum FailedActionType : byte
    {
        None,
        NoEffect,
        OutOfRange,
        TooFarAway,
        CannotSee,
        CannotAttack,
        UnableToCast,
        UnableToUse,
        Interrupted,
        Intimidated,
        FailedToActivate,
        Paralyzed,
        NotEnoughMP,
        NotEnoughTP,
        Autotarget,
        CannotAct,
        Overloaded,
        Discovered,
        MoveInterrupt,
    }

    public enum DamageModifier : byte
    {
        None,
        Critical,
        MagicBurst,
    }

    public enum DefenseType : byte
    {
        None,
        Evasion,
        Evade,
        Shadow,
        Parry,
        Anticipate,
        Counter,
        Block,
        Guard,
        Resist,
        Intimidate,
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
        TreasureChest,
        CharmedPlayer,
        CharmedMob,
    }

    [Flags]
    public enum ActorType : byte
    {
        Unknown = 0x00,
        Self = 0x01,
        Party = 0x02,
        Other = 0x04,
        Pet = 0x08,
        NPC = 0x10,
    }

    public enum ActorPlayerType : byte
    {
        None,
        Unknown,
        Self,
        Party,
        Alliance,
        Other
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
