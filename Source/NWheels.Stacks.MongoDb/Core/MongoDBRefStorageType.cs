using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;
using Hapil.Operands;
using Hapil.Writers;
using MongoDB.Bson;
using MongoDB.Driver;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Entities.Core;

namespace NWheels.Stacks.MongoDb.Core
{
    public class MongoDBRefStorageType<TContractValue> : IStorageDataType<TContractValue, MongoDBRef>, IStorageContractConversionWriter
    {
        private readonly ITypeMetadataCache _metadataCache;
        private readonly ITypeMetadata _metaType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoDBRefStorageType(ITypeMetadataCache metadataCache)
        {
            _metadataCache = metadataCache;
            _metaType = _metadataCache.GetTypeMetadata(typeof(TContractValue));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoDBRef ContractToStorage(TContractValue contractValue)
        {
            if ( contractValue == null )
            {
                return new MongoDBRef(collectionName: _metaType.Name, id: null);
            }

            var idValue = ((IEntityObject)contractValue).GetId().GetValue();
            var idBsonValue = BsonValue.Create(idValue);

            return new MongoDBRef(collectionName: _metaType.Name, id: idBsonValue);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TContractValue StorageToContract(MongoDBRef storageValue)
        {
            throw new NotSupportedException("Lazy load is not implemented");
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
            //storageValue.Assign(Static.Func(JsonConvert.SerializeObject, contractValue.CastTo<object>()).CastTo<TypeTemplate.TValue>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IStorageContractConversionWriter.WriteStorageToContractConversion(
            MethodWriterBase method,
            MutableOperand<TypeTemplate.TContract> contractValue,
            IOperand<TypeTemplate.TValue> storageValue)
        {
            //contractValue.Assign(Static.Func(JsonConvert.DeserializeObject<TypeTemplate.TContract>, storageValue.CastTo<string>()).CastTo<TypeTemplate.TContract>());
        }
    }
}
