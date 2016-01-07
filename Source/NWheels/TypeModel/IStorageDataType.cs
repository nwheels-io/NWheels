using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects
{
    public interface IStorageDataType
    {
        object ContractToStorage(IPropertyMetadata metaProperty, object contractValue);
        object StorageToContract(IPropertyMetadata metaProperty, object storageValue);
        Type StorageDataType { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IStorageDataType<TContractValue, TStorageValue> : IStorageDataType
    {
        TStorageValue ContractToStorage(IPropertyMetadata metaProperty, TContractValue contractValue);
        TContractValue StorageToContract(IPropertyMetadata metaProperty, TStorageValue storageValue);
    }
}
