using FluentAssertions;
using System;
using Xunit;

namespace NWheels.Logging.UnitTests
{
    public class ClassFromLoggingTests
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
