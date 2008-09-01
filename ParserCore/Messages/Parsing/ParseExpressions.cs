using System;
using System.Text.RegularExpressions;

namespace WaywardGamers.KParser
{
    // Field names used in the named substrings of parse expressions.
    internal class ParseFields
    {
        internal static readonly string Name       = "name";
        internal static readonly string Fullname   = "fullname";
        internal static readonly string RepeatName = "repeatname";
        internal static readonly string Target     = "target";
        internal static readonly string Fulltarget = "fulltarget";
        internal static readonly string Damage     = "damage";
        internal static readonly string Number     = "number";
        internal static readonly string Item       = "item";
        internal static readonly string Money      = "money";
        internal static readonly string Spell      = "spell";
        internal static readonly string Ability    = "ability";
        internal static readonly string Effect     = "effect";
        internal static readonly string SC         = "skillchain";
        internal static readonly string DrainType  = "draintype";
        internal static readonly string DrainStat  = "drainstat";
        internal static readonly string Remainder  = "remainder";
    }

    // Class to store regular expressions in.
    internal class ParseExpressions
    {
        #region Named substrings
        private static readonly string playername  = @"(?<name>\w{3,16})";
        private static readonly string npcName     = @"(?<fullname>([Tt]he )?(?<name>\w+((,)|(\.\w?)|['\- ](\d|\w)+)*))";

        private static readonly string name        = @"(?<fullname>([Tt]he )?(?<name>\w+(((('s (?=\w+'s))\w+)|('\w{2,}('s)?)|(-(\w|\d)+)|(the \w+)|( No.\d+)|( \w+)){0,3}|(( \w+)?'s \w+))))";
        private static readonly string target      = @"(?<fulltarget>([Tt]he )?(?<target>\w+(((('s (?=\w+'s))\w+)|('\w{2,}('s)?)|(-(\w|\d)+)|(the \w+)|( No.\d+)|( \w+)){0,3}|(( \w+)?'s \w+))))";
        private static readonly string repeatname  = @"([Tt]he )?(?<repeatname>\w+(((('s (?=\w+'s))\w+)|('\w{2,}('s)?)|(-(\w|\d)+)|(the \w+)|( No.\d+)|( \w+)){0,3}|(( \w+)?'s \w+)))";

        private static readonly string damage      = @"(?<damage>\d{1,4})";
        private static readonly string number      = @"(?<number>\d{1,4})";
        private static readonly string item        = @"(([Aa]|[Aa]n|[Tt]he) )?(?<item>(\?\?\? )?.{3,})";
        private static readonly string money       = @"((?<money>\d{1,4}?) gil)";
        private static readonly string spell       = @"(?<spell>\w+((: (Ichi|Ni|San))|(((('s |. |-)\w+)|(( \w+(?<! (on|III|II|IV|VI|V))){1,2}))?( (III|II|IV|VI|V))?))?)";
        private static readonly string ability     = @"(?<ability>\w+((: \w+)|(-\w+)( \w+)?|('s \w+)|( \w+)( \w+)?)?( \w+'\w{2,})?)";
        private static readonly string effect      = @"(?<effect>\w+( \w+){0,2})";
        private static readonly string skillchain  = @"(?<skillchain>\w+)";
        
        private static readonly string afflictLvl  = @"\(lv\.\d\)";
        private static readonly string drainType   = @"(?<draintype>(H|M|T)P)";
        private static readonly string drainStat   = @"(?<drainstat>STR|DEX|AGI|VIT|INT|MND|CHR)";

        private static readonly string remainder   = @"(?<remainder>.*)";
        #endregion

        #region Plugin corrections
        internal static readonly Regex TimestampPlugin =
            new Regex(@"^\x1e(\x3f|\xfa|\xfc)\[\d{2}:\d{2}:\d{2}\] \x1e\x01(?<remainder>.*)$");
        #endregion

