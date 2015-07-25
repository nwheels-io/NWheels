using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Autofac;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Extensions;
using NWheels.Utilities;

namespace NWheels.Stacks.MongoDb.Impl
{
    public class MongoEntityRepository<TEntityContract, TEntityImpl> : IEntityRepository<TEntityContract>, IEntityRepository
        where TEntityContract : class
        where TEntityImpl : class, TEntityContract
    {
        private readonly MongoDataRepositoryBase _ownerRepo;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly ITypeMetadata _metadata;
        private readonly IEntityObjectFactory _objectFactory;
        private readonly MongoCollection<TEntityImpl> _objectSet;
        private InterceptingQueryProvider _queryProvider;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoEntityRepository(MongoDataRepositoryBase ownerRepo, ITypeMetadataCache metadataCache, IEntityObjectFactory objectFactory)
        {
            _ownerRepo = ownerRepo;
            _metadataCache = metadataCache;
            _metadata = metadataCache.GetTypeMetadata(typeof(TEntityContract));
            _objectFactory = objectFactory;
            _objectSet = ownerRepo.GetCollection<TEntityImpl>(GetMongoCollectionName(_metadata));
            _queryProvider = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerator<TEntityContract> GetEnumerator()
        {
            _ownerRepo.ValidateOperationalState();
            
            var actualEnumerator = _objectSet.AsQueryable().GetEnumerator();
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
                return _objectSet.AsQueryable().ElementType;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        System.Linq.Expressions.Expression IQueryable.Expression
        {
            get
            {
                _ownerRepo.ValidateOperationalState();
                return _objectSet.AsQueryable().Expression;
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

        public TEntityContract New()
        {
            _ownerRepo.ValidateOperationalState();
            return _objectFactory.NewEntity<TEntityContract>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TConcreteEntity New<TConcreteEntity>() where TConcreteEntity : class, TEntityContract
        {
            _ownerRepo.ValidateOperationalState();
            return _objectFactory.NewEntity<TConcreteEntity>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntityContract New(Type concreteContract)
        {
            _ownerRepo.ValidateOperationalState();
            return (TEntityContract)_objectFactory.NewEntity(concreteContract);
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
            _objectSet.Insert((TEntityImpl)entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Update(TEntityContract entity)
        {
            _ownerRepo.ValidateOperationalState();
            _objectSet.Save((TEntityImpl)entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Delete(TEntityContract entity)
        {
            _ownerRepo.ValidateOperationalState();

            var keyProperty = _metadata.PrimaryKey.Properties[0].GetImplementationBy<MongoEntityObjectFactory>();
            var keyPropertyExpression = PropertyExpression<TEntityImpl, object>(keyProperty);
            var query = Query<TEntityImpl>.EQ(keyPropertyExpression, keyProperty.GetValue(entity));
            
            _objectSet.Remove(query);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntityContract CheckOutOne<TState>(Expression<Func<TEntityContract, bool>> where, Expression<Func<TEntityContract, TState>> stateProperty, TState newStateValue)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoCollection<TEntityImpl> ObjectSet
        {
            get
            {
                return _objectSet;
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
                _actualQueryProvider = ownerRepo.ObjectSet.AsQueryable().Provider;
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
                return new InterceptingQuery(query);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TResult Execute<TResult>(Expression expression)
            {
                var specializedExpression = _expressionSpecializer.Specialize(expression);
                var result = _actualQueryProvider.Execute<TResult>(specializedExpression);
                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object Execute(Expression expression)
            {
                var result = _actualQueryProvider.Execute(expression);
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
                return new ObjectUtility.DependencyInjectingEnumerator<T>(enumerator, _ownerRepo._ownerRepo.Components);
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public Type ElementType
            {
                get
                {
                    var elementType = _underlyingQuery.ElementType;
                    return elementType;
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public Expression Expression
            {
                get
                {
                    var expression = _underlyingQuery.Expression;
                    return expression;
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

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

        private class InterceptingQuery : IOrderedQueryable
        {
            private readonly IQueryable _underlyingQuery;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InterceptingQuery(IQueryable underlyingQuery)
            {
                _underlyingQuery = underlyingQuery;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                var enumerator = _underlyingQuery.GetEnumerator();
                return enumerator;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public Type ElementType
            {
                get
                {
                    var elementType = _underlyingQuery.ElementType;
                    return elementType;
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public Expression Expression
            {
                get
                {
                    var expression = _underlyingQuery.Expression;
                    return expression;
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public IQueryProvider Provider
            {
                get
                {
                    var provider = _underlyingQuery.Provider;
                    return provider;
                }
            }
        }
    }
}
