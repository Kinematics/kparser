using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser
{
    public class ChatLine
    {
        private string chatText = string.Empty;
        private DateTime timestamp = MagicNumbers.MinSQLDateTime;

        public ChatLine(string chatText)
        {
            this.chatText = chatText;
            this.timestamp = DateTime.Now;
        }

        public ChatLine(string chatText, DateTime timestamp)
        {
            this.chatText = chatText;
            this.timestamp = timestamp;
        }

        public string ChatText
        {
            get { return chatText; }
        }

        public DateTime Timestamp
        {
            get
            {
                return timestamp;
            }
            set
            {
                if (value < MagicNumbers.MinSQLDateTime)
                    timestamp = MagicNumbers.MinSQLDateTime;
                else
                    timestamp = value;
            }
        }
    }
}
