using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaywardGamers.KParser.Database
{
    /// <summary>
    /// This class is designed to handle the entry of data into the database
    /// using passed in Messages.
    /// </summary>
    internal class DatabaseEntry
    {
        #region Member Variables
        private KPDatabaseDataSet localDB = null;

        private Dictionary<KPDatabaseDataSet.BattlesRow, DateTime> activeBattleList;
        private Dictionary<string, KPDatabaseDataSet.BattlesRow> activeMobBattleList;

        private List<KPDatabaseDataSet.InteractionsRow> deferredInteractions;

        private KPDatabaseDataSet.BattlesRow lastFinishedBattle;
        private DateTime mostRecentTimestamp = MagicNumbers.MinSQLDateTime;
        #endregion

        #region Constructor
        internal DatabaseEntry()
        {
            // Initialize record-keeping variables
            activeBattleList = new Dictionary<KPDatabaseDataSet.BattlesRow, DateTime>();
            activeMobBattleList = new Dictionary<string, KPDatabaseDataSet.BattlesRow>();
            deferredInteractions = new List<KPDatabaseDataSet.InteractionsRow>();
        }

        /// <summary>
        /// Reset this class to empty tracking values.
        /// </summary>
        internal void Reset()
        {
            UpdateActiveBattleList(true);
            
            activeBattleList.Clear();
            activeMobBattleList.Clear();
            deferredInteractions.Clear();

            lastFinishedBattle = null;
            localDB = null;
            mostRecentTimestamp = MagicNumbers.MinSQLDateTime;
        }
        #endregion

        #region Callable methods to request modifications to a given database.
        /// <summary>
        /// Add a new message to the supplied database.
        /// </summary>
        /// <param name="db">The database to insert the message into.</param>
        /// <param name="message">The message to be inserted.</param>
        internal void AddMessageToDatabase(KPDatabaseDataSet db, Message message)
        {
            if (db == null)
                throw new ArgumentNullException("db");

            if (message == null)
                throw new ArgumentNullException("message");

            if (message.Timestamp > mostRecentTimestamp)
                mostRecentTimestamp = message.Timestamp;

            // Don't try to insert data from unsuccessful parses.
            // Record log already defaults to false on ParseSuccessful, so don't have to update.
            if (message.IsParseSuccessful == false)
            {
                return;
            }

            try
            {
                this.localDB = db;

                // For successful parses, update the RecordLog table entries for each messageline in the
                // message.
                foreach (var msgLine in message.MessageLineCollection)
                {
                    if (msgLine.ChatRecordID >= 0)
                    {
                        var chatLogRow = localDB.RecordLog[msgLine.ChatRecordID];
                        chatLogRow.ParseSuccessful = message.IsParseSuccessful;
                    }
                    else
                    {
                        var recordRows = localDB.RecordLog.Where(t => t.Timestamp == msgLine.Timestamp);

                        if (recordRows.Count() > 0)
                            recordRows = recordRows.Where(t => t.MessageText == msgLine.OriginalText);

                        if (recordRows.Count() > 0)
                            recordRows = recordRows.Where(t => t.ParseSuccessful == false);

                        if (recordRows.Count() > 0)
                            recordRows.First().ParseSuccessful = true;
                    }
                }



                // Call functions depending on type of message.
                switch (message.MessageCategory)
                {
                    case MessageCategoryType.Chat:
                        InsertChat(message);
                        break;
                    case MessageCategoryType.Event:
                        // If this is a loot or experience message it needs to be attached to the most recently killed battle.
                        switch (message.EventDetails.EventMessageType)
                        {
                            case EventMessageType.Experience:
                                InsertExperience(message);
                                break;
                            case EventMessageType.Loot:
                                InsertLoot(message);
                                break;
                            case EventMessageType.Steal:
                            case EventMessageType.Interaction:
                                InsertCombat(message);
                                break;
                        }
                        // Other message types (fishing, crafting, etc) are ignored.
                        break;
                    case MessageCategoryType.System:
                        // Ignore system messages
                        break;
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Log(e, message);
            }
            finally
            {
                localDB = null;
            }
        }

        /// <summary>
        /// Call this when each batch of messages is processed, to allow updating
        /// of the battle list.
        /// </summary>
        internal void MessageBatchSent()
        {
            UpdateActiveBattleList(false);
        }

        /// <summary>
        /// Update the provided database with the info supplied in the
        /// player info list.
        /// </summary>
        /// <param name="db">The database to insert the info into.</param>
        /// <param name="playerInfoList">The list of player information to update.</param>
        internal void UpdatePlayerInfo(KPDatabaseDataSet db, List<PlayerInfo> playerInfoList)
        {
            if (db == null)
                throw new ArgumentNullException("db");

            if (playerInfoList == null)
                return;

            foreach (var player in playerInfoList)
            {
                var combatantLine = db.Combatants.FirstOrDefault(c => c.CombatantName == player.Name &&
                    (EntityType)c.CombatantType == player.CombatantType);

                if (combatantLine != null)
                    combatantLine.PlayerInfo = player.Info;
            }
        }

        /// <summary>
        /// Call this function to purge all (potentially sensitive) chat
        /// info from the supplied database.
        /// </summary>
        /// <param name="db">The database to purge chat info from.</param>
        internal void PurgeChatInfo(KPDatabaseDataSet db)
        {
            if (db == null)
                throw new ArgumentNullException("db");

            // First delete all non-Arena rows in the message table.
            foreach (KPDatabaseDataSet.ChatMessagesRow row in db.ChatMessages)
            {
                if (((ChatMessageType)row.ChatType != ChatMessageType.Arena) &&
                    ((ChatMessageType)row.ChatType != ChatMessageType.Echo))
                    row.Delete();
            }

            // Then all the speakers (parent row to messages).
            foreach (KPDatabaseDataSet.ChatSpeakersRow row in db.ChatSpeakers)
            {
                if (row.GetChatMessagesRows().Count() == 0)
                    row.Delete();
            }

            // Then all chat messages from the original parse log.
            string[] chatCode = new string[] { "01", "02", "03", "04", "05", "06", "07",
                "09", "0a", "0b", "0c", "0d", "0e", "0f", "8e", "90", "98"};
            string rowChatValue;

            foreach (KPDatabaseDataSet.RecordLogRow row in db.RecordLog.Rows)
            {
                rowChatValue = row.MessageText.Substring(0, 2);
                if (chatCode.Contains(rowChatValue) == true)
                    row.Delete();
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Add chat messages to database.
        /// </summary>
        /// <param name="message">Chat message to add.</param>
        private void InsertChat(Message message)
        {
            var chatSpeakerRow = localDB.ChatSpeakers.GetSpeaker(message.ChatDetails.ChatSpeakerName);

            // If total chat line is longer than the database will accept, chop it up
            // into 256-char sequences.
            string limitChatMessage = message.CompleteMessageText;
            string frontChatMessage;
            while (limitChatMessage.Length > 256)
            {
                frontChatMessage = limitChatMessage.Substring(0, 256);
                limitChatMessage = limitChatMessage.Substring(256);

                localDB.ChatMessages.AddChatMessagesRow(message.Timestamp, (byte)message.ChatDetails.ChatMessageType,
                    chatSpeakerRow, frontChatMessage);
            }

            localDB.ChatMessages.AddChatMessagesRow(message.Timestamp, (byte)message.ChatDetails.ChatMessageType,
                 chatSpeakerRow, limitChatMessage);
        }

        /// <summary>
        /// Update battles with experience info.
        /// </summary>
        /// <param name="message">The message containing experience data.</param>
        private void InsertExperience(Message message)
        {
            if ((lastFinishedBattle == null) || (lastFinishedBattle.ExperiencePoints != 0))
            {
                lastFinishedBattle = null;

                if (activeBattleList.Count > 0)
                {
                    lastFinishedBattle = activeBattleList.FirstOrDefault(
                        b => b.Key.DefaultBattle == false &&
                            (b.Key.IsOver == false || b.Key.ExperiencePoints == 0)).Key;

                    if (lastFinishedBattle != null)
                    {
                        // close out this battle
                        if (lastFinishedBattle.IsOver == false)
                        {
                            lastFinishedBattle.EndTime = message.Timestamp;
                            lastFinishedBattle.Killed = true;
                        }

                        if (lastFinishedBattle.IsEnemyIDNull() == false)
                            activeMobBattleList.Remove(lastFinishedBattle.CombatantsRowByEnemyCombatantRelation.CombatantName);

                        lock (activeBattleList)
                            activeBattleList.Remove(lastFinishedBattle);

                    }
                }

                if (lastFinishedBattle == null)
                {
                    lastFinishedBattle = localDB.Battles.AddBattlesRow(null,
                        message.Timestamp, message.Timestamp, true,
                        null, (byte)ActorPlayerType.Unknown,
                        0, 0, // XP Points & Chain
                        (byte)MobDifficulty.Unknown, false);
                }
            }

            lastFinishedBattle.ExperiencePoints = message.EventDetails.ExperienceDetails.ExperiencePoints;
            lastFinishedBattle.ExperienceChain = message.EventDetails.ExperienceDetails.ExperienceChain;
        }

        /// <summary>
        /// Subset code for inserting loot table information.
        /// </summary>
        /// <param name="message">The message containing loot data.</param>
        private void InsertLoot(Message message)
        {
            // Messages for when items are found on mob.
            if (message.EventDetails.LootDetails.IsFoundMessage == true)
            {
                // First locate the target (mob or chest) in the combatants table
                var targetCombatant = localDB.Combatants.GetCombatant(message.EventDetails.LootDetails.TargetName,
                    message.EventDetails.LootDetails.TargetType);

                // Get all battles the target has been involved in
                var targetBattles = targetCombatant.GetBattlesRowsByEnemyCombatantRelation();

                KPDatabaseDataSet.BattlesRow lastBattle = null;

                // If any battles, get the last one.
                if ((targetBattles != null) && (targetBattles.Count() > 0))
                {
                    lastBattle = targetBattles.OrderBy(b => b.EndTime).Last();
                }

                // If we found one, make sure it's within 1:00 of Now.  This is
                // for initial 'found' messages.
                if (lastBattle != null)
                {
                    if (lastBattle.EndTime < (message.Timestamp.Subtract(TimeSpan.FromSeconds(60))))
                    {
                        lastBattle = localDB.Battles.AddBattlesRow(targetCombatant, message.Timestamp,
                            message.Timestamp, true, null, (byte)ActorPlayerType.Unknown, 0, 0,
                            (byte)MobDifficulty.Unknown, false);
                    }
                }
                else
                {
                    // If we didn't find any battles for this target, look for recently
                    // ended battles with no target (ie: battle created when player
                    // gained xp, but no enemy name given so no targetCombatant).

                    var recentNonTargetBattles = localDB.Battles.Where(
                        b => b.DefaultBattle == false &&
                            b.IsOver == true &&
                            b.IsEnemyIDNull() == true &&
                            b.EndTime > message.Timestamp.AddSeconds(-30));

                    if ((recentNonTargetBattles != null) && (recentNonTargetBattles.Count() > 0))
                    {
                        lastBattle = recentNonTargetBattles.Last();
                        lastBattle.EnemyID = targetCombatant.CombatantID;
                    }
                    else
                    {
                        // Or if we didn't find any battles for this target, create a new one.
                        lastBattle = localDB.Battles.AddBattlesRow(targetCombatant, message.Timestamp,
                            message.Timestamp, true, null, (byte)ActorPlayerType.Unknown, 0, 0,
                            (byte)MobDifficulty.Unknown, false);
                    }
                }

                // Locate the item by name in the item table.
                var itemRow = localDB.Items.GetItem(message.EventDetails.LootDetails.ItemName);

                // Add the entry to the loot table.
                localDB.Loot.AddLootRow(itemRow, lastBattle, null, 0, false);
            }
            else
            {
                // Messages for when items or gil are distributed to players.

                if (message.EventDetails.LootDetails.Gil == 0)
                {
                    // handle item drops

                    // Locate the item in the item names table.
                    var itemRow = localDB.Items.GetItem(message.EventDetails.LootDetails.ItemName);

                    // Get an array of all loot entries for this item.
                    var lootEntries = itemRow.GetLootRows();
                    KPDatabaseDataSet.LootRow lootEntry = null;

                    if (lootEntries != null)
                    {
                        // If there are any loot entries of the given type, search for an unclaimed one
                        lootEntry = lootEntries.FirstOrDefault(l => l.Lost == false && l.CombatantsRow == null);
                    }

                    // If there is no (free) loot entry, create a new one.
                    if (lootEntry == null)
                        lootEntry = localDB.Loot.AddLootRow(itemRow, null, null, 0, false);

                    // Update table based on whether someone got the drop or if it was lost
                    if (message.EventDetails.LootDetails.WasLost == true)
                    {
                        // Update Lost value of the entry
                        lootEntry.Lost = true;
                    }
                    else
                    {
                        // Ok, someone got the drop.  Check for WhoObtained;
                        // if it's empty, this is a bogus message (eg: warning when
                        // player isn't qualified to receive drop).
                        if (message.EventDetails.LootDetails.WhoObtained != string.Empty)
                        {
                            var player = localDB.Combatants.GetCombatant(message.EventDetails.LootDetails.WhoObtained, EntityType.Player);

                            lootEntry.CombatantsRow = player;
                        }
                    }
                }
                else
                {
                    // handle gil drops

                    // If no record of the last kill for this mob type, we cannot create a new one
                    // because we have no mob name.

                    if (lastFinishedBattle != null)
                    {
                        // Get the "Gil" item from the items table (created if necessary).
                        var itemRow = localDB.Items.GetItem("Gil");

                        if (message.EventDetails.LootDetails.WhoObtained != string.Empty)
                        {
                            var player = localDB.Combatants.GetCombatant(message.EventDetails.LootDetails.WhoObtained, EntityType.Player);
                            localDB.Loot.AddLootRow(itemRow, lastFinishedBattle, player, message.EventDetails.LootDetails.Gil, false);
                        }
                        else
                        {
                            localDB.Loot.AddLootRow(itemRow, lastFinishedBattle, null, message.EventDetails.LootDetails.Gil, false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Subset code for inserting combat detail information.
        /// </summary>
        /// <param name="message">The message containing combat data.</param>
        private void InsertCombat(Message message)
        {
            // Handle death events separately
            if (message.EventDetails.CombatDetails.InteractionType == InteractionType.Death)
            {
                ProcessDeath(message);
                return;
            }

            KPDatabaseDataSet.CombatantsRow actor = null;
            KPDatabaseDataSet.BattlesRow battle = null;
            KPDatabaseDataSet.ActionsRow action = null;
            KPDatabaseDataSet.ActionsRow secondAction = null;
            KPDatabaseDataSet.ItemsRow item = null;

            // Get the actor combatant, if any.
            if (message.EventDetails.CombatDetails.ActorName != string.Empty)
                actor = localDB.Combatants.GetCombatant(message.EventDetails.CombatDetails.ActorName,
                    message.EventDetails.CombatDetails.ActorEntityType);

            // Get the action row, if any is applicable to the message.
            if (message.EventDetails.CombatDetails.ActionName != string.Empty)
                action = localDB.Actions.GetAction(message.EventDetails.CombatDetails.ActionName);

            // If an item is used, get it.
            if (message.EventDetails.CombatDetails.ItemName != string.Empty)
                item = localDB.Items.GetItem(message.EventDetails.CombatDetails.ItemName);

            // Bogus target for passing in data on incomplete messages.
            TargetDetails bogusTarget = null;
            bogusTarget = message.EventDetails.CombatDetails
                                 .Targets.FirstOrDefault(t => t.Name == string.Empty || t.Name == null);

            // Get the battle (if any) this interaction is associated with.
            if ((message.EventDetails.CombatDetails.Targets.Count == 0) || (bogusTarget != null))
            {
                // No targets, so preparing a move

                if ((message.EventDetails.CombatDetails.ActorEntityType == EntityType.Mob) ||
                    (message.EventDetails.CombatDetails.ActorEntityType == EntityType.CharmedPlayer))
                {
                    // If a mob is taking action, look it up in the battle list, or
                    // create a new battle for it.

                    if (activeMobBattleList.TryGetValue(message.EventDetails.CombatDetails.ActorName, out battle) == false)
                    {
                        // First check the last finished battle.  It's possible a message at the end of the fight
                        // came out-of-order.  If the last finished battle ended within 1 second of the current
                        // message (and it's for the same mob), add it to that battle.
                        if (lastFinishedBattle != null)
                        {
                            if ((lastFinishedBattle.CombatantsRowByEnemyCombatantRelation.CombatantName ==
                                message.EventDetails.CombatDetails.ActorName) &&
                                (lastFinishedBattle.EndTime.AddSeconds(2) > message.Timestamp))
                            {
                                battle = lastFinishedBattle;
                            }
                        }

                        if (battle == null)
                        {
                            battle = localDB.Battles.AddBattlesRow(actor, message.Timestamp,
                                MagicNumbers.MinSQLDateTime, false, null, 0, 0, 0, (byte)MobDifficulty.Unknown, false);

                            activeMobBattleList[message.EventDetails.CombatDetails.ActorName] = battle;
                        }
                    }
                }
                else
                {
                    // If a player is taking action, put this in the most
                    // recent active battle.
                    battle = MostRecentActiveBattle();
                }
            }
            else
            {
                // Ok, in this case there are targets
                if ((message.EventDetails.CombatDetails.ActorEntityType == EntityType.Mob) ||
                    (message.EventDetails.CombatDetails.ActorEntityType == EntityType.CharmedPlayer))
                {
                    // If there is none, create a new battle.
                    // If it's one mob buffing another, assume this is a link and must be
                    // considered the start of a new battle.
                    if (activeMobBattleList.TryGetValue(message.EventDetails.CombatDetails.ActorName, out battle) == false)
                    {
                        // First check the last finished battle.  It's possible a message at the end of the fight
                        // came out-of-order.  If the last finished battle ended within 1 second of the current
                        // message (and it's for the same mob), add it to that battle.
                        if (lastFinishedBattle != null)
                        {
                            if ((lastFinishedBattle.CombatantsRowByEnemyCombatantRelation.CombatantName ==
                                message.EventDetails.CombatDetails.ActorName) &&
                                (lastFinishedBattle.EndTime.AddSeconds(2) > message.Timestamp))
                            {
                                battle = lastFinishedBattle;
                            }
                        }

                        if (battle == null)
                        {
                            battle = localDB.Battles.AddBattlesRow(actor, message.Timestamp,
                                MagicNumbers.MinSQLDateTime, false, null, 0, 0, 0, (byte)MobDifficulty.Unknown, false);

                            activeMobBattleList[message.EventDetails.CombatDetails.ActorName] = battle;
                        }
                    }
                }
                else if (actor == null)
                {
                    // No actor specified (eg: shadows absorb attack)
                    // Place in most recent battle.
                    battle = MostRecentActiveBattle();
                }
                else if ((message.EventDetails.CombatDetails.ActorEntityType == EntityType.Player) &&
                    (message.EventDetails.CombatDetails.Targets.Any(t => t.EntityType == EntityType.Player)))
                {
                    // Buffs/Cures/etc where actor and target are both players should be
                    // put in the most recent active battle.
                    battle = MostRecentActiveBattle();
                }
            }

            if ((battle != null) && (battle != lastFinishedBattle))
            {
                // Update the most recent activity record for this fight.
                lock (activeBattleList)
                    activeBattleList[battle] = message.Timestamp;
            }

            if ((battle != null) && (battle != localDB.Battles.GetDefaultBattle()))
            {
                while (deferredInteractions.Count > 0)
                {
                    var deferredInteraction = deferredInteractions.First();
                    if (deferredInteraction != null)
                    {
                        deferredInteraction.BattlesRow = battle;
                        deferredInteractions = deferredInteractions.Skip(1).ToList<KPDatabaseDataSet.InteractionsRow>();
                    }
                }
            }

            if (message.EventDetails.CombatDetails.Targets.Count == 0)
            {
                localDB.Interactions.AddInteractionsRow(
                    message.Timestamp,
                    actor,
                    null, // no target
                    battle,
                    (byte)message.EventDetails.CombatDetails.ActorPlayerType,
                    message.EventDetails.CombatDetails.IsPreparing,
                    action,
                    (byte)message.EventDetails.CombatDetails.ActionType,
                    (byte)message.EventDetails.CombatDetails.FailedActionType,
                    (byte)DefenseType.None,
                    0,
                    (byte)message.EventDetails.CombatDetails.AidType,
                    (byte)RecoveryType.None,
                    (byte)message.EventDetails.CombatDetails.HarmType,
                    0,
                    (byte)DamageModifier.None,
                    (byte)AidType.None,
                    (byte)RecoveryType.None,
                    (byte)HarmType.None,
                    0,
                    secondAction,
                    item);

            }
            else if (bogusTarget != null)
            {
                var targetRow = battle.CombatantsRowByEnemyCombatantRelation;

                var deferredBattleInteraction =
                localDB.Interactions.AddInteractionsRow(
                    message.Timestamp,
                    actor,
                    targetRow,
                    battle,
                    (byte)message.EventDetails.CombatDetails.ActorPlayerType,
                    message.EventDetails.CombatDetails.IsPreparing,
                    action,
                    (byte)message.EventDetails.CombatDetails.ActionType,
                    (byte)message.EventDetails.CombatDetails.FailedActionType,
                    (byte)bogusTarget.DefenseType,
                    0,
                    (byte)message.EventDetails.CombatDetails.AidType,
                    (byte)RecoveryType.None,
                    (byte)message.EventDetails.CombatDetails.HarmType,
                    0,
                    (byte)DamageModifier.None,
                    (byte)AidType.None,
                    (byte)RecoveryType.None,
                    (byte)HarmType.None,
                    0,
                    secondAction,
                    item);

                if (battle == localDB.Battles.GetDefaultBattle())
                    deferredInteractions.Add(deferredBattleInteraction);
            }
            else
            {

                foreach (var target in message.EventDetails.CombatDetails.Targets)
                {
                    // Get database row for target combatant.
                    KPDatabaseDataSet.CombatantsRow targetRow = null;
                    if (target.Name != null)
                        targetRow = localDB.Combatants.GetCombatant(target.Name, target.EntityType);

                    if (target.SecondaryAction != string.Empty)
                        secondAction = localDB.Actions.GetAction(target.SecondaryAction);

                    // Get the battle each time through the loop if the targets are mobs.
                    if ((target.EntityType == EntityType.Mob) ||
                        (target.EntityType == EntityType.CharmedPlayer))
                    {
                        if (activeMobBattleList.TryGetValue(target.Name, out battle) == false)
                        {
                            // No active battle for this mob.  Check if this is spillover
                            // messaging for a mob that just died (less than 4 seconds ago).

                            var recentKills = localDB.Battles.Where(b =>
                                b.Killed == true &&
                                b.EndTime >= (message.Timestamp.AddSeconds(-4)));

                            if (recentKills.Count() > 0)
                            {
                                battle = recentKills.LastOrDefault(b =>
                                    b.CombatantsRowByEnemyCombatantRelation.CombatantName == target.Name);
                            }

                            // If none found, create a new battle.
                            if (battle == null)
                            {
                                battle = localDB.Battles.AddBattlesRow(targetRow, message.Timestamp,
                                    MagicNumbers.MinSQLDateTime, false, null, 0, 0, 0, (byte)MobDifficulty.Unknown, false);

                                activeMobBattleList[target.Name] = battle;
                            }
                        }

                        // Update the most recent activity record for the fight.
                        lock (activeBattleList)
                            activeBattleList[battle] = message.Timestamp;
                    }

                    // Special double-entry for counters
                    if (message.EventDetails.CombatDetails.ActionType == ActionType.Counterattack)
                    {
                        // Initial attack is stopped by counter.
                        localDB.Interactions.AddInteractionsRow(
                           message.Timestamp,
                           targetRow, // swap with actor
                           actor, // swap with target
                           battle,
                           (byte)message.EventDetails.CombatDetails.ActorPlayerType,
                           message.EventDetails.CombatDetails.IsPreparing,
                           action,
                           (byte)ActionType.Melee, // original attack -must- be melee to be countered
                           (byte)target.FailedActionType,
                           (byte)DefenseType.Counter, // defense type, of course
                           0, // no shadows used here
                           (byte)target.AidType,
                           (byte)target.RecoveryType,
                           (byte)target.HarmType,
                           0, // no damage done
                           (byte)target.DamageModifier,
                           (byte)target.SecondaryAidType,
                           (byte)target.SecondaryRecoveryType,
                           (byte)target.SecondaryHarmType,
                           target.SecondaryAmount,
                           secondAction,
                           item);

                        // The attack that the combatant that countered got
                        localDB.Interactions.AddInteractionsRow(
                            message.Timestamp,
                            actor,
                            targetRow,
                            battle,
                            (byte)message.EventDetails.CombatDetails.ActorPlayerType,
                            message.EventDetails.CombatDetails.IsPreparing,
                            action,
                            (byte)message.EventDetails.CombatDetails.ActionType,
                            (byte)target.FailedActionType,
                            (byte)target.DefenseType,
                            target.ShadowsUsed,
                            (byte)target.AidType,
                            (byte)target.RecoveryType,
                            (byte)target.HarmType,
                            target.Amount,
                            (byte)target.DamageModifier,
                            (byte)target.SecondaryAidType,
                            (byte)target.SecondaryRecoveryType,
                            (byte)target.SecondaryHarmType,
                            target.SecondaryAmount,
                            secondAction,
                            item);
                    }
                    else
                    {
                        localDB.Interactions.AddInteractionsRow(
                            message.Timestamp,
                            actor,
                            targetRow,
                            battle,
                            (byte)message.EventDetails.CombatDetails.ActorPlayerType,
                            message.EventDetails.CombatDetails.IsPreparing,
                            action,
                            (byte)message.EventDetails.CombatDetails.ActionType,
                            (byte)target.FailedActionType,
                            (byte)target.DefenseType,
                            target.ShadowsUsed,
                            (byte)target.AidType,
                            (byte)target.RecoveryType,
                            (byte)target.HarmType,
                            target.Amount,
                            (byte)target.DamageModifier,
                            (byte)target.SecondaryAidType,
                            (byte)target.SecondaryRecoveryType,
                            (byte)target.SecondaryHarmType,
                            target.SecondaryAmount,
                            secondAction,
                            item);
                    }
                }
            }
        }

        /// <summary>
        /// Subset code for handling mob deaths.
        /// </summary>
        /// <param name="message"></param>
        private void ProcessDeath(Message message)
        {
            KPDatabaseDataSet.CombatantsRow actor = null;
            KPDatabaseDataSet.BattlesRow battle = null;

            // If there are no targets, we can't do anything.
            if ((message.EventDetails.CombatDetails.Targets == null) || (message.EventDetails.CombatDetails.Targets.Count == 0))
                return;

            // If no actor name given, target just "fell to the ground".
            if (message.EventDetails.CombatDetails.ActorName != string.Empty)
            {
                // Determine the acting combatant for this event
                actor = localDB.Combatants.GetCombatant(message.EventDetails.CombatDetails.ActorName,
                    message.EventDetails.CombatDetails.ActorEntityType);
            }


            // Record an interaction entry for each death
            foreach (var target in message.EventDetails.CombatDetails.Targets)
            {
                var targetRow = localDB.Combatants.GetCombatant(target.Name, target.EntityType);

                if ((target.EntityType == EntityType.Mob) ||
                    (target.EntityType == EntityType.CharmedPlayer))
                {
                    // If target is mob, pick that battle

                    // If actorPlayerType is Other, make sure there is an Other action against it
                    if (message.EventDetails.CombatDetails.ActorPlayerType == ActorPlayerType.Other)
                    {
                        // If we find such a battle, use it.
                        foreach (var mobBattle in activeMobBattleList.Where(b => b.Key == target.Name))
                        {
                            var sameActorRows = mobBattle.Value.GetInteractionsRows()
                                .Where(i => i.IsActorIDNull() == false &&
                                       i.CombatantsRowByActorCombatantRelation == actor &&
                                       (FailedActionType)i.FailedActionType == FailedActionType.None);

                            //if (mobBattle.Value.GetInteractionsRows().Any(i => (ActorPlayerType)i.ActorType == ActorPlayerType.Other))
                            if (sameActorRows.Count() > 0)
                            {
                                battle = mobBattle.Value;

                                battle.EndTime = message.Timestamp;
                                battle.Killed = true;
                                battle.CombatantsRowByBattleKillerRelation = actor;
                                battle.KillerPlayerType = (byte)message.EventDetails.CombatDetails.ActorPlayerType;

                                break;
                            }
                        }

                        // Otherwise this is likely a death notice of a fight by someone outside the party.
                        // Create a new entry for it.
                        if (battle == null)
                        {
                            // Make a new one if one doesn't exist, created already killed
                            battle = localDB.Battles.AddBattlesRow(targetRow, message.Timestamp, message.Timestamp,
                                true, actor, (byte)message.EventDetails.CombatDetails.ActorPlayerType, 0, 0,
                                (byte)MobDifficulty.Unknown, false);
                        }
                        else
                        {
                            // Don't try to modify the enumeration during the foreach loop.
                            activeMobBattleList.Remove(target.Name);

                            lock (activeBattleList)
                                activeBattleList.Remove(battle);
                        }
                    }

                    if (battle == null)
                    {
                        if (activeMobBattleList.TryGetValue(target.Name, out battle) == true)
                        {
                            // close out this battle
                            battle.EndTime = message.Timestamp;
                            battle.Killed = true;
                            battle.CombatantsRowByBattleKillerRelation = actor;
                            battle.KillerPlayerType = (byte)message.EventDetails.CombatDetails.ActorPlayerType;

                            activeMobBattleList.Remove(target.Name);
                            lock (activeBattleList)
                                activeBattleList.Remove(battle);
                        }
                        else
                        {
                            // Make a new one if one doesn't exist, created already killed
                            battle = localDB.Battles.AddBattlesRow(targetRow, message.Timestamp, message.Timestamp,
                                true, actor, (byte)message.EventDetails.CombatDetails.ActorPlayerType, 0, 0,
                                (byte)MobDifficulty.Unknown, false);
                        }
                    }

                    lastFinishedBattle = battle;
                }
                else
                {
                    // If target is player, find the battle to put this in
                    if (actor == null)
                    {
                        // If no actor, get the most recent battle
                        battle = MostRecentActiveBattle();
                    }
                    else
                    {
                        // Otherwise get the actor's battle
                        if (activeMobBattleList.TryGetValue(actor.CombatantName, out battle) == false)
                        {
                            // First check the last finished battle.  It's possible a message at the end of the fight
                            // came out-of-order.  If the last finished battle ended within 1 second of the current
                            // message (and it's for the same mob), add it to that battle.
                            if (lastFinishedBattle != null)
                            {
                                if ((lastFinishedBattle.CombatantsRowByEnemyCombatantRelation.CombatantName ==
                                    message.EventDetails.CombatDetails.ActorName) &&
                                    (lastFinishedBattle.EndTime.AddSeconds(2) > message.Timestamp))
                                {
                                    battle = lastFinishedBattle;
                                }
                            }


                            // If they're not in the active battle list, create a new one.
                            if (battle == null)
                            {
                                battle = localDB.Battles.AddBattlesRow(actor, message.Timestamp,
                                    MagicNumbers.MinSQLDateTime, false, null, 0, 0, 0,
                                    (byte)MobDifficulty.Unknown, false);

                                activeMobBattleList.Add(actor.CombatantName, battle);
                                activeBattleList.Add(battle, message.Timestamp);
                            }
                        }
                    }
                }

                // Make a note of the death event in the interactions table
                localDB.Interactions.AddInteractionsRow(
                       message.Timestamp,
                       actor,
                       targetRow,
                       battle,
                       (byte)message.EventDetails.CombatDetails.ActorPlayerType,
                       message.EventDetails.CombatDetails.IsPreparing,
                       null,
                       (byte)message.EventDetails.CombatDetails.ActionType,
                       (byte)message.EventDetails.CombatDetails.FailedActionType,
                       (byte)target.DefenseType,
                       target.ShadowsUsed,
                       (byte)message.EventDetails.CombatDetails.AidType,
                       (byte)target.RecoveryType,
                       (byte)message.EventDetails.CombatDetails.HarmType,
                       target.Amount,
                       (byte)target.DamageModifier,
                       (byte)target.SecondaryAidType,
                       (byte)target.SecondaryRecoveryType,
                       (byte)target.SecondaryHarmType,
                       target.SecondaryAmount,
                       null,
                       null);
            }
        }
        #endregion

        #region Utility functions
        /// <summary>
        /// Close out inactive battles.  And parse is ending, close out all remaining battles.
        /// </summary>
        /// <param name="closeOutAllBattles">Flag to indicate whether the
        /// parse is ending and all battles should be closed.</param>
        private void UpdateActiveBattleList(bool closeOutAllBattles)
        {
            List<KPDatabaseDataSet.BattlesRow> battlesToRemove = new List<KPDatabaseDataSet.BattlesRow>();
            DateTime tenMinutesAgo = mostRecentTimestamp.Subtract(TimeSpan.FromMinutes(10));

            lock (activeBattleList)
            {
                if (activeBattleList.Count > 0)
                {
                    foreach (var activeBattle in activeBattleList)
                    {
                        // Never close out the default battle, if it's in the list.
                        if (activeBattle.Key.DefaultBattle == false)
                        {
                            // Anything that hasn't had any activity in more than
                            // 10 minutes is marked for removal.
                            if ((closeOutAllBattles == true) || (activeBattle.Value < tenMinutesAgo))
                            {
                                battlesToRemove.Add(activeBattle.Key);
                            }
                        }
                    }

                    foreach (var battleToRemove in battlesToRemove)
                    {
                        // If the battle hasn't been marked as ended, set the
                        // ending timestamp to match the most recent message's.
                        // If reparsing, this will be the last message in the
                        // log.
                        if (battleToRemove.EndTime == MagicNumbers.MinSQLDateTime)
                            battleToRemove.EndTime = mostRecentTimestamp;
                        activeBattleList.Remove(battleToRemove);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the most recent still-active battle.  If there are none, returns
        /// the default battle.
        /// </summary>
        /// <returns></returns>
        private KPDatabaseDataSet.BattlesRow MostRecentActiveBattle()
        {
            if (activeBattleList.Count == 0)
                return localDB.Battles.GetDefaultBattle();

            // find the most recent non-default battle.
            KPDatabaseDataSet.BattlesRow mostRecentBattle;
            lock (activeBattleList)
                mostRecentBattle = (KPDatabaseDataSet.BattlesRow)activeBattleList
                    .Where(ab => ab.Key.DefaultBattle == false)
                    .MaxEntry(ab => ab.Value, ab => ab.Key);

            if (mostRecentBattle == null)
                mostRecentBattle = localDB.Battles.GetDefaultBattle();

            return mostRecentBattle;
        }
        #endregion
    }
}
