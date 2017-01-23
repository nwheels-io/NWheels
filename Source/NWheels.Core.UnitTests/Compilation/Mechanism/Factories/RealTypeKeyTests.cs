using FluentAssertions;
using NWheels.Compilation.Mechanism.Factories;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NWheels.Core.UnitTests.Compilation.Mechanism.Factories
{
    public class RealTypeKeyTests
    {
        [Fact]
        public void CanCreateTypeKeyWithEmptyExtension()
        {
            //-- arrange & act

            var keyUnderTest = new RealTypeKey<Empty.KeyExtension>(
                primaryContract: typeof(InterfaceA),
                secondaryContracts: new TypeMember[] { typeof(InterfaceB), typeof(InterfaceC) },
                extension: null);

            //-- assert

            keyUnderTest.PrimaryContract.Should().Be(new TypeMember(typeof(InterfaceA)));
            keyUnderTest.SecondaryContracts.Should().Equal(new TypeMember[] { typeof(InterfaceB), typeof(InterfaceC) });
            keyUnderTest.Extension.Should().BeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanCreateTypeKeyWithCustomExtension()
        {
            //-- arrange

            var extension = new TestExtension(data: "ABC");

            //-- act

            var keyUnderTest = new RealTypeKey<TestExtension>(
                primaryContract: typeof(InterfaceA),
                secondaryContracts: new TypeMember[] { typeof(InterfaceB), typeof(InterfaceC) },
                extension: extension);

            //-- assert

            keyUnderTest.PrimaryContract.Should().Be(new TypeMember(typeof(InterfaceA)));
            keyUnderTest.SecondaryContracts.Should().Equal(new TypeMember[] { typeof(InterfaceB), typeof(InterfaceC) });
            keyUnderTest.Extension.Should().BeSameAs(extension);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<object[]> TestCases_CanTestTypeKeysEquality = new object[][] {
            #region Test Cases
            new object[] {
                new RealTypeKey<Empty.KeyExtension>(null, null, null),
                new RealTypeKey<Empty.KeyExtension>(null, null, null),
                true
            },
            new object[] {
                new RealTypeKey<Empty.KeyExtension>(typeof(string), null, null),
                new RealTypeKey<Empty.KeyExtension>(typeof(string), null, null),
                true
            },
            new object[] {
                new RealTypeKey<Empty.KeyExtension>(typeof(string), null, null),
                new RealTypeKey<Empty.KeyExtension>(typeof(int), null, null),
                false
            },
            new object[] {
                new RealTypeKey<Empty.KeyExtension>(typeof(string), null, null),
                new RealTypeKey<TestExtension>(typeof(string), null, null),
                false
            },
            new object[] {
                new RealTypeKey<Empty.KeyExtension>(null, new TypeMember[] { typeof(string), typeof(int) }, null),
                new RealTypeKey<Empty.KeyExtension>(null, new TypeMember[] { typeof(string), typeof(int) }, null),
                true
            },
            new object[] {
                new RealTypeKey<Empty.KeyExtension>(null, new TypeMember[] { typeof(string), typeof(int) }, null),
                new RealTypeKey<Empty.KeyExtension>(null, new TypeMember[] { typeof(string) }, null),
                false
            },
            new object[] {
                new RealTypeKey<Empty.KeyExtension>(null, new TypeMember[] { typeof(string), typeof(int) }, null),
                new RealTypeKey<Empty.KeyExtension>(null, new TypeMember[] { typeof(int), typeof(string) }, null),
                false
            },
            new object[] {
                new RealTypeKey<Empty.KeyExtension>(null, new TypeMember[] { typeof(string), typeof(int) }, null),
                new RealTypeKey<Empty.KeyExtension>(null, new TypeMember[] { typeof(string) }, null),
                false
            },
            new object[] {
                new RealTypeKey<Empty.KeyExtension>(null, new TypeMember[] { typeof(string), typeof(int) }, null),
                new RealTypeKey<Empty.KeyExtension>(null, null, null),
                false
            },
            new object[] {
                new RealTypeKey<Empty.KeyExtension>(null, null, new Empty.KeyExtension()),
                new RealTypeKey<Empty.KeyExtension>(null, null, new Empty.KeyExtension()),
                true
            },
            new object[] {
                new RealTypeKey<Empty.KeyExtension>(null, null, new Empty.KeyExtension()),
                new RealTypeKey<Empty.KeyExtension>(null, null, null),
                false
            },
            new object[] {
                new RealTypeKey<TestExtension>(null, null, new TestExtension("ABC")),
                new RealTypeKey<TestExtension>(null, null, new TestExtension("ABC")),
                true
            },
            new object[] {
                new RealTypeKey<TestExtension>(null, null, new TestExtension("ABC")),
                new RealTypeKey<TestExtension>(null, null, new TestExtension("DEF")),
                false
            },
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

            var hash1 = key1?.GetHashCode();
            var hash2 = key2?.GetHashCode();

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

        public class TestExtension : ITypeKeyExtension
        {
            public TestExtension()
            {
            }

            public TestExtension(string data)
            {
                this.Data = data;
            }

            public string Data { get; private set; }

            public void Deserialize(object[] values)
            {
                Data = (string)values[0];
            }

            public object[] Serialize()
            {
                return new object[] { Data };
            }

            public override bool Equals(object obj)
            {
                if (obj is TestExtension other)
                {
                    return other.Data == this.Data;
                }
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return (Data?.GetHashCode()).GetValueOrDefault();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface InterfaceA { }
        public interface InterfaceB { }
        public interface InterfaceC { }
    }
}
