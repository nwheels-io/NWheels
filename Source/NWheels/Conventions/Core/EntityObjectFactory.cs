using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.StorageTypes;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Extensions;
using TT = Hapil.TypeTemplate;

namespace NWheels.Conventions.Core
{
    public class EntityObjectFactory : ConventionObjectFactory, IEntityObjectFactory
    {
        private readonly IComponentContext _components;
        private readonly TypeMetadataCache _metadataCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityObjectFactory(IComponentContext components, DynamicModule module, TypeMetadataCache metadataCache)
            : base(module)
        {
            _components = components;
            _metadataCache = metadataCache;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public TEntityContract NewEntity<TEntityContract>() where TEntityContract : class
        {
            return CreateInstanceOf<TEntityContract>().UsingConstructor<IComponentContext>(_components, constructorIndex: 1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object NewEntity(Type entityContractType)
        {
            return CreateInstanceOf(entityContractType).UsingConstructor<IComponentContext>(_components, constructorIndex: 1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type GetOrBuildEntityImplementation(Type entityContractInterface) 
        {
            var typeKey = new TypeKey(primaryInterface: entityContractInterface);
            return base.GetOrBuildType(typeKey).DynamicType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeMetadataCache MetadataCache
        {
            get { return _metadataCache; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override IObjectFactoryConvention[] BuildConventionPipeline(ObjectFactoryContext context)
        {
            return new IObjectFactoryConvention[] {
                new EntityObjectConvention(_metadataCache)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnClassTypeCreated(TypeKey key, TypeEntry type)
        {
            var entityMetadata = (TypeMetadataBuilder)_metadataCache.GetTypeMetadata(key.PrimaryInterface);
            entityMetadata.UpdateImplementation(this, type.DynamicType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EntityObjectConvention : ImplementationConvention
        {
            private readonly TypeMetadataCache _metadataCache;
            private readonly List<Action<ConstructorWriter>> _initializers;
            private readonly List<Action<ConstructorWriter>> _newEntityInitializers;
            private readonly Dictionary<Type, Dependency> _dependencies;
            private readonly HashSet<PropertyInfo> _baseContractProperties;
            private ITypeMetadata _entityMetadata;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityObjectConvention(TypeMetadataCache metadataCache)
                : base(Will.InspectDeclaration | Will.ImplementBaseClass | Will.FinalizeImplementation)
            {
                _metadataCache = metadataCache;
                _initializers = new List<Action<ConstructorWriter>>();
                _newEntityInitializers = new List<Action<ConstructorWriter>>();
                _dependencies = new Dictionary<Type, Dependency>();
                _baseContractProperties = new HashSet<PropertyInfo>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnInspectDeclaration(ObjectFactoryContext context)
            {
                _entityMetadata = _metadataCache.GetTypeMetadata(context.TypeKey.PrimaryInterface);
                _metadataCache.EnsureRelationalMapping(_entityMetadata);

                if ( _entityMetadata.BaseType != null )
                {
                    var baseTypeKey = new TypeKey(primaryInterface: _entityMetadata.BaseType.ContractType);
                    context.BaseType = base.Context.Factory.FindDynamicType(baseTypeKey);

                    _baseContractProperties.UnionWith(_entityMetadata.BaseType.Properties.Select(p => p.ContractPropertyInfo));
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                ImplementEntityContract(writer, _entityMetadata.ContractType);

                foreach ( var mixinType in _entityMetadata.MixinContractTypes )
                {
                    ImplementEntityContract(writer, mixinType);
                }

                ImplementIEntityObject(writer);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnFinalizeImplementation(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                AddPropertyValueInitializers(writer);

                ImplementDefaultConstructor(writer);
                ImplementNewEntityConstructor(writer);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementDefaultConstructor(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                writer.Constructor(cw => {
                    cw.Base();
                    _initializers.ForEach(init => init(cw));
                });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementNewEntityConstructor(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                writer.Constructor<IComponentContext>((cw, components) => {
                    if ( _entityMetadata.BaseType == null )
                    {
                        cw.Base();
                    }
                    else
                    {
                        cw.Base(components);
                    }

                    _initializers.ForEach(init => init(cw));
                    _dependencies.Values.ForEach(dependency => dependency.WriteResolveStatement(cw, components));
                    _newEntityInitializers.ForEach(init => init(cw));
                });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementEntityContract(ImplementationClassWriter<TypeTemplate.TBase> writer, Type contractType)
            {
                using ( TT.CreateScope<TT.TInterface>(contractType) )
                {
                    var implicitImpl = writer.ImplementInterface<TypeTemplate.TInterface>();

                    implicitImpl.AllProperties(IsScalarReadWriteProperty).ImplementAutomatic();

                    var explicitImpl = writer.ImplementInterfaceExplicitly<TypeTemplate.TInterface>();

                    explicitImpl.AllProperties(IsScalarReadOrWriteOnlyProperty).ForEach(p => ImplementReadOrWriteOnlyScalarProperty(explicitImpl, p));
                    explicitImpl.AllProperties(IsSpecialStorageTypeProperty).ForEach(p => ImplementSpecialStorageTypeProperty(explicitImpl, p));
                    explicitImpl.AllProperties(IsSingleNavigationProperty).ForEach(p => ImplementSingleNavigationProperty(explicitImpl, p));
                    explicitImpl.AllProperties(IsNavigationCollectionProperty).ForEach(p => ImplementNavigationCollectionProperty(explicitImpl, p));
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementReadOrWriteOnlyScalarProperty(
                ImplementationClassWriter<TypeTemplate.TInterface> explicitImplementation,
                PropertyInfo property)
            {
                using ( TT.CreateScope<TT.TProperty>(property.PropertyType) )
                { 
                    var backingField = explicitImplementation.Field<TT.TProperty>("m_" + property.Name);
                    explicitImplementation.NewVirtualWritableProperty<TT.TProperty>(property.Name).ImplementAutomatic(backingField);

                    explicitImplementation.Property(property).Implement(
                        getter: p => property.CanRead ? p.Get(m => m.Return(backingField.CastTo<TT.TProperty>())) : null,
                        setter: p => property.CanWrite ? p.Set((m, value) => backingField.Assign(value.CastTo<TT.TProperty>())) : null
                    );

                    explicitImplementation.OwnerClass.SetPropertyBackingField(property, backingField);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementSpecialStorageTypeProperty(ImplementationClassWriter<TT.TInterface> explicitImpl, PropertyInfo property)
            {
                var propertyMetadata = _entityMetadata.GetPropertyByDeclaration(property);
                var storageAdapter = propertyMetadata.RelationalMapping.StorageType;
                var conversionWriter = (IStorageContractConversionWriter)storageAdapter;

                using ( TT.CreateScope<TT.TContract, TT.TValue>(propertyMetadata.ClrType, storageAdapter.StorageDataType) )
                {
                    var contractField = explicitImpl.Field<TT.TContract>("m_" + property.Name + "_ContractValue");
                    var storageField = explicitImpl.Field<TT.TValue>("m_" + property.Name + "_StorageValue");
                    var stateField = explicitImpl.Field<DualValueStates>("m_" + property.Name + "_ValueState");

                    explicitImpl.Property(property).Implement(
                        getter: p => p.Get(m => {
                            m.If(stateField == DualValueStates.Storage).Then(() => {
                                conversionWriter.WriteStorageToContractConversion(m, contractField, storageField);
                                stateField.Assign(stateField | DualValueStates.Contract);
                            });
                            m.Return(contractField.CastTo<TT.TProperty>());
                        }),
                        setter: p => p.Set((m, value) => {
                            contractField.Assign(value.CastTo<TT.TContract>());
                            stateField.Assign(DualValueStates.Contract);
                        })
                    );

                    explicitImpl.NewVirtualWritableProperty<TT.TValue>(property.Name).Implement(
                        getter: p => p.Get(m => {
                            m.If(stateField == DualValueStates.Contract).Then(() => {
                                conversionWriter.WriteContractToStorageConversion(m, contractField, storageField);
                                stateField.Assign(stateField | DualValueStates.Storage);
                            });
                            m.Return(storageField);
                        }),
                        setter: p => p.Set((m, value) => {
                            storageField.Assign(value);
                            stateField.Assign(DualValueStates.Storage);
                        })
                    );
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementSingleNavigationProperty(
                ImplementationClassWriter<TypeTemplate.TInterface> explicitImplementation,
                PropertyInfo property)
            {
                var entityTypeKey = new TypeKey(primaryInterface: property.PropertyType);
                var entityType = base.Context.Factory.FindDynamicType(entityTypeKey);

                using ( TT.CreateScope<TT.TValue>(entityType) )
                {
                    var backingField = explicitImplementation.Field<TT.TValue>("m_" + property.Name);
                    explicitImplementation.NewVirtualWritableProperty<TT.TValue>(property.Name).ImplementAutomatic(backingField);

                    explicitImplementation.Property(property).Implement(
                        getter: p => p.Get(m => m.Return(backingField.CastTo<TT.TProperty>())),
                        setter: p => p.Set((m, value) => backingField.Assign(value.CastTo<TT.TValue>()))
                    );
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementNavigationCollectionProperty(
                ImplementationClassWriter<TypeTemplate.TInterface> explicitImplementation,
                PropertyInfo property)
            {
                Type entityContractType;
                property.PropertyType.IsCollectionType(out entityContractType);

                var entityTypeKey = new TypeKey(primaryInterface: entityContractType);
                var entityType = base.Context.Factory.FindDynamicType(entityTypeKey);

                using ( TT.CreateScope<TT.TValue, TT.TItem>(entityType, entityContractType) )
                {
                    var backingField = explicitImplementation.Field<ICollection<TT.TValue>>("m_" + property.Name);
                    var adapterField = explicitImplementation.Field<CollectionAdapter<TT.TValue, TT.TItem>>("m_" + property.Name + "_Adapter");

                    _initializers.Add(new Action<ConstructorWriter>(cw => {
                        using ( TT.CreateScope<TT.TValue, TT.TItem>(entityType, entityContractType) )
                        {
                            backingField.Assign(cw.New<HashSet<TT.TValue>>());
                            adapterField.Assign(cw.New<CollectionAdapter<TT.TValue, TT.TItem>>(backingField));
                        }
                    }));

                    explicitImplementation.NewVirtualWritableProperty<ICollection<TT.TValue>>(property.Name).Implement(
                        getter: p => p.Get(m => m.Return(backingField)),
                        setter: p => p.Set((m, value) => {
                            adapterField.Assign(m.New<CollectionAdapter<TT.TValue, TT.TItem>>(value));
                            backingField.Assign(value);
                        }));

                    explicitImplementation.Property(property).Implement(
                        getter: p => p.Get(m => m.Return(adapterField.CastTo<TT.TProperty>()))
                    );
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementIEntityObject(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                var interfaceImpl = writer.ImplementInterfaceExplicitly<IEntityObject>();

                interfaceImpl.Property(x => x.ContractType).Implement(
                    getter: p => p.Get(w => 
                        w.Return(w.Const(_entityMetadata.ContractType))));

                if ( _entityMetadata.PrimaryKey == null )
                {
                    interfaceImpl.Method<IEntityId>(x => x.GetId).Throw<NotSupportedException>("Entity has no primary key");
                    interfaceImpl.Method<object>(x => x.SetId).Throw<NotSupportedException>("Entity has no primary key");
                }
                else if ( ThisContractDeclaresKeyProperties() )
                {
                    interfaceImpl.Method<IEntityId>(x => x.GetId).Implement(ImplementEntityObjectGetId);
                    interfaceImpl.Method<object>(x => x.SetId).Implement(ImplementEntityObjectSetId);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementEntityObjectGetId(FunctionMethodWriter<IEntityId> writer)
            {
                var w = writer;

                if ( _entityMetadata.PrimaryKey.Properties.Count != 1 )
                {
                    w.Throw<NotSupportedException>("Currently only scalar primary keys are supported.");
                }
                else
                {
                    var keyProperty = _entityMetadata.PrimaryKey.Properties[0].ContractPropertyInfo;

                    using ( TT.CreateScope<TT.TContract, TT.TKey>(_entityMetadata.ContractType, keyProperty.PropertyType) )
                    {
                        w.Return(w.New<EntityId<TT.TContract, TT.TKey>>(w.This<TT.TContract>().Prop<TT.TKey>(keyProperty)));
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementEntityObjectSetId(VoidMethodWriter writer, Argument<object> value)
            {
                var w = writer;

                if ( _entityMetadata.PrimaryKey.Properties.Count != 1 )
                {
                    w.Throw<NotSupportedException>("Currently only scalar primary keys are supported.");
                }
                else
                {
                    var keyProperty = _entityMetadata.PrimaryKey.Properties[0].ContractPropertyInfo;
                    var backingField = w.OwnerClass.GetPropertyBackingField(keyProperty);

                    using ( TT.CreateScope<TT.TContract, TT.TKey>(_entityMetadata.ContractType, keyProperty.PropertyType) )
                    {
                        backingField.AsOperand<TT.TKey>().Assign(value.CastTo<TT.TKey>());
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void AddPropertyValueInitializers(ImplementationClassWriter<TT.TBase> writer)
            {
                foreach ( var property in _entityMetadata.Properties.Where(IsScalarProperty).Cast<PropertyMetadataBuilder>() )
                {
                    if ( property.DefaultValue != null )
                    {
                        AddDefaultValueInitializer(writer, property);
                    }
                    else if ( property.DefaultValueGeneratorType != null )
                    {
                        AddGeneratedValueInitializer(writer, property);
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void AddDefaultValueInitializer(ImplementationClassWriter<TypeTemplate.TBase> writer, PropertyMetadataBuilder property)
            {
                _newEntityInitializers.Add(cw => {
                    using ( TT.CreateScope<TT.TProperty>(property.ClrType) )
                    {
                        IOperand<TT.TProperty> defaultValue;

                        if ( property.TryGetDefaultValueOperand(cw, out defaultValue) )
                        {
                            var backingField = writer.OwnerClass.GetPropertyBackingField(property.ContractPropertyInfo).AsOperand<TT.TProperty>();
                            backingField.Assign(defaultValue);
                        }
                    }
                });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void AddGeneratedValueInitializer(ImplementationClassWriter<TypeTemplate.TBase> writer, PropertyMetadataBuilder property)
            {
                var dependency = GetOrAddConstructorDependencyLocal(property.DefaultValueGeneratorType);

                _newEntityInitializers.Add(cw => {
                    using ( TT.CreateScope<TT.TProperty>(property.ClrType) )
                    {
                        var backingField = writer.OwnerClass.GetPropertyBackingField(property.ContractPropertyInfo).AsOperand<TT.TProperty>();
                        backingField.Assign(dependency.ResolvedLocal
                            .CastTo<IPropertyValueGenerator<TT.TProperty>>()
                            .Func<string, TT.TProperty>(x => x.GenerateValue, cw.Const(property.ContractQualifiedName)));
                    }
                });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private Dependency GetOrAddConstructorDependencyLocal(Type dependencyType)
            {
                return _dependencies.GetOrAdd(
                    dependencyType, 
                    key => new Dependency(dependencyType));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool ThisContractDeclaresKeyProperties()
            {
                return (
                    _entityMetadata.PrimaryKey != null &&
                    _entityMetadata.PrimaryKey.Properties.Any(p => !IsImplementedByBaseEntity(p.ContractPropertyInfo)));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsImplementedByBaseEntity(PropertyInfo property)
            {
                return _baseContractProperties.Contains(property);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsScalarReadWriteProperty(PropertyInfo property)
            {
                if ( IsImplementedByBaseEntity(property) )
                {
                    return false;
                }

                var propertyMetadata = _entityMetadata.GetPropertyByDeclaration(property);
                
                return (
                    propertyMetadata.Kind == PropertyKind.Scalar && 
                    propertyMetadata.RelationalMapping.StorageType == null &&
                    property.CanRead && 
                    property.CanWrite);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsScalarReadOrWriteOnlyProperty(PropertyInfo property)
            {
                if ( IsImplementedByBaseEntity(property) )
                {
                    return false;
                }

                var propertyMetadata = _entityMetadata.GetPropertyByDeclaration(property);

                return (
                    propertyMetadata.Kind == PropertyKind.Scalar &&
                    propertyMetadata.RelationalMapping.StorageType == null &&
                    !(property.CanRead && property.CanWrite));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsSpecialStorageTypeProperty(PropertyInfo property)
            {
                if ( IsImplementedByBaseEntity(property) )
                {
                    return false;
                }

                var propertyMetadata = _entityMetadata.GetPropertyByDeclaration(property);
                return (propertyMetadata.Kind == PropertyKind.Scalar && propertyMetadata.RelationalMapping.StorageType != null);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsScalarProperty(IPropertyMetadata property)
            {
                if ( IsImplementedByBaseEntity(property.ContractPropertyInfo) )
                {
                    return false;
                }

                return (property.Kind == PropertyKind.Scalar);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsSingleNavigationProperty(PropertyInfo property)
            {
                if ( IsImplementedByBaseEntity(property) )
                {
                    return false;
                }

                return (!property.PropertyType.IsCollectionType() && IsEntityContract(property.PropertyType));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsNavigationCollectionProperty(PropertyInfo property)
            {
                if ( IsImplementedByBaseEntity(property) )
                {
                    return false;
                }

                Type itemType;
                return (property.PropertyType.IsCollectionType(out itemType) && IsEntityContract(itemType));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsEntityContract(Type type)
            {
                return EntityContractAttribute.IsEntityContract(type);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Dependency
        {
            public Dependency(Type type)
            {
                this.Type = type;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void WriteResolveStatement(MethodWriterBase method, Argument<IComponentContext> components)
            {
                using ( TT.CreateScope<TT.TDependency>(Type) )
                {
                    ResolvedLocal = method.Local<TT.TDependency>();
                    ResolvedLocal.Assign(Static.Func(ResolutionExtensions.Resolve<TT.TDependency>, components));
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Type Type { get; private set; }
            public Local<TT.TDependency> ResolvedLocal { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class CollectionAdapter<TFrom, TTo> : ICollection<TTo>
        {
            private readonly ICollection<TFrom> _innerCollection;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public CollectionAdapter(ICollection<TFrom> innerCollection)
            {
                _innerCollection = innerCollection;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Add(TTo item)
            {
                _innerCollection.Add((TFrom)(object)item);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Clear()
            {
                _innerCollection.Clear();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool Contains(TTo item)
            {
                return _innerCollection.Contains((TFrom)(object)item);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void CopyTo(TTo[] array, int arrayIndex)
            {
                var items = new TFrom[_innerCollection.Count];
                _innerCollection.CopyTo(items, 0);

                for ( int i = 0 ; i < items.Length ; i++ )
                {
                    array[i + arrayIndex] = (TTo)(object)items[i];
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public int Count
            {
                get
                {
                    return _innerCollection.Count;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsReadOnly
            {
                get
                {
                    return _innerCollection.IsReadOnly;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool Remove(TTo item)
            {
                return _innerCollection.Remove((TFrom)(object)item);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerator<TTo> GetEnumerator()
            {
                return (IEnumerator<TTo>)_innerCollection.GetEnumerator();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return _innerCollection.GetEnumerator();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ListAdapter<TFrom, TTo> : CollectionAdapter<TFrom, TTo>, IList<TTo>
        {
            private readonly IList<TFrom> _innerList;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ListAdapter(IList<TFrom> innerList)
                : base(innerList)
            {
                _innerList = innerList;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IList<TTo>

            public int IndexOf(TTo item)
            {
                return _innerList.IndexOf((TFrom)(object)item);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Insert(int index, TTo item)
            {
                _innerList.Insert(index, (TFrom)(object)item);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void RemoveAt(int index)
            {
                _innerList.RemoveAt(index);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TTo this[int index]
            {
                get
                {
                    return (TTo)(object)_innerList[index];
                }
                set
                {
                    _innerList[index] = (TFrom)(object)value;
                }
            }

            #endregion
        }
    }
}


