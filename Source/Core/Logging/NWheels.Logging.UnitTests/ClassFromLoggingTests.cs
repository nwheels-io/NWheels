using FluentAssertions;
using NWheels.Testability;
using System;
using Xunit;

namespace NWheels.Logging.UnitTests
{
    public class ClassFromLoggingTests : TestBase.UnitTest
    {
        [Fact]
        public void TestL()
        {
            var l = new ClassFromLogging();
            var result = l.L();
            result.Should().Be("LLL");
        }
    }
}
