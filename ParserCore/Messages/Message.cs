using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

namespace WaywardGamers.KParser
{
    /// <summary>
    /// MessageTuple is a container class for Messages, since multiple
    /// messages can be tied together in the same event (eg: additional effects).
    /// </summary>
    internal class OldMessage
    {
        #region Member Variables
        uint messageID;
        Collection<MessageLine> msgLineCollection = new Collection<MessageLine>();
        List<string> messageTextStrings = new List<string>(1);

        uint messageCode, extraCode1, extraCode2;

        bool parseSuccessful;

        MessageCategoryType messageCategory;
        SystemMessageType systemMessageType;
        ActionMessageType actionMessageType;
        ChatMessageType chatType;

        string currentMessageText;

        SpeakerType chatSpeakerType;
        string chatSpeakerName;

        // Combat details
        CombatDetails combatDetails;
        LootDetails lootDetails;

        static ParseCodes parseCodesLookup = new ParseCodes();

        internal bool FlagDoNotAddToManagerCollection;
        #endregion

        #region Constructor
        /// <summary>
        /// The constructor takes a starting Message to place in the tuple.
        /// </summary>
        /// <param name="msg">The initial Message to add to the tuple.</param>
        internal OldMessage(MessageLine msg)
        {
            if (msg == null)
                throw new ArgumentNullException("msg");
            
            messageID = msg.EventSequence;
            msgLineCollection.Add(msg);

            ExtractInitialData(msg);
        }
        #endregion

        #region internal Methods
        /// <summary>
        /// Add a new message line to the existing message.
        /// </summary>
        /// <param name="msg">Message line to add to the collection.</param>
        internal void AddMessage(MessageLine msg)
        {
            if (msg == null)
                throw new ArgumentNullException("msg");

            msgLineCollection.Add(msg);

            UpdateTupleData(msg);
        }
        #endregion

        #region Properties pulled directly from contained message lines.
        /// <summary>
        /// Gets the ID number (message event value) of the tuple.
        /// </summary>
        internal uint MessageID
        {
            get
            {
                return messageID;
            }
        }

        /// <summary>
        /// Gets the overall message code for the tuple.
        /// </summary>
        internal uint MessageCode
        {
            get
            {
                return messageCode;
            }
        }

        /// <summary>
        /// Get the secondary code for the tuple.
        /// </summary>
        internal uint ExtraCode1
        {
            get
            {
                return extraCode1;
            }
        }

        /// <summary>
        /// Get the tertiary code for the tuple.
        /// </summary>
        internal uint ExtraCode2
        {
            get
            {
                return extraCode2;
            }
        }

        /// <summary>
        /// Gets the timestamp for the message.
        /// </summary>
        internal DateTime Timestamp
        {
            get
            {
                return msgLineCollection[0].Timestamp;
            }
        }

        /// <summary>
        /// Get the message type.  Message type separates messages into
        /// three general categories: system, chat, action.
        /// </summary>
        internal MessageCategoryType MessageCategory
        {
            get
            {
                return messageCategory;
            }
        }

        /// <summary>
        /// Gets the array of strings comprising all included messages.
        /// </summary>
        internal List<string> MessageTextStrings
        {
            get
            {
                return messageTextStrings;
            }
        }

        /// <summary>
        /// Get the collection of all included messages.
        /// </summary>
        internal Collection<MessageLine> MessageLineCollection
        {
            get
            {
                return msgLineCollection;
            }
        }
        #endregion

        #region Derived properties.
        /// <summary>
        /// Gets the specific type of system message.
        /// This is determined by the initial message code.
        /// </summary>
        internal SystemMessageType SystemMessageType
        {
            get
            {
                return systemMessageType;
            }
        }

        /// <summary>
        /// Gets the specific type of chat category the message falls under.
        /// This is determined by the initial message code.
        /// </summary>
        internal ChatMessageType ChatType
        {
            get
            {
                return chatType;
            }
        }

        /// <summary>
        /// Get the specific type of action the message refers to,
        /// to distinguish combat from other actions.
        /// </summary>
        internal ActionMessageType ActionMessageType
        {
            get
            {
                return actionMessageType;
            }
        }

        /// <summary>
        /// The current complete line of text for the message.
        /// </summary>
        internal string CurrentMessageText
        {
            get
            {
                return currentMessageText;
            }
        }

        #endregion

        #region Parsing methods for general message categories.
        /// <summary>
        /// Extract basic data from the initial message passed in to create
        /// the message tuple.
        /// </summary>
        /// <param name="msg"></param>
        private void ExtractInitialData(MessageLine msg)
        {
            messageCode = msg.MessageCode;
            extraCode1 = msg.ExtraCode1;
            extraCode2 = msg.ExtraCode2;

            messageCategory = msg.MessageCategory;

            messageTextStrings.Add(msg.TextOutput);

            currentMessageText = msg.TextOutput;

            switch (messageCategory)
            {
                case MessageCategoryType.Action:
                    ParseAction();
                    break;
                case MessageCategoryType.Chat:
                    ParseChat();
                    break;
                case MessageCategoryType.System:
                    ParseSystem();
                    break;
            }
        }

        /// <summary>
        /// Parses chat messages to determine message type and speaker.
        /// </summary>
        private void ParseChat()
        {
            // Determine specific chat type.
            if (messageCategory == MessageCategoryType.Chat)
            {
                switch (messageCode)
                {
                    case 0x01: // <me>
                    case 0x09: // Others
                        chatType = ChatMessageType.Say;
                        break;
                    case 0x02: // <me>
                    case 0x0a: // Others
                        chatType = ChatMessageType.Shout;
                        break;
                    case 0x04: // <me>
                    case 0x0c: // Others
                        chatType = ChatMessageType.Tell;
                        break;
                    case 0x05: // <me>
                    case 0x0d: // Others
                        chatType = ChatMessageType.Party;
                        break;
                    case 0x06: // <me>
                    case 0x0e: // Others
                        chatType = ChatMessageType.Linkshell;
                        break;
                    case 0x07: // <me>
                    case 0x0f: // Others
                        chatType = ChatMessageType.Emote;
                        break;
                    case 0x98:
                        chatType = ChatMessageType.NPC;
                        break;
                    // The following codes fit into the above pattern, but
                    // I haven't encountered the specific code values. (NPCs?)
                    case 0x03: // <me>?
                    case 0x0b: // Others?
                    default:
                        Debug.WriteLine(string.Format("Unknown message chat code: {0}", messageCode), "Chat");
                        chatType = ChatMessageType.Unknown;
                        break;
                }
            }
            else
            {
                chatType = ChatMessageType.Unknown;
            }

            if (chatType == ChatMessageType.Unknown)
                return;

            // Determine whether user was speaking or someone else.
            if (messageCode < 9)
            {
                chatSpeakerType = SpeakerType.Self;
            }
            else if (messageCode < 0x10)
            {
                chatSpeakerType = SpeakerType.Other;
            }
            else
            {
                chatSpeakerType = SpeakerType.Other;
            }

            // Determine the name of the speaker
            Match chatName;

            switch (chatType)
            {
                case ChatMessageType.Say:
                    chatName = ParseExpressions.ChatSay.Match(currentMessageText);
                    break;
                case ChatMessageType.Party:
                    chatName = ParseExpressions.ChatParty.Match(currentMessageText);
                    break;
                case ChatMessageType.Linkshell:
                    chatName = ParseExpressions.ChatLinkshell.Match(currentMessageText);
                    break;
                case ChatMessageType.Shout:
                    chatName = ParseExpressions.ChatShout.Match(currentMessageText);
                    break;
                case ChatMessageType.Emote:
                    chatName = ParseExpressions.ChatEmote.Match(currentMessageText);
                    break;
                case ChatMessageType.Tell:
                    if (chatSpeakerType == SpeakerType.Self)
                        chatName = ParseExpressions.ChatTellTo.Match(currentMessageText);
                    else
                        chatName = ParseExpressions.ChatTellFrom.Match(currentMessageText);
                    break;
                case ChatMessageType.NPC:
                    chatName = ParseExpressions.ChatNPC.Match(currentMessageText);
                    break;
                default:
                    parseSuccessful = true;
                    chatName = null;
                    break;
            }

            if ((chatName != null) && (chatName.Success == true))
            {
                chatSpeakerName = chatName.Groups["name"].Value;
                parseSuccessful = true;
            }
        }

