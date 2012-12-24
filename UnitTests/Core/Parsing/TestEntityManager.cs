using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace WaywardGamers.KParser.Parsing
{
    [TestFixture]
    public class TestEntityManager
    {
        [Test]
        public void AccessInstance()
        {
            Assert.That(EntityManager.Instance, Is.InstanceOf(typeof(EntityManager)));
        }
    }
}