        #region Chat name extractions
        internal static readonly Regex ChatSay       = new Regex(string.Format("^{0} : (.+)$", playername));
        internal static readonly Regex ChatParty     = new Regex(string.Format("^\\({0}\\) (.+)$", playername));
        internal static readonly Regex ChatTell      = new Regex(string.Format("^(>>)?{0}(>>)? (.+)$", playername));
        internal static readonly Regex ChatTellFrom  = new Regex(string.Format("^{0}>> (.+)$", playername));
        internal static readonly Regex ChatTellTo    = new Regex(string.Format("^>>{0} (.+)$", playername));
        internal static readonly Regex ChatShout     = new Regex(string.Format("^{0} : (.+)$", playername));
        internal static readonly Regex ChatLinkshell = new Regex(string.Format("^<{0}> (.+)$", playername));
        internal static readonly Regex ChatEmote     = new Regex(string.Format("^{0} (.+)$", playername));
        internal static readonly Regex ChatEmoteA    = new Regex(string.Format("^{0}'s (.+)$", playername));
        internal static readonly Regex ChatNPC       = new Regex(string.Format("^{0} : (.+)$", npcName));
        #endregion

        #region Name Tests
        // If any of the specified characters occur in the name, it should be a mob type (may possibly be a pet).
        // Otherwise have to check against the Avatar/Wyvern/Puppet name lists.
        internal static readonly Regex MobNameTest = new Regex(@"['\- \d]");
        // Bst jug pets are named either as "CrabFamiliar" (all one word) or "CourierCarrie" (all one word).
        internal static readonly Regex BstJugPetName = new Regex(@"(^\w+Familiar$)|(^[A-Z][a-z]+[A-Z][a-z]+$)");
        #endregion


        #region Experience
        internal static readonly Regex ExpChain     = new Regex(string.Format("^(EXP|Limit) chain #{0}!$", number));
        internal static readonly Regex Experience   = new Regex(string.Format("^{0} gains {1} (experience|limit) points\\.$", name, number));
        internal static readonly Regex NoExperience = new Regex(string.Format("^No experience gained\\.$"));
        #endregion

        #region Loot
        internal static readonly Regex FindLootOn = new Regex(string.Format("^You find {0} on {1}\\.$", item, target));
        internal static readonly Regex FindLootIn = new Regex(string.Format("^You find {0} in {1}\\.$", item, target));
        internal static readonly Regex OpenChest  = new Regex(string.Format("^{0} opens {1} and finds {2}\\.$", playername, target, item));
        internal static readonly Regex GetLoot    = new Regex(string.Format("^{0} obtains {1}(\\.|!)$", playername, item));
        internal static readonly Regex GetGil     = new Regex(string.Format("^{0} obtains {1}\\.$", playername, money));
        internal static readonly Regex LootReqr   = new Regex(string.Format("^You do not meet the requirements to obtain {0}\\.$", item));
        internal static readonly Regex LootLost   = new Regex(string.Format("^{0} (?!was )lost\\.$", item));
        internal static readonly Regex LotItem    = new Regex(string.Format("^{0}'s lot for {1}: {2} points\\.$", playername, item, number));
        internal static readonly Regex Steal      = new Regex(string.Format("^{0} steals {1} from {2}\\.$", playername, item, target));
        internal static readonly Regex FailSteal  = new Regex(string.Format("^{0} fails to steal from {1}\\.$", playername, target));
        internal static readonly Regex Mug        = new Regex(string.Format("^{0} mugs {1} from {2}\\.$", playername, money, target));
        internal static readonly Regex FailMug    = new Regex(string.Format("^{0} fails to mug {1}\\.$", playername, target));
        internal static readonly Regex DiceRoll   = new Regex(string.Format("^Dice roll! {0} rolls {1}!$", playername, number));
        #endregion


        #region Preparing to take action
        internal static readonly Regex PrepSpell    = new Regex(string.Format("^{0} start(s)? casting {1}\\.$", name, spell));
        internal static readonly Regex PrepSpellOn  = new Regex(string.Format("^{0} start(s)? casting {1} on {2}\\.$", name, spell, target));
        internal static readonly Regex PrepAbility  = new Regex(string.Format("^{0} read(y|ies) {1}\\.$", name, ability));
        #endregion

