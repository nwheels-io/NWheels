using Xunit;
using NWheels.Api;
using NWheels.Api.Extensions;

namespace NWheels.Api.Tests.Extensions
{
    public class StringExtensionsTests
    {
        [Fact]
        public void TrimSuffix_SuffixFound_Trimmed()
        {
            //- act

            var output = "ABCTT".TrimSuffix("TT");

            //- assert

            Assert.Equal("ABC", output);
        }
    }
}