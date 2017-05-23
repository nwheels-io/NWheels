using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NWheels.Samples.FirstHappyPath.HelloService.Tests.Unit
{
    public class HelloWorldTxTests
    {
        [Fact]
        public void TestHello()
        {
            //-- arrange

            var tx = new HelloWorldTx();
            var input = "TEST";

            //-- act

            var output = tx.Hello(input).Result;

            //-- assert 

            output.Should().Be("Hello world, from TEST!");
        }
    }
}
