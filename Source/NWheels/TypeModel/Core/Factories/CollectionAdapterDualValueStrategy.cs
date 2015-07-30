using System;
using System.Collections.Generic;
using System.Linq;
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

namespace NWheels.TypeModel.Core.Factories
{
    public abstract class CollectionAdapterDualValueStrategy : PropertyImplementationStrategy
    {
        private readonly Type _storageType;
        private Type _itemContractType;
        private Type _itemConcreteType;
        private Type _concreteCollectionType;
        private Field<ConcreteToAbstractCollectionAdapter<TT.TConcrete, TT.TAbstract>> _collectionAdapterField;
        private Field<TT.TConcrete2> _concreteCollectionField;
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

            using ( TT.CreateScope<TT.TValue, TT.TConcrete, TT.TAbstract, TT.TConcrete2>(
                _storageType, _itemConcreteType, _itemContractType, _concreteCollectionType) )
            {
                _collectionAdapterField = writer.Field<ConcreteToAbstractCollectionAdapter<TT.TConcrete, TT.TAbstract>>("m_" + MetaProperty.Name + "$adapter");
                _concreteCollectionField = writer.Field<TT.TConcrete2>("m_" + MetaProperty.Name + "$concrete");
                _storageField = writer.Field<TT.TValue>("m_" + MetaProperty.Name + "$storage");
                _stateField = writer.Field<DualValueStates>("m_" + MetaProperty.Name + "$state");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnImplementContractProperty(ImplementationClassWriter<TT.TInterface> writer)
        {
            using ( TT.CreateScope<TT.TValue, TT.TConcrete, TT.TAbstract>(_storageType, _itemConcreteType, _itemContractType) )
            {
                writer.ImplementInterfaceExplicitly<TT.TInterface>().Property(MetaProperty.ContractPropertyInfo).Implement(
                    getter: p => p.Get(m => {
                        m.If(_stateField == DualValueStates.Storage).Then(() => {
                            OnWritingStorageToConcreteCollectionConversion(m, _concreteCollectionField, _storageField);
                            _collectionAdapterField.Assign(m.New<ConcreteToAbstractCollectionAdapter<TT.TConcrete, TT.TAbstract>>(_concreteCollectionField));
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

            using ( TT.CreateScope<TT.TContract, TT.TImpl, TT.TConcrete, TT.TConcrete2>(
                _itemContractType, _itemConcreteType, _concreteCollectionType, _concreteCollectionType) )
            {
                _concreteCollectionField.Assign(writer.New<TT.TConcrete2>());
                _collectionAdapterField.Assign(writer.New<ConcreteToAbstractCollectionAdapter<TT.TConcrete, TT.TAbstract>>(_concreteCollectionField));
                _stateField.Assign(DualValueStates.Contract);
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void OnWritingConcreteCollectionToStorageConversion(
            MethodWriterBase method,
            IOperand<TT.TConcrete2> concreteCollection,
            MutableOperand<TT.TValue> storageValue);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void OnWritingStorageToConcreteCollectionConversion(
            MethodWriterBase method,
            MutableOperand<TT.TConcrete2> concreteCollection,
            IOperand<TT.TValue> storageValue);
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected Type StorageType
        {
            get { return _storageType; }
        }
    }
}
