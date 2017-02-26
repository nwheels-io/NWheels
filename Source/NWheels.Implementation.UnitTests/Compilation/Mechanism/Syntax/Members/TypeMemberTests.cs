using FluentAssertions;
using NWheels.Compilation.Mechanism.Syntax.Members;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NWheels.Implementation.UnitTests.Compilation.Mechanism.Syntax.Members
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
                "NWheels.Implementation.UnitTests.Compilation.Mechanism.Syntax.Members.TypeMemberTests.TestNestedType"
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

        [Fact]
        public void CanInitializeFromClrArrayType()
        {
            //-- act

            TypeMember arrayOfInt = typeof(int[]);

            //-- assert

            arrayOfInt.ClrBinding.Should().BeSameAs(typeof(int[]));
            arrayOfInt.IsArray.Should().BeTrue();
            arrayOfInt.UnderlyingType.Should().NotBeNull();
            arrayOfInt.UnderlyingType.ClrBinding.Should().BeSameAs(typeof(int));

            arrayOfInt.IsAbstract.Should().BeFalse();
            arrayOfInt.IsAwaitable.Should().BeFalse();
            arrayOfInt.IsCollection.Should().BeTrue();
            arrayOfInt.IsGenericType.Should().BeFalse();
            arrayOfInt.IsGenericTypeDefinition.Should().BeFalse();
            arrayOfInt.IsNullable.Should().BeFalse();
            arrayOfInt.IsValueType.Should().BeFalse();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanInitializeFromClrConstructedGenericType()
        {
            //-- act

            TypeMember listOfInt = typeof(List<int>);

            //-- assert

            listOfInt.ClrBinding.Should().BeSameAs(typeof(List<int>));
            listOfInt.IsGenericType.Should().BeTrue();
            listOfInt.IsGenericTypeDefinition.Should().BeFalse();
            listOfInt.GenericTypeArguments.Should().NotBeNull();
            listOfInt.GenericTypeArguments.Count.Should().Be(1);
            listOfInt.GenericTypeArguments[0].ClrBinding.Should().BeSameAs(typeof(int));
            listOfInt.GenericTypeParameters.Should().NotBeNull();
            listOfInt.GenericTypeParameters.Count.Should().Be(0);

            listOfInt.IsAbstract.Should().BeFalse();
            listOfInt.IsArray.Should().BeFalse();
            listOfInt.IsAwaitable.Should().BeFalse();
            listOfInt.IsCollection.Should().BeTrue();
            listOfInt.IsNullable.Should().BeFalse();
            listOfInt.IsValueType.Should().BeFalse();

            listOfInt.UnderlyingType.Should().BeSameAs(listOfInt.GenericTypeArguments[0]);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanInitializeFromClrGenericTypeDefinition()
        {
            //-- act

            TypeMember genericList = typeof(List<>);

            //-- assert

            genericList.ClrBinding.Should().BeSameAs(typeof(List<>));
            genericList.IsGenericType.Should().BeTrue();
            genericList.IsGenericTypeDefinition.Should().BeTrue();
            genericList.GenericTypeArguments.Should().NotBeNull();
            genericList.GenericTypeArguments.Count.Should().Be(0);
            genericList.GenericTypeParameters.Should().NotBeNull();
            genericList.GenericTypeParameters.Count.Should().Be(1);
            genericList.GenericTypeParameters[0].ClrBinding.Should().BeNull();
            genericList.GenericTypeParameters[0].TypeKind.Should().Be(TypeMemberKind.GenericParameter);

            genericList.IsAbstract.Should().BeFalse();
            genericList.IsArray.Should().BeFalse();
            genericList.IsAwaitable.Should().BeFalse();
            genericList.IsCollection.Should().BeTrue();
            genericList.IsNullable.Should().BeFalse();
            genericList.IsValueType.Should().BeFalse();

            genericList.UnderlyingType.Should().BeSameAs(genericList.GenericTypeParameters[0]);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanCacheClrBoundTypeMembers()
        {
            //-- arrange

            TypeMember typeInt1 = typeof(int);
            TypeMember typeString1 = typeof(string);
            TypeMember typeArrayOfInt1 = typeof(int[]);
            TypeMember typeListOfString1 = typeof(List<string>);

            //-- act

            TypeMember typeInt2 = typeof(int);
            TypeMember typeString2 = typeof(string);
            TypeMember typeArrayOfInt2 = typeof(int[]);
            TypeMember typeListOfString2 = typeof(List<string>);

            //-- assert

            typeInt2.Should().NotBeNull();
            typeString2.Should().NotBeNull();
            typeArrayOfInt2.Should().NotBeNull();
            typeListOfString2.Should().NotBeNull();

            typeInt2.Should().BeSameAs(typeInt1);
            typeString2.Should().BeSameAs(typeString1);
            typeArrayOfInt2.Should().BeSameAs(typeArrayOfInt1);
            typeListOfString2.Should().BeSameAs(typeListOfString1);

            typeArrayOfInt1.UnderlyingType.Should().BeSameAs(typeInt1);
            typeListOfString1.UnderlyingType.Should().BeSameAs(typeString1);
            typeArrayOfInt2.UnderlyingType.Should().BeSameAs(typeInt1);
            typeListOfString2.UnderlyingType.Should().BeSameAs(typeString1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void MakeGenericTypeOfBoundTypesResultsInBoundType()
        {
            //-- arrange

            TypeMember typeInt = typeof(int);
            TypeMember typeString = typeof(string);
            TypeMember typeIDictionaryOpen = typeof(IDictionary<,>);

            //-- act

            TypeMember constructedType = typeIDictionaryOpen.MakeGenericType(typeInt, typeString);

            //-- assert

            constructedType.ClrBinding.Should().NotBeNull();
            constructedType.ClrBinding.Should().BeSameAs(typeof(IDictionary<int, string>));

            constructedType.Should().NotBeNull();
            constructedType.IsGenericType.Should().BeTrue();
            constructedType.IsGenericTypeDefinition.Should().BeFalse();
            constructedType.GenericTypeDefinition.Should().BeSameAs(typeIDictionaryOpen);
            constructedType.GenericTypeArguments.Count.Should().Be(2);
            constructedType.GenericTypeArguments[0].Should().BeSameAs(typeInt);
            constructedType.GenericTypeArguments[1].Should().BeSameAs(typeString);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void MakeGenericTypeOfMixedBoundAndGeneratedTypesResultsInGeneratedType()
        {
            //-- arrange

            TypeMember typeKey = typeof(int);
            TypeMember typeValue = new TypeMember(MemberVisibility.Public, TypeMemberKind.Class, "ClassValue");
            TypeMember typeIDictionaryOpen = typeof(IDictionary<,>);

            //-- act

            TypeMember constructedType = typeIDictionaryOpen.MakeGenericType(typeKey, typeValue);

            //-- assert

            constructedType.ClrBinding.Should().BeNull();

            constructedType.Should().NotBeNull();
            constructedType.IsGenericType.Should().BeTrue();
            constructedType.IsGenericTypeDefinition.Should().BeFalse();
            constructedType.GenericTypeDefinition.Should().BeSameAs(typeIDictionaryOpen);
            constructedType.GenericTypeArguments.Count.Should().Be(2);
            constructedType.GenericTypeArguments[0].Should().BeSameAs(typeKey);
            constructedType.GenericTypeArguments[1].Should().BeSameAs(typeValue);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TestNestedType
        {
        }
    }
}
