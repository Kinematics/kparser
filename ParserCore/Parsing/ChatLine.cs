using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser
{
    public class ChatLine
    {
        #region Member Variables
        private readonly string rawChatText;
        private readonly string filteredChatText;
        private DateTime timestamp = MagicNumbers.MinSQLDateTime;
        private int recordLogID = -1;
        #endregion

        #region Constructors
        /// <summary>
        /// Use this constructor for strings read from LOG files.
        /// </summary>
        /// <param name="chatText"></param>
        public ChatLine(string chatText)
        {
            if (IsByteArrayString(chatText))
                this.rawChatText = chatText;
            else
                this.rawChatText = ConvertToByteArrayString(chatText);

            filteredChatText = FilterText(rawChatText);

            this.timestamp = DateTime.Now.ToUniversalTime();
        }

        /// <summary>
        /// Use this constructor when reparsing database files.
        /// </summary>
        /// <param name="chatText"></param>
        /// <param name="timestamp"></param>
        public ChatLine(string chatText, DateTime timestamp)
        {
            if (IsByteArrayString(chatText))
                this.rawChatText = chatText;
            else
                this.rawChatText = ConvertToByteArrayString(chatText);

            filteredChatText = FilterText(rawChatText);

            this.timestamp = timestamp;
        }

        /// <summary>
        /// Use this constructor for the new methodology of reading
        /// RAM as byte arrays instead of marshalled as a string.
        /// </summary>
        /// <param name="chatBytes"></param>
        public ChatLine(byte[] chatBytes)
        {
            // Create a string representation of the bytes provided that
            // can be stored in the database.  Each 'char' is one byte
            // from the original data, but may actually be part of a
            // multi-byte encoding.
            char[] charArray = ConvertToCharArray(chatBytes);
            rawChatText = new string(charArray);

            // Run the byte array through the filter and get a proper
            // unicode string back that can be used by the parser.
            this.filteredChatText = FilterByteArray(chatBytes);

            // Record the current timestamp of the chatline creation.
            this.timestamp = DateTime.Now.ToUniversalTime();
        }

        /// <summary>
        /// Use this constructor when reparsing database files if the
        /// database file format ever changes to storing byte arrays
        /// instead of strings for the record log.
        /// </summary>
        /// <param name="chatBytes"></param>
        /// <param name="timestamp"></param>
        public ChatLine(byte[] chatBytes, DateTime timestamp)
        {
            // Create a string representation of the bytes provided that
            // can be stored in the database.  Each 'char' is one byte
            // from the original data, but may actually be part of a
            // multi-byte encoding.
            char[] charArray = ConvertToCharArray(chatBytes);
            rawChatText = new string(charArray);

            // Run the byte array through the filter and get a proper
            // unicode string back that can be used by the parser.
            this.filteredChatText = FilterByteArray(chatBytes);

            // Set the timestamp as provided.
            this.timestamp = timestamp.ToUniversalTime();
        }
        #endregion

        #region Misc string conversions and array manipulations
        /// <summary>
        /// Determines whether the provided string can be considered a pure
        /// byte/ASCII array; that is, if none of the characters have a value
        /// higher than 0xff then all chars can actually be considered bytes.
        /// If any chars have values that make use of both bytes of the char
        /// datatype then the string has been encoded in some way, and needs
        /// to be processed differently.
        /// </summary>
        /// <param name="chatText">The string to check.</param>
        /// <returns>Returns true if all chars can be considered bytes.</returns>
        private bool IsByteArrayString(string chatText)
        {
            char[] stringAsCharArray = chatText.ToCharArray();
            if (stringAsCharArray.Any(c => c > 0xff))
                return false;

            return true;
        }

        /// <summary>
        /// Reencodes the given string back to the POL default Shift_JIS encoding
        /// of chars so that it can be properly broken up into individual bytes to
        /// be filtered.  This should only be used when reparsing older saved
        /// parses.
        /// </summary>
        /// <param name="chatText">Chat text with non-ASCII characters.</param>
        /// <returns>A string composed of chars that represent the original byte array.</returns>
        private string ConvertToByteArrayString(string chatText)
        {
            byte[] originalBytes = UnicodeEncoding.Default.GetBytes(chatText);

            char[] charBytes = ConvertToCharArray(originalBytes);

            return new string(charBytes);
        }

        /// <summary>
        /// Function to remove embedded 0's from a generated Unicode
        /// byte array.  Unicode chars are 2 bytes, and the high order byte
        /// of ASCII chars are 0, which get embedded in the generated byte
        /// array.  These need to be removed in order to properly process
        /// the array back into Unicode after filtering.
        /// </summary>
        /// <param name="chatStringBytes">A Unicode string byte array.</param>
        /// <returns>Returns the byte array without any embedded 0's.</returns>
        private byte[] PackUnicodeBytes(byte[] chatStringBytes)
        {
            byte[] packedBytes = new byte[chatStringBytes.Length];
            int packedIndex = 0;

            for (int i = 0; i < chatStringBytes.Length; i++)
            {
                if (chatStringBytes[i] != 0)
                {
                    packedBytes[packedIndex++] = chatStringBytes[i];
                }
            }

            byte[] constrainedPackedBytes = new byte[packedIndex];

            Array.ConstrainedCopy(packedBytes, 0, constrainedPackedBytes, 0, packedIndex);

            return constrainedPackedBytes;
        }

        /// <summary>
        /// Function to convert a byte array to a char array without encoding conversion.
        /// </summary>
        /// <param name="byteArray">Byte array to convert.</param>
        /// <returns>Char array of the same length and same content.</returns>
        private char[] ConvertToCharArray(byte[] byteArray)
        {
            if (byteArray == null)
                throw new ArgumentNullException();

            char[] charArray = new char[byteArray.Length];

            for (int i = 0; i < byteArray.Length; i++)
            {
                charArray[i] = (char)byteArray[i];
            }

            return charArray;
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
        private string FilterByteArray(byte[] chatBytes)
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
                            filteredChatBytes[dstIndex++] = (byte)'[';
                            skipByte = true;
                        }
                        else if (nextByte == 0x28) // Close
                        {
                            filteredChatBytes[dstIndex++] = (byte)']';
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

                        // Any instance of it before position 66 (end of header is 53) should
                        // be considered skippable.
                        else if (srcIndex < 66)
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
            string newChatString = Encoding.GetEncoding("Shift_JIS").GetString(trimmedChatBytes);
            
            return newChatString;
        }

        /// <summary>
        /// Takes a Unicode string and converts/filters it using the above byte array
        /// function.  It's necessary to process the original unicode string and pack
        /// the generated byte array because otherwise it's just a conversion of the
        /// char array to a byte array, including all embedded 0's (high bits of the
        /// 2-byte char).
        /// </summary>
        /// <param name="baseChatString">A Unicode string.</param>
        /// <returns>Returns a filtered Unicode string.</returns>
        private string FilterText(string baseChatString)
        {
            byte[] chatStringBytes = Encoding.Unicode.GetBytes(baseChatString);
            chatStringBytes = PackUnicodeBytes(chatStringBytes);
            return FilterByteArray(chatStringBytes);
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
