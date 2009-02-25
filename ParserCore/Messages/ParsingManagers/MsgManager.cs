using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using WaywardGamers.KParser.Interface;

namespace WaywardGamers.KParser.Parsing
{
    internal class MsgManager
    {
        #region Private enum for debugging purposes
        private enum DebugDumpMode
        {
            Init,
            Normal,
            Complete
        }
        #endregion

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
            periodicUpdates = new System.Timers.Timer(5000.0);
            periodicUpdates.AutoReset = true;
            periodicUpdates.Elapsed += new ElapsedEventHandler(periodicUpdates_Elapsed);
            periodicUpdates.Enabled = false;

            Monitoring.Monitor.Instance.ReaderDataChanged += ChatLinesListener;
        }
        #endregion

        #region Member variables
        List<Message> currentMessageCollection = new List<Message>();
        List<Message> pendingDeathsCollection = new List<Message>();

        string debugOutputFileName;
        bool dumpDebugDataToFile;

        System.Timers.Timer periodicUpdates;
        object periodicUpdateLock = new object();
        #endregion

        #region Properties
        internal uint LastMessageEventNumber { get; private set; }
        #endregion

        #region Public control methods
        /// <summary>
        /// Notify the MsgManager to start listening to the
        /// supplied IReader for new chat message lines.
        /// </summary>
        /// <param name="reader">The reader to listen to.</param>
        internal void StartNewSession()
        {
            Reset();

            // When we start listening to a new reader, note whether
            // we should be dumping the data to the debug output file.
            Properties.Settings appSettings = new WaywardGamers.KParser.Properties.Settings();
            dumpDebugDataToFile = appSettings.DebugMode;
#if DEBUG
            dumpDebugDataToFile = true;
#endif

            debugOutputFileName = Path.Combine(
                appSettings.DefaultParseSaveDirectory, "debugOutput.txt");

            periodicUpdates.Start();
        }

        /// <summary>
        /// Notify the MsgManager that it no longer needs to listen
        /// to the given IReader.
        /// </summary>
        /// <param name="reader">The reader to stop listening to.</param>
        internal void EndSession()
        {
            periodicUpdates.Stop();

            ProcessRemainingMessages();
        }

        /// <summary>
        /// Have the MsgManager reset its internal state.
        /// </summary>
        private void Reset()
        {
            // Clear the message collections
            lock (currentMessageCollection)
            {
                currentMessageCollection.Clear();
            }

            lock (pendingDeathsCollection)
            {
                pendingDeathsCollection.Clear();
            }

            // Notify the EntityManager to reset as well.
            EntityManager.Instance.Reset();
        }
        #endregion

