using FluentAssertions;
using System;
using Xunit;

namespace NWheels.Transactions.UnitTests
{
    public class ClassFromTransactionsTests
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
