using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser
{
    internal class SystemDetails
    {
        internal SystemMessageType SystemMessageType { get; set; }

        #region Overrides
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("System Details:\n");
            sb.AppendFormat("  System Message Type: {0}\n", SystemMessageType);

            return sb.ToString();
        }
        #endregion
    }
}
