using System;
using Hapil;
using Hapil.Decorators;
using Hapil.Members;
using NWheels.DataObjects;
using MongoDB.Bson.Serialization.Attributes;

namespace NWheels.Stacks.MongoDb.Factories
{
    public class BsonIgnoreConvention : DecorationConvention
    {
        private readonly ITypeMetadataCache _metadataCache;
        private readonly ITypeMetadata _metaType;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public BsonIgnoreConvention(ITypeMetadataCache metadataCache, ITypeMetadata metaType)
            : base(Will.DecorateProperties)
        {
            _metadataCache = metadataCache;
            _metaType = metaType;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnProperty(PropertyMember property, Func<PropertyDecorationBuilder> decorate)
        {
            IPropertyMetadata metaProperty = null;

            if ( property.PropertyDeclaration != null )
            {
                _metaType.TryGetPropertyByDeclaration(property.PropertyDeclaration, out metaProperty);
            }

            if ( metaProperty == null )
            {
                _metaType.TryGetPropertyByName(property.Name, out metaProperty);
            }

            if ( metaProperty != null && metaProperty.IsCalculated )
            {
                decorate().Attribute<BsonIgnoreAttribute>();
            }
        }
    }
}