using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using FluentAssertions.Primitives;

namespace NWheels.Testability.Extensions
{
    public static class StringAssertionsExtensions
    {
        public static AndConstraint<StringAssertions> BeJson(this StringAssertions assertions, string expectedJson, string because = "", params object[] becauseArgs)
        {
            JsonAssert.AreEqual(assertions.Subject, expectedJson, because, becauseArgs);
            return new AndConstraint<StringAssertions>(assertions);
        }
    }
}
