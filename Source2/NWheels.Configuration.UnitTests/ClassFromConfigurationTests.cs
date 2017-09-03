using FluentAssertions;
using System;
using Xunit;

namespace NWheels.Configuration.UnitTests
{
    public class ClassFromConfigurationTests
    {
        [Fact]
        public void TestC()
        {
            var c = new ClassFromConfiguration();
            var result = c.C();
            result.Should().Be("CCC");
        }
    }
}
