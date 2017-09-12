using FluentAssertions;
using NWheels.Kernel.Api.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NWheels.Kernel.UnitTests.Api.Extensions
{
    public class TypeExtensionsTests
    {
        [Theory]
        [InlineData(typeof(TypeExtensionsTests), "NWheels.Kernel.UnitTests")]
        public void TestAssemblyShortName(Type type, string expected)
        {
            //-- act

            var actual = type.AssemblyShortName();

            //-- assert

            actual.Should().Be(expected);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Theory]
        [InlineData(typeof(string), "String")]
        [InlineData(typeof(string[]), "String[]")]
        [InlineData(typeof(List<string>), "List<String>")]
        [InlineData(typeof(Dictionary<int, string>), "Dictionary<Int32,String>")]
        [InlineData(typeof(List<Dictionary<int, string>>), "List<Dictionary<Int32,String>>")]
        [InlineData(typeof(Dictionary<List<DayOfWeek>, List<Dictionary<int, string>>>), "Dictionary<List<DayOfWeek>,List<Dictionary<Int32,String>>>")]
        [InlineData(typeof(AnOuterGenericType<string>.AnInnerType), "TypeExtensionsTests.AnOuterGenericType<String>.AnInnerType")]
        [InlineData(typeof(AnOuterGenericType<string>.AGenericInnerType<int>), "TypeExtensionsTests.AnOuterGenericType<String>.AGenericInnerType<Int32>")]
        public void TestFriendlyName(Type type, string expected)
        {
            //-- act

            var actual = type.FriendlyName();

            //-- assert

            actual.Should().Be(expected);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class AnOuterGenericType<T>
        {
            public class AnInnerType
            {
            }
            public class AGenericInnerType<S>
            {
            }
        }
    }
}
