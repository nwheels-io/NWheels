using FluentAssertions;
using NWheels.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NWheels.Implementation.UnitTests.Extensions
{
    public class TypeExtensionTests
    {
        [Theory]
        [InlineData(typeof(TypeExtensionTests), "NWheels.Implementation.UnitTests")]
        public void TestAssemblyShortName(Type type, string expected)
        {
            //-- act

            var actual = type.AssemblyShortName();

            //-- assert

            actual.Should().Be(expected);
        }
    }
}
