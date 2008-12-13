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
    [TestFixture]
    public class TestParser
    {
        [Test]
        public void TestSystemChatLine()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        #region Mob Names
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
        [Ignore]
        public void TestMobMissParty()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
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
        [Ignore]
        public void TestPartyRMissMob()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
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
        [Ignore]
        public void TestMobBlink()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
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
        [Ignore]
        public void TestPlayerCounter()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
        }

        [Test]
        [Ignore]
        public void TestMobCounter()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
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
        [Ignore]
        public void TestPlayerRetaliateHit()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";
            ChatLine chatLine = new ChatLine(chatText);
            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = Parser.Parse(msgLine);

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
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
        #endregion

        #region Experience
        #endregion

        #region Loot
        #endregion
    }
}
