using FluentAssertions;
using NWheels.Testability;
using System;
using Xunit;

namespace NWheels.Microservices.UnitTests
{
    public class ClassFromMicroservicesTests : TestBase.UnitTest
    {
        [Fact]
        public void TestM()
        {
            var m = new ClassFromMicroservices();
            var result = m.M();
            result.Should().Be("MMM");
        }
    }
}
