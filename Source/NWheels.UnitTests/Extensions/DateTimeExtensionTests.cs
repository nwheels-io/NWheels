using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Extensions;
using Shouldly;

namespace NWheels.UnitTests.Extensions
{
    [TestFixture]
    public class DateTimeExtensionTests
    {
        [Test]
        public void MakeUtc_Test()
        {
            var utc = new DateTime(2010, 10, 10, 5, 30, 11, DateTimeKind.Local).MakeUtc();
            utc.ShouldBe(new DateTime(2010, 10, 10, 5, 30, 11, DateTimeKind.Utc));
            utc.Kind.ShouldBe(DateTimeKind.Utc);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void DateTimeEquals_KindIsIgnored()
        {
            var value1 = new DateTime(2010, 10, 10, 5, 30, 11, DateTimeKind.Utc);
            var value2 = new DateTime(2010, 10, 10, 5, 30, 11, DateTimeKind.Unspecified);
            var value3 = new DateTime(2010, 10, 10, 5, 30, 11, DateTimeKind.Local);

            value1.Equals(value2).ShouldBe(true);
            value1.Equals(value3).ShouldBe(true);
            value2.Equals(value3).ShouldBe(true);
        }
    }
}
