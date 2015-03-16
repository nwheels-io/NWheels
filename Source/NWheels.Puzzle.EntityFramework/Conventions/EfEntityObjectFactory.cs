using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Core.DataObjects;
using NWheels.Entities;
using TT = Hapil.TypeTemplate;

namespace NWheels.Puzzle.EntityFramework.Conventions
{
    public class EfEntityObjectFactory : ConventionObjectFactory
    {
        private readonly TypeMetadataCache _metadataCache;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EfEntityObjectFactory(DynamicModule module, TypeMetadataCache metadataCache) 
            : base(module, new EntityObjectConvention())
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
            var implementationType =base.GetOrBuildType(new TypeKey(primaryInterface: entityContractInterface)).DynamicType;
            var typeMetadata = (TypeMetadataBuilder)_metadataCache.GetTypeMetadata(entityContractInterface);
            typeMetadata.ImplementationType = implementationType;
            return implementationType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class EntityObjectConvention : ImplementationConvention
        {
            public EntityObjectConvention()
                : base(Will.ImplementPrimaryInterface)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override void OnImplementPrimaryInterface(ImplementationClassWriter<TypeTemplate.TInterface> writer)
            {
                writer.AllProperties(IsScalarProperty).ImplementAutomatic();

                var initializers = new List<Action<ConstructorWriter>>();
                var explicitImpl = writer.ImplementInterfaceExplicitly<TypeTemplate.TInterface>();

                explicitImpl.AllProperties(IsSingleNavigationProperty).ForEach(p => ImplementSingleNavigationProperty(explicitImpl, p, initializers));
                explicitImpl.AllProperties(IsNavigationCollectionProperty).ForEach(p => ImplementNavigationCollectionProperty(explicitImpl, p, initializers));

                writer.Constructor(cw => initializers.ForEach(init => init(cw)));
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ImplementSingleNavigationProperty(
                ImplementationClassWriter<TypeTemplate.TInterface> explicitImplementation,
                PropertyInfo property,
                List<Action<ConstructorWriter>> initializers)
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
                PropertyInfo property,
                List<Action<ConstructorWriter>> initializers)
            {
                Type entityContractType;
                property.PropertyType.IsCollectionType(out entityContractType);

                var entityTypeKey = new TypeKey(primaryInterface: entityContractType);
                var entityType = base.Context.Factory.FindDynamicType(entityTypeKey);

                using ( TT.CreateScope<TT.TValue, TT.TItem>(entityType, entityContractType) )
                {
                    var backingField = explicitImplementation.Field<ICollection<TT.TValue>>("m_" + property.Name);
                    var adapterField = explicitImplementation.Field<CollectionAdapter<TT.TValue, TT.TItem>>("m_" + property.Name + "_Adapter");

                    initializers.Add(new Action<ConstructorWriter>(cw => {
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

            private bool IsScalarProperty(PropertyInfo property)
            {
                return (!property.PropertyType.IsCollectionType() && !IsEntityContract(property.PropertyType));
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
