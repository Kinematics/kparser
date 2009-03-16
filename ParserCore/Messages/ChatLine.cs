using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser
{
    public class ChatLine
    {
        private readonly string chatText;
        private DateTime timestamp = MagicNumbers.MinSQLDateTime;

        public ChatLine(string chatTextParam)
        {
            this.chatText = chatTextParam;
            this.timestamp = DateTime.Now.ToUniversalTime();
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
