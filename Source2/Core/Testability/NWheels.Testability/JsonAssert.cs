using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;

namespace NWheels.Testability
{
    public static class JsonAssert
    {
        public static void AreEqual(string actual, string expected, string because = "", params object[] becauseArgs)
        {
            dynamic actualDeserialized = JsonConvert.DeserializeObject(actual);
            dynamic expectedDeserialized = JsonConvert.DeserializeObject(expected);

            string actualNormalized = JsonConvert.SerializeObject(actualDeserialized, Formatting.None);
            string expectedNormalized = JsonConvert.SerializeObject(expectedDeserialized, Formatting.None);

            actualNormalized.Should().Be(expectedNormalized, because, becauseArgs);
        }
    }
}
