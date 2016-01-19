using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Entities;
using NWheels.Entities.Core;

namespace NWheels.Stacks.MongoDb
{
    public class AutoIncrementIntegerIdGenerator : IPropertyValueGenerator<int>
    {
        private class BulkInfo
        {
            internal int LastGiven { get; set; }
            internal int MaxInBulk { get; set; }
        }

        private readonly IFramework _framework;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly Hashtable _bulkSizeByQualifiedPropertyName;
        private readonly object _bulkSizeSyncRoot;
        Dictionary<string, BulkInfo> _bulkInfoMap = new Dictionary<string, BulkInfo>();

        public AutoIncrementIntegerIdGenerator(IFramework framework, ITypeMetadataCache metadataCache)
        {
            _metadataCache = metadataCache;
            _framework = framework;
            _bulkSizeByQualifiedPropertyName = new Hashtable();
        }

        //todo: test SetBulkSize...
        public void SetBulkSize<TEntity>(int bulkSize)
        {
            var metaType = _metadataCache.GetTypeMetadata(typeof(TEntity));

            if ( !metaType.IsEntity || metaType.EntityIdProperty == null )
            {
                throw new NotSupportedException("Id property does not exist in type: " + metaType.QualifiedName);
            }

            var qualifiedPropertyName = metaType.EntityIdProperty.ContractQualifiedName;

            lock ( _bulkSizeSyncRoot )
            {
                _bulkSizeByQualifiedPropertyName[qualifiedPropertyName] = bulkSize;
            }
        }

        public int GenerateValue(string qualifiedPropertyName)
        {
            BulkInfo bulkInfo;
            if (_bulkInfoMap.TryGetValue(qualifiedPropertyName, out bulkInfo))
            {
                if (bulkInfo.LastGiven < bulkInfo.MaxInBulk)
                {
                    bulkInfo.LastGiven++;
                    return bulkInfo.LastGiven;
                }
            }

            //note - if reach here need new bulk

            using (var db = _framework.NewUnitOfWork<IAutoIncrementIdDataRepository>())
            {
                AutoIncrementHandler handler = new AutoIncrementHandler();
                handler.QualifiedPropertyName = qualifiedPropertyName;
                handler.BulkSize = GetBulkSize(qualifiedPropertyName);
                db.InvokeGenericOperation(typeof(IAutoIncrementIdEntity), handler);
                bulkInfo = new BulkInfo()
                {
                    LastGiven = handler.NewMaxInBulk - handler.BulkSize + 1,
                    MaxInBulk = handler.NewMaxInBulk
                };
                _bulkInfoMap[qualifiedPropertyName] = bulkInfo;
                return bulkInfo.LastGiven;
            }

        }

        private int GetBulkSize(string qualifiedPropertyName)
        {
            var value = _bulkSizeByQualifiedPropertyName[qualifiedPropertyName];

            if ( value != null )
            {
                return (int)value;
            }

            return 1;
        }

        private class AutoIncrementHandler : IDataRepositoryCallback<IAutoIncrementIdEntity>
        {
            internal string QualifiedPropertyName;
            internal int NewMaxInBulk;
            internal int BulkSize;


            public void Invoke<TEntityImpl>(IEntityRepository<IAutoIncrementIdEntity> repo) where TEntityImpl : IAutoIncrementIdEntity
            {
                var collection = repo.GetMongoCollection();

                FindAndModifyArgs args = new FindAndModifyArgs
                {
                    Query = Query<TEntityImpl>.Where(x => x.Id == QualifiedPropertyName),
                    Update = Update<TEntityImpl>.Inc(x => x.Value, BulkSize), //todo: bucketSize
                    Upsert = true,
                    VersionReturned = FindAndModifyDocumentVersion.Modified,
                };
                FindAndModifyResult result = collection.FindAndModify(args);
                NewMaxInBulk = result.GetModifiedDocumentAs<TEntityImpl>().Value;
            }

        }

    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IAutoIncrementIdEntity
    {
        string Id { get; set; }
        int Value { get; set; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IAutoIncrementIdDataRepository : IApplicationDataRepository
    {
        IEntityRepository<IAutoIncrementIdEntity> AutoIncrementId { get; }
    }

}
