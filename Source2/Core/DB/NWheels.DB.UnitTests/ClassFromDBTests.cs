using FluentAssertions;
using NWheels.Testability;
using System;
using Xunit;

namespace NWheels.DB.UnitTests
{
    public class ClassFromDBTests : TestBase.UnitTest
    {
        [Fact]
        public void TestDB()
        {
            var d = new ClassFromDB();
            var result = d.DB();
            result.Should().Be("DBDBDB");
        }
    }
}
