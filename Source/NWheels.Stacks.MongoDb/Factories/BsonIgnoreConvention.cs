using System;
using Hapil;
using Hapil.Decorators;
using Hapil.Members;
using NWheels.DataObjects;

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
            //if ( property.PropertyDeclaration != null && property.PropertyDeclaration.DeclaringType == typeof(IEntityObject) ) // TODO: generalize this condition
            //{
            //    return;
            //}

            //var metaProperty = (
            //    property.PropertyDeclaration != null ? 
            //        _metaType.GetPropertyByDeclaration(property.PropertyDeclaration) :
            //        _metaType.GetPropertyByName(property.Name));

            //if ( metaProperty.Kind == PropertyKind.Relation && metaProperty.Relation.ThisPartyKind == RelationPartyKind.Dependent )
            //{
            //    decorate().Attribute<BsonIgnoreAttribute>();
            //}
        }
    }
}