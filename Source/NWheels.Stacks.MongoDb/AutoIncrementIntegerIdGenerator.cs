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
        private IFramework _framework;
        public AutoIncrementIntegerIdGenerator(IFramework framework)
        {
            _framework = framework;
        }

        public int GenerateValue(string qualifiedPropertyName)
        {
            using (var db = _framework.NewUnitOfWork<IAutoIncrementIdDataRepository>())
            {
                Console.WriteLine("AutoIncrementIntegerIdGenerator::GenerateValue()");
                AutoIncrementHandler handler = new AutoIncrementHandler();
                handler.QualifiedPropertyName = qualifiedPropertyName;
                db.InvokeGenericOperation(typeof(IAutoIncrementIdEntity), handler);
                return handler.Record.Value;
            }
        }

        private class AutoIncrementHandler : IDataRepositoryCallback<IAutoIncrementIdEntity>
        {
            internal string QualifiedPropertyName;
            internal IAutoIncrementIdEntity Record;


            public void Invoke<TEntityImpl>(IEntityRepository<IAutoIncrementIdEntity> repo) where TEntityImpl : IAutoIncrementIdEntity
            {
                var collection = repo.GetMongoCollection();

                FindAndModifyArgs args = new FindAndModifyArgs
                {
                    Query = Query<TEntityImpl>.Where(x => x.Id == QualifiedPropertyName),
                    Update = Update<TEntityImpl>.Inc(x => x.Value, 1), //todo: bucketSize
                    Upsert = true,
                    VersionReturned = FindAndModifyDocumentVersion.Modified,
                };
                FindAndModifyResult result = collection.FindAndModify(args);
                Record = result.GetModifiedDocumentAs<TEntityImpl>();
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
