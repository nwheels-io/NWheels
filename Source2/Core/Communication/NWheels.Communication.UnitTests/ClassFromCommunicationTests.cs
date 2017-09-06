using FluentAssertions;
using NWheels.Testability;
using System;
using Xunit;

namespace NWheels.Communication.UnitTests
{
    public class ClassFromCommunicationTests : TestBase.UnitTest
    {
        [Fact]
        public void TestMC()
        {
            var m = new ClassFromCommunication();
            var result = m.Com();
            result.Should().Be("COMCOMCOM");
        }
    }
}