        #region Completes action
        internal static readonly Regex CastSpell    = new Regex(string.Format("^{0} cast(s)? {1}\\.$", name, spell));
        internal static readonly Regex CastSpellOn  = new Regex(string.Format("^{0} cast(s)? {1} on {2}\\.$", name, spell, target));
        internal static readonly Regex UseAbility   = new Regex(string.Format("^{0} use(s)? {1}\\.$", name, ability));
        internal static readonly Regex UseAbilityOn = new Regex(string.Format("^{0} use(s)? {1} on {2}\\.$", name, ability, target));
        internal static readonly Regex MissAbility  = new Regex(string.Format("^{0} use(s)? {1}, but miss(es)? {2}\\.$", name, ability, target));
        internal static readonly Regex MissAbility2 = new Regex(string.Format("^{0} use(s)? {1}, but miss(es)?\\.$", name, ability, target));
        internal static readonly Regex FailsCharm   = new Regex(string.Format("^{0} fail(s)? to charm {1}\\.$", name, target));
        internal static readonly Regex UseItem      = new Regex(string.Format("^{0} use(s)? {1}\\.$", name, item));
        // Corsair stuff (6f/65|70/66):
        internal static readonly Regex UseCorRoll   = new Regex(string.Format("^{0} uses {1}\\. The total comes to {2}!$", name, ability, number));
        internal static readonly Regex TotalCorRoll = new Regex(string.Format("^The total for {0} increases to {1}!$", ability, number));
        internal static readonly Regex GainCorRoll  = new Regex(string.Format("^{0} receives the effect of {1}\\.$", playername, ability));
        internal static readonly Regex BustCorRoll  = new Regex(string.Format("^Bust!$"));
        internal static readonly Regex LoseCorRoll  = new Regex(string.Format("^{0} loses the effect of {1}\\.$", playername, ability));
        // Cover
        internal static readonly Regex UseCover     = new Regex(string.Format("^{0} covers {1}\\.$", name, target));
        // Accomplice/Collaborator
        internal static readonly Regex StealEnmity = new Regex(string.Format("^Enmity is stolen from {0}\\.$", target));
        #endregion

        #region Spell/Ability Effects
        internal static readonly Regex RecoversHP = new Regex(string.Format("^{0} recover(s)? {1} HP\\.$", target, number));
        internal static readonly Regex RecoversMP = new Regex(string.Format("^{0} recover(s)? {1} MP\\.$", target, number));
        internal static readonly Regex Afflict    = new Regex(string.Format("^{0} (is|are) afflicted with {1} {2}\\.$", target, effect, afflictLvl));
        internal static readonly Regex Enfeeble   = new Regex(string.Format("{0} (is|are) {1}\\.$", target, effect));
        internal static readonly Regex Buff       = new Regex(string.Format("^{0} gain(s)? the effect of {1}\\.$", target, effect));
        internal static readonly Regex GainResistance = new Regex(string.Format("^{0} gain(s)? resistance against {1}\\.$", target, effect));
        internal static readonly Regex Debuff     = new Regex(string.Format("^{0} receive(s)? the effect of {1}\\.$", target, effect));
        internal static readonly Regex Enhance    = new Regex(string.Format("^{0}'(s)? attacks are enhanced\\.$", target));
        internal static readonly Regex Charmed    = new Regex(string.Format("^{0} (is|are) now under {1}'s control\\.$", target, name));
        internal static readonly Regex NotCharmed = new Regex(string.Format("^{0} (is|are) no longer charmed\\.$", target));
        internal static readonly Regex Dispelled  = new Regex(string.Format("^{0}'(s)? {1} effect disappears!$", target, effect));
        internal static readonly Regex RemoveStatus = new Regex(string.Format("^{0} successfully removes {1}'s {2}$", name, target, effect));
        internal static readonly Regex ItemBuff    = new Regex(string.Format("^{0} receive(s)? the effect of {1}\\.$", target, effect));
        internal static readonly Regex ItemCleanse = new Regex(string.Format("^{0} is no longer {1}\\.$", target, effect));
        internal static readonly Regex ItemReduceTP = new Regex(string.Format("^{0}'s TP is reduced\\.$", target));
        internal static readonly Regex Hide        = new Regex(string.Format("^{0} hides!$", name));
        #endregion

