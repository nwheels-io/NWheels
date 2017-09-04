using FluentAssertions;
using System;
using Xunit;

namespace NWheels.UI.UnitTests
{
    public class ClassFromUITests
    {
        [Fact]
        public void TestU()
        {
            var u = new ClassFromUI();
            var result = u.U();
            result.Should().Be("UUU");
        }
    }
}
