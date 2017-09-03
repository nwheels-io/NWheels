using FluentAssertions;
using System;
using Xunit;

namespace NWheels.Microservices.UnitTests
{
    public class ClassFromMicroservicesTests
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
