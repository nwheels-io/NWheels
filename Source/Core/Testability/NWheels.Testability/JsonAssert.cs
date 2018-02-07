using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace NWheels.Testability
{
    public static class JsonAssert
    {
        public static void AreEqual(string actual, string expected, string because = "", params object[] becauseArgs)
        {
            JToken actualDeserialized = JToken.Parse(actual);
            JToken expectedDeserialized = JToken.Parse(expected);

            actualDeserialized = NormalizeJToken(actualDeserialized);
            expectedDeserialized = NormalizeJToken(expectedDeserialized);

            string actualNormalized = JsonConvert.SerializeObject(actualDeserialized, Formatting.None);
            string expectedNormalized = JsonConvert.SerializeObject(expectedDeserialized, Formatting.None);

            actualNormalized.Should().Be(expectedNormalized, because, becauseArgs);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static JToken NormalizeJToken(JToken token)
        {
            if (token is JObject obj)
            {
                return NormalizeJObject(obj);
            }
            else if (token is JArray array)
            {
                return NormalizeJArray(array);
            }
            else
            {
                return token;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static JObject NormalizeJObject(JObject original)
        {
            var result = new JObject();

            foreach (var property in original.Properties().OrderBy(p => p.Name))
            {
                result.Add(property.Name, NormalizeJToken(property.Value));
            }

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static JArray NormalizeJArray(JArray array)
        {
            var result = new JArray();

            foreach (JToken element in array)
            {
                result.Add(NormalizeJToken(element));
            }

            return result;
        }
    }
}
