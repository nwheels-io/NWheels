using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NWheels.UI.Core;
using NWheels.Utilities;

namespace NWheels.Domains.Security.UI
{
    public class JsonSerializationExtension : IJsonSerializationExtension
    {
        #region Implementation of IJsonSerializationExtension

        public void ApplyTo(JsonSerializerSettings settings)
        {
            settings.Converters.Add(new SecureStringConverter());
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class SecureStringConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var secureString = value as SecureString;

                if ( secureString != null )
                {
                    writer.WriteValue(secureString.SecureToClear());
                }
                else
                {
                    writer.WriteNull();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if ( reader.TokenType == JsonToken.Null || reader.Value == null )
                {
                    return null;
                }

                if ( reader.TokenType == JsonToken.String )
                {
                    return reader.Value.ToString().ClearToSecure();
                }

                throw new NotSupportedException("Input token in not supported");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(SecureString));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanRead
            {
                get
                {
                    return true;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override bool CanWrite 
            {
                get
                {
                    return true;
                }
            }
        }
    }
}
