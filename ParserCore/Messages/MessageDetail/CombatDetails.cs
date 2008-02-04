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
                if (actorName.StartsWith("The ") || actorName.StartsWith("the "))
                {
                    return actorName.Substring(4);
                }

                return actorName;
            }
            set
            {
                if (value != null)
                {
                    actorName = value;
                    ActorEntityType = Parsing.ClassifyEntity.Classify(actorName);
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
