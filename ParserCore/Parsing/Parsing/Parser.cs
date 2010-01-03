using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace WaywardGamers.KParser.Parsing
{
    internal static class Parser
    {
        #region Member Variables
        private static uint lastMessageID = 0;
        #endregion

        #region Top-level category breakdown
        /// <summary>
        /// Takes the provided Message Line and parses its contents, updating
        /// the internal data of the message as it goes.
        /// </summary>
        /// <param name="messageLine">Individual message lines passed in by the
        /// message manager as data is accumulated by the Monitor.</param>
        internal static Message Parse(MessageLine messageLine)
        {
            Message message = GetBaseMessage(messageLine);

            if (message == null)
            {
                message = new Message(messageLine);

                InitialParse(message);
                lastMessageID = message.MessageID;
            }
            else
            {
                message.AddMessageLine(messageLine);
                ContinueParse(message);
            }

            //if (message.IsParseSuccessful == true)
            //{
            //    if ((message.MessageCategory == MessageCategoryType.Event) &&
            //        (message.EventDetails.EventMessageType == EventMessageType.Interaction) &&
            //        (message.EventDetails.CombatDetails.InteractionType != InteractionType.Unknown))
            //    {
            //        if ((message.EventDetails.CombatDetails.IsPreparing == false) &&
            //            (message.EventDetails.CombatDetails.ActorName == string.Empty) &&
            //            ((message.EventDetails.CombatDetails.SuccessLevel != SuccessType.Failed) ||
            //             (message.EventDetails.CombatDetails.ActionType == ActionType.Spell)))
            //        {
            //            Message prevMsg = MsgManager.Instance.FindLastMessageToMatch(messageLine,
            //                ParseCodes.Instance.GetAlternateCodes(messageLine.MessageCode), message);

            //            if (prevMsg != null)
            //            {
            //                foreach (var msgLine in message.MessageLineCollection)
            //                    prevMsg.AddMessageLine(msgLine);

            //                message = prevMsg;
            //                ContinueParse(message);
            //            }
            //            else
            //            {
            //                // No previous message with same code.  Probably additional effect
            //            }
            //        }
            //    }
            //}

            return message;
        }

        /// <summary>
        /// Do a systematic search for past messages to find any that the current
        /// message can be attached to before we decide to create a new message.
        /// </summary>
        /// <param name="messageLine">The message line to search on.</param>
        /// <returns>Returns the message that we can attach to if found, null if none found.</returns>
        private static Message GetBaseMessage(MessageLine messageLine)
        {
            Message msg = null;

            // Messages with the same event number are tied together.  If we've already
            // processed a message with the same event number, get that.  This can include
            // things like critical hits, or messages that were broken up to fit the screen
            // width.
            msg = MsgManager.Instance.FindMessageWithEventNumber(messageLine.EventSequence);

            // If there was no prior message with the same event number, do more complicated checking.

            // Additional messages can be:

            // AOE Cure (Curaga/etc)
            // Player casts spell/uses ability.
            // PlayerX recovers Y HP.

            // AOE Buff (protectra/ballad/etc)
            // Player casts spell.
            // PlayerX gains the effect of Protect.
            // Player uses Ability.
            // PlayerX receives the effect of Samurai Roll.

            // Item use.
            // Player uses Hi-Potion.
            // Player recovers 100 HP. [possible extended delay here]

            // AOE Damage (blizzaga, etc)
            // Player casts spell.
            // Target takes X points of damage.

            // AOE Status (poisonga/graviga/etc)
            // Player casts Poisonga.
            // MobX receives the effect of poison.

            // Additional Effect
            // Samba: Additional effect: 11 HP drained from the Mamool Ja Mimer.
            // Enspell: Additional effect: 11 points of damage.
            // etc
            // Base message can only have 1 additional effect attached.
            // Base message must be a ranged or melee attack.

            // Critical hits.
            // Taken care of already, since they'll have the same event number


            // Curaga
            if (msg == null)
            {
                if (ParseExpressions.RecoversHP.Match(messageLine.TextOutput).Success)
                {
                    msg = MsgManager.Instance.FindMatchingSpellCastOrAbilityUse(messageLine,
                        ParseCodes.Instance.GetAlternateCodes(messageLine.MessageCode));
                }
            }

            // AOE Buff
            if (msg == null)
            {
                Match buffMatch = ParseExpressions.Buff.Match(messageLine.TextOutput);
                Match debuffMatch = ParseExpressions.Debuff.Match(messageLine.TextOutput);
                Match resMatch = ParseExpressions.GainResistance.Match(messageLine.TextOutput);
                Match corMatch = ParseExpressions.GainCorRoll.Match(messageLine.TextOutput);

                if (buffMatch.Success ||
                    debuffMatch.Success ||
                    resMatch.Success ||
                    corMatch.Success)
                {
                    string effectName = string.Empty;
                    string targetName = string.Empty;

                    if (buffMatch.Success)
                    {
                        effectName = buffMatch.Groups[ParseFields.Effect].Value;
                        targetName = buffMatch.Groups[ParseFields.Target].Value;
                    }
                    else if (debuffMatch.Success)
                    {
                        effectName = debuffMatch.Groups[ParseFields.Effect].Value;
                        targetName = debuffMatch.Groups[ParseFields.Target].Value;
                    }
                    else if (resMatch.Success)
                    {
                        effectName = resMatch.Groups[ParseFields.Effect].Value;
                        targetName = resMatch.Groups[ParseFields.Target].Value;
                    }
                    else if (corMatch.Success)
                    {
                        effectName = corMatch.Groups[ParseFields.Ability].Value;
                        targetName = corMatch.Groups[ParseFields.Target].Value;
                    }

                    msg = MsgManager.Instance.FindMatchingSpellCastOrAbilityUseWithEffect(messageLine,
                        ParseCodes.Instance.GetAlternateCodes(messageLine.MessageCode), effectName, targetName);
                }
            }

            // AOE Buff (2)
            if (msg == null)
            {
                if (ParseExpressions.Enhance.Match(messageLine.TextOutput).Success ||
                    ParseExpressions.RemoveStatus.Match(messageLine.TextOutput).Success)
                {
                    msg = MsgManager.Instance.FindMatchingSpellCastOrAbilityUse(messageLine,
                        ParseCodes.Instance.GetAlternateCodes(messageLine.MessageCode));
                }
            }

            // AOE Damage
            if (msg == null)
            {
                Match damageMatch = ParseExpressions.TargetTakesDamage.Match(messageLine.TextOutput);
                if (damageMatch.Success)
                {
                    string targetName = damageMatch.Groups[ParseFields.Target].Value;
                    msg = MsgManager.Instance.FindMatchingSpellCastOrAbilityUseForDamage(messageLine,
                        ParseCodes.Instance.GetAlternateCodes(messageLine.MessageCode), targetName);
                }
            }

            // AOE Status/Debuff
            if (msg == null)
            {
                if (ParseExpressions.Dispelled.Match(messageLine.TextOutput).Success ||
                    ParseExpressions.Debuff.Match(messageLine.TextOutput).Success ||
                    ParseExpressions.Enfeeble.Match(messageLine.TextOutput).Success)
                {
                    msg = MsgManager.Instance.FindMatchingSpellCastOrAbilityUse(messageLine,
                        ParseCodes.Instance.GetAlternateCodes(messageLine.MessageCode));
                }
            }

            // Additional Effect
            if (msg == null)
            {
                if (ParseExpressions.AdditionalEffect.Match(messageLine.TextOutput).Success)
                {
                    msg = MsgManager.Instance.FindMatchingMeleeOrRanged(messageLine,
                        ParseCodes.Instance.GetAEAlternateCodes(messageLine.MessageCode));

                    // If we got a valid message, we can mark it has having an additional effect.
                    if (msg != null)
                        msg.EventDetails.CombatDetails.HasAdditionalEffect = true;
                }
            }

            return msg;
        }

        /// <summary>
        /// Code branching for initial parse of a new message.
        /// </summary>
        /// <param name="message"></param>
        private static void InitialParse(Message message)
        {
            switch (message.MessageCategory)
            {
                case MessageCategoryType.Chat:
                    ParseChat(message);
                    break;
                case MessageCategoryType.System:
                    ParseSystem(message);
                    break;
                case MessageCategoryType.Event:
                    ParseEvent(message);
                    break;
            }
        }

        /// <summary>
        /// Code branching for a continuing parse of a message that was partly parsed before.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static Message ContinueParse(Message message)
        {
            switch (message.MessageCategory)
            {
                case MessageCategoryType.Chat:
                    // No further parsing needed; pull CompleteMessageText when storing to database.
                    break;
                case MessageCategoryType.System:
                    // No further parsing needed since we're only using the message code.
                    break;
                case MessageCategoryType.Event:
                    ParseEvent(message);
                    break;
            }
            
            return message;
        }
        #endregion

        #region Parsing of general Message Categories
        /// <summary>
        /// Parse out system messages.
        /// </summary>
        /// <param name="message">The message to parse.</param>
        private static void ParseSystem(Message message)
        {
            switch (message.CurrentMessageCode)
            {
                case 0x00:
                    message.SystemDetails.SystemMessageType = SystemMessageType.ZoneChange;
                    break;
                case 0x9d:
                    message.SystemDetails.SystemMessageType = SystemMessageType.CommandError;
                    break;
                case 0xa1:
                    message.SystemDetails.SystemMessageType = SystemMessageType.ConquestUpdate;
                    break;
                case 0xbf:
                    message.SystemDetails.SystemMessageType = SystemMessageType.EffectWearsOff;

                    //Match charmCheck = ParseExpressions.NotCharmed.Match(message.CurrentMessageText);
                    //if (charmCheck.Success == true)
                    //    MessageManager.Instance.RemovePetEntity(charmCheck.Groups[ParseFields.Fulltarget].Value);
                    break;
                case 0xcc:
                    message.SystemDetails.SystemMessageType = SystemMessageType.SearchComment;
                    break;
                case 0xcd: // Linkshell message
                    message.SetMessageCategory(MessageCategoryType.Chat);
                    message.ChatDetails.ChatMessageType = ChatMessageType.Linkshell;
                    message.ChatDetails.ChatSpeakerName = "-Linkshell-";
                    message.ChatDetails.ChatSpeakerType = SpeakerType.Unknown;
                    message.ChatDetails.FullChatText = message.CompleteMessageText;
                    break;
                case 0xce: // Echo messages - Store those marked with appropriate prefix as chat messages.
                    if (message.CompleteMessageText.StartsWith("KP:", StringComparison.InvariantCultureIgnoreCase) ||
                        message.CompleteMessageText.StartsWith("KPARSER:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        message.SetMessageCategory(MessageCategoryType.Chat);
                        message.ChatDetails.ChatMessageType = ChatMessageType.Echo;
                        message.ChatDetails.ChatSpeakerName = "-Echo-";
                        message.ChatDetails.ChatSpeakerType = SpeakerType.Self;
                        message.ChatDetails.FullChatText = message.CompleteMessageText;
                    }
                    else
                    {
                        message.SystemDetails.SystemMessageType = SystemMessageType.Echo;
                    }
                    break;
                case 0xd0:
                    message.SystemDetails.SystemMessageType = SystemMessageType.Examine;
                    break;
                case 0xd1:
                    message.SystemDetails.SystemMessageType = SystemMessageType.ReuseTime;
                    break;
                case 0x7b:
                    message.SystemDetails.SystemMessageType = SystemMessageType.OutOfRange;
                    break;
                case 0xbe: // Order to enter
                case 0x92: // Arena time remaining
                case 0x94: // Arena intro/announcement
                    // Mark these as chat messages
                    message.SetMessageCategory(MessageCategoryType.Chat);
                    message.ChatDetails.ChatMessageType = ChatMessageType.Arena;
                    message.ChatDetails.ChatSpeakerName = "-Arena-";
                    message.ChatDetails.ChatSpeakerType = SpeakerType.NPC;
                    message.ChatDetails.FullChatText = message.CompleteMessageText;
                    break;
                default:
                    message.SystemDetails.SystemMessageType = SystemMessageType.Unknown;
                    break;
            }

            // If we successfully determined the message type, or recategorized it as Chat,
            // mark it as successfully parsed.
            if ((message.SystemDetails.SystemMessageType != SystemMessageType.Unknown) ||
                (message.MessageCategory == MessageCategoryType.Chat))
                message.SetParseSuccess(true);
        }

        /// <summary>
        /// Parse out information about chat messages.
        /// </summary>
        /// <param name="message">Message to parse.</param>
        private static void ParseChat(Message message)
        {
            // Determine the message type based on the code
            switch (message.MessageCode)
            {
                case 0x01: // <me> say
                case 0x09: // Others say
                case 0x98: // <npc> say
                case 0x90: // <fellow> say
                    message.ChatDetails.ChatMessageType = ChatMessageType.Say;
                    break;
                case 0x02: // <me> shout
                case 0x0a: // Others shout
                case 0x8e: // <npc> shout
                    message.ChatDetails.ChatMessageType = ChatMessageType.Shout;
                    break;
                case 0x04: // <me> tell
                case 0x0c: // Others tell
                    message.ChatDetails.ChatMessageType = ChatMessageType.Tell;
                    break;
                case 0x05: // <me> party
                case 0x0d: // Others party
                    message.ChatDetails.ChatMessageType = ChatMessageType.Party;
                    break;
                case 0x06: // <me> linkshell
                case 0x0e: // Others linkshell
                    message.ChatDetails.ChatMessageType = ChatMessageType.Linkshell;
                    break;
                case 0x07: // <me> emote
                case 0x0f: // Others emote
                    message.ChatDetails.ChatMessageType = ChatMessageType.Emote;
                    break;
                // The following codes fit into the above pattern, but
                // I haven't encountered the specific code values.
                case 0x03: // <me>?
                case 0x0b: // Others?
                default:
                    message.ChatDetails.ChatMessageType = ChatMessageType.Unknown;
                    break;
            }

            // Determine the speaker type based on the code
            switch (message.MessageCode)
            {
                case 0x01: // <me> say
                case 0x02: // <me> shout
                case 0x03: // <me> ?
                case 0x04: // <me> tell
                case 0x05: // <me> party
                case 0x06: // <me> linkshell
                case 0x07: // <me> emote
                    message.ChatDetails.ChatSpeakerType = SpeakerType.Self;
                    break;
                case 0x09: // Others say
                case 0x0a: // Others shout
                case 0x0b: // Others ?
                case 0x0c: // Others tell
                case 0x0d: // Others party
                case 0x0e: // Others linkshell
                case 0x0f: // Others emote
                    message.ChatDetails.ChatSpeakerType = SpeakerType.Player;
                    break;
                case 0x90: // <fellow> say
                case 0x98: // <npc> say
                case 0x8e: // <npc> shout
                    message.ChatDetails.ChatSpeakerType = SpeakerType.NPC;
                    break;
                default:
                    message.ChatDetails.ChatSpeakerType = SpeakerType.Unknown;
                    break;
            }

            // Determine the name of the speaker
            Match chatName = null;

            switch (message.ChatDetails.ChatMessageType)
            {
                case ChatMessageType.Say:
                    if (message.ChatDetails.ChatSpeakerType == SpeakerType.NPC)
                        chatName = ParseExpressions.ChatNPC.Match(message.CurrentMessageText);
                    else
                        chatName = ParseExpressions.ChatSay.Match(message.CurrentMessageText);
                    break;
                case ChatMessageType.Party:
                    chatName = ParseExpressions.ChatParty.Match(message.CurrentMessageText);
                    break;
                case ChatMessageType.Linkshell:
                    chatName = ParseExpressions.ChatLinkshell.Match(message.CurrentMessageText);
                    break;
                case ChatMessageType.Shout:
                    if (message.ChatDetails.ChatSpeakerType == SpeakerType.NPC)
                        chatName = ParseExpressions.ChatNPC.Match(message.CurrentMessageText);
                    else
                        chatName = ParseExpressions.ChatShout.Match(message.CurrentMessageText);
                    break;
                case ChatMessageType.Emote:
                    chatName = ParseExpressions.ChatEmote.Match(message.CurrentMessageText);
                    break;
                case ChatMessageType.Tell:
                    chatName = ParseExpressions.ChatTell.Match(message.CurrentMessageText);
                    break;
            }

            if ((chatName != null) && (chatName.Success == true))
            {
                message.ChatDetails.ChatSpeakerName = chatName.Groups[ParseFields.Name].Value;
                message.ChatDetails.FullChatText = message.CompleteMessageText;
                message.SetParseSuccess(true);
            }
        }

        /// <summary>
        /// Break down action messages into subgroups for further parsing.
        /// </summary>
        /// <param name="message">The message to parse.</param>
        private static void ParseEvent(Message message)
        {
            // An action is an automatically generated message.  As such, it will
            // always end in a period or exclamation point.  If it does not, then
            // this is only part of the message line and we shouldn't bother trying
            // to analyze the text at this point.  Return it as incomplete.

            if ((message.CurrentMessageText.EndsWith(".") == false) &&
                (message.CurrentMessageText.EndsWith("!") == false) &&
                (message.CurrentMessageCode != 0x79))
                return;

            uint currentMsgCode = message.CurrentMessageCode;

            // Determine type of action message
            if (message.EventDetails.EventMessageType == EventMessageType.Unknown)
            {
                switch (currentMsgCode)
                {
                    case 0x83: // Exp, no chain
                    case 0x79: // Item drop, Lot for item, xp chain, xp on chain, equipment changed, /recast message, /anon changed, etc)
                    case 0x7f: // Item obtained
                    case 0x95: // Item found in a chest
                        message.EventDetails.EventMessageType = EventMessageType.EndBattle;
                        break;
                    case 0x92: // <me> caught a fish!
                    case 0x94: // other fishing messages, other stuff
                        message.EventDetails.EventMessageType = EventMessageType.Fishing;
                        break;
                    default: // Mark the large swaths of possible combat messages
                        if ((currentMsgCode >= 0x13) && (currentMsgCode <= 0x84))
                            message.EventDetails.EventMessageType = EventMessageType.Interaction;
                        else if ((currentMsgCode >= 0xa2) && (currentMsgCode <= 0xbf))
                            message.EventDetails.EventMessageType = EventMessageType.Interaction;
                        else if (currentMsgCode == 0x8d)
                        {
                            ParseCode8d(message);
                            return;
                        }
                        else // Everything else is ignored.
                            message.EventDetails.EventMessageType = EventMessageType.Other;
                        break;
                }
            }

            // Based on event subcategory, continue parsing
            switch (message.EventDetails.EventMessageType)
            {
                case EventMessageType.Interaction:
                    ParseCombat(message);
                    break;
                case EventMessageType.EndBattle:
                case EventMessageType.Experience:
                case EventMessageType.Loot:
                    ParseEndBattle(message);
                    break;
                case EventMessageType.Steal:
                    ParseStealing(message);
                    break;
                default:
                    // Ignore Fishing for now
                    // Ignore Other always
                    break;
            }
        }

        private static bool ParseStealing(Message message)
        {
            Match stealMatch = null;
            CombatDetails msgCombatDetails = message.EventDetails.CombatDetails;
            string currentMessageText = message.CurrentMessageText;
            TargetDetails target = null;

            stealMatch = ParseExpressions.Steal.Match(currentMessageText);
            if (stealMatch.Success == true)
            {
                msgCombatDetails.ActorName = stealMatch.Groups[ParseFields.Name].Value;
                msgCombatDetails.ActionName = "Steal";
                target = msgCombatDetails.AddTarget(stealMatch.Groups[ParseFields.Fulltarget].Value);
                msgCombatDetails.ActionType = ActionType.Steal;
                msgCombatDetails.ItemName = stealMatch.Groups[ParseFields.Item].Value;
                msgCombatDetails.HarmType = HarmType.None;
                msgCombatDetails.AidType = AidType.None;
                message.SetParseSuccess(true);
                return true;
            }

            stealMatch = ParseExpressions.FailSteal.Match(currentMessageText);
            if (stealMatch.Success == true)
            {
                msgCombatDetails.ActorName = stealMatch.Groups[ParseFields.Name].Value;
                msgCombatDetails.ActionName = "Steal";
                target = msgCombatDetails.AddTarget(stealMatch.Groups[ParseFields.Fulltarget].Value);
                msgCombatDetails.ActionType = ActionType.Steal;
                msgCombatDetails.FailedActionType = FailedActionType.NoEffect;
                target.FailedActionType = FailedActionType.NoEffect;
                msgCombatDetails.HarmType = HarmType.None;
                msgCombatDetails.AidType = AidType.None;
                message.SetParseSuccess(true);
                return true;
            }

            stealMatch = ParseExpressions.Mug.Match(currentMessageText);
            if (stealMatch.Success == true)
            {
                msgCombatDetails.ActorName = stealMatch.Groups[ParseFields.Name].Value;
                msgCombatDetails.ActionName = "Mug";
                target = msgCombatDetails.AddTarget(stealMatch.Groups[ParseFields.Fulltarget].Value);
                msgCombatDetails.ActionType = ActionType.Steal;
                target.Amount = int.Parse(stealMatch.Groups[ParseFields.Money].Value);
                msgCombatDetails.HarmType = HarmType.None;
                msgCombatDetails.AidType = AidType.None;
                message.SetParseSuccess(true);
                return true;
            }

            stealMatch = ParseExpressions.FailMug.Match(currentMessageText);
            if (stealMatch.Success == true)
            {
                msgCombatDetails.ActorName = stealMatch.Groups[ParseFields.Name].Value;
                msgCombatDetails.ActionName = "Mug";
                target = msgCombatDetails.AddTarget(stealMatch.Groups[ParseFields.Fulltarget].Value);
                msgCombatDetails.ActionType = ActionType.Steal;
                msgCombatDetails.FailedActionType = FailedActionType.NoEffect;
                target.FailedActionType = FailedActionType.NoEffect;
                msgCombatDetails.HarmType = HarmType.None;
                msgCombatDetails.AidType = AidType.None;
                message.SetParseSuccess(true);
                return true;
            }

            return false;
        }

        private static void ParseCode8d(Message message)
        {
            // Can be a failed message, or Arena messages in Limbus.
            message.SetMessageCategory(MessageCategoryType.Chat);
            message.ChatDetails.ChatMessageType = ChatMessageType.Arena;
            message.ChatDetails.ChatSpeakerName = "-Arena-";
            message.ChatDetails.ChatSpeakerType = SpeakerType.NPC;
            message.SetParseSuccess(true);
        }

        #endregion

        #region Parsing of end-combat data
        private static void ParseEndBattle(Message message)
        {
            Match lootOrXP;

            switch (message.CurrentMessageCode)
            {
                // Item drop, Lot for item, equipment changed, /recast message
                case 0x79:
                    // Further specificity based on extraCode1
                    switch (message.ExtraCode1)
                    {
                        case 0:
                            // Experience chain
                            lootOrXP = ParseExpressions.ExpChain.Match(message.CurrentMessageText);
                            if (lootOrXP.Success == true)
                            {
                                message.EventDetails.EventMessageType = EventMessageType.Experience;
                                message.EventDetails.ExperienceDetails.ExperienceChain = int.Parse(lootOrXP.Groups[ParseFields.Number].Value);
                                message.SetParseSuccess(true);
                                return;
                            }
                            // Experience points (only if got chain)
                            lootOrXP = ParseExpressions.Experience.Match(message.CurrentMessageText);
                            if (lootOrXP.Success == true)
                            {
                                message.EventDetails.EventMessageType = EventMessageType.Experience;
                                message.EventDetails.ExperienceDetails.ExperiencePoints = int.Parse(lootOrXP.Groups[ParseFields.Number].Value);
                                message.EventDetails.ExperienceDetails.ExperienceRecipient = lootOrXP.Groups[ParseFields.Name].Value;
                                message.SetParseSuccess(true);
                                return;
                            }
                            // Loot found/dropped by mob
                            lootOrXP = ParseExpressions.FindLootOn.Match(message.CurrentMessageText);
                            if (lootOrXP.Success == true)
                            {
                                message.EventDetails.EventMessageType = EventMessageType.Loot;
                                message.EventDetails.LootDetails.IsFoundMessage = true;
                                message.EventDetails.LootDetails.ItemName = lootOrXP.Groups[ParseFields.Item].Value;
                                message.EventDetails.LootDetails.TargetName = lootOrXP.Groups[ParseFields.Target].Value;
                                message.EventDetails.LootDetails.TargetType = EntityType.Mob;
                                message.SetParseSuccess(true);
                                break;
                            }
                            // Loot found in a chest/coffer
                            lootOrXP = ParseExpressions.FindLootIn.Match(message.CurrentMessageText);
                            if (lootOrXP.Success == true)
                            {
                                message.EventDetails.EventMessageType = EventMessageType.Loot;
                                message.EventDetails.LootDetails.IsFoundMessage = true;
                                message.EventDetails.LootDetails.ItemName = lootOrXP.Groups[ParseFields.Item].Value;
                                message.EventDetails.LootDetails.TargetName = lootOrXP.Groups[ParseFields.Target].Value;
                                message.EventDetails.LootDetails.TargetType = EntityType.TreasureChest;
                                message.SetParseSuccess(true);
                                break;
                            }
                            // May also be the "not qualified" message (out of space, rare/ex)
                            lootOrXP = ParseExpressions.LootReqr.Match(message.CurrentMessageText);
                            if (lootOrXP.Success == true)
                            {
                                message.EventDetails.EventMessageType = EventMessageType.Loot;
                                message.EventDetails.LootDetails.IsFoundMessage = false;
                                message.EventDetails.LootDetails.ItemName = lootOrXP.Groups[ParseFields.Item].Value;
                                message.SetParseSuccess(true);
                                break;
                            }
                            // Or the "Item is lost" message.
                            lootOrXP = ParseExpressions.LootLost.Match(message.CurrentMessageText);
                            if (lootOrXP.Success == true)
                            {
                                message.EventDetails.EventMessageType = EventMessageType.Loot;
                                message.EventDetails.LootDetails.IsFoundMessage = false;
                                message.EventDetails.LootDetails.WasLost = true;
                                message.EventDetails.LootDetails.ItemName = lootOrXP.Groups[ParseFields.Item].Value;
                                message.SetParseSuccess(true);
                                break;
                            }
                            lootOrXP = ParseExpressions.LotItem.Match(message.CurrentMessageText);
                            if (lootOrXP.Success == true)
                            {
                                // Don't record lots at this time. Just mark the message as parsed.
                                //message.EventDetails.EventMessageType = EventMessageType.Loot;
                                message.SetParseSuccess(true);
                                break;
                            }
                            lootOrXP = ParseExpressions.DiceRoll.Match(message.CurrentMessageText);
                            if (lootOrXP.Success == true)
                            {
                                // Don't record rolls at this time. Just mark the message as parsed.
                                message.EventDetails.EventMessageType = EventMessageType.Other;
                                message.SetParseSuccess(true);
                                break;
                            }
                            break;
                        case 2:
                            // Obtaining temporary items, Salvage restriction removal
                            lootOrXP = ParseExpressions.GetLoot.Match(message.CurrentMessageText);
                            if (lootOrXP.Success == true)
                            {
                                message.EventDetails.EventMessageType = EventMessageType.Loot;
                                message.EventDetails.LootDetails.IsFoundMessage = false;
                                message.EventDetails.LootDetails.ItemName = lootOrXP.Groups[ParseFields.Item].Value;
                                message.EventDetails.LootDetails.WhoObtained = lootOrXP.Groups[ParseFields.Name].Value;
                                message.SetParseSuccess(true);
                                break;
                            }
                            message.EventDetails.EventMessageType = EventMessageType.Other;
                            message.SetParseSuccess(true);
                            break;
                        case 3:
                            // Equipment changes, recast times, /search results
                            message.EventDetails.EventMessageType = EventMessageType.Other;
                            message.SetParseSuccess(true);
                            break;
                        default:
                            // Other
                            message.EventDetails.EventMessageType = EventMessageType.Other;
                            break;
                    }
                    break;
                // Item/gil obtained
                case 0x7f:
                    lootOrXP = ParseExpressions.GetGil.Match(message.CurrentMessageText);
                    if (lootOrXP.Success == true)
                    {
                        message.EventDetails.EventMessageType = EventMessageType.Loot;
                        message.EventDetails.LootDetails.IsFoundMessage = false;
                        message.EventDetails.LootDetails.Gil = int.Parse(lootOrXP.Groups[ParseFields.Money].Value);
                        message.EventDetails.LootDetails.WhoObtained = lootOrXP.Groups[ParseFields.Name].Value;
                        message.SetParseSuccess(true);
                        break;
                    }
                    lootOrXP = ParseExpressions.GetLoot.Match(message.CurrentMessageText);
                    if (lootOrXP.Success == true)
                    {
                        message.EventDetails.EventMessageType = EventMessageType.Loot;
                        message.EventDetails.LootDetails.IsFoundMessage = false;
                        message.EventDetails.LootDetails.ItemName = lootOrXP.Groups[ParseFields.Item].Value;
                        message.EventDetails.LootDetails.WhoObtained = lootOrXP.Groups[ParseFields.Name].Value;
                        message.SetParseSuccess(true);
                        break;
                    }
                    break;
                // XP with no chain
                case 0x83:
                    lootOrXP = ParseExpressions.ExpChain.Match(message.CurrentMessageText);
                    if (lootOrXP.Success == true)
                    {
                        message.EventDetails.EventMessageType = EventMessageType.Experience;
                        message.EventDetails.ExperienceDetails.ExperienceChain = int.Parse(lootOrXP.Groups[ParseFields.Number].Value);
                        message.SetParseSuccess(true);
                        return;
                    }
                    lootOrXP = ParseExpressions.Experience.Match(message.CurrentMessageText);
                    if (lootOrXP.Success == true)
                    {
                        message.EventDetails.EventMessageType = EventMessageType.Experience;
                        message.EventDetails.ExperienceDetails.ExperiencePoints = int.Parse(lootOrXP.Groups[ParseFields.Number].Value);
                        message.EventDetails.ExperienceDetails.ExperienceRecipient = lootOrXP.Groups[ParseFields.Name].Value;
                        message.SetParseSuccess(true);
                        return;
                    }
                    break;
                // Item found in a chest
                case 0x95:
                    lootOrXP = ParseExpressions.OpenChest.Match(message.CurrentMessageText);
                    if (lootOrXP.Success == true)
                    {
                        message.EventDetails.EventMessageType = EventMessageType.Loot;
                        message.EventDetails.LootDetails.IsFoundMessage = true;
                        message.EventDetails.LootDetails.ItemName = lootOrXP.Groups[ParseFields.Item].Value;
                        message.EventDetails.LootDetails.TargetName = lootOrXP.Groups[ParseFields.Target].Value;
                        message.EventDetails.LootDetails.TargetType = EntityType.TreasureChest;
                        message.SetParseSuccess(true);
                        break;
                    }
                    break;
            }
        }
        #endregion

        #region Combat / Combat sub-category Parsing
        private static void ParseCombat(Message message)
        {
            // Entity type only needs to be resolved when damage is done (codes 14, 19, 1c and 28),
            // with misses (15, 1a, 1d and 29) being included just because they will also be
            // frequent and easily separable.  All other message codes can retrieve the entity
            // type by querying the MessageManager of its collection record.

            // Unresolved entity types can be implicitly resolved (if known player attacks
            // another unidentified entity, that entity must be a mob) even if we don't know
            // the specific code breakdown.

            // If a battle begins with two unresolved entities (say at the start of the parse,
            // and pulling with provoke or elegy, which won't generate a resolution), the
            // battle table entry will have a null value for the Enemy until a mob entity is
            // identified.

            // Once a named entity has been identified, it will always remain that way.
            // Searches in the database must account for Unknown entity types (replacing
            // with new information), or otherwise search on both name and entity type.
            // This is to account for potential name conflicts (Sylvestre mob vs wyvern).

            CombatDetails msgCombatDetails = message.EventDetails.CombatDetails;

            msgCombatDetails.InteractionType = ParseCodes.Instance.GetInteractionType(message.CurrentMessageCode);

            // Swap types for spell-cast drains (ie: non-Additional Effect)
            if (msgCombatDetails.InteractionType == InteractionType.Aid)
            {
                switch (message.CurrentMessageCode)
                {
                    case 0x1e:
                    case 0x22:
                    case 0x2a:
                    case 0xbb:
                        if (ParseExpressions.AdditionalEffect.Match(message.CurrentMessageText).Success == false)
                            msgCombatDetails.InteractionType = InteractionType.Harm;
                        break;
                }
            }

            msgCombatDetails.ActorPlayerType = ParseCodes.Instance.GetActorPlayerType(message.CurrentMessageCode);

            if (msgCombatDetails.InteractionType == InteractionType.Aid)
            {
                msgCombatDetails.AidType = ParseCodes.Instance.GetAidType(message.CurrentMessageCode);
                msgCombatDetails.SuccessLevel = ParseCodes.Instance.GetSuccessType(message.CurrentMessageCode);
            }

            if (msgCombatDetails.InteractionType == InteractionType.Harm)
            {
                msgCombatDetails.HarmType = ParseCodes.Instance.GetHarmType(message.CurrentMessageCode);
                msgCombatDetails.SuccessLevel = ParseCodes.Instance.GetSuccessType(message.CurrentMessageCode);
            }

            if ((msgCombatDetails.HarmType == HarmType.Unknown) ||
                (msgCombatDetails.AidType == AidType.Unknown))
            {
                if (ParseStealing(message) == true)
                    return;
            }

            try
            {
                switch (msgCombatDetails.InteractionType)
                {
                    case InteractionType.Aid:
                        ParseCombatBuffs(message);
                        break;
                    case InteractionType.Harm:
                        ParseCombatAttack(message);
                        break;
                    case InteractionType.Death:
                        ParseDeath(message);
                        break;
                    case InteractionType.Unknown:
                        ParseCombatUnknown(message);
                        break;
                }

            }
            finally
            {
                ClassifyEntity.VerifyAllEntities(ref message, msgCombatDetails.InteractionType == InteractionType.Death);
            }
        }

        /// <summary>
        /// Break out the different types of attack messages for further parsing.
        /// </summary>
        /// <param name="message"></param>
        private static void ParseCombatAttack(Message message)
        {
            switch (message.EventDetails.CombatDetails.HarmType)
            {
                case HarmType.Damage:
                    ParseAttackDamage(message);
                    break;
                case HarmType.Enfeeble:
                    ParseAttackEnfeeble(message);
                    break;
                case HarmType.Drain:
                case HarmType.Aspir:
                    ParseAttackDrain(message);
                    break;
                case HarmType.Unknown:
                    ParseAttackUnknown(message);
                    break;
            }
        }

        /// <summary>
        /// Parse combat buff messages.
        /// </summary>
        /// <param name="message"></param>
        private static void ParseCombatBuffs(Message message)
        {
            Match combatMatch = null;
            CombatDetails msgCombatDetails = message.EventDetails.CombatDetails;
            string currentMessageText = message.CurrentMessageText;
            TargetDetails target = null;

            switch (msgCombatDetails.SuccessLevel)
            {
                case SuccessType.Successful:
                    // Check first for lines where the buff is activated
                    // eg: Player casts Protectra IV.
                    combatMatch = ParseExpressions.CastSpell.Match(currentMessageText);
                    if (combatMatch.Success == true)
                    {
                        msgCombatDetails.ActionType = ActionType.Spell;
                        msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                        msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                        message.SetParseSuccess(true);
                        return;
                    }
                    // eg: Player uses warcry
                    if (msgCombatDetails.AidType != AidType.Item)
                    {
                        combatMatch = ParseExpressions.UseAbility.Match(currentMessageText);
                        if (combatMatch.Success == true)
                        {
                            msgCombatDetails.ActionType = ActionType.Ability;
                            msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                            msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                            message.SetParseSuccess(true);

                            if ((msgCombatDetails.ActionName == "Steal") || (msgCombatDetails.ActionName == "Mug"))
                                message.EventDetails.EventMessageType = EventMessageType.Steal;

                            return;
                        }
                    }

                    switch (msgCombatDetails.AidType)
                    {
                        case AidType.Recovery:
                            combatMatch = ParseExpressions.RecoversHP.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                                target.Amount = int.Parse(combatMatch.Groups[ParseFields.Number].Value);
                                target.AidType = msgCombatDetails.AidType;
                                target.RecoveryType = RecoveryType.RecoverHP;
                                message.SetParseSuccess(true);
                                return;
                            }
                            combatMatch = ParseExpressions.RecoversMP.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                                target.Amount = int.Parse(combatMatch.Groups[ParseFields.Number].Value);
                                target.AidType = msgCombatDetails.AidType;
                                target.RecoveryType = RecoveryType.RecoverMP;
                                message.SetParseSuccess(true);
                                return;
                            }
                            combatMatch = ParseExpressions.Drain.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                                target.Amount = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
                                target.AidType = msgCombatDetails.AidType;
                                switch (combatMatch.Groups[ParseFields.Fulltarget].Value)
                                {
                                    case "HP":
                                        target.RecoveryType = RecoveryType.RecoverHP;
                                        break;
                                    case "MP":
                                        target.RecoveryType = RecoveryType.RecoverMP;
                                        break;
                                    case "TP":
                                        target.RecoveryType = RecoveryType.RecoverTP;
                                        break;
                                }
                                message.SetParseSuccess(true);
                                return;
                            }
                            // Check for additional effect: drain effects (eg: drain samba)
                            combatMatch = ParseExpressions.AdditionalDrain.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = msgCombatDetails.Targets.Find(t => t.Name == combatMatch.Groups[ParseFields.Target].Value);
                                if (target != null)
                                {
                                    msgCombatDetails.InteractionType = InteractionType.Harm;
                                    target.SecondaryAidType = AidType.Recovery;
                                    target.SecondaryRecoveryType = RecoveryType.RecoverHP;
                                    target.SecondaryAmount = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
                                }
                                message.SetParseSuccess(true);
                                return;
                            }
                            // Check for additional effect: aspir effects (eg: aspir samba)
                            combatMatch = ParseExpressions.AdditionalAspir.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = msgCombatDetails.Targets.Find(t => t.Name == combatMatch.Groups[ParseFields.Target].Value);
                                if (target != null)
                                {
                                    msgCombatDetails.InteractionType = InteractionType.Harm;
                                    target.SecondaryAidType = AidType.Recovery;
                                    target.SecondaryRecoveryType = RecoveryType.RecoverMP;
                                    target.SecondaryAmount = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
                                }
                                message.SetParseSuccess(true);
                                return;
                            }

                            combatMatch = ParseExpressions.AdditionalHeal.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = msgCombatDetails.Targets.Find(t => t.Name == combatMatch.Groups[ParseFields.Target].Value);
                                if (target != null)
                                {
                                    msgCombatDetails.InteractionType = InteractionType.Harm;
                                    target.SecondaryAidType = AidType.None;
                                    target.SecondaryRecoveryType = RecoveryType.RecoverHP;
                                    target.SecondaryHarmType = HarmType.Damage;
                                    // Enter negative value for healing the mob with the attack
                                    target.SecondaryAmount = (0 - int.Parse(combatMatch.Groups[ParseFields.Damage].Value));
                                }
                                message.SetParseSuccess(true);
                                return;
                            }
                            // Check for additional effect: drain effects (eg: drain samba)
                            combatMatch = ParseExpressions.DreadSpikes.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = msgCombatDetails.Targets.Find(t => t.Name == combatMatch.Groups[ParseFields.Target].Value);
                                if (target == null)
                                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);

                                if (string.IsNullOrEmpty(msgCombatDetails.ActorName))
                                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;

                                msgCombatDetails.InteractionType = InteractionType.Harm;
                                msgCombatDetails.HarmType = HarmType.Drain;
                                msgCombatDetails.ActionType = ActionType.Spikes;

                                target.Amount = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
                                target.HarmType = HarmType.Drain;

                                message.SetParseSuccess(true);
                                return;
                            }
                            break;
                        case AidType.Enhance:
                            // Corsair rolls
                            combatMatch = ParseExpressions.UseCorRoll.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                                msgCombatDetails.ActionType = ActionType.Ability;
                                msgCombatDetails.CorsairRoll = int.Parse(combatMatch.Groups[ParseFields.Number].Value);
                                message.SetParseSuccess(true);
                                return;
                            }
                            combatMatch = ParseExpressions.TotalCorRoll.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                                msgCombatDetails.ActionType = ActionType.Ability;
                                msgCombatDetails.CorsairRoll = int.Parse(combatMatch.Groups[ParseFields.Number].Value);
                                message.SetParseSuccess(true);
                                return;
                            }
                            combatMatch = ParseExpressions.UseCover.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                                msgCombatDetails.ActionName = "Cover";
                                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                                target.AidType = msgCombatDetails.AidType;
                                target.EffectName = "Cover";
                                message.SetParseSuccess(true);
                                return;
                            }
                            // target gains a buff
                            combatMatch = ParseExpressions.Buff.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                                if (msgCombatDetails.ActionName == string.Empty)
                                    msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Effect].Value;
                                target.AidType = msgCombatDetails.AidType;
                                target.EffectName = combatMatch.Groups[ParseFields.Effect].Value;
                                message.SetParseSuccess(true);
                                return;
                            }
                            // eg: Target's attacks are enhanced.
                            combatMatch = ParseExpressions.Enhance.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                                if (msgCombatDetails.ActionName == string.Empty)
                                    msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Effect].Value;
                                target.AidType = msgCombatDetails.AidType;
                                target.EffectName = "EnhanceAttack";
                                message.SetParseSuccess(true);
                                return;
                            }
                            // eg: Target's attacks are enhanced.
                            combatMatch = ParseExpressions.GainCorRoll.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Name].Value);
                                target.EffectName = combatMatch.Groups[ParseFields.Ability].Value;
                                target.AidType = msgCombatDetails.AidType;
                                target.Amount = msgCombatDetails.CorsairRoll;
                                message.SetParseSuccess(true);
                                return;
                            }
                            combatMatch = ParseExpressions.GainResistance.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                                target.EffectName = combatMatch.Groups[ParseFields.Effect].Value;
                                target.AidType = msgCombatDetails.AidType;
                                message.SetParseSuccess(true);
                                return;
                            }
                            combatMatch = ParseExpressions.RemoveStatus.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                                target.EffectName = combatMatch.Groups[ParseFields.Effect].Value;
                                target.SecondaryAction = combatMatch.Groups[ParseFields.Effect].Value;
                                target.AidType = AidType.RemoveStatus;
                                message.SetParseSuccess(true);
                                return;
                            }
                            combatMatch = ParseExpressions.Dispelled.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                // Erased
                                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                                target.EffectName = combatMatch.Groups[ParseFields.Effect].Value;
                                target.SecondaryAction = combatMatch.Groups[ParseFields.Effect].Value;
                                target.AidType = AidType.RemoveStatus;
                                message.SetParseSuccess(true);
                                return;
                            }
                            combatMatch = ParseExpressions.StealEnmity.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                                target.AidType = AidType.RemoveEnmity;
                                message.SetParseSuccess(true);
                                return;
                            }
                            combatMatch = ParseExpressions.Hide.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                // Player hides
                                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                                msgCombatDetails.ActionName = "Hide";
                                msgCombatDetails.ActionType = ActionType.Ability;
                                message.SetParseSuccess(true);
                                return;
                            }
                            // Self-target buffs have various strings which we won't check for.
                            // Only look to see if the message has determined an actor and action, and that
                            // the message line completes the sentence (ends in a period).
                            if (currentMessageText.EndsWith("."))
                            {
                                if ((msgCombatDetails.ActorName != string.Empty) &&
                                    (msgCombatDetails.ActionName != string.Empty))
                                {
                                    target = msgCombatDetails.AddTarget(msgCombatDetails.ActorName);
                                    target.AidType = msgCombatDetails.AidType;
                                    message.SetParseSuccess(true);
                                }
                                return;
                            }
                            break;
                        case AidType.Item:
                            // eg: Player uses a rolanberry pie.
                            combatMatch = ParseExpressions.UseItem.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                                msgCombatDetails.ActorEntityType = EntityType.Player;
                                msgCombatDetails.ItemName = combatMatch.Groups[ParseFields.Item].Value;
                                message.SetParseSuccess(true);
                                return;
                            }
                            // Uses reraise earring; receives the effect of reraise
                            combatMatch = ParseExpressions.ItemBuff.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                                target.HarmType = msgCombatDetails.HarmType;
                                target.AidType = msgCombatDetails.AidType;
                                target.EffectName = combatMatch.Groups[ParseFields.Effect].Value;
                                message.SetParseSuccess(true);
                                return;
                            }
                            // Uses echo drops; is no longer silenced
                            combatMatch = ParseExpressions.ItemCleanse.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                                target.HarmType = HarmType.None;
                                target.AidType = msgCombatDetails.AidType;
                                target.EffectName = combatMatch.Groups[ParseFields.Effect].Value;
                                message.SetParseSuccess(true);
                                return;
                            }
                            // Uses demoralizer
                            combatMatch = ParseExpressions.ReduceTP.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                                target.HarmType = HarmType.None;
                                target.AidType = msgCombatDetails.AidType;
                                message.SetParseSuccess(true);
                                return;
                            }
                            // Uses poison potion; is poisoned
                            combatMatch = ParseExpressions.Enfeeble.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                                target.HarmType = HarmType.Enfeeble;
                                target.AidType = msgCombatDetails.AidType;
                                target.EffectName = combatMatch.Groups[ParseFields.Effect].Value;
                                message.SetParseSuccess(true);
                                return;
                            }
                            break;
                        case AidType.Unknown:
                            Logger.Instance.Log("Successful Buff, AidType.Unknown", message);
                            break;
                    }
                    break;
                case SuccessType.Unsuccessful:
                    Logger.Instance.Log("Unsuccessful Buff", message);
                    switch (msgCombatDetails.AidType)
                    {
                        case AidType.Recovery:
                            break;
                        case AidType.Enhance:
                            break;
                        case AidType.Item:
                            break;
                        case AidType.Unknown:
                            break;
                    }
                    break;
                case SuccessType.Failed:
                    switch (msgCombatDetails.AidType)
                    {
                        case AidType.Recovery:
                            Logger.Instance.Log("Failed Recovery", message);
                            break;
                        case AidType.Enhance:
                            combatMatch = ParseExpressions.NoEffect.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                                target.FailedActionType = FailedActionType.NoEffect;
                                target.AidType = msgCombatDetails.AidType;
                                message.SetParseSuccess(true);
                                return;
                            }
                            break;
                        case AidType.Item:
                            combatMatch = ParseExpressions.FailActivate.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                msgCombatDetails.ItemName = combatMatch.Groups[ParseFields.Item].Value;
                                target.FailedActionType = FailedActionType.FailedToActivate;
                                target.AidType = msgCombatDetails.AidType;
                                message.SetParseSuccess(true);
                                return;
                            }
                            break;
                        case AidType.Unknown:
                            Logger.Instance.Log("Failed, Unknown Aid type", message);
                            break;
                    }
                    break;
                case SuccessType.Unknown:
                    switch (msgCombatDetails.AidType)
                    {
                        case AidType.Recovery:
                            break;
                        case AidType.Enhance:
                            break;
                        case AidType.Item:
                            break;
                        case AidType.Unknown:
                            combatMatch = ParseExpressions.PrepSpellOn.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                msgCombatDetails.IsPreparing = true;
                                msgCombatDetails.ActionType = ActionType.Spell;
                                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                                // Since we have a target, try to determine the interaction type
                                if ((msgCombatDetails.ActorEntityType != EntityType.Unknown) &&
                                    (target.EntityType != EntityType.Unknown))
                                {
                                    if ((msgCombatDetails.ActorEntityType == EntityType.Mob) ||
                                        (target.EntityType == EntityType.Mob))
                                    {
                                        if ((msgCombatDetails.ActorEntityType == EntityType.Mob) &&
                                            (target.EntityType == EntityType.Mob))
                                        {
                                            // If both are mobs, aid type between mobs
                                            msgCombatDetails.InteractionType = InteractionType.Aid;
                                        }
                                        else
                                        {
                                            // One is mob, another is player/pet/etc, therefore must be harm
                                            msgCombatDetails.InteractionType = InteractionType.Harm;
                                        }
                                    }
                                    else
                                    {
                                        // Neither is a mob, therefore must be aid
                                        msgCombatDetails.InteractionType = InteractionType.Aid;
                                    }
                                }
                                if (msgCombatDetails.InteractionType == InteractionType.Harm)
                                {
                                    msgCombatDetails.HarmType = HarmType.Unknown;
                                    target.HarmType = HarmType.Unknown;
                                }
                                if (msgCombatDetails.InteractionType == InteractionType.Aid)
                                {
                                    msgCombatDetails.AidType = AidType.Unknown;
                                    target.AidType = AidType.Unknown;
                                }
                                message.SetParseSuccess(true);
                                return;
                            }
                            combatMatch = ParseExpressions.PrepSpell.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                msgCombatDetails.IsPreparing = true;
                                msgCombatDetails.ActionType = ActionType.Spell;
                                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                                message.SetParseSuccess(true);
                                return;
                            }
                            combatMatch = ParseExpressions.PrepAbility.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                msgCombatDetails.IsPreparing = true;
                                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                                if (Weaponskills.NamesList.Contains(msgCombatDetails.ActionName))
                                    msgCombatDetails.ActionType = ActionType.Weaponskill;
                                else
                                    msgCombatDetails.ActionType = ActionType.Ability;
                                message.SetParseSuccess(true);
                                return;
                            }
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// Parse messages for death events.
        /// </summary>
        /// <param name="message"></param>
        private static void ParseDeath(Message message)
        {
            Match combatMatch = null;
            CombatDetails combatDetails = message.EventDetails.CombatDetails;
            TargetDetails target = null;

            combatMatch = ParseExpressions.Defeat.Match(message.CurrentMessageText);
            if (combatMatch.Success == true)
            {
                combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);

                switch (combatDetails.ActorEntityType)
                {
                    case EntityType.Player:
                        combatDetails.ActorPlayerType = ParseCodes.Instance.GetActorPlayerType(message.CurrentMessageCode);
                        break;
                    case EntityType.Pet:
                        combatDetails.ActorPlayerType = ActorPlayerType.Other;
                        break;
                    default:
                        combatDetails.ActorPlayerType = ActorPlayerType.Unknown;
                        break;
                }

                message.SetParseSuccess(true);
                return;
            }

            combatMatch = ParseExpressions.Defeated.Match(message.CurrentMessageText);
            if (combatMatch.Success == true)
            {
                combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                message.SetParseSuccess(true);
                return;
            }

            combatMatch = ParseExpressions.Dies.Match(message.CurrentMessageText);
            if (combatMatch.Success == true)
            {
                target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                message.SetParseSuccess(true);
                return;
            }
        }

        /// <summary>
        /// Parse messages that haven't been identified as any particular combat type.
        /// Usually preparing abilities/spells where we don't know if it's a buff or
        /// an attack.
        /// </summary>
        /// <param name="message"></param>
        private static void ParseCombatUnknown(Message message)
        {
            Match combatMatch = null;
            CombatDetails msgCombatDetails = message.EventDetails.CombatDetails;
            TargetDetails target = null;

            // Failed enfeebles or enhancements.  IE: <spell> had no effect.
            combatMatch = ParseExpressions.NoEffect.Match(message.CurrentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.ActionType = ActionType.Spell;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                msgCombatDetails.FailedActionType = FailedActionType.NoEffect;

                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                target.FailedActionType = FailedActionType.NoEffect;

                if (msgCombatDetails.ActorEntityType == target.EntityType)
                {
                    msgCombatDetails.InteractionType = InteractionType.Aid;

                    if ((msgCombatDetails.ActionName == Resources.ParsedStrings.Erase) ||
                        (msgCombatDetails.ActionName == Resources.ParsedStrings.HealingWaltz) ||
                        (Regex.Match(msgCombatDetails.ActionName, Resources.ParsedStrings.NaRegex).Success))
                        target.AidType = AidType.RemoveStatus;
                    else
                        target.AidType = AidType.Enhance;

                    msgCombatDetails.AidType = target.AidType;
                    target.HarmType = HarmType.None;
                }
                else
                {
                    msgCombatDetails.InteractionType = InteractionType.Harm;

                    if ((msgCombatDetails.ActionName == Resources.ParsedStrings.Dispel) ||
                        (msgCombatDetails.ActionName == Resources.ParsedStrings.MagicFinale))
                        target.HarmType = HarmType.Dispel;
                    else
                        target.HarmType = HarmType.Enfeeble;

                    msgCombatDetails.HarmType = target.HarmType;
                    target.AidType = AidType.None;
                }

                msgCombatDetails.SuccessLevel = SuccessType.Failed;
                message.SetParseSuccess(true);
                return;
            }

            // Resisted spell

            combatMatch = ParseExpressions.ResistSpell.Match(message.CurrentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.ActionType = ActionType.Spell;
                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                target.DefenseType = DefenseType.Resist;
                target.HarmType = msgCombatDetails.HarmType;

                msgCombatDetails.SuccessLevel = SuccessType.Unsuccessful;
                message.SetParseSuccess(true);
                return;
            }


            // Prepping spell or ability of unknown type

            combatMatch = ParseExpressions.PrepSpellOn.Match(message.CurrentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.IsPreparing = true;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                msgCombatDetails.ActionType = ActionType.Spell;
                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);

                // Since we have a target, try to determine the interaction type
                if ((msgCombatDetails.ActorEntityType != EntityType.Unknown) &&
                    (target.EntityType != EntityType.Unknown))
                {
                    if ((msgCombatDetails.ActorEntityType == EntityType.Mob) ||
                        (target.EntityType == EntityType.Mob))
                    {
                        if ((msgCombatDetails.ActorEntityType == EntityType.Mob) &&
                            (target.EntityType == EntityType.Mob))
                        {
                            // If both are mobs, aid type between mobs
                            msgCombatDetails.InteractionType = InteractionType.Aid;
                        }
                        else
                        {
                            // One is mob, another is player/pet/etc, therefore must be harm
                            msgCombatDetails.InteractionType = InteractionType.Harm;
                        }
                    }
                    else
                    {
                        // Neither is a mob, therefore it's either aid, or
                        // a Harm action against an NM with a player-like name.
                        // We can't tell which is which at this point, so setting
                        // to Unknown.
                        //msgCombatDetails.InteractionType = InteractionType.Unknown;
                        // Shouldn't need to force entity types.  They're drawn from
                        // the EntityManager, and will be specific or Unknown already.
                        //msgCombatDetails.ActorEntityType = EntityType.Unknown;
                        //target.EntityType = EntityType.Unknown;
                    }
                }

                if (msgCombatDetails.InteractionType == InteractionType.Harm)
                {
                    msgCombatDetails.HarmType = HarmType.Unknown;
                    target.HarmType = HarmType.Unknown;
                }

                if (msgCombatDetails.InteractionType == InteractionType.Aid)
                {
                    msgCombatDetails.AidType = AidType.Unknown;
                    target.AidType = AidType.Unknown;
                }

                message.SetParseSuccess(true);
                return;
            }

            combatMatch = ParseExpressions.PrepSpell.Match(message.CurrentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.IsPreparing = true;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                msgCombatDetails.ActionType = ActionType.Spell;
                message.SetParseSuccess(true);
                return;
            }

            combatMatch = ParseExpressions.PrepAbility.Match(message.CurrentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.IsPreparing = true;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;

                switch (message.CurrentMessageCode)
                {
                    case 0x69:
                        // Ability being used on a player
                        if (msgCombatDetails.ActorEntityType == EntityType.Mob)
                        {
                            // by mob; therefore harm type
                            msgCombatDetails.InteractionType = InteractionType.Harm;
                        }
                        else if (msgCombatDetails.ActorEntityType != EntityType.Unknown)
                        {
                            // by player/pet/etc; therefore aid type
                            msgCombatDetails.InteractionType = InteractionType.Aid;
                        }
                        break;
                    case 0x6e:
                        // Ability being used on a mob
                        if (msgCombatDetails.ActorEntityType == EntityType.Mob)
                        {
                            // by mob; therefore aid type
                            msgCombatDetails.InteractionType = InteractionType.Aid;
                        }
                        else if (msgCombatDetails.ActorEntityType != EntityType.Unknown)
                        {
                            // by player/pet/etc; therefore harm type
                            msgCombatDetails.InteractionType = InteractionType.Harm;
                        }
                        break;
                }


                msgCombatDetails.ActionType = ActionType.Ability;

                if (msgCombatDetails.ActorEntityType == EntityType.Player)
                {
                    // Check for weaponskills if actor is a player
                    if (Weaponskills.NamesList.Contains(msgCombatDetails.ActionName))
                        msgCombatDetails.ActionType = ActionType.Weaponskill;
                }

                message.SetParseSuccess(true);
                return;
            }

            combatMatch = ParseExpressions.CastSpell.Match(message.CurrentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.IsPreparing = false;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                msgCombatDetails.ActionType = ActionType.Spell;
                msgCombatDetails.HarmType = HarmType.Unknown;
                message.SetParseSuccess(true);
                return;
            }

            combatMatch = ParseExpressions.UseAbility.Match(message.CurrentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.IsPreparing = false;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                //msgCombatDetails.InteractionType = InteractionType.Harm;
                msgCombatDetails.ActionType = ActionType.Ability;
                //msgCombatDetails.HarmType = HarmType.Enfeeble;
                message.SetParseSuccess(true);
                return;
            }

            combatMatch = ParseExpressions.NoEffect2.Match(message.CurrentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.SuccessLevel = SuccessType.Failed;
                msgCombatDetails.FailedActionType = FailedActionType.NoEffect;

                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                target.FailedActionType = FailedActionType.NoEffect;

                // If we know the entities involved, we can specify the type of interaction.
                // Do we want to do this, or leave it as unknown?
                if ((msgCombatDetails.ActorEntityType == EntityType.Player) &&
                    (target.EntityType == EntityType.Player))
                {
                    msgCombatDetails.InteractionType = InteractionType.Aid;
                    msgCombatDetails.AidType = AidType.Enhance;
                }
                else if ((msgCombatDetails.ActorEntityType == EntityType.Mob) &&
                    (target.EntityType == EntityType.Mob))
                {
                    msgCombatDetails.InteractionType = InteractionType.Aid;
                    msgCombatDetails.AidType = AidType.Enhance;
                }
                else if ((msgCombatDetails.ActorEntityType == EntityType.Player) &&
                    (target.EntityType == EntityType.Mob))
                {
                    msgCombatDetails.InteractionType = InteractionType.Harm;
                    msgCombatDetails.HarmType = HarmType.Enfeeble;
                }
                else if (msgCombatDetails.ActorEntityType == EntityType.Mob)
                {
                    msgCombatDetails.InteractionType = InteractionType.Harm;
                    msgCombatDetails.HarmType = HarmType.Enfeeble;

                    if (target.EntityType == EntityType.Unknown)
                        target.EntityType = EntityType.Player;
                }
                else if ((msgCombatDetails.ActorEntityType == EntityType.Unknown) &&
                    (target.EntityType == EntityType.Unknown))
                {
                    if (msgCombatDetails.ActorName == target.Name)
                    {
                        msgCombatDetails.InteractionType = InteractionType.Aid;
                        msgCombatDetails.AidType = AidType.Enhance;
                        msgCombatDetails.ActorEntityType = EntityType.Player;
                        target.EntityType = EntityType.Player;
                    }
                }

                target.HarmType = msgCombatDetails.HarmType;
                target.AidType = msgCombatDetails.AidType;

                message.SetParseSuccess(true);
                return;
            }

            combatMatch = ParseExpressions.NoEffect3.Match(message.CurrentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.SuccessLevel = SuccessType.Failed;
                msgCombatDetails.FailedActionType = FailedActionType.NoEffect;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionType = ActionType.Spell;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;

                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                target.FailedActionType = FailedActionType.NoEffect;
                target.DefenseType = DefenseType.Resist;

                // If we know the entities involved, we can specify the type of interaction.
                // Do we want to do this, or leave it as unknown?
                if ((msgCombatDetails.ActorEntityType == EntityType.Player) &&
                    (target.EntityType == EntityType.Player))
                {
                    msgCombatDetails.InteractionType = InteractionType.Aid;
                    msgCombatDetails.AidType = AidType.Enhance;
                }
                else if ((msgCombatDetails.ActorEntityType == EntityType.Mob) &&
                    (target.EntityType == EntityType.Mob))
                {
                    msgCombatDetails.InteractionType = InteractionType.Aid;
                    msgCombatDetails.AidType = AidType.Enhance;
                }
                else if ((msgCombatDetails.ActorEntityType == EntityType.Player) &&
                    (target.EntityType == EntityType.Mob))
                {
                    msgCombatDetails.InteractionType = InteractionType.Harm;
                    msgCombatDetails.HarmType = HarmType.Enfeeble;
                }
                else if ((msgCombatDetails.ActorEntityType == EntityType.Mob) &&
                    (target.EntityType == EntityType.Player))
                {
                    msgCombatDetails.InteractionType = InteractionType.Harm;
                    msgCombatDetails.HarmType = HarmType.Enfeeble;
                }

                target.HarmType = msgCombatDetails.HarmType;
                target.AidType = msgCombatDetails.AidType;

                message.SetParseSuccess(true);
                return;
            }
        }
        #endregion

        #region Attack / sub-category Parsing
        private static void ParseAttackDamage(Message message)
        {
            switch (message.EventDetails.CombatDetails.SuccessLevel)
            {
                case SuccessType.Successful:
                    ParseSuccessfulDamageAttack(message);
                    break;
                case SuccessType.Unsuccessful:
                    ParseUnuccessfulDamageAttack(message);
                    break;
            }
        }

        private static void ParseSuccessfulDamageAttack(Message message)
        {
            Match combatMatch = null;
            CombatDetails combatDetails = message.EventDetails.CombatDetails;
            string currentMessageText = message.CurrentMessageText;
            TargetDetails target = null;

            // Check for Cover!
            Match coverMatch = ParseExpressions.Cover.Match(currentMessageText);
            if (coverMatch.Success == true)
            {
                currentMessageText = coverMatch.Groups[ParseFields.Remainder].Value;
                combatDetails.FlagCover = true;
            }

            // Make all the type checks up front

            // First up are the first-pass entries of possible multi-line messages.

            // Ranged attack before melee because it follows the same form
            combatMatch = ParseExpressions.RangedAttack.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                combatDetails.ActionType = ActionType.Ranged;
                combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                target.Amount = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.RangedGoodSpot.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionType = ActionType.Ranged;
                    combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.Amount = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.RangedSweetSpot.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionType = ActionType.Ranged;
                    combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.Amount = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.MeleeHit.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionType = ActionType.Melee;
                    combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.Amount = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.Skillchain.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActorName = string.Format("SC: {0}", combatMatch.Groups[ParseFields.SC].Value);
                    combatDetails.ActorEntityType = EntityType.Skillchain;
                    combatDetails.ActionType = ActionType.Skillchain;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.RangedHit.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionType = ActionType.Ranged;
                    combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.Counter.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionType = ActionType.Counterattack;
                    combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.Amount = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.Retaliate.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionType = ActionType.Retaliation;
                    combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.Amount = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.Spikes.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionType = ActionType.Spikes;
                    combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.Amount = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.CastSpell.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionType = ActionType.Spell;
                    combatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                    combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.UseAbility.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                    combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;

                    if (Weaponskills.NamesList.Contains(combatDetails.ActionName))
                        combatDetails.ActionType = ActionType.Weaponskill;
                    else
                        combatDetails.ActionType = ActionType.Ability;

                    if (JobAbilities.StealJAs.Contains(combatDetails.ActionName))
                        message.EventDetails.EventMessageType = EventMessageType.Steal;

                    if (JobAbilities.EnfeebleJAs.Contains(combatDetails.ActionName))
                        combatDetails.HarmType = HarmType.Enfeeble;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.RangedCriticalHit.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.FlagCrit = true;
                    combatDetails.ActionType = ActionType.Ranged;
                    combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.CriticalHit.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.FlagCrit = true;
                    combatDetails.ActionType = ActionType.Melee;
                    combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                }
            }


            // Handle this group first
            if (combatMatch.Success == true)
            {
                if (target != null)
                {
                    target.HarmType = combatDetails.HarmType;
                    target.AidType = combatDetails.AidType;

                    if (combatDetails.FlagCover == true)
                    {
                        if (target.SecondaryAction == string.Empty)
                            target.SecondaryAction = "Cover";
                    }
                }

                message.SetParseSuccess(true);
                return;
            }


            // These are second-pass entries for multi-line messages.

            // Takes damage from: crits, magic bursts, additional effects
            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.TargetTakesDamage.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    // Check for modifiers
                    Match modifyMatch = ParseExpressions.AdditionalEffect.Match(currentMessageText);
                    if (modifyMatch.Success == true)
                    {
                        target = combatDetails.Targets.Find(t => t.Name == combatMatch.Groups[ParseFields.Target].Value);
                        if (target != null)
                        {
                            target.SecondaryHarmType = HarmType.Damage;
                            target.SecondaryAmount = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
                        }
                    }
                    else
                    {
                        modifyMatch = ParseExpressions.MagicBurst.Match(currentMessageText);
                        if (modifyMatch.Success == true)
                        {
                            target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                            target.DamageModifier = DamageModifier.MagicBurst;
                            target.Amount = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
                        }
                    }

                    if (target == null)
                        target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);

                    if (combatDetails.FlagCrit == true)
                        target.DamageModifier = DamageModifier.Critical;

                    if (modifyMatch.Success == false)
                        target.Amount = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
                }
            }

            // Violent Flourish: damage + stun
            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.DamageAndStun.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    target = combatDetails.Targets.Find(t => t.Name == combatMatch.Groups[ParseFields.Target].Value);
                    if (target == null)
                        target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);

                    combatDetails.HarmType = HarmType.Damage;
                    combatDetails.AidType = AidType.None;

                    target.HarmType = HarmType.Damage;
                    target.Amount = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
                    target.SecondaryHarmType = HarmType.Enfeeble;
                    target.SecondaryAction = Resources.ParsedStrings.Stun;
                }
            }

            // Check for limited additional damage
            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.AdditionalDamage.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    target = combatDetails.Targets.LastOrDefault();
                    if (target != null)
                    {
                        target.SecondaryHarmType = HarmType.Damage;
                        target.SecondaryAmount = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
                    }
                }
            }
             
            // Check for additional effect: status effects
            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.AdditionalStatus.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    target = combatDetails.Targets.LastOrDefault();
                    if (target != null)
                    {
                        target.SecondaryHarmType = HarmType.Enfeeble;
                        target.SecondaryAction = combatMatch.Groups[ParseFields.Effect].Value;
                    }
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.AdditionalHeal.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    target = combatDetails.Targets.Find(t => t.Name == combatMatch.Groups[ParseFields.Target].Value);
                    if (target != null)
                    {
                        target.SecondaryHarmType = HarmType.Damage;
                        // Enter negative value for healing the mob with the attack
                        target.SecondaryAmount = (0 - int.Parse(combatMatch.Groups[ParseFields.Damage].Value));
                    }
                }
            }


            // Handle entity settings on additional targets
            if (combatMatch.Success == true)
            {
                message.SetParseSuccess(true);

                if (target != null)
                {
                    target.HarmType = combatDetails.HarmType;
                    target.AidType = combatDetails.AidType;

                    if (combatDetails.FlagCover == true)
                    {
                        if (target.SecondaryAction == string.Empty)
                            target.SecondaryAction = "Cover";
                    }
                }
            }
        }

        private static void ParseUnuccessfulDamageAttack(Message message)
        {
            Match combatMatch = null;
            CombatDetails combatDetails = message.EventDetails.CombatDetails;
            string currentMessageText = message.CurrentMessageText;
            TargetDetails target = null;

            // Check for Cover!
            Match coverMatch = ParseExpressions.Cover.Match(currentMessageText);
            if (coverMatch.Success == true)
            {
                currentMessageText = coverMatch.Groups[ParseFields.Remainder].Value;
                combatDetails.FlagCover = true;
            }


            // First lines of multi-line values

            combatMatch = ParseExpressions.CastSpell.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                combatDetails.ActionType = ActionType.Spell;
                combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                combatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.UseAbility.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    combatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                    if (Weaponskills.NamesList.Contains(combatDetails.ActionName))
                        combatDetails.ActionType = ActionType.Weaponskill;
                    else
                        combatDetails.ActionType = ActionType.Ability;

                    // Make corrections for certain special actions that are really enfeebles
                    if (JobAbilities.CharmJAs.Contains(combatDetails.ActionName) ||
                        JobAbilities.EnfeebleJAs.Contains(combatDetails.ActionName))
                        combatDetails.HarmType = HarmType.Enfeeble;

                    if (JobAbilities.StealJAs.Contains(combatDetails.ActionName))
                        message.EventDetails.EventMessageType = EventMessageType.Steal;
                }
            }

            // Standalone and followup message lines

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.MeleeMiss.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionType = ActionType.Melee;
                    target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.DefenseType = DefenseType.Evasion;
                    target.HarmType = combatDetails.HarmType;
                    combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.RangedMiss.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionType = ActionType.Ranged;
                    target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.DefenseType = DefenseType.Evasion;
                    target.HarmType = combatDetails.HarmType;
                    combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.RangedMiss2.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionType = ActionType.Ranged;
                    combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    // No target available in this message. Create a bogus target to hold state.
                    target = new TargetDetails();
                    combatDetails.Targets.Add(target);
                    target.DefenseType = DefenseType.Evasion;
                    target.HarmType = combatDetails.HarmType;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.MissAbility.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                    if (Weaponskills.NamesList.Contains(combatDetails.ActionName))
                    {
                        combatDetails.ActionType = ActionType.Weaponskill;
                        combatDetails.HarmType = HarmType.Damage;
                    }
                    else
                        combatDetails.ActionType = ActionType.Ability;
                    target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.DefenseType = DefenseType.Evasion;
                    target.HarmType = combatDetails.HarmType;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.Blink.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    // default value is already Unknown.  Don't overwrite pre-set values.
                    //combatDetails.ActionType = ActionType.Unknown;
                    target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.DefenseType = DefenseType.Shadow;
                    target.HarmType = combatDetails.HarmType;
                    target.ShadowsUsed = byte.Parse(combatMatch.Groups[ParseFields.Number].Value);
                    // Multiple shadows wiped means it must have been an ability attack.
                    if (target.ShadowsUsed > 1)
                        combatDetails.ActionType = ActionType.Ability;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.CounterShadow.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionType = ActionType.Counterattack;
                    combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.DefenseType = DefenseType.Shadow;
                    target.HarmType = combatDetails.HarmType;
                    target.ShadowsUsed = byte.Parse(combatMatch.Groups[ParseFields.Number].Value);
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.RetaliateShadow.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionType = ActionType.Retaliation;
                    combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.DefenseType = DefenseType.Shadow;
                    target.HarmType = combatDetails.HarmType;
                    target.ShadowsUsed = byte.Parse(combatMatch.Groups[ParseFields.Number].Value);
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.Parry.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionType = ActionType.Unknown;
                    target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.DefenseType = DefenseType.Parry;
                    target.HarmType = combatDetails.HarmType;
                    combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.Anticipate2.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionType = ActionType.Unknown;
                    target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.DefenseType = DefenseType.Anticipate;
                    target.HarmType = combatDetails.HarmType;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.Anticipate.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionType = ActionType.Unknown;
                    target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.DefenseType = DefenseType.Anticipate;
                    target.HarmType = combatDetails.HarmType;
                    combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.Evade.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionType = ActionType.Unknown;
                    target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.DefenseType = DefenseType.Evade;
                    target.HarmType = combatDetails.HarmType;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.MeleeDodge.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionType = ActionType.Melee;
                    target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.DefenseType = DefenseType.Evasion;
                    target.HarmType = HarmType.Damage;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.ResistSpell.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionType = ActionType.Spell;
                    target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.DefenseType = DefenseType.Resist;
                    target.HarmType = combatDetails.HarmType;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.ResistEffect.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionType = ActionType.Ability;
                    target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.DefenseType = DefenseType.Resist;
                    target.HarmType = combatDetails.HarmType;
                }
            }


            if (combatMatch.Success == true)
            {
                if (target != null)
                {
                    if (combatDetails.FlagCover == true)
                    {
                        if (target.SecondaryAction == string.Empty)
                            target.SecondaryAction = "Cover";
                    }
                }

                message.SetParseSuccess(true);
            }
        }

        private static void ParseAttackEnfeeble(Message message)
        {
            Match combatMatch = null;
            CombatDetails msgCombatDetails = message.EventDetails.CombatDetails;
            string currentMessageText = message.CurrentMessageText;
            TargetDetails target = null;

            // Check for Cover!
            Match coverMatch = ParseExpressions.Cover.Match(currentMessageText);
            if (coverMatch.Success == true)
            {
                currentMessageText = coverMatch.Groups[ParseFields.Remainder].Value;
                msgCombatDetails.FlagCover = true;
            }

            try
            {
                // First check for prepping attack spells or abilities
                // eg: Player starts casting Poison on the Mandragora.
                combatMatch = ParseExpressions.PrepSpellOn.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.IsPreparing = true;
                    msgCombatDetails.ActionType = ActionType.Spell;
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.HarmType = msgCombatDetails.HarmType;
                    target.AidType = msgCombatDetails.AidType;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.PrepSpell.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.IsPreparing = true;
                    msgCombatDetails.ActionType = ActionType.Spell;
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.PrepAbility.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.IsPreparing = true;
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                    if (Weaponskills.NamesList.Contains(msgCombatDetails.ActionName))
                        msgCombatDetails.ActionType = ActionType.Weaponskill;
                    else
                        msgCombatDetails.ActionType = ActionType.Ability;
                    message.SetParseSuccess(true);
                    return;
                }

                // Then check for lines where the action is activated
                combatMatch = ParseExpressions.CastSpell.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActionType = ActionType.Spell;
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.UseAbilityOn.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActionType = ActionType.Ability;
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.HarmType = msgCombatDetails.HarmType;
                    target.AidType = msgCombatDetails.AidType;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.UseAbility.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActionType = ActionType.Ability;
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                    message.SetParseSuccess(true);

                    if ((msgCombatDetails.ActionName == "Steal") || (msgCombatDetails.ActionName == "Mug"))
                        message.EventDetails.EventMessageType = EventMessageType.Steal;

                    return;
                }

                // Misses (The Mandragora uses Dream Flower but misses Player.)
                combatMatch = ParseExpressions.MissAbility.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                    if (Weaponskills.NamesList.Contains(msgCombatDetails.ActionName))
                    {
                        msgCombatDetails.ActionType = ActionType.Weaponskill;
                        msgCombatDetails.HarmType = HarmType.Damage;
                    }
                    else
                        msgCombatDetails.ActionType = ActionType.Ability;

                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.DefenseType = DefenseType.Evade;
                    target.HarmType = msgCombatDetails.HarmType;
                    target.AidType = msgCombatDetails.AidType;
                    message.SetParseSuccess(true);
                    return;
                }

                // Then check for individual lines about the effect on the target
                combatMatch = ParseExpressions.Debuff.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.EffectName = combatMatch.Groups[ParseFields.Effect].Value;
                    target.HarmType = msgCombatDetails.HarmType;
                    target.AidType = msgCombatDetails.AidType;
                    message.SetParseSuccess(true);
                    return;
                }

                combatMatch = ParseExpressions.ReduceTP.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.EffectName = "ReduceTP";
                    target.HarmType = msgCombatDetails.HarmType;
                    target.AidType = msgCombatDetails.AidType;
                    message.SetParseSuccess(true);
                    return;
                }

                combatMatch = ParseExpressions.Enfeeble.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.EffectName = combatMatch.Groups[ParseFields.Effect].Value;

                    // If player is Overloaded, this is actually a Failed Action of an Enhancement.
                    if (target.EffectName == Resources.ParsedStrings.Overloaded)
                    {
                        target.FailedActionType = FailedActionType.Overloaded;
                        msgCombatDetails.ActionType = ActionType.Ability;
                        msgCombatDetails.InteractionType = InteractionType.Aid;
                        msgCombatDetails.HarmType = HarmType.None;
                        msgCombatDetails.AidType = AidType.Enhance;
                        msgCombatDetails.FailedActionType = FailedActionType.Overloaded;
                        msgCombatDetails.SuccessLevel = SuccessType.Failed;
                    }
                    target.HarmType = msgCombatDetails.HarmType;
                    target.AidType = msgCombatDetails.AidType;
                    message.SetParseSuccess(true);
                    return;
                }

                combatMatch = ParseExpressions.Dispelled.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.HarmType = HarmType.Dispel;
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.EffectName = combatMatch.Groups[ParseFields.Effect].Value;
                    target.SecondaryAction = combatMatch.Groups[ParseFields.Ability].Value;
                    target.HarmType = HarmType.Dispel;
                    target.AidType = msgCombatDetails.AidType;
                    message.SetParseSuccess(true);
                    return;
                }

                combatMatch = ParseExpressions.BustCorRoll.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.HarmType = HarmType.Dispel;
                    message.SetParseSuccess(true);
                    return;
                }

                combatMatch = ParseExpressions.LoseCorRoll.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Name].Value);
                    target.EffectName = combatMatch.Groups[ParseFields.Ability].Value;
                    target.SecondaryAction = combatMatch.Groups[ParseFields.Ability].Value;
                    target.HarmType = HarmType.Dispel;
                    target.AidType = msgCombatDetails.AidType;
                    message.SetParseSuccess(true);
                    return;
                }

                combatMatch = ParseExpressions.Afflict.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.HarmType = msgCombatDetails.HarmType;
                    target.AidType = msgCombatDetails.AidType;
                    message.SetParseSuccess(true);
                    return;
                }

                combatMatch = ParseExpressions.ResistSpell.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActionType = ActionType.Spell;
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.DefenseType = DefenseType.Resist;
                    target.HarmType = msgCombatDetails.HarmType;
                    target.AidType = msgCombatDetails.AidType;
                    message.SetParseSuccess(true);
                    return;
                }

                combatMatch = ParseExpressions.ResistEffect.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.DefenseType = DefenseType.Resist;
                    target.HarmType = msgCombatDetails.HarmType;
                    target.AidType = msgCombatDetails.AidType;
                    message.SetParseSuccess(true);
                    return;
                }

                // Special handling: Charms
                combatMatch = ParseExpressions.Charmed.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.HarmType = msgCombatDetails.HarmType;
                    target.AidType = msgCombatDetails.AidType;

                    if (JobAbilities.CharmJAs.Contains(msgCombatDetails.ActionName))
                    {
                        // Only for players.  Mobs use other ability names to charm players.
                        // Mob type has been charmed.  Add to the entity lookup list as a pet.
                        EntityManager.Instance.AddCharmedMob(target.Name);
                        EntityManager.Instance.LastCharmedMob = target.Name;
                        target.EntityType = EntityType.CharmedMob;
                        msgCombatDetails.ActorEntityType = EntityType.Player;
                    }
                    message.SetParseSuccess(true);
                    return;
                }

                combatMatch = ParseExpressions.FailsCharm.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActionType = ActionType.Ability;
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.DefenseType = DefenseType.Resist;
                    target.HarmType = msgCombatDetails.HarmType;
                    target.AidType = msgCombatDetails.AidType;
                    message.SetParseSuccess(true);
                    return;
                }

                combatMatch = ParseExpressions.NoEffect.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActionType = ActionType.Spell;
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                    msgCombatDetails.SuccessLevel = SuccessType.Failed;
                    msgCombatDetails.FailedActionType = FailedActionType.NoEffect;
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.FailedActionType = FailedActionType.NoEffect;
                    target.HarmType = msgCombatDetails.HarmType;
                    message.SetParseSuccess(true);
                    return;
                }

                combatMatch = ParseExpressions.NoEffect2.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    if (msgCombatDetails.ActionType == ActionType.Unknown)
                        msgCombatDetails.ActionType = ActionType.Spell;
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.FailedActionType = FailedActionType.NoEffect;
                    target.HarmType = msgCombatDetails.HarmType;
                    msgCombatDetails.SuccessLevel = SuccessType.Failed;
                    if (msgCombatDetails.ActorEntityType == target.EntityType)
                    {
                        if ((msgCombatDetails.ActionName == Resources.ParsedStrings.Erase) ||
                            (msgCombatDetails.ActionName == Resources.ParsedStrings.HealingWaltz) ||
                            (Regex.Match(msgCombatDetails.ActionName, Resources.ParsedStrings.NaRegex).Success))
                            target.AidType = AidType.RemoveStatus;
                        else
                            target.AidType = AidType.Enhance;

                        target.HarmType = HarmType.None;
                    }
                    message.SetParseSuccess(true);
                    return;
                }

                combatMatch = ParseExpressions.NoEffect3.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    // Fails to take effect due to full resist (Blu spells) (?)
                    msgCombatDetails.ActionType = ActionType.Spell;
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.FailedActionType = FailedActionType.NoEffect;
                    //target.HarmType = msgCombatDetails.HarmType;
                    target.HarmType = HarmType.Damage;
                    target.DefenseType = DefenseType.Resist;
                    msgCombatDetails.SuccessLevel = SuccessType.Failed;
                    message.SetParseSuccess(true);
                    return;
                }
            }
            finally
            {
                if (target != null)
                {
                    if (msgCombatDetails.FlagCover == true)
                    {
                        if (target.SecondaryAction == string.Empty)
                            target.SecondaryAction = "Cover";
                    }
                }
            }
        }

        private static void ParseAttackDrain(Message message)
        {
            Match combatMatch = null;
            CombatDetails msgCombatDetails = message.EventDetails.CombatDetails;
            string currentMessageText = message.CurrentMessageText;
            TargetDetails target = null;

            // First check for prepping spells or abilities
            combatMatch = ParseExpressions.PrepSpellOn.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.IsPreparing = true;
                msgCombatDetails.ActionType = ActionType.Spell;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                target.HarmType = msgCombatDetails.HarmType;
                target.AidType = msgCombatDetails.AidType;
                message.SetParseSuccess(true);
                return;
            }
            combatMatch = ParseExpressions.PrepSpell.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.IsPreparing = true;
                msgCombatDetails.ActionType = ActionType.Spell;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                message.SetParseSuccess(true);
                return;
            }
            combatMatch = ParseExpressions.PrepAbility.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.IsPreparing = true;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                if (Weaponskills.NamesList.Contains(msgCombatDetails.ActionName))
                    msgCombatDetails.ActionType = ActionType.Weaponskill;
                else
                    msgCombatDetails.ActionType = ActionType.Ability;
                message.SetParseSuccess(true);
                return;
            }


            // Then check for lines where the action is activated
            combatMatch = ParseExpressions.CastSpell.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.ActionType = ActionType.Spell;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                if (msgCombatDetails.ActionName.StartsWith("Absorb-"))
                    msgCombatDetails.HarmType = HarmType.Enfeeble;
                message.SetParseSuccess(true);
                return;
            }
            combatMatch = ParseExpressions.UseAbility.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.ActionType = ActionType.Ability;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                message.SetParseSuccess(true);

                if ((msgCombatDetails.ActionName == "Steal") || (msgCombatDetails.ActionName == "Mug"))
                    message.EventDetails.EventMessageType = EventMessageType.Steal;

                return;
            }
            combatMatch = ParseExpressions.UseAbilityOn.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.ActionType = ActionType.Ability;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                target.HarmType = msgCombatDetails.HarmType;
                target.AidType = msgCombatDetails.AidType;
                message.SetParseSuccess(true);
                return;
            }

            combatMatch = ParseExpressions.AbsorbStat.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                target.EffectName = combatMatch.Groups[ParseFields.DrainStat].Value;
                target.HarmType = HarmType.Enfeeble;
                target.AidType = AidType.None;
                message.SetParseSuccess(true);
                return;
            }

            // Player drains XXX HP/MP/TP from target.

            combatMatch = ParseExpressions.Drain.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                target.Amount = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
                target.HarmType = HarmType.Drain;
                target.AidType = msgCombatDetails.AidType;

                switch (combatMatch.Groups[ParseFields.DrainType].Value)
                {
                    case "HP":
                        target.RecoveryType = RecoveryType.RecoverHP;
                        break;
                    case "MP":
                        target.HarmType = HarmType.Enfeeble;
                        target.RecoveryType = RecoveryType.RecoverMP;
                        break;
                    case "TP":
                        target.HarmType = HarmType.Enfeeble;
                        target.RecoveryType = RecoveryType.RecoverTP;
                        break;
                }

                msgCombatDetails.SuccessLevel = SuccessType.Successful;

                message.SetParseSuccess(true);
                return;
            }
        }

        private static void ParseAttackUnknown(Message message)
        {
            Match combatMatch = null;
            CombatDetails msgCombatDetails = message.EventDetails.CombatDetails;
            string currentMessageText = message.CurrentMessageText;
            TargetDetails target = null;
            
            #region Failed Actions
            // Deal with possible Failed actions first
            if (msgCombatDetails.SuccessLevel == SuccessType.Failed)
            {
                // Special: Raises are given the same parse code as certain failed actions.
                // Check for them first.
                combatMatch = ParseExpressions.CastSpellOn.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.IsPreparing = false;
                    msgCombatDetails.ActionType = ActionType.Spell;
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                    msgCombatDetails.SuccessLevel = SuccessType.Successful;
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    if (msgCombatDetails.ActionName.StartsWith(Resources.ParsedStrings.Raise))
                    {
                        target.AidType = AidType.Recovery;
                        target.RecoveryType = RecoveryType.Life;
                        target.HarmType = HarmType.None;
                        msgCombatDetails.InteractionType = InteractionType.Aid;
                        msgCombatDetails.AidType = AidType.Recovery;
                        msgCombatDetails.HarmType = HarmType.None;
                        msgCombatDetails.FailedActionType = FailedActionType.None;
                    }
                    message.SetParseSuccess(true);
                    return;
                }

                // Remainder are Failed actions.
                combatMatch = ParseExpressions.AutoTarget.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    msgCombatDetails.FailedActionType = FailedActionType.Autotarget;
                    target.FailedActionType = FailedActionType.Autotarget;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.CannotAttack.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.FailedActionType = FailedActionType.CannotAttack;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.CannotSee.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    msgCombatDetails.FailedActionType = FailedActionType.CannotSee;
                    target.FailedActionType = FailedActionType.CannotSee;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.CannotSee2.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    msgCombatDetails.FailedActionType = FailedActionType.CannotSee;
                    target.FailedActionType = FailedActionType.CannotSee;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.LoseSight.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    msgCombatDetails.FailedActionType = FailedActionType.CannotSee;
                    target.FailedActionType = FailedActionType.CannotSee;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.UnableToCast.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.FailedActionType = FailedActionType.UnableToCast;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.UnableToUse.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.FailedActionType = FailedActionType.UnableToUse;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.UnableToUse2.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.FailedActionType = FailedActionType.UnableToUse;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.UnableToUse3.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.FailedActionType = FailedActionType.UnableToUse;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.NotEnoughMP.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.FailedActionType = FailedActionType.NotEnoughMP;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.NotEnoughTP.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.FailedActionType = FailedActionType.NotEnoughTP;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.NotEnoughTP2.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.FailedActionType = FailedActionType.NotEnoughTP;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.NotEnoughTP3.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.FailedActionType = FailedActionType.NotEnoughTP;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.CannotPerform.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.FailedActionType = FailedActionType.CannotAct;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.TooFarForXP.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.FailedActionType = FailedActionType.TooFarAway;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.TooFarAway.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    msgCombatDetails.FailedActionType = FailedActionType.TooFarAway;
                    target.FailedActionType = FailedActionType.TooFarAway;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.OutOfRange.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    msgCombatDetails.FailedActionType = FailedActionType.OutOfRange;
                    target.FailedActionType = FailedActionType.OutOfRange;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.Interrupted.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.FailedActionType = FailedActionType.Interrupted;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.Paralyzed.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.FailedActionType = FailedActionType.Paralyzed;
                    message.SetParseSuccess(true);
                    return;
                }
                combatMatch = ParseExpressions.Intimidated.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    // Treat intimidation as a defense type, like Parry
                    // Applies to spell casts and melee attacks.  Can we distinguish?
                    // Apply to just melee for now.
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.ActionType = ActionType.Melee;
                    msgCombatDetails.HarmType = HarmType.Damage;
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.HarmType = msgCombatDetails.HarmType;
                    target.AidType = msgCombatDetails.AidType;
                    target.DefenseType = DefenseType.Intimidate;
                    // Adjust message category
                    msgCombatDetails.SuccessLevel = SuccessType.Unsuccessful;
                    // Check for mobs vs pets intimidating each other
                    //ClassifyEntity.VerifyEntities(ref message, ref target, false);
                    message.SetParseSuccess(true);
                    return;
                }

                return;
            }
            #endregion

            // For prepping attack spells or abilities
            combatMatch = ParseExpressions.PrepSpellOn.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.IsPreparing = true;
                msgCombatDetails.ActionType = ActionType.Spell;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                message.SetParseSuccess(true);
                return;
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.PrepSpell.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.IsPreparing = true;
                    msgCombatDetails.ActionType = ActionType.Spell;
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                    message.SetParseSuccess(true);
                    return;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.PrepAbility.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.IsPreparing = true;
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                    if (Weaponskills.NamesList.Contains(msgCombatDetails.ActionName))
                        msgCombatDetails.ActionType = ActionType.Weaponskill;
                    else
                        msgCombatDetails.ActionType = ActionType.Ability;
                    message.SetParseSuccess(true);
                    return;
                }
            }

            // got through the prep checks; clarify certain actions which aren't defined precisely by code
            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.CastSpell.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActionType = ActionType.Spell;
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                    message.SetParseSuccess(true);
                    return;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.UseAbility.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActionType = ActionType.Ability;
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                    message.SetParseSuccess(true);

                    if ((msgCombatDetails.ActionName == "Steal") || (msgCombatDetails.ActionName == "Mug"))
                        message.EventDetails.EventMessageType = EventMessageType.Steal;

                    return;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.ResistSpell.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActionType = ActionType.Spell;
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.DefenseType = DefenseType.Resist;
                    target.HarmType = msgCombatDetails.HarmType;
                    message.SetParseSuccess(true);
                    return;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.NoEffect.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActionType = ActionType.Spell;
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.SuccessLevel = SuccessType.Failed;
                    msgCombatDetails.FailedActionType = FailedActionType.NoEffect;
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    message.SetParseSuccess(true);
                    return;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.MissAbility.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    string possibleActionName = combatMatch.Groups[ParseFields.Ability].Value;

                    if (possibleActionName == "Ranged Attack")
                    {
                        msgCombatDetails.ActionType = ActionType.Ranged;
                        msgCombatDetails.HarmType = HarmType.Damage;
                    }
                    else
                    {
                        msgCombatDetails.ActionName = possibleActionName;
                        if (Weaponskills.NamesList.Contains(msgCombatDetails.ActionName))
                        {
                            msgCombatDetails.ActionType = ActionType.Weaponskill;
                            msgCombatDetails.HarmType = HarmType.Damage;
                        }
                        else
                        {
                            msgCombatDetails.ActionType = ActionType.Ability;

                            if (JobAbilities.EnfeebleJAs.Contains(msgCombatDetails.ActionName))
                                msgCombatDetails.HarmType = HarmType.Enfeeble;
                        }
                    }

                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.DefenseType = DefenseType.Evasion;
                    target.HarmType = msgCombatDetails.HarmType;
                    message.SetParseSuccess(true);
                    return;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.MissAbilityNoTarget.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                    if (Weaponskills.NamesList.Contains(msgCombatDetails.ActionName))
                        msgCombatDetails.ActionType = ActionType.Weaponskill;
                    else
                        msgCombatDetails.ActionType = ActionType.Ability;

                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    message.SetParseSuccess(true);
                    return;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.RangedMiss2.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActionType = ActionType.Ranged;
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    // No target available in this message. Create a bogus target to hold state.
                    target = new TargetDetails();
                    msgCombatDetails.Targets.Add(target);
                    target.DefenseType = DefenseType.Evasion;
                    target.HarmType = msgCombatDetails.HarmType;
                    message.SetParseSuccess(true);
                    return;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.FailHide.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActionType = ActionType.Ability;
                    msgCombatDetails.InteractionType = InteractionType.Aid;
                    msgCombatDetails.AidType = AidType.Enhance;
                    msgCombatDetails.HarmType = HarmType.None;
                    msgCombatDetails.SuccessLevel = SuccessType.Failed;
                    msgCombatDetails.FailedActionType = FailedActionType.Discovered;
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    //target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    message.SetParseSuccess(true);
                    return;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.FailsCharm.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActionType = ActionType.Ability;
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.DefenseType = DefenseType.Resist;
                    target.HarmType = msgCombatDetails.HarmType;
                    target.AidType = msgCombatDetails.AidType;
                    message.SetParseSuccess(true);
                    return;
                }
            }


            // Check for drain messages
            ParseAttackDrain(message);
        }

        #endregion
    }
}
