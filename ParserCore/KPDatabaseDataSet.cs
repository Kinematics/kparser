using System;
using System.Collections.Generic;
using System.Linq;

namespace WaywardGamers.KParser
{

    public partial class KPDatabaseDataSet
    {
        /// <summary>
        /// Class to allow Distinct comparisons between interaction elements based on timestamps.
        /// </summary>
        public class InteractionTimestampComparer : IEqualityComparer<KPDatabaseDataSet.InteractionsRow>
        {
            public InteractionTimestampComparer()
            {
            }

            public bool Equals(KPDatabaseDataSet.InteractionsRow x, KPDatabaseDataSet.InteractionsRow y)
            {
                if (x.Timestamp == y.Timestamp)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public int GetHashCode(KPDatabaseDataSet.InteractionsRow obj)
            {
                return obj.Timestamp.GetHashCode();
            }
        }

        /// <summary>
        /// Extensions for the Battles table.
        /// </summary>
        public partial class BattlesDataTable
        {
            /// <summary>
            /// Get the default battle row.
            /// </summary>
            /// <returns>Returns the default battle row, or null if it is not found.</returns>
            public BattlesRow GetDefaultBattle()
            {
                return this.SingleOrDefault(b => b.DefaultBattle == true);
            }

            /// <summary>
            /// Gets a list of all battle rows that have not been closed (marked as killed
            /// or timed out).
            /// </summary>
            /// <returns>A list of all currently active battles.</returns>
            public IEnumerable<BattlesRow> GetActiveBattles()
            {
                var activeBattles = from b in this
                                    where ((b.EndTime == MagicNumbers.MinSQLDateTime || b.Killed == false) &&
                                           (b.DefaultBattle == false))
                                    select b;

                return activeBattles;
            }
        }

