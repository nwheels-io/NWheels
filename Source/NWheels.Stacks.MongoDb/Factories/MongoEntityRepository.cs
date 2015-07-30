using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Extensions;
using NWheels.Utilities;

namespace NWheels.Stacks.MongoDb.Factories
{
    public class MongoEntityRepository<TEntityContract, TEntityImpl> : IEntityRepository<TEntityContract>, IEntityRepository, IMongoEntityRepository
        where TEntityContract : class
        where TEntityImpl : class, TEntityContract
    {
        private readonly MongoDataRepositoryBase _ownerRepo;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly ITypeMetadata _metadata;
        private readonly IEntityObjectFactory _objectFactory;
        private readonly MongoCollection<TEntityImpl> _mongoCollection;
        private readonly Expression<Func<TEntityImpl, object>> _keyPropertyExpression;
        private InterceptingQueryProvider _queryProvider;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoEntityRepository(MongoDataRepositoryBase ownerRepo, ITypeMetadataCache metadataCache, IEntityObjectFactory objectFactory)
        {
            _ownerRepo = ownerRepo;
            _metadataCache = metadataCache;
            _metadata = metadataCache.GetTypeMetadata(typeof(TEntityContract));
            _keyPropertyExpression = GetKeyPropertyExpression(_metadata);
            _objectFactory = objectFactory;
            _mongoCollection = ownerRepo.GetCollection<TEntityImpl>(GetMongoCollectionName(_metadata));
            _queryProvider = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerator<TEntityContract> GetEnumerator()
        {
            _ownerRepo.ValidateOperationalState();
            
            var actualEnumerator = _mongoCollection.AsQueryable().GetEnumerator();
            var dependencyInjectionWrapper = new ObjectUtility.DependencyInjectingEnumerator<TEntityContract>(actualEnumerator, _ownerRepo.Components);
            
            return dependencyInjectionWrapper;
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
                return _mongoCollection.AsQueryable().Expression;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IQueryProvider IQueryable.Provider
        {
            get
            {
                _ownerRepo.ValidateOperationalState();

                if ( _queryProvider == null )
                {
                    _queryProvider = new InterceptingQueryProvider(this);
                }

                return _queryProvider;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region IEntityRepository members

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

        void IEntityRepository.Insert(object entity)
        {
            this.Insert((TEntityContract)entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        void IEntityRepository.Update(object entity)
        {
            this.Update((TEntityContract)entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IEntityRepository.Delete(object entity)
        {
            this.Delete((TEntityContract)entity);
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

        IEntityObject IMongoEntityRepository.GetById(object id)
        {
            var entity = _mongoCollection.FindOneById(BsonValue.Create(id));

            ObjectUtility.InjectDependenciesToObject(entity, _ownerRepo.Components);
            _ownerRepo.TrackEntity(ref entity, EntityState.RetrievedPristine);

            return (IEntityObject)entity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEnumerable<IEntityObject> IMongoEntityRepository.GetByIdList(System.Collections.IEnumerable idList)
        {
            var query = Query.In("_id", new BsonArray(idList));
            var cursor = _mongoCollection.Find(query);

            return new InterceptingResultEnumerable<IEntityObject>(_ownerRepo, cursor.Cast<IEntityObject>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IMongoEntityRepository.CommitInsert(IEntityObject entity)
        {
            _mongoCollection.Insert(entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        void IMongoEntityRepository.CommitUpdate(IEntityObject entity)
        {
            _mongoCollection.Save(entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        void IMongoEntityRepository.CommitDelete(IEntityObject entity)
        {
            var query = Query<TEntityImpl>.EQ(_keyPropertyExpression, entity.GetId().Value);
            _mongoCollection.Remove(query);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        void IMongoEntityRepository.CommitInsert(IEnumerable<IEntityObject> entities)
        {
            _mongoCollection.InsertBatch(entities.Cast<TEntityImpl>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        void IMongoEntityRepository.CommitUpdate(IEnumerable<IEntityObject> entities)
        {
            foreach ( var entity in entities )
            {
                _mongoCollection.Save(entity);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IMongoEntityRepository.CommitDelete(IEnumerable<IEntityObject> entities)
        {
            foreach ( var entity in entities )
            {
                var query = Query<TEntityImpl>.EQ(_keyPropertyExpression, entity.GetId().Value);
                _mongoCollection.Remove(query);
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntityContract New()
        {
            _ownerRepo.ValidateOperationalState();
            
            var entity = _objectFactory.NewEntity<TEntityContract>();
            _ownerRepo.TrackEntity(ref entity, EntityState.NewPristine);
            
            return entity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TConcreteEntity New<TConcreteEntity>() where TConcreteEntity : class, TEntityContract
        {
            _ownerRepo.ValidateOperationalState();

            var entity = _objectFactory.NewEntity<TConcreteEntity>();
            _ownerRepo.TrackEntity(ref entity, EntityState.NewPristine);

            return entity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntityContract New(Type concreteContract)
        {
            _ownerRepo.ValidateOperationalState();

            var entity = (TEntityContract)_objectFactory.NewEntity(concreteContract);
            _ownerRepo.TrackEntity(ref entity, EntityState.NewPristine);

            return entity;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IQueryable<TEntityContract> Include(Expression<Func<TEntityContract, object>>[] properties)
        {
            _ownerRepo.ValidateOperationalState();
            return this;//QueryWithIncludedProperties(_objectSet.AsQueryable(), properties);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Insert(TEntityContract entity)
        {
            _ownerRepo.ValidateOperationalState();
            _ownerRepo.NotifyEntityState((IEntityObject)entity, EntityState.NewModified);
            //_mongoCollection.Insert((TEntityImpl)entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Update(TEntityContract entity)
        {
            _ownerRepo.ValidateOperationalState();
            _ownerRepo.NotifyEntityState((IEntityObject)entity, EntityState.RetrievedModified);
            //_mongoCollection.Save((TEntityImpl)entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Delete(TEntityContract entity)
        {
            _ownerRepo.ValidateOperationalState();
            _ownerRepo.NotifyEntityState((IEntityObject)entity, EntityState.RetrievedDeleted);

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

        private static string GetMongoCollectionName(ITypeMetadata metadata)
        {
            if ( metadata.BaseType != null )
            {
                return GetMongoCollectionName(metadata.BaseType);
            }

            return metadata.Name;
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
                var specializedExpression = _expressionSpecializer.Specialize(expression);
                var query = _actualQueryProvider.CreateQuery<TElement>(specializedExpression);
                return new InterceptingQuery<TElement>(_ownerRepo, query);
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
                var result = _actualQueryProvider.Execute<TResult>(specializedExpression);

                var entity = result as IEntityObject;

                if ( entity != null )
                {
                    ObjectUtility.InjectDependenciesToObject(entity, _ownerRepo._ownerRepo.Components);
                    _ownerRepo._ownerRepo.TrackEntity(ref entity, EntityState.RetrievedPristine);
                    result = (TResult)entity;
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
                    ObjectUtility.InjectDependenciesToObject(entity, _ownerRepo._ownerRepo.Components);
                    _ownerRepo._ownerRepo.TrackEntity(ref entity, EntityState.RetrievedPristine);
                    result = entity;
                }
                
                return result;
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class InterceptingQuery<T> : IOrderedQueryable<T>
        {
            private readonly MongoEntityRepository<TEntityContract, TEntityImpl> _ownerRepo;
            private readonly IQueryable<T> _underlyingQuery;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InterceptingQuery(MongoEntityRepository<TEntityContract, TEntityImpl> ownerRepo, IQueryable<T> underlyingQuery)
            {
                _ownerRepo = ownerRepo;
                _underlyingQuery = underlyingQuery;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerator<T> GetEnumerator()
            {
                var enumerator = _underlyingQuery.GetEnumerator();
                return new InterceptingResultEnumerator<T>(_ownerRepo._ownerRepo, enumerator);
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
                var enumerator = _underlyingQuery.GetEnumerator();
                return new InterceptingResultEnumerator(_ownerRepo._ownerRepo, enumerator);
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

        private class InterceptingResultEnumerator<T> : IEnumerator<T>
        {
            private readonly MongoDataRepositoryBase _ownerUnitOfWork;
            private readonly IEnumerator<T> _underlyingEnumerator;
            private T _current;
            private bool _hasCurrent;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InterceptingResultEnumerator(MongoDataRepositoryBase ownerUnitOfWork, IEnumerator<T> underlyingEnumerator)
            {
                _ownerUnitOfWork = ownerUnitOfWork;
                _underlyingEnumerator = underlyingEnumerator;
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
                    _current = _underlyingEnumerator.Current;

                    ObjectUtility.InjectDependenciesToObject(_current, _ownerUnitOfWork.Components);
                    _ownerUnitOfWork.TrackEntity(ref _current, EntityState.RetrievedPristine);

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

            public T Current
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

        private class InterceptingResultEnumerator : System.Collections.IEnumerator
        {
            private readonly MongoDataRepositoryBase _ownerUnitOfWork;
            private readonly System.Collections.IEnumerator _underlyingEnumerator;
            private object _current;
            private bool _hasCurrent;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InterceptingResultEnumerator(MongoDataRepositoryBase ownerUnitOfWork, System.Collections.IEnumerator underlyingEnumerator)
            {
                _ownerUnitOfWork = ownerUnitOfWork;
                _underlyingEnumerator = underlyingEnumerator;
                _hasCurrent = false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool MoveNext()
            {
                if ( _underlyingEnumerator.MoveNext() )
                {
                    _current = _underlyingEnumerator.Current;

                    ObjectUtility.InjectDependenciesToObject(_current, _ownerUnitOfWork.Components);
                    _ownerUnitOfWork.TrackEntity(ref _current, EntityState.RetrievedPristine);

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

            public object Current
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
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class InterceptingResultEnumerable<T> : IEnumerable<T>
        {
            private readonly MongoDataRepositoryBase _ownerUnitOfWork;
            private readonly IEnumerable<T> _underlyingEnumerable;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InterceptingResultEnumerable(MongoDataRepositoryBase ownerUnitOfWork, IEnumerable<T> underlyingEnumerable)
            {
                _ownerUnitOfWork = ownerUnitOfWork;
                _underlyingEnumerable = underlyingEnumerable;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IEnumerator<T> GetEnumerator()
            {
                return new InterceptingResultEnumerator<T>(_ownerUnitOfWork, _underlyingEnumerable.GetEnumerator());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}
