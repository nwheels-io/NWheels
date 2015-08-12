using System;
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public DomainObjectFactory(IComponentContext components, DynamicModule module, ITypeMetadataCache metadataCache)
            : base(module)
        {
            _components = components;
            _metadataCache = metadataCache;
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
            var typeEntry = GetOrBuildDomainObjectTypeEntry(
                contractType: typeof(TEntityContract), 
                persistableFactoryType: underlyingPersistableObject.As<IObject>().FactoryType);
            
            return typeEntry.CreateInstance<TEntityContract, TEntityContract, IComponentContext>(0, underlyingPersistableObject, _components);
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
                new ContainedPersistableObjectConvention(domainFactoryContext),
                new ImplementIObjectConvention(),
                new ActiveRecordConvention(domainFactoryContext),
                new DomainObjectMethodsConvention(domainFactoryContext)
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

        public class ContractPropertyDelegationConvention : ImplementationConvention
        {
            private readonly DomainObjectFactoryContext _conventionState;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ContractPropertyDelegationConvention(DomainObjectFactoryContext conventionState)
                : base(Will.ImplementPrimaryInterface)
            {
                _conventionState = conventionState;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of ImplementationConvention

            protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                writer.AllProperties().ImplementPropagate(_conventionState.PersistableObjectField.CastTo<TT.TInterface>());
                writer.AllMethods().Implement(m => m.Throw<NotSupportedException>("No domain object was registered for this entity."));
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class DomainObjectPropertyDelegationConvention : ImplementationConvention
        {
            private readonly DomainObjectFactoryContext _conventionState;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DomainObjectPropertyDelegationConvention(DomainObjectFactoryContext conventionState)
                : base(Will.ImplementBaseClass)
            {
                _conventionState = conventionState;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of ImplementationConvention

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                var allProperties = writer.AllProperties().ToArray();

                using ( TT.CreateScope<TT.TContract, TT.TImpl>(_conventionState.MetaType.ContractType, _conventionState.PersistableObjectType) )
                {
                    foreach ( var property in allProperties )
                    {
                        using ( TT.CreateScope<TT.TProperty>(property.PropertyType) )
                        {
                            ImplementProperty(writer, property);
                        }
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementProperty(ImplementationClassWriter<TypeTemplate.TBase> writer, PropertyInfo property)
            {
                var metaProperty = _conventionState.MetaType.GetPropertyByName(property.Name);
                var contractCanRead = metaProperty.ContractPropertyInfo.CanRead;
                var contractCanWrite = metaProperty.ContractPropertyInfo.CanWrite;
                var domainCanRead = property.GetMethod != null;
                var domainCanWrite = property.SetMethod != null;

                writer.Property(property).Implement(
                    getter: p =>
                        domainCanRead
                        ? p.Get(w => ImplementPropertyGetter(w, metaProperty, contractCanRead))
                        : null,
                    setter: p =>
                        domainCanWrite
                        ? p.Set((w, value) => ImplementPropertySetter(w, value, metaProperty, contractCanWrite))
                        : null
                );
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementPropertyGetter(
                FunctionMethodWriter<TypeTemplate.TProperty> w, 
                IPropertyMetadata metaProperty,
                bool contractCanRead)
            {
                if ( contractCanRead )
                {
                    w.Return(_conventionState.PersistableObjectField.Prop<TT.TProperty>(metaProperty.ContractPropertyInfo));
                }
                else
                {
                    var propertyReadMethodName = PropertyImplementationStrategy.GetReadAccessorMethodName(metaProperty);
                    var propertyReadMethod = TypeMemberCache.Of(_conventionState.PersistableObjectType).Methods.Single(m => m.Name == propertyReadMethodName);

                    w.Return(_conventionState.PersistableObjectField.Func<TT.TProperty>(propertyReadMethod));
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementPropertySetter(
                VoidMethodWriter w,
                Argument<TypeTemplate.TProperty> value,
                IPropertyMetadata metaProperty,
                bool contractCanWrite)
            {
                if ( contractCanWrite )
                {
                    _conventionState.PersistableObjectField.Prop<TT.TProperty>(metaProperty.ContractPropertyInfo).Assign(value);
                }
                else
                {
                    var propertyWriteMethodName = PropertyImplementationStrategy.GetWriteAccessorMethodName(metaProperty);
                    var propertyWriteMethod = TypeMemberCache.Of(_conventionState.PersistableObjectType).Methods.Single(m => m.Name == propertyWriteMethodName);

                    _conventionState.PersistableObjectField.Void<TT.TProperty>(propertyWriteMethod, value);
                }
            }
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
    }
}
