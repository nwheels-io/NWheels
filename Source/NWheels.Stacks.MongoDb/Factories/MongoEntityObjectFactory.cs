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
            
            return new IObjectFactoryConvention[] {
                new BaseTypeConvention(MetadataCache, metaType), 
                new PropertyImplementationConvention(metaType, propertyMap),
                new EntityContractMethodsNotSupportedConvention(),
                new MaterializationConstructorConvention(metaType, propertyMap),
                new InitializationConstructorConvention(metaType, propertyMap),
                new ImplementIObjectConvention(), 
                new ImplementIEntityObjectConvention(metaType, propertyMap), 
                new ImplementIEntityPartObjectConvention(metaType), 
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

        private PropertyImplementationStrategyMap BuildPropertyStrategyMap(ObjectFactoryContext context, ITypeMetadata metaType)
        {
            var builder = new PropertyImplementationStrategyMap.Builder();
            Type collectionItemType;

            builder.AddRule(
                p => p.ClrType.IsCollectionType(out collectionItemType) && collectionItemType.IsEntityContract(), 
                p => new ArrayOfDocumentIdsStrategy(context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.ClrType.IsEntityContract(),
                p => new DocumentIdStrategy(context, MetadataCache, metaType, p));
            
            builder.AddRule(
                p => p.ClrType.IsCollectionType(out collectionItemType) && collectionItemType.IsEntityPartContract(),
                p => new CollectionAdapterStrategy(context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.ClrType.IsEntityPartContract(),
                p => new RelationTypecastStrategy(context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.Kind == PropertyKind.Scalar && p.RelationalMapping != null && p.RelationalMapping.StorageType != null,
                p => new StorageDataTypeStrategy(context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.Kind == PropertyKind.Scalar && !(p.ContractPropertyInfo.CanRead && p.ContractPropertyInfo.CanWrite),
                p => new PublicAccessorWrapperStrategy(context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.Kind == PropertyKind.Scalar && p.ContractPropertyInfo.CanRead && p.ContractPropertyInfo.CanWrite,
                p => new AutomaticPropertyStrategy(context, MetadataCache, metaType, p));

            return builder.Build(MetadataCache, metaType);
        }
    }
}
