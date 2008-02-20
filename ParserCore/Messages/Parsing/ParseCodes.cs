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
        #endregion

        #region Setup
        private void InitLookupTables()
        {
            InitCombatCategoryLookup();
            InitAidTypeLookup();
            InitHarmTypeLookup();
            InitSuccessTypeLookup();
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
            interactionTypeLookup[0x3b] = InteractionType.Harm; // enfeeble <me>, resisted (failed enhance?)
            interactionTypeLookup[0x3c] = InteractionType.Aid; // buff <pm>
            interactionTypeLookup[0x3d] = InteractionType.Harm; // enfeeble <pm>
            interactionTypeLookup[0x3e] = InteractionType.Unknown; //
            interactionTypeLookup[0x3f] = InteractionType.Harm; // enfeeble <pm>, resisted
            interactionTypeLookup[0x40] = InteractionType.Aid; // buff <pm>, other?
            interactionTypeLookup[0x41] = InteractionType.Harm; // enfeeble target (any)
            interactionTypeLookup[0x42] = InteractionType.Unknown; //
            interactionTypeLookup[0x43] = InteractionType.Harm; // enfeeble, no effect (any target?)
            interactionTypeLookup[0x44] = InteractionType.Harm; // enfeeble, resisted (any target?)
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
            interactionTypeLookup[0x51] = InteractionType.Unknown; //
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
            interactionTypeLookup[0xaa] = InteractionType.Harm; // <am> is resisted, or misses /ra
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
            aidTypeLookup[0x44] = AidType.None; // enfeeble, resisted (any target?)
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
            aidTypeLookup[0x51] = AidType.None; //
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
            aidTypeLookup[0xaa] = AidType.None; // <am> is resisted, or misses /ra
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
            harmTypeLookup[0x51] = HarmType.None; //
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
            harmTypeLookup[0xaa] = HarmType.Unknown; // <am> is resisted, or misses /ra
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
            successTypeLookup[0x51] = SuccessType.None; //
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
                    return new List<uint>() { 0x38, 0x6a, 0x3c };
                case 0x6a:
                    return new List<uint>() { 0x38, 0x40, 0x3c };
                // Corsair/etc buffs (brd songs?)
                case 0x65:
                    return new List<uint>() { 0x6f };
                case 0x6f:
                    return new List<uint>() { 0x65 };
                case 0x66:
                    return new List<uint>() { 0x70, 0x6b };
                case 0x70:
                    return new List<uint>() { 0x66 };
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
                // Item use
                case 0x5b:
                    return new List<uint>() { 0xab };
            }

            return null;
        }
        #endregion

    }
}
