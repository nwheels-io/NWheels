using FluentAssertions;
using NWheels.Compilation.Mechanism.Factories;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NWheels.Implementation.UnitTests.Compilation.Mechanism.Factories
{
    public class RealTypeKeyTests
    {
        [Fact]
        public void CanCreateTypeKey()
        {
            //-- act

            var keyUnderTest = new TypeKey(
                factoryType: this.GetType(),
                primaryContract: typeof(InterfaceA),
                secondaryContract1: typeof(InterfaceB),
                secondaryContract2: typeof(InterfaceC),
                secondaryContract3: typeof(InterfaceD),
                extensionValue1: 123,
                extensionValue2: 456,
                extensionValue3: 789);

            //-- assert

            keyUnderTest.FactoryType.Should().BeSameAs(this.GetType());
            keyUnderTest.PrimaryContract.Should().Be(new TypeMember(typeof(InterfaceA)));
            keyUnderTest.SecondaryContract1.Should().Be(new TypeMember(typeof(InterfaceB)));
            keyUnderTest.SecondaryContract2.Should().Be(new TypeMember(typeof(InterfaceC)));
            keyUnderTest.SecondaryContract3.Should().Be(new TypeMember(typeof(InterfaceD)));
            keyUnderTest.ExtensionValue1.Should().Be(123);
            keyUnderTest.ExtensionValue2.Should().Be(456);
            keyUnderTest.ExtensionValue3.Should().Be(789);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<object[]> TestCases_CanTestTypeKeysEquality = new object[][] {
            #region Test Cases
            new object[] {
                new TypeKey(typeof(TestFactory), null, null, null, null, 0, 0, 0),
                new TypeKey(typeof(TestFactory), null, null, null, null, 0, 0, 0),
                true
            },
            new object[] {
                new TypeKey(typeof(TestFactory),    null, null, null, null, 0, 0, 0),
                new TypeKey(typeof(TestFactoryTwo), null, null, null, null, 0, 0, 0),
                false
            },
            new object[] {
                new TypeKey(typeof(TestFactory), typeof(InterfaceA), typeof(InterfaceB), typeof(InterfaceC), typeof(InterfaceD), 1, 2, 3),
                new TypeKey(typeof(TestFactory), typeof(InterfaceA), typeof(InterfaceB), typeof(InterfaceC), typeof(InterfaceD), 1, 2, 3),
                true
            },
            new object[] {
                new TypeKey(typeof(TestFactory), typeof(InterfaceA), null, null, null, 0, 0, 0),
                new TypeKey(typeof(TestFactory), typeof(InterfaceA), null, null, null, 0, 0, 0),
                true
            },
            new object[] {
                new TypeKey(typeof(TestFactory), typeof(InterfaceA), null, null, null, 0, 0, 0),
                new TypeKey(typeof(TestFactory), null,               null, null, null, 0, 0, 0),
                false
            },
            new object[] {
                new TypeKey(typeof(TestFactory), typeof(InterfaceA), null, null, null, 0, 0, 0),
                new TypeKey(typeof(TestFactory), typeof(InterfaceB), null, null, null, 0, 0, 0),
                false
            },
            new object[] {
                new TypeKey(typeof(TestFactory), null, typeof(InterfaceA), null, null, 0, 0, 0),
                new TypeKey(typeof(TestFactory), null, typeof(InterfaceA), null, null, 0, 0, 0),
                true
            },
            new object[] {
                new TypeKey(typeof(TestFactory), null, typeof(InterfaceA), null, null, 0, 0, 0),
                new TypeKey(typeof(TestFactory), null, null,               null, null, 0, 0, 0),
                false
            },
            new object[] {
                new TypeKey(typeof(TestFactory), null, typeof(InterfaceA), null, null, 0, 0, 0),
                new TypeKey(typeof(TestFactory), null, typeof(InterfaceB), null, null, 0, 0, 0),
                false
            },
            new object[] {
                new TypeKey(typeof(TestFactory), null, null, typeof(InterfaceA), null, 0, 0, 0),
                new TypeKey(typeof(TestFactory), null, null, typeof(InterfaceA), null, 0, 0, 0),
                true
            },
            new object[] {
                new TypeKey(typeof(TestFactory), null, null, typeof(InterfaceA), null, 0, 0, 0),
                new TypeKey(typeof(TestFactory), null, null, null,               null, 0, 0, 0),
                false
            },
            new object[] {
                new TypeKey(typeof(TestFactory), null, null, typeof(InterfaceA), null, 0, 0, 0),
                new TypeKey(typeof(TestFactory), null, null, typeof(InterfaceB), null, 0, 0, 0),
                false
            },
            new object[] {
                new TypeKey(typeof(TestFactory), null, null, null, typeof(InterfaceA),  0, 0, 0),
                new TypeKey(typeof(TestFactory), null, null, null, typeof(InterfaceA),  0, 0, 0),
                true
            },
            new object[] {
                new TypeKey(typeof(TestFactory), null, null, null, typeof(InterfaceA), 0, 0, 0),
                new TypeKey(typeof(TestFactory), null, null, null, null,               0, 0, 0),
                false
            },
            new object[] {
                new TypeKey(typeof(TestFactory), null, null, null, typeof(InterfaceA), 0, 0, 0),
                new TypeKey(typeof(TestFactory), null, null, null, typeof(InterfaceB), 0, 0, 0),
                false
            },
            new object[] {
                new TypeKey(typeof(TestFactory), null, null, null, null, 1, 0, 0),
                new TypeKey(typeof(TestFactory), null, null, null, null, 1, 0, 0),
                true
            },
            new object[] {
                new TypeKey(typeof(TestFactory), null, null, null, null, 1, 0, 0),
                new TypeKey(typeof(TestFactory), null, null, null, null, 0, 0, 0),
                false
            },
            new object[] {
                new TypeKey(typeof(TestFactory), null, null, null, null, 0, 1, 0),
                new TypeKey(typeof(TestFactory), null, null, null, null, 0, 1, 0),
                true
            },
            new object[] {
                new TypeKey(typeof(TestFactory), null, null, null, null, 0, 1, 0),
                new TypeKey(typeof(TestFactory), null, null, null, null, 0, 0, 0),
                false
            },
            new object[] {
                new TypeKey(typeof(TestFactory), null, null, null, null, 0, 0, 1),
                new TypeKey(typeof(TestFactory), null, null, null, null, 0, 0, 1),
                true
            },
            new object[] {
                new TypeKey(typeof(TestFactory), null, null, null, null, 0, 0, 1),
                new TypeKey(typeof(TestFactory), null, null, null, null, 0, 0, 0),
                false
            }
            #endregion
        };

        [Theory]
        [MemberData(nameof(TestCases_CanTestTypeKeysEquality))]
        public void CanTestTypeKeysEquality(TypeKey key1, TypeKey key2, bool expectedResult)
        {
            //-- act

            var actualResult1 = key1.Equals(key2);
            var actualResult2 = key2.Equals(key1);
            var actualResult1Op = (key1 == key2);
            var actualResult2Op = (key2 == key1);
            var actualResult1NotOp = (key1 != key2);
            var actualResult2NotOp = (key2 != key1);

            var hash1 = key1.GetHashCode();
            var hash2 = key2.GetHashCode();

            //-- assert

            actualResult1.Should().Be(expectedResult);
            actualResult2.Should().Be(expectedResult);
            actualResult1Op.Should().Be(expectedResult);
            actualResult2Op.Should().Be(expectedResult);
            actualResult1NotOp.Should().Be(!expectedResult);
            actualResult2NotOp.Should().Be(!expectedResult);

            if (expectedResult)
            {
                hash2.Should().Be(hash1);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface InterfaceA { }
        public interface InterfaceB { }
        public interface InterfaceC { }
        public interface InterfaceD { }
        public interface InterfaceX { }
        public class TestFactory { }
        public class TestFactoryTwo { }
    }
}
