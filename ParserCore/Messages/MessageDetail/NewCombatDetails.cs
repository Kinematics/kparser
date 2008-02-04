using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WaywardGamers.KParser
{
    internal class NewCombatDetails
    {
        #region member Variables
        string actorName = string.Empty;
        string actionName = string.Empty;
        string additionalEffectName = string.Empty;
        #endregion

        #region Constructor
        internal NewCombatDetails()
        {
            Targets = new List<NewTargetDetails>();
        }
        #endregion

        #region Properties
        internal CombatActionType CombatCategory { get; set; }

        internal BuffType BuffType { get; set; }

        internal AttackType AttackType { get; set; }

        internal FailedActionType FailedActionType { get; set; }

        
        internal string FullActorName
        {
            get
            {
                return actorName;
            }
        }

        internal string ActorName
        {
            get
            {
                if (actorName != string.Empty)
                {
                    if (actorName.StartsWith("The ") || actorName.StartsWith("the "))
                    {
                        return actorName.Substring(4);
                    }
                }

                return actorName;
            }
            set
            {
                if (value != null)
                {
                    actorName = value;
                    DetermineEntityType();
                }
            }
        }

        internal bool HasActor
        {
            get { return (actorName != string.Empty); }
        }

        internal EntityType ActorEntityType { get; set; }


        internal List<NewTargetDetails> Targets { get; private set; }

        internal NewTargetDetails CurrentTarget { get; set; }


        internal bool IsPreparing { get; set; }

        internal string ActionName
        {
            get { return actionName; }
            set { if (value != null) actionName = value; }
        }

        internal ActionSourceType ActionSource { get; set; }

        internal SuccessType SuccessLevel { get; set; }

        internal bool FlagCrit { get; set; }

        #endregion

        #region Methods
        internal NewTargetDetails AddTarget(string targetName)
        {
            CurrentTarget = new NewTargetDetails(targetName);
            Targets.Add(CurrentTarget);
            return CurrentTarget;
        }

        private void DetermineEntityType()
        {
            if ((actorName == null) || (actorName == string.Empty))
            {
                ActorEntityType = EntityType.Unknown;
                return;
            }

            // Check if we already have this name in the entity list.  If so, use that.
            ActorEntityType = MessageManager.Instance.LookupEntity(ActorName);
            if (ActorEntityType != EntityType.Unknown)
                return;

            // If the actorName starts with 'the' it's a mob, though possibly a charmed one.
            if (actorName.StartsWith("The") || actorName.StartsWith("the"))
            {
                ActorEntityType = EntityType.Mob;
                return;
            }

            // Check for characters that only show up in mob names and some sublists.
            Match actorNameMatch = ParseExpressions.MobNameTest.Match(actorName);

            if (actorNameMatch.Success == true)
            {
                // Probably a mob, but possibly a pet.  Check the short names lists.
                if (Puppets.ShortNamesList.Contains(actorName))
                    ActorEntityType = EntityType.Pet;
                else if (Wyverns.ShortNamesList.Contains(actorName))
                    ActorEntityType = EntityType.Pet;
                else if (NPCFellows.ShortNamesList.Contains(actorName))
                    ActorEntityType = EntityType.Fellow;
                else
                    ActorEntityType = EntityType.Mob;

                return;
            }

            // Check for the pattern of beastmaster jug pet actorNames.
            actorNameMatch = ParseExpressions.BstJugPetName.Match(actorName);
            if (actorNameMatch.Success == true)
            {
                ActorEntityType = EntityType.Pet;
                return;
            }

            // Check known pet lists
            if (Avatars.NamesList.Contains(actorName))
            {
                ActorEntityType = EntityType.Pet;
                return;
            }

            if (Wyverns.NamesList.Contains(actorName))
            {
                ActorEntityType = EntityType.Pet;
                return;
            }

            if (Puppets.NamesList.Contains(actorName))
            {
                ActorEntityType = EntityType.Pet;
                return;
            }

            // Check known NPC fellows
            if (NPCFellows.NamesList.Contains(actorName))
            {
                ActorEntityType = EntityType.Fellow;
                return;
            }


            // Anything else must be a player.
            ActorEntityType = EntityType.Player;
        }
        #endregion

        #region Overrides
        /// <summary>
        /// Override for debugging output.
        /// </summary>
        /// <returns>String containing all details of this object.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("  Combat Details:\n");
            sb.AppendFormat("    Actor Name: {0}\n", ActorName);
            sb.AppendFormat("    Entity Type: {0}\n", ActorEntityType);
            sb.AppendFormat("    Success Level: {0}\n", SuccessLevel);
            sb.AppendFormat("    Action Name: {0}\n", ActionName);
            sb.AppendFormat("    IsPreparing: {0}\n", IsPreparing);
            sb.AppendFormat("    Combat Category: {0}\n", CombatCategory);
            sb.AppendFormat("    Attack Type: {0}\n", AttackType);
            sb.AppendFormat("    Buff Type: {0}\n", BuffType);
            sb.AppendFormat("    Action Source: {0}\n", ActionSource);
            sb.AppendFormat("    Failed Action Type: {0}\n", FailedActionType);
            sb.AppendFormat("    Is Crit: {0}\n", FlagCrit);

            foreach (NewTargetDetails target in Targets)
                sb.Append(target.ToString());

            return sb.ToString();
        }
        #endregion

    }
}
