using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NWheels.Testability.Extensions;
using Xunit;
using Xunit.Sdk;

namespace NWheels.Testability.Tests.Unit.Extensions
{
    public class StringAssertionsExtensionsTests : TestBase.UnitTest
    {
        [Fact]
        public void BeJson_JsonMatches_Pass()
        {
            //-- arrange

            var actual = "{\"num\":123, \"str\": \"ABC\"}";
            var expected = "{ \"str\" : \"ABC\" , \"num\" : 123 }";

            //-- act & assert

            actual.Should().BeJson(expected);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void BeJson_JsonDoesNotMatch_Fail()
        {
            //-- arrange

            var actual = "{\"num\":123}";
            var expected = "{\"num\":124}";

            Action act = () => {
                actual.Should().BeJson(expected);
            };

            //-- act & assert

            act.ShouldThrow<XunitException>();
        }
    }
}
