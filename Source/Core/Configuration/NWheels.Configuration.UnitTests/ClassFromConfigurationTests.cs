using FluentAssertions;
using NWheels.Testability;
using System;
using Xunit;

namespace NWheels.Configuration.UnitTests
{
    public class ClassFromConfigurationTests : TestBase.UnitTest
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
