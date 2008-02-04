using System;
using System.Collections.Generic;
using System.Linq;

namespace WaywardGamers.KParser {
    
    public partial class KPDatabaseDataSet 
    {
        public partial class BattlesDataTable{

            public BattlesRow GetDefaultBattle()
            {
                return this.SingleOrDefault(b => b.DefaultBattle == true);
            }

            public IEnumerable<BattlesRow> GetActiveBattles()
            {
                var activeBattles = from b in this
                                    where ((b.EndTime == MagicNumbers.MinSQLDateTime || b.Killed == false) &&
                                           (b.DefaultBattle == false))
                                    select b;

                return activeBattles;
            }
        }

        public partial class BattlesRow
        {
            public int BaseExperience()
            {
                if (ExperiencePoints == 0)
                    return 0;

                double xpFactor;

                switch (ExperienceChain)
                {
                    case 0:
                        xpFactor = 1.00;
                        break;
                    case 1:
                        xpFactor = 1.20;
                        break;
                    case 2:
                        xpFactor = 1.25;
                        break;
                    case 3:
                        xpFactor = 1.30;
                        break;
                    case 4:
                        xpFactor = 1.40;
                        break;
                    case 5:
                    default:
                        xpFactor = 1.50;
                        break;
                }

                double baseXP = Math.Ceiling((double)ExperiencePoints / xpFactor);

                return (int)baseXP;
            }

            public TimeSpan FightLength()
            {
                if (Killed == true)
                {
                    return (EndTime - StartTime);
                }
                else
                {
                    throw new InvalidOperationException("Cannot get the length of a fight that hasn't ended.");
                }
            }
        }

        public partial class CombatantsDataTable
        {
            public CombatantsRow FindCombatantByName(string name)
            {
                return this.FirstOrDefault(c => c.CombatantName == name);
            }

            public CombatantsRow FindPlayerByName(string name)
            {
                var player = this.SingleOrDefault(c => c.CombatantName == name &&
                    c.CombatantType == (byte)EntityType.Player);

                if (player == null)
                    player = this.SingleOrDefault(c => c.CombatantName == name &&
                        c.CombatantType == (byte)EntityType.Unknown);

                return player;
            }

            public CombatantsRow FindPlayerOrPetByName(string name)
            {
                var player = this.First(c => c.CombatantName == name &&
                    (c.CombatantType == (byte)EntityType.Player || c.CombatantType == (byte)EntityType.Pet));

                if (player == null)
                    player = this.SingleOrDefault(c => c.CombatantName == name &&
                        c.CombatantType == (byte)EntityType.Unknown);

                return player;
            }


            public CombatantsRow FindMobByName(string name)
            {
                var mob = this.SingleOrDefault(c => c.CombatantName == name &&
                    c.CombatantType == (byte)EntityType.Mob);

                if (mob == null)
                    mob = this.SingleOrDefault(c => c.CombatantName == name &&
                        c.CombatantType == (byte)EntityType.Unknown);

                return mob;
            }

            public CombatantsRow FindByNameAndType(string name, EntityType entityType)
            {
                var combatant = this.SingleOrDefault(c => c.CombatantName == name &&
                    c.CombatantType == (byte)entityType);

                if (combatant == null)
                    combatant = this.SingleOrDefault(c => c.CombatantName == name &&
                        c.CombatantType == (byte)EntityType.Unknown);

                return combatant;
            }

            public CombatantsRow GetCombatant(string name, EntityType entityType)
            {
                CombatantsRow combatant = null;

                if (name != string.Empty)
                {
                    if (entityType == EntityType.Unknown)
                    {
                        combatant = FindCombatantByName(name);

                        if (combatant == null)
                            combatant = AddCombatantsRow(name, (byte)entityType, null);
                    }
                    else
                    {
                        combatant = FindByNameAndType(name, entityType);

                        if ((combatant != null) && (combatant.CombatantType != (byte)entityType))
                            combatant.CombatantType = (byte)entityType;

                        if (combatant == null)
                            combatant = AddCombatantsRow(name, (byte)entityType, null);
                    }
                }

                return combatant;
            }
        }

        public partial class ItemsDataTable
        {
            public ItemsRow FindByItemName(string itemName)
            {
                return this.SingleOrDefault(i => i.ItemName == itemName);
            }

            public ItemsRow GetItem(string itemName)
            {
                ItemsRow item = null;

                if (itemName != string.Empty)
                {
                    // Get the row for the action name
                    item = FindByItemName(itemName);

                    if (item == null)
                        item = AddItemsRow(itemName);
                }

                return item;
            }
        }

        public partial class ActionsDataTable
        {
            public ActionsRow FindByActionName(string actionName)
            {
                return this.SingleOrDefault(i => i.ActionName == actionName);
            }

            public ActionsRow GetAction(string actionName)
            {
                ActionsRow action = null;

                if (actionName != string.Empty)
                {
                    // Get the row for the action name
                    action = FindByActionName(actionName);

                    if (action == null)
                        action = AddActionsRow(actionName);
                }

                return action;
            }
        }
    }
}
