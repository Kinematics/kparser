using System;
using System.Text;
using System.Text.RegularExpressions;

namespace WaywardGamers.KParser.Parsing
{
    /// <summary>
    /// Class to handle generic determination of entity types by name.
    /// </summary>
    internal static class ClassifyEntity
    {
        static Random random = new Random();

        /// <summary>
        /// Gets the entity type of the specified name based on naming rules.
        /// </summary>
        /// <param name="entityName">The name of the entity to analyze.</param>
        /// <returns>The determined entity type.</returns>
        internal static EntityType ClassifyByName(string entityName)
        {
            if ((entityName == null) || (entityName == string.Empty))
            {
                return EntityType.Unknown;
            }

            // Check if we already have this name in the entity list.  If so, use that.
            EntityType entityType = MessageManager.Instance.LookupEntity(entityName);
            if (entityType != EntityType.Unknown)
                return entityType;

            // If the entityName starts with 'the' it's a mob, though possibly a charmed one.
            if (entityName.StartsWith("The ") || entityName.StartsWith("the "))
            {
                return EntityType.Mob;
            }

            // If the entityName starts with 'SC: ', it's a skillchain.  Mark it as such.
            if (entityName.StartsWith("SC: "))
                return EntityType.Skillchain;

            // Check for characters that only show up in mob names and some sublists.
            Match entityNameMatch = ParseExpressions.MobNameTest.Match(entityName);

            if (entityNameMatch.Success == true)
            {
                // Probably a mob, but possibly a pet.  Check the short names lists.
                if (Puppets.ShortNamesList.Contains(entityName))
                    return EntityType.Pet;
                else if (Wyverns.ShortNamesList.Contains(entityName))
                    return EntityType.Pet;
                else if (NPCFellows.ShortNamesList.Contains(entityName))
                    return EntityType.Fellow;
                else
                    return EntityType.Mob;
            }

            // Check for the pattern of beastmaster jug pet entityNames.
            entityNameMatch = ParseExpressions.BstJugPetName.Match(entityName);
            if (entityNameMatch.Success == true)
            {
                return EntityType.Pet;
            }

            // Check known pet lists
            if (Avatars.NamesList.Contains(entityName))
            {
                return EntityType.Pet;
            }

            if (Wyverns.NamesList.Contains(entityName))
            {
                return EntityType.Pet;
            }

            if (Puppets.NamesList.Contains(entityName))
            {
                return EntityType.Pet;
            }

            // Check known NPC fellows
            if (NPCFellows.NamesList.Contains(entityName))
            {
                return EntityType.Fellow;
            }


            // Anything else must be a player.
            return EntityType.Player;
        }


        internal static void DetermineCharmedPet(ref CombatDetails combatDetails, ref TargetDetails target, bool death)
        {
            if (combatDetails == null)
                throw new ArgumentNullException("combatDetails");

            if (target == null)
                throw new ArgumentNullException("target");

            // Handle identifying charmed pet entities
            if (target.EntityType == EntityType.Mob && combatDetails.ActorEntityType == EntityType.Mob)
            {
                EntityType checkTarg = MessageManager.Instance.LookupPetEntity(target.Name);
                EntityType checkActor = MessageManager.Instance.LookupPetEntity(combatDetails.ActorName);

                if ((checkActor == EntityType.Pet) ^ (checkTarg == EntityType.Pet))
                {
                    // If only one shows up as being a pet, use that.
                    if (checkActor == EntityType.Pet)
                        combatDetails.ActorEntityType = checkActor;
                    else
                        target.EntityType = checkTarg;
                }
                else if ((checkActor == EntityType.Pet) && (checkTarg == EntityType.Pet))
                {
                    // If both show up as being a pet, check the last charmed mob
                    // to break the tie.
                    string lastPetName = MessageManager.Instance.LastAddedPetEntity;

                    if (lastPetName != string.Empty)
                    {
                        // Check to make sure we don't have dhalmel fighting dhalmel, or whatever.
                        if (target.Name != combatDetails.ActorName)
                        {
                            if (target.Name == lastPetName)
                            {
                                target.EntityType = EntityType.Pet;
                            }
                            else if (combatDetails.ActorName == lastPetName)
                            {
                                combatDetails.ActorEntityType = EntityType.Pet;
                            }
                        }
                        else
                        {
                            // actor and target are same mob type; cannot accurately determine
                            // which is the mob and which is the pet; random?
                            if (death == true)
                            {
                                combatDetails.FlagPetDeath = true;
                            }
                            else
                            {
                                if (random.Next(1000) < 500)
                                {
                                    target.EntityType = EntityType.Pet;
                                }
                                else
                                {
                                    combatDetails.ActorEntityType = EntityType.Pet;
                                }
                            }
                        }
                    }
                    else
                    {
                        // if no last pet entry, ignore
                    }
                }
            }
        }

        internal static void VerifyEntities(ref Message message, ref TargetDetails target, bool death)
        {
            if (message == null)
                throw new ArgumentNullException("message");
            if (message.EventDetails == null)
                throw new ArgumentNullException("message.EventDetails");
            if (message.EventDetails.CombatDetails == null)
                throw new ArgumentNullException("message.EventDetails.CombatDetails");
            if (target == null)
                throw new ArgumentNullException("target");

            uint msgCode = message.MessageCode;
            CombatDetails combatDetails = message.EventDetails.CombatDetails;

            // If both entities are players, verify this isn't a combat message
            // where one should be a mob.  If it is, the mob is probably a
            // misclassified Notorius Monster.  Adjust as needed.
            if (combatDetails.ActorEntityType == EntityType.Player &&
                target.EntityType == EntityType.Player)
            {
                EntityType checkActor = ParseCodes.Instance.GetActorEntityType(msgCode);
                if (checkActor == EntityType.Mob)
                {
                    combatDetails.ActorEntityType = EntityType.Mob;
                    MessageManager.Instance.OverridePlayerToMob(combatDetails.ActorName);
                    return;
                }

                EntityType checkTarget = ParseCodes.Instance.GetTargetEntityType(msgCode);
                if (checkTarget == EntityType.Mob)
                {
                    target.EntityType = EntityType.Mob;
                    MessageManager.Instance.OverridePlayerToMob(target.Name);
                    return;
                }

                return;
            }

            // If both entities are mobs, run the check for charmed pets.
            if (combatDetails.ActorEntityType == EntityType.Mob &&
                target.EntityType == EntityType.Mob)
            {
                DetermineCharmedPet(ref combatDetails, ref target, death);
            }
        }
    }
}
