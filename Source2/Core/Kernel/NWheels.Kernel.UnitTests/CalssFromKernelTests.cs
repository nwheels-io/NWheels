using FluentAssertions;
using System;
using Xunit;

namespace NWheels.Kernel.UnitTests
{
    public class CalssFromKernelTests
    {
        [Fact]
        public void TestK()
        {
            var k = new ClassFromKernel();
            var result = k.K();
            result.Should().Be("KKK");
        }
    }
}
