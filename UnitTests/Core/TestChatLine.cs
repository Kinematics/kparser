using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace WaywardGamers.KParser.Messages
{
    [TestFixture]
    public class TestChatLine
    {
        /// <summary>
        /// Test basic constructor that only takes the chat text.  Verify that the
        /// assigned timestamp falls between the two DateTime.Now checks.
        /// </summary>
        [Test]
        public void TestConstruction1()
        {
            DateTime timestamp = DateTime.Now.ToUniversalTime();
            ChatLine testChatLine = new ChatLine("Test Line");

            Assert.That(testChatLine.Timestamp, Is.GreaterThanOrEqualTo(timestamp));
            Assert.That(testChatLine.Timestamp, Is.LessThanOrEqualTo(DateTime.Now.ToUniversalTime()));

            Assert.That(testChatLine.ChatText, Is.EqualTo("Test Line"));
        }

        /// <summary>
        /// Test the full constructor that includes setting the timestamp.
        /// Verify that both the text and the timestamps are properly set.
        /// </summary>
        [Test]
        public void TestConstruction2()
        {
            DateTime timestamp = DateTime.Now;
            ChatLine testChatLine = new ChatLine("Test Line", timestamp);

            Assert.That(testChatLine.Timestamp, Is.EqualTo(timestamp));

            Assert.That(testChatLine.ChatText, Is.EqualTo("Test Line"));
        }

        /// <summary>
        /// Test whether setting the timestamp property works properly.
        /// </summary>
        [Test]
        public void TestTimestampSet()
        {
            ChatLine testChatLine = new ChatLine("Test Line");

            DateTime timestamp = DateTime.Now.AddMinutes(1);

            testChatLine.Timestamp = timestamp;

            Assert.That(testChatLine.Timestamp, Is.EqualTo(timestamp));
        }

        /// <summary>
        /// Test the scenario of setting an invalid timestamp (one that falls
        /// prior to the arbitrarily set MinSQLDateTime value, as determined by
        /// MS SQL documentation).  In the case of a time that lands too early,
        /// it needs to set and return the MinSQLDateTime value instead.
        /// </summary>
        [Test]
        public void TestTimestampNull()
        {
            ChatLine testChatLine = new ChatLine("Test Line");

            testChatLine.Timestamp = DateTime.FromBinary(0);

            Assert.That(testChatLine.Timestamp, Is.EqualTo(MagicNumbers.MinSQLDateTime));
        }
    }
}
