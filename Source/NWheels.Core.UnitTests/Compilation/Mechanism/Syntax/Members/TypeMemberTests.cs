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
    }
}
