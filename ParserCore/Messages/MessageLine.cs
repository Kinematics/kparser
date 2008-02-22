using System;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

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
        ChatLine originalChatLine;
		#endregion

        #region Constructor
        internal MessageLine(ChatLine chatLine)
		{
            if (chatLine == null)
                throw new ArgumentNullException("chatLine");

            if (chatLine.ChatText == "")
				throw new ArgumentException("Cannot process an empty message");

            originalChatLine = chatLine;

			TokenizeOriginalChatLine();
		}
		#endregion

		#region Parsing functions
		/// <summary>
		/// Function to break down the message into its component parts.
		/// </summary>
        private void TokenizeOriginalChatLine()
		{
            // Make a copy of the original text to work with
            string msg = originalChatLine.ChatText;

            // Characters that separate code values from text output.
            //string breakString = string.Format("{0}{1}", (char)0x1E, (char)0x01);
            // Patch for Oct 18, 2006 change -- string now has two breakpoint positions,
            // with possible additional material between (unknown use).
            string breakString = string.Format("{0}{1}", (char)0x1E, (char)0x01);

			// Locate the end of the code strip in the message
			int endCodesBreakPoint = msg.IndexOf(breakString);

            if (endCodesBreakPoint < 0)
                throw new FormatException("Message string does not contain the proper breakpoint values (position 1).\n"
                    + msg);

            // Extract the text codes from the front half of the message.
            string preMsg = msg.Substring(0, endCodesBreakPoint);

            // Locate the beginning of the text string
            int beingTextBreakPoint = msg.IndexOf(breakString, endCodesBreakPoint + breakString.Length);

            if (beingTextBreakPoint < 0)
                throw new FormatException("Message string does not contain the proper breakpoint values (position 2).\n"
                    + msg);

            // Extract the display text from the back half of the message.
            TextOutput = msg.Substring(beingTextBreakPoint + breakString.Length);

            int breakPoint;

            // Drop the extraneous characters at start of various messages.

            char[] textOutputCharArray = TextOutput.ToCharArray();
            char[] clippedTextOutputCharArray;

            if (textOutputCharArray.Length > 2)
            {
                if (textOutputCharArray[0] == 0x1F)
                {
                    if ((textOutputCharArray[1] == 0x79) || // Item drops
                        (textOutputCharArray[1] == 0x7F) || // Item distribution
                        (textOutputCharArray[1] == 0x3F) || // Time limit warning
                        (textOutputCharArray[1] == 0x8D) || // Limbus time limit
                        (textOutputCharArray[1] == 0x2019))   // Assault time limit, byte=0x92
                    {
                        clippedTextOutputCharArray = new char[textOutputCharArray.Length - 2];
                        Array.Copy(textOutputCharArray, 2, clippedTextOutputCharArray, 0, clippedTextOutputCharArray.Length);

                        TextOutput = new string(clippedTextOutputCharArray);
                    }
                }
            }

            /*
            // Item drops
            breakString = string.Format("{0}{1}", (char)0x1F, (char)0x79);
            breakPoint = TextOutput.IndexOf(breakString);

            if (breakPoint == 0)
                TextOutput = TextOutput.Substring(2);

            // Item distribution
            breakString = string.Format("{0}{1}", (char)0x1F, (char)0x7F);
            breakPoint = TextOutput.IndexOf(breakString);

            if (breakPoint == 0)
                TextOutput = TextOutput.Substring(2);

            // Time limit warnings
            breakString = string.Format("{0}{1}", (char)0x1F, (char)0x3F);
            breakPoint = TextOutput.IndexOf(breakString);

            if (breakPoint == 0)
                TextOutput = TextOutput.Substring(2);

            // Limbus time limit warnings
            breakString = string.Format("{0}{1}", (char)0x1F, (char)0x8D);
            breakPoint = TextOutput.IndexOf(breakString);

            if (breakPoint == 0)
                TextOutput = TextOutput.Substring(2);

            // Assault time limit warnings
            breakString = string.Format("{0}{1}", (char)0x1F, (char)0x92);
            breakPoint = TextOutput.IndexOf(breakString);

            if (breakPoint == 0)
                TextOutput = TextOutput.Substring(2);

            // System error messages (eg: "There are no party members.")
            breakString = string.Format("{0}{1}", (char)0x1F, (char)0x7B);
            breakPoint = TextOutput.IndexOf(breakString);

            if (breakPoint == 0)
                TextOutput = TextOutput.Substring(2);
            */


            // Drop the extraneous characters at the end of non-chat messages.
            breakString = string.Format("{0}{1}", (char)0x7F, (char)0x31);
            breakPoint = TextOutput.IndexOf(breakString);

            if (breakPoint > 0)
                TextOutput = TextOutput.Substring(0, breakPoint);



			// Convert auto-translated characters to []'s
            // 10/18/2006 - Update changed 0xEF+0x27/8 to 0x3F+0x27/8
            // 01/08/2008 - Unknown update changed 3F back to EF
			string autoTrans;
			// Open
			autoTrans = string.Format("{0}{1}", (char) 0xEF, (char) 0x27);
            TextOutput = TextOutput.Replace(autoTrans, "[");
			// Close
			autoTrans = string.Format("{0}{1}", (char) 0xEF, (char) 0x28);
            TextOutput = TextOutput.Replace(autoTrans, "]");

			// Remove item highlighting (green wording)
			string itemTrans;
			// Open
			itemTrans = string.Format("{0}{1}", (char) 0x1E, (char) 0x02);
            TextOutput = TextOutput.Replace(itemTrans, "");
			// Close
			itemTrans = string.Format("{0}{1}", (char) 0x1E, (char) 0x01);
            TextOutput = TextOutput.Replace(itemTrans, "");

			// Remove key item highlighting (purple wording)
			string keyTrans;
			// Open
			keyTrans = string.Format("{0}{1}", (char) 0x1E, (char) 0x03);
            TextOutput = TextOutput.Replace(keyTrans, "");
			// Close
			keyTrans = string.Format("{0}{1}", (char) 0x1E, (char) 0x01);
            TextOutput = TextOutput.Replace(keyTrans, "");

            // Other version?
            //// Open
            //keyTrans = string.Format("{0}{1}", (char)0x1E, (char)0xFA);
            //TextOutput = TextOutput.Replace(keyTrans, "");
            //// Close
            //keyTrans = string.Format("{0}{1}", (char)0x1E, (char)0x01);
            //TextOutput = TextOutput.Replace(keyTrans, "");

            // Convert text encoding for display of JP characters
            byte[] originalBytes = UnicodeEncoding.Default.GetBytes(TextOutput);
            byte[] convertedBytes = Encoding.Convert(Encoding.GetEncoding("Shift-JIS"), Encoding.Unicode, originalBytes);
            this.TextOutput = Encoding.Unicode.GetString(convertedBytes).Trim();


            // Extract timestamp plugin modification from onscreen text, if applicable
            Match tsCheck = ParseExpressions.timestampPlugin.Match(TextOutput);
            if (tsCheck.Success == true)
            {
                TextOutput = tsCheck.Groups[1].Value;
            }

			// Break up the code sequence values into their individual entries
			string delimStr = ",";
			char [] delimiter = delimStr.ToCharArray();

			string[] codeStrings = preMsg.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

            if (codeStrings.Length < 11)
                throw new InvalidOperationException(string.Format("Invalid code string set:\n{0}", preMsg));

			MessageCode = uint.Parse(codeStrings[0], NumberStyles.HexNumber);
            ExtraCode1 = uint.Parse(codeStrings[1], NumberStyles.HexNumber);
            ExtraCode2 = uint.Parse(codeStrings[2], NumberStyles.HexNumber);
            TextColor = uint.Parse(codeStrings[3], NumberStyles.HexNumber);
            EventSequence = uint.Parse(codeStrings[4], NumberStyles.HexNumber);
            UniqueSequence = uint.Parse(codeStrings[5], NumberStyles.HexNumber);
            TextLength = uint.Parse(codeStrings[6], NumberStyles.HexNumber);
            // codeStrings[7] - undefined, always 0
            // codeStrings[8] - undefined, always 1
            MessageCategoryNumber = uint.Parse(codeStrings[9], NumberStyles.HexNumber);
 
            MessageCategory = (MessageCategoryType)MessageCategoryNumber;
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
        /// Get the message category.  Message category separates messages into
        /// three general types: system, chat, action/other.
        /// </summary>
        internal MessageCategoryType MessageCategory { get; private set; }
        #endregion
    }
}