        /// <summary>
        /// Extensions for the Battle rows.
        /// </summary>
        public partial class BattlesRow
        {
            /// <summary>
            /// Gets the base experience value for the mob of the given battle.
            /// This is calculated by removing the experience chain bonus amount
            /// from the reported value.
            /// </summary>
            /// <returns>Returns the base experience value for the mob.</returns>
            public int BaseExperience()
            {
                if (ExperiencePoints == 0)
                    return 0;

                double xpFactor;

                switch (ExperienceChain)
                {
                    case 0:
                        //xpFactor = 1.00;
                        return ExperiencePoints;
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

            public int MinBaseExperience()
            {
                int baseXP = this.BaseExperience();

                var closeMin = from b in this.CombatantsRowByEnemyCombatantRelation.GetBattlesRowsByEnemyCombatantRelation()
                               where (Math.Abs(baseXP - b.BaseExperience()) < 2)
                               select b;

                return closeMin.Min(x => x.BaseExperience());
            }

            /// <summary>
            /// Gets the length of the fight for a given battle.
            /// </summary>
            /// <returns>A timespan of the battle's length.</returns>
            public TimeSpan FightLength()
            {
                if ((Killed == true) || (EndTime != MagicNumbers.MinSQLDateTime))
                {
                    return (EndTime - StartTime);
                }
                else
                {
                    return TimeSpan.Zero;
                }
                //throw new InvalidOperationException("Cannot get the fight length.  No End Time specified.");
            }
        }

        /// <summary>
        /// Extensions for the Combatants table.
        /// </summary>
        public partial class CombatantsDataTable
        {
            /// <summary>
            /// Find any combatant with the given name.  Name cannot be null or empty string.
            /// </summary>
            /// <param name="name">The name to search for.</param>
            /// <returns>Returns the table row with that name, or null if the name is not found.</returns>
            public CombatantsRow FindCombatantByName(string name)
            {
                if (name == null)
                    throw new ArgumentNullException("name");

                if (name == string.Empty)
                    throw new ArgumentOutOfRangeException("name", "Name cannot be empty.");

                return this.FirstOrDefault(c => c.CombatantName == name);
            }

            /// <summary>
            /// Find a player (or unknown) combatant with the given name.
            /// Name cannot be null or empty string.
            /// </summary>
            /// <param name="name">The name to search for.</param>
            /// <returns>Returns the table row with that name, or null if the name is not found.</returns>
            public CombatantsRow FindPlayerByName(string name)
            {
                if (name == null)
                    throw new ArgumentNullException("name");

                if (name == string.Empty)
                    throw new ArgumentOutOfRangeException("name", "Name cannot be empty.");

                var player = this.SingleOrDefault(c => c.CombatantName == name &&
                    c.CombatantType == (byte)EntityType.Player);

                if (player == null)
                    player = this.SingleOrDefault(c => c.CombatantName == name &&
                        c.CombatantType == (byte)EntityType.Unknown);

                return player;
            }

            /// <summary>
            /// Find a player or pet (or unknown) combatant with the given name.
            /// Name cannot be null or empty string.
            /// </summary>
            /// <param name="name">The name to search for.</param>
            /// <returns>Returns the table row with that name, or null if the name is not found.</returns>
            public CombatantsRow FindPlayerOrPetByName(string name)
            {
                if (name == null)
                    throw new ArgumentNullException("name");

                if (name == string.Empty)
                    throw new ArgumentOutOfRangeException("name", "Name cannot be empty.");

                var player = this.FirstOrDefault(c => c.CombatantName == name &&
                    ((EntityType)c.CombatantType == EntityType.Player ||
                     (EntityType)c.CombatantType == EntityType.Pet));

                if (player == null)
                    player = this.SingleOrDefault(c => c.CombatantName == name &&
                        (EntityType)c.CombatantType == EntityType.Unknown);

                return player;
            }

            /// <summary>
            /// Find a mob (or unknown) combatant with the given name.
            /// Name cannot be null or empty string.
            /// </summary>
            /// <param name="name">The name to search for.</param>
            /// <returns>Returns the table row with that name, or null if the name is not found.</returns>
            public CombatantsRow FindMobByName(string name)
            {
                if (name == null)
                    throw new ArgumentNullException("name");

                if (name == string.Empty)
                    throw new ArgumentOutOfRangeException("name", "Name cannot be empty.");

                var mob = this.SingleOrDefault(c => c.CombatantName == name &&
                    (EntityType)c.CombatantType == EntityType.Mob);

                if (mob == null)
                    mob = this.SingleOrDefault(c => c.CombatantName == name &&
                        (EntityType)c.CombatantType == EntityType.Unknown);

                return mob;
            }

            /// <summary>
            /// Find a combatant of specified type by name.
            /// Name cannot be null or empty string.
            /// </summary>
            /// <param name="name">The name to look for.</param>
            /// <param name="entityType">The entity type to look for.</param>
            /// <returns>A CombatantsRow containing the specified combatant, or null if not found.</returns>
            public CombatantsRow FindByNameAndType(string name, EntityType entityType)
            {
                if (name == null)
                    throw new ArgumentNullException("name");

                if (name == string.Empty)
                    throw new ArgumentOutOfRangeException("name", "Name cannot be empty.");

                var combatant = this.SingleOrDefault(c => c.CombatantName == name &&
                    c.CombatantType == (byte)entityType);

                if (combatant == null)
                    combatant = this.SingleOrDefault(c => c.CombatantName == name &&
                        c.CombatantType == (byte)EntityType.Unknown);

                return combatant;
            }

            /// <summary>
            /// Find a combatant of specified type by name.
            /// Name cannot be null or empty string.
            /// Side effects: If the combatant name was found as an Unknown entity type,
            /// the combatant's type is set to the requested type.
            /// If combatant was not found, a new row is created and returned.
            /// </summary>
            /// <param name="name">The name to look for.</param>
            /// <param name="entityType">The entity type to look for.</param>
            /// <returns>A CombatantsRow containing the specified combatant.</returns>
            public CombatantsRow GetCombatant(string name, EntityType entityType)
            {
                if (name == null)
                    throw new ArgumentNullException("name");

                if (name == string.Empty)
                    throw new ArgumentOutOfRangeException("name", "Name cannot be empty.");

                CombatantsRow combatant = null;

                if (entityType == EntityType.Unknown)
                {
                    combatant = FindCombatantByName(name);

                    if (combatant == null)
                        combatant = AddCombatantsRow(name, (byte)entityType, null);
                }
                else
                {
                    combatant = FindByNameAndType(name, entityType);

                    if ((combatant != null) && (combatant.CombatantType == (byte) EntityType.Unknown))
                        combatant.CombatantType = (byte)entityType;

                    if (combatant == null)
                        combatant = AddCombatantsRow(name, (byte)entityType, null);
                }

                return combatant;
            }
        }

        /// <summary>
        /// Extensions for the Items table.
        /// </summary>
        public partial class ItemsDataTable
        {
            /// <summary>
            /// Find an item by name.  Name cannot be null or empty string.
            /// </summary>
            /// <param name="itemName">The name of the item to look for.</param>
            /// <returns>The row the item was found on.  Returns null if not found.</returns>
            public ItemsRow FindByItemName(string itemName)
            {
                if (itemName == null)
                    throw new ArgumentNullException("itemName");

                if (itemName == string.Empty)
                    throw new ArgumentOutOfRangeException("itemName", "Name cannot be empty.");

                return this.SingleOrDefault(i => string.Compare(i.ItemName, itemName, true) == 0);
            }

            /// <summary>
            /// Gets an item with the specified name.
            /// Name cannot be null or empty string.
            /// Side effects: If the name is not found in the table, a new
            /// row is created.
            /// </summary>
            /// <param name="itemName">The name to search for.</param>
            /// <returns>The ItemsRow containing the specified item name.</returns>
            public ItemsRow GetItem(string itemName)
            {
                if (itemName == null)
                    throw new ArgumentNullException("itemName");

                if (itemName == string.Empty)
                    throw new ArgumentOutOfRangeException("itemName", "Name cannot be empty.");

                ItemsRow item = null;

                // Get the row for the action name
                item = FindByItemName(itemName);

                if (item == null)
                    item = AddItemsRow(itemName);

                return item;
            }
        }

        /// <summary>
        /// Extensions for the Actions table.
        /// </summary>
        public partial class ActionsDataTable
        {
            /// <summary>
            /// Find an action by name.  Name cannot be null or empty string.
            /// </summary>
            /// <param name="actionName">The name of the action to look for.</param>
            /// <returns>The row the action was found on.  Returns null if not found.</returns>
            public ActionsRow FindByActionName(string actionName)
            {
                if (actionName == null)
                    throw new ArgumentNullException("actionName");

                if (actionName == string.Empty)
                    throw new ArgumentOutOfRangeException("actionName", "Name cannot be empty.");

                return this.SingleOrDefault(i => string.Compare(i.ActionName, actionName, true) == 0);
            }

            /// <summary>
            /// Gets an action with the specified name.
            /// Name cannot be null or empty string.
            /// Side effects: If the name is not found in the table, a new
            /// row is created.
            /// </summary>
            /// <param name="actionName">The name to search for.</param>
            /// <returns>The ActionsRow containing the specified action name.</returns>
            public ActionsRow GetAction(string actionName)
            {
                if (actionName == null)
                    throw new ArgumentNullException("actionName");

                if (actionName == string.Empty)
                    throw new ArgumentOutOfRangeException("actionName", "Name cannot be empty.");

                // Get the row for the action name
                ActionsRow action = FindByActionName(actionName);

                if (action == null)
                {
                    // Ensure we don't get fully lower-case entries
                    if (actionName == actionName.ToLower())
                    {
                        string firstChar = actionName.Substring(0, 1);
                        firstChar = firstChar.ToUpper();
                        actionName = firstChar + actionName.Substring(1);
                    }

                    action = AddActionsRow(actionName);
                }

                return action;
            }
        }

        /// <summary>
        /// Extensions for the ChatSpeakers table.
        /// </summary>
        public partial class ChatSpeakersDataTable
        {
            /// <summary>
            /// Find a chat speaker by name.  Name cannot be null or empty string.
            /// </summary>
            /// <param name="speakerName">The name of the speaker to look for.</param>
            /// <returns>The row containing the specified speaker.  Returns null if not found.</returns>
            public ChatSpeakersRow FindBySpeakerName(string speakerName)
            {
                if (speakerName == null)
                    throw new ArgumentNullException("speakerName");

                if (speakerName == string.Empty)
                    throw new ArgumentOutOfRangeException("speakerName", "Name cannot be empty.");

                return this.SingleOrDefault(cs => string.Compare(cs.SpeakerName, speakerName, true) == 0);
            }

            /// <summary>
            /// Gets a chat speaker with the specified name.
            /// Name cannot be null or empty string.
            /// Side effects: If the name is not found in the table, a new
            /// row is created.
            /// </summary>
            /// <param name="speakerName">The name of the speaker to look for.</param>
            /// <returns>The row containing the specified speaker.</returns>
            public ChatSpeakersRow GetSpeaker(string speakerName)
            {
                if (speakerName == null)
                    throw new ArgumentNullException("speakerName");

                if (speakerName == string.Empty)
                    throw new ArgumentOutOfRangeException("speakerName", "Name cannot be empty.");

                // Get the row for the action name
                ChatSpeakersRow speaker = FindBySpeakerName(speakerName);

                if (speaker == null)
                    speaker = AddChatSpeakersRow(speakerName);

                return speaker;
            }
        }
    }
}
