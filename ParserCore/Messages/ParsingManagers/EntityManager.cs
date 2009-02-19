using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser.Parsing
{
    /// <summary>
    /// This class is for storing and querying information about parsed entities.
    /// </summary>
    internal class EntityManager
    {
        #region Singleton Construction
        /// <summary>
        /// Make the class a singleton
        /// </summary>
        private static readonly EntityManager instance = new EntityManager();

        /// <summary>
        /// Gets the singleton instance of the NewMessageManager class.
        /// </summary>
        public static EntityManager Instance { get { return instance; } }
        
        /// <summary>
        /// Private constructor ensures singleton purity.
        /// </summary>
        private EntityManager()
		{
        }
        #endregion

        #region Member variables
        Dictionary<string, EntityType> entityCollection = new Dictionary<string, EntityType>();

        internal string LastAddedPetEntity { get; private set; }
        #endregion

        #region Initialization
        internal void Reset()
        {
            lock (entityCollection)
            {
                entityCollection.Clear();
            }

            LastAddedPetEntity = string.Empty;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Maintain a listing of entity types for combatants so that we
        /// don't have to continually reevaluate them at the message level.
        /// </summary>
        /// <param name="message">The message to add to the entity collection.</param>
        internal void AddEntitiesFromMessage(Message msg)
        {
            if (msg == null)
                return;

            if (msg.EventDetails == null)
                return;

            if (msg.EventDetails.CombatDetails == null)
                return;

            // Update base actor name
            string name = msg.EventDetails.CombatDetails.ActorName;

            if (name != string.Empty)
            {
                CheckAndAddEntity(name, msg.EventDetails.CombatDetails.ActorEntityType);
            }

            // Update all target names
            foreach (TargetDetails target in msg.EventDetails.CombatDetails.Targets)
            {
                name = target.Name;

                if ((name != null) && (name != string.Empty))
                {
                    CheckAndAddEntity(name, target.EntityType);
                }
            }
        }

        /// <summary>
        /// Explicitly add an player name as charmed.  This adds a _Charmed
        /// modifier to the lookup name to distinguish between players and
        /// normal mobs.
        /// </summary>
        /// <param name="name">The name of the mob that was charmed.</param>
        internal void AddCharmedPlayerEntity(string name)
        {
            if ((name == null) || (name == string.Empty))
                return;

            string charmedName = name + "_Charmed";

            if (entityCollection.ContainsKey(charmedName) == false)
            {
                entityCollection[charmedName] = EntityType.Mob;
            }
        }

        /// <summary>
        /// Explicitly add an entity name as a pet.  Called when the parse
        /// encounters a successful Charm attempt.  This adds a _Pet modifier
        /// to the lookup name to distinguish between pets and normal mobs.
        /// </summary>
        /// <param name="name">The name of the mob that was charmed.</param>
        internal void AddPetEntity(string name)
        {
            if ((name == null) || (name == string.Empty))
                return;

            string petName = name + "_Pet";
            LastAddedPetEntity = name;

            if (entityCollection.ContainsKey(petName) == false)
            {
                entityCollection[petName] = EntityType.Pet;
            }
        }

        /// <summary>
        /// Check to see if we've encountered the named combatant before.
        /// If so, use the entity type we got last time.  This checks for
        /// _Pets -after- normal mob name lookup.
        /// </summary>
        /// <param name="name">The name of the combatant to look up.</param>
        /// <returns>The entity type for the name provided, if available.</returns>
        internal EntityType LookupEntity(string name)
        {
            if ((name == null) || (name == string.Empty))
                return EntityType.Unknown;

            if (entityCollection.ContainsKey(name))
                return entityCollection[name];

            if (entityCollection.ContainsKey(name + "_Pet"))
                return EntityType.Pet;

            if (entityCollection.ContainsKey(name + "_Charmed"))
                return EntityType.Mob;

            return EntityType.Unknown;
        }

        /// <summary>
        /// Check to see if we've encountered the named combatant before.
        /// If so, use the entity type we got last time.  This checks for
        /// _Pets -before- normal mob name lookup.
        /// </summary>
        /// <param name="name">The name of the combatant to look up.</param>
        /// <returns>The entity type for the name provided, if available.</returns>
        internal EntityType LookupPetEntity(string name)
        {
            if ((name == null) || (name == string.Empty))
                return EntityType.Unknown;

            if (entityCollection.ContainsKey(name + "_Pet"))
                return EntityType.Pet;

            if (entityCollection.ContainsKey(name + "_Charmed"))
                return EntityType.Mob;

            if (entityCollection.ContainsKey(name))
                return entityCollection[name];

            return EntityType.Unknown;
        }

        internal void OverridePlayerToMob(string name)
        {
            if (entityCollection.ContainsKey(name))
            {
                if (entityCollection[name] == EntityType.Player)
                    entityCollection[name] = EntityType.Mob;
            }
        }
        #endregion

        #region Private methods
        private void CheckAndAddEntity(string name, EntityType entityType)
        {
            // Check to see if we have the specified name in the entity list
            if (entityCollection.ContainsKey(name))
            {
                // Currently known entity type value
                EntityType entityVal = entityCollection[name];

                // If it's the same as what we have, ignore and return.
                if (entityType == entityVal)
                    return;

                // If it's currently unknown, update with whatever we're given
                if (entityVal == EntityType.Unknown)
                {
                    entityCollection[name] = entityType;
                    return;
                }

                // If we have a pet/mob combination, change the collection type to
                // mob and add a new pet version (_Pet suffix).
                if (((entityVal == EntityType.Mob) || (entityType == EntityType.Mob)) &&
                    ((entityVal == EntityType.Pet) || (entityType == EntityType.Pet)))
                {
                    entityCollection[name] = EntityType.Mob;
                    AddPetEntity(name);
                    return;
                }

                // If we have a pet/mob combination, change the collection type to
                // player and add a new charmed version (_Charmed suffix).
                if (((entityVal == EntityType.Mob) || (entityType == EntityType.Mob)) &&
                    ((entityVal == EntityType.Player) || (entityType == EntityType.Player)))
                {
                    entityCollection[name] = EntityType.Player;
                    AddCharmedPlayerEntity(name);
                    return;
                }

                // If we get to here we have some other combination that's unaccounted for.
                // Log it and continue.
                Logger.Instance.Log("Unresolved entity type mismatch.",
                    string.Format("Entity table for {0} is {1}, being asked to add as type {2}.",
                    name, entityVal, entityType));
            }

            // If it's not currently in the collection, or we didn't get it
            // taken care of above, add it.
            entityCollection[name] = entityType;
        }

        #endregion

        #region Debugging output
        internal void DumpData(StreamWriter sw)
        {
            if (sw == null)
                throw new ArgumentNullException("sw");

            if (sw.BaseStream.CanWrite == false)
                throw new InvalidOperationException("Provided stream does not support writing.");

            if (entityCollection.Count > 0)
            {
                sw.WriteLine("".PadRight(42, '-'));
                sw.WriteLine("Entity List\n");
                sw.WriteLine(string.Format("{0}{1}", "Name".PadRight(32), "Type"));
                sw.WriteLine(string.Format("{0}    {1}", "".PadRight(28, '-'), "".PadRight(10, '-')));

                foreach (var entity in entityCollection)
                {
                    sw.WriteLine(string.Format("{0}{1}", entity.Key.PadRight(32), entity.Value));
                }

                sw.WriteLine("".PadRight(42, '-'));
                sw.WriteLine();
            }
            else
            {
                sw.WriteLine("".PadRight(42, '-'));
                sw.WriteLine("Entity List is Empty\n");
            }
        }
        #endregion
    }
}
