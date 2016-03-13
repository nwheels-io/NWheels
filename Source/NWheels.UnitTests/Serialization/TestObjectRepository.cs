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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AClassWithStructs
        {
            public PrimitiveStruct One { get; set; }
            public AnotherPrimitiveStruct Two { get; set; }
            public NonPrimitiveStruct Three { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public struct PrimitiveStruct
        {
            public PrimitiveStruct(
                int intValue, 
                bool boolValue, 
                bool anotherBoolValue, 
                string stringValue, 
                string anotherStringValue, 
                DayOfWeek systemEnumValue, 
                TimeSpan timeSpanValue)
            {
                IntValue = intValue;
                BoolValue = boolValue;
                AnotherBoolValue = anotherBoolValue;
                StringValue = stringValue;
                AnotherStringValue = anotherStringValue;
                SystemEnumValue = systemEnumValue;
                TimeSpanValue = timeSpanValue;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public readonly int IntValue;
            public readonly bool BoolValue;
            public readonly bool AnotherBoolValue;
            public readonly string StringValue;
            public readonly string AnotherStringValue;
            public readonly DayOfWeek SystemEnumValue;
            public readonly TimeSpan TimeSpanValue;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public struct AnotherPrimitiveStruct
        {
            public AnotherPrimitiveStruct(string stringValue)
            {
                StringValue = stringValue;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public readonly string StringValue;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public struct NonPrimitiveStruct
        {
            private readonly Primitive _first;
            private readonly PrimitiveStruct _second;
            private readonly Primitive _third;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public NonPrimitiveStruct(Primitive first, PrimitiveStruct second, Primitive third)
            {
                _first = first;
                _second = second;
                _third = third;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Primitive First { get { return _first; } }
            public PrimitiveStruct Second { get { return _second; } }
            public Primitive Third { get { return _third; } }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class WithPolymorphicObjects
        {
            public BaseClass Base { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class BaseClass
        {
            public string StringValue { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class DerivedClassOne : BaseClass
        {
            public int IntValue { get; set; }
            public TimeSpan TimeSpanValue { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class DerivedClassTwo : BaseClass
        {
            public long LongValue { get; set; }
            public DateTime DateTimeValue { get; set; }
        }
    }
}
