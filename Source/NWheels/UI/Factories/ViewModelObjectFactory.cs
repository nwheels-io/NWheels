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

namespace NWheels.UI.Factories
{
    public interface IViewModelObjectFactory : IEntityObjectFactory
    {
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override TypeKey CreateImplementationTypeKey(Type entityContractInterface)
        {
            return new TypeKey(
                primaryInterface: entityContractInterface,
                baseType: BaseTypeConvention.GetBaseType(this, base.MetadataCache.GetTypeMetadata(entityContractInterface)));
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
