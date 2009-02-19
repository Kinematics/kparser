using System;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using WaywardGamers.KParser.Monitoring;

namespace WaywardGamers.KParser
{
	/// <summary>
	/// Breaks down a message line into its unique components.
    /// Does not attempt to interpret those components.
	/// 
	/// Message format from the log files:
	/// ##[mC],##[ec1],##[ec2],########[tC],########[eS],########[uS],
    /// ####[length],00,01,##[mC],00,0x1E+0x01+{??}+0x1E+0x01"TextString"[cC]0x00
	/// 
	/// In order:
	/// ## - msgCode (overall message code)
	/// ## - extraCode1 (unknown)
	/// ## - extraCode2 (unknown)
	/// ######## - textColor
	/// ######## - eventSequence
	/// ######## - uniqueSequence
	/// #### - textLength
	/// 00 - delimiter
	/// 01 - delimiter
	/// ## - message category [00 = system, 01 = chat, 02 = action]
	/// 00 - delimiter
	/// 0x1E+0x01 - end codes marker
    /// 0x1E+0x01 - begin text marker
    /// "text string"
	/// [closeCode] (optional) - 0x7F+0x31
	/// 0x00 - end message string
    /// </summary>
	internal class MessageLine
	{
		#region Member Variables
        readonly ChatLine originalChatLine;
        #endregion

        #region Message parsing regexes (static data)

        // Regex describing general chat line format
        static Regex msgLineBreakdown = new Regex(
            @"^(?<msgCode>[0-9a-f]{2}),(?<xCode1>[0-9a-f]{2}),(?<xCode2>[0-9a-f]{2}),(?<msgColor>[0-9a-f]{8})," +
            @"(?<eventSeq>[0-9a-f]{8}),(?<uniqSeq>[0-9a-f]{8}),(?<strLen>[0-9a-f]{4}),(?<unk1>[0-9a-f]{2})," +
            @"(?<unk2>[0-9a-f]{2}),(?<msgCat>[0-9a-f]{2}),(?<unk3>[0-9a-f]{2})," +
            @"(\x1e\x01)+(\x81\x40)?(\x1e\x01)*" +
            @"(?<tsPlugin>((\x1e(\x3f|\xfa|\xfc)\[)|(\x1e.))(?<time>\d{2}:\d{2}:\d{2})\] \x1e\x01)?" +
            @"(?<remainder>.+)$");


        // Regexes for finding words that have special coding that needs to be stripped
        static Regex autoTrans = new Regex(@"(\xEF\x27(?<autoTransWord>[^\xEF]+)\xEF\x28)|(\x3F\x27(?<autoTransWord>[^\x3F]+)\x3F\x28)");
        static Regex itemWords = new Regex(@"(\x1E\x02(?<itemWord>[^\x1E]+)\x1E\x01)");
        static Regex keyItemWords = new Regex(@"(\x1E\x03(?<itemWord>[^\x1E]+)\x1E\x01)");

        // Regexes for extraneous code values
        static Regex eolMark = new Regex(@"\x7F\x31$");
        static Regex clipText = new Regex(@"\x1f[\x79\x7f\x3f\x8d\x2019]");
        //(nextChar == 0x79) || // Item drops
        //(nextChar == 0x7F) || // Item distribution
        //(nextChar == 0x3F) || // Time limit warning
        //(nextChar == 0x8D) || // Limbus time limit, Moogle job change
        //(nextChar == 0x2019)) // Assault time limit, byte=0x92

        #endregion

        #region Constructor
        internal MessageLine(ChatLine chatLine)
		{
            if (chatLine == null)
                throw new ArgumentNullException("chatLine");

            if (chatLine.ChatText == "")
				throw new ArgumentException("Cannot process an empty message");

            originalChatLine = chatLine;

			ExtractDataFromOriginalChatLine();
		}
		#endregion

