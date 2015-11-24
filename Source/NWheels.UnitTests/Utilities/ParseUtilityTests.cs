using System;
using NUnit.Framework;
using NWheels.Utilities;
using Shouldly;

namespace NWheels.UnitTests.Utilities
{
    [TestFixture]
    public class ParseUtilityTests
    {
        [TestCase("2010-10-30", "2010-10-30 00:00:00")]
        [TestCase("2010-10-30 15:48:49", "2010-10-30 15:48:49")]
        public void CanParseDateTime(string input, string expectedOutput)
        {
            var actualOutput = (DateTime)ParseUtility.Parse(input, typeof(DateTime));
            actualOutput.ToString("yyyy-MM-dd HH:mm:ss").ShouldBe(expectedOutput);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TestCase("2010-10-30", "2010-10-30 00:00:00")]
        [TestCase("2010-10-30 15:48:49", "2010-10-30 15:48:49")]
        public void CanParseDateTimeAsGeneric(string input, string expectedOutput)
        {
            var actualOutput = ParseUtility.Parse<DateTime>(input);
            actualOutput.ToString("yyyy-MM-dd HH:mm:ss").ShouldBe(expectedOutput);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanParseNullableDateTimeWithValue()
        {
            DateTime? value = (DateTime?)ParseUtility.Parse("2010-10-30", typeof(DateTime?));
            value.ShouldBe(new DateTime(2010, 10, 30));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanParseNullableDateTimeWithNoValue()
        {
            DateTime? value = (DateTime?)ParseUtility.Parse(null, typeof(DateTime?));
            value.ShouldBe(null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanParseNullableDateTimeWithValueAsGeneric()
        {
            DateTime? value = ParseUtility.Parse<DateTime?>("2010-10-30");
            value.ShouldBe(new DateTime(2010, 10, 30));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanParseNullableDateTimeWithNoValueAsGeneric()
        {
            DateTime? value = ParseUtility.Parse<DateTime?>(null);
            value.ShouldBe(null);
        }
    }
}
