using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WaywardGamers.KParser
{
    /// <summary>
    /// Class to maintain details about an actor/target interaction
    /// as it relates to the target.
    /// </summary>
    internal class TargetDetails
    {
        #region Member Variables
        readonly string targetName;
        #endregion

        #region Constructor
        /// <summary>
        /// Create a new instance of TargetDetails.
        /// </summary>
        /// <param name="newTargetName">The name of the target to initialize the class with.</param>
        internal TargetDetails(string newTargetName)
        {
            if (newTargetName == null)
                throw new ArgumentNullException("newTargetName");

            if (newTargetName == string.Empty)
                throw new ArgumentOutOfRangeException("newTargetName", "Cannot create a target with an empty name.");


            targetName = newTargetName;

            if (targetName.StartsWith("The ") || targetName.StartsWith("the "))
                Name = targetName.Substring(4);
            else
                Name = targetName;

            EntityType = Parsing.ClassifyEntity.Classify(targetName);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the unmodified target name.
        /// </summary>
        internal string FullName
        {
            get { return targetName; }
        }

        /// <summary>
        /// Gets the short (most commonly used) version of the target name.
        /// Set is only done during construction.
        /// </summary>
        internal string Name { get; private set; }

        /// <summary>
        /// Gets and sets the entity type of the target.
        /// </summary>
        internal EntityType EntityType { get; set; }

        /// <summary>
        /// Gets and sets the failed action type of the interaction with the target.
        /// </summary>
        internal FailedActionType FailedActionType { get; set; }

        /// <summary>
        /// Gets and sets the defense type of the interaction with the target.
        /// </summary>
        internal DefenseType DefenseType { get; set; }

        /// <summary>
        /// Gets and sets the number of shadows used in the interaction
        /// with the target (only applicable if DefenseType is Blink).
        /// </summary>
        internal byte ShadowsUsed { get; set; }

        /// <summary>
        /// Gets and sets the type of aid being applied to the target.
        /// </summary>
        internal AidType AidType { get; set; }

        /// <summary>
        /// Gets and sets the type of recovery (HP or MP) applied
        /// to the target if AidType is Recovery.
        /// </summary>
        internal RecoveryType RecoveryType { get; set; }

        /// <summary>
        /// Gets and sets the harm type being applied to the target.
        /// </summary>
        internal HarmType HarmType { get; set; }

        /// <summary>
        /// Gets and sets the quantity of the Aid/Harm effect.
        /// </summary>
        internal int Amount { get; set; }

        /// <summary>
        /// Gets and sets the modifier flag that can be applied
        /// to damage done to the target (ie: crit or magic burst).
        /// </summary>
        internal DamageModifier DamageModifier { get; set; }

        /// <summary>
        /// Gets and sets the secondary aid type generated during interaction
        /// with the target.  Aid gets applied to the source Actor.
        /// </summary>
        internal AidType SecondaryAidType { get; set; }

        /// <summary>
        /// Gets and sets the type of recovery (HP or MP) applied
        /// to the Actor if SecondaryAidType is Recovery.
        /// </summary>
        internal RecoveryType SecondaryRecoveryType { get; set; }

        /// <summary>
        /// Gets and sets the secondary/additional effect harm type
        /// being applied to the target.
        /// </summary>
        internal HarmType SecondaryHarmType { get; set; }

        /// <summary>
        /// Gets and sets the quantity of the secondary Aid/Harm effect.
        /// </summary>
        internal int SecondaryAmount { get; set; }

        #endregion

        #region Overrides
        /// <summary>
        /// Override of the result of calling ToString() on this object.
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
            sb.AppendFormat("      Shadows Used: {0}\n", ShadowsUsed);
            sb.AppendFormat("      Aid Type: {0}\n", AidType);
            sb.AppendFormat("      Recovery Type: {0}\n", RecoveryType);
            sb.AppendFormat("      Harm Type: {0}\n", HarmType);
            sb.AppendFormat("      Amount: {0}\n", Amount);
            sb.AppendFormat("      Damage Modifier: {0}\n", DamageModifier);
            sb.AppendFormat("      Secondary Aid Type: {0}\n", AidType);
            sb.AppendFormat("      Secondary Recovery Type: {0}\n", RecoveryType);
            sb.AppendFormat("      Secondary Harm Type: {0}\n", HarmType);
            sb.AppendFormat("      Secondary Amount: {0}\n", Amount);

            return sb.ToString();
        }
        #endregion

    }
}
