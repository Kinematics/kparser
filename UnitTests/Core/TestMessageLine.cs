using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace WaywardGamers.KParser.Messages
{
    [TestFixture]
    public class TestMessageLine
    {
        #region Basic Construction
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
        public void TestStandardConstruction()
        {
            string chatText =
                "01,00,00,80808080,00000019,00000017,0010,00,01,01,00,Motenten : one";

            DateTime timestamp = DateTime.Now;
            ChatLine chatLine = new ChatLine(chatText, timestamp);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.OriginalText, Is.EqualTo(chatText));
            Assert.That(msgLine.Timestamp, Is.EqualTo(timestamp));
        }
        #endregion

        #region Basic Tokenization
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
        #endregion

        #region Character sets
        [Test]
        public void TestConvertJapaneseChars()
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
        [Ignore("No sample data yet.")]
        public void TestConvertFrenchChars()
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
        #endregion

        #region Timestamp plugin
        [Test]
        public void TestTokenizeTimestampPluginRAM1()
        {
            DateTime timestamp = DateTime.Now;

            DateTime pluginTimestamp = DateTime.Today;
            pluginTimestamp = pluginTimestamp.AddHours(12);
            pluginTimestamp = pluginTimestamp.AddMinutes(15);
            pluginTimestamp = pluginTimestamp.AddSeconds(10);

            string chatText = "01,00,00,80808080,00000019,00000017,0010,00,01,01,00,?[12:15:10] Motenten : one";

            ChatLine chatLine = new ChatLine(chatText, timestamp);

            Monitor.SetParseMode(DataSource.Ram);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.TextOutput, Is.EqualTo("Motenten : one"));
            Assert.That(msgLine.Timestamp, Is.EqualTo(timestamp));
        }

        [Test]
        public void TestTokenizeTimestampPluginLOG1()
        {
            DateTime timestamp = DateTime.Now;

            DateTime pluginTimestamp = DateTime.Today;
            pluginTimestamp = pluginTimestamp.AddHours(12);
            pluginTimestamp = pluginTimestamp.AddMinutes(15);
            pluginTimestamp = pluginTimestamp.AddSeconds(10);

            string chatText = "01,00,00,80808080,00000019,00000017,0010,00,01,01,00,?[12:15:10] Motenten : one";

            ChatLine chatLine = new ChatLine(chatText, timestamp);

            Monitor.SetParseMode(DataSource.Log);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.TextOutput, Is.EqualTo("Motenten : one"));
            Assert.That(msgLine.Timestamp, Is.EqualTo(pluginTimestamp));
        }

        [Test]
        public void TestTokenizeTimestampPluginRAM2()
        {
            DateTime timestamp = DateTime.Now;

            DateTime pluginTimestamp = DateTime.Today;
            pluginTimestamp = pluginTimestamp.AddHours(12);
            pluginTimestamp = pluginTimestamp.AddMinutes(15);
            pluginTimestamp = pluginTimestamp.AddSeconds(10);

            string chatText = "01,00,00,80808080,00000019,00000017,0010,00,01,01,00,ú[12:15:10] Motenten : one";

            ChatLine chatLine = new ChatLine(chatText, timestamp);

            Monitor.SetParseMode(DataSource.Ram);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.TextOutput, Is.EqualTo("Motenten : one"));
            Assert.That(msgLine.Timestamp, Is.EqualTo(timestamp));
        }

        [Test]
        public void TestTokenizeTimestampPluginLOG2()
        {
            DateTime timestamp = DateTime.Now;

            DateTime pluginTimestamp = DateTime.Today;
            pluginTimestamp = pluginTimestamp.AddHours(12);
            pluginTimestamp = pluginTimestamp.AddMinutes(15);
            pluginTimestamp = pluginTimestamp.AddSeconds(10);

            string chatText = "01,00,00,80808080,00000019,00000017,0010,00,01,01,00,ú[12:15:10] Motenten : one";

            ChatLine chatLine = new ChatLine(chatText, timestamp);

            Monitor.SetParseMode(DataSource.Log);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.TextOutput, Is.EqualTo("Motenten : one"));
            Assert.That(msgLine.Timestamp, Is.EqualTo(pluginTimestamp));
        }

        [Test]
        public void TestTokenizeTimestampPluginRAM3()
        {
            DateTime timestamp = DateTime.Now;

            DateTime pluginTimestamp = DateTime.Today;
            pluginTimestamp = pluginTimestamp.AddHours(12);
            pluginTimestamp = pluginTimestamp.AddMinutes(15);
            pluginTimestamp = pluginTimestamp.AddSeconds(10);

            string chatText = "01,00,00,80808080,00000019,00000017,0010,00,01,01,00,ú[12:15:10] Motenten : one";

            ChatLine chatLine = new ChatLine(chatText, timestamp);

            Monitor.SetParseMode(DataSource.Ram);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.TextOutput, Is.EqualTo("Motenten : one"));
            Assert.That(msgLine.Timestamp, Is.EqualTo(timestamp));
        }

        [Test]
        public void TestTokenizeTimestampPluginLOG3()
        {
            DateTime timestamp = DateTime.Now;

            DateTime pluginTimestamp = DateTime.Today;
            pluginTimestamp = pluginTimestamp.AddHours(12);
            pluginTimestamp = pluginTimestamp.AddMinutes(15);
            pluginTimestamp = pluginTimestamp.AddSeconds(10);

            string chatText = "01,00,00,80808080,00000019,00000017,0010,00,01,01,00,ú[12:15:10] Motenten : one";

            ChatLine chatLine = new ChatLine(chatText, timestamp);

            Monitor.SetParseMode(DataSource.Log);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.TextOutput, Is.EqualTo("Motenten : one"));
            Assert.That(msgLine.Timestamp, Is.EqualTo(pluginTimestamp));
        }

        [Test]
        public void TestTokenizeTimestampPluginRAMCorruptedConversion()
        {
            DateTime timestamp = DateTime.Now;

            DateTime pluginTimestamp = DateTime.Today;
            pluginTimestamp = pluginTimestamp.AddHours(0);
            pluginTimestamp = pluginTimestamp.AddMinutes(40);
            pluginTimestamp = pluginTimestamp.AddSeconds(54);

            string chatText = "01,00,00,80808080,00000009,00000009,001c,00,01,01,00,・00:40:54] Aluri : hello";

            ChatLine chatLine = new ChatLine(chatText, timestamp);

            Monitor.SetParseMode(DataSource.Ram);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.TextOutput, Is.EqualTo("Aluri : hello"));
            Assert.That(msgLine.Timestamp, Is.EqualTo(timestamp));
        }

        [Test]
        public void TestTokenizeTimestampPluginLOGCorruptedConversion()
        {
            DateTime timestamp = DateTime.Now;

            DateTime pluginTimestamp = DateTime.Today;
            pluginTimestamp = pluginTimestamp.AddHours(0);
            pluginTimestamp = pluginTimestamp.AddMinutes(40);
            pluginTimestamp = pluginTimestamp.AddSeconds(54);

            string chatText = "01,00,00,80808080,00000009,00000009,001c,00,01,01,00,・00:40:54] Aluri : hello";

            ChatLine chatLine = new ChatLine(chatText, timestamp);

            Monitor.SetParseMode(DataSource.Log);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.TextOutput, Is.EqualTo("Aluri : hello"));
            Assert.That(msgLine.Timestamp, Is.EqualTo(pluginTimestamp));
        }
        #endregion

        #region Text inclusions
        [Test]
        public void TestTokenizeTextWithAutotranslate()
        {
            string chatText = "05,00,00,8020c0a0,00000018,00000018,0020,00,01,01,00,(Motenten) stuff for ï'Salvageï(";

            DateTime timestamp = DateTime.Now;
            ChatLine chatLine = new ChatLine(chatText, timestamp);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.TextOutput, Is.EqualTo("(Motenten) stuff for [Salvage]"));
        }

        [Test]
        public void TestTokenizeTextWithItems()
        {
            string chatText = "55,00,00,80808010,0000265a,00002c51,0022,00,01,02,00,Lans uses a toolbag (shihei).1";

            DateTime timestamp = DateTime.Now;
            ChatLine chatLine = new ChatLine(chatText, timestamp);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.TextOutput, Is.EqualTo("Lans uses a toolbag (shihei)."));
        }

        [Test]
        public void TestTokenizeTextWithKeyItems()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";

            DateTime timestamp = DateTime.Now;
            ChatLine chatLine = new ChatLine(chatText, timestamp);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.TextOutput, Is.EqualTo("Obtained key item: Healer's attire claim slip."));
        }
        #endregion

        #region Unusual characters
        [Test]
        public void TestCorrectMogJobChange()
        {
            string chatText = "8d,00,00,60808010,00000023,00000023,0028,00,01,02,00,Moogle : Chaaange...job! Kupopopooo!1";

            DateTime timestamp = DateTime.Now;
            ChatLine chatLine = new ChatLine(chatText, timestamp);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.TextOutput, Is.EqualTo("Moogle : Chaaange...job! Kupopopooo!"));
        }

        [Test]
        public void TestCorrectItemDrops()
        {
            string chatText = "79,00,00,80c0c050,000026cd,00002cd6,0035,00,01,02,00,yYou find a wind crystal on the Greater Colibri.1";

            DateTime timestamp = DateTime.Now;
            ChatLine chatLine = new ChatLine(chatText, timestamp);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.TextOutput, Is.EqualTo("You find a wind crystal on the Greater Colibri."));
        }

        [Test]
        public void TestCorrectItemDistribution()
        {
            string chatText = "7f,00,00,80c0c050,0000277a,00002dac,0022,00,01,02,00,Motenten obtains a wind crystal.1";

            DateTime timestamp = DateTime.Now;
            ChatLine chatLine = new ChatLine(chatText, timestamp);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.TextOutput, Is.EqualTo("Motenten obtains a wind crystal."));
        }

        [Test]
        [Ignore("Need sample message.")]
        public void TestCorrectBaseTimeLimit()
        {
            string chatText = "8d,00,00,60808010,00000023,00000023,0028,00,01,02,00,Moogle : Chaaange...job! Kupopopooo!1";

            DateTime timestamp = DateTime.Now;
            ChatLine chatLine = new ChatLine(chatText, timestamp);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.TextOutput, Is.EqualTo("Moogle : Chaaange...job! Kupopopooo!"));
        }

        [Test]
        public void TestCorrectAssaultTimeLimit()
        {
            string chatText = "92,02,00,80808080,00006c88,00007d44,002b,00,01,00,00,?Time remaining: 5 minutes (Earth time).1";

            DateTime timestamp = DateTime.Now;
            ChatLine chatLine = new ChatLine(chatText, timestamp);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.TextOutput, Is.EqualTo("Time remaining: 5 minutes (Earth time)."));
        }

        [Test]
        [Ignore("Need sample message.")]
        public void TestCorrectLimbusTimeLimit()
        {
            string chatText = "8d,00,00,60808010,00000023,00000023,0028,00,01,02,00,Moogle : Chaaange...job! Kupopopooo!1";

            DateTime timestamp = DateTime.Now;
            ChatLine chatLine = new ChatLine(chatText, timestamp);

            MessageLine msgLine = new MessageLine(chatLine);

            Assert.That(msgLine.TextOutput, Is.EqualTo("Moogle : Chaaange...job! Kupopopooo!"));
        }
        #endregion
    }
}