        #region Failed Actions
        internal static readonly Regex Interrupted = new Regex(string.Format("^{0}'(s)? casting is interrupted\\.$", name));
        internal static readonly Regex Paralyzed   = new Regex(string.Format("^{0} (is|are) paralyzed\\.$", name));
        internal static readonly Regex CannotSee   = new Regex(string.Format("^Unable to see {0}\\.$", target));
        internal static readonly Regex CannotSee2  = new Regex(string.Format("^You cannot see {0}\\.$", target));
        internal static readonly Regex TooFarAway  = new Regex(string.Format("^{0} (is|are) too far away\\.$", target));
        internal static readonly Regex OutOfRange  = new Regex(string.Format("^{0} (is|are) out of range\\.$", target));
        internal static readonly Regex CannotAttack = new Regex(string.Format("^You cannot attack that target\\.$"));
        internal static readonly Regex NotEnoughTP  = new Regex(string.Format("^You do not have enough TP\\.$"));
        internal static readonly Regex NotEnoughTP2 = new Regex(string.Format("^Not enough TP\\.$"));
        internal static readonly Regex NotEnoughTP3 = new Regex(string.Format("^{0} does not have enough TP\\.$", name));
        internal static readonly Regex NotEnoughMP  = new Regex(string.Format("^You do not have enough MP\\.$", target));
        internal static readonly Regex Intimidated  = new Regex(string.Format("^{0} (is|are) intimidated by {1}'s presence\\.$", name, target));
        internal static readonly Regex UnableToCast = new Regex(string.Format("^Unable to cast spells at this time\\.$"));
        internal static readonly Regex UnableToUse  = new Regex(string.Format("^Unable to use job ability\\.$"));
        internal static readonly Regex UnableToUse2 = new Regex(string.Format("^Unable to use weapon skill\\.$"));
        internal static readonly Regex UnableToUse3 = new Regex(string.Format("^{0} is unable to use weapon skills\\.$", name));
        internal static readonly Regex NoEffect     = new Regex(string.Format("^{0}'(s)? {1} has no effect on {2}\\.$", name, spell, target));
        internal static readonly Regex NoEffect2   = new Regex(string.Format("^No effect on {0}\\.$", target));
        internal static readonly Regex NoEffect3   = new Regex(string.Format("^{0} casts {1} on {2}, but the spell fails to take effect\\.$", name, spell, target));
        internal static readonly Regex AutoTarget  = new Regex(string.Format("^Auto-targeting {0}\\.$", target));
        internal static readonly Regex TooFarForXP = new Regex(string.Format("^You are too far from the battle to gain experience\\.$"));
        internal static readonly Regex LoseSight   = new Regex(string.Format("^You lose sight of {0}\\.$", target));
        internal static readonly Regex FailActivate = new Regex(string.Format("^{0} fails? to activate\\.$", item));
        internal static readonly Regex CannotPerform = new Regex(string.Format("^{0} cannot perform that action\\.$", name));
        internal static readonly Regex FailHide    = new Regex(string.Format("^{0} (try|tries) to hide, but (is|are) spotted by {1}\\.$", name, target));
        #endregion

        #region Modifiers on existing lines
        internal static readonly Regex AdditionalEffect = new Regex(@"^Additional (E|e)ffect:");
        internal static readonly Regex MagicBurst       = new Regex(@"^Magic Burst!");
        internal static readonly Regex Cover            = new Regex(string.Format("^Cover! {0}", remainder));
        internal static readonly Regex AdditionalDamage = new Regex(string.Format("^Additional effect: {0} point(s)? of damage\\.$", damage));
        internal static readonly Regex AdditionalStatus = new Regex(string.Format("^Additional effect: {0}\\.$", effect));
        internal static readonly Regex AdditionalDrain  = new Regex(string.Format("^Additional effect: {0} HP drained from {1}\\.$", damage, target));
        internal static readonly Regex AdditionalAspir  = new Regex(string.Format("^Additional effect: {0} MP drained from {1}\\.$", damage, target));
        internal static readonly Regex AdditionalHeal   = new Regex(string.Format("^Additional effect: {0} recovers {1} HP\\.$", target, damage));
        #endregion

        #region Combat damage
        internal static readonly Regex MeleeHit          = new Regex(string.Format("^{0} hit(s)? {1} for {2} point(s)? of damage\\.$", name, target, damage));
        internal static readonly Regex RangedAttack      = new Regex(string.Format("^{0}'(s)? ranged attack hit(s)? {1} for {2} point(s)? of damage\\.$", name, target, damage));
        internal static readonly Regex RangedHit         = new Regex(string.Format("^{0} use(s)? Ranged Attack\\.$", name));
        internal static readonly Regex CriticalHit       = new Regex(string.Format("^{0} score(s)? a critical hit!$", name));
        internal static readonly Regex RangedCriticalHit = new Regex(string.Format("^{0}'(s)? ranged attack scores a critical hit!$", name));
        internal static readonly Regex TargetTakesDamage = new Regex(string.Format("{0} take(s)? {1}( additional)? point(s)? of damage\\.$", target, damage));
        internal static readonly Regex Spikes            = new Regex(string.Format("{0}'(s)? spikes deal {1} point(s)? of damage to {2}\\.", name, damage, target));
        internal static readonly Regex Skillchain        = new Regex(string.Format("^Skillchain: {0}\\.$", skillchain));
        #endregion

