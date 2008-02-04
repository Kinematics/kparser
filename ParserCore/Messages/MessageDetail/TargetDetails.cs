using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser
{
    internal class TargetDetails
    {
        #region Member Variables
        internal string Name = string.Empty;
        internal EntityType EntityType;

        internal bool Defended;
        internal DefenseType DefenseType;
        internal byte ShadowsUsed;

        internal SuccessType SuccessLevel;

        // Amount of effect (damage, HP healed, whatever)
        internal RecoveryType RecoveryType;
        internal int Amount;
        internal DamageModifier DamageModifier;

        internal bool AdditionalEffect;
        internal uint AdditionalDamage;
        #endregion

        #region Constructor
        internal TargetDetails(string newTargetName)
        {
            Name = newTargetName;
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

            sb.AppendFormat("    Target Name: {0}\n", Name);
            sb.AppendFormat("    Entity Type: {0}\n", EntityType);
            sb.AppendFormat("    Success Level: {0}\n", SuccessLevel);
            sb.AppendFormat("    Defended: {0}\n", Defended);
            sb.AppendFormat("    Defense Type: {0}\n", DefenseType);
            sb.AppendFormat("    Shadows Used: {0}\n", ShadowsUsed);
            sb.AppendFormat("    Recovery Type: {0}\n", RecoveryType);
            sb.AppendFormat("    Damage Modifier: {0}\n", DamageModifier);
            sb.AppendFormat("    Amount: {0}\n", Amount);
            sb.AppendFormat("    Additional Effect: {0}\n", AdditionalEffect);
            sb.AppendFormat("    Additional Damage: {0}\n", AdditionalDamage);

            return sb.ToString();
        }
        #endregion

    }
}
