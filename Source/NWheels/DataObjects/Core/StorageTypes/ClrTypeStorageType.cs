using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using NWheels.Extensions;

namespace NWheels.DataObjects.Core.StorageTypes
{
    public class ClrTypeStorageType : IStorageDataType<System.Type, string>, IStorageContractConversionWriter
    {
        public string ContractToStorage(Type contractValue)
        {
            return contractValue.AssemblyQualifiedNameNonVersioned();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type StorageToContract(string storageValue)
        {
            return Type.GetType(storageValue);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type StorageDataType
        {
            get { return typeof(string); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IStorageContractConversionWriter.WriteContractToStorageConversion(
            MethodWriterBase method,
            IOperand<TypeTemplate.TContract> contractValue,
            MutableOperand<TypeTemplate.TValue> storageValue)
        {
            storageValue.Assign(
                Static.Func(NWheels.Extensions.TypeExtensions.AssemblyQualifiedNameNonVersioned, contractValue.CastTo<Type>())
                .CastTo<TypeTemplate.TValue>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IStorageContractConversionWriter.WriteStorageToContractConversion(
            MethodWriterBase method,
            MutableOperand<TypeTemplate.TContract> contractValue,
            IOperand<TypeTemplate.TValue> storageValue)
        {
            contractValue.Assign(
                Static.Func(Type.GetType, storageValue.CastTo<string>())
                .CastTo<TypeTemplate.TContract>());
        }
    }
}
