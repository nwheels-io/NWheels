using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Members;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Entities;
using TT = Hapil.TypeTemplate;

namespace NWheels.Puzzle.EntityFramework.Conventions
{
    public class EfEntityObjectFactory : ConventionObjectFactory
    {
        private readonly TypeMetadataCache _metadataCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EfEntityObjectFactory(DynamicModule module, TypeMetadataCache metadataCache)
            : base(module, context => new[] { new EntityObjectConvention(metadataCache) })
        {
            _metadataCache = metadataCache;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public TEntityContract NewEntity<TEntityContract>() where TEntityContract : class
        {
            return CreateInstanceOf<TEntityContract>().UsingDefaultConstructor();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type GetOrBuildEntityImplementation(Type entityContractInterface) 
        {
            var typeKey = new TypeKey(primaryInterface: entityContractInterface);
            return base.GetOrBuildType(typeKey).DynamicType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnClassTypeCreated(TypeKey key, TypeEntry type)
        {
            var entityMetadata = (TypeMetadataBuilder)_metadataCache.GetTypeMetadata(key.PrimaryInterface);
            entityMetadata.UpdateImplementation(type.DynamicType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class EntityObjectConvention : ImplementationConvention
        {
            private readonly TypeMetadataCache _metadataCache;
            private readonly List<Action<ConstructorWriter>> _initializers;
            private ITypeMetadata _entityMetadata;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public EntityObjectConvention(TypeMetadataCache metadataCache)
                : base(Will.InspectDeclaration | Will.ImplementBaseClass | Will.FinalizeImplementation)
            {
                _metadataCache = metadataCache;
                _initializers = new List<Action<ConstructorWriter>>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnInspectDeclaration(ObjectFactoryContext context)
            {
                _entityMetadata = _metadataCache.GetTypeMetadata(context.TypeKey.PrimaryInterface);
                _metadataCache.EnsureRelationalMapping(_entityMetadata);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementBaseClass(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                ImplementEntityContract(writer, _entityMetadata.ContractType);

                foreach ( var mixinType in _entityMetadata.MixinContractTypes )
                {
                    ImplementEntityContract(writer, mixinType);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnFinalizeImplementation(ImplementationClassWriter<TypeTemplate.TBase> writer)
            {
                AddDefaultValueInitializers(writer);
                writer.Constructor(cw => _initializers.ForEach(init => init(cw)));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementEntityContract(ImplementationClassWriter<TypeTemplate.TBase> writer, Type contractType)
            {
                using ( TT.CreateScope<TT.TInterface>(contractType) )
                {
                    var implicitImpl = writer.ImplementInterface<TypeTemplate.TInterface>();

                    implicitImpl.AllProperties(IsScalarProperty).ImplementAutomatic();

                    var explicitImpl = writer.ImplementInterfaceExplicitly<TypeTemplate.TInterface>();

                    explicitImpl.AllProperties(IsSingleNavigationProperty).ForEach(p => ImplementSingleNavigationProperty(explicitImpl, p));
                    explicitImpl.AllProperties(IsNavigationCollectionProperty).ForEach(p => ImplementNavigationCollectionProperty(explicitImpl, p));
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

            private void AddDefaultValueInitializers(ImplementationClassWriter<TT.TBase> writer)
            {
                foreach ( var property in _entityMetadata.Properties.Where(IsScalarProperty).Cast<PropertyMetadataBuilder>() )
                {
                    if ( property.DefaultValue != null )
                    {
                        _initializers.Add(cw => {
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
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsScalarProperty(PropertyInfo property)
            {
                return (_entityMetadata.GetPropertyByDeclaration(property).Kind == PropertyKind.Scalar);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsScalarProperty(IPropertyMetadata property)
            {
                return (property.Kind == PropertyKind.Scalar);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsSingleNavigationProperty(PropertyInfo property)
            {
                return (!property.PropertyType.IsCollectionType() && IsEntityContract(property.PropertyType));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private bool IsNavigationCollectionProperty(PropertyInfo property)
            {
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
    }
}
