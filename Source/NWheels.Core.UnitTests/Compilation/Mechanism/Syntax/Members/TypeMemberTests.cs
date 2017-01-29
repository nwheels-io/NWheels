using FluentAssertions;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NWheels.Core.UnitTests.Compilation.Mechanism.Syntax.Members
{
    public class TypeMemberTests
    {
        [Theory]
        [InlineData(typeof(string), typeof(string), true)]
        [InlineData(typeof(string), typeof(int), false)]
        [InlineData(typeof(string), null, false)]
        [InlineData(null, typeof(string), false)]
        public void Equals_WithClrBinding_ComparedByBinding(Type binding1, Type binding2, bool expectedResult)
        {
            //-- arrange

            var typeMember1 = (binding1 != null ? new TypeMember(binding1) : new TypeMember());
            var typeMember2 = (binding2 != null ? new TypeMember(binding2) : new TypeMember());

            //-- act

            var actualResult1 = typeMember1.Equals(typeMember2);
            var actualResult2 = typeMember2.Equals(typeMember1);

            //-- assert

            actualResult1.Should().Be(expectedResult);
            actualResult2.Should().Be(expectedResult);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Theory]
        [InlineData(typeof(string), typeof(string), true)]
        [InlineData(typeof(string), typeof(int), false)]
        [InlineData(typeof(string), null, false)]
        [InlineData(null, typeof(string), false)]
        public void OperatorEquals_WithClrBinding_ComparedByBinding(Type binding1, Type binding2, bool expectedResult)
        {
            //-- arrange

            var typeMember1 = (binding1 != null ? new TypeMember(binding1) : new TypeMember());
            var typeMember2 = (binding2 != null ? new TypeMember(binding2) : new TypeMember());

            //-- act

            var actualResult1 = (typeMember1 == typeMember2);
            var actualResult2 = (typeMember2 == typeMember1);

            //-- assert

            actualResult1.Should().Be(expectedResult);
            actualResult2.Should().Be(expectedResult);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void GetHashCode_EqualClrBindings_MustBeEqual()
        {
            //-- arrange

            var typeMember1 = new TypeMember(typeof(IFormatProvider));
            var typeMember2 = new TypeMember(typeof(IFormatProvider));

            //-- act

            var hash1 = typeMember1.GetHashCode();
            var hash2 = typeMember2.GetHashCode();

            //-- assert

            hash2.Should().Be(hash1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static IEnumerable<object[]> TestCases_TestFullName = new object[][] {
            #region Test cases
            new object[] {
                new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassOne"),
                "ClassOne"
            },
            new object[] {
                new TypeMember("NS1.NS2", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne"),
                "NS1.NS2.ClassOne"
            },
            new object[] {
                new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "NestedOne") {
                    DeclaringType = new TypeMember("NS1.NS2", MemberVisibility.Public, TypeMemberKind.Class, "ClassOne")
                },
                "NS1.NS2.ClassOne.NestedOne"
            },
            new object[] {
                new TypeMember(typeof(System.IFormattable)),
                "System.IFormattable"
            },
            new object[] {
                new TypeMember(typeof(TestNestedType)),
                "NWheels.Core.UnitTests.Compilation.Mechanism.Syntax.Members.TypeMemberTests.TestNestedType"
            },
            #endregion
        };

        [Theory]
        [MemberData(nameof(TestCases_TestFullName))]
        public void TestFullName(TypeMember typeUnderTest, string expectedFullName)
        {
            //-- act & assert

            typeUnderTest.FullName.Should().Be(expectedFullName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestNestedType
        {
        }
    }
}
