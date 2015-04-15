using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects
{
    public interface IStorageDataType
    {
        Type StorageDataType { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IStorageDataType<TContractValue, TStorageValue> : IStorageDataType
    {
        TStorageValue ContractToStorage(TContractValue contractValue);
        TContractValue StorageToContract(TStorageValue storageValue);
    }
}
