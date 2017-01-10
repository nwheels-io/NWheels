using NUnit.Framework;
using System;

namespace NWheels.Mechanism.UnitTests
{
    [TestFixture]
    public class TestClassOne
    {
        [Test]
        public void TestPass()
        {
            Assert.AreEqual(1, 1);
        }

        [Test]
        public void TestFail()
        {
            Assert.AreEqual(0, 1);
        }
    }
}
