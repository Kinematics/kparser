using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser
{
    internal class ExperienceDetails
    {
        #region Member Variables
        // experience type? (ie: xp or limit points)
        #endregion

        #region Properties
        internal int ExperiencePoints { get; set; }
        internal int ExperienceChain { get; set; }
        internal string ExperienceRecipient { get; set; }
        #endregion

        #region Overrides
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("  Experience Details:\n");

            sb.AppendFormat("    Experience Points: {0}\n", ExperiencePoints);
            sb.AppendFormat("    Experience Chain: {0}\n", ExperienceChain);

            return sb.ToString();
        }
        #endregion

    }
}
