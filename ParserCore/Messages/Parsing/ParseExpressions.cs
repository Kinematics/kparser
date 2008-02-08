using System;
using System.Text.RegularExpressions;

namespace WaywardGamers.KParser
{
    // Field names used in the named substrings of parse expressions.
    public class ParseFields
    {
        public static readonly string Name       = "name";
        public static readonly string Fullname   = "fullname";
        public static readonly string RepeatName = "repeatname";
        public static readonly string Target     = "target";
        public static readonly string Fulltarget = "fulltarget";
        public static readonly string Damage     = "damage";
        public static readonly string Number     = "number";
        public static readonly string Item       = "item";
        public static readonly string Money      = "money";
        public static readonly string Spell      = "spell";
        public static readonly string Ability    = "ability";
        public static readonly string Effect     = "effect";
        public static readonly string SC         = "skillchain";
    }

    // Class to store regular expressions in.
    public class ParseExpressions
    {
        #region Named substrings
        private static readonly string playerName  = @"(?<name>\w{3,16})";
        private static readonly string name        = @"(?<fullname>([Tt]he )?(?<name>\w+(['\- ](\d|\w)+)*))";
        private static readonly string repeatname  = @"(([Tt]he )?(?<repeatname>\w+(['\- ](\d|\w)+)*))";
        private static readonly string target      = @"(?<fulltarget>([Tt]he )?(?<target>\w+(['\- ](\d|\w)+)*))";
        private static readonly string damage      = @"(?<damage>\d{1,4})";
        private static readonly string number      = @"(?<number>\d{1,4})";
        private static readonly string item        = @"(([Aa]|[Aa]n|[Tt]he) )?(?<item>.{3,})";
        private static readonly string money       = @"((?<money>\d{1,4}?) gil)";
        private static readonly string spell       = @"(?<spell>\w+((\:)? \w+)?)";
        private static readonly string ability     = @"(?<ability>\w+((\:)? \w+)?)";
        private static readonly string effect      = @"(?<effect>\w+( \w+)?)";
        private static readonly string skillchain  = @"(?<skillchain>\w+)";
        #endregion

        #region Plugin corrections
        public static readonly Regex timestampPlugin = new Regex(@"^\[\d{2}:\d{2}:\d{2}\] (.*)$");
        #endregion

        #region Chat name extractions
        public static readonly Regex ChatSay       = new Regex(string.Format("^{0} : (.+)$", playerName));
        public static readonly Regex ChatParty     = new Regex(string.Format("^\\({0}\\) (.+)$", playerName));
        public static readonly Regex ChatTell      = new Regex(string.Format("^(>>)?{0}(>>)? (.+)$", playerName));
        public static readonly Regex ChatTellFrom  = new Regex(string.Format("^{0}>> (.+)$", playerName));
        public static readonly Regex ChatTellTo    = new Regex(string.Format("^>>{0} (.+)$", playerName));
        public static readonly Regex ChatShout     = new Regex(string.Format("^{0} : (.+)$", playerName));
        public static readonly Regex ChatLinkshell = new Regex(string.Format("^<{0}> (.+)$", playerName));
        public static readonly Regex ChatEmote     = new Regex(string.Format("^{0} (.+)$", playerName));
        public static readonly Regex ChatEmoteA    = new Regex(string.Format("^{0}'s (.+)$", playerName));
        public static readonly Regex ChatNPC       = new Regex(string.Format("^{0} (.+)$", name));
        #endregion

        #region Loot
        public static readonly Regex FindLoot = new Regex(string.Format("^You find {0} on {1}\\.$", item, target));
        public static readonly Regex GetLoot  = new Regex(string.Format("^{0} obtains {1}\\.$", playerName, item));
        public static readonly Regex GetGil   = new Regex(string.Format("^{0} obtains {1}\\.$", playerName, money));
        public static readonly Regex LootReqr = new Regex(string.Format("^You do not meet the requirements to obtain {0}\\.$", item));
        public static readonly Regex LootLost = new Regex(string.Format("^{0} lost\\.$", item));
        #endregion

