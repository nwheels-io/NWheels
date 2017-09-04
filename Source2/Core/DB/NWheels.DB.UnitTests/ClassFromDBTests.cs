using FluentAssertions;
using System;
using Xunit;

namespace NWheels.DB.UnitTests
{
    public class ClassFromDBTests
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
