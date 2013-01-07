using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace WaywardGamers.KParser.Parsing
{
    [TestFixture]
    public class TextMsgManager
    {
        [Test]
        public void AccessInstance()
        {
            Assert.That(MsgManager.Instance, Is.InstanceOf(typeof(MsgManager)));
        }
    }
}
