using System;
using Autofac;
using Hapil;
using Hapil.Writers;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Factories;
using NWheels.Entities.Factories;
using NWheels.Extensions;
using NWheels.TypeModel.Core.Factories;

namespace NWheels.UI.ServerSide
{
    public interface IViewModelObjectFactory : IEntityObjectFactory
    {
    }

    public class ViewModelObjectFactory : EntityObjectFactory, IViewModelObjectFactory
    {
        public ViewModelObjectFactory(IComponentContext components, DynamicModule module, TypeMetadataCache metadataCache)
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
                new NeverModifiedConvention()
                //new ImplementIEntityObjectConvention(metaType, propertyMap), 
                //new ImplementIEntityPartObjectConvention(metaType), 
                //new EnsureDomainObjectConvention(metaType), 
                //new DependencyInjectionConvention(metaType, propertyMap, forceApply: true), 
                //new NestedObjectsConvention(propertyMap), 
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class NeverModifiedConvention : ImplementationConvention
        {
            public NeverModifiedConvention()
                : base(Will.ImplementBaseClass)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of ImplementationConvention

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                writer.ImplementInterfaceExplicitly<IObject>()
                    .Property(x => x.IsModified).Implement(p =>
                        p.Get(gw => gw.Return(false)
                    )
                );
            }

            #endregion
        }
    }
}
