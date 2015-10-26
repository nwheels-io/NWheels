using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NWheels.Concurrency;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Entities.Factories;
using NWheels.Extensions;

namespace NWheels.Stacks.ODataBreeze
{
    public class DomainObjectContractResolver : DefaultContractResolver
    {
        private readonly IBreezeEndpointLogger _logger;
        private readonly ITypeMetadataCache _metadataCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DomainObjectContractResolver(ITypeMetadataCache metadataCache, IBreezeEndpointLogger logger)
        {
            _logger = logger;
            _metadataCache = metadataCache;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        //{
        //    IList<JsonProperty> result = base.CreateProperties(type, memberSerialization);
        //    var contractType = JsonSerializationUtility.TryGetEntityContractType(type);

        //    if ( contractType != null )
        //    {
        //        var metaType = _metadataCache.GetTypeMetadata(contractType);
        //        var relationProperties = result.Where(p => p.PropertyType.IsEntityContract()).ToArray();

        //        foreach ( var relation in relationProperties )
        //        {
        //            _logger.ContractResolverAddingForeignKeyProperty(type.Name, relation.PropertyName);

        //            var foreignKeyProperty = new JsonProperty {
        //                PropertyType = typeof(string),
        //                DeclaringType = relation.DeclaringType,
        //                PropertyName = JsonSerializationUtility.GetForeignKeyPropertyName(relation.PropertyName),
        //                ValueProvider = new ForeignKeyValueProvider(metaType, relation),
        //                Readable = true,
        //                Writable = true
        //            };

        //            result.Add(foreignKeyProperty);
        //        }
        //    }

        //    return result;
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ForeignKeyValueProvider : IValueProvider
        {
            private readonly JsonProperty _relationProperty;
            private readonly ITypeMetadata _relatedMetaType;
            private readonly IPropertyMetadata _relatedKeyMetaProperty;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ForeignKeyValueProvider(ITypeMetadata metaType, JsonProperty relationProperty)
            {
                _relationProperty = relationProperty;

                var metaProperty = metaType.GetPropertyByName(_relationProperty.PropertyName);
                _relatedMetaType = metaProperty.Relation.RelatedPartyType;
                
                var relatedTypeKey = (metaProperty.Relation.RelatedPartyKey ?? _relatedMetaType.PrimaryKey);
                _relatedKeyMetaProperty = relatedTypeKey.Properties[0];
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void SetValue(object target, object value)
            {
                var domainContext = JsonSerializationUtility.GetCurrentDomainContext();

                object relatedEntityObject;

                if ( value != null )
                {
                    var relatedEntityId = JsonSerializationUtility.ParseEntityId(_relatedMetaType, _relatedKeyMetaProperty, value.ToString());
                    var repository = domainContext.GetEntityRepository(_relatedMetaType.ContractType);
                    relatedEntityObject = repository.TryGetById(relatedEntityId);
                }
                else
                {
                    relatedEntityObject = null;
                }

                _relationProperty.ValueProvider.SetValue(target, relatedEntityObject);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object GetValue(object target)
            {
                var relatedEntity = _relationProperty.ValueProvider.GetValue(target);

                if ( relatedEntity != null )
                {
                    var relatedEntityId = EntityId.ValueOf(relatedEntity);
                    return relatedEntityId.ToStringOrDefault();
                }
                else
                {
                    return null;
                }
            }
        }
    }
}

