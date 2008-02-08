﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser
{
    internal class Message
    {
        #region Member Variables
        MessageCategoryType messageCategory;

        Collection<MessageLine> msgLineCollection = new Collection<MessageLine>();
        List<string> activeMessageStrings = new List<string>();
        List<string> completedMessageStrings = new List<string>();

        bool parseSuccessful;
        #endregion

        #region Base MessageLine Properties
        internal uint MessageID { get; set; }

        internal uint MessageCode { get; set; }

        internal uint ExtraCode1 { get; set; }

        internal uint ExtraCode2 { get; set; }

        internal MessageCategoryType MessageCategory
        {
            get { return messageCategory; }
            set
            {
                switch (value)
                {
                    case MessageCategoryType.Chat:
                        ChatDetails = new ChatDetails();
                        break;
                    case MessageCategoryType.System:
                        SystemDetails = new SystemDetails();
                        break;
                    case MessageCategoryType.Event:
                        EventDetails = new EventDetails();
                        break;
                }

                messageCategory = value;
            }
        }

        internal Collection<MessageLine> MessageLineCollection
        {
            get { return msgLineCollection; }
        }

        internal DateTime Timestamp
        {
            get
            {
                if (msgLineCollection.Count > 0)
                    return msgLineCollection[0].Timestamp;
                else
                    return MagicNumbers.MinSQLDateTime;
            }
        }
        #endregion

        #region Details Properties -- only created when MessageCateogory is set.
        internal ChatDetails ChatDetails { get; private set; }

        internal SystemDetails SystemDetails { get; private set; }

        internal EventDetails EventDetails { get; private set; }
        #endregion

        #region Text Grouping
        internal void AddMessageLine(MessageLine msgLine)
        {
            if (msgLine == null)
                return;

            msgLineCollection.Add(msgLine);
            activeMessageStrings.Add(msgLine.TextOutput);
        }

        internal string CurrentMessageText
        {
            get
            {
                if (activeMessageStrings.Count == 0)
                    return string.Empty;
                else if (activeMessageStrings.Count > 1)
                    return activeMessageStrings.Aggregate(string.Empty, (s1, s2) => s1 + " " + s2);
                else
                    return activeMessageStrings[0];
            }
        }

        internal string PreviousMessageText
        {
            get
            {
                if (completedMessageStrings.Count == 0)
                    return string.Empty;
                else if (completedMessageStrings.Count > 1)
                    return completedMessageStrings.Aggregate(string.Empty, (s1, s2) => s1 + " " + s2);
                else
                    return completedMessageStrings[0];
            }
        }

        internal string CompleteMessageText
        {
            get
            {
                string fullText;

                if (completedMessageStrings.Count == 0)
                    fullText = string.Empty;
                else if (completedMessageStrings.Count > 1)
                    fullText = completedMessageStrings.Aggregate(string.Empty, (s1, s2) => s1 + " " + s2);
                else
                    fullText = completedMessageStrings[0];

                if (activeMessageStrings.Count > 0)
                {
                    if (fullText != string.Empty)
                    {
                        fullText = activeMessageStrings.Aggregate(fullText, (s1, s2) => s1 + " " + s2);
                    }
                    else
                    {
                        if (activeMessageStrings.Count > 1)
                            fullText = activeMessageStrings.Aggregate(string.Empty, (s1, s2) => s1 + " " + s2);
                        else
                            fullText = activeMessageStrings[0];
                    }
                }

                return fullText;
            }
        }
        #endregion

        #region Parsing Updates
        internal bool ParseSuccessful
        {
            get { return parseSuccessful; }
            set
            {
                parseSuccessful = value;
                if (parseSuccessful == true)
                {
                    completedMessageStrings.Add(CurrentMessageText);
                    activeMessageStrings.Clear();
                }
            }
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("----------------");
            sb.AppendFormat("Message ID: {0}  @ {1}\n", MessageID, Timestamp);

            foreach (MessageLine msgLine in msgLineCollection)
            {
                sb.AppendFormat("Raw chatline data: {0}\n", msgLine.OriginalText);
            }

            sb.AppendFormat("Message Code: {0:x}\n", MessageCode);

            sb.AppendFormat("Message Category: {0}\n", MessageCategory);

            if (SystemDetails != null)
                sb.Append(SystemDetails.ToString());
            if (ChatDetails != null)
                sb.Append(ChatDetails.ToString());
            if (EventDetails != null)
                sb.Append(EventDetails.ToString());

            sb.Append("\n");

            return sb.ToString();
        }
        #endregion
    }
}
