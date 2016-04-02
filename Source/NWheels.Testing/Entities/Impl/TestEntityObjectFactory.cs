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
                new EnsureDomainObjectConvention(metaType), 
                new DependencyInjectionConvention(metaType, propertyMap, forceApply: true), 
                new NestedObjectsConvention(metaType, propertyMap), 
                new TestIdGeneratorConvention(metaType), 
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

            builder.AddRule(
                p => p.ClrType.IsCollectionType(out collectionItemType) && (collectionItemType.IsEntityPartContract() || collectionItemType.IsEntityContract()),
                p => new CollectionAdapterStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.ClrType.IsEntityPartContract(),
                p => new RelationTypecastStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            builder.AddRule(
                p => !(p.ContractPropertyInfo.CanRead && p.ContractPropertyInfo.CanWrite),
                p => new PublicAccessorWrapperStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            builder.AddRule(
                p => p.ContractPropertyInfo.CanRead && p.ContractPropertyInfo.CanWrite,
                p => new AutomaticPropertyStrategy(builder.MapBeingBuilt, context, MetadataCache, metaType, p));

            return builder.Build(MetadataCache, metaType);
        }
    }
}
