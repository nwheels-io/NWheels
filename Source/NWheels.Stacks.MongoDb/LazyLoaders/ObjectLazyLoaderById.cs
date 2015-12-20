using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using NWheels.Entities;
using NWheels.Entities.Core;

namespace NWheels.Stacks.MongoDb.LazyLoaders
{
    public class ObjectLazyLoaderById : IPersistableObjectLazyLoader
    {
        private readonly Type _domainContextType;
        private readonly Type _entityContractType;
        private readonly MongoDatabase _database;
        private readonly string _collectionName;
        private readonly Type _documentType;
        private readonly object _documentId;

        //---------------------------------------------------------------------------------------------------------------------------------------------

        public ObjectLazyLoaderById(IEntityRepository entityRepo, object documentId)
        {
            var collection = ((IMongoEntityRepository)entityRepo).GetMongoCollection();

            _domainContextType = entityRepo.OwnerContext.GetType();
            _entityContractType = entityRepo.ContractType;
            _database = collection.Database;
            _collectionName = collection.Name;
            _documentType = entityRepo.ImplementationType;
            _documentId = documentId;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------

        public IDomainObject Load(IDomainObject target)
        {
            //PocQueryLog.LogQuery("LLID[{0}[{1}]]", _collectionName, _documentId);

            var collection = _database.GetCollection(_collectionName);
            var entityRepo = MongoDataRepositoryBase.ThreadStaticRepositoryStack.Peek(_domainContextType).GetEntityRepository(_entityContractType);

            var persistableObject = (IPersistableObject)collection.FindOneByIdAs(_documentType, BsonValue.Create(_documentId));
            var domainObject = (IDomainObject)(target ?? entityRepo.New(persistableObject.ContractType));
            var values = persistableObject.ExportValues(entityRepo);
            domainObject.ImportValues(entityRepo, values);

            return domainObject;
        }

        //---------------------------------------------------------------------------------------------------------------------------------------------

        public object EntityId
        {
            get { return _documentId; }
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
