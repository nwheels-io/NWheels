using FluentAssertions;
using NWheels.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Xunit;

namespace NWheels.Implementation.UnitTests.Extensions
{
    public class StringExtensionTests
    {
        [Theory]
        [InlineData("PrefixEndOfString", "Prefix", "EndOfString")]
        [InlineData("PrefixEndOfString", "pREFIX", "PrefixEndOfString")]
        [InlineData("EndOfString", "Prefix", "EndOfString")]
        [InlineData("Prefix", "Prefix", "Prefix")]
        [InlineData("", "Prefix", "")]
        [InlineData(null, "Prefix", null)]
        [InlineData("PrefixEndOfString", "", "PrefixEndOfString")]
        [InlineData("PrefixEndOfString", null, "PrefixEndOfString")]
        [InlineData("", "", "")]
        [InlineData("", null, "")]
        [InlineData(null, "", null)]
        [InlineData(null, null, null)]
        public void TestTrimPrefix(string input, string prefix, string expectedOutput)
        {
            //-- act

            var actualOutput = input.TrimPrefix(prefix);

            //-- assert

            actualOutput.Should().Be(expectedOutput);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Theory]
        [InlineData("BeginOfStringSuffix", "Suffix", "BeginOfString")]
        [InlineData("BeginOfStringSuffix", "sUFFIX", "BeginOfStringSuffix")]
        [InlineData("BeginOfString", "Suffix", "BeginOfString")]
        [InlineData("Suffix", "Suffix", "Suffix")]
        [InlineData("", "Suffix", "")]
        [InlineData(null, "Suffix", null)]
        [InlineData("BeginOfStringSuffix", "", "BeginOfStringSuffix")]
        [InlineData("BeginOfStringSuffix", null, "BeginOfStringSuffix")]
        [InlineData("", "", "")]
        [InlineData("", null, "")]
        [InlineData(null, "", null)]
        [InlineData(null, null, null)]
        public void TestTrimSuffixx(string input, string suffix, string expectedOutput)
        {
            //-- act

            var actualOutput = input.TrimSuffix(suffix);

            //-- assert

            actualOutput.Should().Be(expectedOutput);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Theory]
        [InlineData("ABC", "DEF", "ABC")]
        [InlineData(null, "DEF", "DEF")]
        [InlineData("", "DEF", "DEF")]
        [InlineData("", null, null)]
        [InlineData(null, null, null)]
        public void TestDefaultIfNullOrEmpty(string input, string defaultValue, string expectedOutput)
        {
            //-- act

            var actualOutput = input.DefaultIfNullOrEmpty(defaultValue);

            //-- assert

            actualOutput.Should().Be(expectedOutput);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<object[]> TestCases_TestToPathString = new object[][] {
            #region Test cases
            new object[] { @"abc", "abc" },
            new object[] { @"abc/def", $"abc{Path.DirectorySeparatorChar}def" },
            new object[] { @"abc\def", $"abc{Path.DirectorySeparatorChar}def" },
            new object[] { @"a/b/c", $"a{Path.DirectorySeparatorChar}b{Path.DirectorySeparatorChar}c" },
            new object[] { @"a\b\c", $"a{Path.DirectorySeparatorChar}b{Path.DirectorySeparatorChar}c" },
            new object[] { @"   ", "   " },
            new object[] { @"", "" },
            new object[] { null, null },
            #endregion
        };

        [Theory]
        [MemberData(nameof(TestCases_TestToPathString))]
        public void TestToPathString(string input, string expectedOutput)
        {
            //-- act

            var actualOutput = input.ToPathString();

            //-- assert

            actualOutput.Should().Be(expectedOutput);
        }
    }
}
