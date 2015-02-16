using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NWheels.Logging;

namespace NWheels.Core.UnitTests.Logging
{
    [TestFixture]
    public class LogNameValuePairTests
    {
        [Test]
        public void CanFormatValueType()
        {
            //-- Arrange

            var withoutFormat = new LogNameValuePair<int> {
                Value = 12345,
            };

            var withFormat = new LogNameValuePair<int> {
                Value = 12345,
                Format = "#,###"
            };

            var nonFormattable = new LogNameValuePair<NonFormattableStruct>();

            //-- Act

            var formattedWithoutFormat = withoutFormat.FormatValue();
            var formattedWithFormat = withFormat.FormatValue();
            var formattedNonFormattable = nonFormattable.FormatValue();

            //-- Assert

            Assert.That(formattedWithoutFormat, Is.EqualTo("12345"));
            Assert.That(formattedWithFormat, Is.EqualTo("12,345"));
            Assert.That(formattedNonFormattable, Is.EqualTo("IAmNonFormattableStruct"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Test]
        public void CanFormatReferenceType()
        {
            //-- Arrange

            var withValue = new LogNameValuePair<string> {
                Value = "ABC",
            };

            var withNull = new LogNameValuePair<string> {
                Value = null
            };

            //-- Act

            var formattedWithValue = withValue.FormatValue();
            var formattedWithNull = withNull.FormatValue();

            //-- Assert

            Assert.That(formattedWithValue, Is.EqualTo("ABC"));
            Assert.That(formattedWithNull, Is.EqualTo("null"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private struct NonFormattableStruct
        {
            public override string ToString()
            {
                return "IAmNonFormattableStruct";
            }
        }
    }
}
