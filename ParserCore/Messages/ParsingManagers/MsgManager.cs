using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser.Parsing
{
    internal class MsgManager
    {
        #region Singleton Construction
        /// <summary>
        /// Make the class a singleton
        /// </summary>
        private static readonly MsgManager instance = new MsgManager();

        /// <summary>
        /// Gets the singleton instance of the NewMessageManager class.
        /// </summary>
        public static MsgManager Instance { get { return instance; } }
        
        /// <summary>
        /// Private constructor ensures singleton purity.
        /// </summary>
        private MsgManager()
		{
        }
        #endregion

        #region Member variables
        List<Message> currentMessageCollection = new List<Message>();
        #endregion

        #region Public methods
        internal void StartProcessingMessages()
        {
            Monitor.AddDataListener(ChatLinesListener);
        }

        internal void StopProcessingMessages()
        {
            Monitor.RemoveDataListener(ChatLinesListener);
        }
        #endregion

        #region Event listeners
        internal void ChatLinesListener(object sender, ReaderDataEventArgs e)
        {
            MessageLine messageLine = null;

            foreach (ChatLine chat in e.ChatLines)
            {
                try
                {
                    // Add this directly to the database before starting to parse.
                    DatabaseManager.Instance.AddChatLineToRecordLog(chat);

                    // Create a message line to extract the embedded data
                    // from the raw text chat line.
                    messageLine = new MessageLine(chat);

                    // Create a message based on parsing the message line.
                    Message msg = Parser.Parse(messageLine);

                    // Have the EntityManager update its entity list from
                    // the Message.
                    EntityManager.Instance.AddEntitiesFromMessage(msg);

                }
                catch (Exception ex)
                {
                    Logger.Instance.Log(ex, messageLine);
                }
            }
        }

        #endregion

        #region Private methods
        internal void Reset()
        {
            lock (currentMessageCollection)
            {
                currentMessageCollection.Clear();
            }

            EntityManager.Instance.Reset();
        }

        #endregion

        #region Properties
        internal uint LastMessageEventNumber { get; private set; }
        #endregion


        #region Debug output
        internal void DumpToFile(List<Message> messagesToDump, bool init, bool dumpEntities)
        {
            string fileName = "debugOutput.txt";

            if (init == true)
            {
                //if (File.Exists(fileName) == true)
                //    File.Delete(fileName);

                using (StreamWriter sw = File.CreateText(fileName))
                {
                }
            }

            if (((messagesToDump == null) || (messagesToDump.Count == 0)) &&
                (dumpEntities == false))
                return;

            using (StreamWriter sw = File.AppendText(fileName))
            {
                foreach (Message msg in messagesToDump)
                {
                    sw.Write(msg.ToString());
                }

                if (dumpEntities == true)
                {
                    sw.WriteLine("".PadRight(42, '-'));
                    sw.WriteLine("Entity List\n");
                    sw.WriteLine(string.Format("{0}{1}", "Name".PadRight(32), "Type"));
                    sw.WriteLine(string.Format("{0}    {1}", "".PadRight(28, '-'), "".PadRight(10, '-')));

                    EntityManager.Instance.DumpData(sw);

                    sw.WriteLine("".PadRight(42, '-'));
                    sw.WriteLine();
                }
            }
        }
        #endregion

    }
}
