using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Diagnostics;

namespace WaywardGamers.KParser.Monitoring.Packet
{
    public enum RawPacketStringType
    {
        HexEncode,
        Base64Encode
    }

    public class RawPacket
    {
        #region Class Variables
        byte[] packetBytes;

        int bitPointer = 0;
        int bytePointer = 0;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor, given the original byte stream array.
        /// </summary>
        /// <param name="packetBytes"></param>
        public RawPacket(byte[] packetBytes)
        {
            this.packetBytes = packetBytes;
        }

        /// <summary>
        /// Constructor, given a string representing the original byte stream array.
        /// </summary>
        /// <param name="packetString">The string representing the packet.</param>
        /// <param name="stringType">Set to the type of formatting used to encode
        /// the data as a string: Base64Encode that will decode directly to the
        /// original byte array, or HexEncode of the original bytes.</param>
        public RawPacket(string packetString, RawPacketStringType stringType)
        {
            // If in base64, assume it's the full raw byte array data.
            if (stringType == RawPacketStringType.Base64Encode)
            {
                this.packetBytes = System.Convert.FromBase64String(packetString);
            }
            else if (stringType == RawPacketStringType.HexEncode)
            {
                // If we're simply given an arbitrary string, it can be presented as something like:
                // 28 18 79 10 2A D3 E6 05 00 01 44 18 DD 1A 0C 00 00 00 40 54
                // So take two characters at a time, convert them to numeric values, and store in the array.

                string byteString = string.Empty;
                List<byte> byteList = new List<byte>(packetString.Length / 2);

                foreach (char c in packetString)
                {
                    if (c != ' ')
                    {
                        byteString += c;

                        if (byteString.Length == 2)
                        {
                            byteList.Add((byte)int.Parse(byteString, NumberStyles.AllowHexSpecifier));
                            byteString = string.Empty;
                        }
                    }
                }

                this.packetBytes = byteList.ToArray();
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Function to increment the bit pointer in the pattern used
        /// in the original bitstream.  Bits are added from least to
        /// most significant, and then carried over to the next byte
        /// in the array.
        /// </summary>
        private void IncrementBit()
        {
            bitPointer++;
            if (bitPointer > 7)
            {
                bitPointer = 0;
                bytePointer++;
            }
        }

        /// <summary>
        /// Reset pointers to the start of the packet.  Set to private until there's
        /// an actual use case for it.
        /// </summary>
        private void Reset()
        {
            bitPointer = 0;
            bytePointer = 0;
        }

        internal void ReportStatus()
        {
            Trace.WriteLine(string.Format("Packet Length: {0}  Byte pointer: {1}  Bit pointer: {2}",
                packetBytes.Length, bytePointer, bitPointer));
        }
        #endregion


        #region Public methods
        /// <summary>
        /// Reads bits from the packet data and stores them in a byte.
        /// </summary>
        /// <param name="bits">The number of bits to read.  Max: 8</param>
        /// <returns>Returns a byte containing the requested bits.</returns>
        public byte ReadByte(int bits)
        {
            if (bits > 8)
                throw new ArgumentOutOfRangeException("bits", bits, "Invalid number of bits requested");

            byte value = 0;

            // 0 or fewer requested bits, just return 0
            if (bits < 1)
                return value;

            for (int bit = 0; bit < bits; bit++)
            {
                if (bytePointer >= packetBytes.Length)
                    throw new IndexOutOfRangeException("Exceeded length of packet data.");

                value |= (byte)(((packetBytes[bytePointer] >> bitPointer) & 1) << bit);

                IncrementBit();
            }

            return value;
        }

        /// <summary>
        /// Reads bits from the packet data and stores them in a short integer.
        /// </summary>
        /// <param name="bits">The number of bits to read.  Max: 16</param>
        /// <returns>Returns a short containing the requested bits.</returns>
        public short ReadShort(int bits)
        {
            if ((bits < 1) || (bits > 16))
                throw new ArgumentOutOfRangeException("bits", bits, "Invalid number of bits requested");

            byte[] bytes = new byte[2];

            int bitsToRead = bits;
            if (bitsToRead > 8)
                bitsToRead = 8;
            if (bitsToRead > 0)
                bytes[0] = ReadByte(bitsToRead);

            bitsToRead = bits - 8;
            if (bitsToRead > 8)
                bitsToRead = 8;
            if (bitsToRead > 0)
                bytes[1] = ReadByte(bitsToRead);

            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// Reads bits from the packet data and stores them in an int.
        /// </summary>
        /// <param name="bits">The number of bits to read.  Max: 32</param>
        /// <returns>Returns an int containing the requested bits.</returns>
        public int ReadInt(int bits)
        {
            if ((bits < 1) || (bits > 32))
                throw new ArgumentOutOfRangeException("bits", bits, "Invalid number of bits requested");

            byte[] bytes = new byte[4];

            int bitsToRead = bits;
            if (bitsToRead > 8)
                bitsToRead = 8;
            if (bitsToRead > 0)
                bytes[0] = ReadByte(bitsToRead);

            bitsToRead = bits - 8;
            if (bitsToRead > 8)
                bitsToRead = 8;
            if (bitsToRead > 0)
                bytes[1] = ReadByte(bitsToRead);

            bitsToRead = bits - 16;
            if (bitsToRead > 8)
                bitsToRead = 8;
            if (bitsToRead > 0)
                bytes[2] = ReadByte(bitsToRead);

            bitsToRead = bits - 24;
            if (bitsToRead > 8)
                bitsToRead = 8;
            if (bitsToRead > 0)
                bytes[3] = ReadByte(bitsToRead);

            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// Reads a single bit from the byte stream and returns the bool
        /// value of the bit.
        /// </summary>
        /// <returns></returns>
        public bool ReadBool()
        {
            return (ReadByte(1) == 1);
        }

        /// <summary>
        /// Shortcut for reading a full byte.
        /// </summary>
        /// <param name="value">Value read will be returned in this variable.</param>
        public byte ReadByte()
        {
            return ReadByte(8);
        }

        /// <summary>
        /// Shortcut for reading a full int16.
        /// </summary>
        /// <param name="value">Value read will be returned in this variable.</param>
        public short ReadShort()
        {
            return ReadShort(16);
        }

        /// <summary>
        /// Shortcut for reading a full int32.
        /// </summary>
        /// <param name="value">Value read will be returned in this variable.</param>
        public int ReadInt()
        {
            return ReadInt(32);
        }

        #endregion
    }
}
