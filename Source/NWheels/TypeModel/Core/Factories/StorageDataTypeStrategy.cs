﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.DataObjects.Core.Factories;
using TT = Hapil.TypeTemplate;

namespace NWheels.TypeModel.Core.Factories
{
    public class StorageDataTypeStrategy : DualValueStrategy
    {
        public StorageDataTypeStrategy(
            PropertyImplementationStrategyMap ownerMap,
            ObjectFactoryContext factoryContext, 
            ITypeMetadataCache metadataCache, 
            ITypeMetadata metaType, 
            IPropertyMetadata metaProperty)
            : base(ownerMap, factoryContext, metadataCache, metaType, metaProperty, metaProperty.RelationalMapping.StorageType.StorageDataType)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of DualValueStrategy

        protected override void OnWritingContractToStorageConversion(
            MethodWriterBase method, 
            IOperand<TypeTemplate.TProperty> contractValue, 
            MutableOperand<TypeTemplate.TValue> storageValue)
        {
            var conversiotWriter = (IStorageContractConversionWriter)MetaProperty.RelationalMapping.StorageType;

            using ( TT.CreateScope<TT.TContract>(MetaProperty.ClrType) )
            {
                conversiotWriter.WriteContractToStorageConversion(
                    MetaProperty,
                    method, 
                    contractValue.CastTo<TT.TContract>(), 
                    storageValue);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnWritingStorageToContractConversion(
            MethodWriterBase method,
            MutableOperand<TypeTemplate.TProperty> contractValue,
            IOperand<TypeTemplate.TValue> storageValue)
        {
            var conversiotWriter = (IStorageContractConversionWriter)MetaProperty.RelationalMapping.StorageType;

            using ( TT.CreateScope<TT.TContract>(MetaProperty.ClrType) )
            {
                conversiotWriter.WriteStorageToContractConversion(
                    MetaProperty,
                    method, 
                    (MutableOperand<TT.TContract>)contractValue.CastTo<TT.TContract>(), 
                    storageValue);
            }
        }

        #endregion
    }
}
