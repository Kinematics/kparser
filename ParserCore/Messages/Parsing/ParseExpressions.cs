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
        public static readonly string DrainType  = "draintype";
        public static readonly string DrainStat  = "drainstat";
    }

    // Class to store regular expressions in.
    public class ParseExpressions
    {
        #region Named substrings
        private static readonly string playername  = @"(?<name>\w{3,16})";
        private static readonly string npcName     = @"(?<fullname>([Tt]he )?(?<name>\w+((,)|(\.\w?)|['\- ](\d|\w)+)*))";

        private static readonly string name        = @"(?<fullname>([Tt]he )?(?<name>\w+(((('s (?=\w+'s))\w+)|('\w{2,}('s)?)|(-(\w|\d)+)|(the \w+)|( \w+)){0,3}|(( \w+)?'s \w+))))";
        private static readonly string target      = @"(?<fulltarget>([Tt]he )?(?<target>\w+(((('s (?=\w+'s))\w+)|('\w{2,}('s)?)|(-(\w|\d)+)|(the \w+)|( \w+)){0,3}|(( \w+)?'s \w+))))";
        private static readonly string repeatname  = @"([Tt]he )?(?<repeatname>\w+(((('s (?=\w+'s))\w+)|('\w{2,}('s)?)|(-(\w|\d)+)|(the \w+)|( \w+)){0,3}|(( \w+)?'s \w+)))";

        private static readonly string damage      = @"(?<damage>\d{1,4})";
        private static readonly string number      = @"(?<number>\d{1,4})";
        private static readonly string item        = @"(([Aa]|[Aa]n|[Tt]he) )?(?<item>.{3,})";
        private static readonly string money       = @"((?<money>\d{1,4}?) gil)";
        private static readonly string spell       = @"(?<spell>\w+((: (Ichi|Ni|San))|(((('s |. |-)\w+)|(( \w+(?<! (on|III|II|IV|VI|V))){1,2}))?( (III|II|IV|VI|V))?))?)";
        private static readonly string ability     = @"(?<ability>\w+((: \w+)|(-\w+)|('s \w+)|( \w+)( \w+)?)?( \w+'\w{2,})?)";
        private static readonly string effect      = @"(?<effect>\w+( \w+){0,2})";
        private static readonly string skillchain  = @"(?<skillchain>\w+)";
        
        private static readonly string afflictLvl  = @"\(lv\.\d\)";
        private static readonly string drainType   = @"(?<draintype>(H|M|T)P)";
        private static readonly string drainStat   = @"(?<drainstat>STR|DEX|AGI|VIT|INT|MND|CHR)";
        #endregion

        #region Plugin corrections
        public static readonly Regex timestampPlugin = new Regex(@"^\[\d{2}:\d{2}:\d{2}\] (.*)$");
        #endregion

        #region Chat name extractions
        public static readonly Regex ChatSay       = new Regex(string.Format("^{0} : (.+)$", playername));
        public static readonly Regex ChatParty     = new Regex(string.Format("^\\({0}\\) (.+)$", playername));
        public static readonly Regex ChatTell      = new Regex(string.Format("^(>>)?{0}(>>)? (.+)$", playername));
        public static readonly Regex ChatTellFrom  = new Regex(string.Format("^{0}>> (.+)$", playername));
        public static readonly Regex ChatTellTo    = new Regex(string.Format("^>>{0} (.+)$", playername));
        public static readonly Regex ChatShout     = new Regex(string.Format("^{0} : (.+)$", playername));
        public static readonly Regex ChatLinkshell = new Regex(string.Format("^<{0}> (.+)$", playername));
        public static readonly Regex ChatEmote     = new Regex(string.Format("^{0} (.+)$", playername));
        public static readonly Regex ChatEmoteA    = new Regex(string.Format("^{0}'s (.+)$", playername));
        public static readonly Regex ChatNPC       = new Regex(string.Format("^{0} : (.+)$", npcName));
        #endregion

        #region Name Tests
        // If any of the specified characters occur in the name, it should be a mob type (may possibly be a pet).
        // Otherwise have to check against the Avatar/Wyvern/Puppet name lists.
        public static readonly Regex MobNameTest = new Regex(@"['\- \d]");
        // Bst jug pets are named either as "CrabFamiliar" (all one word) or "CourierCarrie" (all one word).
        public static readonly Regex BstJugPetName = new Regex(@"(^\w+Familiar$)|(^[A-Z][a-z]+[A-Z][a-z]+$)");
        #endregion


        #region Experience
        public static readonly Regex ExpChain     = new Regex(string.Format("^(EXP|Limit) chain #{0}!$", number));
        public static readonly Regex Experience   = new Regex(string.Format("^{0} gains {1} (experience|limit) points\\.$", name, number));
        public static readonly Regex NoExperience = new Regex(string.Format("^No experience gained\\.$"));
        #endregion

        #region Loot
        public static readonly Regex FindLootOn = new Regex(string.Format("^You find {0} on {1}\\.$", item, target));
        public static readonly Regex FindLootIn = new Regex(string.Format("^You find {0} in {1}\\.$", item, target));
        public static readonly Regex GetLoot    = new Regex(string.Format("^{0} obtains {1}\\.$", playername, item));
        public static readonly Regex GetGil     = new Regex(string.Format("^{0} obtains {1}\\.$", playername, money));
        public static readonly Regex LootReqr   = new Regex(string.Format("^You do not meet the requirements to obtain {0}\\.$", item));
        public static readonly Regex LootLost   = new Regex(string.Format("^{0} lost\\.$", item));
        public static readonly Regex LotItem    = new Regex(string.Format("^{0}'s lot for {1}: {2} points\\.$", playername, item, number));
        public static readonly Regex Steal      = new Regex(string.Format("^{0} steals {1} from {2}\\.$", playername, item, target));
        public static readonly Regex FailSteal  = new Regex(string.Format("^{0} fails to steal from {1}\\.$", playername, target));
        public static readonly Regex Mug        = new Regex(string.Format("^{0} mugs {1} from {2}\\.$", playername, money, target));
        public static readonly Regex FailMug    = new Regex(string.Format("^{0} fails to mug {1}\\.$", playername, target));
        #endregion


        #region Preparing to take action
        public static readonly Regex PrepSpell    = new Regex(string.Format("^{0} start(s)? casting {1}\\.$", name, spell));
        public static readonly Regex PrepSpellOn  = new Regex(string.Format("^{0} start(s)? casting {1} on {2}\\.$", name, spell, target));
        public static readonly Regex PrepAbility  = new Regex(string.Format("^{0} read(y|ies) {1}\\.$", name, ability));
        #endregion

        #region Completes action
        public static readonly Regex CastSpell    = new Regex(string.Format("^{0} cast(s)? {1}\\.$", name, spell));
        public static readonly Regex UseAbility   = new Regex(string.Format("^{0} use(s)? {1}\\.$", name, ability));
        public static readonly Regex UseAbilityOn = new Regex(string.Format("^{0} use(s)? {1} on {2}\\.$", name, ability, target));
        public static readonly Regex MissAbility  = new Regex(string.Format("^{0} use(s)? {1}, but miss(es)? {2}\\.$", name, ability, target));
        public static readonly Regex FailsCharm   = new Regex(string.Format("^{0} fail(s)? to charm {1}\\.$", name, target));
        public static readonly Regex UseItem      = new Regex(string.Format("^{0} use(s)? {1}\\.$", name, item));
        // Corsair stuff (6f/65|70/66):
        public static readonly Regex UseCorRoll   = new Regex(string.Format("^{0} uses {1}\\. The total comes to {2}!$", name, ability, number));
        public static readonly Regex TotalCorRoll = new Regex(string.Format("^The total for {0} increases to {1}!$", ability, number));
        public static readonly Regex GainCorRoll  = new Regex(string.Format("^{0} receives the effect of {1}\\.$", playername, ability));
        public static readonly Regex BustCorRoll  = new Regex(string.Format("^Bust!$"));
        public static readonly Regex LoseCorRoll  = new Regex(string.Format("^{0} loses the effect of {1}\\.$", playername, ability));
        #endregion

        #region Spell/Ability Effects
        public static readonly Regex RecoversHP = new Regex(string.Format("^{0} recover(s)? {1} HP\\.$", target, number));
        public static readonly Regex RecoversMP = new Regex(string.Format("^{0} recover(s)? {1} MP\\.$", target, number));
        public static readonly Regex Afflict    = new Regex(string.Format("^{0} (is|are) afflicted with {1} {2}\\.$", target, effect, afflictLvl));
        public static readonly Regex Enfeeble   = new Regex(string.Format("{0} (is|are) {1}\\.$", target, effect));
        public static readonly Regex Buff       = new Regex(string.Format("^{0} gain(s)? the effect of {1}\\.$", target, effect));
        public static readonly Regex GainResistance = new Regex(string.Format("^{0} gain(s)? resistance against {1}\\.$", target, effect));
        public static readonly Regex Debuff     = new Regex(string.Format("^{0} receive(s)? the effect of {1}\\.$", target, effect));
        public static readonly Regex Enhance    = new Regex(string.Format("^{0}'(s)? attacks are enhanced\\.$", target));
        public static readonly Regex Charmed    = new Regex(string.Format("^{0} (is|are) now under {1}'s control\\.$", target, name));
        public static readonly Regex NotCharmed = new Regex(string.Format("^{0} (is|are) no longer charmed\\.$", target));
        public static readonly Regex Dispelled  = new Regex(string.Format("^{0}'(s)? {1} effect disappears!$", target, effect));
        #endregion

        #region Failed Actions
        public static readonly Regex Interrupted = new Regex(string.Format("^{0}'(s)? casting is interrupted\\.$", name));
        public static readonly Regex Paralyzed   = new Regex(string.Format("^{0} (is|are) paralyzed\\.$", name));
        public static readonly Regex CannotSee   = new Regex(string.Format("^Unable to see {0}\\.$", target));
        public static readonly Regex CannotSee2  = new Regex(string.Format("^You cannot see {0}\\.$", target));
        public static readonly Regex TooFarAway  = new Regex(string.Format("^{0} (is|are) too far away\\.$", target));
        public static readonly Regex OutOfRange  = new Regex(string.Format("^{0} (is|are) out of range\\.$", target));
        public static readonly Regex CannotAttack = new Regex(string.Format("^You cannot attack that target\\.$"));
        public static readonly Regex NotEnoughTP  = new Regex(string.Format("^You do not have enough TP\\.$"));
        public static readonly Regex NotEnoughTP2 = new Regex(string.Format("^Not enough TP\\.$"));
        public static readonly Regex NotEnoughTP3 = new Regex(string.Format("^{0} does not have enough TP\\.$", name));
        public static readonly Regex NotEnoughMP  = new Regex(string.Format("^You do not have enough MP\\.$", target));
        public static readonly Regex Intimidated  = new Regex(string.Format("^{0} (is|are) intimidated by {1}'s presence\\.$", name, target));
        public static readonly Regex UnableToCast = new Regex(string.Format("^Unable to cast spells at this time\\.$"));
        public static readonly Regex UnableToUse  = new Regex(string.Format("^Unable to use job ability\\.$"));
        public static readonly Regex UnableToUse2 = new Regex(string.Format("^Unable to use weapon skill\\.$"));
        public static readonly Regex UnableToUse3 = new Regex(string.Format("^{0} is unable to use weapon skills\\.$", name));
        public static readonly Regex NoEffect     = new Regex(string.Format("^{0}'(s)? {1} has no effect on {2}\\.$", name, spell, target));
        public static readonly Regex NoEffect2   = new Regex(string.Format("^No effect on {0}\\.$", target));
        public static readonly Regex NoEffect3   = new Regex(string.Format("^{0} casts {1} on {2}, but the spell fails to take effect\\.$", name, spell, target));
        public static readonly Regex AutoTarget  = new Regex(string.Format("^Auto-targeting {0}\\.$", target));
        public static readonly Regex TooFarForXP = new Regex(string.Format("^You are too far from the battle to gain experience\\.$"));
        public static readonly Regex LoseSight   = new Regex(string.Format("^You lose sight of {0}\\.$", target));
        public static readonly Regex FailActivate = new Regex(string.Format("^{0} fails to activate\\.$", item));
        #endregion

        #region Modifiers on existing lines
        public static readonly Regex AdditionalEffect = new Regex(@"^Additional effect:");
        public static readonly Regex MagicBurst       = new Regex(@"^Magic Burst!");
        public static readonly Regex AdditionalDamage = new Regex(string.Format("^Additional effect: {0} point(s)? of damage\\.$", damage));
        public static readonly Regex AdditionalStatus = new Regex(string.Format("^Additional effect: {0}\\.$", effect));
        public static readonly Regex AdditionalDrain  = new Regex(string.Format("^Additional effect: {0} HP drained from {1}\\.$", damage, target));
        public static readonly Regex AdditionalAspir  = new Regex(string.Format("^Additional effect: {0} MP drained from {1}\\.$", damage, target));
        public static readonly Regex AdditionalHeal   = new Regex(string.Format("^Additional effect: {0} recovers {1} HP\\.$", target, damage));
        #endregion

        #region Combat damage
        public static readonly Regex MeleeHit          = new Regex(string.Format("^{0} hit(s)? {1} for {2} point(s)? of damage\\.$", name, target, damage));
        public static readonly Regex RangedAttack      = new Regex(string.Format("^{0}'(s)? ranged attack hit(s)? {1} for {2} point(s)? of damage\\.$", name, target, damage));
        public static readonly Regex RangedHit         = new Regex(string.Format("^{0} use(s)? Ranged Attack\\.$", name));
        public static readonly Regex CriticalHit       = new Regex(string.Format("^{0} score(s)? a critical hit!$", name));
        public static readonly Regex RangedCriticalHit = new Regex(string.Format("^{0}'(s)? ranged attack scores a critical hit!$", name));
        public static readonly Regex TargetTakesDamage = new Regex(string.Format("{0} take(s)? {1}( additional)? point(s)? of damage\\.$", target, damage));
        public static readonly Regex Spikes            = new Regex(string.Format("{0}'(s)? spikes deal {1} point(s)? of damage to {2}\\.", name, damage, target));
        public static readonly Regex Skillchain        = new Regex(string.Format("^Skillchain: {0}\\.$", skillchain));
        #endregion

        #region Combat defenses
        public static readonly Regex MeleeMiss    = new Regex(string.Format("^{0} miss(es)? {1}\\.$", name, target));
        public static readonly Regex RangedMiss   = new Regex(string.Format("^{0} use(s)? Ranged Attack, but miss(es)? {1}\\.$", name, target));
        public static readonly Regex RangedMiss2  = new Regex(string.Format("^{0}'(s)? ranged attack misses\\.$", name));
        public static readonly Regex Blink        = new Regex(string.Format("^{0} of {1}'s shadows absorb(s)? the damage and disappear(s)?\\.$", number, target));
        public static readonly Regex Parry        = new Regex(string.Format("^{0} parr(y|ies) {1}'(s)? attack with (his|her|its) weapon\\.$", target, name));
        public static readonly Regex Anticipate   = new Regex(string.Format("^{0} anticipate(s)? {1}'(s)? attack\\.$", target, name));
        public static readonly Regex Anticipate2  = new Regex(string.Format("^{0} anticipate(s)? the attack\\.$", target));
        public static readonly Regex Evade        = new Regex(string.Format("^{0} evade(s)? the attack\\.$", target));
        public static readonly Regex Evade2       = new Regex(string.Format("^{0} evade(s)\\.$", target));
        public static readonly Regex Counter      = new Regex(string.Format("^{0}'(s)? attack is countered by {1}\\. {2} takes {3} point(s)? of damage\\.$",
            target, name, repeatname, damage));
        public static readonly Regex CounterShadow = new Regex(string.Format("^{0}'(s)? attack is countered by {1}\\. {2} of {3}'(s)? shadows absorbs the damage and disappears\\.$",
            target, name, number, repeatname));
        public static readonly Regex ResistSpell  = new Regex(string.Format("^{0} resist(s)? the spell\\.$", target));
        public static readonly Regex ResistSpell2 = new Regex(string.Format("^{0} resist(s)? the effects of the spell!$", target));
        public static readonly Regex ResistEffect = new Regex(string.Format("^{0} resist(s)? the effect\\.$", target));
        #endregion

        #region Drains
        public static readonly Regex Drain = new Regex(string.Format("^{0} {1} drained from {2}\\.$", damage, drainType, target));
        public static readonly Regex AbsorbStat = new Regex(string.Format("^{0}'(s)? {1} is drained\\.$", target, drainStat));
        public static readonly Regex ReduceTP = new Regex(string.Format("^{0}'(s)? TP is reduced to 0\\.$", target));
        #endregion

        #region Defeated
        public static readonly Regex Defeated = new Regex(string.Format("^{0} was defeated by {1}\\.$", target, name));
        public static readonly Regex Defeat   = new Regex(string.Format("^{0} defeats {1}\\.$", name, target));
        public static readonly Regex Dies     = new Regex(string.Format("^{0} falls to the ground\\.$", target));
        #endregion
    }
}
