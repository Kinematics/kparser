using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser.Monitoring
{
    public class PacketReader : AbstractReader
    {
        #region Singleton Constructor
        // Make the class a singleton
        private static readonly PacketReader instance = new PacketReader();

        /// <summary>
        /// Gets the singleton instance of the LogParser class.
        /// This is the only internal way to access the singleton.
        /// </summary>
        internal static PacketReader Instance { get { return instance; } }

        /// <summary>
        /// Private constructor ensures singleton purity.
        /// </summary>
        private PacketReader()
		{
            // Layout of total structure:
            // 50 offsets to new log records (short): 100 bytes
            // 50 offsets to old log offsets (short): 100 bytes
            // ChatLogInfoStruct block
            //sizeOfChatLogInfoStruct = (uint)(Marshal.SizeOf(typeof(ChatLogInfoStruct)));

            //sizeOfChatLogControlStruct = (uint)(Marshal.SizeOf(typeof(ChatLogControlStruct)));
        }
		#endregion

        public override void Stop()
        {
            throw new NotImplementedException();
        }

        public override DataSource ParseModeType
        {
            get { throw new NotImplementedException(); }
        }
    }
}
