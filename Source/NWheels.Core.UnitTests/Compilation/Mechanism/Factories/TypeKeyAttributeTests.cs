using FluentAssertions;
using NWheels.Compilation.Mechanism.Factories;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace NWheels.Core.UnitTests.Compilation.Mechanism.Factories
{
    public class TypeKeyAttributeTests
    {
        [Fact]
        public void CanRetrieveAttributeValues()
        {
            //-- arrange & act

            var attributeUnderTest = typeof(TestTarget).GetTypeInfo().GetCustomAttribute<TypeKeyAttribute>();

            //-- assert

            attributeUnderTest.Should().NotBeNull();
            attributeUnderTest.FactoryType.Should().BeSameAs(typeof(TypeKeyAttributeTests));

            attributeUnderTest.PrimaryContract.Should().BeSameAs(typeof(TestContractOne));

            attributeUnderTest.SecondaryContracts.Count.Should().Be(2);
            attributeUnderTest.SecondaryContracts[0].Should().BeSameAs(typeof(TestContractTwo));
            attributeUnderTest.SecondaryContracts[1].Should().BeSameAs(typeof(TestContractThree));

            attributeUnderTest.ExtensionType.Should().BeSameAs(typeof(TestTypeKeyExtension));

            attributeUnderTest.ExtensionValues.Count.Should().Be(3);
            attributeUnderTest.ExtensionValues[0].Should().Be(123);
            attributeUnderTest.ExtensionValues[1].Should().Be("ABC");
            attributeUnderTest.ExtensionValues[2].Should().BeSameAs(typeof(AnotherTestObject));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanDeserializeTypeKeyExtension()
        {
            //-- arrange

            var attributeUnderTest = typeof(TestTarget).GetTypeInfo().GetCustomAttribute<TypeKeyAttribute>();

            //-- act

            var extension = attributeUnderTest.DeserializeTypeKeyExtension<TestTypeKeyExtension>();

            //-- assert

            extension.Should().NotBeNull();
            extension.IntValue.Should().Be(123);
            extension.StringValue.Should().Be("ABC");
            extension.TypeValue.Should().BeSameAs(typeof(AnotherTestObject));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanSerializeTypeKeyExtension()
        {
            //-- arrange & act

            var extension = new TestTypeKeyExtension() {
                IntValue = 987,
                StringValue = "XYZ",
                TypeValue = typeof(Decimal)
            };

            //-- act

            var values = TypeKeyAttribute.SerializeTypeKeyExtension(extension);

            //-- assert

            values.Should().Equal(987, "XYZ", typeof(Decimal));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TypeKey(
            factoryType: typeof(TypeKeyAttributeTests),
            primaryContract: typeof(TestContractOne),
            secondaryContracts: new Type[] { typeof(TestContractTwo), typeof(TestContractThree) },
            extensionType: typeof(TestTypeKeyExtension),
            extensionValues: new object[] { 123, "ABC", typeof(AnotherTestObject) })]
        public class TestTarget
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestFactory { }
        public interface TestContractOne {  }
        public interface TestContractTwo { }
        public interface TestContractThree { }
        public class AnotherTestObject { }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public struct TestTypeKeyExtension : ITypeKeyExtension
        {
            public TestTypeKeyExtension(string s, int n, Type t)
            {
                StringValue = s;
                IntValue = n;
                TypeValue = t;
            }

            public int IntValue;
            public string StringValue;
            public Type TypeValue;

            object[] ITypeKeyExtension.Serialize()
            {
                return new object[] { IntValue, StringValue, TypeValue };
            }

            void ITypeKeyExtension.Deserialize(object[] values)
            {
                IntValue = (int)values[0];
                StringValue = (string)values[1];
                TypeValue = (Type)values[2];
            }
        }
    }
}
