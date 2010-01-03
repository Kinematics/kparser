using System;
using System.Text;
using System.Collections.Generic;
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
            // Redirect to the alternate code
            return ClassifyByNameAndLookup(entityName);

            //return GetNameClassification(entityName);
        }

        /// <summary>
        /// Gets the entity type of the specified name based on naming rules.
        /// If the entity name aleady exists in the EntityManager's lists,
        /// use that instead.
        /// </summary>
        /// <param name="entityName">The name of the entity to analyze.</param>
        /// <returns>The determined entity type.</returns>
        internal static EntityType ClassifyByNameAndLookup(string entityName)
        {
            if ((entityName == null) || (entityName == string.Empty))
            {
                return EntityType.Unknown;
            }

            // Check if we already have this name in the entity list.  If so, use that.
            List<EntityType> entityTypeList = EntityManager.Instance.LookupEntity(entityName);

            // If we only have one entry, return that
            if (entityTypeList.Count == 1)
            {
                return entityTypeList[0];
            }

            // If there are multiple entries in the entity manager's list (ie: normal + charmed),
            // we cannot determine the entity type by name alone.
            if (entityTypeList.Count > 1)
            {
                return EntityType.Unknown;
            }

            return GetNameClassification(entityName);
        }

        /// <summary>
        /// Extract name classification rules from above functions so that
        /// they both can reference the same rule set.
        /// </summary>
        /// <param name="entityName">The name of the entity to classify.</param>
        /// <returns>Returns the type of entity named.</returns>
        private static EntityType GetNameClassification(string entityName)
        {
            if ((entityName == null) || (entityName == string.Empty))
            {
                return EntityType.Unknown;
            }

            // If the entityName starts with 'the' it's a mob, though possibly a charmed one.
            if (entityName.StartsWith("The ", StringComparison.InvariantCultureIgnoreCase))
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

            // Technically we can't be certain at this point whether we have a player's
            // name or a 'special' NM (eg: Genbu) that is otherwise indistinguishable
            // from standard player names (no "the" prefix).  Return Unknown and
            // disambiguate the name under VerifyEntities.
            return EntityType.Unknown;

            // Anything else must be a player.
            //return EntityType.Player;
        }

        /// <summary>
        /// Verify all entities in the message.  Forced to do this if called from
        /// a point where we don't know what the last target was.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="death"></param>
        internal static void VerifyAllEntities(ref Message message, bool death)
        {
            foreach (var target in message.EventDetails.CombatDetails.Targets)
            {
                VerifyEntities(ref message, target, death);
            }
        }

        /// <summary>
        /// Verify a specific pair of entities involved in a message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="target"></param>
        /// <param name="death"></param>
        internal static void VerifyEntities(ref Message message, TargetDetails target, bool death)
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

            EntityType supposedActorType = ParseCodes.Instance.GetActorEntityType(message.MessageCode);
            EntityType supposedTargetType = ParseCodes.Instance.GetTargetEntityType(message.MessageCode);

            // Disambiguate Unknowns based on known message code values.

            if (combatDetails.ActorEntityType == EntityType.Unknown)
            {
                combatDetails.ActorEntityType = supposedActorType;
            }

            if (target.EntityType == EntityType.Unknown)
            {
                target.EntityType = supposedTargetType;
            }



            // If the message is of type Aid, both entities should be of the same type.

            if (combatDetails.InteractionType == InteractionType.Aid)
            {
                // If one or the other (but not both) are type mob, one of them
                // must be charmed.  Since I don't believe it's possible for a mob
                // to aid a charmed player, and a charmed player cannot do any actions
                // except melee, if one of these is a mob, it must be charmed.
                if ((combatDetails.ActorEntityType == EntityType.Mob) ^
                    (target.EntityType == EntityType.Mob))
                {
                    // However exclude instances of a skillchain as an actor in
                    // cases of skillchains healing mobs (eg: elementals, puks, etc).
                    if (combatDetails.ActorEntityType != EntityType.Skillchain)
                    {
                        if (combatDetails.ActorEntityType == EntityType.Mob)
                            combatDetails.ActorEntityType = EntityType.CharmedMob;
                        else if (target.EntityType == EntityType.Mob)
                            target.EntityType = EntityType.CharmedMob;
                    }
                }

                if (combatDetails.ActorEntityType == EntityType.Unknown)
                {
                    if (target.EntityType == EntityType.Player)
                    {
                        combatDetails.ActorEntityType = EntityType.Player;
                    }
                    else if (target.EntityType == EntityType.Unknown)
                    {
                        if ((combatDetails.ActorPlayerType == ActorPlayerType.Self) ||
                            (combatDetails.ActorPlayerType == ActorPlayerType.Party) ||
                            (combatDetails.ActorPlayerType == ActorPlayerType.Alliance))
                        {
                            combatDetails.ActorEntityType = EntityType.Player;
                            target.EntityType = EntityType.Player;
                        }
                    }
                }
                else if (target.EntityType == EntityType.Unknown)
                {
                    if (combatDetails.ActorEntityType != EntityType.Mob)
                    {
                        target.EntityType = EntityType.Player;
                    }
                }


                return;
            }

            // If the message is of type Harm, the entities should be of different types.
            // Also handle unknown interaction types here.

            if (combatDetails.InteractionType == InteractionType.Harm)
            {
                // If both entities are mobs, one of them must be charmed.
                if ((combatDetails.ActorEntityType == EntityType.Mob) &&
                    (target.EntityType == EntityType.Mob))
                {
                    // If they have different names, see if we have one registered
                    // as the most recently charmed mob.
                    if (combatDetails.ActorName != target.Name)
                    {
                        if (combatDetails.ActorName == EntityManager.Instance.LastCharmedMob)
                        {
                            combatDetails.ActorEntityType = EntityType.CharmedMob;
                            return;
                        }

                        if (target.Name == EntityManager.Instance.LastCharmedMob)
                        {
                            target.EntityType = EntityType.CharmedMob;
                            return;
                        }

                        // If neither are registered as the most recent, see if either
                        // (but not both) have been registered as charmed.
                        EntityType actorCharmEntity = EntityManager.Instance.LookupCharmedEntity(combatDetails.ActorName);
                        EntityType targetCharmEntity = EntityManager.Instance.LookupCharmedEntity(target.Name);

                        if ((actorCharmEntity == EntityType.CharmedMob) ^ (targetCharmEntity == EntityType.CharmedMob))
                        {
                            if (actorCharmEntity == EntityType.CharmedMob)
                            {
                                combatDetails.ActorEntityType = EntityType.CharmedMob;
                                return;
                            }

                            if (targetCharmEntity == EntityType.CharmedMob)
                            {
                                target.EntityType = EntityType.CharmedMob;
                                return;
                            }
                        }

                    }

                    // At this point, either the mobs have the same name, or both mobs are
                    // listed in our entity list as possibly charmed and neither one is our
                    // 'last charmed' mob.  Cannot accurately determine which is which, so random.

                    if (death == true)
                    {
                        // If it's a death, defer resolution until we know if we
                        // get experience.
                        combatDetails.FlagPetDeath = true;
                    }
                    else
                    {
                        // Otherwise just random it.
                        if (random.Next(1000) < 500)
                            target.EntityType = EntityType.CharmedMob;
                        else
                            combatDetails.ActorEntityType = EntityType.CharmedMob;
                    }
                }

                // If both entities are players, one of them must be charmed,
                // or one of them is an NM.
                // Charmed players will not attack NPCs/Pets/etc.
                if ((combatDetails.ActorEntityType == EntityType.Player) &&
                    (target.EntityType == EntityType.Player))
                {
                    // Charmed players can only melee.  If this is anything aside from
                    // melee, one of the entities must be an NM
                    if (combatDetails.ActionType != ActionType.Melee)
                    {
                        // This is probably an NM.  Set the entity type based on the message code.
                        if (ParseCodes.Instance.GetActorEntityType(msgCode) == EntityType.Mob)
                        {
                            combatDetails.ActorEntityType = EntityType.Mob;
                            return;
                        }
                        else if (ParseCodes.Instance.GetTargetEntityType(msgCode) == EntityType.Mob)
                        {
                            target.EntityType = EntityType.Mob;
                            return;
                        }

                        // This could be the result of a busted Corsair roll.  If so, leave as is.
                        if ((string.IsNullOrEmpty(target.EffectName) == false) &&
                            (JobAbilities.CorRolls.Contains(target.EffectName) == true))
                            return;

                        // Check if either combatant is in the entity table already.
                        // If not, then a player is likely initiating action against an NM.
                        var actorEntityList = EntityManager.Instance.LookupEntity(combatDetails.ActorName);
                        var targetEntityList = EntityManager.Instance.LookupEntity(target.Name);

                        if (targetEntityList.Count == 0)
                        {
                            target.EntityType = EntityType.Mob;
                            return;
                        }

                        // If we have not yet determined the entity type, it's probably an NM.
                        if (targetEntityList.Contains(EntityType.Unknown))
                        {
                            target.EntityType = EntityType.Mob;
                            return;
                        }

                        // If the target is already known (ie: they're present in the requested
                        // entity list above) and the actor is unknown, then presumably the
                        // NM is taking action against the player.
                        if (actorEntityList.Count == 0)
                        {
                            combatDetails.ActorEntityType = EntityType.Mob;
                            return;
                        }

                        // If we have not yet determined the entity type, it's probably an NM.
                        if (actorEntityList.Contains(EntityType.Unknown))
                        {
                            combatDetails.ActorEntityType = EntityType.Mob;
                            return;
                        }

                        // Otherwise it's likely spells/etc cast on charmed players
                        target.EntityType = EntityType.CharmedPlayer;
                        return;
                    }

                    // If we get here, this is either melee, or we're not sure of the
                    // defined entity types.  Try looking up details from the entity manager first.
                    EntityType actorCharmEntity = EntityManager.Instance.LookupCharmedEntity(combatDetails.ActorName);
                    if (actorCharmEntity == EntityType.CharmedPlayer)
                    {
                        combatDetails.ActorEntityType = EntityType.CharmedPlayer;
                        return;
                    }

                    EntityType targetCharmEntity = EntityManager.Instance.LookupCharmedEntity(target.Name);
                    if (targetCharmEntity == EntityType.CharmedPlayer)
                    {
                        target.EntityType = EntityType.CharmedPlayer;
                        return;
                    }

                    // If we don't get anything from the entity manager, assume the
                    // aggressive melee'er is the charmed one.
                    combatDetails.ActorEntityType = EntityType.CharmedPlayer;
                    return;
                }

                // If a pet is attacking a player, that player must be charmed.
                if ((combatDetails.ActorEntityType == EntityType.Pet) &&
                    (target.EntityType == EntityType.Player))
                {
                    target.EntityType = EntityType.CharmedPlayer;
                    return;
                }

            }

            if (combatDetails.InteractionType == InteractionType.Death)
            {
                // If a mob killed a pet, we'd know the entity type of the target.
                // As such, if there's an unknown target entity type, it must be a player.
                if ((combatDetails.ActorEntityType == EntityType.Mob) &&
                    (target.EntityType == EntityType.Unknown))
                {
                    target.EntityType = EntityType.Player;
                }
            }
        }
    }
}