        /// <summary>
        /// Determine the specific type of system message.
        /// </summary>
        private void ParseSystem()
        {
            if (messageCategory == MessageCategoryType.System)
            {
                switch (messageCode)
                {
                    case 0x00:
                        systemMessageType = SystemMessageType.ZoneChange;
                        break;
                    case 0xce:
                        systemMessageType = SystemMessageType.Echo;
                        break;
                    case 0xbf:
                        systemMessageType = SystemMessageType.EffectWearsOff;
                        Match charmCheck = ParseExpressions.NotCharmed.Match(currentMessageText);
                        if (charmCheck.Success == true)
                            MessageManager.Instance.RemovePetEntity(charmCheck.Groups["target"].Value);
                        break;
                    case 0x7b:
                        systemMessageType = SystemMessageType.OutOfRange;
                        break;
                    default:
                        Debug.WriteLine(string.Format("Unknown system message code: {0}", messageCode), "System");
                        systemMessageType = SystemMessageType.Unknown;
                        break;
                }
                parseSuccessful = true;
            }
            else
            {
                systemMessageType = SystemMessageType.Unknown;
            }
        }

        /// <summary>
        /// Determine base action type and initiate further parsing based on type.
        /// </summary>
        private void ParseAction()
        {
            if (messageCategory == MessageCategoryType.Action)
            {
                switch (messageCode)
                {
                    case 0x79: // Item drop, Lot for item, or other (equipment changed, /recast message, /anon changed, etc)
                    case 0x7f: // Item obtained
                        actionMessageType = ActionMessageType.Loot;
                        break;
                    case 0x83: // Experience gained
                        actionMessageType = ActionMessageType.Experience;
                        break;
                    case 0x92: // <me> caught a fish!
                    case 0x94: // other fishing messages, other stuff
                        actionMessageType = ActionMessageType.Fishing;
                        break;
                    default: // Mark the large swaths of possible combat messages
                        if ((messageCode >= 0x13) && (messageCode <= 0x84))
                            actionMessageType = ActionMessageType.Combat;
                        else if ((messageCode >= 0xa0) && (messageCode <= 0xbf))
                            actionMessageType = ActionMessageType.Combat;
                        else // Everything else is ignored.
                            actionMessageType = ActionMessageType.Other;
                        break;
                }
            }
            else
            {
                actionMessageType = ActionMessageType.Unknown;
            }

            switch (actionMessageType)
            {
                case ActionMessageType.Combat:
                    combatDetails = new CombatDetails();
                    ParseCombat();
                    break;
                case ActionMessageType.Loot:
                    lootDetails = new LootDetails();
                    ParseLoot();
                    break;
                case ActionMessageType.Experience:
                    ParseExperience();
                    break;
                default:
                    // Ignore Fishing for now
                    // Ignore Other always
                    break;
            }
        }

        private void ParseExperience()
        {
            Match exp;

            exp = ParseExpressions.Experience.Match(currentMessageText);
            if (exp.Success == true)
            {
                combatDetails.Experience = int.Parse(exp.Groups["number"].Value);
                combatDetails.ActorName = exp.Groups["name"].Value;
                return;
            }

            exp = ParseExpressions.ExpChain.Match(currentMessageText);
            if (exp.Success == true)
            {
                combatDetails.ExperienceChain = short.Parse(exp.Groups["number"].Value);
                return;
            }
        }


        /// <summary>
        /// Update general data based on additional messages added to the tuple.
        /// </summary>
        /// <param name="msg"></param>
        private void UpdateTupleData(MessageLine msg)
        {
            messageTextStrings.Add(msg.TextOutput);

            switch (messageCategory)
            {
                case MessageCategoryType.Action:
                    UpdateAction(msg);
                    break;
                case MessageCategoryType.Chat:
                    UpdateChat(msg);
                    break;
                case MessageCategoryType.System:
                    UpdateSystem(msg);
                    break;
            }
        }

        /// <summary>
        /// A new line has been added to the chat message.
        /// That means that the total line was broken up for the viewer display.
        /// Combine the broken parts, adding an extra space to make sure words
        /// don't run together.
        /// </summary>
        /// <param name="msg">The newest message line to be added to the message.</param>
        private void UpdateChat(MessageLine msg)
        {
            currentMessageText += " " + msg.TextOutput;
        }

        /// <summary>
        /// Placeholder code in case we need to do updates for system messages.
        /// </summary>
        /// <param name="msg">The newest message line to be added to the message.</param>
        private void UpdateSystem(MessageLine msg)
        {
            // Nothing for us to do with this at this time.
        }

        /// <summary>
        /// Update action message details.
        /// </summary>
        /// <param name="msg">The newest message line to be added to the message.</param>
        private void UpdateAction(MessageLine msg)
        {
            switch (actionMessageType)
            {
                case ActionMessageType.Combat:
                    UpdateCombat(msg);
                    break;
                case ActionMessageType.Loot:
                    UpdateLoot(msg);
                    break;
                default:
                    // Ignoring crafting/fishing for now
                    break;
            }
        }
        #endregion

        #region Loot
        /// <summary>
        /// Parse a message for loot items, including mob it dropped
        /// from and/or person who got it.
        /// </summary>
        private void ParseLoot()
        {
            Match loot;

            switch (messageCode)
            {
                // Item drop, Lot for item, equipment changed, /recast message
                case 0x79:
                    // Further specificity based on extraCode1
                    switch (extraCode1)
                    {
                        case 0:
                            // Loot drops from mob, player lots (ignored)
                            // Check to see if this is the initial message about loot being found.
                            loot = ParseExpressions.FindLoot.Match(currentMessageText);
                            if (loot.Success == true)
                            {
                                lootDetails.IsFoundMessage = true;
                                lootDetails.ItemName = loot.Groups["item"].Value;
                                lootDetails.MobName = loot.Groups["target"].Value;
                                parseSuccessful = true;
                                break;
                            }
                            // May also be the "not qualified" message (out of space, rare/ex)
                            loot = ParseExpressions.LootReqr.Match(currentMessageText);
                            if (loot.Success == true)
                            {
                                lootDetails.IsFoundMessage = false;
                                lootDetails.ItemName = loot.Groups["item"].Value;
                                parseSuccessful = true;
                                break;
                            }
                            // Or the "Item is lost" message.
                            loot = ParseExpressions.LootLost.Match(currentMessageText);
                            if (loot.Success == true)
                            {
                                lootDetails.IsFoundMessage = false;
                                lootDetails.WasLost = true;
                                lootDetails.ItemName = loot.Groups["item"].Value;
                                parseSuccessful = true;
                                break;
                            }
                            break;
                        case 3:
                            // Equipment changes, recast times, /search results
                            actionMessageType = ActionMessageType.Other;
                            break;
                        default:
                            // Other
                            actionMessageType = ActionMessageType.Other;
                            Debug.WriteLine(string.Format("Unknown loot subcode: {0}\nMessage: {1}", extraCode1, currentMessageText), "Loot");
                            break;
                    }
                    break;
                // Item obtained
                case 0x7f:
                    loot = ParseExpressions.GetGil.Match(currentMessageText);
                    if (loot.Success == true)
                    {
                        lootDetails.IsFoundMessage = false;
                        lootDetails.Gil = int.Parse(loot.Groups["money"].Value);
                        lootDetails.WhoObtained = loot.Groups["name"].Value;
                        parseSuccessful = true;
                        break;
                    }
                    loot = ParseExpressions.GetLoot.Match(currentMessageText);
                    if (loot.Success == true)
                    {
                        lootDetails.IsFoundMessage = false;
                        lootDetails.ItemName = loot.Groups["item"].Value;
                        lootDetails.WhoObtained = loot.Groups["name"].Value;
                        parseSuccessful = true;
                        break;
                    }
                    break;
            }
        }

        /// <summary>
        /// Continue parsing multi-message for loot details.
        /// </summary>
        /// <param name="msg">The additional message line to add to the message.</param>
        private void UpdateLoot(MessageLine msg)
        {
            if (parseSuccessful == true)
            {
                // If parsing the previous message was successful, this is additional
                // information (eg: "Item is lost.").
                // Send this directly back through the basic parsing structure.

                currentMessageText = msg.TextOutput;
                ParseLoot();
            }
            else
            {
                // If parsing the previous message was not successful, the result was
                // broken up across multiple lines.  Combine this line with the previous
                // and try re-parsing.

                currentMessageText = currentMessageText + " " + msg.TextOutput;
                ParseLoot();
            }
        }
        #endregion

