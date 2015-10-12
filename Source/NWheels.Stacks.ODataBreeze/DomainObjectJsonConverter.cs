using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Entities.Factories;
using NWheels.Extensions;

namespace NWheels.Stacks.ODataBreeze
{
    public class DomainObjectJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IDomainObject).IsAssignableFrom(objectType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if ( existingValue == null )
            {
                var domainObject = CreateDomainObject(objectType, reader);
                existingValue = domainObject;
            }

            serializer.Populate(reader, existingValue);

            return existingValue;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IDomainObject CreateDomainObject(Type implementationType, JsonReader reader)
        {
            var domainContext = RuntimeEntityModelHelpers.CurrentDomainContext;

            if ( domainContext == null )
            {
                throw new InvalidOperationException("No domain context on current thread.");
            }

            var contractType = implementationType.GetInterfaces().First(intf => intf.IsEntityContract());
            var persistableObject = (IPersistableObject)domainContext.PersistableObjectFactory.NewEntity(contractType);
            
            var domainFactory = domainContext.Components.Resolve<IDomainObjectFactory>();
            var domainObject = domainFactory.CreateDomainObjectInstance(persistableObject);

            var tokenReader = (JTokenReader)reader;
            var objectToken = (JObject)tokenReader.CurrentToken;

            var metaType = domainContext.Components.Resolve<ITypeMetadataCache>().GetTypeMetadata(contractType);
            
            if ( metaType.PrimaryKey != null && metaType.PrimaryKey.Properties.Count == 1 )
            {
                var keyMetaProperty = metaType.PrimaryKey.Properties[0];
                var temporaryKeyValue = objectToken.Property(keyMetaProperty.Name).Value;
                domainObject.TemporaryKey = temporaryKeyValue;
            }

            return domainObject;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }
    }
}
