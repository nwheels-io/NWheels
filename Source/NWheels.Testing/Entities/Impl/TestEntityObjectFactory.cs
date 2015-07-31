using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Factories;
using NWheels.Entities.Factories;
using NWheels.Extensions;
using NWheels.TypeModel.Core.Factories;

namespace NWheels.Testing.Entities.Impl
{
    public class TestEntityObjectFactory : EntityObjectFactory
    {
        public TestEntityObjectFactory(IComponentContext components, DynamicModule module, TypeMetadataCache metadataCache)
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
                new MaterializationConstructorConvention(metaType, propertyMap),
                new InitializationConstructorConvention(metaType, propertyMap),
                new ImplementIObjectConvention(), 
                new ImplementIEntityObjectConvention(metaType, propertyMap), 
                new ImplementIEntityPartObjectConvention(metaType), 
                new DependencyInjectionConvention(propertyMap), 
                new NestedObjectsConvention(propertyMap), 
                new TestIdGeneratorConvention(metaType), 
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private PropertyImplementationStrategyMap BuildPropertyStrategyMap(ObjectFactoryContext context, ITypeMetadata metaType)
        {
            var builder = new PropertyImplementationStrategyMap.Builder();
            Type collectionItemType;

            builder.AddRule(
                p => p.ClrType.IsCollectionType(out collectionItemType) && (collectionItemType.IsEntityPartContract() || collectionItemType.IsEntityContract()),
                p => new CollectionAdapterStrategy(context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.ClrType.IsEntityPartContract(),
                p => new RelationTypecastStrategy(context, MetadataCache, metaType, p));

            builder.AddRule(
                p => !(p.ContractPropertyInfo.CanRead && p.ContractPropertyInfo.CanWrite),
                p => new PublicAccessorWrapperStrategy(context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.ContractPropertyInfo.CanRead && p.ContractPropertyInfo.CanWrite,
                p => new AutomaticPropertyStrategy(context, MetadataCache, metaType, p));

            return builder.Build(MetadataCache, metaType);
        }
    }
}
