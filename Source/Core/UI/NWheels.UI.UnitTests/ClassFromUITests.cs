using FluentAssertions;
using NWheels.Testability;
using System;
using Xunit;

namespace NWheels.UI.UnitTests
{
    public class ClassFromUITests : TestBase.UnitTest
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
