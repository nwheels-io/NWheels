using Xunit;
using NWheels.Api;
using NWheels.Api.Extensions;
using FluentAssertions;
using System;

namespace NWheels.Api.Tests
{
    public class YearMonthTests
    {
        [Fact]
        public void CanInitializeWithNumbers()
        {
            //- act

            var ym = new YearMonth(year: 2010, month: 10);

            //- assert

            ym.Year.Should().Be(2010);
            ym.Month.Should().Be(10);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanInitializeWithDateTime()
        {
            //- act

            var ym = new YearMonth(new DateTime(year: 2010, month: 10, day: 11));

            //- assert

            ym.Year.Should().Be(2010);
            ym.Month.Should().Be(10);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanAddMonths()
        {
            //- arrange

            var ym0 = new YearMonth(new DateTime(year: 2010, month: 10, day: 11));

            //- act

            var ym1 = ym0.AddMonths(3);
            
            //- assert

            ym1.Year.Should().Be(2011);
            ym1.Month.Should().Be(1);
        }
    }
}