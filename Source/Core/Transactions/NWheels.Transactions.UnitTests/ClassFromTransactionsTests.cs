using FluentAssertions;
using NWheels.Testability;
using System;
using Xunit;

namespace NWheels.Transactions.UnitTests
{
    public class ClassFromTransactionsTests : TestBase.UnitTest
    {
        [Fact]
        public void TestT()
        {
            var t = new ClassFromTransactions();
            var result = t.T();
            result.Should().Be("TTT");
        }
    }
}
