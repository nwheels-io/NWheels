using FluentAssertions;
using NWheels.Kernel.Api.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using NWheels.Testability;

namespace NWheels.Kernel.UnitTests.Api.Extensions
{
    public class TypeExtensionsTests : TestBase.UnitTest
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

        [Theory]
        [InlineData(
            typeof(string), false,
            "System.String")]
        [InlineData(
            typeof(string), true,
            "System.String")]
        [InlineData(
            typeof(string[]), false,
            "System.String[]")]
        [InlineData(
            typeof(string[]), true,
            "System.String[]")]
        [InlineData(
            typeof(List<string>), false,
            "System.Collections.Generic.List<String>")]
        [InlineData(
            typeof(List<string>), true,
            "System.Collections.Generic.List<System.String>")]
        [InlineData(
            typeof(Dictionary<int, string>), false,
            "System.Collections.Generic.Dictionary<Int32,String>")]
        [InlineData(
            typeof(Dictionary<int, string>), true,
            "System.Collections.Generic.Dictionary<System.Int32,System.String>")]
        [InlineData(
            typeof(List<Dictionary<int, string>>), false,
            "System.Collections.Generic.List<Dictionary<Int32,String>>")]
        [InlineData(
            typeof(List<Dictionary<int, string>>), true,
            "System.Collections.Generic.List<System.Collections.Generic.Dictionary<System.Int32,System.String>>")]
        [InlineData(
            typeof(Dictionary<List<DayOfWeek>, List<Dictionary<int, string>>>), false,
            "System.Collections.Generic.Dictionary<List<DayOfWeek>,List<Dictionary<Int32,String>>>")]
        [InlineData(
            typeof(Dictionary<List<DayOfWeek>, List<Dictionary<int, string>>>), true,
            "System.Collections.Generic.Dictionary<System.Collections.Generic.List<System.DayOfWeek>,System.Collections.Generic.List<System.Collections.Generic.Dictionary<System.Int32,System.String>>>")]
        [InlineData(
            typeof(AnOuterGenericType<string>.AnInnerType), false,
            "NWheels.Kernel.UnitTests.Api.Extensions.TypeExtensionsTests.AnOuterGenericType<String>.AnInnerType")]
        [InlineData(
            typeof(AnOuterGenericType<string>.AnInnerType), true,
            "NWheels.Kernel.UnitTests.Api.Extensions.TypeExtensionsTests.AnOuterGenericType<System.String>.AnInnerType")]
        [InlineData(
            typeof(AnOuterGenericType<string>.AGenericInnerType<int>), false,
            "NWheels.Kernel.UnitTests.Api.Extensions.TypeExtensionsTests.AnOuterGenericType<String>.AGenericInnerType<Int32>")]
        [InlineData(
            typeof(AnOuterGenericType<string>.AGenericInnerType<int>), true,
            "NWheels.Kernel.UnitTests.Api.Extensions.TypeExtensionsTests.AnOuterGenericType<System.String>.AGenericInnerType<System.Int32>")]
        public void TestFriendlyFullName(Type type, bool fullGenericArgNames, string expected)
        {
            //-- act

            var actual = type.FriendlyFullName(fullGenericArgNames);

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
