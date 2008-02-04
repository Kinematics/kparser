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
        /// <summary>
        /// Gets the entity type of the specified name based on naming rules.
        /// </summary>
        /// <param name="entityName">The name of the entity to analyze.</param>
        /// <returns>The determined entity type.</returns>
        internal static EntityType Classify(string entityName)
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
    }
}
