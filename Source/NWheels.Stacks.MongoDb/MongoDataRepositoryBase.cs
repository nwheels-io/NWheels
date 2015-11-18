using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using NWheels.Concurrency;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Entities.Core;
using NWheels.Extensions;
using NWheels.Stacks.MongoDb.Factories;

namespace NWheels.Stacks.MongoDb
{
    public abstract class MongoDataRepositoryBase : UnitOfWorkDataRepositoryBase
    {
        private readonly MongoDatabase _database;
        private readonly IEntityObjectFactory _entityObjectFactory;

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
            _entityObjectFactory = objectFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void BeginLifetimeScope()
        {
            base.BeginLifetimeScope();
            _s_anchor.Current = this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void EndLifetimeScope()
        {
            _s_anchor.Current = null;
            base.EndLifetimeScope();
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

        public TEntityContract LazyLoadOneByForeignKey<TEntityContract, TEntityImpl, TKey>(string keyPropertyName, TKey keyValue)
        {
            return LazyLoadManyByForeignKey<TEntityContract, TEntityImpl, TKey>(keyPropertyName, keyValue).FirstOrDefault();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<TEntityContract> LazyLoadManyByForeignKey<TEntityContract, TEntityImpl, TKey>(string keyPropertyName, TKey keyValue)
        {
            var entityRepo = (IMongoEntityRepository)base.GetEntityRepository(typeof(TEntityContract));
            var mongoCollection = entityRepo.GetMongoCollection();

            var cursor = mongoCollection.FindAs<TEntityImpl>(Query.EQ(keyPropertyName, BsonValue.Create(keyValue)));

            return entityRepo.TrackMongoCursor<TEntityContract, TEntityImpl>(cursor);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IEntityObjectFactory PersistableObjectFactory 
        {
            get
            {
                return _entityObjectFactory;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void OnCommitChanges()
        {
            foreach ( var entityGroup in base.InsertBatch.GroupBy(x => x.As<IObject>().ContractType) )
            {
                ((IMongoEntityRepository)base.GetEntityRepository(entityGroup.Key)).CommitInsert(entityGroup);
            }

            foreach ( var entityGroup in base.UpdateBatch.GroupBy(x => x.As<IObject>().ContractType) )
            {
                ((IMongoEntityRepository)base.GetEntityRepository(entityGroup.Key)).CommitUpdate(entityGroup);
            }

            foreach ( var entityGroup in base.SaveBatch.GroupBy(x => x.As<IObject>().ContractType) )
            {
                ((IMongoEntityRepository)base.GetEntityRepository(entityGroup.Key)).CommitSave(entityGroup);
            }

            foreach ( var entityGroup in base.DeleteBatch.GroupBy(x => x.As<IObject>().ContractType) )
            {
                ((IMongoEntityRepository)base.GetEntityRepository(entityGroup.Key)).CommitDelete(entityGroup);
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly LogicalCallContextAnchor<MongoDataRepositoryBase> _s_anchor = new LogicalCallContextAnchor<MongoDataRepositoryBase>();

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static MongoDataRepositoryBase ResolveFrom(IComponentContext components)
        {
            var instance = _s_anchor.Current;

            if ( instance == null )
            {
                return (MongoDataRepositoryBase)components.Resolve<DataRepositoryBase>();
                //throw new InvalidOperationException("There is currently no instance of MongoDataRepositoryBase associated with the current thread.");
            }
            
            return instance;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static MongoDataRepositoryBase Current
        {
            get
            {
                return _s_anchor.Current;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal static string GetMongoCollectionName(ITypeMetadata metadata, object partitionValue, Func<object, string> partitionNameFunc)
        {
            if ( metadata.BaseType != null )
            {
                return GetMongoCollectionName(metadata.BaseType, partitionValue, partitionNameFunc);
            }

            var baseName = metadata.Name.TrimLead("Abstract");

            if ( partitionValue != null )
            {
                var nameProperty = partitionValue.GetType().GetProperty("Name");
                partitionValue = nameProperty.GetValue(partitionValue);
            }

            var partitionSuffix = (partitionValue != null ? "_" + partitionValue.ToString() : "");

            return baseName + partitionSuffix;
        }
    }
}
