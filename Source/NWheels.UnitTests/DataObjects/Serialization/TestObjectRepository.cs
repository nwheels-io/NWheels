using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.UnitTests.DataObjects.Serialization
{
    public static class TestObjectRepository
    {
        public class SimpliestFlat
        {
            public int IntValue { get; set; }
            public string StringValue { get; set; }
            public string AnotherStringValue { get; set; }
            public DayOfWeek SystemEnumValue { get; set; }
            public AnAppEnum AppEnumValue { get; set; }
            public TimeSpan TimeSpanValue { get; set; }
            public DateTime DateTimeValue { get; set; }
            public Guid GuidValue { get; set; }
            public long LongValue { get; set; }
            public float FloatValue { get; set; }
            public decimal DecimalValue { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public enum AnAppEnum
        {
            First,
            Second,
            Third
        }
    }
}