        #region Name Tests
        // If any of the specified characters occur in the name, it should be a mob type (may possibly be a puppet).
        // Otherwise have to check against the Avatar/Wyvern/Puppet name lists.
        public static readonly Regex MobNameTest = new Regex(@"['\- \d]");
        // Bst jug pets are named either as "CrabFamiliar" (all one word) or "CourierCarrie" (all one word).
        public static readonly Regex BstJugPetName = new Regex(@"(^\w+Familiar$)|(^[A-Z][a-z]+[A-Z][a-z]+$)");
        #endregion


        #region Preparing to take action
        public static readonly Regex PrepSpell    = new Regex(string.Format("^{0} starts casting {1}\\.$", name, spell));
        public static readonly Regex PrepSpellOn  = new Regex(string.Format("^{0} starts casting {1} on {2}\\.$", name, spell, target));
        public static readonly Regex PrepAbility  = new Regex(string.Format("^{0} readies {1}\\.$", name, ability));
        #endregion

        #region Completes action
        public static readonly Regex CastSpell    = new Regex(string.Format("^{0} casts {1}\\.$", name, spell));
        public static readonly Regex UseAbility   = new Regex(string.Format("^{0} uses {1}\\.$", name, ability));
        public static readonly Regex UseAbilityOn = new Regex(string.Format("^{0} uses {1} on {2}\\.$", name, ability, target));
        public static readonly Regex MissAbility  = new Regex(string.Format("^{0} uses {1}, but misses {2}\\.$", name, ability, target));
        public static readonly Regex FailsCharm   = new Regex(string.Format("^{0} fails to charm {1}\\.$", name, target));
        #endregion

        #region Spell/Ability Effects
        public static readonly Regex RecoversHP         = new Regex(string.Format("^{0} recovers {1} HP\\.$", target, number));
        public static readonly Regex RecoversMP         = new Regex(string.Format("^{0} recovers {1} MP\\.$", target, number));
        public static readonly Regex Enfeeble           = new Regex(string.Format("{0} is {1}\\.$", target, effect));
        public static readonly Regex Buff       = new Regex(string.Format("^{0} gains the effect of {1}\\.$", target, effect));
        public static readonly Regex Debuff     = new Regex(string.Format("^{0} receives the effect of {1}\\.$", target, effect));
        public static readonly Regex Enhance    = new Regex(string.Format("^{0}'s attacks are enhanced\\.$", target));
        public static readonly Regex Charmed    = new Regex(string.Format("^{0} is now under {1}'s control\\.$", target, name));
        public static readonly Regex NotCharmed = new Regex(string.Format("^{0} is no longer charmed\\.$", target));
        #endregion

        #region Failed Actions
        public static readonly Regex Interrupted = new Regex(string.Format("^{0}'s casting is interrupted\\.$", name));
        public static readonly Regex Paralyzed   = new Regex(string.Format("^{0} is paralyzed\\.$", name));
        public static readonly Regex CannotSee   = new Regex(string.Format("^Unable to see {0}\\.$", target));
        public static readonly Regex TooFarAway  = new Regex(string.Format("^{0} is too far away\\.$", target));
        public static readonly Regex OutOfRange  = new Regex(string.Format("^{0} is out of range\\.$", target));
        public static readonly Regex NotEnoughTP = new Regex(string.Format("^You do not have enough TP\\.$", target));
        public static readonly Regex NotEnoughMP = new Regex(string.Format("^You do not have enough MP\\.$", target));
        public static readonly Regex Intimidated = new Regex(string.Format("^{0} is intimidated by {1}'s presence\\.$", name, target));
        public static readonly Regex UnableToCast = new Regex(string.Format("^Unable to cast spells at this time\\.$"));
        public static readonly Regex UnableToUse = new Regex(string.Format("^Unable to use job ability\\.$"));
        public static readonly Regex NoEffect    = new Regex(string.Format("^{0}'s {1} has no effect on {2}\\.$", name, spell, target));
        #endregion

