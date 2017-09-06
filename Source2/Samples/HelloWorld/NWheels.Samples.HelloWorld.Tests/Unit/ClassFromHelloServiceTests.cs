using FluentAssertions;
using NWheels.Samples.HelloWorld.HelloService;
using NWheels.Testability;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace NWheels.Samples.HelloWorld.Tests.Unit
{
    public class ClassFromHelloServiceTests : TestBase.UnitTest
    {
        [Fact]
        public void TestH()
        {
            var h = new ClassFromHelloService();
            var result = h.H();
            result.Should().Be("HHH");
        }
    }
}
