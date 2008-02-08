using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WaywardGamers.KParser
{
    internal class TargetDetails
    {
        #region Member Variables
        string targetName = string.Empty;
        #endregion

        #region Constructor
        internal TargetDetails(string newTargetName)
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


        internal FailedActionType FailedActionType { get; set; }

        internal DefenseType DefenseType { get; set; }

        internal byte ShadowsUsed { get; set; }

        internal AidType AidType { get; set; }
        internal RecoveryType RecoveryType { get; set; }
        internal HarmType HarmType { get; set; }

        internal int Amount { get; set; }
        internal DamageModifier DamageModifier { get; set; }


        internal AidType SecondaryAidType { get; set; }
        internal RecoveryType SecondaryRecoveryType { get; set; }
        internal HarmType SecondaryHarmType { get; set; }

        internal int SecondaryAmount { get; set; }

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
            sb.AppendFormat("      Failed Action Type: {0}\n", FailedActionType);
            sb.AppendFormat("      Defense Type: {0}\n", DefenseType);
            sb.AppendFormat("      Recovery Type: {0}\n", RecoveryType);
            //sb.AppendFormat("      Recovery Amount: {0}\n", RecoveryAmount);
            //sb.AppendFormat("      Shadows Used: {0}\n", ShadowsUsed);
            //sb.AppendFormat("      Damage: {0}\n", Damage);
            //sb.AppendFormat("      Damage Modifier: {0}\n", DamageModifier);
            //sb.AppendFormat("      Additional Effect: {0}\n", AdditionalEffect);
            //sb.AppendFormat("      Additional Damage: {0}\n", AdditionalDamage);

            return sb.ToString();
        }
        #endregion

    }
}