        #region Modifiers on existing lines
        public static readonly Regex AdditionalEffect = new Regex(@"^Additional effect:");
        public static readonly Regex MagicBurst       = new Regex(@"^Magic Burst!");
        #endregion

        #region Combat damage
        public static readonly Regex MeleeHit          = new Regex(string.Format("^{0} hits {1} for {2} point(s)? of damage\\.$", name, target, damage));
        public static readonly Regex RangedAttack      = new Regex(string.Format("^{0}'s ranged attack hits {1} for {2} point(s)? of damage\\.$", name, target, damage));
        public static readonly Regex RangedHit         = new Regex(string.Format("^{0} uses Ranged Attack\\.$", name));
        public static readonly Regex CriticalHit       = new Regex(string.Format("^{0} scores a critical hit!$", name));
        public static readonly Regex RangedCriticalHit = new Regex(string.Format("^{0}'s ranged attack scores a critical hit!$", name));
        public static readonly Regex TargetTakesDamage = new Regex(string.Format("{0} takes {1}( additional)? point(s)? of damage\\.$", target, damage));
        public static readonly Regex Spikes            = new Regex(string.Format("{0}'s spikes deal {1} point(s)? of damage to {2}\\.", name, damage, target));
        public static readonly Regex Skillchain        = new Regex(string.Format("^Skillchain: {0}\\.$", skillchain));
        #endregion

        #region Combat defenses
        public static readonly Regex MeleeMiss    = new Regex(string.Format("^{0} misses {1}\\.$", name, target));
        public static readonly Regex RangedMiss   = new Regex(string.Format("^{0} uses Ranged Attack, but misses {1}\\.$", name, target));
        public static readonly Regex Blink        = new Regex(string.Format("^{0} of {1}'s shadows absorbs the damage and disappears\\.$", number, target));
        public static readonly Regex Parry        = new Regex(string.Format("^{0} parries {1}'s attack with (his|her|its) weapon\\.$", target, name));
        public static readonly Regex Anticipate   = new Regex(string.Format("^{0} anticipates {1}'s attack\\.$", target, name));
        public static readonly Regex Evade        = new Regex(string.Format("^{0} evades the attack\\.$", target));
        public static readonly Regex Counter      = new Regex(string.Format("^{0}'s attack is countered by {1}\\. {2} takes {3} point(s)? of damage\\.$",
            target, name, repeatname, damage));
        public static readonly Regex ResistSpell  = new Regex(string.Format("^{0} resists the spell\\.$", target));
        public static readonly Regex ResistEffect = new Regex(string.Format("^{0} resists the effect\\.$", target));
        #endregion

        #region Defeated
        public static readonly Regex Defeat = new Regex(string.Format("^{0} defeats {1}\\.$", name, target));
        public static readonly Regex Dies   = new Regex(string.Format("^{0} falls to the ground\\.$", target));
        #endregion

        #region Experience
        public static readonly Regex ExpChain = new Regex(string.Format("^(EXP|Limit) chain {0}!$", number));
        public static readonly Regex Experience = new Regex(string.Format("^{0} gains {1} (experience|limit points)\\.$", name, number));
        #endregion


        public static readonly Regex DrainHP = new Regex(string.Format("^{0} HP drained from {1}\\.$", damage, target));
        public static readonly Regex DrainMP = new Regex(string.Format("^{0} MP drained from {1}\\.$", damage, target));

        public static readonly Regex DrainSamba = new Regex(string.Format("^Additional effect: {0} HP drained from {1}\\.$", damage, target));
        public static readonly Regex AspirSamba = new Regex(string.Format("^Additional effect: {0} MP drained from {1}\\.$", damage, target));

    }
}
