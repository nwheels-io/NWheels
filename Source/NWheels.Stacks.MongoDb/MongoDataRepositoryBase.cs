using System.Collections.Generic;
using System.Linq;
using Autofac;
using MongoDB.Driver;
using NWheels.Concurrency;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.Entities.Core;
using NWheels.Extensions;
using NWheels.Stacks.MongoDb.Factories;

namespace NWheels.Stacks.MongoDb
{
    public abstract class MongoDataRepositoryBase : UnitOfWorkDataRepositoryBase
    {
        private readonly MongoDatabase _database;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected MongoDataRepositoryBase(
            IResourceConsumerScopeHandle consumerScope,
            IComponentContext components,
            IEntityObjectFactory objectFactory, 
            object emptyModel, 
            MongoDatabase database, 
            bool autoCommit)
            : base(consumerScope, components, autoCommit)
        {
            _database = database;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntityContract LazyLoadById<TEntityContract, TId>(TId id)
        {
            TEntityContract entityFromCache;

            if ( base.TryGetFromCache<TEntityContract, TId>(id, out entityFromCache) )
            {
                return entityFromCache;
            }
            else
            {
                var entityRepo = (IMongoEntityRepository)base.GetEntityRepository(typeof(TEntityContract));
                return entityRepo.GetById<TEntityContract>(id);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<TEntityContract> LazyLoadByIdList<TEntityContract, TId>(IEnumerable<TId> idList)
        {
            var result = new List<TEntityContract>();
            var idsNotInCache = new List<TId>();

            foreach ( var id in idList )
            {
                TEntityContract entityFromCache;

                if ( base.TryGetFromCache<TEntityContract, TId>(id, out entityFromCache) )
                {
                    result.Add(entityFromCache);
                }
                else
                {
                    idsNotInCache.Add(id);
                }
            }

            var entityRepo = (IMongoEntityRepository)base.GetEntityRepository(typeof(TEntityContract));
            result.AddRange(entityRepo.GetByIdList<TEntityContract>(idsNotInCache));

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnCommitChanges()
        {
            foreach ( var entityToInsert in base.InsertBatch )
            {
                ((IMongoEntityRepository)base.GetEntityRepository(entityToInsert.ContractType)).CommitInsert(entityToInsert);
            }
            
            foreach ( var entityToUpdate in base.UpdateBatch )
            {
                ((IMongoEntityRepository)base.GetEntityRepository(entityToUpdate.ContractType)).CommitUpdate(entityToUpdate);
            }

            foreach ( var entityToDelete in base.DeleteBatch )
            {
                ((IMongoEntityRepository)base.GetEntityRepository(entityToDelete.ContractType)).CommitDelete(entityToDelete);
            }
        }

        //TODO: make the version below work instead:
        //protected override void OnCommitChanges()
        //{
        //    var insertBatchPerEntityType = base.InsertBatch.GroupBy(e => e.ContractType);
        //    var updateBatchPerEntityType = base.UpdateBatch.GroupBy(e => e.ContractType);
        //    var deleteBatchPerEntityType = base.DeleteBatch.GroupBy(e => e.ContractType);

        //    foreach (var entityGroupToInsert in insertBatchPerEntityType)
        //    {
        //        ((IMongoEntityRepository)base.GetEntityRepository(entityGroupToInsert.Key)).CommitInsert(entityGroupToInsert);
        //    }

        //    foreach (var entityGroupToUpdate in updateBatchPerEntityType)
        //    {
        //        ((IMongoEntityRepository)base.GetEntityRepository(entityGroupToUpdate.Key)).CommitInsert(entityGroupToUpdate);
        //    }

        //    foreach (var entityGroupToDelete in deleteBatchPerEntityType)
        //    {
        //        ((IMongoEntityRepository)base.GetEntityRepository(entityGroupToDelete.Key)).CommitDelete(entityGroupToDelete);
        //    }
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnRollbackChanges()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal MongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal MongoDatabase Database
        {
            get { return _database; }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static MongoDataRepositoryBase ResolveFrom(IComponentContext components)
        {
            return (MongoDataRepositoryBase)components.Resolve<DataRepositoryBase>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static string GetMongoCollectionName(ITypeMetadata metadata, object partitionValue)
        {
            if ( metadata.BaseType != null )
            {
                return GetMongoCollectionName(metadata.BaseType, partitionValue);
            }

            var baseName = metadata.Name.TrimLead("Abstract");
            var partitionSuffix = (partitionValue != null ? "_" + partitionValue.ToString() : "");

            return baseName + partitionSuffix;
        }
    }
}
