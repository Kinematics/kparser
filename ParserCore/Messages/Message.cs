using System;
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
        #endregion

        #region Readonly Properties
        internal Collection<MessageLine> MessageLineCollection
        {
            get { return msgLineCollection; }
        }

        internal MessageCategoryType MessageCategory
        {
            get { return messageCategory; }
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

        internal uint PrimaryMessageCode
        {
            get
            {
                MessageLine lastMsgLine = MessageLineCollection.FirstOrDefault();
                if (lastMsgLine != null)
                {
                    return lastMsgLine.MessageCode;
                }
                else
                {
                    return 0;
                }
            }
        }

        internal uint CurrentMessageCode
        {
            get
            {
                MessageLine lastMsgLine = MessageLineCollection.LastOrDefault();
                if (lastMsgLine != null)
                {
                    return lastMsgLine.MessageCode;
                }
                else
                {
                    return 0;
                }
            }
        }

        internal bool IsParseSuccessful
        {
            get { return parseSuccessful; }
        }

        #endregion


        #region Details Properties -- only created when MessageCateogory is set.
        internal ChatDetails ChatDetails { get; private set; }

        internal SystemDetails SystemDetails { get; private set; }

        internal EventDetails EventDetails { get; private set; }
        #endregion


        #region Text Grouping
        internal string CurrentMessageText
        {
            get
            {
                if (activeMessageStrings.Count == 0)
                    return string.Empty;
                else if (activeMessageStrings.Count > 1)
                    return activeMessageStrings.Aggregate((s1, s2) => s1 + " " + s2);
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
                    return completedMessageStrings.Aggregate((s1, s2) => s1 + " " + s2);
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
                    fullText = completedMessageStrings.Aggregate((s1, s2) => s1 + " " + s2);
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
                            fullText = activeMessageStrings.Aggregate((s1, s2) => s1 + " " + s2);
                        else
                            fullText = activeMessageStrings[0];
                    }
                }

                return fullText;
            }
        }
        #endregion


        #region Methods to modify Message
        internal void AddMessageLine(MessageLine msgLine)
        {
            if (msgLine == null)
                return;

            //if (parseSuccessful == true)
            //    return;

            msgLineCollection.Add(msgLine);
            activeMessageStrings.Add(msgLine.TextOutput);
        }

        internal void SetMessageCategory(MessageCategoryType _messageCategory)
        {
            switch (_messageCategory)
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
                default:
                    throw new ArgumentOutOfRangeException("_messageCategory", _messageCategory,
                        "Unknown message category.");
            }

            messageCategory = _messageCategory;
        }

        internal void SetParseSuccess(bool _parseSuccess)
        {
            parseSuccessful = _parseSuccess;
            if (parseSuccessful == true)
            {
                completedMessageStrings.Add(CurrentMessageText);
                activeMessageStrings.Clear();
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
            sb.AppendFormat("Parse Successful: {0}\n", IsParseSuccessful);

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
