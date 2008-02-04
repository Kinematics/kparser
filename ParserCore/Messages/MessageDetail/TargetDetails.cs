using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WaywardGamers.KParser
{
    internal class NewTargetDetails
    {
        #region Member Variables
        string targetName = string.Empty;
        #endregion

        #region Constructor
        internal NewTargetDetails(string newTargetName)
        {
            if (newTargetName == null)
                throw new ArgumentNullException("newTargetName");

            Name = newTargetName;
        }
        #endregion

        #region Properties
        internal string FullName
        {
            get { return targetName; }
        }

        internal string Name
        {
            get
            {
                if (targetName.StartsWith("The ") || targetName.StartsWith("the "))
                {
                    return targetName.Substring(4);
                }

                return targetName;
            }
            set
            {
                if (value != null)
                {
                    targetName = value;
                    EntityType = Parsing.ClassifyEntity.Classify(targetName);
                }
            }
        }

        internal EntityType EntityType { get; set; }

        internal SuccessType SuccessLevel { get; set; }

        internal bool Defended { get; set; }

        internal DefenseType DefenseType { get; set; }


        internal RecoveryType RecoveryType { get; set; }

        internal int RecoveryAmount { get; set; }

        internal byte ShadowsUsed { get; set; }

        
        internal bool AdditionalEffect { get; set; }

        internal int Damage { get; set; }

        internal int AdditionalDamage { get; set; }

        internal DamageModifier DamageModifier { get; set; }

        #endregion

        #region Overrides
        /// <summary>
        /// Override for debugging output.
        /// </summary>
        /// <returns>String containing all details of this object.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("    Target Details:\n");
            sb.AppendFormat("      Target Name: {0}\n", Name);
            sb.AppendFormat("      Entity Type: {0}\n", EntityType);
            sb.AppendFormat("      Success Level: {0}\n", SuccessLevel);
            sb.AppendFormat("      Defended: {0}\n", Defended);
            sb.AppendFormat("      Defense Type: {0}\n", DefenseType);
            sb.AppendFormat("      Recovery Type: {0}\n", RecoveryType);
            sb.AppendFormat("      Recovery Amount: {0}\n", RecoveryAmount);
            sb.AppendFormat("      Shadows Used: {0}\n", ShadowsUsed);
            sb.AppendFormat("      Damage: {0}\n", Damage);
            sb.AppendFormat("      Damage Modifier: {0}\n", DamageModifier);
            sb.AppendFormat("      Additional Effect: {0}\n", AdditionalEffect);
            sb.AppendFormat("      Additional Damage: {0}\n", AdditionalDamage);

            return sb.ToString();
        }
        #endregion

    }
}
