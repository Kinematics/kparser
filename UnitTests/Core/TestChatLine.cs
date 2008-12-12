using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace WaywardGamers.KParser
{
    [TestFixture]
    public class TestChatLine
    {
        [Test]
        public void TestConstruction1()
        {
            DateTime timestamp = DateTime.Now;
            ChatLine testChatLine = new ChatLine("Test Line");

            Assert.That(testChatLine.Timestamp, Is.GreaterThanOrEqualTo(timestamp));
            Assert.That(testChatLine.Timestamp, Is.LessThanOrEqualTo(DateTime.Now));

            Assert.That(testChatLine.ChatText, Is.EqualTo("Test Line"));
        }

        [Test]
        public void TestConstruction2()
        {
            DateTime timestamp = DateTime.Now;
            ChatLine testChatLine = new ChatLine("Test Line", timestamp);

            Assert.That(testChatLine.Timestamp, Is.EqualTo(timestamp));

            Assert.That(testChatLine.ChatText, Is.EqualTo("Test Line"));
        }

        [Test]
        public void TestTimestampSet()
        {
            ChatLine testChatLine = new ChatLine("Test Line");

            DateTime timestamp = DateTime.Now;

            testChatLine.Timestamp = timestamp;

            Assert.That(testChatLine.Timestamp, Is.EqualTo(timestamp));
        }

        [Test]
        public void TestTimestampNull()
        {
            ChatLine testChatLine = new ChatLine("Test Line");

            testChatLine.Timestamp = DateTime.FromBinary(0);

            Assert.That(testChatLine.Timestamp, Is.EqualTo(MagicNumbers.MinSQLDateTime));
        }
    }
}
