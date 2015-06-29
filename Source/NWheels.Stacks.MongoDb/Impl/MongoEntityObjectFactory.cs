using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using Hapil.Decorators;
using Hapil.Members;
using MongoDB.Bson.Serialization.Attributes;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Stacks.MongoDb.Core;

namespace NWheels.Stacks.MongoDb.Impl
{
    public class MongoEntityObjectFactory : EntityObjectFactory
    {
        public MongoEntityObjectFactory(IComponentContext components, DynamicModule module, TypeMetadataCache metadataCache)
            : base(components, module, metadataCache)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            var metaType = MetadataCache.GetTypeMetadata(context.TypeKey.PrimaryInterface);

            return new IObjectFactoryConvention[] {
                //new MongoDBRefConvention(MetadataCache, (TypeMetadataBuilder)metaType),
                new EntityObjectConvention(MetadataCache),
                new BsonIgnoreConvention(MetadataCache, metaType)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class MongoDBRefConvention : ImplementationConvention
        {
            private readonly TypeMetadataCache _metadataCache;
            private readonly TypeMetadataBuilder _metaType;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public MongoDBRefConvention(TypeMetadataCache metadataCache, TypeMetadataBuilder metaType)
                : base(Will.InspectDeclaration)
            {
                _metadataCache = metadataCache;
                _metaType = (TypeMetadataBuilder)metaType;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnInspectDeclaration(ObjectFactoryContext context)
            {
                foreach ( var property in _metaType.Properties )
                {
                    if ( property.Kind == PropertyKind.Relation && property.Relation.Kind != RelationKind.Composition && !property.IsCollection )
                    {
                        var relationalMapping = property.SafeGetRelationalMapping();

                        if ( relationalMapping.StorageType == null )
                        {
                            relationalMapping.StorageType = _metadataCache.GetStorageTypeInstance(typeof(MongoDBRefStorageType<>), property.ClrType);
                        }
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

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
                var metaProperty = (
                    property.PropertyDeclaration != null ? 
                    _metaType.GetPropertyByDeclaration(property.PropertyDeclaration) :
                    _metaType.GetPropertyByName(property.Name));

                if ( metaProperty.Kind == PropertyKind.Relation && metaProperty.Relation.ThisPartyKind == RelationPartyKind.Dependent )
                {
                    decorate().Attribute<BsonIgnoreAttribute>();
                }
            }
        }
    }
}
