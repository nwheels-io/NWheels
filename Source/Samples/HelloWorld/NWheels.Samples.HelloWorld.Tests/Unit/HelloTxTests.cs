using FluentAssertions;
using NWheels.Samples.HelloWorld.HelloService;
using NWheels.Testability;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using System.Threading.Tasks;

namespace NWheels.Samples.HelloWorld.Tests.Unit
{
    public class HelloTxTests : TestBase.UnitTest
    {
        [Fact]
        public async Task TestHello()
        {
            var tx = new Program.HelloTx();

            string output = await tx.Hello("world");

            output.Should().Be("Hello, world!"); 
        }
    }
}
