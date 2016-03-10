using System;
using System.Collections.Generic;

namespace NWheels.UnitTests.Serialization
{
    public static class TestObjectRepository
    {
        public class Primitive
        {
            public int IntValue { get; set; }
            public bool BoolValue { get; set; }
            public bool AnotherBoolValue { get; set; }
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

        public class AnotherPrimitive
        {
            public string StringValue { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class WithNestedObjects
        {
            public Primitive First { get; set; }
            public AnotherPrimitive Second { get; set; }
            public Primitive Third { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class WithCollectionsOfPrimitiveTypes
        {
            public AnAppEnum[] EnumArray { get; set; }
            public List<string> StringList { get; set; }
            public HashSet<int> IntHashSet { get; set; }
            public Dictionary<int, string> IntStringDictionary { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class WithCollectionsOfNestedObjects
        {
            public Primitive[] FirstArray { get; set; }
            public List<AnotherPrimitive> SecondList { get; set; }
            public Dictionary<AnotherPrimitive, Primitive> PrimitiveByAnother { get; set; }
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
