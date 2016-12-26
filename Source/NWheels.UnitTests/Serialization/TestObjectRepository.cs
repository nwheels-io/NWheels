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

        public class AClassWithNullables
        {
            public int? IntValue { get; set; }
            public AnAppEnum? EnumValue { get; set; }
            public AnotherPrimitiveStruct? AnotherValue { get; set; }
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class WithCollectionsOfPolymorphicObjects
        {
            public BaseClass[] FirstArray { get; set; }
            public List<BaseClass> SecondList { get; set; }
            public Dictionary<string, BaseClass> ThirdDictionary { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IApiContract
        {
            void VoidOperation();
            void VoidOperationWithParameters(int n, string s);
            //int NonVoidOperation();
            //TimeSpan NonVoidOperationWithParameters(int n, string s);
            void VoidOperationWithPolymorphicParameters(int n, BaseClass b, string s);
            //BaseClass NonVoidOperationWithPolymorphicParameters(int n, string s);
            event EventHandler EventWithNoArgs;
            event EventHandler<BaseEventArgs> EventWithBaseArgs;
            event Action CustomDelegateEventWithNoArgs;
            event Action<BaseEventArgs> CustomDelegateEventWithBaseArgs;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class BaseEventArgs
        {
            public int IntValue { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class WithEntityObjects
        {
            public List<IEntityA> TheA { get; set; }
            public IEntityB TheB { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IEntityA
        {
            int Id { get; set; }
            string Name { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IEntityB
        {
            string Id { get; set; }
            IEntityA TheA { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IEntityAOne : IEntityA
        {
            int IntValue { get; set; }
            TimeSpan TimeValue { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IEntityATwo : IEntityA
        {
            DateTime DateValue { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ServerEntityAOne : IEntityAOne
        {
            #region Implementation of IEntityA

            public int Id { get; set; }
            public string Name { get; set; }

            #endregion

            #region Implementation of IEntityAOne

            public int IntValue { get; set; }
            public TimeSpan TimeValue { get; set; }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ServerEntityATwo : IEntityATwo
        {
            #region Implementation of IEntityA

            public int Id { get; set; }
            public string Name { get; set; }

            #endregion

            #region Implementation of IEntityATwo

            public DateTime DateValue { get; set; }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ServerEntityB : IEntityB
        {
            #region Implementation of IEntityB

            public string Id { get; set; }
            public IEntityA TheA { get; set; }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ClientEntityAOne : IEntityAOne
        {
            #region Implementation of IEntityA

            public int Id { get; set; }
            public string Name { get; set; }

            #endregion

            #region Implementation of IEntityAOne

            public int IntValue { get; set; }
            public TimeSpan TimeValue { get; set; }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ClientEntityATwo : IEntityATwo
        {
            #region Implementation of IEntityA

            public int Id { get; set; }
            public string Name { get; set; }

            #endregion

            #region Implementation of IEntityATwo

            public DateTime DateValue { get; set; }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ClientEntityB : IEntityB
        {
            #region Implementation of IEntityB

            public string Id { get; set; }
            public IEntityA TheA { get; set; }

            #endregion
        }
    }
}
