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
        internal static readonly string Spell      = "spell";
        internal static readonly string Ability    = "ability";
        internal static readonly string Effect     = "effect";
        internal static readonly string SC         = "skillchain";
        internal static readonly string DrainType  = "draintype";
        internal static readonly string DrainStat  = "drainstat";
        internal static readonly string Remainder  = "remainder";
        internal static readonly string Light      = "light";
    }

    // Class to store regular expressions in.
    internal class ParseExpressions
    {
        internal static void Reset(string parseCulture)
        {
        }

        #region Named substrings
        private static readonly string playername  = @"(?<name>\w{3,16})";
        private static readonly string npcName     = @"(?<fullname>([Tt]he )?(?<name>\w+((,)|(\.\w{0,2})|['\- ](\d|\w)+)*))";

        private static readonly string name        = @"(?<fullname>([Tt]he )?(?<name>\w+(((('s (?=\w+'s))\w+)|('[^s]\w*('s)?)|(-(\w|\d)+)|(.\w+)|(the \w+)|( No.\d+)|( \w+)){0,3}|(( \w+)?'s \w+))))";
        private static readonly string target      = @"(?<fulltarget>([Tt]he )?(?<target>\w+(((('s (?=\w+'s))\w+)|('[^s]\w*('s)?)|(-(\w|\d)+)|(.\w+)|(the \w+)|( No.\d+)|( \w+)){0,3}|(( \w+)?'s \w+))))";
        private static readonly string repeatname  = @"([Tt]he )?(?<repeatname>\w+(((('s (?=\w+'s))\w+)|('[^s]\w*('s)?)|(-(\w|\d)+)|(.\w+)|(the \w+)|( No.\d+)|( \w+)){0,3}|(( \w+)?'s \w+)))";

        private static readonly string damage      = @"(?<damage>\d{1,5})";
        private static readonly string number      = @"(?<number>\d{1,5})";
        private static readonly string item        = @"(([Aa]|[Aa]n|[Tt]he) )?(?<item>(\?\?\? )?.{3,})";
        private static readonly string money       = @"((?<number>\d{1,5}?) gil)";
        private static readonly string cruor       = @"((?<number>\d{1,5}?) cruor)";
        private static readonly string spell       = @"(?<spell>\w+((: (Ichi|Ni|San))|(((('s |. |-)\w+( [A-Z]\w+)?)|(( \w+(?<! (on|III|II|IV|VI|V))){1,2}))?( (III|II|IV|VI|V))?))?)";
        private static readonly string ability     = @"(?<ability>\w+((: \w+)|((-|,)\w+)( \w+)?|('s \w+)|( \w+)( \w+)?|( of the \w+))?( \w+'\w{2,})?)";
        private static readonly string effect      = @"(?<effect>\w+( \w+(\.)?){0,2})";
        private static readonly string skillchain  = @"(?<skillchain>\w+)";
        
        private static readonly string afflictLvl  = @"\(lv\.\d\)";
        private static readonly string drainType   = @"(?<draintype>(H|M|T)P)";
        private static readonly string drainStat   = @"(?<drainstat>STR|DEX|AGI|VIT|INT|MND|CHR)";

        private static readonly string colorLight  = @"(?<light>(?<color>pearlescent|azure|ruby|amber|ebon|golden|silver) light)";

        private static readonly string remainder   = @"(?<remainder>.*)";
        #endregion

        #region Plugin corrections
        internal static readonly Regex TimestampPlugin =
            new Regex(@"^\x1e(\x3f|\xfa|\xfc)\[(?<time>\d{2}:\d{2}:\d{2})\] \x1e\x01(?<remainder>.*)$");
        #endregion

        #region Chat name extractions
        internal static readonly Regex ChatSay       = new Regex(string.Format("^{0} : (.+)$", playername));
        internal static readonly Regex ChatParty     = new Regex(string.Format("^\\({0}\\) (.+)$", playername));
        internal static readonly Regex ChatTell      = new Regex(string.Format("^(>>)?{0}(>>)? (.+)$", playername));
        internal static readonly Regex ChatTellFrom  = new Regex(string.Format("^{0}>> (.+)$", playername));
        internal static readonly Regex ChatTellTo    = new Regex(string.Format("^>>{0} (.+)$", playername));
        internal static readonly Regex ChatShout     = new Regex(string.Format("^{0} : (.+)$", playername));
        internal static readonly Regex ChatLinkshell = new Regex(string.Format("^<{0}> (.+)$", playername));
        internal static readonly Regex ChatEmote     = new Regex(string.Format("^{0}('s)? (.+)$", playername));
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
        internal static readonly Regex GetCruor   = new Regex(string.Format("^{0} obtained {1}\\.$", playername, cruor));
        internal static readonly Regex TreasureChest = new Regex(string.Format("^The monster was concealing a treasure chest!$"));
        internal static readonly Regex OpenLock   = new Regex(string.Format("^{0} succeeded in opening the lock!$", playername));
        internal static readonly Regex FailOpenLock = new Regex(string.Format("^{0} failed to open the lock\\.$", playername));
        internal static readonly Regex OpenLockWithKey = new Regex(string.Format("^{0} uses {1} and opens the lock!$", playername, item));
        internal static readonly Regex LootReqr = new Regex(string.Format("^You do not meet the requirements to obtain {0}\\.$", item));
        internal static readonly Regex LootLost   = new Regex(string.Format("^{0} (?!was )lost\\.$", item));
        internal static readonly Regex LotItem    = new Regex(string.Format("^{0}'s lot for {1}: {2} points\\.$", playername, item, number));
        internal static readonly Regex Steal      = new Regex(string.Format("^{0} steals {1} from {2}\\.$", playername, item, target));
        internal static readonly Regex FailSteal  = new Regex(string.Format("^{0} fails to steal from {1}\\.$", playername, target));
        internal static readonly Regex Mug        = new Regex(string.Format("^{0} mugs {1} from {2}\\.$", playername, money, target));
        internal static readonly Regex FailMug    = new Regex(string.Format("^{0} fails to mug {1}\\.$", playername, target));
        internal static readonly Regex DiceRoll   = new Regex(string.Format("^Dice roll! {0} rolls {1}!$", playername, number));

        internal static readonly Regex EmitLight = new Regex(string.Format("^{0}'s body emits a (faint|feeble) {1}!$", playername, colorLight));
        internal static readonly Regex VisitantTE = new Regex(string.Format("^Your visitant status has been extended by {0} minutes\\.$", number));
        #endregion

        #region Preparing to take action
        internal static readonly Regex PrepSpell    = new Regex(string.Format("^{0} start(s)? casting {1}\\.$", name, spell));
        internal static readonly Regex PrepSpellOn  = new Regex(string.Format("^{0} start(s)? casting {1} on {2}\\.$", name, spell, target));
        internal static readonly Regex PrepAbility  = new Regex(string.Format("^{0} read(y|ies) {1}\\.$", name, ability));
        #endregion

        #region Completes action
        internal static readonly Regex CastSpell    = new Regex(string.Format("^{0} casts? {1}\\.$", name, spell));
        internal static readonly Regex CastSpellOn  = new Regex(string.Format("^{0} casts? {1} on {2}\\.$", name, spell, target));
        internal static readonly Regex UseAbility   = new Regex(string.Format("^{0} uses? {1}\\.$", name, ability));
        internal static readonly Regex UseAbilityOn = new Regex(string.Format("^{0} uses? {1} on {2}\\.$", name, ability, target));
        internal static readonly Regex MissAbility  = new Regex(string.Format("^{0} uses? {1}, but miss(es)? {2}\\.$", name, ability, target));
        internal static readonly Regex MissAbilityNoTarget = new Regex(string.Format("^{0} uses? {1}, but miss(es)?\\.$", name, ability, target));
        internal static readonly Regex FailsCharm   = new Regex(string.Format("^{0} fails? to charm {1}\\.$", name, target));
        internal static readonly Regex UseItem      = new Regex(string.Format("^{0} uses? {1}\\.$", name, item));
        // Corsair stuff (6f/65|70/66):
        internal static readonly Regex UseCorRoll   = new Regex(string.Format("^{0} uses {1}\\. The total comes to {2}!$", name, ability, number));
        internal static readonly Regex TotalCorRoll = new Regex(string.Format("^The total for {0} increases to {1}!$", ability, number));
        internal static readonly Regex GainCorRoll = new Regex(string.Format("^{0} receives the effect of {1}\\.$", playername, ability), RegexOptions.Compiled);
        internal static readonly Regex BustCorRoll  = new Regex(string.Format("^Bust!$"));
        internal static readonly Regex LoseCorRoll  = new Regex(string.Format("^{0} loses the effect of {1}\\.$", playername, ability));
        // Cover
        internal static readonly Regex UseCover     = new Regex(string.Format("^{0} covers {1}\\.$", name, target));
        // Accomplice/Collaborator
        internal static readonly Regex StealEnmity = new Regex(string.Format("^Enmity is stolen from {0}\\.$", target));
        #endregion

        #region Spell/Ability Effects
        internal static readonly Regex RecoversHP  = new Regex(string.Format("^{0} recovers? {1} HP\\.$", target, number), RegexOptions.Compiled);
        internal static readonly Regex RecoversMP  = new Regex(string.Format("^{0} recovers? {1} MP\\.$", target, number));
        internal static readonly Regex Afflict     = new Regex(string.Format("^{0} (is|are) afflicted with {1} {2}\\.$", target, effect, afflictLvl));
        internal static readonly Regex ShortEnfeeble = new Regex(string.Format(" (is|are) "), RegexOptions.Compiled);
        internal static readonly Regex Enfeeble    = new Regex(string.Format("{0} (is|are) {1}\\.$", target, effect), RegexOptions.Compiled);
        internal static readonly Regex Buff        = new Regex(string.Format("^{0} gains? the effect of {1}\\.$", target, effect), RegexOptions.Compiled);
        internal static readonly Regex GainResistance = new Regex(string.Format("^{0} gains? resistance against {1}\\.$", target, effect), RegexOptions.Compiled);
        internal static readonly Regex Debuff      = new Regex(string.Format("^{0} receives? the effect of {1}\\.$", target, effect), RegexOptions.Compiled);
        internal static readonly Regex Enhance     = new Regex(string.Format("^{0}'(s)? attacks are enhanced\\.$", target), RegexOptions.Compiled);
        internal static readonly Regex Charmed     = new Regex(string.Format("^{0} (is|are) now under {1}'s control\\.$", target, name));
        internal static readonly Regex NotCharmed  = new Regex(string.Format("^{0} (is|are) no longer charmed\\.$", target));
        internal static readonly Regex Dispelled   = new Regex(string.Format("^{0}'s? {1} effect disappears!$", target, effect), RegexOptions.Compiled);
        internal static readonly Regex RemoveStatus = new Regex(string.Format("^{0} successfully removes {1}'s {2}\\.$", name, target, effect), RegexOptions.Compiled);
        internal static readonly Regex ItemBuff    = new Regex(string.Format("^{0} receives? the effect of {1}\\.$", target, effect));
        internal static readonly Regex ItemCleanse = new Regex(string.Format("^{0} is no longer {1}\\.$", target, effect));
        internal static readonly Regex ReduceTP    = new Regex(string.Format("^{0}'s? TP is reduced( to 0)?\\.$", target));
        internal static readonly Regex Hide        = new Regex(string.Format("^{0} hides!$", name));
        #endregion

        #region Failed Actions
        internal static readonly Regex Interrupted = new Regex(string.Format("^{0}'(s)? casting is interrupted\\.$", name));
        internal static readonly Regex MoveInterrupt = new Regex(string.Format("^You move and interrupt your aim\\.$", name));
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
        internal static readonly Regex AdditionalEffect = new Regex(@"^Additional (E|e)ffect:", RegexOptions.Compiled);
        internal static readonly Regex MagicBurst       = new Regex(@"^Magic Burst!");
        internal static readonly Regex Cover            = new Regex(string.Format("^Cover! {0}", remainder));
        internal static readonly Regex AdditionalDamage = new Regex(string.Format("^Additional effect: {0} point(s)? of damage\\.$", damage));
        internal static readonly Regex AdditionalStatus = new Regex(string.Format("^Additional effect: {0}\\.$", effect));
        internal static readonly Regex AdditionalDrain  = new Regex(string.Format("^Additional effect: {0} HP drained from {1}\\.$", damage, target));
        internal static readonly Regex AdditionalAspir  = new Regex(string.Format("^Additional effect: {0} MP drained from {1}\\.$", damage, target));
        internal static readonly Regex AdditionalTP     = new Regex(string.Format("^Additional effect: {0} TP drained from {1}\\.$", damage, target));
        internal static readonly Regex AdditionalHeal   = new Regex(string.Format("^Additional effect: {0} recovers {1} HP\\.$", target, damage));
        #endregion

        #region Combat damage
        internal static readonly Regex MeleeHit          = new Regex(string.Format("^{0} hits? {1} for {2} points? of damage\\.$", name, target, damage));
        internal static readonly Regex RangedAttack      = new Regex(string.Format("^{0}'s? ranged attack hits? {1} for {2} points? of damage\\.$", name, target, damage));
        internal static readonly Regex RangedGoodSpot    = new Regex(string.Format("^{0}'s? ranged attack hits? {1} squarely for {2} points? of damage!$", name, target, damage));
        internal static readonly Regex RangedSweetSpot   = new Regex(string.Format("^{0}'s? ranged attack strikes true, pummeling {1} for {2} points? of damage!$", name, target, damage));
        internal static readonly Regex RangedHit         = new Regex(string.Format("^{0} uses? Ranged Attack\\.$", name));
        internal static readonly Regex CriticalHit       = new Regex(string.Format("^{0} scores? a critical hit!$", name));
        internal static readonly Regex RangedCriticalHit = new Regex(string.Format("^{0}'s? ranged attack scores a critical hit!$", name));
        internal static readonly Regex TargetTakesDamage = new Regex(string.Format("{0} takes? {1}( additional)? points? of damage\\.$", target, damage), RegexOptions.Compiled);
        internal static readonly Regex ShortTargetTakesDamage = new Regex(string.Format("takes? {0}( additional)? points? of damage\\.$", damage), RegexOptions.Compiled);
        internal static readonly Regex DamageAndStun     = new Regex(string.Format("^{0} takes? {1} points? of damage and is stunned\\.$", target, damage));
        internal static readonly Regex Spikes            = new Regex(string.Format("{0}'s? spikes deal {1} points? of damage to {2}\\.", name, damage, target));
        internal static readonly Regex DreadSpikes       = new Regex(string.Format("{0}'s? spikes drain {1} HP from {2}\\.", name, damage, target));
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
        internal static readonly Regex ResistSpell = new Regex(string.Format("^(Resist! )?{0} resist(s)? the (effects of the )?spell(\\.|!)$", target), RegexOptions.Compiled);
        internal static readonly Regex ResistEffect    = new Regex(string.Format("^{0} resist(s)? the effect\\.$", target));
        #endregion

        #region Drains
        internal static readonly Regex Drain = new Regex(string.Format("^{0} {1} drained from {2}\\.$", damage, drainType, target));
        internal static readonly Regex AbsorbStat = new Regex(string.Format("^{0}'(s)? {1} is drained\\.$", target, drainStat));
        #endregion

        #region Defeated
        internal static readonly Regex Defeated = new Regex(string.Format("^{0} was defeated by {1}\\.$", target, name));
        internal static readonly Regex Defeat   = new Regex(string.Format("^{0} defeats {1}\\.$", name, target));
        internal static readonly Regex Dies     = new Regex(string.Format("^{0} falls to the ground\\.$", target));
        #endregion
    }

    public class RegexUtility
    {
        public static readonly Regex ExcludedPlayer = new Regex("exclude", RegexOptions.IgnoreCase);
    }

    internal static class ParseExpressionsA
    {
        internal static void Reset(string parseCulture)
        {
            switch (parseCulture)
            {
                case "fr-FR":
                    LoadFrenchStrings();
                    break;
                case "de-DE":
                    LoadGermanStrings();
                    break;
                case "ja-JP":
                    //LoadJapaneseStrings();
                    //break;
                default:
                    LoadDefaultStrings();
                    break;
            }
        }

        /// <summary>
        /// English parse strings.
        /// </summary>
        private static void LoadDefaultStrings()
        {
            #region Named substrings
            playername   = @"(?<name>\w{3,16})";
            npcName      = @"(?<fullname>([Tt]he )?(?<name>\w+((,)|(\.\w{0,2})|['\- ](\d|\w)+)*))";

            name         = @"(?<fullname>([Tt]he )?(?<name>\w+(((('s (?=\w+'s))\w+)|('[^s]\w*('s)?)|(-(\w|\d)+)|(.\w+)|(the \w+)|( No.\d+)|( \w+)){0,3}|(( \w+)?'s \w+))))";
            target       = @"(?<fulltarget>([Tt]he )?(?<target>\w+(((('s (?=\w+'s))\w+)|('[^s]\w*('s)?)|(-(\w|\d)+)|(.\w+)|(the \w+)|( No.\d+)|( \w+)){0,3}|(( \w+)?'s \w+))))";
            repeatname   = @"([Tt]he )?(?<repeatname>\w+(((('s (?=\w+'s))\w+)|('[^s]\w*('s)?)|(-(\w|\d)+)|(.\w+)|(the \w+)|( No.\d+)|( \w+)){0,3}|(( \w+)?'s \w+)))";

            damage       = @"(?<damage>\d{1,5})";
            number       = @"(?<number>\d{1,5})";
            item         = @"(([Aa]|[Aa]n|[Tt]he) )?(?<item>(\?\?\? )?.{3,})";
            money        = @"((?<number>\d{1,5}?) gil)";
            spell        = @"(?<spell>\w+((: (Ichi|Ni|San))|(((('s |. |-)\w+)|(( \w+(?<! (on|III|II|IV|VI|V))){1,2}))?( (III|II|IV|VI|V))?))?)";
            ability      = @"(?<ability>\w+((: \w+)|(-\w+)( \w+)?|('s \w+)|( \w+)( \w+)?| of the \w+)?( \w+'\w{2,})?)";
            effect       = @"(?<effect>\w+( \w+(\.)?){0,2})";
            skillchain   = @"(?<skillchain>\w+)";

            afflictLvl   = @"\(lv\.\d\)";
            drainType    = @"(?<draintype>(H|M|T)P)";
            drainStat    = @"(?<drainstat>STR|DEX|AGI|VIT|INT|MND|CHR)";

            colorLight   = @"(?<color>pearlescent|azure|ruby|amber|ebon|gold|silver)";

            remainder    = @"(?<remainder>.*)";
            #endregion

            #region Chat name extractions
            ChatSay      = new Regex(string.Format("^{0} : (.+)$", playername));
            ChatParty    = new Regex(string.Format("^\\({0}\\) (.+)$", playername));
            ChatTell     = new Regex(string.Format("^(>>)?{0}(>>)? (.+)$", playername));
            ChatTellFrom = new Regex(string.Format("^{0}>> (.+)$", playername));
            ChatTellTo   = new Regex(string.Format("^>>{0} (.+)$", playername));
            ChatShout    = new Regex(string.Format("^{0} : (.+)$", playername));
            ChatLinkshell = new Regex(string.Format("^<{0}> (.+)$", playername));
            ChatEmote    = new Regex(string.Format("^{0}('s)? (.+)$", playername));
            ChatNPC      = new Regex(string.Format("^{0} : (.+)$", npcName));
            #endregion

            #region Defeated
            Defeated     = new Regex(string.Format("^{0} was defeated by {1}\\.$", target, name));
            Defeat       = new Regex(string.Format("^{0} defeats {1}\\.$", name, target));
            Dies         = new Regex(string.Format("^{0} falls to the ground\\.$", target));
            #endregion

            #region Experience
            ExpChain     = new Regex(string.Format("^(EXP|Limit) chain #{0}!$", number));
            Experience   = new Regex(string.Format("^{0} gains {1} (experience|limit) points\\.$", playername, number));
            NoExperience = new Regex(string.Format("^No experience gained\\.$"));
            #endregion

            #region Loot
            FindLootOn   = new Regex(string.Format("^You find {0} on {1}\\.$", item, target));
            FindLootIn   = new Regex(string.Format("^You find {0} in {1}\\.$", item, target));
            OpenChest    = new Regex(string.Format("^{0} opens {1} and finds {2}\\.$", playername, target, item));
            GetLoot      = new Regex(string.Format("^{0} obtains {1}(\\.|!)$", playername, item));
            GetGil       = new Regex(string.Format("^{0} obtains {1}\\.$", playername, money));
            GetCruor     = new Regex(string.Format("^{0} obtains {1}\\.$", playername, cruor));
            TreasureChest = new Regex(string.Format("^The monster was concealing a treasure chest!$"));
            OpenLock     = new Regex(string.Format("^{0} succeeded in opening the lock!$", playername));
            LootReqr     = new Regex(string.Format("^You do not meet the requirements to obtain {0}\\.$", item));
            LootLost     = new Regex(string.Format("^{0} (?!was )lost\\.$", item));
            LotItem      = new Regex(string.Format("^{0}'s lot for {1}: {2} points\\.$", playername, item, number));
            Steal        = new Regex(string.Format("^{0} steals {1} from {2}\\.$", playername, item, target));
            FailSteal    = new Regex(string.Format("^{0} fails to steal from {1}\\.$", playername, target));
            Mug          = new Regex(string.Format("^{0} mugs {1} from {2}\\.$", playername, money, target));
            FailMug      = new Regex(string.Format("^{0} fails to mug {1}\\.$", playername, target));
            DiceRoll     = new Regex(string.Format("^Dice roll! {0} rolls {1}!$", playername, number));

            EmitLight = new Regex(string.Format("^{0}'s body emits a faint {1} light!$", playername, colorLight));
            #endregion

            #region Preparing to take action
            PrepSpell    = new Regex(string.Format("^{0} starts? casting {1}\\.$", name, spell));
            PrepSpellOn  = new Regex(string.Format("^{0} starts? casting {1} on {2}\\.$", name, spell, target));
            PrepAbility  = new Regex(string.Format("^{0} read(y|ies) {1}\\.$", name, ability));
            #endregion

            #region Completes action
            CastSpell    = new Regex(string.Format("^{0} casts? {1}\\.$", name, spell));
            CastSpellOn  = new Regex(string.Format("^{0} casts? {1} on {2}\\.$", name, spell, target));
            UseAbility   = new Regex(string.Format("^{0} uses? {1}\\.$", name, ability));
            UseAbilityOn = new Regex(string.Format("^{0} uses? {1} on {2}\\.$", name, ability, target));
            MissAbility  = new Regex(string.Format("^{0} uses? {1}, but miss(es)? {2}\\.$", name, ability, target));
            MissAbilityNoTarget = new Regex(string.Format("^{0} use(s)? {1}, but miss(es)?\\.$", name, ability, target));
            FailsCharm   = new Regex(string.Format("^{0} fails? to charm {1}\\.$", name, target));
            UseItem      = new Regex(string.Format("^{0} uses? {1}\\.$", name, item));
            // Corsair stuff (6f/65|70/66):
            UseCorRoll   = new Regex(string.Format("^{0} uses {1}\\. The total comes to {2}!$", name, ability, number));
            TotalCorRoll = new Regex(string.Format("^The total for {0} increases to {1}!$", ability, number));
            GainCorRoll  = new Regex(string.Format("^{0} receives the effect of {1}\\.$", playername, ability));
            BustCorRoll  = new Regex(string.Format("^Bust!$"));
            LoseCorRoll  = new Regex(string.Format("^{0} loses the effect of {1}\\.$", playername, ability));
            // Cover
            UseCover     = new Regex(string.Format("^{0} covers {1}\\.$", name, target));
            // Accomplice/Collaborator
            StealEnmity  = new Regex(string.Format("^Enmity is stolen from {0}\\.$", target));
            #endregion

            #region Spell/Ability Effects
            RecoversHP   = new Regex(string.Format("^{0} recovers? {1} HP\\.$", target, number));
            RecoversMP   = new Regex(string.Format("^{0} recovers? {1} MP\\.$", target, number));
            Afflict      = new Regex(string.Format("^{0} (is|are) afflicted with {1} {2}\\.$", target, effect, afflictLvl));
            Enfeeble     = new Regex(string.Format("{0} (is|are) {1}\\.$", target, effect));
            Buff         = new Regex(string.Format("^{0} gains? the effect of {1}\\.$", target, effect));
            GainResistance = new Regex(string.Format("^{0} gains? resistance against {1}\\.$", target, effect));
            Debuff       = new Regex(string.Format("^{0} receives? the effect of {1}\\.$", target, effect));
            Enhance      = new Regex(string.Format("^{0}'s? attacks are enhanced\\.$", target));
            Charmed      = new Regex(string.Format("^{0} (is|are) now under {1}'s control\\.$", target, name));
            NotCharmed   = new Regex(string.Format("^{0} (is|are) no longer charmed\\.$", target));
            Dispelled    = new Regex(string.Format("^{0}'s? {1} effect disappears!$", target, effect));
            RemoveStatus = new Regex(string.Format("^{0} successfully removes {1}'s {2}$", name, target, effect));
            ItemBuff     = new Regex(string.Format("^{0} receives? the effect of {1}\\.$", target, effect));
            ItemCleanse  = new Regex(string.Format("^{0} is no longer {1}\\.$", target, effect));
            ReduceTP     = new Regex(string.Format("^{0}'s? TP is reduced( to 0)?\\.$", target));
            Hide         = new Regex(string.Format("^{0} hides!$", name));
            #endregion

            #region Failed Actions
            Interrupted  = new Regex(string.Format("^{0}'s? casting is interrupted\\.$", name));
            MoveInterrupt = new Regex(string.Format("^You move and interrupt your aim\\.$", name));
            Paralyzed    = new Regex(string.Format("^{0} (is|are) paralyzed\\.$", name));
            CannotSee    = new Regex(string.Format("^Unable to see {0}\\.$", target));
            CannotSee2   = new Regex(string.Format("^You cannot see {0}\\.$", target));
            TooFarAway   = new Regex(string.Format("^{0} (is|are) too far away\\.$", target));
            OutOfRange   = new Regex(string.Format("^{0} (is|are) out of range\\.$", target));
            CannotAttack = new Regex(string.Format("^You cannot attack that target\\.$"));
            NotEnoughTP  = new Regex(string.Format("^You do not have enough TP\\.$"));
            NotEnoughTP2 = new Regex(string.Format("^Not enough TP\\.$"));
            NotEnoughTP3 = new Regex(string.Format("^{0} does not have enough TP\\.$", name));
            NotEnoughMP  = new Regex(string.Format("^You do not have enough MP\\.$", target));
            Intimidated  = new Regex(string.Format("^{0} (is|are) intimidated by {1}'s presence\\.$", name, target));
            UnableToCast = new Regex(string.Format("^Unable to cast spells at this time\\.$"));
            UnableToUse  = new Regex(string.Format("^Unable to use job ability\\.$"));
            UnableToUse2 = new Regex(string.Format("^Unable to use weapon skill\\.$"));
            UnableToUse3 = new Regex(string.Format("^{0} is unable to use weapon skills\\.$", name));
            NoEffect     = new Regex(string.Format("^{0}'s? {1} has no effect on {2}\\.$", name, spell, target));
            NoEffect2    = new Regex(string.Format("^No effect on {0}\\.$", target));
            NoEffect3    = new Regex(string.Format("^{0} casts {1} on {2}, but the spell fails to take effect\\.$", name, spell, target));
            AutoTarget   = new Regex(string.Format("^Auto-targeting {0}\\.$", target));
            TooFarForXP  = new Regex(string.Format("^You are too far from the battle to gain experience\\.$"));
            LoseSight    = new Regex(string.Format("^You lose sight of {0}\\.$", target));
            FailActivate = new Regex(string.Format("^{0} fails? to activate\\.$", item));
            CannotPerform = new Regex(string.Format("^{0} cannot perform that action\\.$", name));
            FailHide     = new Regex(string.Format("^{0} (try|tries) to hide, but (is|are) spotted by {1}\\.$", name, target));
            #endregion

            #region Modifiers on existing lines
            AdditionalEffect = new Regex(@"^Additional (E|e)ffect:");
            MagicBurst       = new Regex(@"^Magic Burst!");
            Cover            = new Regex(string.Format("^Cover! {0}", remainder));
            AdditionalDamage = new Regex(string.Format("^Additional effect: {0} point(s)? of damage\\.$", damage));
            AdditionalStatus = new Regex(string.Format("^Additional effect: {0}\\.$", effect));
            AdditionalDrain  = new Regex(string.Format("^Additional effect: {0} HP drained from {1}\\.$", damage, target));
            AdditionalAspir  = new Regex(string.Format("^Additional effect: {0} MP drained from {1}\\.$", damage, target));
            AdditionalHeal   = new Regex(string.Format("^Additional effect: {0} recovers {1} HP\\.$", target, damage));
            #endregion

            #region Combat damage
            MeleeHit        = new Regex(string.Format("^{0} hits? {1} for {2} point(s)? of damage\\.$", name, target, damage));
            RangedAttack    = new Regex(string.Format("^{0}'s? ranged attack hits? {1} for {2} points? of damage\\.$", name, target, damage));
            RangedGoodSpot  = new Regex(string.Format("^{0}'s? ranged attack hits? {1} squarely for {2} points? of damage!$", name, target, damage));
            RangedSweetSpot = new Regex(string.Format("^{0}'s? ranged attack strikes true, pummeling {1} for {2} points? of damage!$", name, target, damage));
            RangedHit       = new Regex(string.Format("^{0} uses? Ranged Attack\\.$", name));
            CriticalHit     = new Regex(string.Format("^{0} scores? a critical hit!$", name));
            RangedCriticalHit = new Regex(string.Format("^{0}'s? ranged attack scores a critical hit!$", name));
            TargetTakesDamage = new Regex(string.Format("{0} takes? {1}( additional)? point(s)? of damage\\.$", target, damage));
            DamageAndStun   = new Regex(string.Format("^{0} takes? {1} point(s)? of damage and is stunned\\.$", target, damage));
            Spikes          = new Regex(string.Format("{0}'s? spikes deal {1} point(s)? of damage to {2}\\.", name, damage, target));
            DreadSpikes     = new Regex(string.Format("{0}'s? spikes drain {1} HP from {2}\\.", name, damage, target));
            Skillchain      = new Regex(string.Format("^Skillchain: {0}\\.$", skillchain));
            #endregion

            #region Combat defenses
            MeleeMiss    = new Regex(string.Format("^{0} miss(es)? {1}\\.$", name, target));
            MeleeDodge   = new Regex(string.Format("^{0} dodges the attack\\.$", target));
            RangedMiss   = new Regex(string.Format("^{0} use(s)? Ranged Attack, but miss(es)? {1}\\.$", name, target));
            RangedMiss2  = new Regex(string.Format("^{0}'s? ranged attack misses\\.$", name));
            Blink        = new Regex(string.Format("^{0} of {1}'s shadows absorbs? the damage and disappears?\\.$", number, target));
            Parry        = new Regex(string.Format("^{0} parr(y|ies) {1}'s? attack with (his|her|its) weapon\\.$", target, name));
            Anticipate   = new Regex(string.Format("^{0} anticipates? {1}'s? attack\\.$", target, name));
            Anticipate2  = new Regex(string.Format("^{0} anticipates? the attack\\.$", target));
            Evade        = new Regex(string.Format("^{0} evade(s)?( the attack)?\\.$", target));
            Counter      = new Regex(string.Format("^{0}'s? attack is countered by {1}\\. {2} takes {3} points? of damage\\.$",
                target, name, repeatname, damage));
            CounterShadow = new Regex(string.Format("^{0}'s? attack is countered by {1}\\. {2} of {3}'(s)? shadows absorbs the damage and disappears\\.$",
                target, name, number, repeatname));
            Retaliate    = new Regex(string.Format("^{0} retaliates\\. {1} takes {2} points? of damage\\.$",
                name, target, damage));
            RetaliateShadow = new Regex(string.Format("^{0} retaliates\\. {1} of {2}'s? shadows absorbs the damage and disappears\\.$",
                name, number, target));
            ResistSpell  = new Regex(string.Format("^(Resist! )?{0} resists? the (effects of the )?spell(\\.|!)$", target));
            ResistEffect = new Regex(string.Format("^{0} resists? the effect\\.$", target));
            #endregion

            #region Drains
            Drain        = new Regex(string.Format("^{0} {1} drained from {2}\\.$", damage, drainType, target));
            AbsorbStat   = new Regex(string.Format("^{0}'s? {1} is drained\\.$", target, drainStat));
            #endregion
        }

        /// <summary>
        /// French localized parse strings.
        /// </summary>
        private static void LoadFrenchStrings()
        {
            #region Named substrings
            playername   = @"(?<name>\w{3,16})";
            npcName      = @"(?<fullname>([Tt]he )?(?<name>\w+((,)|(\.\w{0,2})|['\- ](\d|\w)+)*))";

            name         = @"(?<fullname>([Tt]he )?(?<name>\w+(((('s (?=\w+'s))\w+)|('[^s]\w*('s)?)|(-(\w|\d)+)|(.\w+)|(the \w+)|( No.\d+)|( \w+)){0,3}|(( \w+)?'s \w+))))";
            target       = @"(?<fulltarget>([Tt]he )?(?<target>\w+(((('s (?=\w+'s))\w+)|('[^s]\w*('s)?)|(-(\w|\d)+)|(.\w+)|(the \w+)|( No.\d+)|( \w+)){0,3}|(( \w+)?'s \w+))))";
            repeatname   = @"([Tt]he )?(?<repeatname>\w+(((('s (?=\w+'s))\w+)|('[^s]\w*('s)?)|(-(\w|\d)+)|(.\w+)|(the \w+)|( No.\d+)|( \w+)){0,3}|(( \w+)?'s \w+)))";

            damage       = @"(?<damage>\d{1,5})";
            number       = @"(?<number>\d{1,3}( \d{3})*)";
            item         = @"(([Aa]|[Aa]n|[Tt]he) )?(?<item>(\?\?\? )?.{3,})";
            money        = @"((?<number>\d{1,5}?) gil)";
            cruor        = @"((?<number>\d{1,5}?) cruor)";
            spell        = @"(?<spell>\w+(( : (Ichi|Ni|San))|(((('s |. |-)\w+)|(( \w+(?<! (on|III|II|IV|VI|V))){1,2}))?( (III|II|IV|VI|V))?))?)";
            ability      = @"(?<ability>\w+((: \w+)|(-\w+)( \w+)?|('s \w+)|( \w+)( \w+)?| of the \w+)?( \w+'\w{2,})?)";
            effect       = @"(?<effect>\w+( \w+(\.)?){0,2})";
            skillchain   = @"(?<skillchain>\w+)";

            afflictLvl   = @"\(lv\.\d\)";
            drainType    = @"(?<draintype>(H|M|T)P)";
            drainStat    = @"(?<drainstat>STR|DEX|AGI|VIT|INT|MND|CHR)";

            colorLight = @"(?<color>pearlescent|azure|ruby|amber|ebon|gold|silver)";

            remainder = @"(?<remainder>.*)";
            #endregion

            #region Chat name extractions
            ChatSay       = new Regex(string.Format("^{0} : (.+)$", playername));
            ChatParty     = new Regex(string.Format("^\\({0}\\) (.+)$", playername));
            ChatTell      = new Regex(string.Format("^(>>)?{0}(>>)?( :)? (.+)$", playername));
            ChatTellFrom  = new Regex(string.Format("^{0}>> (.+)$", playername));
            ChatTellTo    = new Regex(string.Format("^>>{0} : (.+)$", playername));
            ChatShout     = new Regex(string.Format("^{0} : (.+)$", playername));
            ChatLinkshell = new Regex(string.Format("^<{0}> (.+)$", playername));
            ChatEmote     = new Regex(string.Format("^{0} (.+)$", playername));
            ChatNPC       = new Regex(string.Format("^{0} : (.+)$", npcName));
            #endregion

            #region Defeated
            Defeated     = new Regex(string.Format("^{0} was defeated by {1}\\.$", target, name));
            Defeat       = new Regex(string.Format("^{0} defeats {1}\\.$", name, target));
            Dies         = new Regex(string.Format("^{0} falls to the ground\\.$", target));
            #endregion

            #region Experience
            ExpChain     = new Regex(string.Format("^{0} chaînes? (d'expérience|de limite) !$", number));
            Experience   = new Regex(string.Format("^{0} gagne {1} points? (d'expérience|de limite)\\.$", playername, number));
            NoExperience = new Regex(string.Format("^Aucun point d'expérience gagné\\.\\.\\.$"));
            #endregion

            #region Loot
            FindLootOn   = new Regex(string.Format("^You find {0} on {1}\\.$", item, target));
            FindLootIn   = new Regex(string.Format("^You find {0} in {1}\\.$", item, target));
            OpenChest    = new Regex(string.Format("^{0} opens {1} and finds {2}\\.$", playername, target, item));
            GetLoot      = new Regex(string.Format("^{0} obtains {1}(\\.|!)$", playername, item));
            GetGil       = new Regex(string.Format("^{0} obtains {1}\\.$", playername, money));
            GetCruor     = new Regex(string.Format("^{0} obtains {1}\\.$", playername, cruor));
            TreasureChest = new Regex(string.Format("^The monster was concealing a treasure chest!$"));
            OpenLock = new Regex(string.Format("^{0} succeeded in opening the lock!$", playername));
            LootReqr = new Regex(string.Format("^You do not meet the requirements to obtain {0}\\.$", item));
            LootLost     = new Regex(string.Format("^{0} (?!was )lost\\.$", item));
            LotItem      = new Regex(string.Format("^{0}'s lot for {1}: {2} points\\.$", playername, item, number));
            Steal        = new Regex(string.Format("^{0} steals {1} from {2}\\.$", playername, item, target));
            FailSteal    = new Regex(string.Format("^{0} fails to steal from {1}\\.$", playername, target));
            Mug          = new Regex(string.Format("^{0} mugs {1} from {2}\\.$", playername, money, target));
            FailMug      = new Regex(string.Format("^{0} fails to mug {1}\\.$", playername, target));
            EmitLight = new Regex(string.Format("^{0}'s body emits a faint {1} light!$", playername, colorLight));
            #endregion

            #region Preparing to take action
            PrepSpell    = new Regex(string.Format("^{0} commence à lancer {1}\\.$", name, spell));
            PrepSpellOn  = new Regex(string.Format("^{0} commence à lancer {1} sur {2}\\.$", name, spell, target));
            PrepAbility  = new Regex(string.Format("^{0} prépare {1}\\.$", name, ability));
            #endregion

            #region Completes action
            CastSpell    = new Regex(string.Format("^{0} cast(s)? {1}\\.$", name, spell));
            CastSpellOn  = new Regex(string.Format("^{0} cast(s)? {1} on {2}\\.$", name, spell, target));
            UseAbility   = new Regex(string.Format("^{0} use(s)? {1}\\.$", name, ability));
            UseAbilityOn = new Regex(string.Format("^{0} use(s)? {1} on {2}\\.$", name, ability, target));
            MissAbility  = new Regex(string.Format("^{0} use(s)? {1}, but miss(es)? {2}\\.$", name, ability, target));
            MissAbilityNoTarget = new Regex(string.Format("^{0} use(s)? {1}, but miss(es)?\\.$", name, ability, target));
            FailsCharm   = new Regex(string.Format("^{0} fail(s)? to charm {1}\\.$", name, target));
            UseItem      = new Regex(string.Format("^{0} use(s)? {1}\\.$", name, item));
            // Corsair stuff (6f/65|70/66):
            DiceRoll     = new Regex(string.Format("^Dice roll! {0} rolls {1}!$", playername, number));
            UseCorRoll   = new Regex(string.Format("^{0} utilise {1}\\. {2} sort un {3} !$", name, ability, repeatname, number));
            TotalCorRoll = new Regex(string.Format("^Le nombre total de {0} passe à {1} !$", ability, number));
            GainCorRoll  = new Regex(string.Format("^{0} bénéficie de l'effet {1}\\.$", playername, ability));
            BustCorRoll  = new Regex(string.Format("^Bust !$"));
            LoseCorRoll  = new Regex(string.Format("^L'effet {0} sur {1} disparaît\\.$", ability, playername));
            // Cover
            UseCover     = new Regex(string.Format("^{0} covers {1}\\.$", name, target));
            // Accomplice/Collaborator
            StealEnmity  = new Regex(string.Format("^Enmity is stolen from {0}\\.$", target));
            #endregion

            #region Spell/Ability Effects
            RecoversHP   = new Regex(string.Format("^{0} recover(s)? {1} HP\\.$", target, number));
            RecoversMP   = new Regex(string.Format("^{0} recover(s)? {1} MP\\.$", target, number));
            Afflict      = new Regex(string.Format("^{0} (is|are) afflicted with {1} {2}\\.$", target, effect, afflictLvl));
            Enfeeble     = new Regex(string.Format("{0} (is|are) {1}\\.$", target, effect));
            Buff         = new Regex(string.Format("^{0} gain(s)? the effect of {1}\\.$", target, effect));
            GainResistance = new Regex(string.Format("^{0} gain(s)? resistance against {1}\\.$", target, effect));
            Debuff       = new Regex(string.Format("^{0} receive(s)? the effect of {1}\\.$", target, effect));
            Enhance      = new Regex(string.Format("^{0}'(s)? attacks are enhanced\\.$", target));
            Charmed      = new Regex(string.Format("^{0} (is|are) now under {1}'s control\\.$", target, name));
            NotCharmed   = new Regex(string.Format("^{0} (is|are) no longer charmed\\.$", target));
            Dispelled    = new Regex(string.Format("^{0}'(s)? {1} effect disappears!$", target, effect));
            RemoveStatus = new Regex(string.Format("^{0} successfully removes {1}'s {2}$", name, target, effect));
            ItemBuff     = new Regex(string.Format("^{0} receive(s)? the effect of {1}\\.$", target, effect));
            ItemCleanse  = new Regex(string.Format("^{0} is no longer {1}\\.$", target, effect));
            ReduceTP     = new Regex(string.Format("^{0}'s? TP is reduced( to 0)?\\.$", target));
            Hide         = new Regex(string.Format("^{0} hides!$", name));
            #endregion

            #region Failed Actions
            Interrupted  = new Regex(string.Format("^{0}'(s)? casting is interrupted\\.$", name));
            Paralyzed    = new Regex(string.Format("^{0} (is|are) paralyzed\\.$", name));
            CannotSee    = new Regex(string.Format("^Unable to see {0}\\.$", target));
            CannotSee2   = new Regex(string.Format("^You cannot see {0}\\.$", target));
            TooFarAway   = new Regex(string.Format("^{0} (is|are) too far away\\.$", target));
            OutOfRange   = new Regex(string.Format("^{0} (is|are) out of range\\.$", target));
            CannotAttack = new Regex(string.Format("^You cannot attack that target\\.$"));
            NotEnoughTP  = new Regex(string.Format("^You do not have enough TP\\.$"));
            NotEnoughTP2 = new Regex(string.Format("^Not enough TP\\.$"));
            NotEnoughTP3 = new Regex(string.Format("^{0} does not have enough TP\\.$", name));
            NotEnoughMP  = new Regex(string.Format("^You do not have enough MP\\.$", target));
            Intimidated  = new Regex(string.Format("^{0} (is|are) intimidated by {1}'s presence\\.$", name, target));
            UnableToCast = new Regex(string.Format("^Unable to cast spells at this time\\.$"));
            UnableToUse  = new Regex(string.Format("^Unable to use job ability\\.$"));
            UnableToUse2 = new Regex(string.Format("^Unable to use weapon skill\\.$"));
            UnableToUse3 = new Regex(string.Format("^{0} is unable to use weapon skills\\.$", name));
            NoEffect     = new Regex(string.Format("^{0}'(s)? {1} has no effect on {2}\\.$", name, spell, target));
            NoEffect2    = new Regex(string.Format("^No effect on {0}\\.$", target));
            NoEffect3    = new Regex(string.Format("^{0} casts {1} on {2}, but the spell fails to take effect\\.$", name, spell, target));
            AutoTarget   = new Regex(string.Format("^Auto-targeting {0}\\.$", target));
            TooFarForXP  = new Regex(string.Format("^You are too far from the battle to gain experience\\.$"));
            LoseSight    = new Regex(string.Format("^You lose sight of {0}\\.$", target));
            FailActivate = new Regex(string.Format("^{0} fails? to activate\\.$", item));
            CannotPerform = new Regex(string.Format("^{0} cannot perform that action\\.$", name));
            FailHide     = new Regex(string.Format("^{0} (try|tries) to hide, but (is|are) spotted by {1}\\.$", name, target));
            #endregion

            #region Modifiers on existing lines
            AdditionalEffect = new Regex(@"^Additional (E|e)ffect:");
            MagicBurst       = new Regex(@"^Magic Burst!");
            Cover            = new Regex(string.Format("^Cover! {0}", remainder));
            AdditionalDamage = new Regex(string.Format("^Additional effect: {0} point(s)? of damage\\.$", damage));
            AdditionalStatus = new Regex(string.Format("^Additional effect: {0}\\.$", effect));
            AdditionalDrain  = new Regex(string.Format("^Additional effect: {0} HP drained from {1}\\.$", damage, target));
            AdditionalAspir  = new Regex(string.Format("^Additional effect: {0} MP drained from {1}\\.$", damage, target));
            AdditionalHeal   = new Regex(string.Format("^Additional effect: {0} recovers {1} HP\\.$", target, damage));
            #endregion

            #region Combat damage
            MeleeHit        = new Regex(string.Format("^{0} hit(s)? {1} for {2} point(s)? of damage\\.$", name, target, damage));
            RangedAttack    = new Regex(string.Format("^{0}'s? ranged attack hits? {1} for {2} points? of damage\\.$", name, target, damage));
            RangedGoodSpot  = new Regex(string.Format("^{0}'s? ranged attack hits? {1} squarely for {2} points? of damage!$", name, target, damage));
            RangedSweetSpot = new Regex(string.Format("^{0}'s? ranged attack strikes true, pummeling {1} for {2} points? of damage!$", name, target, damage));
            RangedHit       = new Regex(string.Format("^{0} use(s)? Ranged Attack\\.$", name));
            CriticalHit     = new Regex(string.Format("^{0} score(s)? a critical hit!$", name));
            RangedCriticalHit = new Regex(string.Format("^{0}'(s)? ranged attack scores a critical hit!$", name));
            TargetTakesDamage = new Regex(string.Format("{0} take(s)? {1}( additional)? point(s)? of damage\\.$", target, damage));
            DamageAndStun   = new Regex(string.Format("^{0} take(s)? {1} point(s)? of damage and is stunned\\.$", target, damage));
            Spikes          = new Regex(string.Format("{0}'s? spikes deal {1} point(s)? of damage to {2}\\.", name, damage, target));
            DreadSpikes     = new Regex(string.Format("{0}'s? spikes drain {1} HP from {2}\\.", name, damage, target));
            Skillchain      = new Regex(string.Format("^Skillchain: {0}\\.$", skillchain));
            #endregion

            #region Combat defenses
            MeleeMiss    = new Regex(string.Format("^{0} miss(es)? {1}\\.$", name, target));
            MeleeDodge   = new Regex(string.Format("^{0} dodges the attack\\.$", target));
            RangedMiss   = new Regex(string.Format("^{0} use(s)? Ranged Attack, but miss(es)? {1}\\.$", name, target));
            RangedMiss2  = new Regex(string.Format("^{0}'(s)? ranged attack misses\\.$", name));
            Blink        = new Regex(string.Format("^{0} of {1}'s shadows absorb(s)? the damage and disappear(s)?\\.$", number, target));
            Parry        = new Regex(string.Format("^{0} parr(y|ies) {1}'(s)? attack with (his|her|its) weapon\\.$", target, name));
            Anticipate   = new Regex(string.Format("^{0} anticipate(s)? {1}'(s)? attack\\.$", target, name));
            Anticipate2  = new Regex(string.Format("^{0} anticipate(s)? the attack\\.$", target));
            Evade        = new Regex(string.Format("^{0} evade(s)?( the attack)?\\.$", target));
            Counter      = new Regex(string.Format("^{0}'(s)? attack is countered by {1}\\. {2} takes {3} point(s)? of damage\\.$",
                target, name, repeatname, damage));
            CounterShadow = new Regex(string.Format("^{0}'(s)? attack is countered by {1}\\. {2} of {3}'(s)? shadows absorbs the damage and disappears\\.$",
                target, name, number, repeatname));
            Retaliate    = new Regex(string.Format("^{0} retaliates\\. {1} takes {2} points? of damage\\.$",
                name, target, damage));
            RetaliateShadow = new Regex(string.Format("^{0} retaliates\\. {1} of {2}'s? shadows absorbs the damage and disappears\\.$",
                name, number, target));
            ResistSpell  = new Regex(string.Format("^(Resist! )?{0} resist(s)? the (effects of the )?spell(\\.|!)$", target));
            ResistEffect = new Regex(string.Format("^{0} resist(s)? the effect\\.$", target));
            #endregion

            #region Drains
            Drain        = new Regex(string.Format("^{0} {1} drained from {2}\\.$", damage, drainType, target));
            AbsorbStat   = new Regex(string.Format("^{0}'(s)? {1} is drained\\.$", target, drainStat));
            #endregion
        }

        /// <summary>
        /// German localized parse strings.
        /// </summary>
        private static void LoadGermanStrings()
        {
            #region Named substrings
            playername   = @"(?<name>\w{3,16})";
            npcName      = @"(?<fullname>([Tt]he )?(?<name>\w+((,)|(\.\w{0,2})|['\- ](\d|\w)+)*))";

            name         = @"(?<fullname>([Tt]he )?(?<name>\w+(((('s (?=\w+'s))\w+)|('[^s]\w*('s)?)|(-(\w|\d)+)|(.\w+)|(the \w+)|( No.\d+)|( \w+)){0,3}|(( \w+)?'s \w+))))";
            target       = @"(?<fulltarget>([Tt]he )?(?<target>\w+(((('s (?=\w+'s))\w+)|('[^s]\w*('s)?)|(-(\w|\d)+)|(.\w+)|(the \w+)|( No.\d+)|( \w+)){0,3}|(( \w+)?'s \w+))))";
            repeatname   = @"([Tt]he )?(?<repeatname>\w+(((('s (?=\w+'s))\w+)|('[^s]\w*('s)?)|(-(\w|\d)+)|(.\w+)|(the \w+)|( No.\d+)|( \w+)){0,3}|(( \w+)?'s \w+)))";

            damage       = @"(?<damage>\d{1,5})";
            number       = @"(?<number>\d{1,5})";
            item         = @"(([Aa]|[Aa]n|[Tt]he) )?(?<item>(\?\?\? )?.{3,})";
            money        = @"((?<number>\d{1,5}?) gil)";
            spell        = @"(?<spell>\w+(( : (Ichi|Ni|San))|(((('s |. |-)\w+)|(( \w+(?<! (on|III|II|IV|VI|V))){1,2}))?( (III|II|IV|VI|V))?))?)";
            ability      = @"(?<ability>\w+((: \w+)|(-\w+)( \w+)?|('s \w+)|( \w+)( \w+)?| of the \w+)?( \w+'\w{2,})?)";
            effect       = @"(?<effect>\w+( \w+(\.)?){0,2})";
            skillchain   = @"(?<skillchain>\w+)";

            afflictLvl   = @"\(lv\.\d\)";
            drainType    = @"(?<draintype>(H|M|T)P)";
            drainStat    = @"(?<drainstat>STR|DEX|AGI|VIT|INT|MND|CHR)";

            colorLight = @"(?<color>pearlescent|azure|ruby|amber|ebon|gold|silver)";

            remainder = @"(?<remainder>.*)";
            #endregion

            #region Chat name extractions
            ChatSay      = new Regex(string.Format("^{0} : (.+)$", playername));
            ChatParty    = new Regex(string.Format("^\\({0}\\) (.+)$", playername));
            ChatTell     = new Regex(string.Format("^(>>)?{0}(>>)?( :)? (.+)$", playername));
            ChatTellFrom = new Regex(string.Format("^{0}>> (.+)$", playername));
            ChatTellTo   = new Regex(string.Format("^>>{0} : (.+)$", playername));
            ChatShout    = new Regex(string.Format("^{0} : (.+)$", playername));
            ChatLinkshell = new Regex(string.Format("^<{0}> (.+)$", playername));
            ChatEmote    = new Regex(string.Format("^{0} (.+)$", playername));
            ChatNPC      = new Regex(string.Format("^{0} : (.+)$", npcName));
            #endregion

            #region Defeated
            Defeated     = new Regex(string.Format("^{0} was defeated by {1}\\.$", target, name));
            Defeat       = new Regex(string.Format("^{0} defeats {1}\\.$", name, target));
            Dies         = new Regex(string.Format("^{0} falls to the ground\\.$", target));
            #endregion

            #region Experience
            ExpChain     = new Regex(string.Format("^(EXP|Limit) chain #{0}!$", number));
            Experience   = new Regex(string.Format("^{0} gains {1} (experience|limit) points\\.$", playername, number));
            NoExperience = new Regex(string.Format("^No experience gained\\.$"));
            #endregion

            #region Loot
            FindLootOn   = new Regex(string.Format("^You find {0} on {1}\\.$", item, target));
            FindLootIn   = new Regex(string.Format("^You find {0} in {1}\\.$", item, target));
            OpenChest    = new Regex(string.Format("^{0} opens {1} and finds {2}\\.$", playername, target, item));
            GetLoot      = new Regex(string.Format("^{0} obtains {1}(\\.|!)$", playername, item));
            GetGil       = new Regex(string.Format("^{0} obtains {1}\\.$", playername, money));
            GetCruor     = new Regex(string.Format("^{0} obtains {1}\\.$", playername, cruor));
            TreasureChest = new Regex(string.Format("^The monster was concealing a treasure chest!$"));
            OpenLock = new Regex(string.Format("^{0} succeeded in opening the lock!$", playername));
            LootReqr = new Regex(string.Format("^You do not meet the requirements to obtain {0}\\.$", item));
            LootLost     = new Regex(string.Format("^{0} (?!was )lost\\.$", item));
            LotItem      = new Regex(string.Format("^{0}'s lot for {1}: {2} points\\.$", playername, item, number));
            Steal        = new Regex(string.Format("^{0} steals {1} from {2}\\.$", playername, item, target));
            FailSteal    = new Regex(string.Format("^{0} fails to steal from {1}\\.$", playername, target));
            Mug          = new Regex(string.Format("^{0} mugs {1} from {2}\\.$", playername, money, target));
            FailMug      = new Regex(string.Format("^{0} fails to mug {1}\\.$", playername, target));
            EmitLight = new Regex(string.Format("^{0}'s body emits a faint {1} light!$", playername, colorLight));
            #endregion

            #region Preparing to take action
            PrepSpell    = new Regex(string.Format("^{0} start(s)? casting {1}\\.$", name, spell));
            PrepSpellOn  = new Regex(string.Format("^{0} start(s)? casting {1} on {2}\\.$", name, spell, target));
            PrepAbility  = new Regex(string.Format("^{0} read(y|ies) {1}\\.$", name, ability));
            #endregion

            #region Completes action
            CastSpell    = new Regex(string.Format("^{0} cast(s)? {1}\\.$", name, spell));
            CastSpellOn  = new Regex(string.Format("^{0} cast(s)? {1} on {2}\\.$", name, spell, target));
            UseAbility   = new Regex(string.Format("^{0} use(s)? {1}\\.$", name, ability));
            UseAbilityOn = new Regex(string.Format("^{0} use(s)? {1} on {2}\\.$", name, ability, target));
            MissAbility  = new Regex(string.Format("^{0} use(s)? {1}, but miss(es)? {2}\\.$", name, ability, target));
            MissAbilityNoTarget = new Regex(string.Format("^{0} use(s)? {1}, but miss(es)?\\.$", name, ability, target));
            FailsCharm   = new Regex(string.Format("^{0} fail(s)? to charm {1}\\.$", name, target));
            UseItem      = new Regex(string.Format("^{0} use(s)? {1}\\.$", name, item));
            // Corsair stuff (6f/65|70/66):
            DiceRoll     = new Regex(string.Format("^Dice roll! {0} rolls {1}!$", playername, number));
            UseCorRoll   = new Regex(string.Format("^{0} utilise {1}\\. {2} sort un {3} !$", name, ability, repeatname, number));
            TotalCorRoll = new Regex(string.Format("^Le nombre total de {0} passe à {1} !$", ability, number));
            GainCorRoll  = new Regex(string.Format("^{0} bénéficie de l'effet {1}\\.$", playername, ability));
            BustCorRoll  = new Regex(string.Format("^Bust !$"));
            LoseCorRoll  = new Regex(string.Format("^L'effet {0} sur {1} disparaît\\.$", ability, playername));
            // Cover
            UseCover     = new Regex(string.Format("^{0} covers {1}\\.$", name, target));
            // Accomplice/Collaborator
            StealEnmity  = new Regex(string.Format("^Enmity is stolen from {0}\\.$", target));
            #endregion

            #region Spell/Ability Effects
            RecoversHP   = new Regex(string.Format("^{0} recover(s)? {1} HP\\.$", target, number));
            RecoversMP   = new Regex(string.Format("^{0} recover(s)? {1} MP\\.$", target, number));
            Afflict      = new Regex(string.Format("^{0} (is|are) afflicted with {1} {2}\\.$", target, effect, afflictLvl));
            Enfeeble     = new Regex(string.Format("{0} (is|are) {1}\\.$", target, effect));
            Buff         = new Regex(string.Format("^{0} gain(s)? the effect of {1}\\.$", target, effect));
            GainResistance = new Regex(string.Format("^{0} gain(s)? resistance against {1}\\.$", target, effect));
            Debuff       = new Regex(string.Format("^{0} receive(s)? the effect of {1}\\.$", target, effect));
            Enhance      = new Regex(string.Format("^{0}'(s)? attacks are enhanced\\.$", target));
            Charmed      = new Regex(string.Format("^{0} (is|are) now under {1}'s control\\.$", target, name));
            NotCharmed   = new Regex(string.Format("^{0} (is|are) no longer charmed\\.$", target));
            Dispelled    = new Regex(string.Format("^{0}'(s)? {1} effect disappears!$", target, effect));
            RemoveStatus = new Regex(string.Format("^{0} successfully removes {1}'s {2}$", name, target, effect));
            ItemBuff     = new Regex(string.Format("^{0} receive(s)? the effect of {1}\\.$", target, effect));
            ItemCleanse  = new Regex(string.Format("^{0} is no longer {1}\\.$", target, effect));
            ReduceTP     = new Regex(string.Format("^{0}'s? TP is reduced( to 0)?\\.$", target));
            Hide         = new Regex(string.Format("^{0} hides!$", name));
            #endregion

            #region Failed Actions
            Interrupted  = new Regex(string.Format("^{0}'(s)? casting is interrupted\\.$", name));
            Paralyzed    = new Regex(string.Format("^{0} (is|are) paralyzed\\.$", name));
            CannotSee    = new Regex(string.Format("^Unable to see {0}\\.$", target));
            CannotSee2   = new Regex(string.Format("^You cannot see {0}\\.$", target));
            TooFarAway   = new Regex(string.Format("^{0} (is|are) too far away\\.$", target));
            OutOfRange   = new Regex(string.Format("^{0} (is|are) out of range\\.$", target));
            CannotAttack = new Regex(string.Format("^You cannot attack that target\\.$"));
            NotEnoughTP  = new Regex(string.Format("^You do not have enough TP\\.$"));
            NotEnoughTP2 = new Regex(string.Format("^Not enough TP\\.$"));
            NotEnoughTP3 = new Regex(string.Format("^{0} does not have enough TP\\.$", name));
            NotEnoughMP  = new Regex(string.Format("^You do not have enough MP\\.$", target));
            Intimidated  = new Regex(string.Format("^{0} (is|are) intimidated by {1}'s presence\\.$", name, target));
            UnableToCast = new Regex(string.Format("^Unable to cast spells at this time\\.$"));
            UnableToUse  = new Regex(string.Format("^Unable to use job ability\\.$"));
            UnableToUse2 = new Regex(string.Format("^Unable to use weapon skill\\.$"));
            UnableToUse3 = new Regex(string.Format("^{0} is unable to use weapon skills\\.$", name));
            NoEffect     = new Regex(string.Format("^{0}'(s)? {1} has no effect on {2}\\.$", name, spell, target));
            NoEffect2    = new Regex(string.Format("^No effect on {0}\\.$", target));
            NoEffect3    = new Regex(string.Format("^{0} casts {1} on {2}, but the spell fails to take effect\\.$", name, spell, target));
            AutoTarget   = new Regex(string.Format("^Auto-targeting {0}\\.$", target));
            TooFarForXP  = new Regex(string.Format("^You are too far from the battle to gain experience\\.$"));
            LoseSight    = new Regex(string.Format("^You lose sight of {0}\\.$", target));
            FailActivate = new Regex(string.Format("^{0} fails? to activate\\.$", item));
            CannotPerform = new Regex(string.Format("^{0} cannot perform that action\\.$", name));
            FailHide     = new Regex(string.Format("^{0} (try|tries) to hide, but (is|are) spotted by {1}\\.$", name, target));
            #endregion

            #region Modifiers on existing lines
            AdditionalEffect = new Regex(@"^Additional (E|e)ffect:");
            MagicBurst       = new Regex(@"^Magic Burst!");
            Cover            = new Regex(string.Format("^Cover! {0}", remainder));
            AdditionalDamage = new Regex(string.Format("^Additional effect: {0} point(s)? of damage\\.$", damage));
            AdditionalStatus = new Regex(string.Format("^Additional effect: {0}\\.$", effect));
            AdditionalDrain  = new Regex(string.Format("^Additional effect: {0} HP drained from {1}\\.$", damage, target));
            AdditionalAspir  = new Regex(string.Format("^Additional effect: {0} MP drained from {1}\\.$", damage, target));
            AdditionalHeal   = new Regex(string.Format("^Additional effect: {0} recovers {1} HP\\.$", target, damage));
            #endregion

            #region Combat damage
            MeleeHit        = new Regex(string.Format("^{0} hit(s)? {1} for {2} point(s)? of damage\\.$", name, target, damage));
            RangedAttack    = new Regex(string.Format("^{0}'s? ranged attack hits? {1} for {2} points? of damage\\.$", name, target, damage));
            RangedGoodSpot  = new Regex(string.Format("^{0}'s? ranged attack hits? {1} squarely for {2} points? of damage!$", name, target, damage));
            RangedSweetSpot = new Regex(string.Format("^{0}'s? ranged attack strikes true, pummeling {1} for {2} points? of damage!$", name, target, damage));
            RangedHit       = new Regex(string.Format("^{0} use(s)? Ranged Attack\\.$", name));
            CriticalHit     = new Regex(string.Format("^{0} score(s)? a critical hit!$", name));
            RangedCriticalHit = new Regex(string.Format("^{0}'(s)? ranged attack scores a critical hit!$", name));
            TargetTakesDamage = new Regex(string.Format("{0} take(s)? {1}( additional)? point(s)? of damage\\.$", target, damage));
            DamageAndStun   = new Regex(string.Format("^{0} take(s)? {1} point(s)? of damage and is stunned\\.$", target, damage));
            Spikes          = new Regex(string.Format("{0}'s? spikes deal {1} point(s)? of damage to {2}\\.", name, damage, target));
            DreadSpikes     = new Regex(string.Format("{0}'s? spikes drain {1} HP from {2}\\.", name, damage, target));
            Skillchain      = new Regex(string.Format("^Skillchain: {0}\\.$", skillchain));
            #endregion

            #region Combat defenses
            MeleeMiss    = new Regex(string.Format("^{0} miss(es)? {1}\\.$", name, target));
            MeleeDodge   = new Regex(string.Format("^{0} dodges the attack\\.$", target));
            RangedMiss   = new Regex(string.Format("^{0} use(s)? Ranged Attack, but miss(es)? {1}\\.$", name, target));
            RangedMiss2  = new Regex(string.Format("^{0}'(s)? ranged attack misses\\.$", name));
            Blink        = new Regex(string.Format("^{0} of {1}'s shadows absorb(s)? the damage and disappear(s)?\\.$", number, target));
            Parry        = new Regex(string.Format("^{0} parr(y|ies) {1}'(s)? attack with (his|her|its) weapon\\.$", target, name));
            Anticipate   = new Regex(string.Format("^{0} anticipate(s)? {1}'(s)? attack\\.$", target, name));
            Anticipate2  = new Regex(string.Format("^{0} anticipate(s)? the attack\\.$", target));
            Evade        = new Regex(string.Format("^{0} evade(s)?( the attack)?\\.$", target));
            Counter      = new Regex(string.Format("^{0}'(s)? attack is countered by {1}\\. {2} takes {3} point(s)? of damage\\.$",
                target, name, repeatname, damage));
            CounterShadow = new Regex(string.Format("^{0}'(s)? attack is countered by {1}\\. {2} of {3}'(s)? shadows absorbs the damage and disappears\\.$",
                target, name, number, repeatname));
            Retaliate    = new Regex(string.Format("^{0} retaliates\\. {1} takes {2} points? of damage\\.$",
                name, target, damage));
            RetaliateShadow = new Regex(string.Format("^{0} retaliates\\. {1} of {2}'s? shadows absorbs the damage and disappears\\.$",
                name, number, target));
            ResistSpell  = new Regex(string.Format("^(Resist! )?{0} resist(s)? the (effects of the )?spell(\\.|!)$", target));
            ResistEffect = new Regex(string.Format("^{0} resist(s)? the effect\\.$", target));
            #endregion

            #region Drains
            Drain        = new Regex(string.Format("^{0} {1} drained from {2}\\.$", damage, drainType, target));
            AbsorbStat   = new Regex(string.Format("^{0}'(s)? {1} is drained\\.$", target, drainStat));
            #endregion
        }

        private static void LoadJapaneseStrings()
        {
            throw new NotImplementedException();
        }


        #region Fixed elements (not modified by language)

        #region Name Tests
        // If any of the specified characters occur in the name, it should be a mob type (may possibly be a pet).
        // Otherwise have to check against the Avatar/Wyvern/Puppet name lists.
        internal static Regex MobNameTest = new Regex(@"['\- \d]");
        // Bst jug pets are named either as "CrabFamiliar" (all one word) or "CourierCarrie" (all one word).
        internal static Regex BstJugPetName = new Regex(@"(^\w+Familiar$)|(^[A-Z][a-z]+[A-Z][a-z]+$)");
        #endregion

        #region Plugin corrections
        internal static Regex TimestampPlugin =
            new Regex(@"^\x1e(\x3f|\xfa|\xfc)\[(?<time>\d{2}:\d{2}:\d{2})\] \x1e\x01(?<remainder>.*)$");
        #endregion

        #endregion

        // Below is all the variables that may be modified by different parse language settings.

        #region Named substrings
        private static string playername;
        private static string npcName;

        private static string name;
        private static string target;
        private static string repeatname;

        private static string damage;
        private static string number;
        private static string item;
        private static string money;
        private static string cruor;
        private static string spell;
        private static string ability;
        private static string effect;
        private static string skillchain;

        private static string afflictLvl;
        private static string drainType;
        private static string drainStat;

        private static string colorLight;

        private static string remainder;
        #endregion

        #region Chat name extractions
        internal static Regex ChatSay;
        internal static Regex ChatParty;
        internal static Regex ChatTell;
        internal static Regex ChatTellFrom;
        internal static Regex ChatTellTo;
        internal static Regex ChatShout;
        internal static Regex ChatLinkshell;
        internal static Regex ChatEmote;
        internal static Regex ChatNPC;
        #endregion

        #region Defeated
        internal static Regex Defeated;
        internal static Regex Defeat;
        internal static Regex Dies;
        #endregion

        #region Experience
        internal static Regex ExpChain;
        internal static Regex Experience;
        internal static Regex NoExperience;
        #endregion

        #region Loot
        internal static Regex FindLootOn;
        internal static Regex FindLootIn;
        internal static Regex OpenChest;
        internal static Regex GetLoot;
        internal static Regex GetGil;
        internal static Regex GetCruor;
        internal static Regex TreasureChest;
        internal static Regex OpenLock;
        internal static Regex LootReqr;
        internal static Regex LootLost;
        internal static Regex LotItem;
        internal static Regex Steal;
        internal static Regex FailSteal;
        internal static Regex Mug;
        internal static Regex FailMug;
        internal static Regex EmitLight;
        #endregion

        #region Preparing to take action
        internal static Regex PrepSpell;
        internal static Regex PrepSpellOn;
        internal static Regex PrepAbility;
        #endregion

        #region Completes action
        internal static Regex CastSpell;
        internal static Regex CastSpellOn;
        internal static Regex UseAbility;
        internal static Regex UseAbilityOn;
        internal static Regex MissAbility;
        internal static Regex MissAbilityNoTarget;
        internal static Regex FailsCharm;
        internal static Regex UseItem;
        // Corsair stuff (6f/65|70/66):
        internal static Regex DiceRoll;
        internal static Regex UseCorRoll;
        internal static Regex TotalCorRoll;
        internal static Regex GainCorRoll;
        internal static Regex BustCorRoll;
        internal static Regex LoseCorRoll;
        // Cover
        internal static Regex UseCover;
        // Accomplice/Collaborator
        internal static Regex StealEnmity;
        #endregion

        #region Spell/Ability Effects
        internal static Regex RecoversHP;
        internal static Regex RecoversMP;
        internal static Regex Afflict;
        internal static Regex Enfeeble;
        internal static Regex Buff;
        internal static Regex GainResistance;
        internal static Regex Debuff;
        internal static Regex Enhance;
        internal static Regex Charmed;
        internal static Regex NotCharmed;
        internal static Regex Dispelled;
        internal static Regex RemoveStatus;
        internal static Regex ItemBuff;
        internal static Regex ItemCleanse;
        internal static Regex ReduceTP;
        internal static Regex Hide;
        #endregion

        #region Failed Actions
        internal static Regex Interrupted;
        internal static Regex MoveInterrupt;
        internal static Regex Paralyzed;
        internal static Regex CannotSee;
        internal static Regex CannotSee2;
        internal static Regex TooFarAway;
        internal static Regex OutOfRange;
        internal static Regex CannotAttack;
        internal static Regex NotEnoughTP;
        internal static Regex NotEnoughTP2;
        internal static Regex NotEnoughTP3;
        internal static Regex NotEnoughMP;
        internal static Regex Intimidated;
        internal static Regex UnableToCast;
        internal static Regex UnableToUse;
        internal static Regex UnableToUse2;
        internal static Regex UnableToUse3;
        internal static Regex NoEffect;
        internal static Regex NoEffect2;
        internal static Regex NoEffect3;
        internal static Regex AutoTarget;
        internal static Regex TooFarForXP;
        internal static Regex LoseSight;
        internal static Regex FailActivate;
        internal static Regex CannotPerform;
        internal static Regex FailHide;
        #endregion

        #region Modifiers on existing lines
        internal static Regex AdditionalEffect;
        internal static Regex MagicBurst;
        internal static Regex Cover;
        internal static Regex AdditionalDamage;
        internal static Regex AdditionalStatus;
        internal static Regex AdditionalDrain;
        internal static Regex AdditionalAspir;
        internal static Regex AdditionalHeal;
        #endregion

        #region Combat damage
        internal static Regex MeleeHit;
        internal static Regex RangedAttack;
        internal static Regex RangedGoodSpot;
        internal static Regex RangedSweetSpot;
        internal static Regex RangedHit;
        internal static Regex CriticalHit;
        internal static Regex RangedCriticalHit;
        internal static Regex TargetTakesDamage;
        internal static Regex DamageAndStun;
        internal static Regex Spikes;
        internal static Regex DreadSpikes;
        internal static Regex Skillchain;
        #endregion

        #region Combat defenses
        internal static Regex MeleeMiss;
        internal static Regex MeleeDodge;
        internal static Regex RangedMiss;
        internal static Regex RangedMiss2;
        internal static Regex Blink;
        internal static Regex Parry;
        internal static Regex Anticipate;
        internal static Regex Anticipate2;
        internal static Regex Evade;
        internal static Regex Counter;
        internal static Regex CounterShadow;
        internal static Regex Retaliate;
        internal static Regex RetaliateShadow;
        internal static Regex ResistSpell;
        internal static Regex ResistEffect;
        #endregion

        #region Drains
        internal static Regex Drain;
        internal static Regex AbsorbStat;
        #endregion
    }




}
