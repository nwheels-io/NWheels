using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using FluentAssertions.Primitives;

namespace NWheels.Testability.Extensions
{
    public static class StringAssertionsExtensions
    {
        public static AndConstraint<StringAssertions> BeJson(this StringAssertions assertions, string expectedJson, string because = "")
        {
            JsonAssert.AreEqual(assertions.Subject, expectedJson, because);
            return new AndConstraint<StringAssertions>(assertions);
        }
    }
}
