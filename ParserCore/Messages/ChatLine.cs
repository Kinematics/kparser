using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser
{
    public class ChatLine
    {
        private readonly string rawChatText;
        private readonly string filteredChatText;
        private DateTime timestamp = MagicNumbers.MinSQLDateTime;
        private int recordLogID = -1;
        private readonly byte[] chatTextBytes;

        #region Constructors
        public ChatLine(string chatTextParam)
        {
            this.rawChatText = chatTextParam;
            this.timestamp = DateTime.Now.ToUniversalTime();
            filteredChatText = FilterText(rawChatText);
        }

        public ChatLine(string chatText, DateTime timestamp)
        {
            this.rawChatText = chatText;
            this.timestamp = timestamp;
            filteredChatText = FilterText(rawChatText);
        }

        public ChatLine(byte[] chatBytes)
        {
            // Create a copy of the byte array so that the pointer
            // reference is local only, and the original can be released.
            chatTextBytes = new byte[chatBytes.Length];
            Array.ConstrainedCopy(chatBytes, 0, chatTextBytes, 0, chatBytes.Length);

            char[] charArray = ConvertToCharArray(chatTextBytes);
            rawChatText = new string(charArray);

            this.filteredChatText = ConvertToText(chatTextBytes);
            this.timestamp = DateTime.Now.ToUniversalTime();
        }

        public ChatLine(byte[] chatBytes, DateTime timestamp)
        {
            // Create a copy of the byte array so that the pointer
            // reference is local only, and the original can be released.
            chatTextBytes = new byte[chatBytes.Length];
            Array.ConstrainedCopy(chatBytes, 0, chatTextBytes, 0, chatBytes.Length);

            char[] charArray = ConvertToCharArray(chatTextBytes);
            rawChatText = new string(charArray);

            this.filteredChatText = ConvertToText(chatTextBytes);
            this.timestamp = timestamp.ToUniversalTime();
        }
        #endregion

        #region Filtering raw data
        /// <summary>
        /// This function takes the byte array provided to the constructor that comprises the
        /// buffer data read from RAM and applies some filters to the incoming data.
        /// Some of the 'text' of the message is actually special codes used for custom
        /// game display, and will corrupt the interpretation of the text itself.
        /// </summary>
        /// <param name="chatBytes">Byte array of unmodified RAM data for a single chat line.</param>
        /// <returns>Returns a cleaned string representation of the bytes provided.</returns>
        private string ConvertToText(byte[] chatBytes)
        {
            byte[] filteredChatBytes = new byte[chatBytes.Length];

            int srcIndex = 0;
            int dstIndex = 0;
            byte nextByte;
            bool skipByte = false;

            for (srcIndex = 0; srcIndex < chatBytes.Length; srcIndex++)
            {
                if (srcIndex < (chatBytes.Length - 1))
                    nextByte = chatBytes[srcIndex + 1];
                else
                    nextByte = 0;

                skipByte = false;

                switch (chatBytes[srcIndex])
                {
                    case 0x1f:
                        // Skip the following:
                        //(nextChar == 0x79) || // Item drops
                        //(nextChar == 0x7F) || // Item distribution
                        //(nextChar == 0x3F) || // Time limit warning
                        //(nextChar == 0x8D) || // Limbus time limit, Moogle job change
                        //(nextChar == 0x2019)) // Assault time limit, byte=0x92
                        if ((nextByte == 0x79) || (nextByte == 0x7f) || (nextByte == 0x8d) || (nextByte == 0x3f) || (nextByte == 0x92))
                            skipByte = true;
                        break;
                    case 0x3f:
                    case 0xef:
                        // Autotranslate codes
                        if (nextByte == 0x27) // Open
                        {
                            filteredChatBytes[dstIndex++] = (byte) ("[".ToCharArray()[0]);
                            skipByte = true;
                        }
                        else if (nextByte == 0x28) // Close
                        {
                            filteredChatBytes[dstIndex++] = (byte)("]".ToCharArray()[0]);
                            skipByte = true;
                        }
                        break;
                    case 0x1e:
                        // 1e02 marks the opening code for item words in text. 1e01 closes it.
                        // 1e03 marks the opening code for key item words in text.  1e01 closes it.
                        // We want to skip them entirely.
                        if ((nextByte == 0x01) || (nextByte == 0x02) || (nextByte == 0x03))
                            skipByte = true;

                        // The Windower Timestamp plugin uses (or used) various codes that
                        // we want to skip: 1e3f, 1efa, 1efc.  1e01 is also used to close
                        // the timestamp entry (taken care of above).
                        else if ((nextByte == 0x3f) || (nextByte == 0xfa) || (nextByte == 0xfc))
                            skipByte = true;
                        break;
                    case 0x81:
                        // Possible extra bit of code in the break area between the headers and the text.
                        if ((nextByte == 0x40) && (chatBytes[srcIndex+2] == 0x1e))
                            skipByte = true;
                        break;
                    case 0x7f:
                        // Final bytes (optional) in the message
                        if ((nextByte == 0x31) && ((srcIndex + 2) == chatBytes.Length))
                            skipByte = true;
                        break;
                    default:
                        skipByte = false;
                        break;
                }

                if (skipByte)
                {
                    srcIndex++;
                }
                else
                {
                    filteredChatBytes[dstIndex++] = chatBytes[srcIndex];
                }
            }

            // Cull any leftover bytes from the end of the filtered array
            byte[] trimmedChatBytes = new byte[dstIndex];
            Array.ConstrainedCopy(filteredChatBytes, 0, trimmedChatBytes, 0, dstIndex);

            // Convert to a string using Shift_JIS encoding.
            string newChatString = System.Text.Encoding.GetEncoding("Shift_JIS").GetString(trimmedChatBytes);

            return newChatString;
        }

        /// <summary>
        /// This function takes the string provided to the constructor that comprises the
        /// buffer data read from RAM or reparsed from another source and applies some
        /// filters to the incoming data.  Some of the 'text' of the message is actually
        /// special codes used for custom game display, and will corrupt the interpretation
        /// of the text itself.
        /// </summary>
        /// <param name="baseChatString">The base read string from a database or
        /// using Marshal.PtrToStrAnsi().  Each character must be a value
        /// of 255 or less.</param>
        /// <returns>Returns a cleaned version of the string provided.</returns>
        private string FilterText(string baseChatString)
        {
            char[] strAsChars = baseChatString.ToCharArray();
            return ConvertToText(ConvertToByteArray(strAsChars));
        }

        /// <summary>
        /// Function to convert a char array to a byte array without encoding conversion.
        /// </summary>
        /// <param name="strAsChars"></param>
        /// <returns></returns>
        private byte[] ConvertToByteArray(char[] charArray)
        {
            byte[] byteArray = new byte[charArray.Length];

            for (int i = 0; i < charArray.Length; i++)
            {
                byteArray[i] = (byte)charArray[i];
            }

            return byteArray;
        }

        /// <summary>
        /// Function to convert a byte array to a char array without encoding conversion.
        /// </summary>
        /// <param name="chatTextBytes"></param>
        /// <returns></returns>
        private char[] ConvertToCharArray(byte[] byteArray)
        {
            char[] charArray = new char[byteArray.Length];

            for (int i = 0; i < byteArray.Length; i++)
            {
                charArray[i] = (char)byteArray[i];
            }

            return charArray;
        }

        #endregion


        #region Properties
        public string ChatText
        {
            get { return rawChatText; }
        }

        public string FilteredChatText
        {
            get { return filteredChatText; }
        }

        public byte[] ChatTextData
        {
            get { return chatTextBytes; }
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

        public int RecordLogID
        {
            get
            {
                return recordLogID;
            }
            set
            {
                recordLogID = value;
            }
        }
        #endregion
    }
}
