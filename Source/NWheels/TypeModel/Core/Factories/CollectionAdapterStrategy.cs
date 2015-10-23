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
using NWheels.DataObjects.Core.StorageTypes;
using NWheels.Extensions;
using ProtoBuf.Meta;
using TT = Hapil.TypeTemplate;

namespace NWheels.TypeModel.Core.Factories
{
    public class CollectionAdapterStrategy : PropertyImplementationStrategy
    {
        private Type _itemContractType;
        private Type _itemStorageType;
        private Type _storageCollectionType;
        private Type _collectionAdapterType;
        private bool _isOrderedCollection;
        private Field<TT.TConcreteCollection<TT.TImpl>> _storageField;
        private Field<TT.TAbstractCollection<TT.TContract>> _contractField;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CollectionAdapterStrategy(
            PropertyImplementationStrategyMap ownerMap,
            ObjectFactoryContext factoryContext, 
            ITypeMetadataCache metadataCache, 
            ITypeMetadata metaType, 
            IPropertyMetadata metaProperty)
            : base(ownerMap, factoryContext, metadataCache, metaType, metaProperty)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override void OnBeforeImplementation(ImplementationClassWriter<TT.TInterface> writer)
        {
            MetaProperty.ContractPropertyInfo.PropertyType.IsCollectionType(out _itemContractType);
            _itemStorageType = FindImplementationType(_itemContractType);
            _storageCollectionType = HelpGetConcreteCollectionType(MetaProperty.ClrType, _itemStorageType);
            _collectionAdapterType = HelpGetCollectionAdapterType(MetaProperty.ClrType, _itemContractType, _itemStorageType, out _isOrderedCollection); 


            using ( TT.CreateScope<TT.TContract, TT.TImpl, TT.TConcreteCollection<TT.TImpl>, TT.TAbstractCollection<TT.TContract>>(
                _itemContractType, _itemStorageType, _storageCollectionType, _collectionAdapterType) )
            {
                _storageField = writer.Field<TT.TConcreteCollection<TT.TImpl>>("m_" + MetaProperty.Name + "$storage");
                _contractField = writer.Field<TT.TAbstractCollection<TT.TContract>>("m_" + MetaProperty.Name + "$adapter");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementContractProperty(ImplementationClassWriter<TT.TInterface> writer)
        {
            writer.Property(MetaProperty.ContractPropertyInfo).Implement(
                getter: p => p.Get(m => {
                    base.ImplementedContractProperty = p.OwnerProperty.PropertyBuilder;
                    m.Return(_contractField.CastTo<TT.TProperty>());
                })
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TT.TInterface> writer)
        {
            using ( TT.CreateScope<TT.TContract, TT.TImpl, TT.TConcreteCollection<TT.TImpl>, TT.TAbstractCollection<TT.TContract>>(
                _itemContractType, _itemStorageType, _storageCollectionType, _collectionAdapterType) )
            {
                writer.ImplementBase<object>().NewVirtualWritableProperty<TT.TConcreteCollection<TT.TImpl>>(MetaProperty.Name).Implement(
                    getter: p => p.Get(m => {
                        base.ImplementedStorageProperty = p.OwnerProperty.PropertyBuilder;
                        m.Return(_storageField);
                    }),
                    setter: p => p.Set((m, value) => {
                        _contractField.Assign(
                            Static.Func<object, bool, object>(RuntimeTypeModelHelpers.CreateCollectionAdapter<TT.TImpl,TT.TContract>, 
                                value, 
                                m.Const(_isOrderedCollection))
                            .CastTo<TT.TAbstractCollection<TT.TContract>>());
                        _storageField.Assign(value);
                    }));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingInitializationConstructor(MethodWriterBase writer, Operand<IComponentContext> components, params IOperand[] args)
        {
            using ( TT.CreateScope<TT.TContract, TT.TImpl, TT.TConcreteCollection<TT.TImpl>, TT.TAbstractCollection<TT.TContract>>(
                _itemContractType, _itemStorageType, _storageCollectionType, _collectionAdapterType) )
            {
                _storageField.Assign(writer.New<TT.TConcreteCollection<TT.TImpl>>());
                _contractField.Assign(
                    Static.Func<object, bool, object>(RuntimeTypeModelHelpers.CreateCollectionAdapter<TT.TImpl, TT.TContract>,
                        _storageField,
                        writer.Const(_isOrderedCollection))
                    .CastTo<TT.TAbstractCollection<TT.TContract>>());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingDeepListNestedObjects(MethodWriterBase writer, IOperand<HashSet<object>> nestedObjects)
        {
            var m = writer;

            using ( TT.CreateScope<TT.TContract>(_itemContractType) )
            {
                Static.Void(RuntimeTypeModelHelpers.DeepListNestedObjectCollection, _contractField.CastTo<System.Collections.IEnumerable>(), nestedObjects);
                

                //m.If(_contractField.IsNotNull()).Then(() => {
                //    nestedObjects.UnionWith(_contractField.CastTo<IEnumerable<TT.TContract>>().Cast<object>());

                //    if ( typeof(IHaveNestedObjects).IsAssignableFrom(_itemStorageType) )
                //    {
                //        m.ForeachElementIn(_contractField.CastTo<IEnumerable<TT.TContract>>().OfType<IHaveNestedObjects>()).Do((loop, item) => {
                //            item.Void(x => x.DeepListNestedObjects, nestedObjects);
                //        });
                //    }
                //});
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override bool OnHasNestedObjects()
        {
            return true;
        }

        #endregion
    }
}
