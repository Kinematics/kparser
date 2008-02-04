using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser
{
    internal class NewChatDetails
    {
        #region Properties
        internal string ChatSpeakerName { get; set; }
        internal SpeakerType ChatSpeakerType { get; set; }
        internal ChatMessageType ChatMessageType { get; set; }
        internal string FullChatText { get; set; }
        #endregion

        #region Overrides
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Chat Details:\n");
            sb.AppendFormat("  Chat Speaker Name: {0}\n", ChatSpeakerName);
            sb.AppendFormat("  Chat Speaker Type: {0}\n", ChatSpeakerType);
            sb.AppendFormat("  Chat Message Type: {0}\n", ChatMessageType);
            sb.AppendFormat("  Chat Text: {0}\n", FullChatText);

            return sb.ToString();
        }
        #endregion

    }
}
