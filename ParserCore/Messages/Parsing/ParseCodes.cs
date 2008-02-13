using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser
{
    internal class ParseCodes
    {
        #region Member lookup variables
        private Dictionary<uint, InteractionType> interactionTypeLookup;
        private Dictionary<uint, AidType> aidTypeLookup;
        private Dictionary<uint, HarmType> harmTypeLookup;
        private Dictionary<uint, SuccessType> successTypeLookup;
        #endregion

        #region Constructor
        internal ParseCodes()
        {
            InitLookupTables();
        }

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
            interactionTypeLookup = new Dictionary<uint, InteractionType>();

            // Successful attacks
            interactionTypeLookup[0x14] = InteractionType.Harm;
            interactionTypeLookup[0x19] = InteractionType.Harm;
            interactionTypeLookup[0x1c] = InteractionType.Harm;
            interactionTypeLookup[0x28] = InteractionType.Harm;
            interactionTypeLookup[0x2a] = InteractionType.Harm;
            interactionTypeLookup[0xa3] = InteractionType.Harm; // ally
            // Unsuccessful attacks
            interactionTypeLookup[0x15] = InteractionType.Harm;
            interactionTypeLookup[0x1a] = InteractionType.Harm;
            interactionTypeLookup[0x1d] = InteractionType.Harm;
            interactionTypeLookup[0x21] = InteractionType.Harm;
            interactionTypeLookup[0x29] = InteractionType.Harm;
            interactionTypeLookup[0xa4] = InteractionType.Harm; // ally
            // Enfeebling
            interactionTypeLookup[0x32] = InteractionType.Harm;
            interactionTypeLookup[0x33] = InteractionType.Harm;
            interactionTypeLookup[0x39] = InteractionType.Harm;
            interactionTypeLookup[0x3d] = InteractionType.Harm;
            interactionTypeLookup[0x41] = InteractionType.Harm;
            interactionTypeLookup[0x66] = InteractionType.Harm;
            interactionTypeLookup[0x70] = InteractionType.Harm;
            // Resisted enfeebles
            interactionTypeLookup[0x3b] = InteractionType.Harm;
            interactionTypeLookup[0x3f] = InteractionType.Harm;
            interactionTypeLookup[0x43] = InteractionType.Harm;
            interactionTypeLookup[0x44] = InteractionType.Harm;
            interactionTypeLookup[0x45] = InteractionType.Harm;
            // ally enfeebles
            interactionTypeLookup[0xbb] = InteractionType.Harm;
            interactionTypeLookup[0xaa] = InteractionType.Harm;
            // Ability moves
            interactionTypeLookup[0x20] = InteractionType.Harm;
            interactionTypeLookup[0x6b] = InteractionType.Harm;
            // Ability moves that miss
            interactionTypeLookup[0x68] = InteractionType.Harm;
            interactionTypeLookup[0x6d] = InteractionType.Harm;
            interactionTypeLookup[0x72] = InteractionType.Harm;
            // Prep attack moves
            interactionTypeLookup[0x6e] = InteractionType.Harm; // <me> prep weaponskill

            interactionTypeLookup[0x24] = InteractionType.Harm; // <me> kills
            interactionTypeLookup[0x25] = InteractionType.Harm; // <party> kills
            interactionTypeLookup[0x26] = InteractionType.Harm;
            interactionTypeLookup[0x27] = InteractionType.Harm;
            interactionTypeLookup[0x2c] = InteractionType.Harm; // <other> kills
            interactionTypeLookup[0xa6] = InteractionType.Harm;
            interactionTypeLookup[0xa7] = InteractionType.Harm;

            // Prep buffs
            //combatCategoryLookup[0x34] = CombatCategory.Buff;
            // Use buffs
            interactionTypeLookup[0x65] = InteractionType.Aid;
            interactionTypeLookup[0x6a] = InteractionType.Aid;
            interactionTypeLookup[0x6f] = InteractionType.Aid;
            // Enhance
            interactionTypeLookup[0x38] = InteractionType.Aid;
            interactionTypeLookup[0x3b] = InteractionType.Aid; // failed
            interactionTypeLookup[0x3c] = InteractionType.Aid;
            interactionTypeLookup[0x40] = InteractionType.Aid;
            // Recovery
            interactionTypeLookup[0x1e] = InteractionType.Aid; // drain samba
            interactionTypeLookup[0x1f] = InteractionType.Aid; // cures
            interactionTypeLookup[0x17] = InteractionType.Aid; // <me> cast cures
            interactionTypeLookup[0x18] = InteractionType.Aid; // <party> cast cures
            interactionTypeLookup[0x2b] = InteractionType.Aid;

            interactionTypeLookup[0x23] = InteractionType.Aid;

            interactionTypeLookup[0x5a] = InteractionType.Aid; // item use
            interactionTypeLookup[0x55] = InteractionType.Aid; // <party> item use


            // Prep spell of unknown type (buff)?
            interactionTypeLookup[0x34] = InteractionType.Unknown;
            // Prepping moves of unknown types (buffs?)
            interactionTypeLookup[0x64] = InteractionType.Unknown;
            interactionTypeLookup[0x66] = InteractionType.Unknown;

            // Considered an attack attempt; success type will be Failed
            interactionTypeLookup[0x7a] = InteractionType.Harm;
            interactionTypeLookup[0x7b] = InteractionType.Harm;
        }

        private void InitAidTypeLookup()
        {
            aidTypeLookup = new Dictionary<uint, AidType>();

            // Prepping spell
            aidTypeLookup[0x34] = AidType.Enhance;
            // Enhance target
            aidTypeLookup[0x38] = AidType.Enhance;
            aidTypeLookup[0x3b] = AidType.Enhance; // failed
            aidTypeLookup[0x3c] = AidType.Enhance;
            aidTypeLookup[0x40] = AidType.Enhance;
            // Enhance self
            aidTypeLookup[0x65] = AidType.Enhance;
            aidTypeLookup[0x6a] = AidType.Enhance;
            aidTypeLookup[0x6f] = AidType.Enhance;

            aidTypeLookup[0x1e] = AidType.Recovery; // drain samba
            aidTypeLookup[0x22] = AidType.Recovery; // drain samba
            aidTypeLookup[0x2a] = AidType.Recovery; // drain samba
            aidTypeLookup[0x1f] = AidType.Recovery;
            aidTypeLookup[0x17] = AidType.Recovery;
            aidTypeLookup[0x18] = AidType.Recovery;
            aidTypeLookup[0x2b] = AidType.Recovery;

            aidTypeLookup[0x23] = AidType.Recovery;

            aidTypeLookup[0x5a] = AidType.Item;
            aidTypeLookup[0x55] = AidType.Item;
        }

        private void InitHarmTypeLookup()
        {
            harmTypeLookup = new Dictionary<uint, HarmType>();

            // Successful attacks
            harmTypeLookup[0x14] = HarmType.Damage;
            harmTypeLookup[0x19] = HarmType.Damage;
            harmTypeLookup[0x1c] = HarmType.Damage;
            harmTypeLookup[0x28] = HarmType.Damage;
            harmTypeLookup[0xa3] = HarmType.Damage;
            harmTypeLookup[0x2a] = HarmType.Drain;
            harmTypeLookup[0xbb] = HarmType.Drain;
            // Unsuccessful attacks
            harmTypeLookup[0x15] = HarmType.Damage;
            harmTypeLookup[0x1a] = HarmType.Damage;
            harmTypeLookup[0x1d] = HarmType.Damage;
            harmTypeLookup[0x21] = HarmType.Damage;
            harmTypeLookup[0x29] = HarmType.Damage;
            harmTypeLookup[0xa4] = HarmType.Damage;
            // Uncertain
            harmTypeLookup[0x32] = HarmType.None;
            // Enfeebling
            harmTypeLookup[0x33] = HarmType.Enfeeble;
            harmTypeLookup[0x34] = HarmType.Enfeeble;
            harmTypeLookup[0x39] = HarmType.Enfeeble;
            harmTypeLookup[0x3d] = HarmType.Enfeeble;
            harmTypeLookup[0x41] = HarmType.Enfeeble;
            harmTypeLookup[0x66] = HarmType.Enfeeble;
            harmTypeLookup[0x6b] = HarmType.Enfeeble;
            harmTypeLookup[0x70] = HarmType.Enfeeble;
            // Resisted enfeebles
            harmTypeLookup[0x3b] = HarmType.Enfeeble;
            harmTypeLookup[0x3f] = HarmType.Enfeeble;
            harmTypeLookup[0x44] = HarmType.Enfeeble;
            harmTypeLookup[0x45] = HarmType.Enfeeble;
            harmTypeLookup[0xaa] = HarmType.Enfeeble;
            // No effect
            harmTypeLookup[0x43] = HarmType.Enfeeble;
            // Ability moves
            harmTypeLookup[0x20] = HarmType.Damage;
            // Ability moves that miss
            harmTypeLookup[0x68] = HarmType.Damage;
            harmTypeLookup[0x6d] = HarmType.Damage;
            harmTypeLookup[0x72] = HarmType.Damage;

            harmTypeLookup[0x24] = HarmType.Death; // <me> kills
            harmTypeLookup[0x25] = HarmType.Death; // <party> kills
            harmTypeLookup[0x26] = HarmType.Death;
            harmTypeLookup[0x27] = HarmType.Death;
            harmTypeLookup[0x2c] = HarmType.Death; // <other> kills
            harmTypeLookup[0xa6] = HarmType.Death;
            harmTypeLookup[0xa7] = HarmType.Death;

            // Failed actions
            harmTypeLookup[0x7a] = HarmType.None;
            harmTypeLookup[0x7b] = HarmType.None;

        }

        private void InitSuccessTypeLookup()
        {
            successTypeLookup = new Dictionary<uint, SuccessType>();

            successTypeLookup[0x7a] = SuccessType.Failed;
            successTypeLookup[0x7b] = SuccessType.Failed;

            // Sucessful damage attempts
            successTypeLookup[0x14] = SuccessType.Successful;
            successTypeLookup[0x19] = SuccessType.Successful;
            successTypeLookup[0x1c] = SuccessType.Successful;
            successTypeLookup[0x28] = SuccessType.Successful;
            successTypeLookup[0xa3] = SuccessType.Successful;
            successTypeLookup[0x2a] = SuccessType.Successful;
            successTypeLookup[0xbb] = SuccessType.Successful;

            // Unsucessful damage attempts
            successTypeLookup[0x15] = SuccessType.Unsuccessful;
            successTypeLookup[0x1a] = SuccessType.Unsuccessful;
            successTypeLookup[0x1d] = SuccessType.Unsuccessful;
            successTypeLookup[0x21] = SuccessType.Unsuccessful;
            successTypeLookup[0x29] = SuccessType.Unsuccessful;
            successTypeLookup[0xa4] = SuccessType.Unsuccessful;

            // Ability moves (AOE?)
            successTypeLookup[0x20] = SuccessType.Successful;
            successTypeLookup[0x6b] = SuccessType.Unsuccessful;

            // Missed ability moves
            successTypeLookup[0x68] = SuccessType.Unsuccessful;
            successTypeLookup[0x6d] = SuccessType.Unsuccessful;
            successTypeLookup[0x72] = SuccessType.Unsuccessful;

            // Enfeebles
            //successTypeLookup[0x32] = SuccessType.Successful; // starts casting, success unknown
            successTypeLookup[0x33] = SuccessType.Successful;
            successTypeLookup[0x39] = SuccessType.Successful;
            successTypeLookup[0x3d] = SuccessType.Successful;
            successTypeLookup[0x41] = SuccessType.Successful;
            successTypeLookup[0x66] = SuccessType.Successful;
            successTypeLookup[0x70] = SuccessType.Successful;

            // Recovery
            successTypeLookup[0x1e] = SuccessType.Successful;
            successTypeLookup[0x17] = SuccessType.Successful;
            successTypeLookup[0x18] = SuccessType.Successful;
            successTypeLookup[0x1f] = SuccessType.Successful;
            successTypeLookup[0x23] = SuccessType.Successful;
            successTypeLookup[0x2b] = SuccessType.Successful;

            // Item use
            successTypeLookup[0x5a] = SuccessType.Successful;
            successTypeLookup[0x55] = SuccessType.Successful; // <party>


            // Resisted enfeebles
            successTypeLookup[0x3b] = SuccessType.Unsuccessful;
            successTypeLookup[0x3f] = SuccessType.Unsuccessful;
            successTypeLookup[0x44] = SuccessType.Unsuccessful;
            successTypeLookup[0x45] = SuccessType.Unsuccessful;
            successTypeLookup[0xaa] = SuccessType.Unsuccessful;

            // Failed enfeeble
            successTypeLookup[0x43] = SuccessType.Failed;
            // Failed enhancement
            successTypeLookup[0x3b] = SuccessType.Failed;

        }
        #endregion

        #region internal lookup calls
        internal InteractionType GetInteractionType(uint messageCode)
        {
            if (interactionTypeLookup.ContainsKey(messageCode))
                return interactionTypeLookup[messageCode];
            else
                return InteractionType.Unknown;
        }

        internal AidType GetAidType(uint messageCode)
        {
            if (aidTypeLookup.ContainsKey(messageCode))
                return aidTypeLookup[messageCode];
            else
                return AidType.None;
        }

        internal HarmType GetHarmType(uint messageCode)
        {
            if (harmTypeLookup.ContainsKey(messageCode))
                return harmTypeLookup[messageCode];
            else
                return HarmType.None;
        }

        internal SuccessType GetSuccessType(uint messageCode)
        {
            if (successTypeLookup.ContainsKey(messageCode))
                return successTypeLookup[messageCode];
            else
                return SuccessType.Unknown;
        }
        #endregion

    }
}