        #region Combat defenses
        internal static readonly Regex MeleeMiss    = new Regex(string.Format("^{0} miss(es)? {1}\\.$", name, target));
        internal static readonly Regex MeleeDodge   = new Regex(string.Format("^{0} dodges the attack\\.$", target));
        internal static readonly Regex RangedMiss   = new Regex(string.Format("^{0} use(s)? Ranged Attack, but miss(es)? {1}\\.$", name, target));
        internal static readonly Regex RangedMiss2  = new Regex(string.Format("^{0}'(s)? ranged attack misses\\.$", name));
        internal static readonly Regex Blink        = new Regex(string.Format("^{0} of {1}'s shadows absorb(s)? the damage and disappear(s)?\\.$", number, target));
        internal static readonly Regex Parry        = new Regex(string.Format("^{0} parr(y|ies) {1}'(s)? attack with (his|her|its) weapon\\.$", target, name));
        internal static readonly Regex Anticipate   = new Regex(string.Format("^{0} anticipate(s)? {1}'(s)? attack\\.$", target, name));
        internal static readonly Regex Anticipate2  = new Regex(string.Format("^{0} anticipate(s)? the attack\\.$", target));
        internal static readonly Regex Evade        = new Regex(string.Format("^{0} evade(s)?( the attack)?\\.$", target));
        internal static readonly Regex Counter      = new Regex(string.Format("^{0}'(s)? attack is countered by {1}\\. {2} takes {3} point(s)? of damage\\.$",
            target, name, repeatname, damage));
        internal static readonly Regex CounterShadow   = new Regex(string.Format("^{0}'(s)? attack is countered by {1}\\. {2} of {3}'(s)? shadows absorbs the damage and disappears\\.$",
            target, name, number, repeatname));
        internal static readonly Regex Retaliate       = new Regex(string.Format("^{0} retaliates\\. {1} takes {2} points? of damage\\.$",
            name, target, damage));
        internal static readonly Regex RetaliateShadow = new Regex(string.Format("^{0} retaliates\\. {1} of {2}'s? shadows absorbs the damage and disappears\\.$",
            name, number, target));
        internal static readonly Regex ResistSpell     = new Regex(string.Format("^(Resist! )?{0} resist(s)? the (effects of the )?spell(\\.|!)$", target));
        internal static readonly Regex ResistEffect    = new Regex(string.Format("^{0} resist(s)? the effect\\.$", target));
        #endregion

        #region Drains
        internal static readonly Regex Drain = new Regex(string.Format("^{0} {1} drained from {2}\\.$", damage, drainType, target));
        internal static readonly Regex AbsorbStat = new Regex(string.Format("^{0}'(s)? {1} is drained\\.$", target, drainStat));
        internal static readonly Regex ReduceTP = new Regex(string.Format("^{0}'(s)? TP is reduced to 0\\.$", target));
        #endregion

        #region Defeated
        internal static readonly Regex Defeated = new Regex(string.Format("^{0} was defeated by {1}\\.$", target, name));
        internal static readonly Regex Defeat   = new Regex(string.Format("^{0} defeats {1}\\.$", name, target));
        internal static readonly Regex Dies     = new Regex(string.Format("^{0} falls to the ground\\.$", target));
        #endregion
    }

    // Special spell names we want to check for, but also allow for localization
    internal class SpellNames
    {
        internal static readonly string Raise = "Raise";

        internal static readonly string Dispel = "Dispel";
        internal static readonly string Finale = "Magic Finale";

        internal static readonly string Erase = "Erase";
        internal static readonly string HealWaltz = "Healing Waltz";

        internal static readonly string RemoveStatus = "na";
    }

    // Special other names we want to check for, but also allow for localization
    internal class EffectNames
    {
        internal static readonly string Overloaded = "Overloaded";
    }

}
