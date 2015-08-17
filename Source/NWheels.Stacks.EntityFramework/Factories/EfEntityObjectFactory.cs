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

namespace NWheels.Stacks.EntityFramework.Factories
{
    public class EfEntityObjectFactory : EntityObjectFactory
    {
        public EfEntityObjectFactory(IComponentContext components, DynamicModule module, TypeMetadataCache metadataCache)
            : base(components, module, metadataCache)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            var metaType = MetadataCache.GetTypeMetadata(context.TypeKey.PrimaryInterface);
            base.MetadataCache.EnsureRelationalMapping(metaType);

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
                new InverseManyToManyCollectionConvention(context, MetadataCache, metaType),
                new EfConfigurationConvention(MetadataCache, metaType, propertyMap),
                new EfNotMappedConvention(metaType)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private PropertyImplementationStrategyMap BuildPropertyStrategyMap(ObjectFactoryContext context, ITypeMetadata metaType)
        {
            var builder = new PropertyImplementationStrategyMap.Builder();
            Type collectionItemType;

            builder.AddRule(
                p => p.ClrType.IsCollectionType(out collectionItemType) && collectionItemType.IsEntityContract(),
                p => new EfPropertyImplementationStrategy( 
                    implementor: new CollectionAdapterStrategy(context, MetadataCache, metaType, p),
                    configurator: new RelationPropertyConfigurationStrategy(context, metaType, p)));

            builder.AddRule(
                p => p.ClrType.IsCollectionType(out collectionItemType) && collectionItemType.IsEntityPartContract(),
                p => new EfPropertyImplementationStrategy( 
                    implementor: new CollectionAdapterJsonStringStrategy(context, MetadataCache, metaType, p),
                    configurator: new JsonStringPropertyConfigurationStrategy(context, metaType, p)));

            builder.AddRule(
                p => p.ClrType.IsEntityContract(),
                p => new EfPropertyImplementationStrategy( 
                    implementor: new RelationTypecastStrategy(context, MetadataCache, metaType, p),
                    configurator: new RelationPropertyConfigurationStrategy(context, metaType, p)));

            builder.AddRule(
                p => p.ClrType.IsEntityPartContract(),
                p => new EfPropertyImplementationStrategy(
                    implementor: new RelationTypecastStrategy(context, MetadataCache, metaType, p),
                    configurator: new ComplexTypePropertyConfigurationStrategy(context, metaType, p)));

            builder.AddRule(
                p => p.Kind == PropertyKind.Scalar && p.RelationalMapping.StorageType != null,
                p => new EfPropertyImplementationStrategy(
                    implementor: new StorageDataTypeStrategy(context, MetadataCache, metaType, p), 
                    configurator: new ScalarPropertyConfigurationStrategy(context, metaType, p)));

            builder.AddRule(
                p => p.Kind == PropertyKind.Scalar && !(p.ContractPropertyInfo.CanRead && p.ContractPropertyInfo.CanWrite),
                p => new EfPropertyImplementationStrategy( 
                    implementor: new PublicAccessorWrapperStrategy(context, MetadataCache, metaType, p),
                    configurator: new ScalarPropertyConfigurationStrategy(context, metaType, p)));

            builder.AddRule(
                p => p.Kind == PropertyKind.Scalar && p.ContractPropertyInfo.CanRead && p.ContractPropertyInfo.CanWrite,
                p => new EfPropertyImplementationStrategy( 
                    implementor: new AutomaticPropertyStrategy(context, MetadataCache, metaType, p),
                    configurator: new ScalarPropertyConfigurationStrategy(context, metaType, p)));

            return builder.Build(MetadataCache, metaType);
        }
    }
}
