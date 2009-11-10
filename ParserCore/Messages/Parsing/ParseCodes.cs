using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser
{
    internal class ParseCodes
    {
        #region Singleton Construction
        /// <summary>
        /// Make the class a singleton
        /// </summary>
        private static readonly ParseCodes instance = new ParseCodes();

         /// <summary>
        /// Private constructor ensures singleton purity.
        /// </summary>
        private ParseCodes()
		{
            InitLookupTables();
        }

		/// <summary>
        /// Gets the singleton instance of the ParseCodes class.
		/// </summary>
        public static ParseCodes Instance
		{
			get
			{
				return instance;
			}
        }
        #endregion

        #region Member lookup variables
        private Dictionary<uint, InteractionType> interactionTypeLookup;
        private Dictionary<uint, AidType> aidTypeLookup;
        private Dictionary<uint, HarmType> harmTypeLookup;
        private Dictionary<uint, SuccessType> successTypeLookup;
        private Dictionary<uint, EntityType> actorEntityTypeLookup;
        private Dictionary<uint, EntityType> targetEntityTypeLookup;
        private Dictionary<uint, ActorPlayerType> actorPlayerTypeLookup;
        #endregion

        #region Setup
        private void InitLookupTables()
        {
            InitCombatCategoryLookup();
            InitAidTypeLookup();
            InitHarmTypeLookup();
            InitSuccessTypeLookup();
            InitActorEntityTypeLookup();
            InitTargetEntityTypeLookup();
            InitActorPlayerTypeLookup();
        }
        #endregion

        #region Individual table initializations
        private void InitCombatCategoryLookup()
        {
            interactionTypeLookup = new Dictionary<uint, InteractionType>(256);

            interactionTypeLookup[0x14] = InteractionType.Harm; // <me> hits
            interactionTypeLookup[0x15] = InteractionType.Harm; // <me> misses
            interactionTypeLookup[0x16] = InteractionType.Harm; // <mob> drains <me>
            interactionTypeLookup[0x17] = InteractionType.Aid; // <me> cures?, <mob> converts damage to healing (???)
            interactionTypeLookup[0x18] = InteractionType.Aid; // <pm> cures
            interactionTypeLookup[0x19] = InteractionType.Harm; // <pm> hits
            interactionTypeLookup[0x1a] = InteractionType.Harm; // <pm> misses
            interactionTypeLookup[0x1b] = InteractionType.Harm; // <mob> drains <pm>
            interactionTypeLookup[0x1c] = InteractionType.Harm; // <mob> hits <me> or <pm>
            interactionTypeLookup[0x1d] = InteractionType.Harm; // <mob> misses <me> or <pm>
            interactionTypeLookup[0x1e] = InteractionType.Aid; // <me> drains
            interactionTypeLookup[0x1f] = InteractionType.Aid; // <me> recovers/cures
            interactionTypeLookup[0x20] = InteractionType.Harm; // <mob> hits <pm> ??
            interactionTypeLookup[0x21] = InteractionType.Harm; // <mob> misses <pm> ??
            interactionTypeLookup[0x22] = InteractionType.Aid; // <pm> Drains
            interactionTypeLookup[0x23] = InteractionType.Aid; // <am> cures <pm>
            interactionTypeLookup[0x24] = InteractionType.Death; // <me> kills
            interactionTypeLookup[0x25] = InteractionType.Death; // <pm> kills
            interactionTypeLookup[0x26] = InteractionType.Death; // <me> dies
            interactionTypeLookup[0x27] = InteractionType.Death; // <pm> dies
            interactionTypeLookup[0x28] = InteractionType.Harm; // <other> hits <other>
            interactionTypeLookup[0x29] = InteractionType.Harm; // <other> miss <other>
            interactionTypeLookup[0x2a] = InteractionType.Aid; // <other> drains
            interactionTypeLookup[0x2b] = InteractionType.Aid; // <other> heals <other>
            interactionTypeLookup[0x2c] = InteractionType.Death; // <other> kills
            interactionTypeLookup[0x2d] = InteractionType.Unknown; // 
            interactionTypeLookup[0x2e] = InteractionType.Unknown; //
            interactionTypeLookup[0x2f] = InteractionType.Unknown; //
            interactionTypeLookup[0x30] = InteractionType.Unknown; //
            interactionTypeLookup[0x31] = InteractionType.Unknown; //
            interactionTypeLookup[0x32] = InteractionType.Unknown; // <me> prep spell/ability (unknown)
            interactionTypeLookup[0x33] = InteractionType.Unknown; // prep spell/ability, target known (not me)
            interactionTypeLookup[0x34] = InteractionType.Unknown; // prep spell/ability, target me, or unknown
            interactionTypeLookup[0x35] = InteractionType.Unknown; //
            interactionTypeLookup[0x36] = InteractionType.Unknown; //
            interactionTypeLookup[0x37] = InteractionType.Unknown; //
            interactionTypeLookup[0x38] = InteractionType.Aid; // buff <me>
            interactionTypeLookup[0x39] = InteractionType.Harm; // enfeeble <me>
            interactionTypeLookup[0x3a] = InteractionType.Unknown; //
            interactionTypeLookup[0x3b] = InteractionType.Unknown; // enfeeble <me>, resisted (failed enhance)
            interactionTypeLookup[0x3c] = InteractionType.Aid; // buff <pm>
            interactionTypeLookup[0x3d] = InteractionType.Harm; // enfeeble <pm>
            interactionTypeLookup[0x3e] = InteractionType.Unknown; //
            interactionTypeLookup[0x3f] = InteractionType.Harm; // enfeeble <pm>, resisted
            interactionTypeLookup[0x40] = InteractionType.Aid; // buff <pm>, other?
            interactionTypeLookup[0x41] = InteractionType.Harm; // enfeeble target (any)
            interactionTypeLookup[0x42] = InteractionType.Unknown; //
            interactionTypeLookup[0x43] = InteractionType.Harm; // enfeeble, no effect (any target?)
            interactionTypeLookup[0x44] = InteractionType.Unknown; // enfeeble, resisted (any target?)
            interactionTypeLookup[0x45] = InteractionType.Harm; // no effect or resisted
            interactionTypeLookup[0x46] = InteractionType.Unknown; //
            interactionTypeLookup[0x47] = InteractionType.Unknown; //
            interactionTypeLookup[0x48] = InteractionType.Unknown; //
            interactionTypeLookup[0x49] = InteractionType.Unknown; //
            interactionTypeLookup[0x4a] = InteractionType.Unknown; //
            interactionTypeLookup[0x4b] = InteractionType.Unknown; //
            interactionTypeLookup[0x4c] = InteractionType.Unknown; //
            interactionTypeLookup[0x4d] = InteractionType.Unknown; //
            interactionTypeLookup[0x4e] = InteractionType.Unknown; //
            interactionTypeLookup[0x4f] = InteractionType.Unknown; //
            interactionTypeLookup[0x50] = InteractionType.Unknown; //
            interactionTypeLookup[0x51] = InteractionType.Aid; // <me> uses item for effect
            interactionTypeLookup[0x52] = InteractionType.Unknown; //
            interactionTypeLookup[0x53] = InteractionType.Unknown; //
            interactionTypeLookup[0x54] = InteractionType.Unknown; //
            interactionTypeLookup[0x55] = InteractionType.Aid; // <pm> uses item
            interactionTypeLookup[0x56] = InteractionType.Unknown; //
            interactionTypeLookup[0x57] = InteractionType.Unknown; //
            interactionTypeLookup[0x58] = InteractionType.Unknown; //
            interactionTypeLookup[0x59] = InteractionType.Unknown; //
            interactionTypeLookup[0x5a] = InteractionType.Aid; // <me> uses item
            interactionTypeLookup[0x5b] = InteractionType.Aid; // <pl> uses item for effect
            interactionTypeLookup[0x5c] = InteractionType.Unknown; //
            interactionTypeLookup[0x5d] = InteractionType.Unknown; //
            interactionTypeLookup[0x5e] = InteractionType.Unknown; //
            interactionTypeLookup[0x5f] = InteractionType.Unknown; //
            interactionTypeLookup[0x60] = InteractionType.Unknown; //
            interactionTypeLookup[0x61] = InteractionType.Unknown; //
            interactionTypeLookup[0x62] = InteractionType.Unknown; //
            interactionTypeLookup[0x63] = InteractionType.Unknown; //
            interactionTypeLookup[0x64] = InteractionType.Unknown; // prep ability, unknown target
            interactionTypeLookup[0x65] = InteractionType.Aid; // <me> uses buff (self-buff?)
            interactionTypeLookup[0x66] = InteractionType.Harm; // <me> is enfeebled
            interactionTypeLookup[0x67] = InteractionType.Unknown; //
            interactionTypeLookup[0x68] = InteractionType.Harm; // <mob> uses ability, misses <me>
            interactionTypeLookup[0x69] = InteractionType.Unknown; // Move being readied for/against player
            interactionTypeLookup[0x6a] = InteractionType.Aid; // <party> uses buff (self-buff?)
            interactionTypeLookup[0x6b] = InteractionType.Harm; // <mob> uses ability, enfeebles <pm>
            interactionTypeLookup[0x6c] = InteractionType.Unknown; //
            interactionTypeLookup[0x6d] = InteractionType.Harm; //
            interactionTypeLookup[0x6e] = InteractionType.Unknown; // Move being readied for/against mob
            interactionTypeLookup[0x6f] = InteractionType.Aid; // <other> uses buff (self-buff?)
            interactionTypeLookup[0x70] = InteractionType.Harm; // <me> enfeebles (ability), use buff and fail (cor bust)
            interactionTypeLookup[0x71] = InteractionType.Unknown; //
            interactionTypeLookup[0x72] = InteractionType.Harm; // <am> uses weaponskill (ability?), misses
            interactionTypeLookup[0x73] = InteractionType.Unknown; //
            interactionTypeLookup[0x74] = InteractionType.Unknown; //
            interactionTypeLookup[0x75] = InteractionType.Unknown; //
            interactionTypeLookup[0x76] = InteractionType.Unknown; //
            interactionTypeLookup[0x77] = InteractionType.Unknown; //
            interactionTypeLookup[0x78] = InteractionType.Unknown; //
            interactionTypeLookup[0x79] = InteractionType.Aid; // <item> fails to activate
            interactionTypeLookup[0x7a] = InteractionType.Harm; // Interrupted/paralyzed/etc.  Failed action
            interactionTypeLookup[0x7b] = InteractionType.Unknown; // Red 'error' text. Ignore
            interactionTypeLookup[0x7c] = InteractionType.Unknown; //
            interactionTypeLookup[0x7d] = InteractionType.Unknown; //
            interactionTypeLookup[0x7e] = InteractionType.Unknown; //
            interactionTypeLookup[0x7f] = InteractionType.Unknown; //
            interactionTypeLookup[0x80] = InteractionType.Unknown; //
            interactionTypeLookup[0x81] = InteractionType.Unknown; //
            interactionTypeLookup[0x82] = InteractionType.Unknown; //
            interactionTypeLookup[0x83] = InteractionType.Unknown; //
            interactionTypeLookup[0x84] = InteractionType.Unknown; //
            interactionTypeLookup[0x85] = InteractionType.Unknown; //
            interactionTypeLookup[0x86] = InteractionType.Unknown; //
            interactionTypeLookup[0x87] = InteractionType.Unknown; //
            interactionTypeLookup[0x88] = InteractionType.Unknown; //
            interactionTypeLookup[0x89] = InteractionType.Unknown; //
            interactionTypeLookup[0x8a] = InteractionType.Unknown; //
            interactionTypeLookup[0x8b] = InteractionType.Unknown; //
            interactionTypeLookup[0x8c] = InteractionType.Unknown; //
            interactionTypeLookup[0x8d] = InteractionType.Harm; // Cannot attack
            interactionTypeLookup[0x8e] = InteractionType.Unknown; //
            interactionTypeLookup[0x8f] = InteractionType.Unknown; //
            interactionTypeLookup[0x90] = InteractionType.Unknown; //
            interactionTypeLookup[0x91] = InteractionType.Unknown; //
            interactionTypeLookup[0x92] = InteractionType.Unknown; //
            interactionTypeLookup[0x93] = InteractionType.Unknown; //
            interactionTypeLookup[0x94] = InteractionType.Unknown; //
            interactionTypeLookup[0x95] = InteractionType.Unknown; //
            interactionTypeLookup[0x96] = InteractionType.Unknown; //
            interactionTypeLookup[0x97] = InteractionType.Unknown; //
            interactionTypeLookup[0x98] = InteractionType.Unknown; //
            interactionTypeLookup[0x99] = InteractionType.Unknown; //
            interactionTypeLookup[0x9a] = InteractionType.Unknown; //
            interactionTypeLookup[0x9b] = InteractionType.Unknown; //
            interactionTypeLookup[0x9c] = InteractionType.Unknown; //
            interactionTypeLookup[0x9d] = InteractionType.Unknown; //
            interactionTypeLookup[0x9e] = InteractionType.Unknown; //
            interactionTypeLookup[0x9f] = InteractionType.Unknown; //
            interactionTypeLookup[0xa0] = InteractionType.Unknown; //
            interactionTypeLookup[0xa1] = InteractionType.Unknown; //
            interactionTypeLookup[0xa2] = InteractionType.Aid; // <am> cures
            interactionTypeLookup[0xa3] = InteractionType.Harm; // <am> hits
            interactionTypeLookup[0xa4] = InteractionType.Harm; // <am> misses
            interactionTypeLookup[0xa5] = InteractionType.Harm; // <mob> casts on <am>
            interactionTypeLookup[0xa6] = InteractionType.Death; // <am> kills
            interactionTypeLookup[0xa7] = InteractionType.Death; // <am> dies
            interactionTypeLookup[0xa8] = InteractionType.Unknown; //
            interactionTypeLookup[0xa9] = InteractionType.Unknown; //
            interactionTypeLookup[0xaa] = InteractionType.Unknown; // <am> is resisted, or misses /ra
            interactionTypeLookup[0xab] = InteractionType.Aid; // <am> uses item
            interactionTypeLookup[0xac] = InteractionType.Unknown; //
            interactionTypeLookup[0xad] = InteractionType.Unknown; //
            interactionTypeLookup[0xae] = InteractionType.Harm; // <am> enfeebled
            interactionTypeLookup[0xaf] = InteractionType.Aid; // <am> uses self-buff
            interactionTypeLookup[0xb0] = InteractionType.Unknown; //
            interactionTypeLookup[0xb1] = InteractionType.Unknown; //
            interactionTypeLookup[0xb2] = InteractionType.Unknown; //
            interactionTypeLookup[0xb3] = InteractionType.Unknown; //
            interactionTypeLookup[0xb4] = InteractionType.Unknown; //
            interactionTypeLookup[0xb5] = InteractionType.Harm; // <am> avoids ability
            interactionTypeLookup[0xb6] = InteractionType.Harm; // <am> is enfeebled
            interactionTypeLookup[0xb7] = InteractionType.Aid; // <am> gains buff
            interactionTypeLookup[0xb8] = InteractionType.Unknown; //
            interactionTypeLookup[0xb9] = InteractionType.Harm; // <am> takes damage
            interactionTypeLookup[0xba] = InteractionType.Harm; // <am> avoids damage
            interactionTypeLookup[0xbb] = InteractionType.Aid; // <am> drains
            interactionTypeLookup[0xbc] = InteractionType.Aid; // <am> cures

        }

        private void InitAidTypeLookup()
        {
            aidTypeLookup = new Dictionary<uint, AidType>(256);


            aidTypeLookup[0x14] = AidType.None; // <me> hits
            aidTypeLookup[0x15] = AidType.None; // <me> misses
            aidTypeLookup[0x16] = AidType.None; // <mob> drains <me>
            aidTypeLookup[0x17] = AidType.Recovery; // <me> cures?, <mob> converts damage to healing (???)
            aidTypeLookup[0x18] = AidType.Recovery; // <pm> cures
            aidTypeLookup[0x19] = AidType.None; // <pm> hits
            aidTypeLookup[0x1a] = AidType.None; // <pm> misses
            aidTypeLookup[0x1b] = AidType.None; // <mob> drains <pm>
            aidTypeLookup[0x1c] = AidType.None; // <mob> hits <me> or <pm>
            aidTypeLookup[0x1d] = AidType.None; // <mob> misses <me> or <pm>
            aidTypeLookup[0x1e] = AidType.Recovery; // <me> drains
            aidTypeLookup[0x1f] = AidType.Recovery; // <me> recovers/cures
            aidTypeLookup[0x20] = AidType.None; // <mob> hits <pm> ??
            aidTypeLookup[0x21] = AidType.None; // <mob> misses <pm> ??
            aidTypeLookup[0x22] = AidType.Recovery; // <pm> drains
            aidTypeLookup[0x23] = AidType.Recovery; // <am> cures <pm>
            aidTypeLookup[0x24] = AidType.None; // <me> kills
            aidTypeLookup[0x25] = AidType.None; // <pm> kills
            aidTypeLookup[0x26] = AidType.None; // <me> dies
            aidTypeLookup[0x27] = AidType.None; // <pm> dies
            aidTypeLookup[0x28] = AidType.None; // <other> hits <other>
            aidTypeLookup[0x29] = AidType.None; // <other> miss <other>
            aidTypeLookup[0x2a] = AidType.Recovery; // <other> drains
            aidTypeLookup[0x2b] = AidType.Recovery; // <other> heals <other>
            aidTypeLookup[0x2c] = AidType.None; // <other> kills
            aidTypeLookup[0x2d] = AidType.None; // 
            aidTypeLookup[0x2e] = AidType.None; //
            aidTypeLookup[0x2f] = AidType.None; //
            aidTypeLookup[0x30] = AidType.None; //
            aidTypeLookup[0x31] = AidType.None; //
            aidTypeLookup[0x32] = AidType.None; // <me> prep spell/ability (unknown)
            aidTypeLookup[0x33] = AidType.Unknown; // prep spell/ability, target known (not me)
            aidTypeLookup[0x34] = AidType.Unknown; // prep spell/ability, target me, or unknown
            aidTypeLookup[0x35] = AidType.Unknown; //
            aidTypeLookup[0x36] = AidType.None; //
            aidTypeLookup[0x37] = AidType.None; //
            aidTypeLookup[0x38] = AidType.Enhance; // buff <me>
            aidTypeLookup[0x39] = AidType.None; // enfeeble <me>
            aidTypeLookup[0x3a] = AidType.None; //
            aidTypeLookup[0x3b] = AidType.Enhance; // enfeeble <me>, resisted (failed enhance?)
            aidTypeLookup[0x3c] = AidType.Enhance; // buff <pm>
            aidTypeLookup[0x3d] = AidType.None; // enfeeble <pm>
            aidTypeLookup[0x3e] = AidType.None; //
            aidTypeLookup[0x3f] = AidType.None; // enfeeble <pm>, resisted
            aidTypeLookup[0x40] = AidType.Enhance; // buff <pm>, other?
            aidTypeLookup[0x41] = AidType.None; // enfeeble target (any)
            aidTypeLookup[0x42] = AidType.None; //
            aidTypeLookup[0x43] = AidType.None; // enfeeble, no effect (any target?)
            aidTypeLookup[0x44] = AidType.Enhance; // enfeeble, resisted (any target?)
            aidTypeLookup[0x45] = AidType.None; // no effect or resisted
            aidTypeLookup[0x46] = AidType.None; //
            aidTypeLookup[0x47] = AidType.None; //
            aidTypeLookup[0x48] = AidType.None; //
            aidTypeLookup[0x49] = AidType.None; //
            aidTypeLookup[0x4a] = AidType.None; //
            aidTypeLookup[0x4b] = AidType.None; //
            aidTypeLookup[0x4c] = AidType.None; //
            aidTypeLookup[0x4d] = AidType.None; //
            aidTypeLookup[0x4e] = AidType.None; //
            aidTypeLookup[0x4f] = AidType.None; //
            aidTypeLookup[0x50] = AidType.None; //
            aidTypeLookup[0x51] = AidType.Item; // <me> uses item for effect
            aidTypeLookup[0x52] = AidType.None; //
            aidTypeLookup[0x53] = AidType.None; //
            aidTypeLookup[0x54] = AidType.None; //
            aidTypeLookup[0x55] = AidType.Item; // <pm> uses item
            aidTypeLookup[0x56] = AidType.None; //
            aidTypeLookup[0x57] = AidType.None; //
            aidTypeLookup[0x58] = AidType.None; //
            aidTypeLookup[0x59] = AidType.None; //
            aidTypeLookup[0x5a] = AidType.Item; // <me> uses item
            aidTypeLookup[0x5b] = AidType.Item; // <pl> uses item for effect
            aidTypeLookup[0x5c] = AidType.None; //
            aidTypeLookup[0x5d] = AidType.None; //
            aidTypeLookup[0x5e] = AidType.None; //
            aidTypeLookup[0x5f] = AidType.None; //
            aidTypeLookup[0x60] = AidType.None; //
            aidTypeLookup[0x61] = AidType.None; //
            aidTypeLookup[0x62] = AidType.None; //
            aidTypeLookup[0x63] = AidType.None; //
            aidTypeLookup[0x64] = AidType.None; // prep ability, unknown target
            aidTypeLookup[0x65] = AidType.Enhance; // <me> uses buff (self-buff?)
            aidTypeLookup[0x66] = AidType.None; // <me> is enfeebled
            aidTypeLookup[0x67] = AidType.None; //
            aidTypeLookup[0x68] = AidType.None; // <mob> uses ability, misses <me>
            aidTypeLookup[0x69] = AidType.None; // <mob> readies dmg move, no target specified
            aidTypeLookup[0x6a] = AidType.Enhance; // <party> uses buff (self-buff?)
            aidTypeLookup[0x6b] = AidType.None; // <mob> uses ability, enfeebles <pm>
            aidTypeLookup[0x6c] = AidType.None; //
            aidTypeLookup[0x6d] = AidType.None; //
            aidTypeLookup[0x6e] = AidType.None; // <me/party> prep weaponskill, <bt> prep self-buff
            aidTypeLookup[0x6f] = AidType.Enhance; // <other> uses buff (self-buff?)
            aidTypeLookup[0x70] = AidType.None; // <me> enfeebles (ability)
            aidTypeLookup[0x71] = AidType.None; //
            aidTypeLookup[0x72] = AidType.None; // <am> uses weaponskill (ability?), misses
            aidTypeLookup[0x73] = AidType.None; //
            aidTypeLookup[0x74] = AidType.None; //
            aidTypeLookup[0x75] = AidType.None; //
            aidTypeLookup[0x76] = AidType.None; //
            aidTypeLookup[0x77] = AidType.None; //
            aidTypeLookup[0x78] = AidType.None; //
            aidTypeLookup[0x79] = AidType.Item; // <item> fails to activate
            aidTypeLookup[0x7a] = AidType.None; // Interrupted/paralyzed/etc.  Failed action
            aidTypeLookup[0x7b] = AidType.None; // Red 'error' text. Ignore
            aidTypeLookup[0x7c] = AidType.None; //
            aidTypeLookup[0x7d] = AidType.None; //
            aidTypeLookup[0x7e] = AidType.None; //
            aidTypeLookup[0x7f] = AidType.None; //
            aidTypeLookup[0x80] = AidType.None; //
            aidTypeLookup[0x81] = AidType.None; //
            aidTypeLookup[0x82] = AidType.None; //
            aidTypeLookup[0x83] = AidType.None; //
            aidTypeLookup[0x84] = AidType.None; //
            aidTypeLookup[0x85] = AidType.None; //
            aidTypeLookup[0x86] = AidType.None; //
            aidTypeLookup[0x87] = AidType.None; //
            aidTypeLookup[0x88] = AidType.None; //
            aidTypeLookup[0x89] = AidType.None; //
            aidTypeLookup[0x8a] = AidType.None; //
            aidTypeLookup[0x8b] = AidType.None; //
            aidTypeLookup[0x8c] = AidType.None; //
            aidTypeLookup[0x8d] = AidType.None; //
            aidTypeLookup[0x8e] = AidType.None; //
            aidTypeLookup[0x8f] = AidType.None; //
            aidTypeLookup[0x90] = AidType.None; //
            aidTypeLookup[0x91] = AidType.None; //
            aidTypeLookup[0x92] = AidType.None; //
            aidTypeLookup[0x93] = AidType.None; //
            aidTypeLookup[0x94] = AidType.None; //
            aidTypeLookup[0x95] = AidType.None; //
            aidTypeLookup[0x96] = AidType.None; //
            aidTypeLookup[0x97] = AidType.None; //
            aidTypeLookup[0x98] = AidType.None; //
            aidTypeLookup[0x99] = AidType.None; //
            aidTypeLookup[0x9a] = AidType.None; //
            aidTypeLookup[0x9b] = AidType.None; //
            aidTypeLookup[0x9c] = AidType.None; //
            aidTypeLookup[0x9d] = AidType.None; //
            aidTypeLookup[0x9e] = AidType.None; //
            aidTypeLookup[0x9f] = AidType.None; //
            aidTypeLookup[0xa0] = AidType.None; //
            aidTypeLookup[0xa1] = AidType.None; //
            aidTypeLookup[0xa2] = AidType.Recovery; // <am> cures
            aidTypeLookup[0xa3] = AidType.None; // <am> hits
            aidTypeLookup[0xa4] = AidType.None; // <am> misses
            aidTypeLookup[0xa5] = AidType.None; // <mob> casts on <am>
            aidTypeLookup[0xa6] = AidType.None; // <am> kills
            aidTypeLookup[0xa7] = AidType.None; // <am> dies
            aidTypeLookup[0xa8] = AidType.None; //
            aidTypeLookup[0xa9] = AidType.None; //
            aidTypeLookup[0xaa] = AidType.Enhance; // <am> is resisted, or misses /ra
            aidTypeLookup[0xab] = AidType.Item; // <am> uses item
            aidTypeLookup[0xac] = AidType.None; //
            aidTypeLookup[0xad] = AidType.None; //
            aidTypeLookup[0xae] = AidType.None; // <am> enfeebled
            aidTypeLookup[0xaf] = AidType.Enhance; // <am> uses self-buff
            aidTypeLookup[0xb0] = AidType.None; //
            aidTypeLookup[0xb1] = AidType.None; //
            aidTypeLookup[0xb2] = AidType.None; //
            aidTypeLookup[0xb3] = AidType.None; //
            aidTypeLookup[0xb4] = AidType.None; //
            aidTypeLookup[0xb5] = AidType.None; // <am> avoids damage
            aidTypeLookup[0xb6] = AidType.None; //
            aidTypeLookup[0xb7] = AidType.Enhance; // <am> gains buff
            aidTypeLookup[0xb8] = AidType.None; //
            aidTypeLookup[0xb9] = AidType.None; // <am> takes damage
            aidTypeLookup[0xba] = AidType.None; // <am> avoids damage
            aidTypeLookup[0xbb] = AidType.Recovery; // <am> drains
            aidTypeLookup[0xbc] = AidType.Recovery; // <am> cures

        }

        private void InitHarmTypeLookup()
        {
            harmTypeLookup = new Dictionary<uint, HarmType>(256);


            harmTypeLookup[0x14] = HarmType.Damage; // <me> hits
            harmTypeLookup[0x15] = HarmType.Damage; // <me> misses
            harmTypeLookup[0x16] = HarmType.Drain; // <mob> drains <me>
            harmTypeLookup[0x17] = HarmType.None; // <me> cures?, <mob> converts damage to healing (???)
            harmTypeLookup[0x18] = HarmType.None; // <pm> cures
            harmTypeLookup[0x19] = HarmType.Damage; // <pm> hits
            harmTypeLookup[0x1a] = HarmType.Damage; // <pm> misses
            harmTypeLookup[0x1b] = HarmType.Drain; // <mob> drains <pm>
            harmTypeLookup[0x1c] = HarmType.Damage; // <mob> hits <me>
            harmTypeLookup[0x1d] = HarmType.Damage; // <mob> misses <me>
            harmTypeLookup[0x1e] = HarmType.Drain; // <me> drains
            harmTypeLookup[0x1f] = HarmType.None; // <me> recovers/cures
            harmTypeLookup[0x20] = HarmType.Damage; // <mob> hits <pm>
            harmTypeLookup[0x21] = HarmType.Damage; // <mob> misses <pm>
            harmTypeLookup[0x22] = HarmType.Drain; // 
            harmTypeLookup[0x23] = HarmType.None; // <am> cures <pm>
            harmTypeLookup[0x24] = HarmType.None; // <me> kills
            harmTypeLookup[0x25] = HarmType.None; // <pm> kills
            harmTypeLookup[0x26] = HarmType.None; // <me> dies
            harmTypeLookup[0x27] = HarmType.None; // <pm> dies
            harmTypeLookup[0x28] = HarmType.Damage; // <other> hits <other>
            harmTypeLookup[0x29] = HarmType.Damage; // <other> miss <other>
            harmTypeLookup[0x2a] = HarmType.Drain; // <other> drains
            harmTypeLookup[0x2b] = HarmType.None; // <other> heals <other>
            harmTypeLookup[0x2c] = HarmType.None; // <other> kills
            harmTypeLookup[0x2d] = HarmType.None; // 
            harmTypeLookup[0x2e] = HarmType.None; //
            harmTypeLookup[0x2f] = HarmType.None; //
            harmTypeLookup[0x30] = HarmType.None; //
            harmTypeLookup[0x31] = HarmType.None; //
            harmTypeLookup[0x32] = HarmType.None; // <me> prep spell/ability (unknown)
            harmTypeLookup[0x33] = HarmType.Unknown; // prep spell/ability, target known (not me)
            harmTypeLookup[0x34] = HarmType.Unknown; // prep spell/ability, target me, or unknown
            harmTypeLookup[0x35] = HarmType.Unknown; //
            harmTypeLookup[0x36] = HarmType.None; //
            harmTypeLookup[0x37] = HarmType.None; //
            harmTypeLookup[0x38] = HarmType.None; // buff <me>
            harmTypeLookup[0x39] = HarmType.Enfeeble; // enfeeble <me>
            harmTypeLookup[0x3a] = HarmType.None; //
            harmTypeLookup[0x3b] = HarmType.Enfeeble; // enfeeble <me>, resisted (failed enhance?)
            harmTypeLookup[0x3c] = HarmType.None; // buff <pm>
            harmTypeLookup[0x3d] = HarmType.Enfeeble; // enfeeble <pm>
            harmTypeLookup[0x3e] = HarmType.None; //
            harmTypeLookup[0x3f] = HarmType.Enfeeble; // enfeeble <pm>, resisted
            harmTypeLookup[0x40] = HarmType.None; // buff <pm>, other?
            harmTypeLookup[0x41] = HarmType.Enfeeble; // enfeeble target (any)
            harmTypeLookup[0x42] = HarmType.None; //
            harmTypeLookup[0x43] = HarmType.Enfeeble; // enfeeble, no effect (any target?)
            harmTypeLookup[0x44] = HarmType.Enfeeble; // enfeeble, resisted (any target?)
            harmTypeLookup[0x45] = HarmType.Enfeeble; // no effect or resisted
            harmTypeLookup[0x46] = HarmType.None; //
            harmTypeLookup[0x47] = HarmType.None; //
            harmTypeLookup[0x48] = HarmType.None; //
            harmTypeLookup[0x49] = HarmType.None; //
            harmTypeLookup[0x4a] = HarmType.None; //
            harmTypeLookup[0x4b] = HarmType.None; //
            harmTypeLookup[0x4c] = HarmType.None; //
            harmTypeLookup[0x4d] = HarmType.None; //
            harmTypeLookup[0x4e] = HarmType.None; //
            harmTypeLookup[0x4f] = HarmType.None; //
            harmTypeLookup[0x50] = HarmType.None; //
            harmTypeLookup[0x51] = HarmType.None; // <me> uses item for effect
            harmTypeLookup[0x52] = HarmType.None; //
            harmTypeLookup[0x53] = HarmType.None; //
            harmTypeLookup[0x54] = HarmType.None; //
            harmTypeLookup[0x55] = HarmType.None; // <pm> uses item
            harmTypeLookup[0x56] = HarmType.None; //
            harmTypeLookup[0x57] = HarmType.None; //
            harmTypeLookup[0x58] = HarmType.None; //
            harmTypeLookup[0x59] = HarmType.None; //
            harmTypeLookup[0x5a] = HarmType.None; // <me> uses item
            harmTypeLookup[0x5b] = HarmType.None; // <am> uses item
            harmTypeLookup[0x5c] = HarmType.None; //
            harmTypeLookup[0x5d] = HarmType.None; //
            harmTypeLookup[0x5e] = HarmType.None; //
            harmTypeLookup[0x5f] = HarmType.None; //
            harmTypeLookup[0x60] = HarmType.None; //
            harmTypeLookup[0x61] = HarmType.None; //
            harmTypeLookup[0x62] = HarmType.None; //
            harmTypeLookup[0x63] = HarmType.None; //
            harmTypeLookup[0x64] = HarmType.None; // prep ability, unknown target
            harmTypeLookup[0x65] = HarmType.None; // <me> uses buff (self-buff?)
            harmTypeLookup[0x66] = HarmType.Enfeeble; // <me> is enfeebled
            harmTypeLookup[0x67] = HarmType.None; //
            harmTypeLookup[0x68] = HarmType.Damage; // <mob> uses ability, misses <me>
            harmTypeLookup[0x69] = HarmType.Damage; // <mob> readies dmg move, no target specified
            harmTypeLookup[0x6a] = HarmType.None; // <party> uses buff (self-buff?)
            harmTypeLookup[0x6b] = HarmType.Enfeeble; // <mob> uses ability, enfeebles <pm>
            harmTypeLookup[0x6c] = HarmType.None; //
            harmTypeLookup[0x6d] = HarmType.Damage; //
            harmTypeLookup[0x6e] = HarmType.None; // <me/party> prep weaponskill, <bt> prep self-buff
            harmTypeLookup[0x6f] = HarmType.None; // <other> uses buff (self-buff?)
            harmTypeLookup[0x70] = HarmType.Enfeeble; // <me> enfeebles (ability)
            harmTypeLookup[0x71] = HarmType.None; //
            harmTypeLookup[0x72] = HarmType.Unknown; // <am> uses weaponskill (ability?), misses
            harmTypeLookup[0x73] = HarmType.None; //
            harmTypeLookup[0x74] = HarmType.None; //
            harmTypeLookup[0x75] = HarmType.None; //
            harmTypeLookup[0x76] = HarmType.None; //
            harmTypeLookup[0x77] = HarmType.None; //
            harmTypeLookup[0x78] = HarmType.None; //
            harmTypeLookup[0x79] = HarmType.None; // <item> fails to activate
            harmTypeLookup[0x7a] = HarmType.Unknown; // Interrupted/paralyzed/etc.  Failed action
            harmTypeLookup[0x7b] = HarmType.None; // Red 'error' text. Ignore
            harmTypeLookup[0x7c] = HarmType.None; //
            harmTypeLookup[0x7d] = HarmType.None; //
            harmTypeLookup[0x7e] = HarmType.None; //
            harmTypeLookup[0x7f] = HarmType.None; //
            harmTypeLookup[0x80] = HarmType.None; //
            harmTypeLookup[0x81] = HarmType.None; //
            harmTypeLookup[0x82] = HarmType.None; //
            harmTypeLookup[0x83] = HarmType.None; //
            harmTypeLookup[0x84] = HarmType.None; //
            harmTypeLookup[0x85] = HarmType.None; //
            harmTypeLookup[0x86] = HarmType.None; //
            harmTypeLookup[0x87] = HarmType.None; //
            harmTypeLookup[0x88] = HarmType.None; //
            harmTypeLookup[0x89] = HarmType.None; //
            harmTypeLookup[0x8a] = HarmType.None; //
            harmTypeLookup[0x8b] = HarmType.None; //
            harmTypeLookup[0x8c] = HarmType.None; //
            harmTypeLookup[0x8d] = HarmType.Unknown; // Cannot attack
            harmTypeLookup[0x8e] = HarmType.None; //
            harmTypeLookup[0x8f] = HarmType.None; //
            harmTypeLookup[0x90] = HarmType.None; //
            harmTypeLookup[0x91] = HarmType.None; //
            harmTypeLookup[0x92] = HarmType.None; //
            harmTypeLookup[0x93] = HarmType.None; //
            harmTypeLookup[0x94] = HarmType.None; //
            harmTypeLookup[0x95] = HarmType.None; //
            harmTypeLookup[0x96] = HarmType.None; //
            harmTypeLookup[0x97] = HarmType.None; //
            harmTypeLookup[0x98] = HarmType.None; //
            harmTypeLookup[0x99] = HarmType.None; //
            harmTypeLookup[0x9a] = HarmType.None; //
            harmTypeLookup[0x9b] = HarmType.None; //
            harmTypeLookup[0x9c] = HarmType.None; //
            harmTypeLookup[0x9d] = HarmType.None; //
            harmTypeLookup[0x9e] = HarmType.None; //
            harmTypeLookup[0x9f] = HarmType.None; //
            harmTypeLookup[0xa0] = HarmType.None; //
            harmTypeLookup[0xa1] = HarmType.None; //
            harmTypeLookup[0xa2] = HarmType.None; //
            harmTypeLookup[0xa3] = HarmType.Damage; // <am> hits
            harmTypeLookup[0xa4] = HarmType.Damage; // <am> misses
            harmTypeLookup[0xa5] = HarmType.Unknown; // <mob> casts on <am>
            harmTypeLookup[0xa6] = HarmType.None; // <am> kills
            harmTypeLookup[0xa7] = HarmType.None; // <am> dies
            harmTypeLookup[0xa8] = HarmType.None; //
            harmTypeLookup[0xa9] = HarmType.None; //
            harmTypeLookup[0xaa] = HarmType.Enfeeble; // <am> is resisted, or misses /ra
            harmTypeLookup[0xab] = HarmType.None; //
            harmTypeLookup[0xac] = HarmType.None; //
            harmTypeLookup[0xad] = HarmType.None; //
            harmTypeLookup[0xae] = HarmType.Enfeeble; // <em> enfeebled
            harmTypeLookup[0xaf] = HarmType.None; // <am> uses self-buff
            harmTypeLookup[0xb0] = HarmType.None; //
            harmTypeLookup[0xb1] = HarmType.None; //
            harmTypeLookup[0xb2] = HarmType.None; //
            harmTypeLookup[0xb3] = HarmType.None; //
            harmTypeLookup[0xb4] = HarmType.None; //
            harmTypeLookup[0xb5] = HarmType.Damage; // <am> avoids ability
            harmTypeLookup[0xb6] = HarmType.Enfeeble; // <am> is enfeebled
            harmTypeLookup[0xb7] = HarmType.None; // <am> gains buff
            harmTypeLookup[0xb8] = HarmType.None; //
            harmTypeLookup[0xb9] = HarmType.Damage; // <am> takes damage
            harmTypeLookup[0xba] = HarmType.Damage; // <am> avoids damage
            harmTypeLookup[0xbb] = HarmType.Drain; // <am> drains
            harmTypeLookup[0xbc] = HarmType.None; // <am> cures

        }

        private void InitSuccessTypeLookup()
        {
            successTypeLookup = new Dictionary<uint, SuccessType>(256);

            successTypeLookup[0x14] = SuccessType.Successful; // <me> hits
            successTypeLookup[0x15] = SuccessType.Unsuccessful; // <me> misses
            successTypeLookup[0x16] = SuccessType.Successful; // <mob> drains <me>
            successTypeLookup[0x17] = SuccessType.Successful; // <me> cures?, <mob> converts damage to healing (???)
            successTypeLookup[0x18] = SuccessType.Successful; // <pm> cures
            successTypeLookup[0x19] = SuccessType.Successful; // <pm> hits
            successTypeLookup[0x1a] = SuccessType.Unsuccessful; // <pm> misses
            successTypeLookup[0x1b] = SuccessType.Successful; // <mob> drains <pm>
            successTypeLookup[0x1c] = SuccessType.Successful; // <mob> hits <me> or <pm>
            successTypeLookup[0x1d] = SuccessType.Unsuccessful; // <mob> misses <me> or <pm>
            successTypeLookup[0x1e] = SuccessType.Successful; // <me> drains
            successTypeLookup[0x1f] = SuccessType.Successful; // <me> recovers/cures
            successTypeLookup[0x20] = SuccessType.Successful; // <mob> hits <pm> ??
            successTypeLookup[0x21] = SuccessType.Unsuccessful; // <mob> misses <pm> ??
            successTypeLookup[0x22] = SuccessType.Successful; // 
            successTypeLookup[0x23] = SuccessType.Successful; // <am> cures <pm>
            successTypeLookup[0x24] = SuccessType.None; // <me> kills
            successTypeLookup[0x25] = SuccessType.None; // <pm> kills
            successTypeLookup[0x26] = SuccessType.None; // <me> dies
            successTypeLookup[0x27] = SuccessType.None; // <pm> dies
            successTypeLookup[0x28] = SuccessType.Successful; // <other> hits <other>
            successTypeLookup[0x29] = SuccessType.Unsuccessful; // <other> miss <other>
            successTypeLookup[0x2a] = SuccessType.Successful; // <other> drains
            successTypeLookup[0x2b] = SuccessType.Successful; // <other> heals <other>
            successTypeLookup[0x2c] = SuccessType.None; // <other> kills
            successTypeLookup[0x2d] = SuccessType.None; // 
            successTypeLookup[0x2e] = SuccessType.None; //
            successTypeLookup[0x2f] = SuccessType.None; //
            successTypeLookup[0x30] = SuccessType.None; //
            successTypeLookup[0x31] = SuccessType.None; //
            successTypeLookup[0x32] = SuccessType.Unknown; // <me> prep spell/ability (unknown)
            successTypeLookup[0x33] = SuccessType.Unknown; // prep spell/ability, target known (not me)
            successTypeLookup[0x34] = SuccessType.Unknown; // prep spell/ability, target me, or unknown
            successTypeLookup[0x35] = SuccessType.None; //
            successTypeLookup[0x36] = SuccessType.None; //
            successTypeLookup[0x37] = SuccessType.None; //
            successTypeLookup[0x38] = SuccessType.Successful; // buff <me>
            successTypeLookup[0x39] = SuccessType.Successful; // enfeeble <me>
            successTypeLookup[0x3a] = SuccessType.None; //
            successTypeLookup[0x3b] = SuccessType.Failed; // enfeeble <me>, resisted (failed enhance?)
            successTypeLookup[0x3c] = SuccessType.Successful; // buff <pm>
            successTypeLookup[0x3d] = SuccessType.Successful; // enfeeble <pm>
            successTypeLookup[0x3e] = SuccessType.None; //
            successTypeLookup[0x3f] = SuccessType.Unsuccessful; // enfeeble <pm>, resisted
            successTypeLookup[0x40] = SuccessType.Successful; // buff <pm>, other?
            successTypeLookup[0x41] = SuccessType.Successful; // enfeeble target (any)
            successTypeLookup[0x42] = SuccessType.Unknown; //
            successTypeLookup[0x43] = SuccessType.Failed; // enfeeble, no effect (any target?)
            successTypeLookup[0x44] = SuccessType.Unsuccessful; // enfeeble, resisted (any target?)
            successTypeLookup[0x45] = SuccessType.Unsuccessful; // no effect or resisted
            successTypeLookup[0x46] = SuccessType.None; //
            successTypeLookup[0x47] = SuccessType.None; //
            successTypeLookup[0x48] = SuccessType.None; //
            successTypeLookup[0x49] = SuccessType.None; //
            successTypeLookup[0x4a] = SuccessType.None; //
            successTypeLookup[0x4b] = SuccessType.None; //
            successTypeLookup[0x4c] = SuccessType.None; //
            successTypeLookup[0x4d] = SuccessType.None; //
            successTypeLookup[0x4e] = SuccessType.None; //
            successTypeLookup[0x4f] = SuccessType.None; //
            successTypeLookup[0x50] = SuccessType.None; //
            successTypeLookup[0x51] = SuccessType.Successful; // <me> uses item for effect
            successTypeLookup[0x52] = SuccessType.None; //
            successTypeLookup[0x53] = SuccessType.None; //
            successTypeLookup[0x54] = SuccessType.None; //
            successTypeLookup[0x55] = SuccessType.Successful; // <pm> uses item
            successTypeLookup[0x56] = SuccessType.None; //
            successTypeLookup[0x57] = SuccessType.None; //
            successTypeLookup[0x58] = SuccessType.None; //
            successTypeLookup[0x59] = SuccessType.None; //
            successTypeLookup[0x5a] = SuccessType.Successful; // <me> uses item
            successTypeLookup[0x5b] = SuccessType.Successful; // <pl> uses item for effect
            successTypeLookup[0x5c] = SuccessType.None; //
            successTypeLookup[0x5d] = SuccessType.None; //
            successTypeLookup[0x5e] = SuccessType.None; //
            successTypeLookup[0x5f] = SuccessType.None; //
            successTypeLookup[0x60] = SuccessType.None; //
            successTypeLookup[0x61] = SuccessType.None; //
            successTypeLookup[0x62] = SuccessType.None; //
            successTypeLookup[0x63] = SuccessType.None; //
            successTypeLookup[0x64] = SuccessType.Unknown; // prep ability, unknown target
            successTypeLookup[0x65] = SuccessType.Successful; // <me> uses buff (self-buff?)
            successTypeLookup[0x66] = SuccessType.Successful; // <me> is enfeebled
            successTypeLookup[0x67] = SuccessType.None; //
            successTypeLookup[0x68] = SuccessType.Unsuccessful; // <mob> uses ability, misses <me>
            successTypeLookup[0x69] = SuccessType.Unknown; // <mob> readies dmg move, no target specified
            successTypeLookup[0x6a] = SuccessType.Successful; // <party> uses buff (self-buff?)
            successTypeLookup[0x6b] = SuccessType.Successful; // <mob> uses ability, enfeebles <pm>
            successTypeLookup[0x6c] = SuccessType.None; //
            successTypeLookup[0x6d] = SuccessType.Unsuccessful; //
            successTypeLookup[0x6e] = SuccessType.Unknown; // <me/party> prep weaponskill, <bt> prep self-buff
            successTypeLookup[0x6f] = SuccessType.Successful; // <other> uses buff (self-buff?)
            successTypeLookup[0x70] = SuccessType.Successful; // <me> enfeebles (ability)
            successTypeLookup[0x71] = SuccessType.None; //
            successTypeLookup[0x72] = SuccessType.Unsuccessful; // <am> uses weaponskill (ability?), misses
            successTypeLookup[0x73] = SuccessType.None; //
            successTypeLookup[0x74] = SuccessType.None; //
            successTypeLookup[0x75] = SuccessType.None; //
            successTypeLookup[0x76] = SuccessType.None; //
            successTypeLookup[0x77] = SuccessType.None; //
            successTypeLookup[0x78] = SuccessType.None; //
            successTypeLookup[0x79] = SuccessType.Failed; // <item> fails to activate
            successTypeLookup[0x7a] = SuccessType.Failed; // Interrupted/paralyzed/etc.  Failed action
            successTypeLookup[0x7b] = SuccessType.Failed; // Red 'error' text. Ignore
            successTypeLookup[0x7c] = SuccessType.None; //
            successTypeLookup[0x7d] = SuccessType.None; //
            successTypeLookup[0x7e] = SuccessType.None; //
            successTypeLookup[0x7f] = SuccessType.None; //
            successTypeLookup[0x80] = SuccessType.None; //
            successTypeLookup[0x81] = SuccessType.None; //
            successTypeLookup[0x82] = SuccessType.None; //
            successTypeLookup[0x83] = SuccessType.None; //
            successTypeLookup[0x84] = SuccessType.None; //
            successTypeLookup[0x85] = SuccessType.None; //
            successTypeLookup[0x86] = SuccessType.None; //
            successTypeLookup[0x87] = SuccessType.None; //
            successTypeLookup[0x88] = SuccessType.None; //
            successTypeLookup[0x89] = SuccessType.None; //
            successTypeLookup[0x8a] = SuccessType.None; //
            successTypeLookup[0x8b] = SuccessType.None; //
            successTypeLookup[0x8c] = SuccessType.None; //
            successTypeLookup[0x8d] = SuccessType.Failed; // Cannot attack
            successTypeLookup[0x8e] = SuccessType.None; //
            successTypeLookup[0x8f] = SuccessType.None; //
            successTypeLookup[0x90] = SuccessType.None; //
            successTypeLookup[0x91] = SuccessType.None; //
            successTypeLookup[0x92] = SuccessType.None; //
            successTypeLookup[0x93] = SuccessType.None; //
            successTypeLookup[0x94] = SuccessType.None; //
            successTypeLookup[0x95] = SuccessType.None; //
            successTypeLookup[0x96] = SuccessType.None; //
            successTypeLookup[0x97] = SuccessType.None; //
            successTypeLookup[0x98] = SuccessType.None; //
            successTypeLookup[0x99] = SuccessType.None; //
            successTypeLookup[0x9a] = SuccessType.None; //
            successTypeLookup[0x9b] = SuccessType.None; //
            successTypeLookup[0x9c] = SuccessType.None; //
            successTypeLookup[0x9d] = SuccessType.None; //
            successTypeLookup[0x9e] = SuccessType.None; //
            successTypeLookup[0x9f] = SuccessType.None; //
            successTypeLookup[0xa0] = SuccessType.None; //
            successTypeLookup[0xa1] = SuccessType.None; //
            successTypeLookup[0xa2] = SuccessType.Successful; // <am> cures
            successTypeLookup[0xa3] = SuccessType.Successful; // <am> hits
            successTypeLookup[0xa4] = SuccessType.Unsuccessful; // <am> misses
            successTypeLookup[0xa5] = SuccessType.Successful; // <mob> casts on <am>
            successTypeLookup[0xa6] = SuccessType.None; // <am> kills
            successTypeLookup[0xa7] = SuccessType.None; // <am> dies
            successTypeLookup[0xa8] = SuccessType.None; //
            successTypeLookup[0xa9] = SuccessType.None; //
            successTypeLookup[0xaa] = SuccessType.Unsuccessful; // <am> is resisted, or misses /ra
            successTypeLookup[0xab] = SuccessType.Successful; // <am> uses item
            successTypeLookup[0xac] = SuccessType.None; //
            successTypeLookup[0xad] = SuccessType.None; //
            successTypeLookup[0xae] = SuccessType.Successful; // <am> enfeebled
            successTypeLookup[0xaf] = SuccessType.Successful; // <am> uses self-buff
            successTypeLookup[0xb0] = SuccessType.None; //
            successTypeLookup[0xb1] = SuccessType.None; //
            successTypeLookup[0xb2] = SuccessType.None; //
            successTypeLookup[0xb3] = SuccessType.None; //
            successTypeLookup[0xb4] = SuccessType.None; //
            successTypeLookup[0xb5] = SuccessType.Unsuccessful; // <am> avoids ability
            successTypeLookup[0xb6] = SuccessType.Successful; // <am> is enfeebled
            successTypeLookup[0xb7] = SuccessType.Successful; // <am> gains buff
            successTypeLookup[0xb8] = SuccessType.None; //
            successTypeLookup[0xb9] = SuccessType.Successful; // <am> takes damage
            successTypeLookup[0xba] = SuccessType.Unsuccessful; // <am> avoids damage
            successTypeLookup[0xbb] = SuccessType.Successful; // <am> drains
            successTypeLookup[0xbc] = SuccessType.Successful; // <am> cures

        }

        private void InitActorEntityTypeLookup()
        {
            // Actor entity types for an action
            actorEntityTypeLookup = new Dictionary<uint, EntityType>(256);

            actorEntityTypeLookup[0x14] = EntityType.Player; // <me> hits
            actorEntityTypeLookup[0x15] = EntityType.Player; // <me> misses
            actorEntityTypeLookup[0x16] = EntityType.Mob; // <mob> drains <me>
            actorEntityTypeLookup[0x17] = EntityType.Player; // <me> cures?, <mob> converts damage to healing (???)
            actorEntityTypeLookup[0x18] = EntityType.Player; // <pm> cures
            actorEntityTypeLookup[0x19] = EntityType.Player; // <pm> hits
            actorEntityTypeLookup[0x1a] = EntityType.Player; // <pm> misses
            actorEntityTypeLookup[0x1b] = EntityType.Mob; // <mob> drains <pm>
            actorEntityTypeLookup[0x1c] = EntityType.Mob; // <mob> hits <me> or <pm>
            actorEntityTypeLookup[0x1d] = EntityType.Mob; // <mob> misses <me> or <pm>
            actorEntityTypeLookup[0x1e] = EntityType.Player; // <me> drains
            actorEntityTypeLookup[0x1f] = EntityType.Player; // <me> recovers/cures
            actorEntityTypeLookup[0x20] = EntityType.Mob; // <mob> hits <pm> ??
            actorEntityTypeLookup[0x21] = EntityType.Mob; // <mob> misses <pm> ??
            actorEntityTypeLookup[0x22] = EntityType.Unknown; // 
            actorEntityTypeLookup[0x23] = EntityType.Player; // <am> cures <pm>
            actorEntityTypeLookup[0x24] = EntityType.Player; // <me> kills
            actorEntityTypeLookup[0x25] = EntityType.Player; // <pm> kills
            actorEntityTypeLookup[0x26] = EntityType.Mob; // <me> dies
            actorEntityTypeLookup[0x27] = EntityType.Mob; // <pm> dies
            actorEntityTypeLookup[0x28] = EntityType.Unknown; // <other> hits <other>
            actorEntityTypeLookup[0x29] = EntityType.Unknown; // <other> miss <other>
            actorEntityTypeLookup[0x2a] = EntityType.Unknown; // <other> drains
            actorEntityTypeLookup[0x2b] = EntityType.Unknown; // <other> heals <other>
            actorEntityTypeLookup[0x2c] = EntityType.Unknown; // <other> kills
            actorEntityTypeLookup[0x2d] = EntityType.Unknown; // 
            actorEntityTypeLookup[0x2e] = EntityType.Unknown; //
            actorEntityTypeLookup[0x2f] = EntityType.Unknown; //
            actorEntityTypeLookup[0x30] = EntityType.Unknown; //
            actorEntityTypeLookup[0x31] = EntityType.Unknown; //
            actorEntityTypeLookup[0x32] = EntityType.Player; // <me> prep spell/ability (unknown)
            actorEntityTypeLookup[0x33] = EntityType.Unknown; // prep spell/ability, target known (not me)
            actorEntityTypeLookup[0x34] = EntityType.Unknown; // prep spell/ability, target me, or unknown
            actorEntityTypeLookup[0x35] = EntityType.Unknown; //
            actorEntityTypeLookup[0x36] = EntityType.Unknown; //
            actorEntityTypeLookup[0x37] = EntityType.Unknown; //
            actorEntityTypeLookup[0x38] = EntityType.Player; // buff <me>
            actorEntityTypeLookup[0x39] = EntityType.Mob; // enfeeble <me>
            actorEntityTypeLookup[0x3a] = EntityType.Unknown; //
            actorEntityTypeLookup[0x3b] = EntityType.Unknown; // enfeeble <me>, resisted (failed enhance?)
            actorEntityTypeLookup[0x3c] = EntityType.Player; // buff <pm>
            actorEntityTypeLookup[0x3d] = EntityType.Mob; // enfeeble <pm>
            actorEntityTypeLookup[0x3e] = EntityType.Unknown; //
            actorEntityTypeLookup[0x3f] = EntityType.Unknown; // enfeeble <pm>, resisted
            actorEntityTypeLookup[0x40] = EntityType.Unknown; // buff <pm>, other?
            actorEntityTypeLookup[0x41] = EntityType.Unknown; // enfeeble target (any)
            actorEntityTypeLookup[0x42] = EntityType.Unknown; //
            actorEntityTypeLookup[0x43] = EntityType.Unknown; // enfeeble, no effect (any target?)
            actorEntityTypeLookup[0x44] = EntityType.Unknown; // enfeeble, resisted (any target?)
            actorEntityTypeLookup[0x45] = EntityType.Unknown; // no effect or resisted
            actorEntityTypeLookup[0x46] = EntityType.Unknown; //
            actorEntityTypeLookup[0x47] = EntityType.Unknown; //
            actorEntityTypeLookup[0x48] = EntityType.Unknown; //
            actorEntityTypeLookup[0x49] = EntityType.Unknown; //
            actorEntityTypeLookup[0x4a] = EntityType.Unknown; //
            actorEntityTypeLookup[0x4b] = EntityType.Unknown; //
            actorEntityTypeLookup[0x4c] = EntityType.Unknown; //
            actorEntityTypeLookup[0x4d] = EntityType.Unknown; //
            actorEntityTypeLookup[0x4e] = EntityType.Unknown; //
            actorEntityTypeLookup[0x4f] = EntityType.Unknown; //
            actorEntityTypeLookup[0x50] = EntityType.Unknown; //
            actorEntityTypeLookup[0x51] = EntityType.Player; // <me> uses item for effect
            actorEntityTypeLookup[0x52] = EntityType.Unknown; //
            actorEntityTypeLookup[0x53] = EntityType.Unknown; //
            actorEntityTypeLookup[0x54] = EntityType.Unknown; //
            actorEntityTypeLookup[0x55] = EntityType.Player; // <pm> uses item
            actorEntityTypeLookup[0x56] = EntityType.Unknown; //
            actorEntityTypeLookup[0x57] = EntityType.Unknown; //
            actorEntityTypeLookup[0x58] = EntityType.Unknown; //
            actorEntityTypeLookup[0x59] = EntityType.Unknown; //
            actorEntityTypeLookup[0x5a] = EntityType.Player; // <me> uses item
            actorEntityTypeLookup[0x5b] = EntityType.Player; // <pl> uses item for effect
            actorEntityTypeLookup[0x5c] = EntityType.Unknown; //
            actorEntityTypeLookup[0x5d] = EntityType.Unknown; //
            actorEntityTypeLookup[0x5e] = EntityType.Unknown; //
            actorEntityTypeLookup[0x5f] = EntityType.Unknown; //
            actorEntityTypeLookup[0x60] = EntityType.Unknown; //
            actorEntityTypeLookup[0x61] = EntityType.Unknown; //
            actorEntityTypeLookup[0x62] = EntityType.Unknown; //
            actorEntityTypeLookup[0x63] = EntityType.Unknown; //
            actorEntityTypeLookup[0x64] = EntityType.Unknown; // prep ability, unknown target
            actorEntityTypeLookup[0x65] = EntityType.Player; // <me> uses buff (self-buff?)
            actorEntityTypeLookup[0x66] = EntityType.Unknown; // <me> is enfeebled
            actorEntityTypeLookup[0x67] = EntityType.Unknown; //
            actorEntityTypeLookup[0x68] = EntityType.Mob; // <mob> uses ability, misses <me>
            actorEntityTypeLookup[0x69] = EntityType.Unknown; // <mob> readies dmg move, no target specified
            actorEntityTypeLookup[0x6a] = EntityType.Player; // <party> uses buff (self-buff?)
            actorEntityTypeLookup[0x6b] = EntityType.Mob; // <mob> uses ability, enfeebles <pm>
            actorEntityTypeLookup[0x6c] = EntityType.Unknown; //
            actorEntityTypeLookup[0x6d] = EntityType.Unknown; //
            actorEntityTypeLookup[0x6e] = EntityType.Unknown; // <me/party> prep weaponskill, <bt> prep self-buff
            actorEntityTypeLookup[0x6f] = EntityType.Unknown; // <other> uses buff (self-buff?)
            actorEntityTypeLookup[0x70] = EntityType.Player; // <me> enfeebles (ability)
            actorEntityTypeLookup[0x71] = EntityType.Unknown; //
            actorEntityTypeLookup[0x72] = EntityType.Player; // <am> uses weaponskill (ability?), misses
            actorEntityTypeLookup[0x73] = EntityType.Unknown; //
            actorEntityTypeLookup[0x74] = EntityType.Unknown; //
            actorEntityTypeLookup[0x75] = EntityType.Unknown; //
            actorEntityTypeLookup[0x76] = EntityType.Unknown; //
            actorEntityTypeLookup[0x77] = EntityType.Unknown; //
            actorEntityTypeLookup[0x78] = EntityType.Unknown; //
            actorEntityTypeLookup[0x79] = EntityType.Unknown; // <item> fails to activate
            actorEntityTypeLookup[0x7a] = EntityType.Unknown; // Interrupted/paralyzed/etc.  Failed action
            actorEntityTypeLookup[0x7b] = EntityType.Unknown; // Red 'error' text. Ignore
            actorEntityTypeLookup[0x7c] = EntityType.Unknown; //
            actorEntityTypeLookup[0x7d] = EntityType.Unknown; //
            actorEntityTypeLookup[0x7e] = EntityType.Unknown; //
            actorEntityTypeLookup[0x7f] = EntityType.Unknown; //
            actorEntityTypeLookup[0x80] = EntityType.Unknown; //
            actorEntityTypeLookup[0x81] = EntityType.Unknown; //
            actorEntityTypeLookup[0x82] = EntityType.Unknown; //
            actorEntityTypeLookup[0x83] = EntityType.Unknown; //
            actorEntityTypeLookup[0x84] = EntityType.Unknown; //
            actorEntityTypeLookup[0x85] = EntityType.Unknown; //
            actorEntityTypeLookup[0x86] = EntityType.Unknown; //
            actorEntityTypeLookup[0x87] = EntityType.Unknown; //
            actorEntityTypeLookup[0x88] = EntityType.Unknown; //
            actorEntityTypeLookup[0x89] = EntityType.Unknown; //
            actorEntityTypeLookup[0x8a] = EntityType.Unknown; //
            actorEntityTypeLookup[0x8b] = EntityType.Unknown; //
            actorEntityTypeLookup[0x8c] = EntityType.Unknown; //
            actorEntityTypeLookup[0x8d] = EntityType.Unknown; // Cannot attack
            actorEntityTypeLookup[0x8e] = EntityType.Unknown; //
            actorEntityTypeLookup[0x8f] = EntityType.Unknown; //
            actorEntityTypeLookup[0x90] = EntityType.Unknown; //
            actorEntityTypeLookup[0x91] = EntityType.Unknown; //
            actorEntityTypeLookup[0x92] = EntityType.Unknown; //
            actorEntityTypeLookup[0x93] = EntityType.Unknown; //
            actorEntityTypeLookup[0x94] = EntityType.Unknown; //
            actorEntityTypeLookup[0x95] = EntityType.Unknown; //
            actorEntityTypeLookup[0x96] = EntityType.Unknown; //
            actorEntityTypeLookup[0x97] = EntityType.Unknown; //
            actorEntityTypeLookup[0x98] = EntityType.Unknown; //
            actorEntityTypeLookup[0x99] = EntityType.Unknown; //
            actorEntityTypeLookup[0x9a] = EntityType.Unknown; //
            actorEntityTypeLookup[0x9b] = EntityType.Unknown; //
            actorEntityTypeLookup[0x9c] = EntityType.Unknown; //
            actorEntityTypeLookup[0x9d] = EntityType.Unknown; //
            actorEntityTypeLookup[0x9e] = EntityType.Unknown; //
            actorEntityTypeLookup[0x9f] = EntityType.Unknown; //
            actorEntityTypeLookup[0xa0] = EntityType.Unknown; //
            actorEntityTypeLookup[0xa1] = EntityType.Unknown; //
            actorEntityTypeLookup[0xa2] = EntityType.Player; // <am> cures
            actorEntityTypeLookup[0xa3] = EntityType.Player; // <am> hits
            actorEntityTypeLookup[0xa4] = EntityType.Player; // <am> misses
            actorEntityTypeLookup[0xa5] = EntityType.Mob; // <mob> casts on <am>
            actorEntityTypeLookup[0xa6] = EntityType.Player; // <am> kills
            actorEntityTypeLookup[0xa7] = EntityType.Unknown; // <am> dies
            actorEntityTypeLookup[0xa8] = EntityType.Unknown; //
            actorEntityTypeLookup[0xa9] = EntityType.Unknown; //
            actorEntityTypeLookup[0xaa] = EntityType.Player; // <am> is resisted, or misses /ra
            actorEntityTypeLookup[0xab] = EntityType.Player; // <am> uses item
            actorEntityTypeLookup[0xac] = EntityType.Unknown; //
            actorEntityTypeLookup[0xad] = EntityType.Unknown; //
            actorEntityTypeLookup[0xae] = EntityType.Unknown; // <am> enfeebled
            actorEntityTypeLookup[0xaf] = EntityType.Player; // <am> uses self-buff
            actorEntityTypeLookup[0xb0] = EntityType.Unknown; //
            actorEntityTypeLookup[0xb1] = EntityType.Unknown; //
            actorEntityTypeLookup[0xb2] = EntityType.Unknown; //
            actorEntityTypeLookup[0xb3] = EntityType.Unknown; //
            actorEntityTypeLookup[0xb4] = EntityType.Unknown; //
            actorEntityTypeLookup[0xb5] = EntityType.Unknown; // <am> avoids ability
            actorEntityTypeLookup[0xb6] = EntityType.Unknown; // <am> is enfeebled
            actorEntityTypeLookup[0xb7] = EntityType.Player; // <am> gains buff
            actorEntityTypeLookup[0xb8] = EntityType.Unknown; //
            actorEntityTypeLookup[0xb9] = EntityType.Mob; // <am> takes damage
            actorEntityTypeLookup[0xba] = EntityType.Mob; // <am> avoids damage
            actorEntityTypeLookup[0xbb] = EntityType.Player; // <am> drains
            actorEntityTypeLookup[0xbc] = EntityType.Player; // <am> cures
        }

        private void InitTargetEntityTypeLookup()
        {
            // Target entity types for an action
            targetEntityTypeLookup = new Dictionary<uint, EntityType>(256);

            targetEntityTypeLookup[0x14] = EntityType.Mob; // <me> hits
            targetEntityTypeLookup[0x15] = EntityType.Mob; // <me> misses
            targetEntityTypeLookup[0x16] = EntityType.Player; // <mob> drains <me>
            targetEntityTypeLookup[0x17] = EntityType.Unknown; // <me> cures?, <mob> converts damage to healing (???)
            targetEntityTypeLookup[0x18] = EntityType.Player; // <pm> cures
            targetEntityTypeLookup[0x19] = EntityType.Mob; // <pm> hits
            targetEntityTypeLookup[0x1a] = EntityType.Mob; // <pm> misses
            targetEntityTypeLookup[0x1b] = EntityType.Player; // <mob> drains <pm>
            targetEntityTypeLookup[0x1c] = EntityType.Player; // <mob> hits <me> or <pm>
            targetEntityTypeLookup[0x1d] = EntityType.Player; // <mob> misses <me> or <pm>
            targetEntityTypeLookup[0x1e] = EntityType.Mob; // <me> drains
            targetEntityTypeLookup[0x1f] = EntityType.Player; // <me> recovers/cures
            targetEntityTypeLookup[0x20] = EntityType.Player; // <mob> hits <pm> ??
            targetEntityTypeLookup[0x21] = EntityType.Player; // <mob> misses <pm> ??
            targetEntityTypeLookup[0x22] = EntityType.Unknown; // 
            targetEntityTypeLookup[0x23] = EntityType.Player; // <am> cures <pm>
            targetEntityTypeLookup[0x24] = EntityType.Mob; // <me> kills
            targetEntityTypeLookup[0x25] = EntityType.Mob; // <pm> kills
            targetEntityTypeLookup[0x26] = EntityType.Player; // <me> dies
            targetEntityTypeLookup[0x27] = EntityType.Player; // <pm> dies
            targetEntityTypeLookup[0x28] = EntityType.Unknown; // <other> hits <other>
            targetEntityTypeLookup[0x29] = EntityType.Unknown; // <other> miss <other>
            targetEntityTypeLookup[0x2a] = EntityType.Unknown; // <other> drains
            targetEntityTypeLookup[0x2b] = EntityType.Unknown; // <other> heals <other>
            targetEntityTypeLookup[0x2c] = EntityType.Unknown; // <other> kills
            targetEntityTypeLookup[0x2d] = EntityType.Unknown; // 
            targetEntityTypeLookup[0x2e] = EntityType.Unknown; //
            targetEntityTypeLookup[0x2f] = EntityType.Unknown; //
            targetEntityTypeLookup[0x30] = EntityType.Unknown; //
            targetEntityTypeLookup[0x31] = EntityType.Unknown; //
            targetEntityTypeLookup[0x32] = EntityType.Unknown; // <me> prep spell/ability (unknown)
            targetEntityTypeLookup[0x33] = EntityType.Unknown; // prep spell/ability, target known (not me)
            targetEntityTypeLookup[0x34] = EntityType.Unknown; // prep spell/ability, target me, or unknown
            targetEntityTypeLookup[0x35] = EntityType.Unknown; //
            targetEntityTypeLookup[0x36] = EntityType.Unknown; //
            targetEntityTypeLookup[0x37] = EntityType.Unknown; //
            targetEntityTypeLookup[0x38] = EntityType.Player; // buff <me>
            targetEntityTypeLookup[0x39] = EntityType.Player; // enfeeble <me>
            targetEntityTypeLookup[0x3a] = EntityType.Unknown; //
            targetEntityTypeLookup[0x3b] = EntityType.Player; // enfeeble <me>, resisted (failed enhance?)
            targetEntityTypeLookup[0x3c] = EntityType.Player; // buff <pm>
            targetEntityTypeLookup[0x3d] = EntityType.Player; // enfeeble <pm>
            targetEntityTypeLookup[0x3e] = EntityType.Unknown; //
            targetEntityTypeLookup[0x3f] = EntityType.Player; // enfeeble <pm>, resisted
            targetEntityTypeLookup[0x40] = EntityType.Unknown; // buff <pm>, other?
            targetEntityTypeLookup[0x41] = EntityType.Unknown; // enfeeble target (any)
            targetEntityTypeLookup[0x42] = EntityType.Unknown; //
            targetEntityTypeLookup[0x43] = EntityType.Unknown; // enfeeble, no effect (any target?)
            targetEntityTypeLookup[0x44] = EntityType.Unknown; // enfeeble, resisted (any target?)
            targetEntityTypeLookup[0x45] = EntityType.Unknown; // no effect or resisted
            targetEntityTypeLookup[0x46] = EntityType.Unknown; //
            targetEntityTypeLookup[0x47] = EntityType.Unknown; //
            targetEntityTypeLookup[0x48] = EntityType.Unknown; //
            targetEntityTypeLookup[0x49] = EntityType.Unknown; //
            targetEntityTypeLookup[0x4a] = EntityType.Unknown; //
            targetEntityTypeLookup[0x4b] = EntityType.Unknown; //
            targetEntityTypeLookup[0x4c] = EntityType.Unknown; //
            targetEntityTypeLookup[0x4d] = EntityType.Unknown; //
            targetEntityTypeLookup[0x4e] = EntityType.Unknown; //
            targetEntityTypeLookup[0x4f] = EntityType.Unknown; //
            targetEntityTypeLookup[0x50] = EntityType.Unknown; //
            targetEntityTypeLookup[0x51] = EntityType.Unknown; // <me> uses item for effect
            targetEntityTypeLookup[0x52] = EntityType.Unknown; //
            targetEntityTypeLookup[0x53] = EntityType.Unknown; //
            targetEntityTypeLookup[0x54] = EntityType.Unknown; //
            targetEntityTypeLookup[0x55] = EntityType.Unknown; // <pm> uses item
            targetEntityTypeLookup[0x56] = EntityType.Unknown; //
            targetEntityTypeLookup[0x57] = EntityType.Unknown; //
            targetEntityTypeLookup[0x58] = EntityType.Unknown; //
            targetEntityTypeLookup[0x59] = EntityType.Unknown; //
            targetEntityTypeLookup[0x5a] = EntityType.Unknown; // <me> uses item
            targetEntityTypeLookup[0x5b] = EntityType.Unknown; // <pl> uses item for effect
            targetEntityTypeLookup[0x5c] = EntityType.Unknown; //
            targetEntityTypeLookup[0x5d] = EntityType.Unknown; //
            targetEntityTypeLookup[0x5e] = EntityType.Unknown; //
            targetEntityTypeLookup[0x5f] = EntityType.Unknown; //
            targetEntityTypeLookup[0x60] = EntityType.Unknown; //
            targetEntityTypeLookup[0x61] = EntityType.Unknown; //
            targetEntityTypeLookup[0x62] = EntityType.Unknown; //
            targetEntityTypeLookup[0x63] = EntityType.Unknown; //
            targetEntityTypeLookup[0x64] = EntityType.Unknown; // prep ability, unknown target
            targetEntityTypeLookup[0x65] = EntityType.Player; // <me> uses buff (self-buff?)
            targetEntityTypeLookup[0x66] = EntityType.Unknown; // <me> is enfeebled
            targetEntityTypeLookup[0x67] = EntityType.Unknown; //
            targetEntityTypeLookup[0x68] = EntityType.Player; // <mob> uses ability, misses <me>
            targetEntityTypeLookup[0x69] = EntityType.Unknown; // <mob> readies dmg move, no target specified
            targetEntityTypeLookup[0x6a] = EntityType.Player; // <party> uses buff (self-buff?)
            targetEntityTypeLookup[0x6b] = EntityType.Player; // <mob> uses ability, enfeebles <pm>
            targetEntityTypeLookup[0x6c] = EntityType.Unknown; //
            targetEntityTypeLookup[0x6d] = EntityType.Unknown; //
            targetEntityTypeLookup[0x6e] = EntityType.Unknown; // <me/party> prep weaponskill, <bt> prep self-buff
            targetEntityTypeLookup[0x6f] = EntityType.Unknown; // <other> uses buff (self-buff?)
            targetEntityTypeLookup[0x70] = EntityType.Unknown; // <me> enfeebles (ability) (possible self-enfeeble)
            targetEntityTypeLookup[0x71] = EntityType.Unknown; //
            targetEntityTypeLookup[0x72] = EntityType.Mob; // <am> uses weaponskill (ability?), misses
            targetEntityTypeLookup[0x73] = EntityType.Unknown; //
            targetEntityTypeLookup[0x74] = EntityType.Unknown; //
            targetEntityTypeLookup[0x75] = EntityType.Unknown; //
            targetEntityTypeLookup[0x76] = EntityType.Unknown; //
            targetEntityTypeLookup[0x77] = EntityType.Unknown; //
            targetEntityTypeLookup[0x78] = EntityType.Unknown; //
            targetEntityTypeLookup[0x79] = EntityType.Unknown; // <item> fails to activate
            targetEntityTypeLookup[0x7a] = EntityType.Unknown; // Interrupted/paralyzed/etc.  Failed action
            targetEntityTypeLookup[0x7b] = EntityType.Unknown; // Red 'error' text. Ignore
            targetEntityTypeLookup[0x7c] = EntityType.Unknown; //
            targetEntityTypeLookup[0x7d] = EntityType.Unknown; //
            targetEntityTypeLookup[0x7e] = EntityType.Unknown; //
            targetEntityTypeLookup[0x7f] = EntityType.Unknown; //
            targetEntityTypeLookup[0x80] = EntityType.Unknown; //
            targetEntityTypeLookup[0x81] = EntityType.Unknown; //
            targetEntityTypeLookup[0x82] = EntityType.Unknown; //
            targetEntityTypeLookup[0x83] = EntityType.Unknown; //
            targetEntityTypeLookup[0x84] = EntityType.Unknown; //
            targetEntityTypeLookup[0x85] = EntityType.Unknown; //
            targetEntityTypeLookup[0x86] = EntityType.Unknown; //
            targetEntityTypeLookup[0x87] = EntityType.Unknown; //
            targetEntityTypeLookup[0x88] = EntityType.Unknown; //
            targetEntityTypeLookup[0x89] = EntityType.Unknown; //
            targetEntityTypeLookup[0x8a] = EntityType.Unknown; //
            targetEntityTypeLookup[0x8b] = EntityType.Unknown; //
            targetEntityTypeLookup[0x8c] = EntityType.Unknown; //
            targetEntityTypeLookup[0x8d] = EntityType.Unknown; // Cannot attack
            targetEntityTypeLookup[0x8e] = EntityType.Unknown; //
            targetEntityTypeLookup[0x8f] = EntityType.Unknown; //
            targetEntityTypeLookup[0x90] = EntityType.Unknown; //
            targetEntityTypeLookup[0x91] = EntityType.Unknown; //
            targetEntityTypeLookup[0x92] = EntityType.Unknown; //
            targetEntityTypeLookup[0x93] = EntityType.Unknown; //
            targetEntityTypeLookup[0x94] = EntityType.Unknown; //
            targetEntityTypeLookup[0x95] = EntityType.Unknown; //
            targetEntityTypeLookup[0x96] = EntityType.Unknown; //
            targetEntityTypeLookup[0x97] = EntityType.Unknown; //
            targetEntityTypeLookup[0x98] = EntityType.Unknown; //
            targetEntityTypeLookup[0x99] = EntityType.Unknown; //
            targetEntityTypeLookup[0x9a] = EntityType.Unknown; //
            targetEntityTypeLookup[0x9b] = EntityType.Unknown; //
            targetEntityTypeLookup[0x9c] = EntityType.Unknown; //
            targetEntityTypeLookup[0x9d] = EntityType.Unknown; //
            targetEntityTypeLookup[0x9e] = EntityType.Unknown; //
            targetEntityTypeLookup[0x9f] = EntityType.Unknown; //
            targetEntityTypeLookup[0xa0] = EntityType.Unknown; //
            targetEntityTypeLookup[0xa1] = EntityType.Unknown; //
            targetEntityTypeLookup[0xa2] = EntityType.Player; // <am> cures
            targetEntityTypeLookup[0xa3] = EntityType.Mob; // <am> hits
            targetEntityTypeLookup[0xa4] = EntityType.Mob; // <am> misses
            targetEntityTypeLookup[0xa5] = EntityType.Player; // <mob> casts on <am>
            targetEntityTypeLookup[0xa6] = EntityType.Mob; // <am> kills
            targetEntityTypeLookup[0xa7] = EntityType.Player; // <am> dies
            targetEntityTypeLookup[0xa8] = EntityType.Unknown; //
            targetEntityTypeLookup[0xa9] = EntityType.Unknown; //
            targetEntityTypeLookup[0xaa] = EntityType.Unknown; // <am> is resisted, or misses /ra
            targetEntityTypeLookup[0xab] = EntityType.Unknown; // <am> uses item
            targetEntityTypeLookup[0xac] = EntityType.Unknown; //
            targetEntityTypeLookup[0xad] = EntityType.Unknown; //
            targetEntityTypeLookup[0xae] = EntityType.Unknown; // <am> enfeebled
            targetEntityTypeLookup[0xaf] = EntityType.Player; // <am> uses self-buff
            targetEntityTypeLookup[0xb0] = EntityType.Unknown; //
            targetEntityTypeLookup[0xb1] = EntityType.Unknown; //
            targetEntityTypeLookup[0xb2] = EntityType.Unknown; //
            targetEntityTypeLookup[0xb3] = EntityType.Unknown; //
            targetEntityTypeLookup[0xb4] = EntityType.Unknown; //
            targetEntityTypeLookup[0xb5] = EntityType.Unknown; // <am> avoids ability
            targetEntityTypeLookup[0xb6] = EntityType.Unknown; // <am> is enfeebled
            targetEntityTypeLookup[0xb7] = EntityType.Player; // <am> gains buff
            targetEntityTypeLookup[0xb8] = EntityType.Unknown; //
            targetEntityTypeLookup[0xb9] = EntityType.Player; // <am> takes damage
            targetEntityTypeLookup[0xba] = EntityType.Player; // <am> avoids damage
            targetEntityTypeLookup[0xbb] = EntityType.Mob; // <am> drains
            targetEntityTypeLookup[0xbc] = EntityType.Player; // <am> cures
        }

        private void InitActorPlayerTypeLookup()
        {
            actorPlayerTypeLookup = new Dictionary<uint, ActorPlayerType>(256);

            actorPlayerTypeLookup[0x14] = ActorPlayerType.Self; // <me> hits
            actorPlayerTypeLookup[0x15] = ActorPlayerType.Self; // <me> misses
            actorPlayerTypeLookup[0x16] = ActorPlayerType.None; // <mob> drains <me>
            actorPlayerTypeLookup[0x17] = ActorPlayerType.Unknown; // <me> cures?, <mob> converts damage to healing (???)
            actorPlayerTypeLookup[0x18] = ActorPlayerType.Party; // <pm> cures
            actorPlayerTypeLookup[0x19] = ActorPlayerType.Party; // <pm> hits
            actorPlayerTypeLookup[0x1a] = ActorPlayerType.Party; // <pm> misses
            actorPlayerTypeLookup[0x1b] = ActorPlayerType.None; // <mob> drains <pm>
            actorPlayerTypeLookup[0x1c] = ActorPlayerType.None; // <mob> hits <me> or <pm>
            actorPlayerTypeLookup[0x1d] = ActorPlayerType.None; // <mob> misses <me> or <pm>
            actorPlayerTypeLookup[0x1e] = ActorPlayerType.Self; // <me> drains
            actorPlayerTypeLookup[0x1f] = ActorPlayerType.Self; // <me> recovers/cures
            actorPlayerTypeLookup[0x20] = ActorPlayerType.None; // <mob> hits <pm> ??
            actorPlayerTypeLookup[0x21] = ActorPlayerType.None; // <mob> misses <pm> ??
            actorPlayerTypeLookup[0x22] = ActorPlayerType.Party; // <pm> Drains
            actorPlayerTypeLookup[0x23] = ActorPlayerType.Unknown; // <am> cures <pm>
            actorPlayerTypeLookup[0x24] = ActorPlayerType.Self; // <me> kills
            actorPlayerTypeLookup[0x25] = ActorPlayerType.Party; // <pm> kills
            actorPlayerTypeLookup[0x26] = ActorPlayerType.None; // <me> dies
            actorPlayerTypeLookup[0x27] = ActorPlayerType.None; // <pm> dies
            actorPlayerTypeLookup[0x28] = ActorPlayerType.Other; // <other> hits <other>
            actorPlayerTypeLookup[0x29] = ActorPlayerType.Other; // <other> miss <other>
            actorPlayerTypeLookup[0x2a] = ActorPlayerType.Other; // <other> drains
            actorPlayerTypeLookup[0x2b] = ActorPlayerType.Other; // <other> heals <other>
            actorPlayerTypeLookup[0x2c] = ActorPlayerType.Other; // <other> kills
            actorPlayerTypeLookup[0x2d] = ActorPlayerType.None; // 
            actorPlayerTypeLookup[0x2e] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x2f] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x30] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x31] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x32] = ActorPlayerType.Self; // <me> prep spell/ability (None)
            actorPlayerTypeLookup[0x33] = ActorPlayerType.Unknown; // prep spell/ability, target known (not me)
            actorPlayerTypeLookup[0x34] = ActorPlayerType.Unknown; // prep spell/ability, target me, or None
            actorPlayerTypeLookup[0x35] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x36] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x37] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x38] = ActorPlayerType.Self; // buff <me>
            actorPlayerTypeLookup[0x39] = ActorPlayerType.Self; // enfeeble <me>
            actorPlayerTypeLookup[0x3a] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x3b] = ActorPlayerType.None; // enfeeble <me>, resisted (failed enhance)
            actorPlayerTypeLookup[0x3c] = ActorPlayerType.Party; // buff <pm>
            actorPlayerTypeLookup[0x3d] = ActorPlayerType.None; // enfeeble <pm>
            actorPlayerTypeLookup[0x3e] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x3f] = ActorPlayerType.None; // enfeeble <pm>, resisted
            actorPlayerTypeLookup[0x40] = ActorPlayerType.Unknown; // buff <pm>, other?
            actorPlayerTypeLookup[0x41] = ActorPlayerType.Unknown; // enfeeble target (any)
            actorPlayerTypeLookup[0x42] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x43] = ActorPlayerType.Unknown; // enfeeble, no effect (any target?)
            actorPlayerTypeLookup[0x44] = ActorPlayerType.Unknown; // enfeeble, resisted (any target?)
            actorPlayerTypeLookup[0x45] = ActorPlayerType.Unknown; // no effect or resisted
            actorPlayerTypeLookup[0x46] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x47] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x48] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x49] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x4a] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x4b] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x4c] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x4d] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x4e] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x4f] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x50] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x51] = ActorPlayerType.Self; // <me> uses item for effect
            actorPlayerTypeLookup[0x52] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x53] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x54] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x55] = ActorPlayerType.Party; // <pm> uses item
            actorPlayerTypeLookup[0x56] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x57] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x58] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x59] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x5a] = ActorPlayerType.Self; // <me> uses item
            actorPlayerTypeLookup[0x5b] = ActorPlayerType.Party; // <pl> uses item for effect
            actorPlayerTypeLookup[0x5c] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x5d] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x5e] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x5f] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x60] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x61] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x62] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x63] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x64] = ActorPlayerType.Unknown; // prep ability, None target
            actorPlayerTypeLookup[0x65] = ActorPlayerType.Self; // <me> uses buff (self-buff?)
            actorPlayerTypeLookup[0x66] = ActorPlayerType.None; // <me> is enfeebled
            actorPlayerTypeLookup[0x67] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x68] = ActorPlayerType.None; // <mob> uses ability, misses <me>
            actorPlayerTypeLookup[0x69] = ActorPlayerType.None; // Move being readied for/against player
            actorPlayerTypeLookup[0x6a] = ActorPlayerType.Party; // <party> uses buff (self-buff?)
            actorPlayerTypeLookup[0x6b] = ActorPlayerType.None; // <mob> uses ability, enfeebles <pm>
            actorPlayerTypeLookup[0x6c] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x6d] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x6e] = ActorPlayerType.None; // Move being readied for/against mob
            actorPlayerTypeLookup[0x6f] = ActorPlayerType.Other; // <other> uses buff (self-buff?)
            actorPlayerTypeLookup[0x70] = ActorPlayerType.Self; // <me> enfeebles (ability), use buff and fail (cor bust)
            actorPlayerTypeLookup[0x71] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x72] = ActorPlayerType.Alliance; // <am> uses weaponskill (ability?), misses
            actorPlayerTypeLookup[0x73] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x74] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x75] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x76] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x77] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x78] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x79] = ActorPlayerType.None; // <item> fails to activate
            actorPlayerTypeLookup[0x7a] = ActorPlayerType.None; // Interrupted/paralyzed/etc.  Failed action
            actorPlayerTypeLookup[0x7b] = ActorPlayerType.None; // Red 'error' text. Ignore
            actorPlayerTypeLookup[0x7c] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x7d] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x7e] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x7f] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x80] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x81] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x82] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x83] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x84] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x85] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x86] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x87] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x88] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x89] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x8a] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x8b] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x8c] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x8d] = ActorPlayerType.None; // Cannot attack
            actorPlayerTypeLookup[0x8e] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x8f] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x90] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x91] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x92] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x93] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x94] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x95] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x96] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x97] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x98] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x99] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x9a] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x9b] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x9c] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x9d] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x9e] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0x9f] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0xa0] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0xa1] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0xa2] = ActorPlayerType.Alliance; // <am> cures
            actorPlayerTypeLookup[0xa3] = ActorPlayerType.Alliance; // <am> hits
            actorPlayerTypeLookup[0xa4] = ActorPlayerType.Alliance; // <am> misses
            actorPlayerTypeLookup[0xa5] = ActorPlayerType.Alliance; // <mob> casts on <am>
            actorPlayerTypeLookup[0xa6] = ActorPlayerType.Alliance; // <am> kills
            actorPlayerTypeLookup[0xa7] = ActorPlayerType.None; // <am> dies
            actorPlayerTypeLookup[0xa8] = ActorPlayerType.Alliance; // prep spell cast
            actorPlayerTypeLookup[0xa9] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0xaa] = ActorPlayerType.Alliance; // <am> is resisted, or misses /ra
            actorPlayerTypeLookup[0xab] = ActorPlayerType.Alliance; // <am> uses item
            actorPlayerTypeLookup[0xac] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0xad] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0xae] = ActorPlayerType.None; // <am> enfeebled
            actorPlayerTypeLookup[0xaf] = ActorPlayerType.Alliance; // <am> uses self-buff
            actorPlayerTypeLookup[0xb0] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0xb1] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0xb2] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0xb3] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0xb4] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0xb5] = ActorPlayerType.None; // <am> avoids ability
            actorPlayerTypeLookup[0xb6] = ActorPlayerType.None; // <am> is enfeebled
            actorPlayerTypeLookup[0xb7] = ActorPlayerType.Alliance; // <am> gains buff
            actorPlayerTypeLookup[0xb8] = ActorPlayerType.None; //
            actorPlayerTypeLookup[0xb9] = ActorPlayerType.None; // <am> takes damage
            actorPlayerTypeLookup[0xba] = ActorPlayerType.None; // <am> avoids damage
            actorPlayerTypeLookup[0xbb] = ActorPlayerType.Alliance; // <am> drains
            actorPlayerTypeLookup[0xbc] = ActorPlayerType.Alliance; // <am> cures
        }
        #endregion

        #region Internal lookup calls
        internal InteractionType GetInteractionType(uint messageCode)
        {
            InteractionType interactType;

            if (interactionTypeLookup.TryGetValue(messageCode, out interactType))
                return interactType;
            else
                return InteractionType.Unknown;
        }

        internal AidType GetAidType(uint messageCode)
        {
            AidType aidType;

            if (aidTypeLookup.TryGetValue(messageCode, out aidType))
                return aidType;
            else
                return AidType.None;
        }

        internal HarmType GetHarmType(uint messageCode)
        {
            HarmType harmType;

            if (harmTypeLookup.TryGetValue(messageCode, out harmType))
                return harmType;
            else
                return HarmType.None;
        }

        internal SuccessType GetSuccessType(uint messageCode)
        {
            SuccessType successType;

            if (successTypeLookup.TryGetValue(messageCode, out successType))
                return successType;
            else
                return SuccessType.Unknown;
        }

        internal EntityType GetActorEntityType(uint messageCode)
        {
            EntityType entityType;

            if (actorEntityTypeLookup.TryGetValue(messageCode, out entityType))
                return entityType;
            else
                return EntityType.Unknown;
        }

        internal EntityType GetTargetEntityType(uint messageCode)
        {
            EntityType entityType;

            if (targetEntityTypeLookup.TryGetValue(messageCode, out entityType))
                return entityType;
            else
                return EntityType.Unknown;
        }

        internal ActorPlayerType GetActorPlayerType(uint messageCode)
        {
            ActorPlayerType playerType;

            if (actorPlayerTypeLookup.TryGetValue(messageCode, out playerType))
                return playerType;
            else
                return ActorPlayerType.None;
        }
        #endregion

        #region Alternate code sets
        internal List<uint> GetAlternateCodes(uint messageCode)
        {
            // case 0x11 -- effect on additional AOE targets
            //   return new List<uint>() { 0x12 -- possible originating message code (first target) }

            switch (messageCode)
            {
                // Enhancements
                case 0x38:
                    return new List<uint>() { 0x40, 0x6a, 0x3c };
                case 0x40:
                    return new List<uint>() { 0x38, 0xaf, 0x6a, 0x3c };
                case 0x6a:
                    return new List<uint>() { 0x38, 0x40, 0x3c };
                // Corsair/etc buffs (brd songs?)
                case 0x65:
                    return new List<uint>() { 0x6f };
                case 0x6f:
                    return new List<uint>() { 0x65 };
                case 0x66:
                    return new List<uint>() { 0x70, 0x6b, 0xae };
                case 0x70:
                    return new List<uint>() { 0x66 };
                case 0x6b:
                    return new List<uint>() { 0x66, 0x70, 0xae };
                case 0x72:
                    return new List<uint>() { 0x65, 0x6f };
                // Enfeebles (successful and resisted)
                case 0x44:
                    return new List<uint>() { 0x41 };
                case 0x45:
                    return new List<uint>() { 0x39, 0x3b, 0x3d, 0x3f, 0xb6 };
                case 0x3f:
                    return new List<uint>() { 0x39, 0x3b, 0x3d, 0x45, 0xb6 };
                case 0x39:
                    return new List<uint>() { 0x45, 0x3b, 0x3d, 0x3f, 0xb6 };
                case 0x3b:
                    return new List<uint>() { 0x39, 0x45, 0x3d, 0x3f, 0xb6 };
                case 0x3d:
                    return new List<uint>() { 0x39, 0x3b, 0x45, 0x3f, 0xb6 };
                case 0xb6:
                    return new List<uint>() { 0x39, 0x3b, 0x45, 0x3f, 0x45 };
                // AOE attacks
                // 6d = hits pm/am?, 1c = hits me
                case 0xb9: // hits <am>
                    return new List<uint>() { 0x6d, 0x1c, 0x20 };
                case 0xb5: // <am> evades
                    return new List<uint>() { 0x6d, 0x1c, 0x20 };
                case 0x68: // <me> evades
                    return new List<uint>() { 0x6d, 0x1c, 0x20 };
                case 0x20: // hits <pm>
                    return new List<uint>() { 0x6d, 0x1c };
                // AOE Drain
                case 0x16:
                    return new List<uint>() { 0x1b, 0xa5 };
                case 0x1b:
                    return new List<uint>() { 0x16, 0xa5 };
                case 0xa5:
                    return new List<uint>() { 0x16, 0x1b };
                // Item use
                case 0x5b:
                    return new List<uint>() { 0xab };
                case 0x17:
                    return new List<uint>() { 0x51 };
                case 0x18:
                    return new List<uint>() { 0x55 };
            }

            return null;
        }
        #endregion

    }
}
