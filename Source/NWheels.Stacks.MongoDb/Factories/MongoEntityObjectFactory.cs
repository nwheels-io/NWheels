using System;
using Autofac;
using Hapil;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Factories;
using NWheels.Entities.Factories;
using NWheels.Extensions;
using NWheels.TypeModel.Core.Factories;

namespace NWheels.Stacks.MongoDb.Factories
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
            var propertyMap = BuildPropertyStrategyMap(context, metaType);

            propertyMap.NeedImplementationTypeKey += (sender, args) => {
                args.TypeKeyToUse = CreateImplementationTypeKey(args.ContractType);
            };
            
            return new IObjectFactoryConvention[] {
                new BaseTypeConvention(MetadataCache, metaType), 
                new PropertyImplementationConvention(metaType, propertyMap),
                new EntityContractMethodsNotSupportedConvention(),
                new MaterializationConstructorConvention(metaType, propertyMap),
                new InitializationConstructorConvention(metaType, propertyMap),
                new ImplementIObjectConvention(), 
                new EntityObjectStateConvention(), 
                new ImplementIEntityObjectConvention(metaType, propertyMap), 
                new ImplementIEntityPartObjectConvention(metaType), 
                new ImplementIPartitionedObjectConvention(metaType), 
                new EnsureDomainObjectConvention(metaType), 
                new DependencyInjectionConvention(metaType, propertyMap, forceApply: true), 
                new NestedObjectsConvention(propertyMap), 
                new ObjectIdGeneratorConvention(propertyMap, metaType), 
                new BsonIgnoreConvention(MetadataCache, metaType),
                new BsonDiscriminatorConvention(context, MetadataCache, metaType),
                new BsonIgnoreExtraElementsConvention()
                //new LazyLoadDomainObjectConvention(metaType)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override TypeKey CreateImplementationTypeKey(Type entityContractInterface)
        {
            return new TypeKey(
                primaryInterface: entityContractInterface,
                baseType: BaseTypeConvention.GetBaseType(this, base.MetadataCache.GetTypeMetadata(entityContractInterface)));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private PropertyImplementationStrategyMap BuildPropertyStrategyMap(ObjectFactoryContext context, ITypeMetadata metaType)
        {
            var builder = new PropertyImplementationStrategyMap.Builder();
            Type collectionItemType;

            //-- calculated properties

            builder.AddRule(
                p => p.IsCalculated,
                p => new NotSupportedPropertyStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            //-- entity parts

            builder.AddRule(
                p => p.Kind == PropertyKind.Part && p.IsCollection,
                p => new CollectionAdapterStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.Kind == PropertyKind.Part && !p.IsCollection,
                p => new RelationTypecastStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            //-- entity relations - collections

            builder.AddRule(
                p => p.Kind == PropertyKind.Relation && p.IsCollection && ShouldPersistAsEmbeddedDocuments(p),
                p => new CollectionAdapterStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.Kind == PropertyKind.Relation && p.IsCollection && ShouldPersistAsArrayOfDocumentIds(p),
                p => new ArrayOfDocumentIdsStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.Kind == PropertyKind.Relation && p.IsCollection,
                p => new LazyLoadCollectionByForeignKeyStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            //-- entity relations - single (non-collection)
            
            builder.AddRule(
                p => p.Kind == PropertyKind.Relation && !p.IsCollection && ShouldPersistAsEmbeddedDocuments(p),
                p => new RelationTypecastStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.Kind == PropertyKind.Relation && !p.IsCollection && ShouldPersistAsDocumentId(p),
                p => new DocumentIdStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.Kind == PropertyKind.Relation && !p.IsCollection,
                p => new LazyLoadDocumentByForeignKeyStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            //-- scalar properties

            builder.AddRule(
                p => p.Kind == PropertyKind.Scalar && p.RelationalMapping != null && p.RelationalMapping.StorageType != null,
                p => new StorageDataTypeStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.Kind == PropertyKind.Scalar && !(p.ContractPropertyInfo.CanRead && p.ContractPropertyInfo.CanWrite),
                p => new PublicAccessorWrapperStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.Kind == PropertyKind.Scalar && p.ContractPropertyInfo.CanRead && p.ContractPropertyInfo.CanWrite,
                p => new AutomaticPropertyStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            return builder.Build(MetadataCache, metaType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool ShouldPersistAsEmbeddedDocuments(IPropertyMetadata metaProperty)
        {
            if ( metaProperty.Kind != PropertyKind.Relation )
            {
                return false;
            }

            if ( metaProperty.Relation.RelatedPartyType.IsEntityPart )
            {
                return true;
            }

            if ( metaProperty.Relation.RelatedPartyType.IsEntity )
            {
                return (metaProperty.Relation.Kind == RelationKind.Composition);
            }

            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool ShouldPersistAsArrayOfDocumentIds(IPropertyMetadata metaProperty)
        {
            if ( metaProperty.Kind != PropertyKind.Relation || !metaProperty.Relation.RelatedPartyType.IsEntity )
            {
                return false;
            }

            switch ( metaProperty.Relation.Multiplicity )
            {
                case RelationMultiplicity.ManyToMany:
                    return (metaProperty.Relation.ThisPartyKind == RelationPartyKind.Dependent || metaProperty.Relation.InverseProperty == null);
                case RelationMultiplicity.OneToMany:
                    return (metaProperty.Relation.InverseProperty == null);
                default:
                    return false;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool ShouldPersistAsDocumentId(IPropertyMetadata metaProperty)
        {
            if ( metaProperty.Kind != PropertyKind.Relation || !metaProperty.Relation.RelatedPartyType.IsEntity )
            {
                return false;
            }

            switch ( metaProperty.Relation.Multiplicity )
            {
                case RelationMultiplicity.OneToOne:
                    return (metaProperty.Relation.ThisPartyKind == RelationPartyKind.Dependent || metaProperty.Relation.InverseProperty == null);
                case RelationMultiplicity.ManyToOne:
                    return true;
                default:
                    return false;
            }
        }
    }
}
