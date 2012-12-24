using System;
using System.Text;

namespace WaywardGamers.KParser
{
    internal class LootDetails
    {
        #region Properties
        /// <summary>
        /// Gets and set the type of loot represented here (mob drops,
        /// stolen items/mugged gil, chest/coffer rewards.
        /// </summary>
        internal LootType LootType { get; set; }

        /// <summary>
        /// Gets and sets whether this object contains details on the
        /// initial finding of the loot item.
        /// </summary>
        internal bool IsFoundMessage { get; set; }

        /// <summary>
        /// Gets and sets whether this loot item was lost rather than
        /// dropped to a particular player.
        /// </summary>
        internal bool WasLost { get; set; }

        /// <summary>
        /// Gets and sets the amount of gil obtained.
        /// </summary>
        internal int Gil { get; set; }

        /// <summary>
        /// Gets and sets the amount of gil obtained.
        /// </summary>
        internal int Amount { get; set; }

        /// <summary>
        /// Gets and sets the name of the item
        /// </summary>
        internal string ItemName { get; set; }

        /// <summary>
        /// Gets and sets the name of the mob the item dropped from.
        /// Can only be accessed if isFoundMessage is true.
        /// </summary>
        internal string TargetName { get; set; }

        /// <summary>
        /// Gets and sets the entity type of the target: Either Mob
        /// or TreasureChest.
        /// </summary>
        internal EntityType TargetType { get; set; }

        /// <summary>
        /// Gets and sets the name of the player who received the item.
        /// Can only be accessed if isFoundMessage is false, and the item
        /// was not lost.
        /// </summary>
        internal string WhoObtained { get; set; }
        #endregion

        #region Overrides
        /// <summary>
        /// Override for debugging output.
        /// </summary>
        /// <returns>String containing all details of this object.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("  Loot Details:\n");
            sb.AppendFormat("    Loot Type: {0}\n", LootType);
            sb.AppendFormat("    Is Found: {0}\n", IsFoundMessage);
            sb.AppendFormat("    Was Lost: {0}\n", WasLost);
            if (ItemName != null)
                sb.AppendFormat("    Item Name: {0}\n", ItemName);
            else
                sb.Append("    No Item Name\n");
            sb.AppendFormat("    Mob Name: {0}\n", TargetName);
            sb.AppendFormat("    Who Obtained: {0}\n", WhoObtained);
            sb.AppendFormat("    Gil: {0}\n", Gil);
            sb.AppendFormat(" Amount: {0}\n", Amount);

            return sb.ToString();
        }
        #endregion
    }
}
