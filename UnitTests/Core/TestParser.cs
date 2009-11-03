using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using WaywardGamers.KParser.Parsing;

namespace WaywardGamers.KParser
{
    // There is only one non-private function visible on the Parser class: Parse(MessageLine msgLine).
    // This test class must exersise all possible code paths through that single input point to
    // verify validity.
    // Note: This set of tests is for messages in the English client of FFXI.  The message lines will need
    // to be rewritten/alt-tested for other cultures (eg: French or German).
    [TestFixture]
    [Culture("en")]
    public class TestParser
    {
        #region Chat
        [Test]
        public void TestSystemChatLine()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
            Assert.That(msg.ChatDetails, Is.Not.Null);
            Assert.That(msg.ChatDetails.ChatSpeakerType, Is.EqualTo(SpeakerType.NPC));
        }
        #endregion

        #region Mob Names
        [Test]
        public void TestMobNames01()
        {
            string chatText = "1c,cf,97,80a04040,000026b9,00002cbe,003e,00,01,02,00,The Mandragora hits Motenten for 170 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Mandragora"));
        }

        [Test]
        public void TestMobNames02()
        {
            string chatText = "1c,cf,97,80a04040,000026b9,00002cbe,003e,00,01,02,00,The Greater Colibri hits Motenten for 170 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Greater Colibri"));
        }

        [Test]
        public void TestMobNames03()
        {
            string chatText = "1c,cf,97,80a04040,000026b9,00002cbe,003e,00,01,02,00,Lamia No.9 hits Motenten for 170 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Lamia No.9"));
        }

        [Test]
        public void TestMobNames04()
        {
            string chatText = "1c,cf,97,80a04040,000026b9,00002cbe,003e,00,01,02,00,Moo Ouzi the Swiftblade hits Motenten for 170 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Moo Ouzi the Swiftblade"));
        }

        [Test]
        public void TestMobNames05()
        {
            string chatText = "1c,cf,97,80a04040,000026b9,00002cbe,003e,00,01,02,00,Ga'Dho Softstep hits Motenten for 170 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Ga'Dho Softstep"));
        }

        [Test]
        public void TestMobNames06()
        {
            string chatText = "1c,cf,97,80a04040,000026b9,00002cbe,003e,00,01,02,00,Maymun 53 hits Motenten for 170 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Maymun 53"));
        }

        [Test]
        public void TestMobNames07()
        {
            string chatText = "1c,cf,97,80a04040,000026b9,00002cbe,003e,00,01,02,00,Fantoccini hits Motenten for 170 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Pet));
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Fantoccini"));
        }

        [Test]
        public void TestMobNames08()
        {
            string chatText = "1c,cf,97,80a04040,000026b9,00002cbe,003e,00,01,02,00,Tzee Xicu's Elemental hits Motenten for 170 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Tzee Xicu's Elemental"));
        }

        [Test]
        public void TestMobNames09()
        {
            string chatText = "1c,cf,97,80a04040,000026b9,00002cbe,003e,00,01,02,00,The Mamool Ja's Wyvern hits Motenten for 170 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Mamool Ja's Wyvern"));
        }

        [Test]
        public void TestMobNames10()
        {
            string chatText = "1c,cf,97,80a04040,000026b9,00002cbe,003e,00,01,02,00,The Lamia's Elemental hits Motenten for 170 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Lamia's Elemental"));
        }
        #endregion

        #region Combat parsing - Melee hits
        [Test]
        public void TestPlayerHitMob()
        {
            string chatText = "14,7a,98,80c08080,0000268b,00002c8b,003e,00,01,02,00,Motenten hits the Greater Colibri for 128 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Melee));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Harm));
            Assert.That(msg.EventDetails.CombatDetails.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Motenten"));
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Player));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Successful));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Empty);
            TargetDetails target = msg.EventDetails.CombatDetails.Targets.First();
            Assert.That(target.EntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(target.Name, Is.EqualTo("Greater Colibri"));
            Assert.That(target.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(target.DefenseType, Is.EqualTo(DefenseType.None));
            Assert.That(target.Amount, Is.EqualTo(128));
            Assert.That(target.DamageModifier, Is.EqualTo(DamageModifier.None));
            Assert.That(target.ShadowsUsed, Is.EqualTo(0));
        }

        [Test]
        public void TestMobHitPlayer()
        {
            string chatText = "1c,cf,97,80a04040,000026b9,00002cbe,003e,00,01,02,00,The Greater Colibri hits Motenten for 170 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Melee));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Harm));
            Assert.That(msg.EventDetails.CombatDetails.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Greater Colibri"));
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Successful));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Empty);
            TargetDetails target = msg.EventDetails.CombatDetails.Targets.First();
            Assert.That(target.EntityType, Is.EqualTo(EntityType.Player));
            Assert.That(target.Name, Is.EqualTo("Motenten"));
            Assert.That(target.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(target.DefenseType, Is.EqualTo(DefenseType.None));
            Assert.That(target.Amount, Is.EqualTo(170));
            Assert.That(target.DamageModifier, Is.EqualTo(DamageModifier.None));
            Assert.That(target.ShadowsUsed, Is.EqualTo(0));
        }

        [Test]
        public void TestPartyHitMob()
        {
            string chatText = "19,99,9a,80c08080,000026ca,00002cd3,003a,00,01,02,00,Lans hits the Greater Colibri for 168 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Melee));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Harm));
            Assert.That(msg.EventDetails.CombatDetails.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Lans"));
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Player));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Successful));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Empty);
            TargetDetails target = msg.EventDetails.CombatDetails.Targets.First();
            Assert.That(target.EntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(target.Name, Is.EqualTo("Greater Colibri"));
            Assert.That(target.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(target.DefenseType, Is.EqualTo(DefenseType.None));
            Assert.That(target.Amount, Is.EqualTo(168));
            Assert.That(target.DamageModifier, Is.EqualTo(DamageModifier.None));
            Assert.That(target.ShadowsUsed, Is.EqualTo(0));
        }

        [Test]
        public void TestMobHitParty()
        {
            string chatText = "20,2d,99,80c08080,000027db,00002e21,003a,00,01,02,00,The Greater Colibri hits Lans for 160 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Melee));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Harm));
            Assert.That(msg.EventDetails.CombatDetails.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Greater Colibri"));
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Successful));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Empty);
            TargetDetails target = msg.EventDetails.CombatDetails.Targets.First();
            Assert.That(target.EntityType, Is.EqualTo(EntityType.Player));
            Assert.That(target.Name, Is.EqualTo("Lans"));
            Assert.That(target.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(target.DefenseType, Is.EqualTo(DefenseType.None));
            Assert.That(target.Amount, Is.EqualTo(160));
            Assert.That(target.DamageModifier, Is.EqualTo(DamageModifier.None));
            Assert.That(target.ShadowsUsed, Is.EqualTo(0));
        }

        [Test]
        [Ignore]
        public void TestAllyHitMob()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        [Ignore]
        public void TestMobHitAlly()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        [Ignore]
        public void TestOtherHitMob()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }
        [Test]
        [Ignore]
        public void TestMobHitOther()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }
        #endregion

        #region Combat parsing - Range hits
        [Test]
        [Ignore]
        public void TestPlayerRHitMob()
        {
            string chatText = "19,e4,98,80c08080,000027e1,00002e27,004c,00,01,02,00,Motenten's ranged attack hits the Greater Colibri for 247 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Ranged));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Harm));
            Assert.That(msg.EventDetails.CombatDetails.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Motenten"));
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Player));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Successful));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Empty);
            TargetDetails target = msg.EventDetails.CombatDetails.Targets.First();
            Assert.That(target.EntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(target.Name, Is.EqualTo("Greater Colibri"));
            Assert.That(target.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(target.DefenseType, Is.EqualTo(DefenseType.None));
            Assert.That(target.Amount, Is.EqualTo(247));
            Assert.That(target.DamageModifier, Is.EqualTo(DamageModifier.None));
            Assert.That(target.ShadowsUsed, Is.EqualTo(0));
        }

        [Test]
        [Ignore]
        public void TestMobRHitPlayer()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        public void TestPartyRHitMob()
        {
            string chatText = "19,e4,98,80c08080,000027e1,00002e27,004c,00,01,02,00,Idelle's ranged attack hits the Greater Colibri for 247 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Ranged));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Harm));
            Assert.That(msg.EventDetails.CombatDetails.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Idelle"));
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Player));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Successful));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Empty);
            TargetDetails target = msg.EventDetails.CombatDetails.Targets.First();
            Assert.That(target.EntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(target.Name, Is.EqualTo("Greater Colibri"));
            Assert.That(target.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(target.DefenseType, Is.EqualTo(DefenseType.None));
            Assert.That(target.Amount, Is.EqualTo(247));
            Assert.That(target.DamageModifier, Is.EqualTo(DamageModifier.None));
            Assert.That(target.ShadowsUsed, Is.EqualTo(0));
        }

        [Test]
        [Ignore]
        public void TestMobRHitParty()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        [Ignore]
        public void TestAllyRHitMob()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        [Ignore]
        public void TestMobRHitAlly()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        [Ignore]
        public void TestOtherRHitMob()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        [Ignore]
        public void TestMobRHitOther()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }
        #endregion

        #region Combat parsing - Melee Miss
        [Test]
        public void TestPlayerMissMob()
        {
            string chatText = "15,28,99,80707070,000027ef,00002e39,0027,00,01,02,00,Motenten misses the Greater Colibri.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Melee));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Harm));
            Assert.That(msg.EventDetails.CombatDetails.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Motenten"));
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Player));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Unsuccessful));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Empty);
            TargetDetails target = msg.EventDetails.CombatDetails.Targets.First();
            Assert.That(target.EntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(target.Name, Is.EqualTo("Greater Colibri"));
            Assert.That(target.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(target.DefenseType, Is.EqualTo(DefenseType.Evasion));
            Assert.That(target.Amount, Is.EqualTo(0));
            Assert.That(target.DamageModifier, Is.EqualTo(DamageModifier.None));
            Assert.That(target.ShadowsUsed, Is.EqualTo(0));
        }

        [Test]
        public void TestMobMissPlayer()
        {
            string chatText = "1d,90,98,80742cd4,00002984,0000301f,0027,00,01,02,00,The Greater Colibri misses Motenten.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Melee));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Harm));
            Assert.That(msg.EventDetails.CombatDetails.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Greater Colibri"));
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Unsuccessful));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Empty);
            TargetDetails target = msg.EventDetails.CombatDetails.Targets.First();
            Assert.That(target.EntityType, Is.EqualTo(EntityType.Player));
            Assert.That(target.Name, Is.EqualTo("Motenten"));
            Assert.That(target.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(target.DefenseType, Is.EqualTo(DefenseType.Evasion));
            Assert.That(target.Amount, Is.EqualTo(0));
            Assert.That(target.DamageModifier, Is.EqualTo(DamageModifier.None));
            Assert.That(target.ShadowsUsed, Is.EqualTo(0));
        }

        [Test]
        public void TestPartyMissMob()
        {
            string chatText = "1a,fd,98,80707070,000027f4,00002e3f,0023,00,01,02,00,Lans misses the Greater Colibri.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Melee));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Harm));
            Assert.That(msg.EventDetails.CombatDetails.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Lans"));
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Player));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Unsuccessful));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Empty);
            TargetDetails target = msg.EventDetails.CombatDetails.Targets.First();
            Assert.That(target.EntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(target.Name, Is.EqualTo("Greater Colibri"));
            Assert.That(target.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(target.DefenseType, Is.EqualTo(DefenseType.Evasion));
            Assert.That(target.Amount, Is.EqualTo(0));
            Assert.That(target.DamageModifier, Is.EqualTo(DamageModifier.None));
            Assert.That(target.ShadowsUsed, Is.EqualTo(0));
        }

        [Test]
        public void TestMobMissParty()
        {
            string chatText = "21,85,95,80707070,00001880,00001c01,0026,00,01,02,00,The Hilltroll Warrior misses Midas.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Melee));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Harm));
            Assert.That(msg.EventDetails.CombatDetails.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Hilltroll Warrior"));
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Unsuccessful));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Empty);
            TargetDetails target = msg.EventDetails.CombatDetails.Targets.First();
            Assert.That(target.EntityType, Is.EqualTo(EntityType.Player));
            Assert.That(target.Name, Is.EqualTo("Midas"));
            Assert.That(target.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(target.DefenseType, Is.EqualTo(DefenseType.Evasion));
            Assert.That(target.Amount, Is.EqualTo(0));
            Assert.That(target.DamageModifier, Is.EqualTo(DamageModifier.None));
            Assert.That(target.ShadowsUsed, Is.EqualTo(0));
        }

        [Test]
        [Ignore]
        public void TestAllyMissMob()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        [Ignore]
        public void TestMobMissAlly()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        [Ignore]
        public void TestOtherMissMob()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        [Ignore]
        public void TestMobMissOther()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }
        #endregion

        #region Combat parsing - Range Misss
        [Test]
        public void TestPlayerRMissMob()
        {
            string chatText = "15,86,95,80707070,00001021,0000125c,0023,00,01,02,00,Motenten's ranged attack misses.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Ranged));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Harm));
            Assert.That(msg.EventDetails.CombatDetails.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Motenten"));
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Player));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Unsuccessful));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
            //Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Empty);
        }

        [Test]
        [Ignore]
        public void TestMobRMissPlayer()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        public void TestPartyRMissMob()
        {
            string chatText = "1a,5c,95,80707070,00003e32,00004900,0021,00,01,02,00,Idelle's ranged attack misses.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Ranged));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Harm));
            Assert.That(msg.EventDetails.CombatDetails.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Idelle"));
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Player));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Unsuccessful));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
        }

        [Test]
        [Ignore]
        public void TestMobRMissParty()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        [Ignore]
        public void TestAllyRMissMob()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        [Ignore]
        public void TestMobRMissAlly()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        [Ignore]
        public void TestOtherRMissMob()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        [Ignore]
        public void TestMobRMissOther()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }
        #endregion

        #region Combat parsing - Blocks
        [Test]
        public void TestPlayerBlink()
        {
            string chatText = "1d,c8,e4,80742cd4,00002753,00002d7b,003d,00,01,02,00,1 of Motenten's shadows absorbs the damage and disappears.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Unknown));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Harm));
            Assert.That(msg.EventDetails.CombatDetails.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.Empty);
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Unknown));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Unsuccessful));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Empty);
            TargetDetails target = msg.EventDetails.CombatDetails.Targets.First();
            Assert.That(target.EntityType, Is.EqualTo(EntityType.Player));
            Assert.That(target.Name, Is.EqualTo("Motenten"));
            Assert.That(target.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(target.DefenseType, Is.EqualTo(DefenseType.Shadow));
            Assert.That(target.Amount, Is.EqualTo(0));
            Assert.That(target.DamageModifier, Is.EqualTo(DamageModifier.None));
            Assert.That(target.ShadowsUsed, Is.EqualTo(1));
        }

        [Test]
        public void TestMobBlink()
        {
            string chatText = "1a,ef,91,80707070,00001487,00001772,004b,00,01,02,00,1 of the Hilltroll Red Mage's shadows absorbs the damage and disappears.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Unknown));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Harm));
            Assert.That(msg.EventDetails.CombatDetails.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.Empty);
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Unknown));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Unsuccessful));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Empty);
            TargetDetails target = msg.EventDetails.CombatDetails.Targets.First();
            Assert.That(target.EntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(target.Name, Is.EqualTo("Hilltroll Red Mage"));
            Assert.That(target.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(target.DefenseType, Is.EqualTo(DefenseType.Shadow));
            Assert.That(target.Amount, Is.EqualTo(0));
            Assert.That(target.DamageModifier, Is.EqualTo(DamageModifier.None));
            Assert.That(target.ShadowsUsed, Is.EqualTo(1));
        }

        [Test]
        public void TestPlayerParry()
        {
            string chatText = "1d,4a,98,80742cd4,00002a98,00003169,0041,00,01,02,00,Motenten parries the Greater Colibri's attack with her weapon.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Unknown));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Harm));
            Assert.That(msg.EventDetails.CombatDetails.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Greater Colibri"));
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Unsuccessful));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Empty);
            TargetDetails target = msg.EventDetails.CombatDetails.Targets.First();
            Assert.That(target.EntityType, Is.EqualTo(EntityType.Player));
            Assert.That(target.Name, Is.EqualTo("Motenten"));
            Assert.That(target.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(target.DefenseType, Is.EqualTo(DefenseType.Parry));
            Assert.That(target.Amount, Is.EqualTo(0));
            Assert.That(target.DamageModifier, Is.EqualTo(DamageModifier.None));
            Assert.That(target.ShadowsUsed, Is.EqualTo(0));
        }

        [Test]
        [Ignore]
        public void TestMobParry()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        public void TestPlayerCounter()
        {
            string chatText = "14,ac,9d,80c08080,00001652,00001987,006c,00,01,02,00,The Hilltroll Paladin's attack is countered by Motenten. The Hilltroll Paladin takes 56 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Counterattack));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Harm));
            Assert.That(msg.EventDetails.CombatDetails.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Motenten"));
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Player));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Successful));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Empty);
            TargetDetails target = msg.EventDetails.CombatDetails.Targets.First();
            Assert.That(target.EntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(target.Name, Is.EqualTo("Hilltroll Paladin"));
            Assert.That(target.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(target.DefenseType, Is.EqualTo(DefenseType.None));
            Assert.That(target.Amount, Is.EqualTo(56));
            Assert.That(target.DamageModifier, Is.EqualTo(DamageModifier.None));
            Assert.That(target.ShadowsUsed, Is.EqualTo(0));
        }

        [Test]
        public void TestPartyCounter()
        {
            string chatText = "19,00,00,80c08080,00002ea7,0000359e,006e,00,01,02,00,The Hilltroll Paladin's attack is countered by Devilpitti. The Hilltroll Paladin takes 27 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Counterattack));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Harm));
            Assert.That(msg.EventDetails.CombatDetails.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Devilpitti"));
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Player));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Successful));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Empty);
            TargetDetails target = msg.EventDetails.CombatDetails.Targets.First();
            Assert.That(target.EntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(target.Name, Is.EqualTo("Hilltroll Paladin"));
            Assert.That(target.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(target.DefenseType, Is.EqualTo(DefenseType.None));
            Assert.That(target.Amount, Is.EqualTo(27));
            Assert.That(target.DamageModifier, Is.EqualTo(DamageModifier.None));
            Assert.That(target.ShadowsUsed, Is.EqualTo(0));
        }

        [Test]
        public void TestMobCounterPlayer()
        {
            string chatText = "1c,ac,9d,80a04040,00001141,000013ac,005d,00,01,02,00,Motenten's attack is countered by the Hilltroll Monk. Motenten takes 148 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Counterattack));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Harm));
            Assert.That(msg.EventDetails.CombatDetails.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Hilltroll Monk"));
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Successful));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Empty);
            TargetDetails target = msg.EventDetails.CombatDetails.Targets.First();
            Assert.That(target.EntityType, Is.EqualTo(EntityType.Player));
            Assert.That(target.Name, Is.EqualTo("Motenten"));
            Assert.That(target.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(target.DefenseType, Is.EqualTo(DefenseType.None));
            Assert.That(target.Amount, Is.EqualTo(148));
            Assert.That(target.DamageModifier, Is.EqualTo(DamageModifier.None));
            Assert.That(target.ShadowsUsed, Is.EqualTo(0));
        }

        [Test]
        public void TestMobCounterParty()
        {
            string chatText = "20,00,00,80c08080,000010df,00001338,0068,00,01,02,00,Disintegration's attack is countered by the Hilltroll Monk. Disintegration takes 71 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Counterattack));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Harm));
            Assert.That(msg.EventDetails.CombatDetails.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Hilltroll Monk"));
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Successful));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Empty);
            TargetDetails target = msg.EventDetails.CombatDetails.Targets.First();
            Assert.That(target.EntityType, Is.EqualTo(EntityType.Player));
            Assert.That(target.Name, Is.EqualTo("Disintegration"));
            Assert.That(target.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(target.DefenseType, Is.EqualTo(DefenseType.None));
            Assert.That(target.Amount, Is.EqualTo(71));
            Assert.That(target.DamageModifier, Is.EqualTo(DamageModifier.None));
            Assert.That(target.ShadowsUsed, Is.EqualTo(0));
        }

        [Test]
        [Ignore]
        public void TestMobCounterAlly()
        {
            string chatText = "20,00,00,80c08080,000010df,00001338,0068,00,01,02,00,Disintegration's attack is countered by the Hilltroll Monk. Disintegration takes 71 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        [Ignore]
        public void TestMobCounterOther()
        {
            string chatText = "20,00,00,80c08080,000010df,00001338,0068,00,01,02,00,Disintegration's attack is countered by the Hilltroll Monk. Disintegration takes 71 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        [Ignore]
        public void TestPlayerCounterBlink()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        [Ignore]
        public void TestMobCounterBlink()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        public void TestPlayerAnticipate()
        {
            string chatText = "1d,69,91,80742cd4,0000365c,00003e83,0023,00,01,02,00,Motenten anticipates the attack.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Unknown));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Harm));
            Assert.That(msg.EventDetails.CombatDetails.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.Empty);
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Unknown));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Unsuccessful));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Empty);
            TargetDetails target = msg.EventDetails.CombatDetails.Targets.First();
            Assert.That(target.EntityType, Is.EqualTo(EntityType.Player));
            Assert.That(target.Name, Is.EqualTo("Motenten"));
            Assert.That(target.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(target.DefenseType, Is.EqualTo(DefenseType.Anticipate));
            Assert.That(target.Amount, Is.EqualTo(0));
            Assert.That(target.DamageModifier, Is.EqualTo(DamageModifier.None));
            Assert.That(target.ShadowsUsed, Is.EqualTo(0));
        }

        [Test]
        [Ignore]
        public void TestPlayerRetaliateHit()
        {
            string chatText = "19,ac,9d,80c08080,00000961,00000a83,0045,00,01,02,00,Midas retaliates. The Hilltroll Paladin takes 52 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
        }

        [Test]
        public void TestPartyRetaliateHit()
        {
            string chatText = "19,ac,9d,80c08080,00000961,00000a83,0045,00,01,02,00,Midas retaliates. The Hilltroll Paladin takes 52 points of damage.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Retaliation));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Harm));
            Assert.That(msg.EventDetails.CombatDetails.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Midas"));
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Player));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Successful));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Empty);
            TargetDetails target = msg.EventDetails.CombatDetails.Targets.First();
            Assert.That(target.EntityType, Is.EqualTo(EntityType.Mob));
            Assert.That(target.Name, Is.EqualTo("Hilltroll Paladin"));
            Assert.That(target.HarmType, Is.EqualTo(HarmType.Damage));
            Assert.That(target.DefenseType, Is.EqualTo(DefenseType.None));
            Assert.That(target.Amount, Is.EqualTo(52));
            Assert.That(target.DamageModifier, Is.EqualTo(DamageModifier.None));
            Assert.That(target.ShadowsUsed, Is.EqualTo(0));
        }

        [Test]
        [Ignore]
        public void TestPlayerRetaliateMiss()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        [Ignore]
        public void TestPartyRetaliateMiss()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        [Ignore]
        public void TestPlayerRetaliateBlink()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        [Ignore]
        public void TestPlayerRetaliateCounter()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }
        #endregion

        #region Combat parsing - Cover
        #endregion

        #region Combat parsing - Magic damage
        #endregion

        #region Combat parsing - Additional effects
        #endregion

        #region Combat parsing - Skillchains
        #endregion

        #region Enfeebling
        #endregion

        #region Buffing
        #endregion

        #region Curing
        #endregion

        #region JAs
        #endregion

        #region Deaths
        #endregion

        #region Preparing Spells/JAs
        #endregion

        #region Drains
        #endregion

        #region Failures
        [Test]
        public void FailSelfBuff()
        {
            string chatText = "44,00,00,80a08040,00001ae1,00001fa1,002f,00,01,02,00,Aurun's Monomi: Ichi has no effect on Aurun.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Spell));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Aid));
            Assert.That(msg.EventDetails.CombatDetails.AidType, Is.EqualTo(AidType.Enhance));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Aurun"));
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Player));
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Spell));
            Assert.That(msg.EventDetails.CombatDetails.ActionName, Is.EqualTo("Monomi: Ichi"));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Failed));
            Assert.That(msg.EventDetails.CombatDetails.FailedActionType, Is.EqualTo(FailedActionType.NoEffect));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Empty);
            TargetDetails target = msg.EventDetails.CombatDetails.Targets.First();
            Assert.That(target.EntityType, Is.EqualTo(EntityType.Player));
            Assert.That(target.Name, Is.EqualTo("Aurun"));
            Assert.That(target.AidType, Is.EqualTo(AidType.Enhance));
            Assert.That(target.DefenseType, Is.EqualTo(DefenseType.None));
            Assert.That(target.FailedActionType, Is.EqualTo(FailedActionType.NoEffect));
            Assert.That(target.Amount, Is.EqualTo(0));
            Assert.That(target.DamageModifier, Is.EqualTo(DamageModifier.None));
            Assert.That(target.ShadowsUsed, Is.EqualTo(0));
        }

        [Test]
        public void FailRemoveStatus()
        {
            string chatText = "44,ee,e6,80a08040,00000ae0,00000cb0,002e,00,01,02,00,Starfall's Paralyna has no effect on Aurun.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);


            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Event));
            Assert.That(msg.EventDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.EventMessageType, Is.EqualTo(EventMessageType.Interaction));
            Assert.That(msg.EventDetails.CombatDetails, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Spell));
            Assert.That(msg.EventDetails.CombatDetails.InteractionType, Is.EqualTo(InteractionType.Aid));
            Assert.That((msg.EventDetails.CombatDetails.AidType & (AidType.Enhance | AidType.RemoveStatus)), Is.Not.EqualTo(AidType.None));
            Assert.That(msg.EventDetails.CombatDetails.HasActor, Is.True);
            Assert.That(msg.EventDetails.CombatDetails.ActorName, Is.EqualTo("Starfall"));
            Assert.That(msg.EventDetails.CombatDetails.ActorEntityType, Is.EqualTo(EntityType.Player));
            Assert.That(msg.EventDetails.CombatDetails.ActionType, Is.EqualTo(ActionType.Spell));
            Assert.That(msg.EventDetails.CombatDetails.ActionName, Is.EqualTo("Paralyna"));
            Assert.That(msg.EventDetails.CombatDetails.HasAdditionalEffect, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.IsPreparing, Is.False);
            Assert.That(msg.EventDetails.CombatDetails.SuccessLevel, Is.EqualTo(SuccessType.Failed));
            Assert.That(msg.EventDetails.CombatDetails.FailedActionType, Is.EqualTo(FailedActionType.NoEffect));
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Null);
            Assert.That(msg.EventDetails.CombatDetails.Targets, Is.Not.Empty);
            TargetDetails target = msg.EventDetails.CombatDetails.Targets.First();
            Assert.That(target.EntityType, Is.EqualTo(EntityType.Player));
            Assert.That(target.Name, Is.EqualTo("Aurun"));
            Assert.That((target.AidType & (AidType.Enhance | AidType.RemoveStatus)), Is.Not.EqualTo(AidType.None));
            Assert.That(target.DefenseType, Is.EqualTo(DefenseType.None));
            Assert.That(target.FailedActionType, Is.EqualTo(FailedActionType.NoEffect));
            Assert.That(target.Amount, Is.EqualTo(0));
            Assert.That(target.DamageModifier, Is.EqualTo(DamageModifier.None));
            Assert.That(target.ShadowsUsed, Is.EqualTo(0));
        }
        
        #endregion

        #region Experience
        #endregion

        #region Loot
        #endregion
    }
}
