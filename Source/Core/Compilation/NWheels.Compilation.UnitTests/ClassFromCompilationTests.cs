using FluentAssertions;
using NWheels.Testability;
using System;
using Xunit;

namespace NWheels.Compilation.UnitTests
{
    public class ClassFromCompilationTests : TestBase.UnitTest
    {
        [Fact]
        public void TestC()
        {
            var c = new ClassFromCompilation();
            var result = c.C();
            result.Should().Be("KKKCCC");
        }
    }
}
