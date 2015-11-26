using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
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

        private IFramework _framework;
        private int _bulkSize;
        Dictionary<string, BulkInfo> _bulkInfoMap = new Dictionary<string, BulkInfo>();


        public AutoIncrementIntegerIdGenerator(IFramework framework)
        {
            _framework = framework;
            _bulkSize = 50;
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
                handler.BulkSize = _bulkSize;
                db.InvokeGenericOperation(typeof(IAutoIncrementIdEntity), handler);
                bulkInfo = new BulkInfo()
                {
                    LastGiven = handler.NewMaxInBulk - _bulkSize + 1,
                    MaxInBulk = handler.NewMaxInBulk
                };
                _bulkInfoMap[qualifiedPropertyName] = bulkInfo;
                return bulkInfo.LastGiven;
            }

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
