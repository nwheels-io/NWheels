using FluentAssertions;
using NWheels.Compilation.Mechanism.Factories;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace NWheels.Implementation.UnitTests.Compilation.Mechanism.Factories
{
    public class TypeKeyAttributeTests
    {
        [Fact]
        public void CanRetrieveCompiledAttributeValues()
        {
            //-- arrange & act

            var attributeUnderTest = typeof(TestTarget).GetTypeInfo().GetCustomAttribute<TypeKeyAttribute>();

            //-- assert

            attributeUnderTest.Should().NotBeNull();
            attributeUnderTest.FactoryType.Should().BeSameAs(typeof(TypeKeyAttributeTests));

            attributeUnderTest.PrimaryContract.Should().BeSameAs(typeof(TestContractOne));

            attributeUnderTest.SecondaryContract1.Should().BeSameAs(typeof(TestContractTwo));
            attributeUnderTest.SecondaryContract2.Should().BeSameAs(typeof(TestContractThree));
            attributeUnderTest.SecondaryContract3.Should().BeSameAs(typeof(TestContractFour));

            attributeUnderTest.ExtensionValue1.Should().Be(123);
            attributeUnderTest.ExtensionValue2.Should().Be(456);
            attributeUnderTest.ExtensionValue3.Should().Be(789);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanGetTypeKeyFromAttribute()
        {
            //-- arrange

            var attributeUnderTest = typeof(TestTarget).GetTypeInfo().GetCustomAttribute<TypeKeyAttribute>();

            //-- act

            var key = attributeUnderTest.ToTypeKey();

            //-- assert

            key.FactoryType.Should().BeSameAs(typeof(TypeKeyAttributeTests));

            key.PrimaryContract.ClrBinding.Should().BeSameAs(typeof(TestContractOne));

            key.SecondaryContract1.ClrBinding.Should().BeSameAs(typeof(TestContractTwo));
            key.SecondaryContract2.ClrBinding.Should().BeSameAs(typeof(TestContractThree));
            key.SecondaryContract3.ClrBinding.Should().BeSameAs(typeof(TestContractFour));

            key.ExtensionValue1.Should().Be(123);
            key.ExtensionValue2.Should().Be(456);
            key.ExtensionValue3.Should().Be(789);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [TypeKey(
            factoryType: typeof(TypeKeyAttributeTests),
            primaryContract: typeof(TestContractOne),
            secondaryContract1: typeof(TestContractTwo), 
            secondaryContract2: typeof(TestContractThree),
            secondaryContract3: typeof(TestContractFour),
            extensionValue1: 123,
            extensionValue2: 456,
            extensionValue3: 789)]
        public class TestTarget
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestFactory { }
        public interface TestContractOne {  }
        public interface TestContractTwo { }
        public interface TestContractThree { }
        public interface TestContractFour { }
    }
}
