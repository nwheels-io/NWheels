using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Globalization;
using TT = Hapil.TypeTemplate;

namespace NWheels.TypeModel.Core.StorageTypes
{
    public class DecimalAsInt64StorageType : IStorageDataType<decimal, Int64>, IStorageContractConversionWriter
    {
        public const int MaxDecimalDigits = 10;
        public const int DefaultDecimalDigits = 6;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Int64 ContractToStorage(IPropertyMetadata metaProperty, decimal contractValue)
        {
            return EncodeDecimalAsInt64(contractValue, metaProperty.NumericPrecision.GetValueOrDefault(DefaultDecimalDigits));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public decimal StorageToContract(IPropertyMetadata metaProperty, Int64 storageValue)
        {
            return DecodeDecimalFromInt64(storageValue, metaProperty.NumericPrecision.GetValueOrDefault(DefaultDecimalDigits));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type StorageDataType
        {
            get { return typeof(Int64); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IStorageContractConversionWriter.WriteContractToStorageConversion(
            IPropertyMetadata metaProperty,
            MethodWriterBase method,
            IOperand<TypeTemplate.TContract> contractValue,
            MutableOperand<TypeTemplate.TValue> storageValue)
        {
            var decimalDigits = metaProperty.NumericPrecision.GetValueOrDefault(DefaultDecimalDigits);
            storageValue.Assign(Static.Func(EncodeDecimalAsInt64, contractValue.CastTo<decimal>(), method.Const(decimalDigits)).CastTo<TT.TValue>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IStorageContractConversionWriter.WriteStorageToContractConversion(
            IPropertyMetadata metaProperty,
            MethodWriterBase method,
            MutableOperand<TypeTemplate.TContract> contractValue,
            IOperand<TypeTemplate.TValue> storageValue)
        {
            var decimalDigits = metaProperty.NumericPrecision.GetValueOrDefault(DefaultDecimalDigits);
            contractValue.Assign(Static.Func(DecodeDecimalFromInt64, storageValue.CastTo<Int64>(), method.Const(decimalDigits)).CastTo<TT.TContract>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object IStorageDataType.ContractToStorage(IPropertyMetadata metaProperty, object contractValue)
        {
            return this.ContractToStorage(metaProperty, (decimal)contractValue);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object IStorageDataType.StorageToContract(IPropertyMetadata metaProperty, object storageValue)
        {
            return this.StorageToContract(metaProperty, (Int64)storageValue);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly long[] _s_powersOfTen = new long[] {
            1, 
            10, 
            100, 
            1000, 
            10000, 
            100000, 
            1000000, 
            10000000, 
            100000000, 
            1000000000, 
            10000000000
        };

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Int64 EncodeDecimalAsInt64(decimal value, int decimalDigits)
        {
            return (Int64)(value * _s_powersOfTen[decimalDigits]);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static decimal DecodeDecimalFromInt64(Int64 value, int decimalDigits)
        {
            return (decimal)value / _s_powersOfTen[decimalDigits];
        }
    }
}
