using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using Newtonsoft.Json;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Factories;
using NWheels.Extensions;
using TT = Hapil.TypeTemplate;

namespace NWheels.TypeModel.Core.Factories
{
    public class CollectionAdapterAsJsonStringStrategy : DualValueStrategy
    {
        private Type _itemContractType;
        private Type _itemStorageType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CollectionAdapterAsJsonStringStrategy(
            ObjectFactoryContext factoryContext, 
            ITypeMetadataCache metadataCache, 
            ITypeMetadata metaType, 
            IPropertyMetadata metaProperty)
            : base(factoryContext, metadataCache, metaType, metaProperty, storageType: typeof(string))
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of PropertyImplementationStrategy

        protected override bool OnShouldApply(IPropertyMetadata metaProperty)
        {
            return (metaProperty.Kind == PropertyKind.Part && metaProperty.ClrType.IsCollectionType());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override PropertyImplementationStrategy OnClone(IPropertyMetadata metaProperty)
        {
            return new CollectionAdapterAsJsonStringStrategy(base.FactoryContext, base.MetadataCache, base.MetaType, metaProperty);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnBeforeImplementation(ClassWriterBase writer)
        {
            base.OnBeforeImplementation(writer);

            MetaProperty.ContractPropertyInfo.PropertyType.IsCollectionType(out _itemContractType);
            _itemStorageType = FindStorageType(_itemContractType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingInitializationConstructor(MethodWriterBase writer, IOperand<IComponentContext> components)
        {
            base.ContractField.Assign(writer.New<TT.TProperty>());
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingContractToStorageConversion(
            MethodWriterBase method, 
            IOperand<TT.TProperty> contractValue, 
            MutableOperand<TT.TValue> storageValue)
        {
            storageValue.Assign(Static.Func(JsonConvert.SerializeObject, contractValue.CastTo<object>()).CastTo<TypeTemplate.TValue>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingStorageToContractConversion(
            MethodWriterBase method, 
            MutableOperand<TT.TProperty> contractValue, 
            IOperand<TT.TValue> storageValue)
        {
            var concreteCollectionType = GetConcreteCollectionType(MetaProperty.ClrType, _itemStorageType);

            using ( TT.CreateScope<TT.TValue>(concreteCollectionType) )
            {
                contractValue.Assign(Static.Func(JsonConvert.DeserializeObject<TT.TValue>, storageValue.CastTo<string>()).CastTo<TT.TProperty>());
            }
        }
    }
}
