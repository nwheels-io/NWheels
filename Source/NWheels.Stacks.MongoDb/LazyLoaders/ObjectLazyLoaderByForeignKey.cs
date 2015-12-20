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
    public class ObjectLazyLoaderByForeignKey : IPersistableObjectLazyLoader
    {
        private readonly Type _domainContextType;
        private readonly Type _entityContractType;
        private readonly MongoDatabase _database;
        private readonly string _collectionName;
        private readonly Type _documentType;
        private readonly string _keyPropertyName;
        private readonly object _keyPropertyValue;

        //---------------------------------------------------------------------------------------------------------------------------------------------

        public ObjectLazyLoaderByForeignKey(IEntityRepository entityRepo, string keyPropertyName, object keyPropertyValue)
        {
            _keyPropertyValue = keyPropertyValue;
            _keyPropertyName = keyPropertyName;
            var collection = ((IMongoEntityRepository)entityRepo).GetMongoCollection();

            _domainContextType = entityRepo.OwnerContext.GetType();
            _entityContractType = entityRepo.ContractType;
            _database = collection.Database;
            _collectionName = collection.Name;
            _documentType = entityRepo.ImplementationType;
            _keyPropertyName = keyPropertyName;
            _keyPropertyValue = keyPropertyValue;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------

        public IDomainObject Load(IDomainObject target)
        {
            //PocQueryLog.LogQuery("LLFK[{0}]", _collectionName);

            var collection = _database.GetCollection(_documentType, _collectionName);
            var entityRepo = MongoDataRepositoryBase.ThreadStaticRepositoryStack.Peek(_domainContextType).GetEntityRepository(_entityContractType);

            var query = Query.EQ(_keyPropertyName, BsonValue.Create(_keyPropertyValue));
            var persistableObject = (IPersistableObject)collection.FindOneAs(_documentType, query);

            var domainObject = (IDomainObject)(target ?? entityRepo.New(persistableObject.ContractType));
            var values = persistableObject.ExportValues(entityRepo);
            domainObject.ImportValues(entityRepo, values);

            return domainObject;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------

        public object EntityId
        {
            get { return null; }
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