		#region Parsing functions
		/// <summary>
		/// Function to break down the message into its component parts.
		/// </summary>
        private void ExtractDataFromOriginalChatLine()
		{
            // Make a copy of the original text to work with
            string msg = originalChatLine.ChatText;
            string testMsg = string.Format("Test: {0}", msg);

            Match msgLineMatch = msgLineBreakdown.Match(msg);

            if (msgLineMatch.Success == false)
            {
                throw new FormatException("Unable to parse the chat message:\n" + msg);
            }

            MessageCode = uint.Parse(msgLineMatch.Groups["msgCode"].Value, NumberStyles.HexNumber);
            ExtraCode1 = uint.Parse(msgLineMatch.Groups["xCode1"].Value, NumberStyles.HexNumber);
            ExtraCode2 = uint.Parse(msgLineMatch.Groups["xCode2"].Value, NumberStyles.HexNumber);
            TextColor = uint.Parse(msgLineMatch.Groups["msgColor"].Value, NumberStyles.HexNumber);
            EventSequence = uint.Parse(msgLineMatch.Groups["eventSeq"].Value, NumberStyles.HexNumber);
            UniqueSequence = uint.Parse(msgLineMatch.Groups["uniqSeq"].Value, NumberStyles.HexNumber);
            TextLength = uint.Parse(msgLineMatch.Groups["strLen"].Value, NumberStyles.HexNumber);
            MessageCategoryNumber = uint.Parse(msgLineMatch.Groups["msgCat"].Value, NumberStyles.HexNumber);

            MessageCategory = (MessageCategoryType)MessageCategoryNumber;

            // Extract Timestamp plugin data if present
            if (msgLineMatch.Groups["tsPlugin"].Success == true)
            {
                // If we're parsing from logs, use the Timestamp field to refine
                // the message timestamps.
                if (Monitor.ParseMode == DataSource.Log)
                {
                    DateTime baseDate = originalChatLine.Timestamp.Date;
                    TimeSpan pluginTime;
                    if (TimeSpan.TryParse(msgLineMatch.Groups["time"].Value, out pluginTime))
                    {
                        baseDate += pluginTime;
                        originalChatLine.Timestamp = baseDate;
                    }
                }
            }

            // All leftover text gets put into the TextOuput property for further conversion.
            TextOutput = msgLineMatch.Groups["remainder"].Value;


            // Adjustments for words with special display markup
            TextOutput = autoTrans.Replace(TextOutput, "[${autoTransWord}]");
            TextOutput = itemWords.Replace(TextOutput, "${itemWord}");
            TextOutput = keyItemWords.Replace(TextOutput, "${itemWord}");


            // Remove other peculiar character values

            // Drop the extraneous characters at the end of non-chat messages.
            TextOutput = eolMark.Replace(TextOutput, "");

            // Drop the extraneous characters at start (or middle, for moogles) of various messages.
            TextOutput = clipText.Replace(TextOutput, "");

            // Convert text encoding for display of JP characters
            byte[] originalBytes = UnicodeEncoding.Default.GetBytes(TextOutput);
            byte[] convertedBytes = Encoding.Convert(Encoding.GetEncoding("Shift-JIS"), Encoding.Unicode, originalBytes);
            TextOutput = Encoding.Unicode.GetString(convertedBytes).Trim();
        }
        #endregion

        #region Base chatline Properties
        /// <summary>
        /// Gets the timestamp of the original chat line the message was created from.
        /// </summary>
        internal DateTime Timestamp
        {
            get
            {
                return originalChatLine.Timestamp;
            }
        }

        /// <summary>
        /// Gets the full text string of the original chat line the message was created from.
        /// </summary>
        internal string OriginalText
        {
            get
            {
                return originalChatLine.ChatText;
            }
        }
        #endregion
        
        #region Raw Source Properties
        internal string TextOutput { get; private set; }

        internal uint MessageCode { get; private set; }

        internal uint ExtraCode1 { get; private set; }

        internal uint ExtraCode2 { get; private set; }

        internal uint TextColor { get; private set; }

        internal uint EventSequence { get; private set; }

        internal uint UniqueSequence { get; private set; }

        internal uint TextLength { get; private set; }

        internal uint MessageCategoryNumber { get; private set; }

        /// <summary>
        /// Get the message category enumeration.  Message category separates messages into
        /// three general types: system, chat and action/event/other.
        /// </summary>
        internal MessageCategoryType MessageCategory { get; private set; }
        #endregion
    }
}
