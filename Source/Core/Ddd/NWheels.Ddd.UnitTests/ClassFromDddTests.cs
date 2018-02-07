using FluentAssertions;
using NWheels.Testability;
using System;
using Xunit;

namespace NWheels.Ddd.UnitTests
{
    public class ClassFromDddTests : TestBase.UnitTest
    {
        [Fact]
        public void TestD()
        {
            var d = new ClassFromDdd();
            var result = d.D();
            result.Should().Be("DDD");
        }
    }
}
