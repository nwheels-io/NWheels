using System.Runtime.Serialization;
using Nancy;
using Nancy.IO;
using Nancy.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Hapil.Members;
using Nancy.Responses;
using NWheels.Extensions;
using NWheels.UI.Uidl;

namespace NWheels.Stacks.NancyFx
{
    public class MetadataJsonSerializer : ISerializer
    {
        public bool CanSerialize(string contentType)
        {
            return true;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void Serialize<TModel>(string contentType, TModel model, Stream outputStream)
        {
            using ( StreamWriter writer = new StreamWriter(new UnclosableStreamWrapper(outputStream)) )
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer(
                    null, false, Int32.MaxValue, Int32.MaxValue, false, true);

                serializer.RegisterConverters(
                    new JavaScriptConverter[] { new ExcludeNonDataMemberPropertiesConverter() },
                    new JavaScriptPrimitiveConverter[] { new StringEnumConverter() });

                var outputText = new StringBuilder();
                serializer.Serialize(model, outputText);

                writer.Write(outputText.ToString());
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<string> Extensions
        {
            get
            {
                yield return "json";
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExcludeNonDataMemberPropertiesConverter : JavaScriptConverter
        {
            #region Overrides of JavaScriptConverter

            public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
            {
                throw new NotSupportedException("Deserialization is not supported.");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
            {
                var members = TypeMemberCache.Of(obj.GetType());
                var propertiesToSerialize = members.SelectAllProperties(where: p => p.HasAttribute<DataMemberAttribute>());
                var serialized = new Dictionary<string, object>();

                foreach ( var property in propertiesToSerialize.ToArray() )
                {
                    serialized[property.Name] = property.GetValue(obj);
                }

                return serialized;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IEnumerable<Type> SupportedTypes
            {
                get
                {
                    return new[] { typeof(AbstractUidlNode) };
                }
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class StringEnumConverter : JavaScriptPrimitiveConverter
        {
            #region Overrides of JavaScriptPrimitiveConverter

            public override object Deserialize(object primitiveValue, Type type, JavaScriptSerializer serializer)
            {
                throw new NotSupportedException("Deserialization is not supported.");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override object Serialize(object obj, JavaScriptSerializer serializer)
            {
                if ( obj != null && obj.GetType().IsEnum )
                {
                    return obj.ToString();
                }
                else
                {
                    return obj;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override IEnumerable<Type> SupportedTypes
            {
                get
                {
                    return new[] { typeof(object) };
                }
            }

            #endregion
        }
    }
}
