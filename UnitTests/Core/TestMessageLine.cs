using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace WaywardGamers.KParser
{
    [TestFixture]
    public class TestMessageLine
    {
        [Test]
        public void TestNullConstruction()
        {
            Assert.Throws<ArgumentNullException>(delegate { new MessageLine(null); });
        }

        [Test]
        public void TestEmptyChat()
        {
            ChatLine chatLine = new ChatLine("");
            Assert.Throws<ArgumentException>(delegate { new MessageLine(chatLine); });
        }

        [Test]
        public void TestInvalidChat()
        {
            ChatLine chatLine = new ChatLine("°l");
            Assert.Throws<FormatException>(delegate { new MessageLine(chatLine); });
        }

        [Test]
        public void TestChatLineConstruction()
        {
            string chatText = "01,00,00,80808080,00000019,00000017,0010,00,01,01,00,Motenten : one";

            DateTime timestamp = DateTime.Now;
            ChatLine chatLine = new ChatLine(chatText, timestamp);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.OriginalText, Is.EqualTo(chatText));
            Assert.That(msgLine.Timestamp, Is.EqualTo(timestamp));
        }

        [Test]
        public void TestTokenizeBaseline()
        {
            string chatText = "01,00,00,80808080,00000019,00000017,0010,00,01,01,00,Motenten : one";

            DateTime timestamp = DateTime.Now;
            ChatLine chatLine = new ChatLine(chatText, timestamp);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.MessageCode,    Is.EqualTo(0x01));
            Assert.That(msgLine.ExtraCode1,     Is.EqualTo(0x00));
            Assert.That(msgLine.ExtraCode2,     Is.EqualTo(0x00));
            Assert.That(msgLine.TextColor,      Is.EqualTo(0x80808080));
            Assert.That(msgLine.EventSequence,  Is.EqualTo(0x00000019));
            Assert.That(msgLine.UniqueSequence, Is.EqualTo(0x00000017));
            Assert.That(msgLine.TextLength,     Is.EqualTo(0x0010));
            Assert.That(msgLine.MessageCategoryNumber, Is.EqualTo(0x01));
            Assert.That(msgLine.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
            Assert.That(msgLine.TextOutput, Is.EqualTo("Motenten : one"));
        }

        [Test]
        public void TestTokenizeJapaneseChars()
        {
            string chatText = "0d,00,00,80ff5000,000001fa,0000021b,001c,00,01,01,00,(Konychanz) HP‰©F‚¢‚¶‚á‚È‚¢‚©EEE‚—";

            DateTime timestamp = DateTime.Now;
            ChatLine chatLine = new ChatLine(chatText, timestamp);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.MessageCode, Is.EqualTo(0x0d));
            Assert.That(msgLine.ExtraCode1, Is.EqualTo(0x00));
            Assert.That(msgLine.ExtraCode2, Is.EqualTo(0x00));
            Assert.That(msgLine.TextColor, Is.EqualTo(0x80ff5000));
            Assert.That(msgLine.EventSequence, Is.EqualTo(0x000001fa));
            Assert.That(msgLine.UniqueSequence, Is.EqualTo(0x0000021b));
            Assert.That(msgLine.TextLength, Is.EqualTo(0x001c));
            Assert.That(msgLine.MessageCategoryNumber, Is.EqualTo(0x01));
            Assert.That(msgLine.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
            Assert.That(msgLine.TextOutput, Is.EqualTo("(Konychanz) HP黄色いじゃないか・・・ｗ"));
        }

        [Test]
        [Ignore]
        public void TestTokenizeFrenchChars()
        {
            string chatText = "01,00,00,80808080,00000019,00000017,0010,00,01,01,00,Motenten : one";

            DateTime timestamp = DateTime.Now;
            ChatLine chatLine = new ChatLine(chatText, timestamp);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.MessageCode, Is.EqualTo(0x01));
            Assert.That(msgLine.ExtraCode1, Is.EqualTo(0x00));
            Assert.That(msgLine.ExtraCode2, Is.EqualTo(0x00));
            Assert.That(msgLine.TextColor, Is.EqualTo(0x80808080));
            Assert.That(msgLine.EventSequence, Is.EqualTo(0x00000019));
            Assert.That(msgLine.UniqueSequence, Is.EqualTo(0x00000017));
            Assert.That(msgLine.TextLength, Is.EqualTo(0x0010));
            Assert.That(msgLine.MessageCategoryNumber, Is.EqualTo(0x01));
            Assert.That(msgLine.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
            Assert.That(msgLine.TextOutput, Is.EqualTo("Motenten : one"));
        }

        [Test]
        [Ignore]
        public void TestTokenizeTimestampPlugin()
        {
            string chatText = "01,00,00,80808080,00000019,00000017,0010,00,01,01,00,Motenten : one";

            DateTime timestamp = DateTime.Now;
            ChatLine chatLine = new ChatLine(chatText, timestamp);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.MessageCode, Is.EqualTo(0x01));
            Assert.That(msgLine.ExtraCode1, Is.EqualTo(0x00));
            Assert.That(msgLine.ExtraCode2, Is.EqualTo(0x00));
            Assert.That(msgLine.TextColor, Is.EqualTo(0x80808080));
            Assert.That(msgLine.EventSequence, Is.EqualTo(0x00000019));
            Assert.That(msgLine.UniqueSequence, Is.EqualTo(0x00000017));
            Assert.That(msgLine.TextLength, Is.EqualTo(0x0010));
            Assert.That(msgLine.MessageCategoryNumber, Is.EqualTo(0x01));
            Assert.That(msgLine.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
            Assert.That(msgLine.TextOutput, Is.EqualTo("Motenten : one"));
        }

        [Test]
        [Ignore]
        public void TestTokenizeError2()
        {
            string chatText = "01,00,00,80808080,00000019,00000017,0010,00,01,01,00,Motenten : one";

            DateTime timestamp = DateTime.Now;
            ChatLine chatLine = new ChatLine(chatText, timestamp);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.MessageCode, Is.EqualTo(0x01));
            Assert.That(msgLine.ExtraCode1, Is.EqualTo(0x00));
            Assert.That(msgLine.ExtraCode2, Is.EqualTo(0x00));
            Assert.That(msgLine.TextColor, Is.EqualTo(0x80808080));
            Assert.That(msgLine.EventSequence, Is.EqualTo(0x00000019));
            Assert.That(msgLine.UniqueSequence, Is.EqualTo(0x00000017));
            Assert.That(msgLine.TextLength, Is.EqualTo(0x0010));
            Assert.That(msgLine.MessageCategoryNumber, Is.EqualTo(0x01));
            Assert.That(msgLine.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
            Assert.That(msgLine.TextOutput, Is.EqualTo("Motenten : one"));
        }

        [Test]
        [Ignore]
        public void TestTokenizeError3()
        {
            string chatText = "01,00,00,80808080,00000019,00000017,0010,00,01,01,00,Motenten : one";

            DateTime timestamp = DateTime.Now;
            ChatLine chatLine = new ChatLine(chatText, timestamp);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.MessageCode, Is.EqualTo(0x01));
            Assert.That(msgLine.ExtraCode1, Is.EqualTo(0x00));
            Assert.That(msgLine.ExtraCode2, Is.EqualTo(0x00));
            Assert.That(msgLine.TextColor, Is.EqualTo(0x80808080));
            Assert.That(msgLine.EventSequence, Is.EqualTo(0x00000019));
            Assert.That(msgLine.UniqueSequence, Is.EqualTo(0x00000017));
            Assert.That(msgLine.TextLength, Is.EqualTo(0x0010));
            Assert.That(msgLine.MessageCategoryNumber, Is.EqualTo(0x01));
            Assert.That(msgLine.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
            Assert.That(msgLine.TextOutput, Is.EqualTo("Motenten : one"));
        }

        [Test]
        [Ignore]
        public void TestTokenizeError4()
        {
            string chatText = "01,00,00,80808080,00000019,00000017,0010,00,01,01,00,Motenten : one";

            DateTime timestamp = DateTime.Now;
            ChatLine chatLine = new ChatLine(chatText, timestamp);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.MessageCode, Is.EqualTo(0x01));
            Assert.That(msgLine.ExtraCode1, Is.EqualTo(0x00));
            Assert.That(msgLine.ExtraCode2, Is.EqualTo(0x00));
            Assert.That(msgLine.TextColor, Is.EqualTo(0x80808080));
            Assert.That(msgLine.EventSequence, Is.EqualTo(0x00000019));
            Assert.That(msgLine.UniqueSequence, Is.EqualTo(0x00000017));
            Assert.That(msgLine.TextLength, Is.EqualTo(0x0010));
            Assert.That(msgLine.MessageCategoryNumber, Is.EqualTo(0x01));
            Assert.That(msgLine.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
            Assert.That(msgLine.TextOutput, Is.EqualTo("Motenten : one"));
        }
    }
}
