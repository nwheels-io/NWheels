using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Factories;
using NWheels.DataObjects.Core.StorageTypes;
using NWheels.Extensions;
using TT = Hapil.TypeTemplate;
using Hapil.Members;

namespace NWheels.TypeModel.Core.Factories
{
    public abstract class CollectionAdapterDualValueStrategy : PropertyImplementationStrategy
    {
        private readonly Type _storageType;
        private Type _itemContractType;
        private Type _itemConcreteType;
        private Type _concreteCollectionType;
        private Type _collectionAdapterType;
        private Field<TT.TAbstractCollection<TT.TAbstract>> _collectionAdapterField;
        private Field<TT.TConcreteCollection<TT.TConcrete>> _concreteCollectionField;
        private Field<TT.TValue> _storageField;
        private Field<DualValueStates> _stateField;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected CollectionAdapterDualValueStrategy(
            ObjectFactoryContext factoryContext, 
            ITypeMetadataCache metadataCache, 
            ITypeMetadata metaType, 
            IPropertyMetadata metaProperty,
            Type storageType)
            : base(factoryContext, metadataCache, metaType, metaProperty)
        {
            _storageType = storageType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override void OnBeforeImplementation(ImplementationClassWriter<TT.TInterface> writer)
        {
            MetaProperty.ContractPropertyInfo.PropertyType.IsCollectionType(out _itemContractType);
            _itemConcreteType = FindImpementationType(_itemContractType);
            _concreteCollectionType = HelpGetConcreteCollectionType(MetaProperty.ClrType, _itemConcreteType);
            _collectionAdapterType = HelpGetCollectionAdapterType(MetaProperty.ClrType, _itemContractType, _itemConcreteType);

            using ( TT.CreateScope<TT.TValue>(_storageType) )
            {
                using ( TT.CreateScope<TT.TAbstract, TT.TConcrete, TT.TAbstractCollection<TT.TAbstract>, TT.TConcreteCollection<TT.TConcrete>>(
                    _itemContractType, _itemConcreteType, _collectionAdapterType, _concreteCollectionType) )
                {
                    _collectionAdapterField = writer.Field<TT.TAbstractCollection<TT.TAbstract>>("m_" + MetaProperty.Name + "$adapter");
                    _concreteCollectionField = writer.Field<TT.TConcreteCollection<TT.TConcrete>>("m_" + MetaProperty.Name + "$concrete");
                    _storageField = writer.Field<TT.TValue>("m_" + MetaProperty.Name + "$storage");
                    _stateField = writer.Field<DualValueStates>("m_" + MetaProperty.Name + "$state");
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementContractProperty(ImplementationClassWriter<TT.TInterface> writer)
        {
            using ( TT.CreateScope<TT.TValue, TT.TConcreteCollection<TT.TConcrete>, TT.TAbstractCollection<TT.TAbstract>>(
                _storageType, _concreteCollectionType, _collectionAdapterType) )
            {
                writer.ImplementInterfaceExplicitly<TT.TInterface>().Property(MetaProperty.ContractPropertyInfo).Implement(
                    getter: p => p.Get(m => {
                        base.ImplementedContractProperty = p.OwnerProperty.PropertyBuilder;
                        
                        m.If(_stateField == DualValueStates.Storage).Then(() => {
                            OnWritingStorageToConcreteCollectionConversion(m, _concreteCollectionField, _storageField);
                            _collectionAdapterField.Assign(m.New<TT.TAbstractCollection<TT.TAbstract>>(_concreteCollectionField));
                            _stateField.Assign(_stateField | DualValueStates.Contract);
                        });
                        
                        m.Return(_collectionAdapterField.CastTo<TT.TProperty>());
                    })
                );
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementStorageProperty(ImplementationClassWriter<TT.TInterface> writer)
        {
            using ( TT.CreateScope<TT.TValue>(_storageType) )
            {
                writer.ImplementBase<object>().NewVirtualWritableProperty<TT.TValue>(MetaProperty.Name).Implement(
                    getter: p => p.Get(m => {
                        base.ImplementedStorageProperty = p.OwnerProperty.PropertyBuilder;

                        m.If(_stateField == DualValueStates.Contract).Then(() => {
                            OnWritingConcreteCollectionToStorageConversion(m, _concreteCollectionField, _storageField);
                            _stateField.Assign(_stateField | DualValueStates.Storage);
                        });

                        m.Return(_storageField);
                    }),
                    setter: p => p.Set((m, value) => {
                        _storageField.Assign(value);
                        _stateField.Assign(DualValueStates.Storage);
                    })
                );
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingInitializationConstructor(MethodWriterBase writer, Operand<IComponentContext> components)
        {
            base.OnWritingInitializationConstructor(writer, components);

            using ( TT.CreateScope<TT.TAbstractCollection<TT.TAbstract>, TT.TConcreteCollection<TT.TConcrete>>(_collectionAdapterType, _concreteCollectionType) )
            {
                _concreteCollectionField.Assign(writer.New<TT.TConcreteCollection<TT.TConcrete>>());
                _collectionAdapterField.Assign(writer.New<TT.TAbstractCollection<TT.TAbstract>>(_concreteCollectionField));
                _stateField.Assign(DualValueStates.Contract);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingDeepListNestedObjects(MethodWriterBase writer, IOperand<HashSet<object>> nestedObjects)
        {
            var m = writer;

            using ( TT.CreateScope<TT.TContract>(_itemContractType) )
            {
                nestedObjects.UnionWith(_collectionAdapterField.CastTo<IEnumerable<TT.TContract>>().Cast<object>());

                if ( typeof(IHaveNestedObjects).IsAssignableFrom(_itemConcreteType) )
                {
                    m.ForeachElementIn(_collectionAdapterField.CastTo<IEnumerable<TT.TContract>>().OfType<IHaveNestedObjects>()).Do((loop, item) => {
                        item.Void(x => x.DeepListNestedObjects, nestedObjects);
                    });
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override bool OnHasNestedObjects()
        {
            return true;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void OnWritingConcreteCollectionToStorageConversion(
            MethodWriterBase method,
            IOperand<TT.TConcreteCollection<TT.TConcrete>> concreteCollection,
            MutableOperand<TT.TValue> storageValue);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void OnWritingStorageToConcreteCollectionConversion(
            MethodWriterBase method,
            MutableOperand<TT.TConcreteCollection<TT.TConcrete>> concreteCollection,
            IOperand<TT.TValue> storageValue);
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected Type StorageType
        {
            get { return _storageType; }
        }
    }
}
