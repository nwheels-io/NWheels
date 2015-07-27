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
    public abstract class CollectionAdapterDualValueStrategy : DualValueStrategy
    {
        private Type _itemContractType;
        private Type _itemStorageType;
        private Type _concreteCollectionType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected CollectionAdapterDualValueStrategy(
            ObjectFactoryContext factoryContext, 
            ITypeMetadataCache metadataCache, 
            ITypeMetadata metaType, 
            IPropertyMetadata metaProperty,
            Type storageType)
            : base(factoryContext, metadataCache, metaType, metaProperty, storageType)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override void OnBeforeImplementation(ClassWriterBase writer)
        {
            MetaProperty.ContractPropertyInfo.PropertyType.IsCollectionType(out _itemContractType);
            _itemStorageType = FindStorageType(_itemContractType);
            _concreteCollectionType = GetConcreteCollectionType(MetaProperty.ClrType, _itemStorageType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingInitializationConstructor(MethodWriterBase writer, IOperand<IComponentContext> components)
        {
            base.OnWritingInitializationConstructor(writer, components);

            using ( TT.CreateScope<TT.TContract, TT.TImpl, TT.TConcrete>(_itemContractType, _itemStorageType, _concreteCollectionType) )
            {
                base.ContractField.Assign(
                    writer.New<ConcreteToAbstractCollectionAdapter<TT.TImpl, TT.TContract>>(
                        writer.New<TT.TConcrete>()
                    )
                    .CastTo<TT.TProperty>()
                );
                base.StateField.Assign(DualValueStates.Contract);
            }
        }

        #endregion
    }
}
