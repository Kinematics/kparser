using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WaywardGamers.KParser.Parsing
{
    internal static class Parser
    {
        #region Member Variables
        private static ParseCodes parseCodesLookup = new ParseCodes();
        private static uint lastMessageID = 0;
        #endregion

        #region Top-level category breakdown
        /// <summary>
        /// Takes the provided Message and parses its contents, updating
        /// the internal data of the message as it goes.
        /// </summary>
        /// <param name="messageLine"></param>
        internal static Message Parse(MessageLine messageLine)
        {
            Message message = GetAttachedMessage(messageLine);

            if (message == null)
            {
                message = new Message();
                InitialParse(message, messageLine);
                lastMessageID = message.MessageID;
            }
            else
            {
                ContinueParse(message, messageLine);
            }


            if (message.ParseSuccessful == true)
            {
                if ((message.MessageCategory == MessageCategoryType.Action) &&
                    (message.ActionDetails.ActionMessageType == ActionMessageType.Combat) &&
                    ((message.ActionDetails.CombatDetails.CombatCategory == CombatActionType.Attack) ||
                     (message.ActionDetails.CombatDetails.CombatCategory == CombatActionType.Buff)))
                {
                    if ((message.ActionDetails.CombatDetails.ActorName == string.Empty) &&
                        (message.ActionDetails.CombatDetails.SuccessLevel != SuccessType.Failed) &&
                        (message.ActionDetails.CombatDetails.IsPreparing == false))
                    {
                        Message prevMsg = MessageManager.Instance.FindLastMessageWithCode(messageLine.MessageCode);

                        if (prevMsg != null)
                        {
                            foreach (var msgLine in message.MessageLineCollection)
                                prevMsg.AddMessageLine(msgLine);

                            message = prevMsg;
                            ContinueParse(message, null);
                        }
                    }
                }
            }

            return message;
        }

        private static Message GetAttachedMessage(MessageLine messageLine)
        {
            if (messageLine.EventSequence > lastMessageID)
                return null;

            Message msg = MessageManager.Instance.FindMessageWithEventNumber(messageLine.EventSequence);

            return msg;
        }

        private static void InitialParse(Message message, MessageLine messageLine)
        {
            message.MessageID = messageLine.EventSequence;
            message.MessageCode = messageLine.MessageCode;
            message.ExtraCode1 = messageLine.ExtraCode1;
            message.ExtraCode2 = messageLine.ExtraCode2;
            message.MessageCategory = messageLine.MessageCategory;

            message.AddMessageLine(messageLine);

            switch (message.MessageCategory)
            {
                case MessageCategoryType.Chat:
                    ParseChat(message);
                    break;
                case MessageCategoryType.System:
                    ParseSystem(message);
                    break;
                case MessageCategoryType.Action:
                    ParseAction(message);
                    break;
            }
        }

        private static Message ContinueParse(Message message, MessageLine messageLine)
        {
            message.AddMessageLine(messageLine);

            switch (message.MessageCategory)
            {
                case MessageCategoryType.Chat:
                    // No further parsing needed; pull CompleteMessageText when storing to database.
                    break;
                case MessageCategoryType.System:
                    // No further parsing needed since we're only using the message code.
                    break;
                case MessageCategoryType.Action:
                    ParseAction(message);
                    break;
            }
            
            return message;
        }
        #endregion

        #region Parsing of general Message Categories
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
                    message.ChatDetails.ChatMessageType = ChatMessageType.Say;
                    break;
                case 0x02: // <me> shout
                case 0x0a: // Others shout
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
                case 0x98: // <npc> say
                    message.ChatDetails.ChatMessageType = ChatMessageType.Say;
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
                case 0x98: // NPCs say
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
                    chatName = ParseExpressions.ChatSay.Match(message.CurrentMessageText);
                    break;
                case ChatMessageType.Party:
                    chatName = ParseExpressions.ChatParty.Match(message.CurrentMessageText);
                    break;
                case ChatMessageType.Linkshell:
                    chatName = ParseExpressions.ChatLinkshell.Match(message.CurrentMessageText);
                    break;
                case ChatMessageType.Shout:
                    chatName = ParseExpressions.ChatShout.Match(message.CurrentMessageText);
                    break;
                case ChatMessageType.Emote:
                    chatName = ParseExpressions.ChatEmote.Match(message.CurrentMessageText);
                    if (chatName.Success == false)
                        chatName = ParseExpressions.ChatEmoteA.Match(message.CurrentMessageText);
                    break;
                case ChatMessageType.Tell:
                    chatName = ParseExpressions.ChatTell.Match(message.CurrentMessageText);
                    break;
            }

            if ((chatName != null) && (chatName.Success == true))
            {
                message.ChatDetails.ChatSpeakerName = chatName.Groups[ParseFields.Name].Value;
                message.ChatDetails.FullChatText = message.CompleteMessageText;
                message.ParseSuccessful = true;
            }
        }

        /// <summary>
        /// Parse out system messages.
        /// </summary>
        /// <param name="message">The message to parse.</param>
        private static void ParseSystem(Message message)
        {
            switch (message.MessageCode)
            {
                case 0x00:
                    message.SystemDetails.SystemMessageType = SystemMessageType.ZoneChange;
                    break;
                case 0xce:
                    message.SystemDetails.SystemMessageType = SystemMessageType.Echo;
                    break;
                case 0xbf:
                    message.SystemDetails.SystemMessageType = SystemMessageType.EffectWearsOff;
                    
                    Match charmCheck = ParseExpressions.NotCharmed.Match(message.CurrentMessageText);
                    if (charmCheck.Success == true)
                        MessageManager.Instance.RemovePetEntity(charmCheck.Groups[ParseFields.Fulltarget].Value);
                    break;
                case 0xd0:
                    message.SystemDetails.SystemMessageType = SystemMessageType.Examine;
                    break;
                case 0x7b:
                    message.SystemDetails.SystemMessageType = SystemMessageType.OutOfRange;
                    break;
                default:
                    message.SystemDetails.SystemMessageType = SystemMessageType.Unknown;
                    break;
            }

            if (message.SystemDetails.SystemMessageType != SystemMessageType.Unknown)
                message.ParseSuccessful = true;
        }

        /// <summary>
        /// Break down action messages into subgroups for further parsing.
        /// </summary>
        /// <param name="message">The message to parse.</param>
        private static void ParseAction(Message message)
        {
            // Determine type of action message
            switch (message.MessageCode)
            {
                case 0x79: // Item drop, Lot for item, or other (equipment changed, /recast message, /anon changed, etc)
                case 0x7f: // Item obtained
                    message.ActionDetails.ActionMessageType = ActionMessageType.Loot;
                    break;
                case 0x83: // Experience gained / chain number
                    message.ActionDetails.ActionMessageType = ActionMessageType.Experience;
                    break;
                case 0x92: // <me> caught a fish!
                case 0x94: // other fishing messages, other stuff
                    message.ActionDetails.ActionMessageType = ActionMessageType.Fishing;
                    break;
                default: // Mark the large swaths of possible combat messages
                    if ((message.MessageCode >= 0x13) && (message.MessageCode <= 0x84))
                        message.ActionDetails.ActionMessageType = ActionMessageType.Combat;
                    else if ((message.MessageCode >= 0xa0) && (message.MessageCode <= 0xbf))
                        message.ActionDetails.ActionMessageType = ActionMessageType.Combat;
                    else // Everything else is ignored.
                        message.ActionDetails.ActionMessageType = ActionMessageType.Other;
                    break;
            }

            // Based on action subcategory, continue parsing
            switch (message.ActionDetails.ActionMessageType)
            {
                case ActionMessageType.Combat:
                    ParseCombat(message);
                    break;
                case ActionMessageType.Loot:
                    ParseLoot(message);
                    break;
                case ActionMessageType.Experience:
                    ParseExperience(message);
                    break;
                default:
                    // Ignore Fishing for now
                    // Ignore Other always
                    break;
            }
        }
        #endregion

        #region Parsing of Action sub-categories
        private static void ParseExperience(Message message)
        {
            // !!! -- should be attached to most recent "XXX was defeated" message.

            Match exp;

            exp = ParseExpressions.Experience.Match(message.CurrentMessageText);
            if (exp.Success == true)
            {
                message.ActionDetails.ExperienceDetails.ExperiencePoints = int.Parse(exp.Groups[ParseFields.Number].Value);
                message.ActionDetails.ExperienceDetails.ExperienceRecipient = exp.Groups[ParseFields.Fullname].Value;
                message.ParseSuccessful = true;
                return;
            }

            exp = ParseExpressions.ExpChain.Match(message.CurrentMessageText);
            if (exp.Success == true)
            {
                message.ActionDetails.ExperienceDetails.ExperienceChain = int.Parse(exp.Groups[ParseFields.Number].Value);
                message.ParseSuccessful = true;
                return;
            }
        }

        private static void ParseLoot(Message message)
        {
            Match loot;

            switch (message.MessageCode)
            {
                // Item drop, Lot for item, equipment changed, /recast message
                case 0x79:
                    // Further specificity based on extraCode1
                    switch (message.ExtraCode1)
                    {
                        case 0:
                            // Loot drops from mob, player lots (ignored)
                            // Check to see if this is the initial message about loot being found.
                            loot = ParseExpressions.FindLoot.Match(message.CurrentMessageText);
                            if (loot.Success == true)
                            {
                                message.ActionDetails.LootDetails.IsFoundMessage = true;
                                message.ActionDetails.LootDetails.ItemName = loot.Groups[ParseFields.Item].Value;
                                message.ActionDetails.LootDetails.MobName = loot.Groups[ParseFields.Target].Value;
                                message.ParseSuccessful = true;
                                break;
                            }
                            // May also be the "not qualified" message (out of space, rare/ex)
                            loot = ParseExpressions.LootReqr.Match(message.CurrentMessageText);
                            if (loot.Success == true)
                            {
                                message.ActionDetails.LootDetails.IsFoundMessage = false;
                                message.ActionDetails.LootDetails.ItemName = loot.Groups[ParseFields.Item].Value;
                                message.ParseSuccessful = true;
                                break;
                            }
                            // Or the "Item is lost" message.
                            loot = ParseExpressions.LootLost.Match(message.CurrentMessageText);
                            if (loot.Success == true)
                            {
                                message.ActionDetails.LootDetails.IsFoundMessage = false;
                                message.ActionDetails.LootDetails.WasLost = true;
                                message.ActionDetails.LootDetails.ItemName = loot.Groups[ParseFields.Item].Value;
                                message.ParseSuccessful = true;
                                break;
                            }
                            break;
                        case 3:
                            // Equipment changes, recast times, /search results
                            message.ActionDetails.ActionMessageType = ActionMessageType.Other;
                            message.ParseSuccessful = true;
                            break;
                        default:
                            // Other
                            message.ActionDetails.ActionMessageType = ActionMessageType.Other;
                            break;
                    }
                    break;
                // Item obtained
                case 0x7f:
                    loot = ParseExpressions.GetGil.Match(message.CurrentMessageText);
                    if (loot.Success == true)
                    {
                        message.ActionDetails.LootDetails.IsFoundMessage = false;
                        message.ActionDetails.LootDetails.Gil = int.Parse(loot.Groups[ParseFields.Money].Value);
                        message.ActionDetails.LootDetails.WhoObtained = loot.Groups[ParseFields.Name].Value;
                        message.ParseSuccessful = true;
                        break;
                    }
                    loot = ParseExpressions.GetLoot.Match(message.CurrentMessageText);
                    if (loot.Success == true)
                    {
                        message.ActionDetails.LootDetails.IsFoundMessage = false;
                        message.ActionDetails.LootDetails.ItemName = loot.Groups[ParseFields.Item].Value;
                        message.ActionDetails.LootDetails.WhoObtained = loot.Groups[ParseFields.Name].Value;
                        message.ParseSuccessful = true;
                        break;
                    }
                    break;
            }
        }
        #endregion

        #region Combat / Combat sub-category Parsing
        private static void ParseCombat(Message message)
        {
            uint searchCode = message.MessageCode;

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

            NewCombatDetails msgCombatDetails = message.ActionDetails.CombatDetails;

            // Use lookup tables for general categories based on message code
            // Only lookup if unknown category type.  Multi-line messages will already know this info.
            if (msgCombatDetails.CombatCategory == CombatActionType.Unknown)
            {
                msgCombatDetails.CombatCategory = parseCodesLookup.GetCombatCategory(message.MessageCode);

                if (msgCombatDetails.CombatCategory == CombatActionType.Buff)
                    msgCombatDetails.BuffType = parseCodesLookup.GetBuffType(message.MessageCode);

                if (msgCombatDetails.CombatCategory == CombatActionType.Attack)
                {
                    msgCombatDetails.AttackType = parseCodesLookup.GetAttackType(message.MessageCode);
                    msgCombatDetails.SuccessLevel = parseCodesLookup.GetSuccessType(message.MessageCode);
                }
            }

            switch (msgCombatDetails.CombatCategory)
            {
                case CombatActionType.Attack:
                    ParseCombatAttack(message);
                    break;
                case CombatActionType.Buff:
                    ParseCombatBuffs(message);
                    break;
                case CombatActionType.Death:
                    ParseCombatDeath(message);
                    break;
                case CombatActionType.Unknown:
                    ParseCombatUnknown(message);
                    break;
            }
        }

        /// <summary>
        /// Break out the different types of attack messages for further parsing.
        /// </summary>
        /// <param name="message"></param>
        private static void ParseCombatAttack(Message message)
        {
            switch (message.ActionDetails.CombatDetails.AttackType)
            {
                case AttackType.Damage:
                    ParseAttackDamage(message);
                    break;
                case AttackType.Enfeeble:
                    ParseAttackEnfeeble(message);
                    break;
                case AttackType.Drain:
                case AttackType.Aspir:
                    ParseAttackDrain(message);
                    break;
                case AttackType.Unknown:
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
            NewCombatDetails msgCombatDetails = message.ActionDetails.CombatDetails;
            string currentMessageText = message.CurrentMessageText;
            NewTargetDetails target = null;

            // Check first for lines where the buff is activated
            // eg: Player casts Protectra IV.
            combatMatch = ParseExpressions.CastSpell.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.ActionSource = ActionSourceType.Spell;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                message.ParseSuccessful = true;
                return;
            }

            // eg: Player uses Warcry.
            combatMatch = ParseExpressions.UseAbility.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.ActionSource = ActionSourceType.Ability;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                message.ParseSuccessful = true;
                return;
            }


            // Break down the results of the spell/ability
            switch (msgCombatDetails.BuffType)
            {
                case BuffType.Enhance:
                    // eg: Target gains the effect of Protect.
                    combatMatch = ParseExpressions.Buff.Match(currentMessageText);
                    if (combatMatch.Success == true)
                    {
                        target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                        target.SuccessLevel = SuccessType.Successful;
                        if (msgCombatDetails.ActionName == string.Empty)
                            msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Effect].Value;
                        message.ParseSuccessful = true;
                        return;
                    }
                    // eg: Target's attacks are enhanced.
                    combatMatch = ParseExpressions.Enhance.Match(currentMessageText);
                    if (combatMatch.Success == true)
                    {
                        target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                        target.SuccessLevel = SuccessType.Successful;
                        if (msgCombatDetails.ActionName == string.Empty)
                            msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Effect].Value;
                        message.ParseSuccessful = true;
                        return;
                    }

                    // Check in case of some out of all targets not getting affected.
                    // eg: Player's Protect has no effect on Target.
                    combatMatch = ParseExpressions.NoEffect.Match(currentMessageText);
                    if (combatMatch.Success == true)
                    {
                        msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                        msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                        target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                        target.SuccessLevel = SuccessType.Failed;
                        msgCombatDetails.SuccessLevel = SuccessType.Unknown;
                        message.ParseSuccessful = true;
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
                            target.SuccessLevel = SuccessType.Successful;
                            message.ParseSuccessful = true;
                        }
                        return;
                    }
                    break;
                case BuffType.Recovery:
                    combatMatch = ParseExpressions.RecoversHP.Match(currentMessageText);
                    if (combatMatch.Success == true)
                    {
                        target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                        target.RecoveryAmount = int.Parse(combatMatch.Groups[ParseFields.Number].Value);
                        target.RecoveryType = RecoveryType.RecoverHP;
                        target.SuccessLevel = SuccessType.Successful;
                        message.ParseSuccessful = true;
                        return;
                    }
                    combatMatch = ParseExpressions.RecoversMP.Match(currentMessageText);
                    if (combatMatch.Success == true)
                    {
                        target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                        target.RecoveryAmount = int.Parse(combatMatch.Groups[ParseFields.Number].Value);
                        target.RecoveryType = RecoveryType.RecoverMP;
                        target.SuccessLevel = SuccessType.Successful;
                        message.ParseSuccessful = true;
                        return;
                    }
                    break;
                case BuffType.Unknown:
                    // For prepping buffing spells or abilities (do we need this?)
                    combatMatch = ParseExpressions.PrepSpell.Match(currentMessageText);
                    if (combatMatch.Success == true)
                    {
                        msgCombatDetails.IsPreparing = true;
                        msgCombatDetails.ActionSource = ActionSourceType.Spell;
                        msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                        msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                        target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                        message.ParseSuccessful = true;
                        return;
                    }
                    combatMatch = ParseExpressions.PrepAbility.Match(currentMessageText);
                    if (combatMatch.Success == true)
                    {
                        msgCombatDetails.IsPreparing = true;
                        msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                        msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                        if (Weaponskills.NamesList.Contains(msgCombatDetails.ActionName))
                            msgCombatDetails.ActionSource = ActionSourceType.Weaponskill;
                        else
                            msgCombatDetails.ActionSource = ActionSourceType.Ability;
                        message.ParseSuccessful = true;
                        return;
                    }
                    break;
            }
        }

        /// <summary>
        /// Parse messages for death events.
        /// </summary>
        /// <param name="message"></param>
        private static void ParseCombatDeath(Message message)
        {
            Match combatMatch = null;

            combatMatch = ParseExpressions.Defeat.Match(message.CurrentMessageText);
            if (combatMatch.Success == true)
            {
                message.ActionDetails.CombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                message.ActionDetails.CombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                message.ParseSuccessful = true;
                return;
            }

            combatMatch = ParseExpressions.Dies.Match(message.CurrentMessageText);
            if (combatMatch.Success == true)
            {
                message.ActionDetails.CombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                message.ParseSuccessful = true;
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
            NewCombatDetails msgCombatDetails = message.ActionDetails.CombatDetails;
           
            // Prepping spell or ability of unknown type

            combatMatch = ParseExpressions.PrepSpell.Match(message.CurrentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.IsPreparing = true;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                msgCombatDetails.ActionSource = ActionSourceType.Spell;
                message.ParseSuccessful = true;
                return;
            }

            combatMatch = ParseExpressions.PrepSpellOn.Match(message.CurrentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.IsPreparing = true;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                msgCombatDetails.ActionSource = ActionSourceType.Spell;
                message.ParseSuccessful = true;
                return;
            }

            combatMatch = ParseExpressions.PrepAbility.Match(message.CurrentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.IsPreparing = true;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                if (Weaponskills.NamesList.Contains(msgCombatDetails.ActionName))
                    msgCombatDetails.ActionSource = ActionSourceType.Weaponskill;
                else
                    msgCombatDetails.ActionSource = ActionSourceType.Ability;
                message.ParseSuccessful = true;
                return;
            }
        }
        #endregion

        #region Parse Combat Attack sub-types
        private static void ParseAttackDamage(Message message)
        {
            switch (message.ActionDetails.CombatDetails.SuccessLevel)
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
            NewCombatDetails combatDetails = message.ActionDetails.CombatDetails;
            string currentMessageText = message.CurrentMessageText;
            NewTargetDetails target = null;

            // Make all the type checks up front

            // First up are the first-pass entries of possible multi-line messages.
            combatMatch = ParseExpressions.MeleeHit.Match(currentMessageText);
            if (combatMatch.Success == true)
                combatDetails.ActionSource = ActionSourceType.Melee;

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.RangedHit.Match(currentMessageText);
                if (combatMatch.Success == true)
                    combatDetails.ActionSource = ActionSourceType.Ranged;
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.Counter.Match(currentMessageText);
                if (combatMatch.Success == true)
                    combatDetails.ActionSource = ActionSourceType.Counterattack;
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.Spikes.Match(currentMessageText);
                if (combatMatch.Success == true)
                    combatDetails.ActionSource = ActionSourceType.Spikes;
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.CastSpell.Match(currentMessageText);
                if (combatMatch.Success == true)
                    combatDetails.ActionSource = ActionSourceType.Spell;
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.UseAbility.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    if (Weaponskills.NamesList.Contains(combatMatch.Groups[ParseFields.Ability].Value))
                        combatDetails.ActionSource = ActionSourceType.Weaponskill;
                    else
                        combatDetails.ActionSource = ActionSourceType.Ability;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.CriticalHit.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.FlagCrit = true;
                    combatDetails.ActionSource = ActionSourceType.Melee;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.RangedCriticalHit.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.FlagCrit = true;
                    combatDetails.ActionSource = ActionSourceType.Ranged;
                }
            }


            // Handle this group first
            if (combatMatch.Success == true)
            {
                message.ParseSuccessful = true;

                switch (combatDetails.ActionSource)
                {
                    case ActionSourceType.Melee:
                    case ActionSourceType.Ranged:
                    case ActionSourceType.Counterattack:
                    case ActionSourceType.Spikes:
                    case ActionSourceType.Spell:
                    case ActionSourceType.Ability:
                    case ActionSourceType.Weaponskill:
                        combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                        break;
                }

                // skip in cases of crit hit lines that won't provide target info
                if (combatDetails.FlagCrit == false)
                {
                    switch (combatDetails.ActionSource)
                    {
                        case ActionSourceType.Melee:
                        case ActionSourceType.Counterattack:
                        case ActionSourceType.Spikes:
                            target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                            target.Damage = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
                            break;
                    }
                }

                // Actor entity
                switch (message.MessageCode)
                {
                    case 0x14:
                    case 0x19:
                        // me or party vs mob
                        combatDetails.ActorEntityType = EntityType.Player;
                        break;
                    case 0x1c:
                        // mob vs me or party
                        combatDetails.ActorEntityType = EntityType.Mob;
                        break;
                    case 0x28:
                        // mob vs pet or pet vs mob or player outside party fighting
                        if (combatDetails.ActorEntityType == EntityType.Unknown)
                            combatDetails.ActorEntityType = MessageManager.Instance.LookupPetEntity(combatDetails.ActorName);
                        
                        // If we have target info, double-check vs target entity type in case
                        // actor type is unknown.
                        if (combatDetails.FlagCrit == false)
                        {
                            if (combatDetails.ActorEntityType == EntityType.Unknown)
                            {
                                if ((target.EntityType == EntityType.Pet) || (target.EntityType == EntityType.Player))
                                {
                                    combatDetails.ActorEntityType = EntityType.Mob;
                                    break;
                                }
                            }
                        }
                        break;
                    default:
                        // other??
                        if (combatDetails.ActorEntityType == EntityType.Unknown)
                            combatDetails.ActorEntityType = MessageManager.Instance.LookupPetEntity(combatDetails.ActorName);
                        break;
                }


                // Crits only get actor details on the first pass through
                if (combatDetails.FlagCrit == true)
                    return;

                // Target entity
                if (target != null)
                {
                    target.SuccessLevel = SuccessType.Successful;

                    switch (message.MessageCode)
                    {
                        case 0x14:
                        case 0x19:
                            // me or party vs mob
                            target.EntityType = EntityType.Mob;
                            break;
                        case 0x1c:
                            // mob vs me or party
                            target.EntityType = EntityType.Player;
                            break;
                        case 0x28:
                            // mob vs pet or pet vs mob or players/mobs outside of party

                            if (target.EntityType == EntityType.Unknown)
                                target.EntityType = MessageManager.Instance.LookupEntity(target.Name);

                            // If crit hit is true, we had to skip this check in the above code,
                            // so rechecking it here.
                            if (combatDetails.FlagCrit == true)
                            {
                                if (combatDetails.ActorEntityType == EntityType.Unknown)
                                {
                                    if ((combatDetails.ActorEntityType == EntityType.Pet) || (combatDetails.ActorEntityType == EntityType.Player))
                                    {
                                        target.EntityType = EntityType.Mob;
                                        break;
                                    }
                                }
                            }

                            if ((target.EntityType == EntityType.Unknown) && (combatDetails.ActorEntityType == EntityType.Pet))
                            {
                                target.EntityType = EntityType.Mob;
                                break;
                            }

                            if ((target.EntityType == EntityType.Unknown) && (combatDetails.ActorEntityType == EntityType.Mob))
                            {
                                target.EntityType = EntityType.Pet;
                                break;
                            }

                            if (combatDetails.ActorEntityType == target.EntityType)
                                combatDetails.ActorEntityType = MessageManager.Instance.LookupPetEntity(combatDetails.ActorName);
                            if (combatDetails.ActorEntityType == target.EntityType)
                                target.EntityType = MessageManager.Instance.LookupPetEntity(target.Name);
                            break;
                        default:
                            // other??
                            if (target.EntityType == EntityType.Unknown)
                                target.EntityType = MessageManager.Instance.LookupEntity(target.Name);
                            break;
                    }
                }

                switch (combatDetails.ActionSource)
                {
                    case ActionSourceType.Spell:
                        combatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                        break;
                    case ActionSourceType.Ability:
                    case ActionSourceType.Weaponskill:
                        combatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                        break;
                }

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
                        target = combatDetails.Targets.Find(t => t.Name == combatMatch.Groups[ParseFields.Fulltarget].Value);
                        if (target != null)
                        {
                            target.AdditionalEffect = true;
                            target.AdditionalDamage = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
                        }
                    }
                    else
                    {
                        modifyMatch = ParseExpressions.MagicBurst.Match(currentMessageText);
                        if (modifyMatch.Success == true)
                        {
                            target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                            target.DamageModifier = DamageModifier.MagicBurst;
                            target.Damage = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
                        }
                    }

                    if (target == null)
                        target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);

                    target.SuccessLevel = SuccessType.Successful;

                    if (combatDetails.FlagCrit == true)
                        target.DamageModifier = DamageModifier.Critical;

                    if (modifyMatch.Success == false)
                        target.Damage = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
                }
            }

            // Handle entity settings on additional targets
            if (combatMatch.Success == true)
            {
                message.ParseSuccessful = true;

                if (target != null)
                {
                    switch (message.MessageCode)
                    {
                        case 0x14:
                        case 0x19:
                            // me or party vs mob
                            target.EntityType = EntityType.Mob;
                            break;
                        case 0x1c:
                            // mob vs me or party
                            target.EntityType = EntityType.Player;
                            break;
                        case 0x28:
                            // mob vs pet or pet vs mob
                            if (target.EntityType == EntityType.Unknown)
                                target.EntityType = MessageManager.Instance.LookupEntity(target.Name);

                            if (target.EntityType == EntityType.Unknown)
                            {
                                if ((combatDetails.ActorEntityType == EntityType.Pet) || (combatDetails.ActorEntityType == EntityType.Pet))
                                {
                                    target.EntityType = EntityType.Mob;
                                    break;
                                }
                            }

                            if (combatDetails.ActorEntityType == target.EntityType)
                                combatDetails.ActorEntityType = MessageManager.Instance.LookupPetEntity(combatDetails.ActorName);
                            if (combatDetails.ActorEntityType == target.EntityType)
                                target.EntityType = MessageManager.Instance.LookupPetEntity(target.Name);
                            break;
                        default:
                            // other??
                            if (target.EntityType == EntityType.Unknown)
                                target.EntityType = MessageManager.Instance.LookupPetEntity(target.Name);
                            break;
                    }
                }
            }
        }

        private static void ParseUnuccessfulDamageAttack(Message message)
        {
            Match combatMatch = null;
            NewCombatDetails combatDetails = message.ActionDetails.CombatDetails;
            string currentMessageText = message.CurrentMessageText;
            NewTargetDetails target = null;

            DefenseType defType = DefenseType.None;

            // First lines of multi-line values

            combatMatch = ParseExpressions.CastSpell.Match(currentMessageText);
            if (combatMatch.Success == true)
                combatDetails.ActionSource = ActionSourceType.Spell;

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.UseAbility.Match(currentMessageText);
                if (combatMatch.Success == true)
                    combatDetails.ActionSource = ActionSourceType.Ability;
            }

            if (combatMatch.Success == true)
            {
                message.ParseSuccessful = true;

                combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;

                switch (combatDetails.ActionSource)
                {
                    case ActionSourceType.Spell:
                        combatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                        break;
                    case ActionSourceType.Ability:
                        combatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                        if (Weaponskills.NamesList.Contains(combatDetails.ActionName))
                            combatDetails.ActionSource = ActionSourceType.Weaponskill;
                        break;
                }

                // Determine actor entity (no target is provided in the above message lines)
                switch (message.MessageCode)
                {
                    case 0x15:
                    case 0x1a:
                        // me or party vs mob
                        combatDetails.ActorEntityType = EntityType.Player;
                        break;
                    case 0x1d:
                        // mob vs player
                        combatDetails.ActorEntityType = EntityType.Mob;
                        break;
                    case 0x29:
                        // Mob vs mob
                        if (combatDetails.ActorEntityType == EntityType.Unknown)
                            combatDetails.ActorEntityType = MessageManager.Instance.LookupPetEntity(combatDetails.ActorName);
                        break;
                    default:
                        // other??
                        if (combatDetails.ActorEntityType == EntityType.Unknown)
                            combatDetails.ActorEntityType = MessageManager.Instance.LookupEntity(combatDetails.ActorName);
                        break;
                }

                // Make corrections for certain special actions that are really enfeebles
                if (combatDetails.ActionName == "Charm")
                    combatDetails.AttackType = AttackType.Enfeeble;

                return;
            }


            // Standalone and followup message lines

            combatMatch = ParseExpressions.MeleeMiss.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                combatDetails.ActionSource = ActionSourceType.Melee;
                defType = DefenseType.Evasion;
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.RangedMiss.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionSource = ActionSourceType.Ranged;
                    defType = DefenseType.Evasion;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.MissAbility.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.ActionSource = ActionSourceType.Ability;
                    defType = DefenseType.Evasion;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.Blink.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    defType = DefenseType.Blink;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.Parry.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    defType = DefenseType.Parry;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.Anticipate.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    defType = DefenseType.Anticipate;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.Evade.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    defType = DefenseType.Evade;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.ResistSpell.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    defType = DefenseType.Resist;
                }
            }


            if (combatMatch.Success == true)
            {
                message.ParseSuccessful = true;

                target = combatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                target.Defended = true;
                target.DefenseType = defType;
                target.SuccessLevel = SuccessType.Unsuccessful;

                switch (defType)
                {
                    case DefenseType.Evasion:
                    case DefenseType.Parry:
                    case DefenseType.Anticipate:
                        combatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                        break;
                }

                if (defType == DefenseType.Blink)
                    target.ShadowsUsed = byte.Parse(combatMatch.Groups[ParseFields.Number].Value);

                // Determine the two entity types involved.
                switch (message.MessageCode)
                {
                    case 0x15:
                    case 0x1a:
                        // me or party vs mob
                        combatDetails.ActorEntityType = EntityType.Player;
                        target.EntityType = EntityType.Mob;
                        break;
                    case 0x1d:
                        // mob vs player
                        combatDetails.ActorEntityType = EntityType.Mob;
                        target.EntityType = EntityType.Player;
                        break;
                    case 0x29:
                        // mob vs pet or pet vs mob
                        if (combatDetails.ActorEntityType == EntityType.Unknown)
                            combatDetails.ActorEntityType = MessageManager.Instance.LookupPetEntity(combatDetails.ActorName);

                        if (combatDetails.ActorEntityType == EntityType.Unknown)
                        {
                            if ((target.EntityType == EntityType.Pet) || (target.EntityType == EntityType.Player))
                            {
                                combatDetails.ActorEntityType = EntityType.Mob;
                                break;
                            }
                        }

                        if (target.EntityType == EntityType.Unknown)
                            target.EntityType = MessageManager.Instance.LookupEntity(target.Name);
                        if (target.EntityType == EntityType.Unknown)
                        {
                            if ((combatDetails.ActorEntityType == EntityType.Pet) || (combatDetails.ActorEntityType == EntityType.Player))
                            {
                                target.EntityType = EntityType.Mob;
                                break;
                            }
                        }

                        if (combatDetails.ActorEntityType == target.EntityType)
                            combatDetails.ActorEntityType = MessageManager.Instance.LookupPetEntity(combatDetails.ActorName);
                        if (combatDetails.ActorEntityType == target.EntityType)
                            target.EntityType = MessageManager.Instance.LookupPetEntity(target.Name);
                        break;
                    default:
                        // other??
                        if (combatDetails.ActorEntityType == EntityType.Unknown)
                            combatDetails.ActorEntityType = MessageManager.Instance.LookupEntity(combatDetails.ActorName);
                        if (target.EntityType == EntityType.Unknown)
                            target.EntityType = MessageManager.Instance.LookupEntity(target.Name);
                        break;
                }
            }
        }

        private static void ParseAttackEnfeeble(Message message)
        {
            Match combatMatch = null;
            NewCombatDetails msgCombatDetails = message.ActionDetails.CombatDetails;
            string currentMessageText = message.CurrentMessageText;
            NewTargetDetails target = null;

            // First check for prepping attack spells or abilities
            // eg: Player starts casting Poison on the Mandragora.
            combatMatch = ParseExpressions.PrepSpell.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.IsPreparing = true;
                msgCombatDetails.ActionSource = ActionSourceType.Spell;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                message.ParseSuccessful = true;
                return;
            }
            combatMatch = ParseExpressions.PrepAbility.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.IsPreparing = true;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                if (Weaponskills.NamesList.Contains(msgCombatDetails.ActionName))
                    msgCombatDetails.ActionSource = ActionSourceType.Weaponskill;
                else
                    msgCombatDetails.ActionSource = ActionSourceType.Ability;
                message.ParseSuccessful = true;
                return;
            }

            // Then check for lines where the action is activated
            combatMatch = ParseExpressions.CastSpell.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.ActionSource = ActionSourceType.Spell;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                message.ParseSuccessful = true;
                return;
            }
            combatMatch = ParseExpressions.UseAbility.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.ActionSource = ActionSourceType.Ability;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                message.ParseSuccessful = true;
                return;
            }
            combatMatch = ParseExpressions.UseAbilityOn.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.ActionSource = ActionSourceType.Ability;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                target.SuccessLevel = SuccessType.Successful;
                message.ParseSuccessful = true;
                return;
            }

            // Failed enfeebles.  IE: <spell> had no effect.
            if (msgCombatDetails.SuccessLevel == SuccessType.Failed)
            {
                combatMatch = ParseExpressions.NoEffect.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActionSource = ActionSourceType.Spell;
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.SuccessLevel = SuccessType.Failed;
                    msgCombatDetails.SuccessLevel = SuccessType.Unknown;
                    message.ParseSuccessful = true;
                    return;
                }
            }
            // Misses (The Mandragora uses Dream Flower but misses Player.)
            combatMatch = ParseExpressions.MissAbility.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.ActionSource = ActionSourceType.Ability;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                target.SuccessLevel = SuccessType.Unsuccessful;
                target.Defended = true;
                target.DefenseType = DefenseType.Evade;
                message.ParseSuccessful = true;
                return;
            }

            // Then check for individual lines about the effect on the target
            combatMatch = ParseExpressions.Debuff.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                target.SuccessLevel = SuccessType.Successful;
                message.ParseSuccessful = true;
                return;
            }
            combatMatch = ParseExpressions.Enfeeble.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                target.SuccessLevel = SuccessType.Successful;
                message.ParseSuccessful = true;
                return;
            }


            combatMatch = ParseExpressions.ResistSpell.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                target.SuccessLevel = SuccessType.Unsuccessful;
                target.Defended = true;
                target.DefenseType = DefenseType.Resist;
                message.ParseSuccessful = true;
                return;
            }
            combatMatch = ParseExpressions.ResistEffect.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                target.SuccessLevel = SuccessType.Unsuccessful;
                target.Defended = true;
                target.DefenseType = DefenseType.Resist;
                message.ParseSuccessful = true;
                return;
            }

            // Special handling: Charms
            combatMatch = ParseExpressions.Charmed.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                target.SuccessLevel = SuccessType.Successful;

                if (msgCombatDetails.ActionName == "Charm")
                {
                    // Only for players.  Mobs use other ability names to charm players.
                    // Mob type has been charmed.  Add to the entity lookup list as a pet.
                    MessageManager.Instance.AddPetEntity(target.Name);
                    target.EntityType = EntityType.Pet;
                    msgCombatDetails.ActorEntityType = EntityType.Player;
                }
                message.ParseSuccessful = true;
                return;
            }

            combatMatch = ParseExpressions.FailsCharm.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.ActionSource = ActionSourceType.Ability;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                target.SuccessLevel = SuccessType.Unsuccessful;
                target.Defended = true;
                target.DefenseType = DefenseType.Resist;
                message.ParseSuccessful = true;
                return;
            }

            // Last check in case of some out of all targets not getting affected.
            combatMatch = ParseExpressions.NoEffect.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                target.SuccessLevel = SuccessType.Failed;
                msgCombatDetails.SuccessLevel = SuccessType.Unknown;
                message.ParseSuccessful = true;
                return;
            }
        }

        private static void ParseAttackDrain(Message message)
        {
            Match combatMatch = null;
            NewCombatDetails msgCombatDetails = message.ActionDetails.CombatDetails;
            string currentMessageText = message.CurrentMessageText;
            NewTargetDetails target = null;

            // Player casts Drain/Aspir.


            // Player drains XXX HP/MP from target.

            combatMatch = ParseExpressions.DrainHP.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = "Drain";
                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                target.SuccessLevel = SuccessType.Successful;
                target.Damage = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
                target.RecoveryType = RecoveryType.RecoverHP;
                msgCombatDetails.SuccessLevel = SuccessType.Successful;

                message.ParseSuccessful = true;
                return;
            }

            combatMatch = ParseExpressions.DrainMP.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = "Aspir";
                target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                target.SuccessLevel = SuccessType.Successful;
                target.Damage = int.Parse(combatMatch.Groups[ParseFields.Damage].Value);
                target.RecoveryType = RecoveryType.RecoverHP;
                msgCombatDetails.SuccessLevel = SuccessType.Successful;

                message.ParseSuccessful = true;
                return;
            }

        }

        private static void ParseAttackUnknown(Message message)
        {
            Match combatMatch = null;
            NewCombatDetails msgCombatDetails = message.ActionDetails.CombatDetails;
            string currentMessageText = message.CurrentMessageText;
            NewTargetDetails target = null;
            
            #region Failed Actions
            // Deal with possible Failed actions first
            if (msgCombatDetails.SuccessLevel == SuccessType.Failed)
            {
                combatMatch = ParseExpressions.Interrupted.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.FailedActionType = FailedActionType.Interrupted;
                    message.ParseSuccessful = true;
                    return;
                }
                combatMatch = ParseExpressions.Paralyzed.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    msgCombatDetails.FailedActionType = FailedActionType.Paralyzed;
                    message.ParseSuccessful = true;
                    return;
                }
                combatMatch = ParseExpressions.Intimidated.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    // Treat intimidation as a defense type, like Parry
                    msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    target.Defended = true;
                    target.DefenseType = DefenseType.Intimidate;
                    // Adjust message category
                    msgCombatDetails.CombatCategory = CombatActionType.Attack;
                    msgCombatDetails.AttackType = AttackType.Unknown;
                    msgCombatDetails.SuccessLevel = SuccessType.Unsuccessful;
                    //msgCombatDetails.FailedActionType = FailedActionType.Intimidated;
                    message.ParseSuccessful = true;
                    return;
                }
                // Matches for the following to note the line as successfully parsed.
                combatMatch = ParseExpressions.UnableToCast.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.FailedActionType = FailedActionType.UnableToCast;
                    message.ParseSuccessful = true;
                    return;
                }
                combatMatch = ParseExpressions.UnableToUse.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.FailedActionType = FailedActionType.UnableToUse;
                    message.ParseSuccessful = true;
                    return;
                }
                combatMatch = ParseExpressions.CannotSee.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    msgCombatDetails.FailedActionType = FailedActionType.CannotSee;
                    message.ParseSuccessful = true;
                    return;
                }
                combatMatch = ParseExpressions.TooFarAway.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    msgCombatDetails.FailedActionType = FailedActionType.TooFarAway;
                    message.ParseSuccessful = true;
                    return;
                }
                combatMatch = ParseExpressions.OutOfRange.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    target = msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                    msgCombatDetails.FailedActionType = FailedActionType.OutOfRange;
                    message.ParseSuccessful = true;
                    return;
                }
                combatMatch = ParseExpressions.NotEnoughMP.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.FailedActionType = FailedActionType.NotEnoughMP;
                    message.ParseSuccessful = true;
                    return;
                }
                combatMatch = ParseExpressions.NotEnoughTP.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    msgCombatDetails.FailedActionType = FailedActionType.NotEnoughTP;
                    message.ParseSuccessful = true;
                    return;
                }

                return;
            }
            #endregion

            // For prepping attack spells or abilities
            combatMatch = ParseExpressions.PrepSpell.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.IsPreparing = true;
                msgCombatDetails.ActionSource = ActionSourceType.Spell;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                message.ParseSuccessful = true;
                return;
            }
            combatMatch = ParseExpressions.PrepSpellOn.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.IsPreparing = true;
                msgCombatDetails.ActionSource = ActionSourceType.Spell;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Spell].Value;
                msgCombatDetails.AddTarget(combatMatch.Groups[ParseFields.Fulltarget].Value);
                message.ParseSuccessful = true;
                return;
            }
            combatMatch = ParseExpressions.PrepAbility.Match(currentMessageText);
            if (combatMatch.Success == true)
            {
                msgCombatDetails.IsPreparing = true;
                msgCombatDetails.ActorName = combatMatch.Groups[ParseFields.Fullname].Value;
                msgCombatDetails.ActionName = combatMatch.Groups[ParseFields.Ability].Value;
                if (Weaponskills.NamesList.Contains(msgCombatDetails.ActionName))
                    msgCombatDetails.ActionSource = ActionSourceType.Weaponskill;
                else
                    msgCombatDetails.ActionSource = ActionSourceType.Ability;
                message.ParseSuccessful = true;
                return;
            }
        }

        #endregion

        #region Helper Functions
        /// <summary>
        /// Function to determine if a mob-type entity is an actual mob or a pet.
        /// </summary>
        /// <param name="name">The mob name to check.</param>
        /// <returns>Returns the entity type determination.</returns>
        private static EntityType DetermineIfMobOrPet(string name)
        {
            if ((name == null) || (name == string.Empty))
                return EntityType.Unknown;

            if (name.StartsWith("The") || name.StartsWith("the"))
                return EntityType.Mob;

            Match nameMatch = null;

            nameMatch = ParseExpressions.MobNameTest.Match(name);

            if (nameMatch.Success == true)
            {
                // Probably a mob, but possibly a puppet
                if (Puppets.ShortNamesList.Contains(name))
                    return EntityType.Pet;
                else
                    return EntityType.Mob;
            }

            nameMatch = ParseExpressions.BstJugPetName.Match(name);
            if (nameMatch.Success == true)
                return EntityType.Pet;

            if (Avatars.NamesList.Contains(name))
                return EntityType.Pet;

            if (Wyverns.NamesList.Contains(name))
                return EntityType.Pet;

            if (Puppets.NamesList.Contains(name))
                return EntityType.Pet;

            // Unable to determine
            return EntityType.Unknown;
        }

        private static EntityType DetermineBaseEntityType(string name)
        {
            if ((name == null) || (name == string.Empty))
                return EntityType.Unknown;

            // If the name starts with 'the' it's a mob, though possibly a charmed one.
            if (name.StartsWith("The") || name.StartsWith("the"))
                return EntityType.Mob;

            // Check for characters that only show up in mob names.
            Match nameMatch = ParseExpressions.MobNameTest.Match(name);

            if (nameMatch.Success == true)
            {
                // Probably a mob, but possibly a puppet
                if (Puppets.ShortNamesList.Contains(name))
                    return EntityType.Pet;
                else
                    return EntityType.Mob;
            }

            // Check for the pattern of beastmaster jug pet names.
            nameMatch = ParseExpressions.BstJugPetName.Match(name);
            if (nameMatch.Success == true)
                return EntityType.Pet;

            // Check known pet lists
            if (Avatars.NamesList.Contains(name))
                return EntityType.Pet;

            if (Wyverns.NamesList.Contains(name))
                return EntityType.Pet;

            if (Puppets.NamesList.Contains(name))
                return EntityType.Pet;

            // Anything else must be a player.
            return EntityType.Player;
        }
        #endregion

    }
}
