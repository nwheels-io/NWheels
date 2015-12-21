using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Autofac;
using MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Logging;
using NWheels.Stacks.MongoDb.Factories;
using NWheels.TypeModel.Core;
using NWheels.Utilities;

namespace NWheels.Stacks.MongoDb
{
    public class MongoEntityRepository<TEntityContract, TEntityImpl> : IEntityRepository<TEntityContract>, IEntityRepository, IMongoEntityRepository
        where TEntityContract : class
        where TEntityImpl : class, TEntityContract
    {
        private readonly MongoDataRepositoryBase _ownerRepo;
        private readonly IFramework _framework;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly IDomainObjectFactory _domainObjectFactory;
        private readonly ITypeMetadata _metadata;
        private readonly IEntityObjectFactory _objectFactory;
        private readonly IMongoDbLogger _logger;
        private readonly MongoCollection<TEntityImpl> _mongoCollection;
        private readonly Expression<Func<TEntityImpl, object>> _keyPropertyExpression;
        private InterceptingQueryProvider _queryProvider;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoEntityRepository(
            MongoDataRepositoryBase ownerRepo, 
            ITypeMetadataCache metadataCache, 
            IEntityObjectFactory objectFactory, 
            object partitionValue = null,
            Func<object, string> partitionNameFunc = null)
        {
            _ownerRepo = ownerRepo;
            _framework = ownerRepo.Components.Resolve<IFramework>();
            _metadataCache = metadataCache;
            _domainObjectFactory = ownerRepo.Components.Resolve<IDomainObjectFactory>();
            _metadata = metadataCache.GetTypeMetadata(typeof(TEntityContract));
            _keyPropertyExpression = GetKeyPropertyExpression(_metadata);
            _objectFactory = objectFactory;
            _logger = ownerRepo.Components.Resolve<IMongoDbLogger>();
            _mongoCollection = ownerRepo.GetCollection<TEntityImpl>(MongoDataRepositoryBase.GetMongoCollectionName(_metadata, partitionValue, partitionNameFunc));
            _queryProvider = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerator<TEntityContract> GetEnumerator()
        {
            _ownerRepo.ValidateOperationalState();

            var queryLog = _logger.ExecutingQuery(((IQueryable)this).Expression, null/*_mongoCollection.AsQueryable().ToMongoQueryText()*/);
            //_logger.QueryPlanExplained(_mongoCollection.AsQueryable().ExplainTyped<TEntityContract>().ToString());

            try
            {
                var underlyingQuery = _mongoCollection.AsQueryable();

                if ( _metadata.BaseType != null )
                {
                    underlyingQuery = underlyingQuery.OfType<TEntityImpl>();
                }

            	var actualEnumerator = underlyingQuery. /*_ownerRepo.AuthorizeQuery(underlyingQuery).*/GetEnumerator();
                var transformingEnumerator = new DelegatingTransformingEnumerator<TEntityImpl, TEntityContract>(
                    actualEnumerator,
                    entity => InjectDependenciesAndTrackAndWrapInDomainObject<TEntityContract>(entity));
                var loggingEnumerator = new ResultLoggingEnumerator<TEntityContract>(transformingEnumerator, _logger, queryLog);

                return loggingEnumerator;
            }
            catch ( Exception e )
            {
                queryLog.Fail(e);
                queryLog.Dispose();
                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IQueryable.ElementType
        {
            get
            {
                _ownerRepo.ValidateOperationalState();
                return _mongoCollection.AsQueryable().ElementType;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        System.Linq.Expressions.Expression IQueryable.Expression
        {
            get
            {
                _ownerRepo.ValidateOperationalState();
                
                if ( _metadata.BaseType != null )
                {
                    return _mongoCollection.AsQueryable().OfType<TEntityImpl>().Expression;
                }
                else
                {
                    return _mongoCollection.AsQueryable().Expression;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IQueryProvider IQueryable.Provider
        {
            get
            {
                _ownerRepo.ValidateOperationalState();
                return SafeGetQueryProvider();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region IEntityRepository members

        public IQueryable<TEntityContract> AsQueryable()
        {
            var authorizedQuery = _ownerRepo.AuthorizeQuery<TEntityContract>(this);
            return authorizedQuery;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object IEntityRepository.New()
        {
            return this.New();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object IEntityRepository.New(Type concreteContract)
        {
            return this.New(concreteContract);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object IEntityRepository.TryGetById(IEntityId id)
        {
            return TryGetById(id);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntityContract TryGetById<TId>(TId id)
        {
            IEntityId entityId = new EntityId<TEntityContract, TId>(id);
            return TryGetById(entityId);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntityContract GetById(IEntityId id)
        {
            var entity = TryGetById(id);

            if ( entity != null )
            {
                return entity;
            }

            throw new EntityNotFoundException(typeof(TEntityContract), id.Value);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntityContract GetById<TId>(TId id)
        {
            var entity = TryGetById(id);

            if ( entity != null )
            {
                return entity;
            }

            throw new EntityNotFoundException(typeof(TEntityContract), id);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEntityId MakeEntityId(object value)
        {
            if ( value is ObjectId )
            {
                return new EntityId<TEntityContract, ObjectId>((ObjectId)value);
            }
            else if ( value is string )
            {
                return new EntityId<TEntityContract, ObjectId>(ObjectId.Parse((string)value));
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntityContract TryGetById(IEntityId id)
        {
            var bsonIdValue = BsonValue.Create(id.Value);
            var persistableObject = _mongoCollection.FindOneById(bsonIdValue);

            if ( persistableObject != null ) //TODO: if not found, does FindOneById() return null or throw an exception?
            {
                var hasDependencies = persistableObject as IHaveDependencies;

                if ( hasDependencies != null )
                {
                    hasDependencies.InjectDependencies(_ownerRepo.Components);
                }

                var domainObject = _domainObjectFactory.CreateDomainObjectInstance<TEntityContract>((TEntityContract)persistableObject);

                if ( _ownerRepo.CanRetrieve<TEntityContract>(domainObject) )
                {
                    return domainObject;
                }
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IEntityRepository.Save(object entity)
        {
            this.Save((TEntityContract)entity.As<IPersistableObject>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IEntityRepository.Insert(object entity)
        {
            this.Insert((TEntityContract)entity.As<IPersistableObject>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        void IEntityRepository.Update(object entity)
        {
            this.Update((TEntityContract)entity.As<IPersistableObject>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IEntityRepository.Delete(object entity)
        {
            this.Delete((TEntityContract)entity.As<IPersistableObject>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        EntityChangeMessage IEntityRepository.CreateChangeMessage(IEnumerable<IDomainObject> entities, EntityState state)
        {
            return EntityChangeMessage.Create<TEntityContract>(_framework, entities, state);
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        Type IEntityRepository.ContractType
        {
            get
            {
                return typeof(TEntityContract);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IEntityRepository.ImplementationType
        {
            get
            {
                return typeof(TEntityImpl);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type PersistableObjectFactoryType
        {
            get
            {
                return typeof(MongoEntityObjectFactory);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        ITypeMetadata IEntityRepository.Metadata
        {
            get
            {
                return _metadata;
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region IMongoEntityRepository Members

        T IMongoEntityRepository.GetById<T>(object id)
        {
            var entity = _mongoCollection.FindOneById(BsonValue.Create(id));
            return InjectDependenciesAndTrackAndCastToContract<T>(entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEnumerable<T> IMongoEntityRepository.GetByIdList<T>(System.Collections.IEnumerable idList)
        {
            var query = Query.In("_id", new BsonArray(idList));
            var cursor = _mongoCollection.Find(query);

            return new DelegatingTransformingEnumerable<TEntityImpl, T>(
                cursor,
                InjectDependenciesAndTrackAndCastToContract<T>);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEnumerable<TContract> IMongoEntityRepository.TrackMongoCursor<TContract, TImpl>(MongoCursor<TImpl> cursor)
        {
            return new DelegatingTransformingEnumerable<TEntityImpl, TContract>(
                cursor.Cast<TEntityImpl>(),
                InjectDependenciesAndTrackAndCastToContract<TContract>);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IMongoEntityRepository.CommitInsert(IEntityObject entity)
        {
            using ( var activity = _logger.ExecutingInsert(_metadata.Name) )
            {
                try
                {
                    entity.As<IDomainObject>().BeforeCommit();
                    var result = _mongoCollection.Insert<TEntityImpl>((TEntityImpl)entity);
                    //_logger.MongoDbWriteResult(result.DocumentsAffected, result.Upserted, result.UpdatedExisting);
                }
                catch ( Exception e )
                {
                    LogMongoDbErrors(e, activity);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        void IMongoEntityRepository.CommitUpdate(IEntityObject entity)
        {
            using ( var activity = _logger.ExecutingSave(_metadata.Name) )
            {
                try
                {
                    entity.As<IDomainObject>().BeforeCommit();
                    var result = _mongoCollection.Save<TEntityImpl>((TEntityImpl)entity);
                    //_logger.MongoDbWriteResult(result.DocumentsAffected, result.Upserted, result.UpdatedExisting);
                }
                catch ( Exception e )
                {
                    LogMongoDbErrors(e, activity);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CommitSave(IEntityObject entity)
        {
            using ( var activity = _logger.ExecutingSave(_metadata.Name) )
            {
                try
                {
                    entity.As<IDomainObject>().BeforeCommit();
                    var result = _mongoCollection.Save<TEntityImpl>((TEntityImpl)entity);
                    //_logger.MongoDbWriteResult(result.DocumentsAffected, result.Upserted, result.UpdatedExisting);
                }
                catch (Exception e)
                {
                    LogMongoDbErrors(e, activity);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IMongoEntityRepository.CommitDelete(IEntityObject entity)
        {
            using ( var activity = _logger.ExecutingDelete(_metadata.Name) )
            {
                try
                {
                    entity.As<IDomainObject>().BeforeCommit();
                    var query = Query<TEntityImpl>.EQ(_keyPropertyExpression, entity.GetId().Value);
                    var result = _mongoCollection.Remove(query);

                    //_logger.MongoDbWriteResult(result.DocumentsAffected, result.Upserted, result.UpdatedExisting);
                }
                catch ( Exception e )
                {
                    LogMongoDbErrors(e, activity);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IMongoEntityRepository.CommitInsert(IEnumerable<IEntityObject> entities)
        {
            using ( var activity = _logger.ExecutingInsert(_metadata.Name) )
            {
                try
                {
                    foreach ( var entity in entities )
                    {
                        entity.As<IDomainObject>().BeforeCommit();
                    }

                    var results = _mongoCollection.InsertBatch<TEntityImpl>(entities.Cast<TEntityImpl>());

                    foreach ( var entity in entities )
                    {
                        entity.As<IDomainObject>().AfterCommit();
                    }

                    //foreach ( var result in results )
                    //{
                    //    _logger.MongoDbWriteResult(result.DocumentsAffected, result.Upserted, result.UpdatedExisting);
                    //}
                }
                catch ( Exception e )
                {
                    LogMongoDbErrors(e, activity);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        void IMongoEntityRepository.CommitUpdate(IEnumerable<IEntityObject> entities)
        {
            CommitSave(entities);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CommitSave(IEnumerable<IEntityObject> entities)
        {
            using ( var activity = _logger.ExecutingSave(_metadata.Name) )
            {
                try
                {
                    var writeOperation = _mongoCollection.InitializeOrderedBulkOperation();
                    int count = 0;

                    foreach ( var entity in entities )
                    {
                        entity.As<IDomainObject>().BeforeCommit();

                        writeOperation
                            .Find(Query<TEntityImpl>.EQ(_keyPropertyExpression, entity.GetId().Value))
                            .Upsert()
                            .UpdateOne(Update<TEntityImpl>.Replace((TEntityImpl)entity));

                        count++;
                    }

                    _logger.WritingEntityBatch(entity: typeof(TEntityContract).Name, operation: "Upsert", size: count);
                    var result = writeOperation.Execute();
                    _logger.BulkWriteResult(
                        entity: typeof(TEntityContract).Name, 
                        operation: "Upsert", 
                        size: count, 
                        inserted: result.InsertedCount,
                        deleted: result.DeletedCount,
                        modified: result.ModifiedCount,
                        matched: result.MatchedCount);
                }
                catch ( Exception e )
                {
                    LogMongoDbErrors(e, activity);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IMongoEntityRepository.CommitDelete(IEnumerable<IEntityObject> entities)
        {
            using ( var activity = _logger.ExecutingDelete(_metadata.Name) )
            {
                try
                {
                    var writeOperation = _mongoCollection.InitializeOrderedBulkOperation();
                    int count = 0;

                    foreach ( var entity in entities )
                    {
                        entity.As<IDomainObject>().BeforeCommit();
                        writeOperation.Find(Query<TEntityImpl>.EQ(_keyPropertyExpression, entity.GetId().Value)).Remove();
                        count++;
                    }

                    _logger.WritingEntityBatch(entity: typeof(TEntityContract).Name, operation: "Upsert", size: count);
                    
                    var result = writeOperation.Execute();

                    _logger.BulkWriteResult(
                        entity: typeof(TEntityContract).Name,
                        operation: "Delete",
                        size: count,
                        inserted: result.InsertedCount,
                        deleted: result.DeletedCount,
                        modified: result.ModifiedCount,
                        matched: result.MatchedCount);
                }
                catch ( Exception e )
                {
                    LogMongoDbErrors(e, activity);
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        MongoCollection IMongoEntityRepository.GetMongoCollection()
        {
            return _mongoCollection;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntityContract New()
        {
            _ownerRepo.ValidateOperationalState();
            _ownerRepo.AuthorizeNew<TEntityContract>();

            var persistableObject = _objectFactory.NewEntity<TEntityContract>();
            return InjectDependenciesAndTrackAndWrapInDomainObject<TEntityContract>((TEntityImpl)persistableObject, shouldTrack: false);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TConcreteEntity New<TConcreteEntity>() where TConcreteEntity : class, TEntityContract
        {
            _ownerRepo.ValidateOperationalState();
            _ownerRepo.AuthorizeNew<TConcreteEntity>();

            var persistableObject = _objectFactory.NewEntity<TConcreteEntity>();
            return InjectDependenciesAndTrackAndWrapInDomainObject<TConcreteEntity>((TEntityImpl)(object)persistableObject, shouldTrack: false);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntityContract New(Type concreteContract)
        {
            _ownerRepo.ValidateOperationalState();
            _ownerRepo.AuthorizeNew<TEntityContract>();
            
            var persistableObject = _objectFactory.NewEntity(concreteContract);
            return InjectDependenciesAndTrackAndWrapInDomainObject<TEntityContract>((TEntityImpl)persistableObject, shouldTrack: false);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IQueryable<TEntityContract> Include(Expression<Func<TEntityContract, object>>[] properties)
        {
            _ownerRepo.ValidateOperationalState();
            return this;//QueryWithIncludedProperties(_objectSet.AsQueryable(), properties);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Save(TEntityContract entity)
        {
            _ownerRepo.ValidateOperationalState();

            var entityState = entity.As<IDomainObject>().State;
            if ( entityState.IsNew() )
            {
                _ownerRepo.AuthorizeInsert<TEntityContract>(entity);
            }

            _ownerRepo.AuthorizeUpdate<TEntityContract>(entity);
            _ownerRepo.SaveEntity((IEntityObject)entity.As<IPersistableObject>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Insert(TEntityContract entity)
        {
            _ownerRepo.ValidateOperationalState();
            _ownerRepo.AuthorizeInsert<TEntityContract>(entity);
            _ownerRepo.NotifyEntityState((IEntityObject)entity.As<IPersistableObject>(), EntityState.NewModified);
            //_mongoCollection.Insert((TEntityImpl)entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Update(TEntityContract entity)
        {
            _ownerRepo.ValidateOperationalState();
            _ownerRepo.AuthorizeUpdate<TEntityContract>(entity);
            _ownerRepo.NotifyEntityState((IEntityObject)entity.As<IPersistableObject>(), EntityState.RetrievedModified);
            //_mongoCollection.Save((TEntityImpl)entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Delete(TEntityContract entity)
        {
            _ownerRepo.ValidateOperationalState();
            _ownerRepo.AuthorizeDelete<TEntityContract>(entity);
            _ownerRepo.NotifyEntityState((IEntityObject)entity.As<IPersistableObject>(), EntityState.RetrievedDeleted);

            //var query = Query<TEntityImpl>.EQ(_keyPropertyExpression, ((IEntityObject)entity).GetId().Value);
            //_mongoCollection.Remove(query);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntityContract CheckOutOne<TState>(Expression<Func<TEntityContract, bool>> where, Expression<Func<TEntityContract, TState>> stateProperty, TState newStateValue)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoCollection<TEntityImpl> MongoCollection
        {
            get
            {
                return _mongoCollection;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IQueryProvider SafeGetQueryProvider()
        {
            if ( _queryProvider == null )
            {
                _queryProvider = new InterceptingQueryProvider(this);
            }

            return _queryProvider;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void LogMongoDbErrors(Exception exception, ILogActivity activity)
        {
            var bulkException = (exception as MongoBulkWriteException);

            if ( bulkException != null && bulkException.WriteErrors != null )
            {
                foreach ( var error in bulkException.WriteErrors )
                {
                    _logger.MongoDbWriteError(error.Message);
                }
            }

            _logger.MongoDbWriteError(exception.Message);
            activity.Fail(exception);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TConcreteContract InjectDependenciesAndTrackAndWrapInDomainObject<TConcreteContract>(TEntityImpl persistableImpl, bool shouldTrack = true)
        {
            TConcreteContract persistableContract = (TConcreteContract)(object)persistableImpl;
            
            ObjectUtility.InjectDependenciesToObject(persistableContract, _ownerRepo.Components);

            if ( shouldTrack )
            {
                _ownerRepo.TrackEntity(ref persistableContract, EntityState.RetrievedPristine);
            }

            var existingDomainObject = persistableContract.AsOrNull<IDomainObject>();

            if ( existingDomainObject != null )
            {
                return (TConcreteContract)existingDomainObject;
            }
            else
            {
                return _domainObjectFactory.CreateDomainObjectInstance<TConcreteContract>(persistableContract);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private TConcreteContract InjectDependenciesAndTrackAndCastToContract<TConcreteContract>(TEntityImpl entity)
        {
            if ( entity != null )
            {
                ObjectUtility.InjectDependenciesToObject(entity, _ownerRepo.Components);
                _ownerRepo.TrackEntity(ref entity, EntityState.RetrievedPristine);

                ((IPersistableObject)entity).EnsureDomainObject();
                //if ( entity.AsOrNull<IDomainObject>() == null )
                //{
                //    _domainObjectFactory.CreateDomainObjectInstance<TConcreteContract>((TConcreteContract)(object)entity);
                //}
            }

            return (TConcreteContract)(object)entity;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static IQueryable<TEntityContract> QueryWithIncludedProperties(
            IQueryable<TEntityContract> query,
            IEnumerable<Expression<Func<TEntityContract, object>>> propertiesToInclude)
        {
            return query;//propertiesToInclude.Aggregate(query, (current, property) => current.Include(property));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static Expression<Func<TEntityImpl, object>> GetKeyPropertyExpression(ITypeMetadata metadata)
        {
            var keyProperty = metadata.PrimaryKey.Properties[0].GetImplementationBy<MongoEntityObjectFactory>();
            var keyPropertyExpression = PropertyExpression<TEntityImpl, object>(keyProperty);
            return keyPropertyExpression;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static Expression<Func<TEntity, TProperty>> PropertyExpression<TEntity, TProperty>(PropertyInfo property)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            return Expression.Lambda<Func<TEntity, TProperty>>(Expression.Convert(Expression.Property(parameter, property), typeof(TProperty)), new[] { parameter });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class InterceptingQueryProvider : IQueryProvider
        {
            private readonly MongoEntityRepository<TEntityContract, TEntityImpl> _ownerRepo;
            private readonly IQueryProvider _actualQueryProvider;
            private readonly MongoQueryExpressionSpecializer _expressionSpecializer;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InterceptingQueryProvider(MongoEntityRepository<TEntityContract, TEntityImpl> ownerRepo)
            {
                _ownerRepo = ownerRepo;
                _actualQueryProvider = ownerRepo.MongoCollection.AsQueryable().Provider;
                _expressionSpecializer = new MongoQueryExpressionSpecializer(ownerRepo._metadata, ownerRepo._metadataCache);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region IQueryProvider Members

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                //var authorizedQuery = _ownerRepo._ownerRepo.AuthorizeQuery<TElement>(query);
                var specializedExpression = _expressionSpecializer.Specialize(expression);
                var query = _actualQueryProvider.CreateQuery<TElement>(specializedExpression);
                return new InterceptingQuery<TElement>(_ownerRepo, query /*authorizedQuery*/, _ownerRepo._logger);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IQueryable CreateQuery(Expression expression)
            {
                var specializedExpression = _expressionSpecializer.Specialize(expression);
                var query = _actualQueryProvider.CreateQuery(specializedExpression);
                return new InterceptingQuery(_ownerRepo, query);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TResult Execute<TResult>(Expression expression)
            {
                var specializedExpression = _expressionSpecializer.Specialize(expression);
                TResult result;

                using ( _ownerRepo._logger.ExecutingQuery(specializedExpression, null) )
                {
                    result = _actualQueryProvider.Execute<TResult>(specializedExpression);
                }

                var entity = result as IEntityObject;

                if ( entity != null )
                {
                    //ObjectUtility.InjectDependenciesToObject(entity, _ownerRepo._ownerRepo.Components);
                    //_ownerRepo._ownerRepo.TrackEntity(ref entity, EntityState.RetrievedPristine);

                    //var domainObject = 
                    //    entity.AsOrNull<IDomainObject>() as TEntityContract ??
                    //    _ownerRepo._domainObjectFactory.CreateDomainObjectInstance<TEntityContract>((TEntityContract)entity);
                    
                    return _ownerRepo.InjectDependenciesAndTrackAndWrapInDomainObject<TResult>((TEntityImpl)entity);
                }

                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object Execute(Expression expression)
            {
                var result = _actualQueryProvider.Execute(expression);
                var entity = result as IEntityObject;

                if ( entity != null )
                {
                    return _ownerRepo.InjectDependenciesAndTrackAndWrapInDomainObject<TEntityContract>((TEntityImpl)entity);

                    //ObjectUtility.InjectDependenciesToObject(entity, _ownerRepo._ownerRepo.Components);
                    //_ownerRepo._ownerRepo.TrackEntity(ref entity, EntityState.RetrievedPristine);

                    //var domainObject =
                    //    entity.AsOrNull<IDomainObject>() as TEntityContract ??
                    //    _ownerRepo._domainObjectFactory.CreateDomainObjectInstance<TEntityContract>((TEntityContract)entity);

                    //return domainObject;
                }
                
                return result;
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------



        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class ResultLoggingEnumerator<T> : IEnumerator<T>
        {
            private readonly IEnumerator<T> _innerEnumerator;
            private readonly IMongoDbLogger _logger;
            private readonly ILogActivity _enumerationActivity;
            private int _rowCount;

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ResultLoggingEnumerator(IEnumerator<T> innerEnumerator, IMongoDbLogger logger, ILogActivity enumerationActivity)
            {
                _enumerationActivity = enumerationActivity;
                _innerEnumerator = innerEnumerator;
                _logger = logger;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IDisposable

            public void Dispose()
            {
                try
                {
                    _enumerationActivity.Dispose();
                    _innerEnumerator.Dispose();
                }
                finally
                {
                    _logger.DisposingQueryResultEnumerator();
                }
            }

            #endregion

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IEnumerator

            public bool MoveNext()
            {
                if ( _innerEnumerator.MoveNext() )
                {
                    //_logger.QueryResult(_innerEnumerator.Current.ToString(), _rowCount);
                    _rowCount++;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void Reset()
            {
                _rowCount = 0;
                _innerEnumerator.Reset();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public T Current
            {
                get
                {
                    return _innerEnumerator.Current;
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class InterceptingQuery<T> : IOrderedQueryable<T>
        {
            private readonly MongoEntityRepository<TEntityContract, TEntityImpl> _ownerRepo;
            private readonly IQueryable<T> _underlyingQuery;
            private readonly IMongoDbLogger _logger;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InterceptingQuery(MongoEntityRepository<TEntityContract, TEntityImpl> ownerRepo, IQueryable<T> underlyingQuery, IMongoDbLogger logger)
            {
                _logger = logger;
                _ownerRepo = ownerRepo;
                _underlyingQuery = underlyingQuery;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerator<T> GetEnumerator()
            {
                var queryActivity = _logger.ExecutingQuery(this.Expression, null /*_underlyingQuery.ToMongoQueryText()*/);
                //_logger.QueryPlanExplained(_underlyingQuery.ExplainTyped<T>().ToString());

                var actualResults = _underlyingQuery.GetEnumerator();

                return new DelegatingTransformingEnumerator<T, T>(
                    new ResultLoggingEnumerator<T>(actualResults, _logger, queryActivity),
                    item => {
                        return _ownerRepo.InjectDependenciesAndTrackAndWrapInDomainObject<T>((TEntityImpl)(object)item);
                    });
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Type ElementType
            {
                get
                {
                    var elementType = _underlyingQuery.ElementType;
                    return elementType;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Expression Expression
            {
                get
                {
                    var expression = _underlyingQuery.Expression;
                    return expression;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IQueryProvider Provider
            {
                get
                {
                    var provider = new InterceptingQueryProvider(_ownerRepo);// _underlyingQuery.Provider;
                    return provider;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class InterceptingQuery : IOrderedQueryable
        {
            private readonly MongoEntityRepository<TEntityContract, TEntityImpl> _ownerRepo;
            private readonly IQueryable _underlyingQuery;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InterceptingQuery(MongoEntityRepository<TEntityContract, TEntityImpl> ownerRepo, IQueryable underlyingQuery)
            {
                _ownerRepo = ownerRepo;
                _underlyingQuery = underlyingQuery;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                throw new NotSupportedException();
                //var enumerator = _underlyingQuery.GetEnumerator();
                //return new InterceptingResultEnumerator<TEntityImpl, TEntityContract>(_ownerRepo._ownerRepo, enumerator, _ownerRepo.);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Type ElementType
            {
                get
                {
                    var elementType = _underlyingQuery.ElementType;
                    return elementType;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Expression Expression
            {
                get
                {
                    var expression = _underlyingQuery.Expression;
                    return expression;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IQueryProvider Provider
            {
                get
                {
                    var provider = _underlyingQuery.Provider;
                    return provider;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #if false

        private class InterceptingResultEnumerator<TIn, TOut> : IEnumerator<TOut>
        {
            private readonly MongoDataRepositoryBase _ownerUnitOfWork;
            private readonly IEnumerator<TIn> _underlyingEnumerator;
            private readonly Func<TIn, TOut> _transform;
            private TOut _current;
            private bool _hasCurrent;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InterceptingResultEnumerator(MongoDataRepositoryBase ownerUnitOfWork, IEnumerator<TIn> underlyingEnumerator, Func<TIn, TOut> transform)
            {
                _ownerUnitOfWork = ownerUnitOfWork;
                _underlyingEnumerator = underlyingEnumerator;
                _transform = transform;
                _hasCurrent = false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                _underlyingEnumerator.Dispose();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool MoveNext()
            {
                if ( _underlyingEnumerator.MoveNext() )
                {
                    _current = _transform(_underlyingEnumerator.Current);

                    //ObjectUtility.InjectDependenciesToObject(_current, _ownerUnitOfWork.Components);
                    //_ownerUnitOfWork.TrackEntity(ref _current, EntityState.RetrievedPristine);

                    _hasCurrent = true;
                }
                else
                {
                    _hasCurrent = false;
                }

                return _hasCurrent;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Reset()
            {
                _underlyingEnumerator.Reset();
                _hasCurrent = false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TOut Current
            {
                get
                {
                    if ( _hasCurrent )
                    {
                        return _current;
                    }
                    else
                    {
                        throw new InvalidOperationException("Current value is not available. Probably at end of sequence.");
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class InterceptingResultEnumerable<TIn, TOut> : IEnumerable<TOut>
        {
            private readonly MongoDataRepositoryBase _ownerUnitOfWork;
            private readonly IEnumerable<TIn> _underlyingEnumerable;
            private readonly Func<TIn, TOut> _transform;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InterceptingResultEnumerable(MongoDataRepositoryBase ownerUnitOfWork, IEnumerable<TIn> underlyingEnumerable, Func<TIn, TOut> transform)
            {
                _transform = transform;
                _ownerUnitOfWork = ownerUnitOfWork;
                _underlyingEnumerable = underlyingEnumerable;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerator<TOut> GetEnumerator()
            {
                return new InterceptingResultEnumerator<TIn, TOut>(_ownerUnitOfWork, _underlyingEnumerable.GetEnumerator(), _transform);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        #endif
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    class AuthorizationQueryable<T> : IQueryable<T>
    {
        private readonly Expression _expression;
        private readonly IQueryable<T> _underlyingQueryable;

        public AuthorizationQueryable(Expression expression, IQueryable<T> underlyingQueryable)
        {
            _underlyingQueryable = underlyingQueryable;
            _expression = expression;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _underlyingQueryable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public Expression Expression
        {
            get { return _expression; }
        }

        public IQueryProvider Provider
        {
            get { return _underlyingQueryable.Provider; }
        }
    }
}
