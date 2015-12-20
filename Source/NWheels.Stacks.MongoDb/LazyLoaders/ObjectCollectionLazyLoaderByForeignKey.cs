using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NWheels.Entities;
using NWheels.Entities.Core;

namespace NWheels.Stacks.MongoDb.LazyLoaders
{
    public class ObjectCollectionLazyLoaderByForeignKey : IPersistableObjectCollectionLazyLoader
    {
        private readonly Type _domainContextType;
        private readonly Type _entityContractType;
        private readonly MongoDatabase _database;
        private readonly string _collectionName;
        private readonly Type _documentType;
        private readonly string _foreignKeyPropertyName;
        private readonly object _foreignKeyValue;

        //---------------------------------------------------------------------------------------------------------------------------------------------

        public ObjectCollectionLazyLoaderByForeignKey(IEntityRepository entityRepo, string foreignKeyPropertyName, object foreignKeyValue)
        {
            var collection = ((IMongoEntityRepository)entityRepo).GetMongoCollection();

            _domainContextType = entityRepo.OwnerContext.GetType();
            _entityContractType = entityRepo.ContractType;
            _database = collection.Database;
            _collectionName = collection.Name;
            _documentType = entityRepo.ImplementationType;

            _foreignKeyValue = foreignKeyValue;
            _foreignKeyPropertyName = foreignKeyPropertyName;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<IDomainObject> Load()
        {
            //PocQueryLog.LogQuery("CLLFK[{0}]", _collectionName);

            var collection = _database.GetCollection(_documentType, _collectionName);
            var entityRepo = MongoDataRepositoryBase.ThreadStaticRepositoryStack.Peek(_domainContextType).GetEntityRepository(_entityContractType);

            var query = Query.EQ(_foreignKeyPropertyName, BsonValue.Create(_foreignKeyValue));
            var cursor = collection.FindAs(_documentType, query);

            return cursor.Cast<IPersistableObject>().Select(persistableObject => {
                var domainObject = (IDomainObject)entityRepo.New(persistableObject.ContractType);
                var values = persistableObject.ExportValues(entityRepo);
                domainObject.ImportValues(entityRepo, values);
                return domainObject;
            });
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------

        public Type EntityContractType
        {
            get { return _entityContractType; }
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------

        public Type DomainContextType
        {
            get { return _domainContextType; }
        }
    }
}
