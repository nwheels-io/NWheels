using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Factories;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.TypeModel.Core;
using NWheels.TypeModel.Core.Factories;
using TT = Hapil.TypeTemplate;

namespace NWheels.Entities.Factories
{
    public class DomainObjectFactory : ConventionObjectFactory, IDomainObjectFactory
    {
        private readonly IComponentContext _components;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly ConcurrentDictionary<Type, PolymorphicActivator> _polymorphicActivatorByConcreteContract;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DomainObjectFactory(IComponentContext components, DynamicModule module, ITypeMetadataCache metadataCache)
            : base(module)
        {
            _components = components;
            _metadataCache = metadataCache;
            _polymorphicActivatorByConcreteContract = new ConcurrentDictionary<Type, PolymorphicActivator>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type GetOrBuildDomainObjectType(Type contractType, Type persistableFactoryType)
        {
            var typeEntry = GetOrBuildDomainObjectTypeEntry(contractType, persistableFactoryType);
            return typeEntry.DynamicType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntityContract CreateDomainObjectInstance<TEntityContract>(TEntityContract underlyingPersistableObject)
        {
            var persistableContractType = ((IObject)underlyingPersistableObject).ContractType;
            var persistableFactoryType = ((IObject)underlyingPersistableObject).FactoryType;

            var typeEntry = GetOrBuildDomainObjectTypeEntry(persistableContractType, persistableFactoryType);
            TEntityContract domainObjectInstance;

            if ( typeof(TEntityContract) == persistableContractType )
            {
                domainObjectInstance = typeEntry
                    .CreateInstance<TEntityContract, TEntityContract, IComponentContext>(0, underlyingPersistableObject, _components);
            }
            else
            {
                domainObjectInstance = GetOrAddPolymorphicActivator(persistableContractType)
                    .CreateConcreteInstance<TEntityContract>(typeEntry, 0, underlyingPersistableObject, _components);
            }

            return domainObjectInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of ConventionObjectFactory

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            var metaType = _metadataCache.GetTypeMetadata(context.TypeKey.PrimaryInterface);
            var persistableObjectFactoryType = context.TypeKey.SecondaryInterfaces[0];

            var propertyMapBuilder = new PropertyImplementationStrategyMap.Builder();
            var domainFactoryContext = 
                new DomainObjectFactoryContext(context, _metadataCache, metaType, persistableObjectFactoryType, propertyMapBuilder.MapBeingBuilt);

            BuildPropertyStrategyMap(propertyMapBuilder, domainFactoryContext);

            return new IObjectFactoryConvention[] {
                new DomainObjectBaseTypeConvention(domainFactoryContext),
                new DomainObjectConstructorInjectionConvention(domainFactoryContext), 
                new DomainObjectPropertyImplementationConvention(domainFactoryContext), 
                new ImplementIDomainObjectConvention(domainFactoryContext),
                new ImplementIObjectConvention(),
                new NestedObjectsConvention(domainFactoryContext.PropertyMap),
                new ActiveRecordConvention(domainFactoryContext),
                new DomainObjectModifiedVectorConvention(domainFactoryContext), 
                new DomainObjectOperationMethodsConvention(domainFactoryContext),
                new DomainObjectWiredPropertiesConvention(domainFactoryContext),
            };
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TypeEntry GetOrBuildDomainObjectTypeEntry(Type contractType, Type persistableFactoryType)
        {
            //Type persistableObjectType;

            //if ( !_metadataCache.GetTypeMetadata(contractType).)

            var typeKey = new TypeKey(
                primaryInterface: contractType, 
                baseType: _metadataCache.GetTypeMetadata(contractType).DomainObjectType,
                secondaryInterfaces: new[] { persistableFactoryType });

            var typeEntry = GetOrBuildType(typeKey);
            return typeEntry;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void BuildPropertyStrategyMap(
            PropertyImplementationStrategyMap.Builder builder, 
            DomainObjectFactoryContext context)
        {
            Type collectionItemType;

            builder.AddRule(
                p => p.ClrType.IsEntityContract() || p.ClrType.IsEntityPartContract(),
                p => new DomainPersistableObjectCastStrategy(context, p));

            builder.AddRule(
                p => p.ClrType.IsCollectionType(out collectionItemType) && (collectionItemType.IsEntityContract() || collectionItemType.IsEntityPartContract()),
                p => new DomainPersistableCollectionCastStrategy(context, p));

            builder.AddRule(
                p => true,
                p => new DomainPersistableDelegationStrategy(context, p));

            builder.Build(context.MetadataCache, context.MetaType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private PolymorphicActivator GetOrAddPolymorphicActivator(Type concreteContract)
        {
            return _polymorphicActivatorByConcreteContract.GetOrAdd(concreteContract, key => PolymorphicActivator.Create(concreteContract));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TemplateTypes
        {
            public interface TDomain : TypeTemplate.ITemplateType<TDomain>, IContain<TPersistable>
            {
            }
            public interface TPersistable : TypeTemplate.ITemplateType<TPersistable>, IContainedIn<TDomain>
            {
            }
            public interface TDomainItem : TypeTemplate.ITemplateType<TDomainItem>, IContain<TPersistableItem>
            {
            }
            public interface TPersistableItem : TypeTemplate.ITemplateType<TPersistableItem>, IContainedIn<TDomainItem>
            {
            }
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
