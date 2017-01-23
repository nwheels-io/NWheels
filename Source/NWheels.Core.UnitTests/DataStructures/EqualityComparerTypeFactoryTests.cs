using FluentAssertions;
using NWheels.Compilation.Adapters.Roslyn;
using NWheels.Compilation.Mechanism.Factories;
using NWheels.DataStructures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xunit;

namespace NWheels.Core.UnitTests.DataStructures
{
    public class EqualityComparerTypeFactoryTests
    {
        [Fact]
        public void CanCreateEqualityComparer()
        {
            //-- arrange

            var typeFactoryUnderTest = CreateFactoryUnderTest();
            IEqualityComparerObjectFactory objectFactoryUnderTest = typeFactoryUnderTest;

            //-- act

            typeFactoryUnderTest.ImplementEqualityComparer(typeof(ASimplestClass));
            IEqualityComparer<ASimplestClass> comparer = objectFactoryUnderTest.GetEqualityComparer<ASimplestClass>();

            //-- assert

            comparer.Should().NotBeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Theory]
        [InlineData(123, "ABC", "2010-10-10", DayOfWeek.Saturday, true)]
        [InlineData(124, "ABC", "2010-10-10", DayOfWeek.Saturday, false)]
        [InlineData(123, "DEF", "2010-10-10", DayOfWeek.Saturday, false)]
        [InlineData(123, "ABC", "2011-11-11", DayOfWeek.Saturday, false)]
        [InlineData(123, "ABC", "2010-10-10", DayOfWeek.Thursday, false)]
        public void CanCompareEquality_AFlatStructWithFields(
            int yNumber, string yText, string yTimeStamp, DayOfWeek yDay, bool expectedResult)
        {
            //-- arrange

            var typeFactoryUnderTest = CreateFactoryUnderTest();
            typeFactoryUnderTest.ImplementEqualityComparer(typeof(ASimplestClass));

            IEqualityComparerObjectFactory objectFactoryUnderTest = typeFactoryUnderTest;
            var comparer = objectFactoryUnderTest.GetEqualityComparer<AFlatStructWithFields>();

            var x = new AFlatStructWithFields {
                Number = 123,
                Text = "ABC",
                TimeStamp = new DateTime(2010, 10, 10),
                Day = DayOfWeek.Saturday
            };

            var y = new AFlatStructWithFields {
                Number = yNumber,
                Text = yText,
                TimeStamp = DateTime.ParseExact(yTimeStamp, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                Day = yDay
            };

            //-- act

            var actualResult = comparer.Equals(x, y);
            var xHashCode = comparer.GetHashCode(x);
            var yHashCode = comparer.GetHashCode(y);

            //-- assert

            actualResult.Should().Be(expectedResult);

            if (expectedResult)
            {
                yHashCode.Should().Be(xHashCode);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private EqualityComparerTypeFactory CreateFactoryUnderTest()
        {
            var backend = new RoslynTypeFactoryBackend();
            var mechanism = new TypeLibrary<IRuntimeTypeFactoryArtifact>(backend);
            var factoryUnderTest = new EqualityComparerTypeFactory(mechanism);

            return factoryUnderTest;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public struct ASimplestClass
        {
            public string StringValue { get; set; }
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public struct AFlatStructWithFields
        {
            public int Number;
            public string Text;
            public DateTime TimeStamp;
            public DayOfWeek Day;
        }
    }
}
