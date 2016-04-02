using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Factories;
using NWheels.Entities.Core;
using NWheels.Extensions;
using NWheels.TypeModel.Core;
using NWheels.TypeModel.Core.Factories;

namespace NWheels.Entities.Factories
{
    public class PresentationObjectFactory : ConventionObjectFactory, IPresentationObjectFactory
    {
        private readonly IComponentContext _components;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly ConcurrentDictionary<Type, PolymorphicActivator> _polymorphicActivatorByConcreteContract;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PresentationObjectFactory(IComponentContext components, DynamicModule module, ITypeMetadataCache metadataCache)
            : base(module)
        {
            _components = components;
            _metadataCache = metadataCache;
            _polymorphicActivatorByConcreteContract = new ConcurrentDictionary<Type, PolymorphicActivator>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type GetOrBuildPresentationObjectType(Type contractType)
        {
            var typeEntry = GetOrBuildType(new TypeKey(primaryInterface: contractType));
            return typeEntry.DynamicType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntityContract CreatePresentationObjectInstance<TEntityContract>(TEntityContract domainObject)
        {
            var domainContractType = ((IObject)domainObject).ContractType;

            var typeEntry = GetOrBuildType(new TypeKey(primaryInterface: typeof(TEntityContract)));
            TEntityContract domainObjectInstance;

            if ( typeof(TEntityContract) == domainContractType )
            {
                domainObjectInstance = typeEntry
                    .CreateInstance<TEntityContract, TEntityContract, IComponentContext>(0, domainObject, _components);
            }
            else
            {
                domainObjectInstance = GetOrAddPolymorphicActivator(domainContractType)
                    .CreateConcreteInstance<TEntityContract>(typeEntry, 0, domainObject, _components);
            }

            return domainObjectInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ConventionObjectFactory

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            var metaType = _metadataCache.GetTypeMetadata(context.TypeKey.PrimaryInterface);

            var propertyMapBuilder = new PropertyImplementationStrategyMap.Builder();
            var presentationFactoryContext = 
                new PresentationObjectFactoryContext(context, _metadataCache, metaType, propertyMapBuilder.MapBeingBuilt);

            propertyMapBuilder.MapBeingBuilt.NeedImplementationTypeKey += (sender, args) => {
                args.TypeKeyToUse = new TypeKey(
                    primaryInterface: args.ContractType,
                    baseType: PresentationObjectBaseTypeConvention.GetBaseType(context, _metadataCache.GetTypeMetadata(args.ContractType)));
            };

            BuildPropertyStrategyMap(propertyMapBuilder, presentationFactoryContext);

            return new IObjectFactoryConvention[] {
                new PresentationObjectBaseTypeConvention(presentationFactoryContext),
                new PresentationObjectConstructorInjectionConvention(presentationFactoryContext), 
                new PresentationObjectPropertyImplementationConvention(presentationFactoryContext), 
                new ImplementIPresentationObjectConvention(presentationFactoryContext),
                new ImplementIObjectConvention(),
                new NestedObjectsConvention(metaType, presentationFactoryContext.PropertyMap),
                new PresentationObjectMethodsConvention(presentationFactoryContext),
            };
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void BuildPropertyStrategyMap(
            PropertyImplementationStrategyMap.Builder builder, 
            PresentationObjectFactoryContext context)
        {
            Type collectionItemType;

            builder.AddRule(
                p => p.ClrType.IsEntityContract() || p.ClrType.IsEntityPartContract(),
                p => new PresentationNestedObjectPropertyStrategy(builder.MapBeingBuilt, context, p));

            builder.AddRule(
                p => p.ClrType.IsCollectionType(out collectionItemType) && (collectionItemType.IsEntityContract() || collectionItemType.IsEntityPartContract()),
                p => new PresentationNestedCollectionPropertyStrategy(builder.MapBeingBuilt, context, p));

            builder.AddRule(
                p => true,
                p => new PresentationScalarPropertyStrategy(builder.MapBeingBuilt, context, p));

            builder.Build(context.MetadataCache, context.MetaType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private PolymorphicActivator GetOrAddPolymorphicActivator(Type concreteContract)
        {
            return _polymorphicActivatorByConcreteContract.GetOrAdd(concreteContract, key => PolymorphicActivator.Create(concreteContract));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private abstract class PolymorphicActivator
        {
            public abstract TBaseContract CreateConcreteInstance<TBaseContract>(
                TypeEntry typeEntry,
                int constructorIndex,
                TBaseContract persistableObject,
                IComponentContext components);

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static PolymorphicActivator Create(Type concreteContract)
            {
                var closedType = typeof(PolymorphicActivator<>).MakeGenericType(concreteContract);
                var instance = Activator.CreateInstance(closedType);
                return (PolymorphicActivator)instance;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class PolymorphicActivator<TConcreteContract> : PolymorphicActivator
        {
            public override TBaseContract CreateConcreteInstance<TBaseContract>(TypeEntry typeEntry, int constructorIndex, TBaseContract persistableObject, IComponentContext components)
            {
                return (TBaseContract)(object)typeEntry.CreateInstance<TConcreteContract, TConcreteContract, IComponentContext>(
                    0, 
                    (TConcreteContract)(object)persistableObject, 
                    components);
            }
        }
    }
}
