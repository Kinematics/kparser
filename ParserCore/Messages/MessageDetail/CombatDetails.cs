using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WaywardGamers.KParser
{
    /// <summary>
    /// Class to maintain details about an actor/target interaction
    /// as it relates to the actor.
    /// </summary>
    internal class CombatDetails
    {
        #region member Variables
        string actorName = string.Empty;
        string actionName = string.Empty;
        string itemName = string.Empty;
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new instance of CombatDetails.
        /// </summary>
        internal CombatDetails()
        {
            Targets = new List<TargetDetails>();
            ShortActorName = string.Empty;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets and sets the type of actor/target interaction.
        /// </summary>
        internal InteractionType InteractionType { get; set; }

        /// <summary>
        /// Gets and sets the Actor name.  When set, the actor entity type
        /// is automatically determined.
        /// </summary>
        internal string ActorName
        {
            get
            {
                return ShortActorName;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("ActorName");

                if (value == string.Empty)
                    throw new ArgumentOutOfRangeException("ActorName", "Cannot create an actor with an empty name.");

                actorName = value;

                if (actorName.StartsWith("The ") || actorName.StartsWith("the "))
                    ShortActorName = actorName.Substring(4);
                else
                    ShortActorName = actorName;

                ActorEntityType = Parsing.ClassifyEntity.Classify(actorName);
            }
        }

        /// <summary>
        /// Gets the full, unmodified actor name.
        /// </summary>
        internal string FullActorName
        {
            get
            {
                return actorName;
            }
        }

        /// <summary>
        /// Gets and sets the short (most commonly used) actor name.
        /// Internal use only; it gets passed through to ActorName.
        /// </summary>
        private string ShortActorName { get; set; }

        /// <summary>
        /// Gets whether the actor name has been set to anything valid.
        /// </summary>
        internal bool HasActor
        {
            get { return (actorName != string.Empty); }
        }

        /// <summary>
        /// Gets and sets the actor's entity type.
        /// </summary>
        internal EntityType ActorEntityType { get; set; }

        /// <summary>
        /// Gets and sets the actor type (self/party/other/etc)
        /// </summary>
        internal ActorType ActorType { get; set; }

        /// <summary>
        /// Gets the list of targets that this interaction encompasses.
        /// </summary>
        internal List<TargetDetails> Targets { get; private set; }


        /// <summary>
        /// Gets and sets the failed action type of this interaction.
        /// </summary>
        internal FailedActionType FailedActionType { get; set; }

        /// <summary>
        /// Gets and sets the aid type of this interaction.
        /// </summary>
        internal AidType AidType { get; set; }

        /// <summary>
        /// Gets and sets the harm type of this interaction.
        /// </summary>
        internal HarmType HarmType { get; set; }


        /// <summary>
        /// Gets and sets the type of action performed for this interaction.
        /// </summary>
        internal ActionType ActionType { get; set; }

        /// <summary>
        /// Gets and sets whether this interaction is for the preparing phase
        /// of a spell or ability.
        /// </summary>
        internal bool IsPreparing { get; set; }

        /// <summary>
        /// Gets and sets whether an additional effect is present in this message.
        /// </summary>
        internal bool HasAdditionalEffect { get; set; }

        /// <summary>
        /// Gets and sets the name of the action being used.
        /// </summary>
        internal string ActionName
        {
            get { return actionName; }
            set { if (value != null) actionName = value; }
        }

        /// <summary>
        /// Gets and sets the name of the item being used.
        /// </summary>
        internal string ItemName
        {
            get { return itemName; }
            set { if (value != null) itemName = value; }
        }

        /// <summary>
        /// Gets and sets the success level of this interaction.  Used for
        /// state tracking.
        /// </summary>
        internal SuccessType SuccessLevel { get; set; }

        /// <summary>
        /// Gets and sets whether an attack was a critical.  Used for
        /// state tracking.
        /// </summary>
        internal bool FlagCrit { get; set; }

        /// <summary>
        /// Gets and sets whether an attack was a covered.  Used for
        /// state tracking.
        /// </summary>
        internal bool FlagCover { get; set; }

        /// <summary>
        /// Gets and sets whether this message should be held pending
        /// info about whether the death is of a pet or a mob.
        /// </summary>
        internal bool FlagPetDeath { get; set; }

        /// <summary>
        /// Gets and sets the value rolled by a Corsair's Phantom Roll.
        /// State tracking to be applied to targets.
        /// </summary>
        internal int CorsairRoll { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Add a new target to the target list for this interaction.
        /// </summary>
        /// <param name="targetName">The name of the target to add.</param>
        /// <returns>Returns a reference to the TargetDetails for the target.</returns>
        internal TargetDetails AddTarget(string targetName)
        {
            TargetDetails newTarget;

            newTarget = new TargetDetails(targetName);
            Targets.Add(newTarget);

            return newTarget;
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
            sb.AppendFormat("    Interaction Type: {0}\n", InteractionType);
            sb.AppendFormat("    Actor Name: {0}\n", ActorName);
            sb.AppendFormat("    Entity Type: {0}\n", ActorEntityType);
            sb.AppendFormat("    Failed Action Type: {0}\n", FailedActionType);
            sb.AppendFormat("    Aid Type: {0}\n", AidType);
            sb.AppendFormat("    Harm Type: {0}\n", HarmType);
            sb.AppendFormat("    Action Type: {0}\n", ActionType);
            sb.AppendFormat("    IsPreparing: {0}\n", IsPreparing);
            sb.AppendFormat("    HasAdditionalEffect: {0}\n", HasAdditionalEffect);
            sb.AppendFormat("    Action Name: {0}\n", ActionName);
            sb.AppendFormat("    Item Name: {0}\n", ItemName);
            sb.AppendFormat("    Success Level: {0}\n", SuccessLevel);
            sb.AppendFormat("    Is Crit: {0}\n", FlagCrit);
            sb.AppendFormat("    Is Covered: {0}\n", FlagCover);

            foreach (TargetDetails target in Targets)
                sb.Append(target.ToString());

            return sb.ToString();
        }
        #endregion

    }
}
