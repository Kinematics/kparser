using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using WaywardGamers.KParser.Parsing;

namespace WaywardGamers.KParser
{
    public class MMHook
    {
        // Allow direct injection of a message line to see how it behaves.
        [System.Diagnostics.Conditional("DEBUG")]
        public static void Hook(string msg)
        {
            ChatLine cl = new ChatLine(msg);
            MessageManager.Instance.AddChatLine(cl);
        }
    }

    internal class MessageManager
    {
        #region Singleton Construction
        /// <summary>
        /// Make the class a singleton
        /// </summary>
        private static readonly MessageManager instance = new MessageManager();

         /// <summary>
        /// Private constructor ensures singleton purity.
        /// </summary>
        private MessageManager()
		{
        }

		/// <summary>
        /// Gets the singleton instance of the NewMessageManager class.
		/// </summary>
        public static MessageManager Instance
		{
			get
			{
				return instance;
			}
        }
        #endregion

        #region Member Variables
        List<Message> messageCollection = new List<Message>();
        Dictionary<string, EntityType> entityCollection = new Dictionary<string, EntityType>();
        internal string LastAddedPetEntity { get; set; }

        List<Message> pendingCollection = new List<Message>();

        Timer periodicUpdates;

        bool prepared;

        Properties.Settings programSettings = new WaywardGamers.KParser.Properties.Settings();
        #endregion

        #region Parsing state control methods
        internal void PrepareToStartParsing()
        {
            if (periodicUpdates == null)
            {
                Reset();
                DumpToFile(null, true, false);
                prepared = true;
            }
        }

        /// <summary>
        /// Activate the timer so that we periodically pass accumulated messages
        /// on to the DatabaseManager to be committed to the database.
        /// </summary>
        internal void StartParsing(bool activateTimer)
        {
            if (prepared == false)
                Reset();

            programSettings.Reload();

            if (activateTimer == true)
            {
                if (periodicUpdates == null)
                    periodicUpdates = new Timer(ProcessMessageList, null, 3000, 3000);
            }

            if (prepared == false)
                DumpToFile(null, true, false);
        }

        /// <summary>
        /// Deactivate the timer so that we're no longer passing on updates.
        /// Send all currently accumulated messages directly to the database.
        /// </summary>
        internal void StopParsing()
        {
            if (periodicUpdates != null)
            {
                periodicUpdates.Dispose();
                periodicUpdates = null;
            }

            ProcessMessageList(true);
            prepared = false;
        }

        /// <summary>
        /// Deactivate the timer, and don't send accumulated messages to
        /// the database.
        /// </summary>
        internal void CancelParsing()
        {
            if (periodicUpdates != null)
            {
                periodicUpdates.Dispose();
                periodicUpdates = null;
            }

            prepared = false;
        }

        /// <summary>
        /// Function to reset the state of the manager to empty.
        /// </summary>
        private void Reset()
        {
            lock (messageCollection)
            {
                messageCollection.Clear();
            }

            entityCollection.Clear();
            LastMessageEventNumber = 0;
            LastAddedPetEntity = string.Empty;

            prepared = false;
        }
        #endregion

        #region Properties
        internal uint LastMessageEventNumber { get; private set; }
        #endregion

