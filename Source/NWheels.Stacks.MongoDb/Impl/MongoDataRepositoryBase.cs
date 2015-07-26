using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Autofac;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NWheels.Conventions.Core;
using NWheels.DataObjects.Core;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Utilities;

namespace NWheels.Stacks.MongoDb.Impl
{
    public abstract class MongoDataRepositoryBase : UnitOfWorkDataRepositoryBase
    {
        private readonly MongoDatabase _database;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected MongoDataRepositoryBase(
            IComponentContext components,
            IEntityObjectFactory objectFactory, 
            object emptyModel, 
            MongoDatabase database, 
            bool autoCommit)
            : base(components, autoCommit)
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
                return (TEntityContract)entityRepo.GetById(id);
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
            result.AddRange(entityRepo.GetByIdList(idsNotInCache).Cast<TEntityContract>());

            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnCommitChanges()
        {
            foreach ( var entityToInsert in base.InsertQueue )
            {
                ((IMongoEntityRepository)base.GetEntityRepository(entityToInsert.ContractType)).CommitInsert(entityToInsert);
            }
            
            foreach ( var entityToUpdate in base.UpdateQueue )
            {
                ((IMongoEntityRepository)base.GetEntityRepository(entityToUpdate.ContractType)).CommitUpdate(entityToUpdate);
            }

            foreach ( var entityToDelete in base.DeleteQueue )
            {
                ((IMongoEntityRepository)base.GetEntityRepository(entityToDelete.ContractType)).CommitDelete(entityToDelete);
            }
        }

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
    }
}
