using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser
{
    internal class NewActionDetails
    {
        #region Member Variables
        ActionMessageType actionMessageType;
        #endregion

        #region Properties
        /// <summary>
        /// Gets and sets the action message type.  When set, creates the
        /// appropriate class for further reference.
        /// </summary>
        internal ActionMessageType ActionMessageType
        {
            get { return actionMessageType; }
            set {

                if (actionMessageType == value)
                    return;

                switch (value)
                {
                    case ActionMessageType.Combat:
                        CombatDetails = new NewCombatDetails();
                        break;
                    case ActionMessageType.Loot:
                        LootDetails = new LootDetails();
                        break;
                    case ActionMessageType.Experience:
                        ExperienceDetails = new NewExperienceDetails();
                        break;
                }
                
                actionMessageType = value;
            }
        }

        /// <summary>
        /// Gets the combat details for this action details object.
        /// </summary>
        internal NewCombatDetails CombatDetails { get; private set; }

        /// <summary>
        /// Gets the loot details for this action details object.
        /// </summary>
        internal LootDetails LootDetails { get; private set; }

        /// <summary>
        /// Gets the experience details for this action details object.
        /// </summary>
        internal NewExperienceDetails ExperienceDetails { get; private set; }
        #endregion

        #region Overrides
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Action Details:\n");
            sb.AppendFormat("  Action Message Type: {0}\n", ActionMessageType);

            if (ActionMessageType == ActionMessageType.Combat)
                sb.Append(CombatDetails.ToString());
            if (ActionMessageType == ActionMessageType.Loot)
                sb.Append(LootDetails.ToString());
            if (ActionMessageType == ActionMessageType.Experience)
                sb.Append(ExperienceDetails.ToString());


            return sb.ToString();
        }
        #endregion

    }
}
