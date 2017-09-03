using FluentAssertions;
using System;
using Xunit;

namespace NWheels.Communication.UnitTests
{
    public class ClassFromCommunicationTests
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
