using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser
{
    internal class EventDetails
    {
        #region Member Variables
        EventMessageType eventMessageType;
        #endregion

        #region Properties
        /// <summary>
        /// Gets and sets the action message type.  When set, creates the
        /// appropriate class for further reference.
        /// </summary>
        internal EventMessageType EventMessageType
        {
            get { return eventMessageType; }
            set {

                if (eventMessageType == value)
                    return;

                switch (value)
                {
                    case EventMessageType.Interaction:
                        if (CombatDetails == null)
                            CombatDetails = new CombatDetails();
                        break;
                    case EventMessageType.Loot:
                        if (LootDetails == null)
                            LootDetails = new LootDetails();
                        break;
                    case EventMessageType.Steal:
                        if (CombatDetails == null)
                            CombatDetails = new CombatDetails();
                        break;
                    case EventMessageType.Experience:
                        if (ExperienceDetails == null)
                            ExperienceDetails = new ExperienceDetails();
                        break;
                }
                
                eventMessageType = value;
            }
        }

        /// <summary>
        /// Gets the combat details for this action details object.
        /// </summary>
        internal CombatDetails CombatDetails { get; private set; }

        /// <summary>
        /// Gets the loot details for this action details object.
        /// </summary>
        internal LootDetails LootDetails { get; private set; }

        /// <summary>
        /// Gets the experience details for this action details object.
        /// </summary>
        internal ExperienceDetails ExperienceDetails { get; private set; }
        #endregion

        #region Overrides
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Action Details:\n");
            sb.AppendFormat("  Action Message Type: {0}\n", EventMessageType);

            if ((EventMessageType == EventMessageType.Interaction) ||
                (EventMessageType == EventMessageType.Steal))
                sb.Append(CombatDetails.ToString());
            if (EventMessageType == EventMessageType.Loot)
                sb.Append(LootDetails.ToString());
            if (EventMessageType == EventMessageType.Experience)
                sb.Append(ExperienceDetails.ToString());


            return sb.ToString();
        }
        #endregion

    }
}
