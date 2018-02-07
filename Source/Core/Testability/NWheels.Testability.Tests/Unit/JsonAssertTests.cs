using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Xunit;

namespace NWheels.Testability.Tests.Unit
{
    public class JsonAssertTests : TestBase.UnitTest
    {
        [Fact]
        public void AreEqual_EqualStrings_AssertPassed()
        {
            //-- arrange

            var json = "{\"num\":123, \"str\":\"ABC\"}";

            //-- act & assert

            JsonAssert.AreEqual(json, json);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Theory]
        [InlineData(
            "{\"num\":123}",
            " { \"num\" : 123 } "
        )]
        [InlineData(
            "{\"num\":123, \"str\":\"ABC\"}",
            "{ \"str\" : \"ABC\", \"num\" : 123 }"
        )]
        [InlineData(
            "{\"num\":123, \"str\":\"ABC\", \"obj\": { \"x\" : 100, \"y\": 200 } }",
            "{ \"str\" : \"ABC\", \"obj\": {\"y\":200,\"x\":100}, \"num\" : 123 }"
        )]
        [InlineData(
            "{\"arr\": [ { \"x\": 100, \"y\":200 } , { \"y\": 400, \"x\":300 }  ]}",
            "{\"arr\": [{\"y\":200,\"x\":100},{\"x\":300,\"y\": 400}]}"
        )]
        [InlineData(
            "[ { \"x\": 100, \"y\":200 } , { \"y\": 400, \"x\":300 }  ]",
            "[ {\"y\":200,\"x\":100},{\"x\":300,\"y\": 400}]"
        )]
        [InlineData(
            "\"ABC\"",
            "\"ABC\""
        )]
        public void AreEqual_DifferentStringsEquivalentJson_AssertPassed(string expectedJson, string actualJson)
        {
            JsonAssert.AreEqual(actualJson, expectedJson);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Theory]
        [InlineData(
            "{\"num\":123}",
            "{\"num\":456}"
        )]
        [InlineData(
            "{\"num\":123, \"str\":\"ABC\"}",
            "{\"num\":123}"
        )]
        [InlineData(
            "{\"arr\": [{\"x\":100,\"y\":200}]}",
            "{\"arr\": [{\"x\":300,\"4\":400}]}"
        )]
        [InlineData(
            "{\"arr\": [{\"x\":100,\"y\":200}]}",
            "{\"arr\": [{\"x\":100,\"y\":200},{\"x\":300,\"4\":400}]}"
        )]
        [InlineData(
            "[{\"x\":100,\"y\":200},{\"x\":300,\"y\":400}]",
            "[{\"x\":200,\"y\":100},{\"x\":400,\"y\":300}]"
        )]
        [InlineData(
            "\"ABC\"",
            "\"DEF\""
        )]
        public void AreEqual_NonEquivalentJson_AssertFailed(string expectedJson, string actualJson)
        {
            Action act = () => {
                JsonAssert.AreEqual(
                    actualJson,
                    expectedJson,
                    because: "this is the {0}",
                    becauseArgs: new object[] {"test"});
            };

            var exception = act.ShouldThrow<Xunit.Sdk.XunitException>().Which;
            exception.Message.Should().Contain("because this is the test");
        }
    }
}
