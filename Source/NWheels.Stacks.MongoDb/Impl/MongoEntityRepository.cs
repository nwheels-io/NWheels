using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Extensions;

namespace NWheels.Stacks.MongoDb.Impl
{
    public class MongoEntityRepository<TEntityContract, TEntityImpl> : IEntityRepository<TEntityContract>
        where TEntityContract : class
        where TEntityImpl : class, TEntityContract
    {
        private readonly MongoDataRepositoryBase _ownerRepo;
        private readonly ITypeMetadataCache _metadataCache;
        private readonly ITypeMetadata _metadata;
        private readonly EntityObjectFactory _objectFactory;
        private readonly MongoCollection<TEntityImpl> _objectSet;
        private InterceptingQueryProvider _queryProvider;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MongoEntityRepository(MongoDataRepositoryBase ownerRepo, ITypeMetadataCache metadataCache, EntityObjectFactory objectFactory)
        {
            _ownerRepo = ownerRepo;
            _metadataCache = metadataCache;
            _metadata = metadataCache.GetTypeMetadata(typeof(TEntityContract));
            _objectFactory = objectFactory;
            _objectSet = ownerRepo.GetCollection<TEntityImpl>(_metadata.Name);
            _queryProvider = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEnumerator<TEntityContract> IEnumerable<TEntityContract>.GetEnumerator()
        {
            _ownerRepo.ValidateOperationalState();
            return _objectSet.AsQueryable().GetEnumerator();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            _ownerRepo.ValidateOperationalState();
            return _objectSet.AsQueryable().GetEnumerator();
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

        public TEntityContract New()
        {
            _ownerRepo.ValidateOperationalState();
            return _objectFactory.NewEntity<TEntityContract>();
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
            return Expression.Lambda<Func<TEntity, TProperty>>(Expression.Property(parameter, property), new[] { parameter });
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
                return query;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IQueryable CreateQuery(Expression expression)
            {
                var specializedExpression = _expressionSpecializer.Specialize(expression);
                var query = _actualQueryProvider.CreateQuery(specializedExpression);
                return query;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TResult Execute<TResult>(Expression expression)
            {
                var specializedExpression = _expressionSpecializer.Specialize(expression);
                return _actualQueryProvider.Execute<TResult>(specializedExpression);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object Execute(Expression expression)
            {
                return _actualQueryProvider.Execute(expression);
            }

            #endregion
        }
    }
}
