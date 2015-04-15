using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NWheels.DataObjects.Core.StorageTypes
{
    public class JsonStorageType<TContractValue> : IStorageDataType<TContractValue, string>
    {
        public string ContractToStorage(TContractValue contractValue)
        {
            return JsonConvert.SerializeObject(contractValue);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TContractValue StorageToContract(string storageValue)
        {
            return JsonConvert.DeserializeObject<TContractValue>(storageValue);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type StorageDataType
        {
            get { return typeof(string); }
        }
    }
}
