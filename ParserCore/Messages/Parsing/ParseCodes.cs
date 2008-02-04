using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser
{
    internal class ParseCodes
    {
        #region Member lookup variables
        private Dictionary<uint, CombatActionType> combatCategoryLookup;
        private Dictionary<uint, BuffType> buffTypeLookup;
        private Dictionary<uint, AttackType> attackTypeLookup;
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
            InitBuffTypeLookup();
            InitAttackTypeLookup();
            InitSuccessTypeLookup();
        }
        #endregion

        #region Individual table initializations
        private void InitCombatCategoryLookup()
        {
            combatCategoryLookup = new Dictionary<uint, CombatActionType>();

            // Successful attacks
            combatCategoryLookup[0x14] = CombatActionType.Attack;
            combatCategoryLookup[0x19] = CombatActionType.Attack;
            combatCategoryLookup[0x1c] = CombatActionType.Attack;
            combatCategoryLookup[0x28] = CombatActionType.Attack;
            combatCategoryLookup[0x2a] = CombatActionType.Attack;
            // Unsuccessful attacks
            combatCategoryLookup[0x15] = CombatActionType.Attack;
            combatCategoryLookup[0x1a] = CombatActionType.Attack;
            combatCategoryLookup[0x1d] = CombatActionType.Attack;
            combatCategoryLookup[0x29] = CombatActionType.Attack;
            // Enfeebling
            combatCategoryLookup[0x32] = CombatActionType.Attack;
            combatCategoryLookup[0x33] = CombatActionType.Attack;
            combatCategoryLookup[0x39] = CombatActionType.Attack;
            combatCategoryLookup[0x3d] = CombatActionType.Attack;
            combatCategoryLookup[0x41] = CombatActionType.Attack;
            combatCategoryLookup[0x70] = CombatActionType.Attack;
            // Resisted enfeebles
            combatCategoryLookup[0x3b] = CombatActionType.Attack;
            combatCategoryLookup[0x3f] = CombatActionType.Attack;
            combatCategoryLookup[0x43] = CombatActionType.Attack;
            combatCategoryLookup[0x44] = CombatActionType.Attack;
            combatCategoryLookup[0x45] = CombatActionType.Attack;
            // Ability moves that miss
            combatCategoryLookup[0x68] = CombatActionType.Attack;
            combatCategoryLookup[0x72] = CombatActionType.Attack;
            // Prep attack moves
            combatCategoryLookup[0x6e] = CombatActionType.Attack; // <me> prep weaponskill

            // Prep buffs
            //combatCategoryLookup[0x34] = CombatCategory.Buff;
            // Use buffs
            combatCategoryLookup[0x65] = CombatActionType.Buff;
            combatCategoryLookup[0x6a] = CombatActionType.Buff;
            combatCategoryLookup[0x6f] = CombatActionType.Buff;
            // Enhance
            combatCategoryLookup[0x38] = CombatActionType.Buff;
            combatCategoryLookup[0x3b] = CombatActionType.Buff; // failed
            combatCategoryLookup[0x3c] = CombatActionType.Buff;
            combatCategoryLookup[0x40] = CombatActionType.Buff;
            // Recovery
            combatCategoryLookup[0x1e] = CombatActionType.Buff; // drain samba
            combatCategoryLookup[0x1f] = CombatActionType.Buff; // cures
            combatCategoryLookup[0x2b] = CombatActionType.Buff;

            combatCategoryLookup[0x5a] = CombatActionType.Buff; // item use


            // Prep spell of unknown type (buff)?
            combatCategoryLookup[0x34] = CombatActionType.Unknown;
            // Prepping moves of unknown types (buffs?)
            combatCategoryLookup[0x64] = CombatActionType.Unknown;
            combatCategoryLookup[0x66] = CombatActionType.Unknown;

            // Considered an attack attempt; success type will be Failed
            combatCategoryLookup[0x7a] = CombatActionType.Attack;
            combatCategoryLookup[0x7b] = CombatActionType.Attack;

            combatCategoryLookup[0x24] = CombatActionType.Death;
            combatCategoryLookup[0x25] = CombatActionType.Death;
            combatCategoryLookup[0x26] = CombatActionType.Death;
            combatCategoryLookup[0x27] = CombatActionType.Death;
            combatCategoryLookup[0x2c] = CombatActionType.Death;
            combatCategoryLookup[0xa6] = CombatActionType.Death;
            combatCategoryLookup[0xa7] = CombatActionType.Death;
        }

        private void InitBuffTypeLookup()
        {
            buffTypeLookup = new Dictionary<uint, BuffType>();

            // Prepping spell
            buffTypeLookup[0x34] = BuffType.Enhance;
            // Enhance target
            buffTypeLookup[0x38] = BuffType.Enhance;
            buffTypeLookup[0x3b] = BuffType.Enhance; // failed
            buffTypeLookup[0x3c] = BuffType.Enhance;
            buffTypeLookup[0x40] = BuffType.Enhance;
            // Enhance self
            buffTypeLookup[0x65] = BuffType.Enhance;
            buffTypeLookup[0x6a] = BuffType.Enhance;
            buffTypeLookup[0x6f] = BuffType.Enhance;

            buffTypeLookup[0x1e] = BuffType.Recovery; // drain samba
            buffTypeLookup[0x1f] = BuffType.Recovery;
            buffTypeLookup[0x2b] = BuffType.Recovery;

            buffTypeLookup[0x5a] = BuffType.Item;
        }

        private void InitAttackTypeLookup()
        {
            attackTypeLookup = new Dictionary<uint, AttackType>();

            // Successful attacks
            attackTypeLookup[0x14] = AttackType.Damage;
            attackTypeLookup[0x19] = AttackType.Damage;
            attackTypeLookup[0x1c] = AttackType.Damage;
            attackTypeLookup[0x28] = AttackType.Damage;
            attackTypeLookup[0x2a] = AttackType.Drain;
            // Unsuccessful attacks
            attackTypeLookup[0x15] = AttackType.Damage;
            attackTypeLookup[0x1a] = AttackType.Damage;
            attackTypeLookup[0x1d] = AttackType.Damage;
            attackTypeLookup[0x29] = AttackType.Damage;
            // Uncertain
            attackTypeLookup[0x32] = AttackType.Unknown;
            // Enfeebling
            attackTypeLookup[0x33] = AttackType.Enfeeble;
            attackTypeLookup[0x34] = AttackType.Enfeeble;
            attackTypeLookup[0x39] = AttackType.Enfeeble;
            attackTypeLookup[0x3d] = AttackType.Enfeeble;
            attackTypeLookup[0x41] = AttackType.Enfeeble;
            attackTypeLookup[0x70] = AttackType.Enfeeble;
            // Resisted enfeebles
            attackTypeLookup[0x3b] = AttackType.Enfeeble;
            attackTypeLookup[0x3f] = AttackType.Enfeeble;
            attackTypeLookup[0x44] = AttackType.Enfeeble;
            attackTypeLookup[0x45] = AttackType.Enfeeble;
            // No effect
            attackTypeLookup[0x43] = AttackType.Enfeeble;
            // Ability moves that miss
            attackTypeLookup[0x68] = AttackType.Damage;
            attackTypeLookup[0x72] = AttackType.Damage;

            // Failed actions
            attackTypeLookup[0x7a] = AttackType.Unknown;
            attackTypeLookup[0x7b] = AttackType.Unknown;

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
            successTypeLookup[0x2a] = SuccessType.Successful;

            // Unsucessful damage attempts
            successTypeLookup[0x15] = SuccessType.Unsuccessful;
            successTypeLookup[0x1a] = SuccessType.Unsuccessful;
            successTypeLookup[0x1d] = SuccessType.Unsuccessful;
            successTypeLookup[0x29] = SuccessType.Unsuccessful;

            // Missed ability moves
            successTypeLookup[0x68] = SuccessType.Unsuccessful;
            successTypeLookup[0x72] = SuccessType.Unsuccessful;

            // Enfeebles
            //successTypeLookup[0x32] = SuccessType.Successful; // starts casting, success unknown
            successTypeLookup[0x33] = SuccessType.Successful;
            successTypeLookup[0x39] = SuccessType.Successful;
            successTypeLookup[0x3d] = SuccessType.Successful;
            successTypeLookup[0x41] = SuccessType.Successful;
            successTypeLookup[0x70] = SuccessType.Successful;

            // Recovery
            successTypeLookup[0x1e] = SuccessType.Successful;
            successTypeLookup[0x1f] = SuccessType.Successful;
            successTypeLookup[0x2b] = SuccessType.Successful;

            // Item use
            successTypeLookup[0x5a] = SuccessType.Successful;


            // Resisted enfeebles
            successTypeLookup[0x3b] = SuccessType.Unsuccessful;
            successTypeLookup[0x3f] = SuccessType.Unsuccessful;
            successTypeLookup[0x44] = SuccessType.Unsuccessful;
            successTypeLookup[0x45] = SuccessType.Unsuccessful;

            // Failed enfeeble
            successTypeLookup[0x43] = SuccessType.Failed;
            // Failed enhancement
            successTypeLookup[0x3b] = SuccessType.Failed;

        }
        #endregion

        #region internal lookup calls
        internal CombatActionType GetCombatCategory(uint messageCode)
        {
            if (combatCategoryLookup.ContainsKey(messageCode))
                return combatCategoryLookup[messageCode];
            else
                return CombatActionType.Unknown;
        }

        internal BuffType GetBuffType(uint messageCode)
        {
            if (buffTypeLookup.ContainsKey(messageCode))
                return buffTypeLookup[messageCode];
            else
                return BuffType.Unknown;
        }

        internal AttackType GetAttackType(uint messageCode)
        {
            if (attackTypeLookup.ContainsKey(messageCode))
                return attackTypeLookup[messageCode];
            else
                return AttackType.Unknown;
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