        #region Message management
        /// <summary>
        /// Given a new chatline, creates a new MessageLine and adds that
        /// MessageLine to the known messages groupings.  This is only called
        /// by the Monitor classes.
        /// </summary>
        /// <param name="chatLine">The chatline to add to the message collection.</param>
        internal void AddChatLine(ChatLine chatLine)
        {
            MessageLine messageLine = null;

            try
            {
                // Create a message line that tokenizes the individual text fields.
                messageLine = new MessageLine(chatLine);

                // Add the chat line directly to the database before starting to parse.
                DatabaseManager.Instance.AddChatLineToRecordLog(chatLine);

                Message msg = Parser.Parse(messageLine);

                UpdateEntityCollection(msg);


                // Special handling for death messages where one might be a pet
                if (msg.EventDetails != null)
                {
                    if (msg.EventDetails.CombatDetails != null)
                    {
                        if (msg.EventDetails.CombatDetails.FlagPetDeath == true)
                        {
                            // If the message is flagged for pending, store it and move on
                            pendingCollection.Add(msg);
                            return;
                        }
                    }
                }

                // If we've built up any pending messages, look for xp reward
                // messages.  That marks the death as death of mob, so mark
                // actor of pending message as pet.
                if (pendingCollection.Count > 0)
                {
                    if (msg.EventDetails != null)
                    {
                        if (msg.EventDetails.EventMessageType == EventMessageType.Experience)
                        {
                            var pendingDeath = pendingCollection.First();
                            pendingCollection = pendingCollection.Skip(1).ToList();

                            pendingDeath.EventDetails.CombatDetails.ActorEntityType = EntityType.Pet;

                            lock (messageCollection)
                            {
                                if (messageCollection.Contains(pendingDeath) == false)
                                {
                                    messageCollection.Add(pendingDeath);
                                }
                            }
                        }
                    }

                    // If pending messages are still lying around 5+ seconds after
                    // originally sent, assume it was a mob killing a pet instead, and mark
                    // them as such.
                    if (pendingCollection.Count > 0)
                    {
                        var oldPending = pendingCollection.Where(m => m.Timestamp < msg.Timestamp.AddSeconds(-5)).ToList();

                        foreach (var pending in oldPending)
                        {
                            foreach (var target in pending.EventDetails.CombatDetails.Targets)
                            {
                                target.EntityType = EntityType.Pet;
                            }

                            lock (messageCollection)
                            {
                                if (messageCollection.Contains(pending) == false)
                                {
                                    messageCollection.Add(pending);
                                }
                            }

                            pendingCollection.Remove(pending);
                        }
                    }
                }

                // Done with pending messages; add regular messages to the normal queue.
                lock (messageCollection)
                {
                    if (messageCollection.Contains(msg) == false)
                    {
                        messageCollection.Add(msg);
                        LastMessageEventNumber = msg.MessageID;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e, messageLine);
            }
        }

        /// <summary>
        /// TODO: fix this
        /// </summary>
        /// <param name="eventNumber"></param>
        /// <returns></returns>
        internal Message FindMessageWithEventNumber(uint eventNumber)
        {
            Message msg = null;

            lock (messageCollection)
            {
                // Reverse search the collection list
                msg = messageCollection.LastOrDefault(m => m.MessageID == eventNumber);
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

            if (messageCollection.Count == 0)
                return null;

            Message msg = null;
            // Don't attach to message that are too far back in time
            DateTime minTimestamp = timestamp - TimeSpan.FromSeconds(10);

            lock (messageCollection)
            {
                // Search the last 50 messages of the collection (restricted in case of reparsing)
                int startIndex = messageCollection.Count - 50;
                if (startIndex < 0)
                    startIndex = 0;

                var searchSet = messageCollection.Skip(startIndex);
                Message lastMessage = messageCollection.Last();
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

        /// <summary>
        /// Go through the messages in the messageCollection and send excess
        /// and older messages to the database for storage.
        /// </summary>
        /// <param name="stateInfo">This is a parameter passed in by the Timer.
        /// When the timer fires, it passes in a null.  When the parse is ending,
        /// this function gets called with a bool value for notification.</param>
        private void ProcessMessageList(Object stateInfo)
        {
            try
            {
                bool dumpDebugToFile = programSettings.DebugMode;
#if DEBUG
                dumpDebugToFile = true;
#endif

                bool parseEnding = false;
                if (stateInfo != null)
                    parseEnding = (bool)stateInfo;

                // If we're in RAM mode, take anything more than 5 seconds old.
                DateTime shortCheckTime = DateTime.Now - TimeSpan.FromSeconds(5);
                // If we're in LOG mode, leave the last 10 messages with the same timestamp
                // for at least 2 minutes in case of log file cross-over.
                DateTime longCheckTime = DateTime.Now - TimeSpan.FromMinutes(2);

                List<Message> messagesToProcess = new List<Message>();

                // --
                // Leave the 20 most recent messages if those messages all have the
                // same timestamp (ie: read from Log) to allow time for a new log
                // file dump to happen, in case of partial messages folding across
                // log files.
                //
                // Otherwise process all messages more than 5 seconds old.
                //
                // In either case, process all messages more than 2 minutes old.
                // --


                switch (Monitoring.Monitor.ParseMode)
                {
                    case DataSource.Ram:
                        lock (messageCollection)
                        {
                            messagesToProcess.AddRange(messageCollection.FindAll(m => m.Timestamp.CompareTo(shortCheckTime) < 0));

                            // Remove those messages from the list.
                            messageCollection.RemoveAll(m => m.Timestamp.CompareTo(shortCheckTime) < 0);
                        }
                        break;
                    case DataSource.Log:
                        // If we're in LOG mode, leave the last 10 messages with the same timestamp
                        // for at least 2 minutes in case of log file cross-over.
                        lock (messageCollection)
                        {
                            if (messageCollection.Count > 0)
                            {
                                Message lastMessage = messageCollection.Last();
                                int numberOfMessagesToLeave = 0;

                                // But only if they haven't crossed the long time threshhold (2 minutes)
                                if (lastMessage.Timestamp.CompareTo(longCheckTime) >= 0)
                                    numberOfMessagesToLeave = messageCollection.Count(m => m.Timestamp == lastMessage.Timestamp);

                                // cap at 10
                                if (numberOfMessagesToLeave > 20)
                                    numberOfMessagesToLeave = 20;

                                int numberOfMessagesToTake = messageCollection.Count() - numberOfMessagesToLeave;

                                if (numberOfMessagesToTake < 0)
                                    numberOfMessagesToTake = 0;

                                if (numberOfMessagesToTake > 0)
                                {
                                    messagesToProcess.AddRange(messageCollection.Take(numberOfMessagesToTake));
                                    messageCollection.RemoveRange(0, numberOfMessagesToTake);
                                }
                            }
                        }
                        break;
                    case DataSource.Database:
                        // In database reading mode, the messages are going to come very quickly
                        // Leave the last 10 messages always.  When the re-parse ends, the
                        // code below will clean up the leftovers.
                        lock (messageCollection)
                        {
                            if (messageCollection.Count > 10)
                            {
                                messagesToProcess.AddRange(messageCollection.GetRange(0, messageCollection.Count - 10));
                                messageCollection.RemoveRange(0, messageCollection.Count - 10);
                            }
                        }
                        break;
                }


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
                        if (dumpDebugToFile == true)
                            DumpToFile(messagesToProcess, false, false);
                    }
                }

                // If we're done parsing, send all remaining messages to the database as well
                if (parseEnding == true)
                {
                    lock (messageCollection)
                    {
                        try
                        {
                            DatabaseManager.Instance.ProcessNewMessages(messageCollection, true);
                        }
                        finally
                        {
                            // Save dump of that data if debug flag is set.  Save out entities
                            // at the end as well.
                            if (dumpDebugToFile == true)
                                DumpToFile(messageCollection, false, true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e);
            }
        }
        #endregion

        #region Entity management
        /// <summary>
        /// Maintain a listing of entity types for combatants so that we
        /// don't have to continually reevaluate them at the message level.
        /// </summary>
        /// <param name="message">The message to add to the entity collection.</param>
        private void UpdateEntityCollection(Message msg)
        {
            if (msg == null)
                return;

            if (msg.EventDetails == null)
                return;

            if (msg.EventDetails.CombatDetails == null)
                return;

            // Update base actor name
            string name = msg.EventDetails.CombatDetails.ActorName;
            bool update = true;

            if (name != string.Empty)
            {
                if (entityCollection.ContainsKey(name))
                {
                    if (entityCollection[name] != EntityType.Unknown)
                    {
                        // If a chamed mob name has been entered as a pet, and we receive notice
                        // of that same named mob as a mob, enter it as a mob and add the special
                        // version name as a pet.
                        if ((entityCollection[name] == EntityType.Pet) &&
                            (msg.EventDetails.CombatDetails.ActorEntityType == EntityType.Mob))
                        {
                            entityCollection[name] = EntityType.Mob;
                            AddPetEntity(name);
                        }
                        else if ((entityCollection[name] == EntityType.Mob) &&
                            (msg.EventDetails.CombatDetails.ActorEntityType == EntityType.Pet))
                        {
                            AddPetEntity(name);
                        }

                        update = false;
                    }
                }

                if (update == true)
                    entityCollection[name] = msg.EventDetails.CombatDetails.ActorEntityType;
            }

            // Update all target names
            foreach (TargetDetails target in msg.EventDetails.CombatDetails.Targets)
            {
                name = target.Name;
                update = true;

                if ((name != null) && (name != string.Empty))
                {
                    if (entityCollection.ContainsKey(name))
                    {
                        if (entityCollection[name] != EntityType.Unknown)
                        {
                            // If a chamed mob name has been entered as a pet, and we receive notice
                            // of that same named mob as a mob, enter it as a mob and add the special
                            // version name as a pet.
                            if ((entityCollection[name] == EntityType.Pet) &&
                                (target.EntityType == EntityType.Mob))
                            {
                                entityCollection[name] = EntityType.Mob;
                                AddPetEntity(name);
                            }
                            else if ((entityCollection[name] == EntityType.Mob) &&
                                     (target.EntityType == EntityType.Pet))
                            {
                                AddPetEntity(name);
                            }

                            update = false;
                        }
                    }

                    if (update == true)
                        entityCollection[name] = target.EntityType;
                }
            }
        }

        /// <summary>
        /// Explicitly add an entity name as a pet.  Called when the parse
        /// encounters a successful Charm attempt.  This adds a _Pet modifier
        /// to the lookup name to distinguish between pets and normal mobs.
        /// </summary>
        /// <param name="name">The name of the mob that was charmed.</param>
        internal void AddPetEntity(string name)
        {
            if ((name == null) || (name == string.Empty))
                return;

            string petName = name + "_Pet";
            LastAddedPetEntity = name;

            if (entityCollection.ContainsKey(petName) == false)
            {
                entityCollection[petName] = EntityType.Pet;
            }
        }

        /// <summary>
        /// Explicitly remove an entity name as a pet.  Called when the parse
        /// encounters a "charm wore off" message.
        /// </summary>
        /// <param name="name">The name of the mob that was charmed.</param>
        internal void RemovePetEntity(string name)
        {
            if ((name == null) || (name == string.Empty))
                return;

            string petName = name + "_Pet";

            if (entityCollection.ContainsKey(petName))
                entityCollection.Remove(petName);
        }

        /// <summary>
        /// Check to see if we've encountered the named combatant before.
        /// If so, use the entity type we got last time.  This checks for
        /// _Pets -after- normal mob name lookup.
        /// </summary>
        /// <param name="name">The name of the combatant to look up.</param>
        /// <returns>The entity type for the name provided, if available.</returns>
        internal EntityType LookupEntity(string name)
        {
            if ((name == null) || (name == string.Empty))
                return EntityType.Unknown;

            if (entityCollection.ContainsKey(name))
                return entityCollection[name];

            if (entityCollection.ContainsKey(name + "_Pet"))
                return EntityType.Pet;

            return EntityType.Unknown;
        }

        /// <summary>
        /// Check to see if we've encountered the named combatant before.
        /// If so, use the entity type we got last time.  This checks for
        /// _Pets -before- normal mob name lookup.
        /// </summary>
        /// <param name="name">The name of the combatant to look up.</param>
        /// <returns>The entity type for the name provided, if available.</returns>
        internal EntityType LookupPetEntity(string name)
        {
            if ((name == null) || (name == string.Empty))
                return EntityType.Unknown;

            if (entityCollection.ContainsKey(name + "_Pet"))
                return EntityType.Pet;

            if (entityCollection.ContainsKey(name))
                return entityCollection[name];

            return EntityType.Unknown;
        }

        internal void OverridePlayerToMob(string name)
        {
            if (entityCollection.ContainsKey(name))
            {
                if (entityCollection[name] == EntityType.Player)
                    entityCollection[name] = EntityType.Mob;
            }
        }
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

                    foreach (var entity in entityCollection)
                    {
                        sw.WriteLine(string.Format("{0}{1}", entity.Key.PadRight(32), entity.Value));
                    }

                    sw.WriteLine("".PadRight(42, '-'));
                    sw.WriteLine();
                }
            }
        }
        #endregion
    }
}
