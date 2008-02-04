using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser
{
    internal class CombatDetails
    {
        #region State
        internal string ActorName = string.Empty;
        internal EntityType ActorEntityType {get; set;}

        internal string ActionName = string.Empty;
        internal bool IsPreparing { get; set; }

        internal CombatActionType CombatCategory { get; set; }
        internal AttackType AttackType { get; set; }
        internal BuffType BuffType { get; set; }
        internal ActionSourceType ActionSource { get; set; }

        internal SuccessType SuccessLevel { get; set; }

        internal FailedActionType FailedActionType { get; set; }

        internal TargetDetailCollection Targets = new TargetDetailCollection();

        internal bool IsCrit { get; set; }

        internal int Experience { get; set; }
        internal short ExperienceChain { get; set; }
        #endregion



        #region Overrides
        /// <summary>
        /// Override for debugging output.
        /// </summary>
        /// <returns>String containing all details of this object.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("  Actor Name: {0}\n", ActorName);
            sb.AppendFormat("  Entity Type: {0}\n", ActorEntityType);
            sb.AppendFormat("  Success Level: {0}\n", SuccessLevel);
            sb.AppendFormat("  Action Name: {0}\n", ActionName);
            sb.AppendFormat("  IsPreparing: {0}\n", IsPreparing);
            sb.AppendFormat("  Combat Category: {0}\n", CombatCategory);
            sb.AppendFormat("  Attack Type: {0}\n", AttackType);
            sb.AppendFormat("  Buff Type: {0}\n", BuffType);
            sb.AppendFormat("  Action Source: {0}\n", ActionSource);
            sb.AppendFormat("  Failed Action Type: {0}\n", FailedActionType);
            sb.AppendFormat("  Is Crit: {0}\n", IsCrit);

            foreach (TargetDetails target in Targets)
                sb.AppendFormat("  Target Details:\n{0}", target.ToString());

            return sb.ToString();
        }
        #endregion
    }
}