        #region Combat
        /// <summary>
        /// Parses the combat message and breaks out the individual pieces.
        /// </summary>
        private void ParseCombat()
        {
            Match combatMatch = null;
            parseSuccessful = false;
            TargetDetails target = null;
            uint searchCode = messageCode;


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


            // Use lookup tables for general categories based on message code
            // Only lookup if unknown category type.  Multi-line messages will already know this info.
            if (combatDetails.CombatCategory == CombatActionType.Unknown)
            {
                combatDetails.CombatCategory = parseCodesLookup.GetCombatCategory(messageCode);

                if (combatDetails.CombatCategory == CombatActionType.Buff)
                    combatDetails.BuffType = parseCodesLookup.GetBuffType(messageCode);

                if (combatDetails.CombatCategory == CombatActionType.Attack)
                {
                    combatDetails.AttackType = parseCodesLookup.GetAttackType(messageCode);
                    combatDetails.SuccessLevel = parseCodesLookup.GetSuccessType(messageCode);
                }
            }

            switch (combatDetails.CombatCategory)
            {
                #region Attacks
                case CombatActionType.Attack:
                    switch (combatDetails.AttackType)
                    {
                        case AttackType.Damage:
                            switch (combatDetails.SuccessLevel)
                            {
                                case SuccessType.Successful:
                                    parseSuccessful = ParseSuccessfulAttack(ref target);
                                    break;
                                case SuccessType.Unsuccessful:
                                    parseSuccessful = ParseUnuccessfulAttack(ref target);
                                    break;
                                case SuccessType.Failed:
                                default:
                                    // for SuccessType.Unknown
                                    throw new ArgumentOutOfRangeException("messageCode", messageCode,
                                        string.Format("Success type: {0}", combatDetails.SuccessLevel));
                            }
                            break;
                        #region Enfeeble Group
                        case AttackType.Enfeeble:
                            // For prepping attack spells or abilities
                            combatMatch = ParseExpressions.PrepSpell.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                combatDetails.IsPreparing = true;
                                combatDetails.ActionSource = ActionSourceType.Spell;
                                combatDetails.ActorName = combatMatch.Groups["name"].Value;
                                combatDetails.ActionName = combatMatch.Groups["spell"].Value;
                                target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                                break;
                            }
                            combatMatch = ParseExpressions.PrepAbility.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                combatDetails.IsPreparing = true;
                                combatDetails.ActorName = combatMatch.Groups["name"].Value;
                                combatDetails.ActionName = combatMatch.Groups["ability"].Value;
                                if (Weaponskills.NamesList.Contains(combatDetails.ActionName))
                                    combatDetails.ActionSource = ActionSourceType.Weaponskill;
                                else
                                    combatDetails.ActionSource = ActionSourceType.Ability;
                                break;
                            }
                            // Failed enfeebles.  IE: <spell> had no effect.
                            if (combatDetails.SuccessLevel == SuccessType.Failed)
                            {
                                combatMatch = ParseExpressions.NoEffect.Match(currentMessageText);
                                if (combatMatch.Success == true)
                                {
                                    combatDetails.ActionSource = ActionSourceType.Spell;
                                    combatDetails.ActorName = combatMatch.Groups["name"].Value;
                                    combatDetails.ActionName = combatMatch.Groups["spell"].Value;
                                    target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                                    target.SuccessLevel = SuccessType.Failed;
                                    combatDetails.SuccessLevel = SuccessType.Unknown;
                                }
                                break;
                            }
                            // Check first for lines where the action is activated
                            combatMatch = ParseExpressions.CastSpell.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                combatDetails.ActionSource = ActionSourceType.Spell;
                                combatDetails.ActorName = combatMatch.Groups["name"].Value;
                                combatDetails.ActionName = combatMatch.Groups["spell"].Value;
                                break;
                            }
                            combatMatch = ParseExpressions.UseAbility.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                combatDetails.ActionSource = ActionSourceType.Ability;
                                combatDetails.ActorName = combatMatch.Groups["name"].Value;
                                combatDetails.ActionName = combatMatch.Groups["ability"].Value;
                                break;
                            }
                            combatMatch = ParseExpressions.UseAbilityOn.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                combatDetails.ActionSource = ActionSourceType.Ability;
                                combatDetails.ActorName = combatMatch.Groups["name"].Value;
                                combatDetails.ActionName = combatMatch.Groups["ability"].Value;
                                target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                                target.SuccessLevel = SuccessType.Successful;
                                break;
                            }
                            combatMatch = ParseExpressions.MissAbility.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                combatDetails.ActionSource = ActionSourceType.Ability;
                                combatDetails.ActorName = combatMatch.Groups["name"].Value;
                                combatDetails.ActionName = combatMatch.Groups["ability"].Value;
                                target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                                target.SuccessLevel = SuccessType.Unsuccessful;
                                target.Defended = true;
                                target.DefenseType = DefenseType.Evade;
                                break;
                            }
                            // Then check for individual lines about the effect on the target
                            combatMatch = ParseExpressions.Debuff.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                                target.SuccessLevel = SuccessType.Successful;
                                break;
                            }
                            combatMatch = ParseExpressions.Enfeeble.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                                target.SuccessLevel = SuccessType.Successful;
                                break;
                            }
                            combatMatch = ParseExpressions.ResistSpell.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                                target.SuccessLevel = SuccessType.Unsuccessful;
                                target.Defended = true;
                                target.DefenseType = DefenseType.Resist;
                                break;
                            }
                            combatMatch = ParseExpressions.ResistEffect.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                                target.SuccessLevel = SuccessType.Unsuccessful;
                                target.Defended = true;
                                target.DefenseType = DefenseType.Resist;
                                break;
                            }
                            // Special handling: Charms
                            combatMatch = ParseExpressions.Charmed.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                                target.SuccessLevel = SuccessType.Successful;

                                if (combatDetails.ActionName == "Charm")
                                {
                                    // Only for players.  Mobs use other ability names to charm players.
                                    // Mob type has been charmed.  Add to the entity lookup list as a pet.
                                    MessageManager.Instance.AddPetEntity(target.Name);
                                    target.EntityType = EntityType.Pet;
                                    combatDetails.ActorEntityType = EntityType.Player;
                                }
                                break;
                            }
                            combatMatch = ParseExpressions.FailsCharm.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                combatDetails.ActionSource = ActionSourceType.Ability;
                                combatDetails.ActorName = combatMatch.Groups["name"].Value;
                                target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                                target.SuccessLevel = SuccessType.Unsuccessful;
                                target.Defended = true;
                                target.DefenseType = DefenseType.Resist;
                                break;
                            }
                            // Last check in case of some out of all targets not getting affected.
                            combatMatch = ParseExpressions.NoEffect.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                combatDetails.ActorName = combatMatch.Groups["name"].Value;
                                combatDetails.ActionName = combatMatch.Groups["spell"].Value;
                                target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                                target.SuccessLevel = SuccessType.Failed;
                                combatDetails.SuccessLevel = SuccessType.Unknown;
                            }
                            break;
                        #endregion
                        case AttackType.Unknown:
                            #region Failed Actions
                            // Deal with possible Failed actions first
                            if (combatDetails.SuccessLevel == SuccessType.Failed)
                            {
                                combatMatch = ParseExpressions.Interrupted.Match(currentMessageText);
                                if (combatMatch.Success == true)
                                {
                                    combatDetails.ActorName = combatMatch.Groups["name"].Value;
                                    combatDetails.FailedActionType = FailedActionType.Interrupted;
                                    break;
                                }
                                combatMatch = ParseExpressions.Paralyzed.Match(currentMessageText);
                                if (combatMatch.Success == true)
                                {
                                    combatDetails.ActorName = combatMatch.Groups["name"].Value;
                                    combatDetails.FailedActionType = FailedActionType.Paralyzed;
                                    break;
                                }
                                combatMatch = ParseExpressions.Intimidated.Match(currentMessageText);
                                if (combatMatch.Success == true)
                                {
                                    // Treat intimidation as a defense type, like Parry
                                    combatDetails.ActorName = combatMatch.Groups["name"].Value;
                                    target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                                    target.Defended = true;
                                    target.DefenseType = DefenseType.Intimidate;
                                    // Adjust message category
                                    combatDetails.CombatCategory = CombatActionType.Attack;
                                    combatDetails.AttackType = AttackType.Unknown;
                                    combatDetails.SuccessLevel = SuccessType.Unsuccessful;
                                    //combatDetails.FailedActionType = FailedActionType.Intimidated;
                                    break;
                                }
                                // Matches for the following to note the line as successfully parsed.
                                combatMatch = ParseExpressions.UnableToCast.Match(currentMessageText);
                                if (combatMatch.Success == true)
                                {
                                    combatDetails.FailedActionType = FailedActionType.UnableToCast;
                                    break;
                                }
                                combatMatch = ParseExpressions.UnableToUse.Match(currentMessageText);
                                if (combatMatch.Success == true)
                                {
                                    combatDetails.FailedActionType = FailedActionType.UnableToUse;
                                    break;
                                }
                                combatMatch = ParseExpressions.CannotSee.Match(currentMessageText);
                                if (combatMatch.Success == true)
                                {
                                    target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                                    combatDetails.FailedActionType = FailedActionType.CannotSee;
                                    break;
                                }
                                combatMatch = ParseExpressions.TooFarAway.Match(currentMessageText);
                                if (combatMatch.Success == true)
                                {
                                    target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                                    combatDetails.FailedActionType = FailedActionType.TooFarAway;
                                    break;
                                }
                                combatMatch = ParseExpressions.OutOfRange.Match(currentMessageText);
                                if (combatMatch.Success == true)
                                {
                                    target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                                    combatDetails.FailedActionType = FailedActionType.OutOfRange;
                                    break;
                                }
                                combatMatch = ParseExpressions.NotEnoughMP.Match(currentMessageText);
                                if (combatMatch.Success == true)
                                {
                                    combatDetails.FailedActionType = FailedActionType.NotEnoughMP;
                                    break;
                                }
                                combatMatch = ParseExpressions.NotEnoughTP.Match(currentMessageText);
                                if (combatMatch.Success == true)
                                {
                                    combatDetails.FailedActionType = FailedActionType.NotEnoughTP;
                                    break;
                                }
                                break;
                            }
                            #endregion
                            // For prepping attack spells or abilities
                            combatMatch = ParseExpressions.PrepSpell.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                combatDetails.IsPreparing = true;
                                combatDetails.ActionSource = ActionSourceType.Spell;
                                combatDetails.ActorName = combatMatch.Groups["name"].Value;
                                combatDetails.ActionName = combatMatch.Groups["spell"].Value;
                                break;
                            }
                            combatMatch = ParseExpressions.PrepSpellOn.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                combatDetails.IsPreparing = true;
                                combatDetails.ActionSource = ActionSourceType.Spell;
                                combatDetails.ActorName = combatMatch.Groups["name"].Value;
                                combatDetails.ActionName = combatMatch.Groups["spell"].Value;
                                combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                                break;
                            }
                            combatMatch = ParseExpressions.PrepAbility.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                combatDetails.IsPreparing = true;
                                combatDetails.ActorName = combatMatch.Groups["name"].Value;
                                combatDetails.ActionName = combatMatch.Groups["ability"].Value;
                                if (Weaponskills.NamesList.Contains(combatDetails.ActionName))
                                    combatDetails.ActionSource = ActionSourceType.Weaponskill;
                                else
                                    combatDetails.ActionSource = ActionSourceType.Ability;
                                break;
                            }
                            break;
                    }
                    break;
                #endregion
                #region Buffs
                case CombatActionType.Buff:
                    // Check first for lines where the buff is activated
                    combatMatch = ParseExpressions.CastSpell.Match(currentMessageText);
                    if (combatMatch.Success == true)
                    {
                        combatDetails.ActionSource = ActionSourceType.Spell;
                        combatDetails.ActorName = combatMatch.Groups["name"].Value;
                        combatDetails.ActionName = combatMatch.Groups["spell"].Value;
                        break;
                    }
                    combatMatch = ParseExpressions.UseAbility.Match(currentMessageText);
                    if (combatMatch.Success == true)
                    {
                        combatDetails.ActionSource = ActionSourceType.Ability;
                        combatDetails.ActorName = combatMatch.Groups["name"].Value;
                        combatDetails.ActionName = combatMatch.Groups["ability"].Value;
                        break;
                    }
                    // Break down the results of the spell/ability
                    switch (combatDetails.BuffType)
                    {
                        case BuffType.Enhance:
                            combatMatch = ParseExpressions.Buff.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                                if (combatDetails.ActionName == string.Empty)
                                    combatDetails.ActionName = combatMatch.Groups["effect"].Value;
                                break;
                            }
                            combatMatch = ParseExpressions.Enhance.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                                if (combatDetails.ActionName == string.Empty)
                                    combatDetails.ActionName = combatMatch.Groups["effect"].Value;
                                break;
                            }
                            // Check in case of some out of all targets not getting affected.
                            combatMatch = ParseExpressions.NoEffect.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                combatDetails.ActorName = combatMatch.Groups["name"].Value;
                                combatDetails.ActionName = combatMatch.Groups["spell"].Value;
                                target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                                target.SuccessLevel = SuccessType.Failed;
                                combatDetails.SuccessLevel = SuccessType.Unknown;
                            }
                            // Self-target buffs have various strings which we won't check for.
                            // Only look to see if the message line completes the sentence (ends in a period).
                            if (currentMessageText.EndsWith("."))
                            {
                                if ((combatDetails.ActorName != string.Empty) &&
                                    (combatDetails.ActionName != string.Empty))
                                {
                                    target = combatDetails.Targets.Add(combatDetails.ActorName);
                                    parseSuccessful = true;
                                }
                            }
                            break;
                        case BuffType.Recovery:
                            combatMatch = ParseExpressions.RecoversHP.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                                target.Amount = int.Parse(combatMatch.Groups["number"].Value);
                                target.RecoveryType = RecoveryType.RecoverHP;
                                break;
                            }
                            combatMatch = ParseExpressions.RecoversMP.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                                target.Amount = int.Parse(combatMatch.Groups["number"].Value);
                                target.RecoveryType = RecoveryType.RecoverMP;
                                break;
                            }
                            break;
                        case BuffType.Unknown:
                            // For prepping buffing spells or abilities (do we need this?)
                            combatMatch = ParseExpressions.PrepSpell.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                combatDetails.IsPreparing = true;
                                combatDetails.ActionSource = ActionSourceType.Spell;
                                combatDetails.ActorName = combatMatch.Groups["name"].Value;
                                combatDetails.ActionName = combatMatch.Groups["spell"].Value;
                                target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                                break;
                            }
                            combatMatch = ParseExpressions.PrepAbility.Match(currentMessageText);
                            if (combatMatch.Success == true)
                            {
                                combatDetails.IsPreparing = true;
                                combatDetails.ActorName = combatMatch.Groups["name"].Value;
                                combatDetails.ActionName = combatMatch.Groups["ability"].Value;
                                if (Weaponskills.NamesList.Contains(combatDetails.ActionName))
                                    combatDetails.ActionSource = ActionSourceType.Weaponskill;
                                else
                                    combatDetails.ActionSource = ActionSourceType.Ability;
                                break;
                            }
                            break;
                    }
                    break;
                #endregion
                #region Death
                case CombatActionType.Death:
                    combatMatch = ParseExpressions.Defeat.Match(currentMessageText);
                    if (combatMatch.Success == true)
                    {
                        combatDetails.ActorName = combatMatch.Groups["name"].Value;
                        target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                        break;
                    }
                    combatMatch = ParseExpressions.Dies.Match(currentMessageText);
                    if (combatMatch.Success == true)
                    {
                        target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                        break;
                    }
                    break;
                #endregion
                #region Unknown
                case CombatActionType.Unknown:
                default:
                    // Prepping spell of unknown type
                    combatMatch = ParseExpressions.PrepSpell.Match(currentMessageText);
                    if (combatMatch.Success == true)
                    {
                        combatDetails.IsPreparing = true;
                        combatDetails.ActorName = combatMatch.Groups["name"].Value;
                        combatDetails.ActionName = combatMatch.Groups["spell"].Value;
                        combatDetails.ActionSource = ActionSourceType.Spell;
                        break;
                    }
                    combatMatch = ParseExpressions.PrepSpellOn.Match(currentMessageText);
                    if (combatMatch.Success == true)
                    {
                        combatDetails.IsPreparing = true;
                        combatDetails.ActorName = combatMatch.Groups["name"].Value;
                        combatDetails.ActionName = combatMatch.Groups["spell"].Value;
                        combatDetails.ActionSource = ActionSourceType.Spell;
                        break;
                    }
                    combatMatch = ParseExpressions.PrepAbility.Match(currentMessageText);
                    if (combatMatch.Success == true)
                    {
                        combatDetails.IsPreparing = true;
                        combatDetails.ActorName = combatMatch.Groups["name"].Value;
                        combatDetails.ActionName = combatMatch.Groups["ability"].Value;
                        if (Weaponskills.NamesList.Contains(combatDetails.ActionName))
                            combatDetails.ActionSource = ActionSourceType.Weaponskill;
                        else
                            combatDetails.ActionSource = ActionSourceType.Ability;
                        break;
                    }
                    break;
                #endregion
            }

            // Note whether the parse was successful.  If external functions were called, they
            // would have set parseSuccessful's value on return.
            if ((combatMatch != null) && (parseSuccessful == false))
                parseSuccessful = combatMatch.Success;


            #region Figure out Entities (messy)
            // First order check based on lookups
            if (combatDetails.ActorName != string.Empty)
            {
                if (combatDetails.ActorEntityType == EntityType.Unknown)
                    combatDetails.ActorEntityType = MessageManager.Instance.LookupEntity(combatDetails.ActorName);

                // If the name isn't in the lookup table, check for pet names.
                if (combatDetails.ActorEntityType == EntityType.Unknown)
                {
                    EntityType checkEntity = DetermineIfMobOrPet(combatDetails.ActorName);
                    // If it comes back as a pet, use that.  An unidentified player name will come back as a mob, so ignore.
                    if (checkEntity == EntityType.Pet)
                        combatDetails.ActorEntityType = EntityType.Pet;
                }
            }

            foreach (TargetDetails targ in combatDetails.Targets)
            {
                if (targ.Name != string.Empty)
                {
                    if (targ.EntityType == EntityType.Unknown)
                        targ.EntityType = MessageManager.Instance.LookupEntity(targ.Name);
                }

                // If the name isn't in the lookup table, check for pet names.
                if (targ.EntityType == EntityType.Unknown)
                {
                    EntityType checkEntity = DetermineIfMobOrPet(targ.Name);
                    // If it comes back as a pet, use that.  An unidentified player name will come back as a mob, so ignore.
                    if (checkEntity == EntityType.Pet)
                        targ.EntityType = EntityType.Pet;
                }
            }


            // Next check for indirect resolutions based on known entity types
            if ((combatDetails.ActorEntityType != EntityType.Unknown) && (combatDetails.Targets.Any(t => t.EntityType == EntityType.Unknown) == true))
            {
                // Targets are unidentified, but we know the actor type

                if (combatDetails.ActorEntityType == EntityType.Player)
                {
                    if ((combatDetails.CombatCategory == CombatActionType.Attack) || (combatDetails.CombatCategory == CombatActionType.Death))
                    {
                        // An attack action must be directed at a mob
                        foreach (TargetDetails targ in combatDetails.Targets)
                        {
                            targ.EntityType = EntityType.Mob;
                        }
                    }
                    else if (combatDetails.CombatCategory == CombatActionType.Buff)
                    {
                        // A buff action must be directed at a player (no known player buffs directly affect pets).
                        foreach (TargetDetails targ in combatDetails.Targets)
                        {
                            if (targ.EntityType == EntityType.Unknown)
                                targ.EntityType = EntityType.Player;
                        }
                    }

                    // If it's an unknown action category, don't attempt to resolve.
                }
                else if (combatDetails.ActorEntityType == EntityType.Pet)
                {
                    // If it resolved as a pet, it's either a normal pet or a charmed mob of a type that hasn't been fought.

                    if ((combatDetails.CombatCategory == CombatActionType.Attack) || (combatDetails.CombatCategory == CombatActionType.Death))
                    {
                        // An attack action must be directed at a mob
                        foreach (TargetDetails targ in combatDetails.Targets)
                        {
                            targ.EntityType = EntityType.Mob;
                        }
                    }
                    else if (combatDetails.CombatCategory == CombatActionType.Buff)
                    {
                        // A buff action must be directed at a player
                        // Example pet buffs: automatons curing players, avatars giving hastega, etc.
                        // If target is a 'normal' pet, it should have already been resolved in the lookup above.
                        foreach (TargetDetails targ in combatDetails.Targets)
                        {
                            if (targ.EntityType == EntityType.Unknown)
                                targ.EntityType = EntityType.Player;
                        }
                    }
                }
                else if (combatDetails.ActorEntityType == EntityType.Mob)
                {
                    // If it resolved as a mob, it may possibly be a charmed pet.
                    // Need to do some juggling here to figure things out.

                    if (combatDetails.CombatCategory == CombatActionType.Attack)
                    {
                        // An attack action must be directed at a player or pet
                        // If we haven't already determined the target to be a pet based on name, it's
                        // a charmed pet.  We can't properly distinguish between all charmed mob names
                        // and player names, so leaving unresolved.
                    }
                    else if (combatDetails.CombatCategory == CombatActionType.Buff)
                    {
                        // A mob buff action must be directed at another mob.
                        foreach (TargetDetails targ in combatDetails.Targets)
                        {
                            if (targ.EntityType == EntityType.Unknown)
                                targ.EntityType = EntityType.Mob;
                        }
                    }
                }
            }
            else if ((combatDetails.ActorEntityType == EntityType.Unknown) && (combatDetails.Targets.Any(t => t.EntityType != EntityType.Unknown) == true))
            {
                // Actor is unknown, but we know at least one of the target types.  Same as above, just reversed.
                TargetDetails targ = combatDetails.Targets.Find(t => t.EntityType != EntityType.Unknown);

                if (targ.EntityType == EntityType.Player)
                {
                    if ((combatDetails.CombatCategory == CombatActionType.Attack) || (combatDetails.CombatCategory == CombatActionType.Death))
                    {
                        // An attack action must be coming from a mob
                        combatDetails.ActorEntityType = EntityType.Mob;
                    }
                    else if (combatDetails.CombatCategory == CombatActionType.Buff)
                    {
                        // A buff action must be coming from a player ('normal' pets would have already been
                        // resolved, and charmed pets can't buff players).
                        combatDetails.ActorEntityType = EntityType.Player;
                    }

                    // If it's an unknown action category, don't attempt to resolve.
                }
                else if (targ.EntityType == EntityType.Pet)
                {
                    // If it resolved as a pet, it's either a normal pet or a charmed mob of a type that hasn't been fought.

                    if ((combatDetails.CombatCategory == CombatActionType.Attack) || (combatDetails.CombatCategory == CombatActionType.Death))
                    {
                        // An attack action must be coming from a mob
                        combatDetails.ActorEntityType = EntityType.Mob;
                    }
                    else if (combatDetails.CombatCategory == CombatActionType.Buff)
                    {
                        // Most pets can only receive self-buffs.  As such, actor should have already been
                        // resolved and we can't get here.  Leaving unresolved at this point.
                    }

                    // If it's an unknown action category, don't attempt to resolve.
                }
                else if (targ.EntityType == EntityType.Mob)
                {
                    // If it resolved as a mob, it may possibly be a charmed pet.
                    // Need to do some juggling here to figure things out.

                    if (combatDetails.CombatCategory == CombatActionType.Attack)
                    {
                        // An attack action must be directed at a player or pet
                        // If we haven't already determined the actor to be a pet based on name, it's
                        // a charmed pet.  We can't properly distinguish between all charmed mob names
                        // and player names, so leaving unresolved.
                    }
                    else if (combatDetails.CombatCategory == CombatActionType.Buff)
                    {
                        // A mob buff action must be directed at another mob.
                        combatDetails.ActorEntityType = EntityType.Mob;
                    }
                }
            }
            else if ((combatDetails.ActorEntityType == EntityType.Mob) && (combatDetails.Targets.Any(t => t.EntityType == EntityType.Mob) == true))
            {
                // Both actor and target(s) are marked as mobs.  If this is an attack/kill move, one or the other must be a pet.

                if ((combatDetails.CombatCategory == CombatActionType.Attack) || (combatDetails.CombatCategory == CombatActionType.Death))
                {
                    // If there are multiple targets, if any are marked as pets or players, the mob target must be in error.
                    if ((combatDetails.Targets.Any(t => t.EntityType == EntityType.Pet) == true) ||
                        (combatDetails.Targets.Any(t => t.EntityType == EntityType.Player) == true))
                    {
                        foreach (TargetDetails targ in combatDetails.Targets)
                        {
                            if (targ.EntityType == EntityType.Mob)
                                targ.EntityType = EntityType.Pet;
                        }
                    }
                    else
                    {
                        EntityType checkActorEntity = MessageManager.Instance.LookupPetEntity(combatDetails.ActorName);
                        EntityType checkTargetEntity = MessageManager.Instance.LookupPetEntity(combatDetails.Targets.Find(t => t.EntityType == EntityType.Mob).Name);

                        // If checking for pets before mobs in the lookup table results in one of them showing up as a pet, use that.
                        if ((checkActorEntity == EntityType.Pet) && (checkActorEntity == EntityType.Mob))
                        {
                            combatDetails.ActorEntityType = EntityType.Pet;
                        }
                        else if ((checkActorEntity == EntityType.Mob) && (checkActorEntity == EntityType.Pet))
                        {
                            foreach (TargetDetails targ in combatDetails.Targets)
                            {
                                if (targ.EntityType == EntityType.Mob)
                                    targ.EntityType = EntityType.Pet;
                            }
                        }
                        else
                        {
                            // If both resolve as possible pets, or if both resolve as mobs, flip a coin because we have no other
                            // way of determining which is which.
                            Random rand = new Random();
                            if (rand.Next(100) < 50)
                            {
                                combatDetails.ActorEntityType = EntityType.Pet;
                            }
                            else
                            {
                                foreach (TargetDetails targ in combatDetails.Targets)
                                    targ.EntityType = EntityType.Pet;
                            }
                        }
                    }
                }
            }
            else if ((combatDetails.ActorEntityType == EntityType.Pet) && (combatDetails.Targets.Any(t => t.EntityType == EntityType.Pet) == true))
            {
                // Both actor and target(s) are marked as pets.  If this is an attack/kill move, one or the other must be a mob.

                if ((combatDetails.CombatCategory == CombatActionType.Attack) || (combatDetails.CombatCategory == CombatActionType.Death))
                {
                    // If there are multiple targets, if any are marked as mobs, the pet target must be in error.
                    if (combatDetails.Targets.Any(t => t.EntityType == EntityType.Mob) == true)
                    {
                        foreach (TargetDetails targ in combatDetails.Targets)
                        {
                            if (targ.EntityType == EntityType.Pet)
                                targ.EntityType = EntityType.Mob;
                        }
                    }
                    else
                    {
                        // If one of the disputed pets is a 'normal' pet (avatar, puppet, etc), take it as the correct one.
                        if (DetermineIfMobOrPet(combatDetails.ActorName) == EntityType.Pet)
                        {
                            foreach (TargetDetails targ in combatDetails.Targets)
                            {
                                if (targ.EntityType == EntityType.Pet)
                                    targ.EntityType = EntityType.Mob;
                            }
                        }
                        else
                        {
                            bool foundTargetPet = false;

                            foreach (TargetDetails targ in combatDetails.Targets)
                            {
                                if (DetermineIfMobOrPet(targ.Name) == EntityType.Pet)
                                {
                                    combatDetails.ActorEntityType = EntityType.Mob;
                                    foundTargetPet = true;
                                    break;
                                }
                            }

                            if (foundTargetPet == false)
                            {
                                // If both resolve as possible pets, or if both resolve as mobs, flip a coin because we have no other
                                // way of determining which is which.
                                Random rand = new Random();
                                if (rand.Next(100) < 50)
                                {
                                    combatDetails.ActorEntityType = EntityType.Pet;
                                }
                                else
                                {
                                    foreach (TargetDetails targ in combatDetails.Targets)
                                        targ.EntityType = EntityType.Pet;
                                }
                            }
                        }
                    }
                }
            }
            #endregion


            // If we have an attack or buff message that parsed successfully, but where the actor type was
            // undetermined, try to locate the message chain it should be a part of from the
            // message manager's list of messages.
            if (parseSuccessful == true)
            {
                if ((combatDetails.CombatCategory == CombatActionType.Attack) ||
                    (combatDetails.CombatCategory == CombatActionType.Buff))
                {
                    if ((combatDetails.ActorName == string.Empty) && (combatDetails.SuccessLevel != SuccessType.Failed) &&
                        (combatDetails.IsPreparing == false))
                    {
                        OldMessage previousMessage = MessageManager.Instance.FindLastOldMessageWithCode(searchCode);

                        // Message was found
                        if (previousMessage != null)
                        {
                            foreach (MessageLine msgLine in msgLineCollection)
                                previousMessage.AddMessage(msgLine);

                            // Flag it so that we don't get duplicates
                            FlagDoNotAddToManagerCollection = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handle the specifics of parsing successful attacks.
        /// </summary>
        /// <param name="target">Target that we may extract from the data.</param>
        /// <returns>Returns true if we made a successful match.</returns>
        private bool ParseSuccessfulAttack(ref TargetDetails target)
        {
            Match combatMatch;
            target = null;

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
                    if (Weaponskills.NamesList.Contains(combatMatch.Groups["ability"].Value))
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
                    combatDetails.IsCrit = true;
                    combatDetails.ActionSource = ActionSourceType.Melee;
                }
            }

            if (combatMatch.Success == false)
            {
                combatMatch = ParseExpressions.RangedCriticalHit.Match(currentMessageText);
                if (combatMatch.Success == true)
                {
                    combatDetails.IsCrit = true;
                    combatDetails.ActionSource = ActionSourceType.Ranged;
                }
            }


            // Handle this group first
            if (combatMatch.Success == true)
            {
                switch (combatDetails.ActionSource)
                {
                    case ActionSourceType.Melee:
                    case ActionSourceType.Ranged:
                    case ActionSourceType.Counterattack:
                    case ActionSourceType.Spikes:
                    case ActionSourceType.Spell:
                    case ActionSourceType.Weaponskill:
                        combatDetails.ActorName = combatMatch.Groups["name"].Value;
                        break;
                }

                // skip in cases of crit hit lines that won't provide target info
                if (combatDetails.IsCrit == false)
                {
                    switch (combatDetails.ActionSource)
                    {
                        case ActionSourceType.Melee:
                        case ActionSourceType.Counterattack:
                        case ActionSourceType.Spikes:
                            target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                            target.Amount = int.Parse(combatMatch.Groups["damage"].Value);
                            break;
                    }
                }

                // Actor entity
                switch (messageCode)
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
                        // mob vs pet or pet vs mob or non-party member vs mob or vice versa
                        combatDetails.ActorEntityType = MessageManager.Instance.LookupPetEntity(combatDetails.ActorName);
                        if (combatDetails.ActorEntityType == EntityType.Unknown)
                            combatDetails.ActorEntityType = DetermineIfMobOrPet(combatDetails.ActorName);

                        // If we have target info, double-check vs target entity type in case
                        // actor type is unknown.
                        if (combatDetails.IsCrit == false)
                        {
                            if (combatDetails.ActorEntityType == EntityType.Unknown)
                            {
                                target.EntityType = MessageManager.Instance.LookupPetEntity(target.Name);
                                if (target.EntityType == EntityType.Unknown)
                                {
                                    target.EntityType = DetermineIfMobOrPet(target.Name);
                                }

                                if ((target.EntityType == EntityType.Pet) || (target.EntityType == EntityType.Player))
                                {
                                    combatDetails.ActorEntityType = EntityType.Mob;
                                    break;
                                }
                                else if (target.EntityType == EntityType.Mob)
                                {
                                    combatDetails.ActorEntityType = EntityType.Player;
                                    break;
                                }
                            }
                        }
                        break;
                    default:
                        // other??
                        combatDetails.ActorEntityType = MessageManager.Instance.LookupEntity(combatDetails.ActorName);
                        break;
                }


                // Crits only get actor details on the first pass through
                if (combatDetails.IsCrit == true)
                    return combatMatch.Success;

                // Target entity
                if (target != null)
                {
                    target.SuccessLevel = SuccessType.Successful;

                    switch (messageCode)
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
                            // mob vs pet or pet vs mob or non-party member vs mob or vice versa
                            target.EntityType = MessageManager.Instance.LookupEntity(target.Name);
                            if (target.EntityType == EntityType.Unknown)
                                target.EntityType = DetermineIfMobOrPet(target.Name);

                            if (combatDetails.ActorEntityType == EntityType.Unknown)
                            {
                                combatDetails.ActorEntityType = MessageManager.Instance.LookupPetEntity(combatDetails.ActorName);
                                if (combatDetails.ActorEntityType == EntityType.Unknown)
                                    combatDetails.ActorEntityType = DetermineIfMobOrPet(combatDetails.ActorName);
                            }

                            if (target.EntityType == EntityType.Unknown)
                            {
                                if ((combatDetails.ActorEntityType == EntityType.Pet) ||
                                 (combatDetails.ActorEntityType == EntityType.Player))
                                {
                                    target.EntityType = EntityType.Mob;
                                    break;
                                }
                                else if (combatDetails.ActorEntityType == EntityType.Mob)
                                {
                                    target.EntityType = EntityType.Player;
                                }
                            }

                            if (combatDetails.ActorEntityType == EntityType.Unknown)
                            {
                                if ((target.EntityType == EntityType.Pet) || 
                                    (target.EntityType == EntityType.Player))
                                {
                                    combatDetails.ActorEntityType = EntityType.Mob;
                                    break;
                                }
                                else if (target.EntityType == EntityType.Mob)
                                {
                                    combatDetails.ActorEntityType = EntityType.Player;
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
                            target.EntityType = MessageManager.Instance.LookupEntity(target.Name);
                            break;
                    }
                }

                switch (combatDetails.ActionSource)
                {
                    case ActionSourceType.Spell:
                        combatDetails.ActionName = combatMatch.Groups["spell"].Value;
                        break;
                    case ActionSourceType.Ability:
                    case ActionSourceType.Weaponskill:
                        combatDetails.ActionName = combatMatch.Groups["ability"].Value;
                        break;
                }

                return combatMatch.Success;
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
                        target = combatDetails.Targets.Find(t => t.Name == combatMatch.Groups["target"].Value);
                        if (target != null)
                        {
                            target.AdditionalEffect = true;
                            target.AdditionalDamage = uint.Parse(combatMatch.Groups["damage"].Value);
                        }
                    }
                    else
                    {
                        modifyMatch = ParseExpressions.MagicBurst.Match(currentMessageText);
                        if (modifyMatch.Success == true)
                        {
                            target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                            target.DamageModifier = DamageModifier.MagicBurst;
                            target.Amount = int.Parse(combatMatch.Groups["damage"].Value);
                        }
                    }

                    if (target == null)
                        target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);

                    target.SuccessLevel = SuccessType.Successful;

                    if (combatDetails.IsCrit == true)
                        target.DamageModifier = DamageModifier.Critical;

                    if (modifyMatch.Success == false)
                        target.Amount = int.Parse(combatMatch.Groups["damage"].Value);
                }
            }

            // Handle entity settings on additional targets
            if (combatMatch.Success == true)
            {
                if (target != null)
                {
                    switch (messageCode)
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
                            target.EntityType = MessageManager.Instance.LookupEntity(target.Name);
                            if (target.EntityType == EntityType.Unknown)
                            {
                                target.EntityType = DetermineIfMobOrPet(target.Name);
                            }

                            if (target.EntityType == EntityType.Unknown)
                            {
                                if ((combatDetails.ActorEntityType == EntityType.Pet) ||
                                    (combatDetails.ActorEntityType == EntityType.Player))
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
                            target.EntityType = MessageManager.Instance.LookupPetEntity(target.Name);
                            break;
                    }
                }
            }

            return combatMatch.Success;
        }

        /// <summary>
        /// Handles the specifics of parsing unsuccessful attacks.
        /// </summary>
        /// <param name="target">Target that we may extract from the data.</param>
        /// <returns>Returns true if we made a successful match.</returns>
        private bool ParseUnuccessfulAttack(ref TargetDetails target)
        {
            Match combatMatch;
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
                combatDetails.ActorName = combatMatch.Groups["name"].Value;

                switch (combatDetails.ActionSource)
                {
                    case ActionSourceType.Spell:
                        combatDetails.ActionName = combatMatch.Groups["spell"].Value;
                        break;
                    case ActionSourceType.Ability:
                        combatDetails.ActionName = combatMatch.Groups["ability"].Value;
                        if (Weaponskills.NamesList.Contains(combatDetails.ActionName))
                            combatDetails.ActionSource = ActionSourceType.Weaponskill;
                        break;
                }

                // Determine actor entity (no target is provided in the above message lines)
                switch (messageCode)
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
                        // mob vs pet or pet vs mob or non-party member vs mob or vice versa
                        combatDetails.ActorEntityType = MessageManager.Instance.LookupPetEntity(combatDetails.ActorName);
                        if (combatDetails.ActorEntityType == EntityType.Unknown)
                            combatDetails.ActorEntityType = DetermineIfMobOrPet(combatDetails.ActorName);
                        break;
                    default:
                        // other??
                        combatDetails.ActorEntityType = MessageManager.Instance.LookupEntity(combatDetails.ActorName);
                        break;
                }

                // Make corrections for certain special actions that are really enfeebles
                if (combatDetails.ActionName == "Charm")
                    combatDetails.AttackType = AttackType.Enfeeble;

                return combatMatch.Success;
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
                target = combatDetails.Targets.Add(combatMatch.Groups["target"].Value);
                target.Defended = true;
                target.DefenseType = defType;
                target.SuccessLevel = SuccessType.Unsuccessful;

                switch (defType)
                {
                    case DefenseType.Evasion:
                    case DefenseType.Parry:
                    case DefenseType.Anticipate:
                        combatDetails.ActorName = combatMatch.Groups["name"].Value;
                        break;
                }

                if (defType == DefenseType.Blink)
                    target.ShadowsUsed = byte.Parse(combatMatch.Groups["number"].Value);

                // Determine the two entity types involved.
                switch (messageCode)
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
                        // mob vs pet or pet vs mob or non-party member vs mob or vice versa
                        combatDetails.ActorEntityType = MessageManager.Instance.LookupPetEntity(combatDetails.ActorName);
                        if (combatDetails.ActorEntityType == EntityType.Unknown)
                        {
                            combatDetails.ActorEntityType = DetermineIfMobOrPet(combatDetails.ActorName);
                        }

                        target.EntityType = MessageManager.Instance.LookupEntity(target.Name);
                        if (target.EntityType == EntityType.Unknown)
                        {
                            target.EntityType = DetermineIfMobOrPet(target.Name);
                        }

                        if (combatDetails.ActorEntityType == EntityType.Unknown)
                        {
                            if ((target.EntityType == EntityType.Pet) || (target.EntityType == EntityType.Player))
                            {
                                combatDetails.ActorEntityType = EntityType.Mob;
                            }
                            else if (target.EntityType == EntityType.Mob)
                            {
                                combatDetails.ActorEntityType = EntityType.Player;
                            }
                        }

                        if (target.EntityType == EntityType.Unknown)
                        {
                            if ((combatDetails.ActorEntityType == EntityType.Pet) || 
                                (combatDetails.ActorEntityType == EntityType.Player))
                            {
                                target.EntityType = EntityType.Mob;
                            }
                            else if (combatDetails.ActorEntityType == EntityType.Mob)
                            {
                                target.EntityType = EntityType.Player;
                            }
                        }


                        if (combatDetails.ActorEntityType == target.EntityType)
                            combatDetails.ActorEntityType = MessageManager.Instance.LookupPetEntity(combatDetails.ActorName);
                        if (combatDetails.ActorEntityType == target.EntityType)
                            target.EntityType = MessageManager.Instance.LookupPetEntity(target.Name);
                        break;
                    default:
                        // other??
                        combatDetails.ActorEntityType = MessageManager.Instance.LookupEntity(combatDetails.ActorName);
                        target.EntityType = MessageManager.Instance.LookupEntity(target.Name);
                        break;
                }
            }

            return combatMatch.Success;

        }

        private void UpdateCombat(MessageLine msg)
        {
            if (parseSuccessful == true)
            {
                // If parsing the previous message was successful, this is additional
                // information (eg: crit hit damage, add. effect damage, etc).
                // Send this directly back through the basic parsing structure.

                currentMessageText = msg.TextOutput;
                ParseCombat();
            }
            else
            {
                // If parsing the previous message was not successful, the result was
                // broken up across multiple lines.  Combine this line with the previous
                // and try re-parsing.

                currentMessageText = currentMessageText + " " + msg.TextOutput;
                ParseCombat();
            }
        }
        #endregion

        #region Helper Functions
        /// <summary>
        /// Function to determine if a mob-type entity is an actual mob or a pet.
        /// </summary>
        /// <param name="name">The mob name to check.</param>
        /// <returns>Returns the entity type determination.</returns>
        private EntityType DetermineIfMobOrPet(string name)
        {
            if ((name == null) || (name == string.Empty))
                return EntityType.Unknown;

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

            // Check the currentMessageText before the specified name to see if the
            // word "the" is used.  If so, it's a mob.
            int indLoc = currentMessageText.IndexOf(name);
            if ((indLoc == 4) && (currentMessageText.StartsWith("The") == true))
                return EntityType.Mob;

            if (indLoc > 4)
            {
                if (currentMessageText.Substring(indLoc - 4).StartsWith("the") == true)
                    return EntityType.Mob;
            }

            if (Avatars.NamesList.Contains(name))
                return EntityType.Pet;

            if (Wyverns.NamesList.Contains(name))
                return EntityType.Pet;

            if (Puppets.NamesList.Contains(name))
                return EntityType.Pet;

            return EntityType.Unknown;
        }
        #endregion

        #region Properties for parsed elements
        /// <summary>
        /// Gets whether this message was successfully parsed.  If it was not,
        /// then attempting to get details is pointless.
        /// </summary>
        internal bool ParseSuccessful
        {
            get
            {
                return parseSuccessful;
            }
        }

        /// <summary>
        /// Gets the name of the speaker for a chat message.
        /// </summary>
        internal string ChatSpeakerName
        {
            get
            {
                return chatSpeakerName;
            }
        }

        /// <summary>
        /// Gets the type of speaker for a chat message.
        /// </summary>
        internal SpeakerType ChatSpeakerType
        {
            get
            {
                return chatSpeakerType;
            }
        }

        /// <summary>
        /// Gets the class containing specific combat details.
        /// </summary>
        internal CombatDetails CombatDetails
        {
            get
            {
                return combatDetails;
            }
        }

        /// <summary>
        /// Gets the class containing specific loot details.
        /// </summary>
        internal LootDetails LootDetails
        {
            get
            {
                return lootDetails;
            }
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Message Code: {0:x}\n", messageCode);

            sb.AppendFormat("Message Category: {0}\n", messageCategory);
            sb.AppendFormat("System Message Type: {0}\n", systemMessageType);
            sb.AppendFormat("Action Message Type: {0}\n", actionMessageType);
            sb.AppendFormat("Chat Type: {0}\n", chatType);
            if (combatDetails != null)
                sb.AppendFormat("Combat Details:\n{0}", combatDetails.ToString());
            if (lootDetails != null)
                sb.AppendFormat("Loot Details:\n{0}", lootDetails.ToString());

            return sb.ToString();
        }
        #endregion

        #region Dump
        internal string Dump()
        {
            StringBuilder dumpStr = new StringBuilder();

            dumpStr.AppendLine("----------------");
            dumpStr.AppendFormat("Message ID: {0}  @ {1}\n", messageID, this.Timestamp);

            foreach (MessageLine msgLine in msgLineCollection)
            {
                dumpStr.AppendFormat("Raw chatline data: {0}\n", msgLine.OriginalText);
            }

            dumpStr.AppendFormat("Category: {0}\n", messageCategory);

            dumpStr.AppendFormat("Parse successful? {0}\n", parseSuccessful);

            if (parseSuccessful == false)
                dumpStr.AppendFormat("[\n{0}]\n", this.ToString());

            if (parseSuccessful == true)
            {
                switch (messageCategory)
                {
                    case MessageCategoryType.System:
                        dumpStr.AppendFormat("System Type: {0}\n", systemMessageType);
                        break;
                    case MessageCategoryType.Chat:
                        dumpStr.AppendFormat("Chat type: {0}\n", chatType);
                        dumpStr.AppendFormat("Speaker: {0}\n", chatSpeakerName);
                        break;
                    case MessageCategoryType.Action:
                        dumpStr.AppendFormat("Action messsage type: {0}\n", actionMessageType);
                        switch (actionMessageType)
                        {
                            case ActionMessageType.Loot:
                                if (lootDetails.IsFoundMessage)
                                {
                                    dumpStr.AppendFormat("Found: {0}\n", lootDetails.ItemName);
                                    dumpStr.AppendFormat("On mob: {0}\n", lootDetails.MobName);
                                }
                                else
                                {
                                    if (lootDetails.WasLost == false)
                                    {
                                        dumpStr.AppendFormat("Player: {0}\n", lootDetails.WhoObtained);
                                        if (lootDetails.ItemName != null)
                                            dumpStr.AppendFormat("Received: {0}\n", lootDetails.ItemName);
                                        else
                                            dumpStr.AppendFormat("Received money: {0} gil\n", lootDetails.Gil);
                                    }
                                    else
                                    {
                                        dumpStr.AppendFormat("Item lost: {0}\n", lootDetails.ItemName);
                                    }
                                }
                                break;
                            case ActionMessageType.Combat:
                                dumpStr.AppendFormat("Combat Category: {0}\n", combatDetails.CombatCategory);
                                switch (combatDetails.CombatCategory)
                                {
                                    case CombatActionType.Attack:
                                        dumpStr.AppendFormat("Attack Type: {0}\n", combatDetails.AttackType);
                                        dumpStr.AppendFormat("Action source: {0}\n", combatDetails.ActionSource);

                                        if (combatDetails.IsPreparing == true)
                                        {
                                            dumpStr.AppendFormat("Attacker ({1}): {0}\n", combatDetails.ActorName, combatDetails.ActorEntityType);
                                            dumpStr.AppendFormat("Preparing: {0}\n", combatDetails.ActionName);
                                        }
                                        else
                                        {
                                            if (combatDetails.ActorName != string.Empty)
                                                dumpStr.AppendFormat("Attacker ({1}): {0}\n", combatDetails.ActorName, combatDetails.ActorEntityType);

                                            if (combatDetails.SuccessLevel == SuccessType.Failed)
                                            {
                                                if (combatDetails.ActorName != string.Empty)
                                                    dumpStr.AppendFormat("{0} failed to act: {1}\n", combatDetails.ActorName, combatDetails.FailedActionType);
                                                else
                                                {
                                                    if (combatDetails.Targets.Count > 0)
                                                    {
                                                        foreach (TargetDetails target in combatDetails.Targets)
                                                            dumpStr.AppendFormat("User failed to act on {0}: {1}\n", target.Name, combatDetails.FailedActionType);
                                                    }
                                                    else
                                                    {
                                                        dumpStr.AppendFormat("User failed to act: {0}\n", combatDetails.FailedActionType);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                switch (combatDetails.AttackType)
                                                {
                                                    case AttackType.Damage:
                                                        if (combatDetails.ActionName != string.Empty)
                                                            dumpStr.AppendFormat("{0} Name: {1}\n", combatDetails.ActionSource, combatDetails.ActionName);

                                                        foreach (TargetDetails target in combatDetails.Targets)
                                                        {
                                                            dumpStr.AppendFormat("Target ({1}): {0}\n", target.Name, target.EntityType);
                                                            dumpStr.AppendFormat("Success level: {0}\n", target.SuccessLevel);

                                                            if (target.Defended == true)
                                                            {
                                                                dumpStr.AppendFormat("Defense Type: {0}\n", target.DefenseType);
                                                                if (target.DefenseType == DefenseType.Blink)
                                                                    dumpStr.AppendFormat("Shadows lost: {0}\n", target.ShadowsUsed);
                                                            }
                                                            else
                                                            {
                                                                dumpStr.AppendFormat("Damage: {0}\n", target.Amount);
                                                                dumpStr.AppendFormat("Damage Modifier: {0}\n", target.DamageModifier);

                                                                if (target.AdditionalEffect == true)
                                                                    dumpStr.AppendFormat("Additional effect damage: {0}\n", target.AdditionalDamage);
                                                            }
                                                        }
                                                        break;
                                                    case AttackType.Enfeeble:
                                                        if (combatDetails.ActionName != string.Empty)
                                                            dumpStr.AppendFormat("{0} Name: {1}\n", combatDetails.ActionSource, combatDetails.ActionName);

                                                        foreach (TargetDetails target in combatDetails.Targets)
                                                        {
                                                            dumpStr.AppendFormat("Target ({1}): {0}\n", target.Name, target.EntityType);
                                                            dumpStr.AppendFormat("Success level: {0}\n", target.SuccessLevel);

                                                            if (target.Defended == true)
                                                            {
                                                                dumpStr.AppendFormat("Defense Type: {0}\n", target.DefenseType);
                                                                if (target.DefenseType == DefenseType.Blink)
                                                                    dumpStr.AppendFormat("Shadows lost: {0}\n", target.ShadowsUsed);
                                                            }
                                                        }
                                                        break;
                                                }
                                            }
                                        }
                                        break;
                                    case CombatActionType.Buff:
                                        dumpStr.AppendFormat("Action source: {0}\n", combatDetails.ActionSource);
                                        dumpStr.AppendFormat("Buff type: {0}\n", combatDetails.BuffType);
                                        if (combatDetails.IsPreparing == true)
                                        {
                                            dumpStr.AppendFormat("User: {0}\n", combatDetails.ActorName);
                                            dumpStr.AppendFormat("Preparing: {0}\n", combatDetails.ActionName);
                                        }
                                        else
                                        {
                                            if (combatDetails.SuccessLevel == SuccessType.Failed)
                                            {
                                                if (combatDetails.ActorName != string.Empty)
                                                    dumpStr.AppendFormat("{0} failed to act: {1}\n", combatDetails.ActorName, combatDetails.FailedActionType);
                                                else
                                                {
                                                    if (combatDetails.Targets.Count > 0)
                                                    {
                                                        foreach (TargetDetails target in combatDetails.Targets)
                                                            dumpStr.AppendFormat("Failed to act on {0}: {1}\n", target.Name, combatDetails.FailedActionType);
                                                    }
                                                    else
                                                    {
                                                        dumpStr.AppendFormat("User failed to act: {0}\n", combatDetails.FailedActionType);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                string recoveryStr = "";

                                                switch (combatDetails.BuffType)
                                                {
                                                    case BuffType.Enhance:
                                                        dumpStr.AppendFormat("{0} used {1}.\n", combatDetails.ActorName, combatDetails.ActionName);
                                                        foreach (TargetDetails target in combatDetails.Targets)
                                                        {
                                                            if (target.SuccessLevel == SuccessType.Failed)
                                                            {
                                                                dumpStr.AppendFormat("{0} failed to gain {1} effect.\n", target.Name, combatDetails.ActionName);
                                                            }
                                                            else
                                                            {
                                                                dumpStr.AppendFormat("{0} gained {1} effect.\n", target.Name, combatDetails.ActionName);
                                                            }
                                                        }
                                                        break;
                                                    case BuffType.Recovery:
                                                        dumpStr.AppendFormat("{0} used {1}.\n", combatDetails.ActorName, combatDetails.ActionName);
                                                        foreach (TargetDetails target in combatDetails.Targets)
                                                        {
                                                            if (target.RecoveryType == RecoveryType.RecoverMP)
                                                                recoveryStr = "MP";
                                                            else
                                                                recoveryStr = "HP";

                                                            dumpStr.AppendFormat("{0} recovers {1} {2}.\n", target.Name, target.Amount, recoveryStr);
                                                        }
                                                        break;
                                                }
                                            }
                                        }
                                        break;
                                    case CombatActionType.Death:
                                        foreach (TargetDetails target in combatDetails.Targets)
                                            dumpStr.AppendFormat("{0} died: {1}\n", target.EntityType, target.Name);
                                        if (combatDetails.ActorName != string.Empty)
                                            dumpStr.AppendFormat("Killed by: {0} (a {1})\n", combatDetails.ActorName, combatDetails.ActorEntityType);
                                        break;
                                    case CombatActionType.Unknown:
                                        if (combatDetails.IsPreparing == true)
                                        {
                                            dumpStr.AppendFormat("User: {0}\n", combatDetails.ActorName);
                                            dumpStr.AppendFormat("Preparing: {0}\n", combatDetails.ActionName);
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                }
            }

            return dumpStr.ToString();
        }
        #endregion
    }
}
