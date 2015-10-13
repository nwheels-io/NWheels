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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DomainObjectContractResolver(IBreezeEndpointLogger logger)
        {
            _logger = logger;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> result = base.CreateProperties(type, memberSerialization);

            var relationProperties = result.Where(p => p.PropertyType.IsEntityContract()).ToArray();

            foreach ( var relation in relationProperties )
            {
                _logger.ContractResolverAddingForeignKeyProperty(type.Name, relation.PropertyName);

                var foreignKeyProperty = new JsonProperty {
                    PropertyType = typeof(string),
                    DeclaringType = relation.DeclaringType,
                    PropertyName = relation.PropertyName + "$FK",
                    ValueProvider = new ForeignKeyValueProvider(relation),
                    Readable = true,
                    Writable = true
                };

                result.Add(foreignKeyProperty);
            }

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ForeignKeyValueProvider : IValueProvider
        {
            private readonly JsonProperty _relationProperty;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ForeignKeyValueProvider(JsonProperty relationProperty)
            {
                _relationProperty = relationProperty;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void SetValue(object target, object value)
            {
                //object relatedEntityObject;

                //if ( value != null )
                //{

                //}
                //else
                //{
                //    relatedEntityObject = null;
                //}

                //_relationProperty.ValueProvider.SetValue(target, relatedEntityObject);
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

