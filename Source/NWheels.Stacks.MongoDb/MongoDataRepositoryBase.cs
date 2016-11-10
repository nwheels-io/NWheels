using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Autofac;
using Hapil;
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
        private readonly IDomainContextLogger _logger;
        private static readonly Hashtable _s_mappedObjectNames = new Hashtable();

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
            _logger = components.Resolve<IDomainContextLogger>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void BeginLifetimeScope()
        {
            base.BeginLifetimeScope();
            ThreadStaticRepositoryStack.Push(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void EndLifetimeScope()
        {
            ThreadStaticRepositoryStack.Pop(this);
            base.EndLifetimeScope();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract void InitializeDatabase(MongoDatabase database);

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
            if ( idList == null )
            {
                return new TEntityContract[0];
            }

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
                CommitChangesOnEntityGroup(entityGroup, (repo, entities) => repo.CommitInsert(entities));
                //((IMongoEntityRepository)base.GetEntityRepository(entityGroup.Key)).CommitInsert(entityGroup);
            }

            foreach ( var entityGroup in base.UpdateBatch.GroupBy(x => x.As<IObject>().ContractType) )
            {
                CommitChangesOnEntityGroup(entityGroup, (repo, entities) => repo.CommitUpdate(entities));
                //((IMongoEntityRepository)base.GetEntityRepository(entityGroup.Key)).CommitUpdate(entityGroup);
            }

            foreach ( var entityGroup in base.SaveBatch.GroupBy(x => x.As<IObject>().ContractType) )
            {
                CommitChangesOnEntityGroup(entityGroup, (repo, entities) => repo.CommitSave(entities));
                //((IMongoEntityRepository)base.GetEntityRepository(entityGroup.Key)).CommitSave(entityGroup);
            }

            foreach ( var entityGroup in base.DeleteBatch.GroupBy(x => x.As<IObject>().ContractType) )
            {
                CommitChangesOnEntityGroup(entityGroup, (repo, entities) => repo.CommitDelete(entities));
                //((IMongoEntityRepository)base.GetEntityRepository(entityGroup.Key)).CommitDelete(entityGroup);
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

        public MongoDatabase Database
        {
            get { return _database; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CommitChangesOnEntityGroup(
            IGrouping<Type, IEntityObject> entityGroup,
            Action<IMongoEntityRepository, IEnumerable<IEntityObject>> commitAction)
        {
            var entityArray = entityGroup.ToArray();
            _logger.CommittingEntityGroup(contract: entityGroup.Key, count: entityArray.Length);

            var isPartitionedEntity = (entityArray.Length > 0 && entityArray[0] is IPartitionedObject);

            if (isPartitionedEntity)
            {
                var entityGroupPerPartition = entityArray.GroupBy(e => (IMongoEntityRepository)base.GetEntityRepository(e));

                foreach (var entityPartitionGroup in entityGroupPerPartition)
                {
                    var partitionRepository = entityPartitionGroup.Key;
                    var entityPartitionGroupArray = entityPartitionGroup.ToArray();
                    commitAction(partitionRepository, entityPartitionGroupArray);
                }
            }
            else
            {
                var repository = (IMongoEntityRepository)base.GetEntityRepository(entityGroup.Key);
                commitAction(repository, entityArray);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //private static readonly LogicalCallContextAnchor<Dictionary<Type, Stack<MongoDataRepositoryBase>>> _s_anchor = 
        //    new LogicalCallContextAnchor<Dictionary<Type, Stack<MongoDataRepositoryBase>>>();

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static MongoDataRepositoryBase ResolveFrom(IComponentContext components, Type implType)
        {
            var instance = ThreadStaticRepositoryStack.Peek(implType);// _s_anchor.Current;

            if ( instance == null )
            {
                return (MongoDataRepositoryBase)components.Resolve<DataRepositoryBase>();
                //throw new InvalidOperationException("There is currently no instance of MongoDataRepositoryBase associated with the current thread.");
            }
            
            return instance;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        //public static MongoDataRepositoryBase Current
        //{
        //    get
        //    {
        //        return _s_anchor.Current;
        //    }
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetMongoCollectionName(
            ITypeMetadataCache metadataCache, 
            ITypeMetadata metaType, 
            object partitionValue, 
            Func<object, string> partitionNameFunc)
        {
            if ( metaType.BaseType != null )
            {
                return GetMongoCollectionName(metadataCache, metaType.BaseType, partitionValue, partitionNameFunc);
            }

            var baseName = metaType.Name.TrimLead("Abstract");
            var partitionValueDomainObject = partitionValue.AsOrNull<IDomainObject>();

            if (partitionValueDomainObject != null)
            {
                var partitionValuePropertyName = metaType.PartitionProperty.PartitionValuePropertyName ?? "Name";
                var partitionValueMetaType = metadataCache.GetTypeMetadata(partitionValueDomainObject.ContractType);
                var partitionValueMetaProperty = partitionValueMetaType.GetPropertyByName(partitionValuePropertyName);

                partitionValue = partitionValueMetaProperty.ReadValue(partitionValue);
            }

            var partitionSuffix = (partitionValue != null ? "_" + partitionValue.ToString() : "");
            return GetValidMongoObjectName(baseName + partitionSuffix);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetValidMongoObjectName(string proposedName)
        {
            string result = (string)_s_mappedObjectNames[proposedName];
            if (result == null)
            {
                StringBuilder sb = new StringBuilder(proposedName.Length * 2);
                // For now - the restriction is over the path characters.
                var invalidChars = Path.GetInvalidFileNameChars().ToDictionary(x => x);
                invalidChars.Add(' ', ' '); // Add space - Databases names cannot contain space.
                foreach (var c in proposedName)
                {
                    if (!invalidChars.ContainsKey(c))
                    {
                        sb.Append(c);
                    }
                    else
                    {
                        sb.Append(((int)c).ToString("X2"));
                    }
                }
                result = sb.ToString();
                lock (_s_mappedObjectNames.SyncRoot)
                {
                    _s_mappedObjectNames[proposedName] = result;
                }
            }
            return result;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static class ThreadStaticRepositoryStack
        {
            [ThreadStatic]
            private static Dictionary<Type, Stack<MongoDataRepositoryBase>> _s_stackByImplType;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static void Push(MongoDataRepositoryBase instance)
            {
                if ( _s_stackByImplType == null )
                {
                    _s_stackByImplType = new Dictionary<Type, Stack<MongoDataRepositoryBase>>();
                }

                var stack = GetOrAddStack(instance.GetType());
                stack.Push(instance);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static MongoDataRepositoryBase Peek(Type implType)
            {
                if ( _s_stackByImplType == null )
                {
                    throw new InvalidOperationException("ThreadStaticRepositoryStack is empty.");
                }

                var stack = GetOrAddStack(implType);
                return stack.Peek();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static void Pop(MongoDataRepositoryBase instance)
            {
                var stack = GetOrAddStack(instance.GetType());
                var popped = stack.Pop();

                if ( !_s_stackByImplType.Values.Any(s => s.Count > 0) )
                {
                    _s_stackByImplType = null;
                }

                if ( popped != instance )
                {
                    throw new InvalidOperationException("ThreadStaticRepositoryStack Push/Pop mismatch.");
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private static Stack<MongoDataRepositoryBase> GetOrAddStack(Type implType)
            {
                if (implType == null && _s_stackByImplType.Count == 1)
                {
                    return _s_stackByImplType.Values.First();
                }

                foreach ( var entry in _s_stackByImplType )
                {
                    if ( implType.IsAssignableFrom(entry.Key) )
                    {
                        return entry.Value;
                    }
                }

                if ( implType.IsInterface )
                {
                    throw new InvalidOperationException("Cannot create thread repository stack for contract type. Implementation type is required.");
                }

                var newStack = new Stack<MongoDataRepositoryBase>();
                _s_stackByImplType.Add(implType, newStack);
                return newStack;
            }
        }
    }
}
