using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using BreezeEntityState = Breeze.ContextProvider.EntityState;

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
            var domainObject = CreateOrRetrieveDomainObject(objectType, reader);
            serializer.Populate(reader, domainObject);
            return domainObject;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //TODO: revive this
        private IDomainObject CreateOrRetrieveDomainObject(Type implementationType, JsonReader reader)
        {
            var domainContext = JsonSerializationUtility.GetCurrentDomainContext();
            var contractType = JsonSerializationUtility.GetEntityContractType(implementationType);
            var metaType = domainContext.Components.Resolve<ITypeMetadataCache>().GetTypeMetadata(contractType);
            var idProperty = metaType.PrimaryKey.Properties[0];

            var entityJson = (JObject)((JTokenReader)reader).CurrentToken;
            var entityState = ReadEntityState(entityJson);

            IDomainObject domainObject;

            switch ( entityState )
            {
                case BreezeEntityState.Added:
                    var persistableObject = (IPersistableObject)domainContext.PersistableObjectFactory.NewEntity(contractType);
                    //var domainFactory = domainContext.Components.Resolve<IDomainObjectFactory>();
                    //domainObject = domainFactory.CreateDomainObjectInstance();
                    //domainObject.TemporaryKey = ReadTemporaryKey(idProperty, entityJson);
                    break;
                case BreezeEntityState.Modified:
                case BreezeEntityState.Deleted:
                    var repository = domainContext.GetEntityRepository(contractType);
                    var entityId = ReadEntityId(metaType, idProperty, entityJson);
                    domainObject = (IDomainObject)repository.TryGetById(entityId);
                    break;
                default:
                    throw new NotSupportedException("cannot handle entity in state: " + entityState);
            }

            return null;
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static IEntityId ReadEntityId(ITypeMetadata metaType, IPropertyMetadata idProperty, JObject entityJson)
        {
            var entityIdJson = (JValue)entityJson.Property(idProperty.Name).Value;
            var entityIdString = entityIdJson.Value.ToString();
            var entityIdInstance = JsonSerializationUtility.ParseEntityId(metaType, idProperty, entityIdString);

            return entityIdInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static object ReadTemporaryKey(IPropertyMetadata idProperty, JObject entityJson)
        {
            var entityIdJson = (JValue)entityJson.Property(idProperty.Name).Value;
            return entityIdJson.Value;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static BreezeEntityState ReadEntityState(JObject entityJson)
        {
            var entityAspectJson = (JObject)entityJson.Property("entityAspect").Value;
            var entityStateJson = (JValue)entityAspectJson.Property("entityState").Value;

            return (BreezeEntityState)Enum.Parse(typeof(BreezeEntityState), entityStateJson.Value.ToString());
        }
    }
}
