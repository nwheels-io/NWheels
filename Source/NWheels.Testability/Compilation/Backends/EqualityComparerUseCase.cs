#if false

using FluentAssertions;
using NWheels.Compilation.Mechanism.Factories;
using NWheels.DataStructures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xunit;

namespace NWheels.Testability.Compilation.Backends
{
    public abstract class EqualityComparerUseCase
    {
        [Fact]
        public void CanCreateEqualityComparer()
        {
            //-- arrange

            var factoryUnderTest = CreateFactoryUnderTest();

            //-- act

            IEqualityComparer<ASimplestClass> comparer = factoryUnderTest.GetEqualityComparer<ASimplestClass>();

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

            var comparer = CreateFactoryUnderTest().GetEqualityComparer<AFlatStructWithFields>();

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

        protected abstract ITypeFactoryBackend<IRuntimeTypeFactoryArtifact> CreateTypeFactoryBackend();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private EqualityComparerTypeFactory CreateFactoryUnderTest()
        {
            var mechanism = new TypeFactoryMechanism<IRuntimeTypeFactoryArtifact>(CreateTypeFactoryBackend());
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

#endif