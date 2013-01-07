using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace WaywardGamers.KParser.Messages
{
    [TestFixture]
    public class TestMessage
    {
        #region Constructor
        [Test]
        public void TestConstructor()
        {
            Message msg = new Message();

            Assert.That(msg.IsParseSuccessful, Is.False);

            Assert.That(msg.CurrentMessageText, Is.Empty);
            Assert.That(msg.PreviousMessageText, Is.Empty);
            Assert.That(msg.CompleteMessageText, Is.Empty);

            Assert.That(msg.ChatDetails, Is.Null);
            Assert.That(msg.SystemDetails, Is.Null);
            Assert.That(msg.EventDetails, Is.Null);

            Assert.That(msg.MessageID, Is.EqualTo(0));
            Assert.That(msg.MessageCode, Is.EqualTo(0));
            Assert.That(msg.ExtraCode1, Is.EqualTo(0));
            Assert.That(msg.ExtraCode2, Is.EqualTo(0));

            Assert.That(msg.MessageLineCollection, Is.Not.Null);
            Assert.That(msg.MessageLineCollection, Is.Empty);

            Assert.That(msg.PrimaryMessageCode, Is.EqualTo(0));
            Assert.That(msg.CurrentMessageCode, Is.EqualTo(0));

            Assert.That(msg.Timestamp, Is.EqualTo(MagicNumbers.MinSQLDateTime));

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.System));
        }
        #endregion

        #region Test modifier functions
        [Test]
        public void TestMessageCategories()
        {
            Message msg = new Message();
            msg.SetMessageCategory(MessageCategoryType.Chat);

            Assert.That(msg.ChatDetails, Is.Not.Null);


            msg = new Message();
            msg.SetMessageCategory(MessageCategoryType.Event);

            Assert.That(msg.EventDetails, Is.Not.Null);


            msg = new Message();
            msg.SetMessageCategory(MessageCategoryType.System);

            Assert.That(msg.SystemDetails, Is.Not.Null);
        }

        [Test]
        public void TestAddMessageLine()
        {
            string chatText = "94,02,00,80808080,00000010,00000010,0033,00,01,00,00,Obtained key item: Healer's attire claim slip.1";

            DateTime timestamp = DateTime.Now;
            ChatLine chatLine = new ChatLine(chatText, timestamp);

            MessageLine msgLine = new MessageLine(chatLine);

            Message msg = new Message();
            msg.AddMessageLine(msgLine);

            Assert.That(msg.Timestamp, Is.EqualTo(timestamp));

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.System));
            Assert.That(msg.MessageID, Is.EqualTo(0x10));
            Assert.That(msg.MessageCode, Is.EqualTo(0x94));
            Assert.That(msg.ExtraCode1, Is.EqualTo(0x02));
            Assert.That(msg.ExtraCode2, Is.EqualTo(0x00));

            Assert.That(msg.PreviousMessageText, Is.Empty);
            Assert.That(msg.CurrentMessageText, Is.EqualTo("Obtained key item: Healer's attire claim slip."));
            Assert.That(msg.CompleteMessageText, Is.EqualTo("Obtained key item: Healer's attire claim slip."));

            msg.SetParseSuccess(false);

            Assert.That(msg.IsParseSuccessful, Is.False);
            Assert.That(msg.PreviousMessageText, Is.Empty);
            Assert.That(msg.CurrentMessageText, Is.EqualTo("Obtained key item: Healer's attire claim slip."));
            Assert.That(msg.CompleteMessageText, Is.EqualTo("Obtained key item: Healer's attire claim slip."));

            msg.SetParseSuccess(true);

            Assert.That(msg.IsParseSuccessful, Is.True);
            Assert.That(msg.PreviousMessageText, Is.EqualTo("Obtained key item: Healer's attire claim slip."));
            Assert.That(msg.CurrentMessageText, Is.Empty);
            Assert.That(msg.CompleteMessageText, Is.EqualTo("Obtained key item: Healer's attire claim slip."));
        }

        [Test]
        public void TestAddMultipleMessageLines()
        {
            string chatText1 = "cd,03,00,8050ff60,0000003a,00000039,0084,00,01,00,01,@Slice of desron slice of foy slice of chelolo hmm where did i put it AHA! Essence of minmin Leftover sandwich yum (Nov. 29, 2008 ";
            string chatText2 = "cd,03,00,8050ff60,0000003a,0000003a,000d,00,01,00,00,@9:59:15pm)";

            DateTime timestamp = DateTime.Now;
            ChatLine chatLine1 = new ChatLine(chatText1, timestamp);
            ChatLine chatLine2 = new ChatLine(chatText2, timestamp.AddSeconds(1));

            Message msg = new Message();
            MessageLine msgLine;
            
            msgLine = new MessageLine(chatLine1);
            msg.AddMessageLine(msgLine);
            msgLine = new MessageLine(chatLine2);
            msg.AddMessageLine(msgLine);

            // This particular message is a linkshell message.  Adjust based on known
            // parsing rules.
            msg.SetMessageCategory(MessageCategoryType.Chat);
            Assert.That(msg.ChatDetails, Is.Not.Null);
            msg.ChatDetails.ChatMessageType = ChatMessageType.Linkshell;
            msg.ChatDetails.ChatSpeakerName = "-Linkshell-";
            msg.ChatDetails.ChatSpeakerType = SpeakerType.Unknown;


            Assert.That(msg.Timestamp, Is.EqualTo(timestamp));

            Assert.That(msg.MessageCategory, Is.EqualTo(MessageCategoryType.Chat));
            Assert.That(msg.MessageID, Is.EqualTo(0x3a));
            Assert.That(msg.MessageCode, Is.EqualTo(0xcd));
            Assert.That(msg.ExtraCode1, Is.EqualTo(0x03));
            Assert.That(msg.ExtraCode2, Is.EqualTo(0x00));


            Assert.That(msg.PreviousMessageText, Is.Empty);
            Assert.That(msg.CurrentMessageText, Is.EqualTo("Slice of desron slice of foy slice of chelolo hmm where did i put it AHA! Essence of minmin Leftover sandwich yum (Nov. 29, 2008 9:59:15pm)"));
            Assert.That(msg.CompleteMessageText, Is.EqualTo("Slice of desron slice of foy slice of chelolo hmm where did i put it AHA! Essence of minmin Leftover sandwich yum (Nov. 29, 2008 9:59:15pm)"));

            msg.SetParseSuccess(true);

            Assert.That(msg.PreviousMessageText, Is.EqualTo("Slice of desron slice of foy slice of chelolo hmm where did i put it AHA! Essence of minmin Leftover sandwich yum (Nov. 29, 2008 9:59:15pm)"));
            Assert.That(msg.CurrentMessageText, Is.Empty);
            Assert.That(msg.CompleteMessageText, Is.EqualTo("Slice of desron slice of foy slice of chelolo hmm where did i put it AHA! Essence of minmin Leftover sandwich yum (Nov. 29, 2008 9:59:15pm)"));
        }
        #endregion
    }
}
