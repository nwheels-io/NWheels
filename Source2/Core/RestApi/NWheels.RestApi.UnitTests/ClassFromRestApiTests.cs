using FluentAssertions;
using NWheels.Testability;
using System;
using Xunit;

namespace NWheels.RestApi.UnitTests
{
    public class ClassFromRestApiTests : TestBase.UnitTest
    {
        [Fact]
        public void TestR()
        {
            var r = new ClassFromRestApi();
            var result = r.R();
            result.Should().Be("RRR");
        }
    }
}
