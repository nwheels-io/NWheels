using FluentAssertions;
using System;
using Xunit;

namespace NWheels.Ddd.UnitTests
{
    public class ClassFromDddTests
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
