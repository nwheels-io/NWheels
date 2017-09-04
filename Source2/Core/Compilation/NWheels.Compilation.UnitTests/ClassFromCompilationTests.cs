using FluentAssertions;
using System;
using Xunit;

namespace NWheels.Compilation.UnitTests
{
    public class ClassFromCompilationTests
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