        #region Event listeners
        /// <summary>
        /// Listener function for when messages have been collected by
        /// the reader class.  Takes the collected messages, parses them,
        /// and adds them to the queue to be stored in the database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void ChatLinesListener(object sender, ReaderDataEventArgs e)
        {
            if (periodicUpdates.Enabled == false)
                return;

            MessageLine messageLine = null;

            foreach (ChatLine chat in e.ChatLines)
            {
                try
                {
                    // Create a message line to extract the embedded data
                    // from the raw text chat line.  Run this first to
                    // weed out invalid/borked data.
                    messageLine = new MessageLine(chat);

                    // Add the chat line directly to the database before starting to parse.
                    DatabaseManager.Instance.AddChatLineToRecordLog(chat);

                    // Create a message based on parsing the message line.
                    Message msg = Parser.Parse(messageLine);

                    // Have the EntityManager update its entity list from
                    // the Message.
                    EntityManager.Instance.AddEntitiesFromMessage(msg);

                    // Deal with issues of determining if a mob dying is a 
                    // real mob or a pet.
                    HandlePossiblePetDeaths(msg);

                    // Add processed messages to the collection that periodically
                    // gets sent to the database manager.
                    lock (currentMessageCollection)
                    {
                        if (currentMessageCollection.Contains(msg) == false)
                        {
                            currentMessageCollection.Add(msg);
                            LastMessageEventNumber = msg.MessageID;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Log(ex, messageLine);
                }
            }
        }

        #endregion

        #region Private methods for dealing with incoming messages

        /// <summary>
        /// Function for dealing with uncertainties about death messages
        /// that may be pets or mobs.
        /// </summary>
        /// <param name="msg"></param>
        private void HandlePossiblePetDeaths(Message msg)
        {

            // Special handling for death messages where one might be a pet
            if (msg.EventDetails != null)
            {
                if (msg.EventDetails.CombatDetails != null)
                {
                    if (msg.EventDetails.CombatDetails.FlagPetDeath == true)
                    {
                        // If the message is flagged for pending, store it and move on
                        pendingDeathsCollection.Add(msg);
                        return;
                    }
                }
            }

            // If we've built up any pending messages, look for xp reward
            // messages.  That marks the death as death of mob, so mark
            // actor of pending message as pet.
            if (pendingDeathsCollection.Count > 0)
            {
                if (msg.EventDetails != null)
                {
                    if (msg.EventDetails.EventMessageType == EventMessageType.Experience)
                    {
                        var pendingDeath = pendingDeathsCollection.First();
                        pendingDeathsCollection = pendingDeathsCollection.Skip(1).ToList();

                        pendingDeath.EventDetails.CombatDetails.ActorEntityType = EntityType.Pet;

                        lock (currentMessageCollection)
                        {
                            if (currentMessageCollection.Contains(pendingDeath) == false)
                            {
                                currentMessageCollection.Add(pendingDeath);
                            }
                        }
                    }
                }

                // If pending messages are still lying around 5+ seconds after
                // originally sent, assume it was a mob killing a pet instead, and mark
                // them as such.
                if (pendingDeathsCollection.Count > 0)
                {
                    var oldPending = pendingDeathsCollection
                        .Where(m => m.Timestamp < msg.Timestamp.AddSeconds(-5))
                        .ToList();

                    foreach (var pending in oldPending)
                    {
                        foreach (var target in pending.EventDetails.CombatDetails.Targets)
                        {
                            target.EntityType = EntityType.Pet;
                        }

                        lock (currentMessageCollection)
                        {
                            if (currentMessageCollection.Contains(pending) == false)
                            {
                                currentMessageCollection.Add(pending);
                            }
                        }

                        pendingDeathsCollection.Remove(pending);
                    }
                }
            }
        }

        #endregion

        #region Methods for querying the message collection
        /// <summary>
        /// Try to locate the most recent message in the collection that
        /// has the given event number value.
        /// </summary>
        /// <param name="eventNumber">The event number to locate.</param>
        /// <returns>Returns the message if found, or null if not.</returns>
        internal Message FindMessageWithEventNumber(uint eventNumber)
        {
            Message msg = null;

            lock (currentMessageCollection)
            {
                // Reverse search the collection list
                msg = currentMessageCollection.LastOrDefault(m => m.MessageID == eventNumber);
            }

            return msg;
        }

        /// <summary>
        /// Locate a recent message in the message collection given a set of code parameters.
        /// </summary>
        /// <param name="messageLine">The original text line.</param>
        /// <param name="altCodes">A list of alternate primary codes to search for.</param>
        /// <param name="parsedMessage">The message as it was previously parsed.</param>
        /// <returns>Returns the most recent found message that matches the request.</returns>
        internal Message FindLastMessageToMatch(MessageLine messageLine, List<uint> altCodes, Message parsedMessage)
        {
            uint mcode = messageLine.MessageCode;
            uint ecode1 = messageLine.ExtraCode1;
            uint ecode2 = messageLine.ExtraCode2;
            DateTime timestamp = messageLine.Timestamp;
            //List<uint> altCodes = null;

            if (mcode == 0)
                throw new ArgumentOutOfRangeException("mcode", "No proper message code provided.");

            if (currentMessageCollection.Count == 0)
                return null;

            Message msg = null;
            // Don't attach to message that are too far back in time
            DateTime minTimestamp = timestamp - TimeSpan.FromSeconds(10);

            lock (currentMessageCollection)
            {
                // Search the last 50 messages of the collection (restricted in case of reparsing)
                int startIndex = currentMessageCollection.Count - 50;
                if (startIndex < 0)
                    startIndex = 0;

                var searchSet = currentMessageCollection.Skip(startIndex);
                Message lastMessage = currentMessageCollection.Last();
                // Check for lastTimestamp in case we're reading from logs
                // where all messages from 50 message blocks will have the same
                // timestamp, then an unknown interval before the next block.
                DateTime lastTimestamp = lastMessage.Timestamp;

                if ((ecode1 != 0) && (ecode2 != 0))
                {
                    // If we have sub codes, include those in the first pass search
                    msg = searchSet.LastOrDefault(m =>
                        ((m.PrimaryMessageCode == mcode) &&
                         (m.ExtraCode1 == ecode1) &&
                         (m.ExtraCode2 == ecode2) &&
                         (m.EventDetails != null) &&
                         ((m.Timestamp >= minTimestamp) || (m.Timestamp == lastTimestamp)) &&
                         (m.EventDetails.CombatDetails != null) &&
                         (m.EventDetails.CombatDetails.ActorName != string.Empty) &&
                         (
                          (m.EventDetails.CombatDetails.Targets.Count == 0) ||
                          (
                           (parsedMessage == null) ||
                           (
                            (parsedMessage.EventDetails.CombatDetails.Targets.Count > 0) &&
                            (m.EventDetails.CombatDetails.Targets.Any(t =>
                               t.Name == parsedMessage.EventDetails.CombatDetails.Targets.First().Name) == false) &&
                            ((m.EventDetails.CombatDetails.Targets.Any(t =>
                               t.EffectName == parsedMessage.EventDetails.CombatDetails.Targets.First().EffectName) == true) ||
                             (parsedMessage.EventDetails.CombatDetails.Targets.First().EffectName == string.Empty)) &&
                            (m.EventDetails.CombatDetails.Targets.Any(t =>
                               t.EntityType == parsedMessage.EventDetails.CombatDetails.Targets.First().EntityType) == true) &&
                            ((parsedMessage.EventDetails.CombatDetails.Targets.First().RecoveryType == RecoveryType.None) ||
                             (m.EventDetails.CombatDetails.Targets.Any(t =>
                               t.RecoveryType == parsedMessage.EventDetails.CombatDetails.Targets.First().RecoveryType))
                            )
                           )
                          )
                         ) &&
                         (m.EventDetails.CombatDetails.HasAdditionalEffect == false)));
                }

                if (msg == null)
                {
                    // If we're given ecodes of 0, or weren't able to find
                    // the specified ecodes, try again.  Make sure we don't
                    // find a message that has ecodes of 0, since we can't
                    // attach to that.
                    msg = searchSet.LastOrDefault(m =>
                        ((m.PrimaryMessageCode == mcode) &&
                         ((m.ExtraCode1 != 0) || (m.ExtraCode2 != 0)) &&
                         (m.EventDetails != null) &&
                         ((m.Timestamp >= minTimestamp) || (m.Timestamp == lastTimestamp)) &&
                         (m.EventDetails.CombatDetails != null) &&
                         (m.EventDetails.CombatDetails.ActorName != string.Empty) &&
                         (
                          (m.EventDetails.CombatDetails.Targets.Count == 0) ||
                          (
                           (parsedMessage == null) ||
                           (
                            (parsedMessage.EventDetails.CombatDetails.Targets.Count > 0) &&
                            (m.EventDetails.CombatDetails.Targets.Any(t =>
                               t.Name == parsedMessage.EventDetails.CombatDetails.Targets.First().Name) == false) &&
                            ((m.EventDetails.CombatDetails.Targets.Any(t =>
                               t.EffectName == parsedMessage.EventDetails.CombatDetails.Targets.First().EffectName) == true) ||
                             (parsedMessage.EventDetails.CombatDetails.Targets.First().EffectName == string.Empty)) &&
                            (m.EventDetails.CombatDetails.Targets.Any(t =>
                               t.EntityType == parsedMessage.EventDetails.CombatDetails.Targets.First().EntityType) == true) &&
                            ((parsedMessage.EventDetails.CombatDetails.Targets.First().RecoveryType == RecoveryType.None) ||
                             (m.EventDetails.CombatDetails.Targets.Any(t =>
                               t.RecoveryType == parsedMessage.EventDetails.CombatDetails.Targets.First().RecoveryType))
                            )
                           )
                          )
                         ) &&
                         (m.EventDetails.CombatDetails.HasAdditionalEffect == false)));
                }

                if (msg == null)
                {
                    // If no message found, and we're given an altCode list, check each of those.
                    if (altCodes != null)
                    {
                        // If checking alt codes, we cannot expect the ecodes to match,
                        // therefore don't bother checking against them (but still make
                        // sure they aren't 0).

                        // First run through, give preference to prior messages with targets
                        // that have the same EffectName as the current one.

                        msg = searchSet.LastOrDefault(m =>
                            ((altCodes.Contains(m.PrimaryMessageCode)) &&
                             (m.EventDetails != null) &&
                             (m.EventDetails.CombatDetails != null) &&
                             ((m.ExtraCode1 != 0) ||
                              (m.ExtraCode2 != 0) ||
                              (m.EventDetails.CombatDetails.AidType == AidType.Item)
                             ) &&
                             ((m.Timestamp >= minTimestamp) || (m.Timestamp == lastTimestamp)) &&
                             (m.EventDetails.CombatDetails.ActorName != string.Empty) &&
                             (
                              (
                               (parsedMessage == null) ||
                               (
                                (parsedMessage.EventDetails.CombatDetails.Targets.Count > 0) &&
                                (m.EventDetails.CombatDetails.Targets.Any(t =>
                                   t.Name == parsedMessage.EventDetails.CombatDetails.Targets.First().Name) == false) &&
                                ((m.EventDetails.CombatDetails.Targets.Any(t =>
                                   t.EffectName == parsedMessage.EventDetails.CombatDetails.Targets.First().EffectName) == true)) &&
                                (m.EventDetails.CombatDetails.Targets.Any(t =>
                                   t.EntityType == parsedMessage.EventDetails.CombatDetails.Targets.First().EntityType) == true) &&
                                ((parsedMessage.EventDetails.CombatDetails.Targets.First().RecoveryType == RecoveryType.None) ||
                                 (m.EventDetails.CombatDetails.Targets.Any(t =>
                                   t.RecoveryType == parsedMessage.EventDetails.CombatDetails.Targets.First().RecoveryType))
                                )
                               )
                              )
                             ) &&
                             (m.EventDetails.CombatDetails.HasAdditionalEffect == false)));

                        // If we didn't find one explicitly with the current effect name,
                        // go back to general search.

                        if (msg == null)
                        {
                            msg = searchSet.LastOrDefault(m =>
                                ((altCodes.Contains(m.PrimaryMessageCode)) &&
                                 (m.EventDetails != null) &&
                                 (m.EventDetails.CombatDetails != null) &&
                                 ((m.ExtraCode1 != 0) ||
                                  (m.ExtraCode2 != 0) ||
                                  (m.EventDetails.CombatDetails.AidType == AidType.Item)
                                 ) &&
                                 ((m.Timestamp >= minTimestamp) || (m.Timestamp == lastTimestamp)) &&
                                 (m.EventDetails.CombatDetails.ActorName != string.Empty) &&
                                 (
                                  (m.EventDetails.CombatDetails.Targets.Count == 0) ||
                                  (
                                   (parsedMessage == null) ||
                                   (
                                    (parsedMessage.EventDetails.CombatDetails.Targets.Count > 0) &&
                                    (m.EventDetails.CombatDetails.Targets.Any(t =>
                                       t.Name == parsedMessage.EventDetails.CombatDetails.Targets.First().Name) == false) &&
                                    ((m.EventDetails.CombatDetails.Targets.Any(t =>
                                       t.EffectName == parsedMessage.EventDetails.CombatDetails.Targets.First().EffectName) == true) ||
                                     (parsedMessage.EventDetails.CombatDetails.Targets.First().EffectName == string.Empty)) &&
                                    (m.EventDetails.CombatDetails.Targets.Any(t =>
                                       t.EntityType == parsedMessage.EventDetails.CombatDetails.Targets.First().EntityType) == true) &&
                                    ((parsedMessage.EventDetails.CombatDetails.Targets.First().RecoveryType == RecoveryType.None) ||
                                     (m.EventDetails.CombatDetails.Targets.Any(t =>
                                       t.RecoveryType == parsedMessage.EventDetails.CombatDetails.Targets.First().RecoveryType))
                                    )
                                   )
                                  )
                                 ) &&
                                 (m.EventDetails.CombatDetails.HasAdditionalEffect == false)));

                        }

                        if (msg == null)
                        {
                            // If altCodes contains item use, open search window to up to 30 seconds.
                            if ((altCodes.Contains(0x51)) || (altCodes.Contains(0x55)))
                            {
                                minTimestamp = timestamp - TimeSpan.FromSeconds(30);

                                // If checking alt codes, we cannot expect the ecodes to match,
                                // therefore don't bother checking against them (but still make
                                // sure they aren't 0).

                                msg = searchSet.LastOrDefault(m =>
                                    ((altCodes.Contains(m.PrimaryMessageCode)) &&
                                     (m.EventDetails != null) &&
                                     (m.EventDetails.CombatDetails != null) &&
                                     ((m.ExtraCode1 != 0) ||
                                      (m.ExtraCode2 != 0) ||
                                      (m.EventDetails.CombatDetails.AidType == AidType.Item)
                                     ) &&
                                     ((m.Timestamp >= minTimestamp) || (m.Timestamp == lastTimestamp)) &&
                                     (m.EventDetails.CombatDetails.ActorName != string.Empty) &&
                                     (
                                      (m.EventDetails.CombatDetails.Targets.Count == 0) ||
                                      (
                                       (parsedMessage == null) ||
                                       (
                                        (parsedMessage.EventDetails.CombatDetails.Targets.Count > 0) &&
                                        (m.EventDetails.CombatDetails.Targets.Any(t =>
                                           t.Name == parsedMessage.EventDetails.CombatDetails.Targets.First().Name) == false) &&
                                        ((m.EventDetails.CombatDetails.Targets.Any(t =>
                                           t.EffectName == parsedMessage.EventDetails.CombatDetails.Targets.First().EffectName) == true) ||
                                         (parsedMessage.EventDetails.CombatDetails.Targets.First().EffectName == string.Empty)) &&
                                        (m.EventDetails.CombatDetails.Targets.Any(t =>
                                           t.EntityType == parsedMessage.EventDetails.CombatDetails.Targets.First().EntityType) == true) &&
                                        ((parsedMessage.EventDetails.CombatDetails.Targets.First().RecoveryType == RecoveryType.None) ||
                                         (m.EventDetails.CombatDetails.Targets.Any(t =>
                                           t.RecoveryType == parsedMessage.EventDetails.CombatDetails.Targets.First().RecoveryType))
                                        )
                                       )
                                      )
                                     ) &&
                                     (m.EventDetails.CombatDetails.HasAdditionalEffect == false)));

                            }
                        }
                    }
                }

                // No normal message search found a match, and no alt codes
                // found a match.  Do one last check to match the msg code,
                // but allow the ecodes to be anything.
                if (msg == null)
                {
                    // Searching for multiple targets of -ga damage.  May have
                    // the same name if they're mobs.
                    if (parsedMessage != null)
                    {
                        if (parsedMessage.EventDetails != null)
                        {
                            if (parsedMessage.EventDetails.CombatDetails != null)
                            {
                                if (parsedMessage.EventDetails.CombatDetails.Targets != null)
                                {
                                    if ((ecode1 != 0) && (ecode2 != 0))
                                    {
                                        // If we have sub codes, include those in the first pass search
                                        msg = searchSet.LastOrDefault(m =>
                                            ((m.PrimaryMessageCode == mcode) &&
                                             (m.EventDetails != null) &&
                                             ((m.Timestamp >= minTimestamp) || (m.Timestamp == lastTimestamp)) &&
                                             (m.EventDetails.CombatDetails != null) &&
                                             (m.EventDetails.CombatDetails.ActorName != string.Empty) &&
                                             (
                                              (m.EventDetails.CombatDetails.Targets.Count == 0) ||
                                              (
                                               (parsedMessage == null) ||
                                               (
                                                (parsedMessage.EventDetails.CombatDetails.Targets.Count > 0) &&
                                                ((m.EventDetails.CombatDetails.Targets.Any(t =>
                                                   t.EffectName == parsedMessage.EventDetails.CombatDetails.Targets.First().EffectName) == true) ||
                                                 (parsedMessage.EventDetails.CombatDetails.Targets.First().EffectName == string.Empty)) &&
                                                (m.EventDetails.CombatDetails.Targets.Any(t =>
                                                   t.EntityType == parsedMessage.EventDetails.CombatDetails.Targets.First().EntityType) == true) &&
                                                ((parsedMessage.EventDetails.CombatDetails.Targets.First().RecoveryType == RecoveryType.None) ||
                                                 (m.EventDetails.CombatDetails.Targets.Any(t =>
                                                   t.RecoveryType == parsedMessage.EventDetails.CombatDetails.Targets.First().RecoveryType))
                                                )
                                               )
                                              )
                                             ) &&
                                             (m.EventDetails.CombatDetails.HasAdditionalEffect == false)));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return msg;
        }

        #endregion

        #region Methods to periodically push the accumulated messages to the database.

        /// <summary>
        /// Go through the messages in the messageCollection and send excess
        /// and older messages to the database for storage.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void periodicUpdates_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Since it's possible that this could be called multiple
            // times by the timer thread before the processing is completed,
            // make sure we don't allow additional attempts through.
            if (!Monitor.TryEnter(periodicUpdateLock))
                return;

            try
            {
                List<Message> messagesToProcess = GetMessagesToProcess();

                // send those messages to the database
                if (messagesToProcess.Count > 0)
                {
                    try
                    {
                        DatabaseManager.Instance.ProcessNewMessages(messagesToProcess, false);
                    }
                    finally
                    {
                        // Save dump of that data if debug flag is set.
                        if (dumpDebugDataToFile == true)
                            DumpToFile(messagesToProcess, DebugDumpMode.Normal);
                    }
                }

            }
            finally
            {
                Monitor.Exit(periodicUpdateLock);
            }
        }

        /// <summary>
        /// Get a filtered subset of the currentMessageList to actually
        /// send to the database.
        /// </summary>
        /// <returns>Returns the list of messages that should be sent
        /// to the database based on filter conditions.</returns>
        private List<Message> GetMessagesToProcess()
        {
            // If we're in RAM mode, take anything more than 5 seconds old.
            DateTime shortCheckTime = DateTime.Now - TimeSpan.FromSeconds(5);
            // If we're in LOG mode, leave the last 10 messages with the same timestamp
            // for at least 2 minutes in case of log file cross-over.
            DateTime longCheckTime = DateTime.Now - TimeSpan.FromMinutes(2);

            List<Message> messagesToProcess = new List<Message>();

            switch (Monitoring.Monitor.Instance.ParseMode)
            {
                case DataSource.Ram:
                    lock (currentMessageCollection)
                    {
                        messagesToProcess.AddRange(currentMessageCollection.FindAll(m => m.Timestamp.CompareTo(shortCheckTime) < 0));

                        // Remove those messages from the list.
                        currentMessageCollection.RemoveAll(m => m.Timestamp.CompareTo(shortCheckTime) < 0);
                    }
                    break;
                case DataSource.Log:
                    // If we're in LOG mode, leave the last 10 messages with the same timestamp
                    // for at least 2 minutes in case of log file cross-over.
                    lock (currentMessageCollection)
                    {
                        if (currentMessageCollection.Count > 0)
                        {
                            Message lastMessage = currentMessageCollection.Last();
                            int numberOfMessagesToLeave = 0;

                            // But only if they haven't crossed the long time threshhold (2 minutes)
                            if (lastMessage.Timestamp.CompareTo(longCheckTime) >= 0)
                                numberOfMessagesToLeave = currentMessageCollection.Count(m => m.Timestamp == lastMessage.Timestamp);

                            // cap at 10
                            if (numberOfMessagesToLeave > 20)
                                numberOfMessagesToLeave = 20;

                            int numberOfMessagesToTake = currentMessageCollection.Count() - numberOfMessagesToLeave;

                            if (numberOfMessagesToTake < 0)
                                numberOfMessagesToTake = 0;

                            if (numberOfMessagesToTake > 0)
                            {
                                messagesToProcess.AddRange(currentMessageCollection.Take(numberOfMessagesToTake));
                                currentMessageCollection.RemoveRange(0, numberOfMessagesToTake);
                            }
                        }
                    }
                    break;
                case DataSource.Database:
                    // In database reading mode, the messages are going to come very quickly
                    // Leave the last 10 messages always.  When the re-parse ends, the
                    // ProcessRemainingMessages method will clean up the leftovers.
                    lock (currentMessageCollection)
                    {
                        if (currentMessageCollection.Count > 10)
                        {
                            messagesToProcess.AddRange(currentMessageCollection.GetRange(0, currentMessageCollection.Count - 10));
                            currentMessageCollection.RemoveRange(0, currentMessageCollection.Count - 10);
                        }
                    }
                    break;
            }

            return messagesToProcess;
        }

        /// <summary>
        /// Send all remaining messages from the currentMessageList to the
        /// database.
        /// </summary>
        private void ProcessRemainingMessages()
        {
            // This locks while waiting for the timer thread function
            // (periodicUpdates_Elapsed) to complete its partial update
            // before completing any remaining messages.
            Monitor.Enter(periodicUpdateLock);

            try
            {
                lock (currentMessageCollection)
                {
                    try
                    {
                        DatabaseManager.Instance.ProcessNewMessages(currentMessageCollection, true);
                    }
                    finally
                    {
                        // Save dump of that data if debug flag is set.  Save out entities
                        // at the end as well.
                        if (dumpDebugDataToFile == true)
                            DumpToFile(currentMessageCollection, DebugDumpMode.Complete);
                    }
                }
            }
            finally
            {
                Monitor.Exit(periodicUpdateLock);
            }
        }
        #endregion

        #region Debug output
        private void DumpToFile(List<Message> messagesToDump, DebugDumpMode dumpMode)
        {
            if (messagesToDump == null)
                throw new ArgumentNullException("messagesToDump");

            if (dumpMode == DebugDumpMode.Init)
            {
                using (StreamWriter sw = File.CreateText(debugOutputFileName))
                {
                    foreach (Message msg in messagesToDump)
                    {
                        sw.Write(msg.ToString());
                    }
                }

                return;
            }

            // Normal mode
            using (StreamWriter sw = File.AppendText(debugOutputFileName))
            {
                foreach (Message msg in messagesToDump)
                {
                    sw.Write(msg.ToString());
                }
            }

            // Finalization -- have the EntityManager dump its data too.
            if (dumpMode == DebugDumpMode.Complete)
            {
                using (StreamWriter sw = File.AppendText(debugOutputFileName))
                {
                    EntityManager.Instance.DumpData(sw);
                }
            }
        }
        #endregion

    }
}
