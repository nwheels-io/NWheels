using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using Autofac;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Extensions;
using NWheels.Utilities;

namespace NWheels.Stacks.EntityFramework
{
    public class EfEntityRepository<TEntityContract, TBaseImpl, TEntityImpl> : IEntityRepository<TEntityContract>, IEntityRepository
        where TEntityContract : class
        where TBaseImpl : class
        where TEntityImpl : class, TEntityContract, TBaseImpl
    {
        private readonly EfDataRepositoryBase _ownerRepo;
        private readonly IDomainObjectFactory _domainObjectFactory;
        private readonly ObjectSet<TBaseImpl> _objectSet;
        private InterceptingQueryProvider _queryProvider;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EfEntityRepository(EfDataRepositoryBase ownerRepo)
        {
            _ownerRepo = ownerRepo;
            _domainObjectFactory = ownerRepo.Components.Resolve<IDomainObjectFactory>();
            _objectSet = ownerRepo.CreateObjectSet<TBaseImpl>();
            _queryProvider = null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerator<TEntityContract> GetEnumerator()
        {
            _ownerRepo.ValidateOperationalState();
            
            var actualEnumerator = _objectSet.AsEnumerable().OfType<TEntityImpl>().Cast<TEntityContract>().GetEnumerator();
            var resultInterceptor = new InterceptingResultEnumerator<TEntityContract>(_ownerRepo, actualEnumerator);

            return resultInterceptor;
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
                return _objectSet.AsQueryable().OfType<TEntityImpl>().ElementType;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        System.Linq.Expressions.Expression IQueryable.Expression
        {
            get
            {
                _ownerRepo.ValidateOperationalState();
                return _objectSet.AsQueryable().OfType<TEntityImpl>().Expression;
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

        #region Implementation of IEntityRepository

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
                throw new NotImplementedException();
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntityContract New()
        {
            var persistableObject = _ownerRepo.EntityFactory.NewEntity<TEntityContract>();
            var domainObject = _domainObjectFactory.CreateDomainObjectInstance<TEntityContract>(persistableObject);

            return domainObject;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TConcreteEntity New<TConcreteEntity>() where TConcreteEntity : class, TEntityContract
        {
            var persistableObject = _ownerRepo.EntityFactory.NewEntity<TConcreteEntity>();
            var domainObject = _domainObjectFactory.CreateDomainObjectInstance<TConcreteEntity>(persistableObject);

            return domainObject;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntityContract New(Type concreteContract)
        {
            var persistableObject = (TEntityContract)_ownerRepo.EntityFactory.NewEntity(concreteContract);
            var domainObject = _domainObjectFactory.CreateDomainObjectInstance<TEntityContract>(persistableObject);

            return domainObject;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IQueryable<TEntityContract> Include(Expression<Func<TEntityContract, object>>[] properties)
        {
            _ownerRepo.ValidateOperationalState();
            return QueryWithIncludedProperties(_objectSet.OfType<TEntityImpl>(), properties);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Insert(TEntityContract entity)
        {
            _ownerRepo.ValidateOperationalState();
            var persistableObject = entity.As<IPersistableObject>();

            _objectSet.AddObject((TEntityImpl)persistableObject);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Update(TEntityContract entity)
        {
            _ownerRepo.ValidateOperationalState();
            var persistableObject = entity.As<IPersistableObject>();

            _objectSet.Attach((TEntityImpl)persistableObject);
            _ownerRepo.ObjectContext.ObjectStateManager.ChangeObjectState(persistableObject, System.Data.Entity.EntityState.Modified);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Delete(TEntityContract entity)
        {
            _ownerRepo.ValidateOperationalState();
            var persistableObject = entity.As<IPersistableObject>();

            _objectSet.DeleteObject((TEntityImpl)persistableObject);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ObjectSet<TBaseImpl> ObjectSet
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
            return propertiesToInclude.Aggregate(query, (current, property) => current.Include(property));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class InterceptingQueryProvider : IQueryProvider
        {
            private readonly EfEntityRepository<TEntityContract, TBaseImpl, TEntityImpl> _ownerRepo;
            private readonly IQueryProvider _actualQueryProvider;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InterceptingQueryProvider(EfEntityRepository<TEntityContract, TBaseImpl, TEntityImpl> ownerRepo)
            {
                _ownerRepo = ownerRepo;
                _actualQueryProvider = ownerRepo.ObjectSet.AsQueryable().Provider;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region IQueryProvider Members

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                var specializedExpression = QueryExpressionSpecializer.Specialize(expression);
                var query = _actualQueryProvider.CreateQuery<TElement>(specializedExpression);
                return new InterceptingQuery<TElement>(_ownerRepo, query);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IQueryable CreateQuery(Expression expression)
            {
                var specializedExpression = QueryExpressionSpecializer.Specialize(expression);
                var query = _actualQueryProvider.CreateQuery(specializedExpression);
                return new InterceptingQuery(_ownerRepo, query);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TResult Execute<TResult>(Expression expression)
            {
                var result = _actualQueryProvider.Execute<TResult>(expression);
                var entity = result as IEntityObject;

                if ( entity != null )
                {
                    ObjectUtility.InjectDependenciesToObject(entity, _ownerRepo._ownerRepo.Components);
                    return (TResult)(object)_ownerRepo._domainObjectFactory.CreateDomainObjectInstance<TEntityContract>((TEntityContract)entity);

                }
                else
                {
                    return default(TResult);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public object Execute(Expression expression)
            {
                var result = _actualQueryProvider.Execute(expression);
                var entity = result as IEntityObject;

                if ( entity != null )
                {
                    ObjectUtility.InjectDependenciesToObject(entity, _ownerRepo._ownerRepo.Components);
                    return _ownerRepo._domainObjectFactory.CreateDomainObjectInstance<TEntityContract>((TEntityContract)entity);
                }
                else
                {
                    return null;
                }
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class InterceptingQuery<T> : IOrderedQueryable<T>
        {
            private readonly EfEntityRepository<TEntityContract, TBaseImpl, TEntityImpl> _ownerRepo;
            private readonly IQueryable<T> _underlyingQuery;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InterceptingQuery(EfEntityRepository<TEntityContract, TBaseImpl, TEntityImpl> ownerRepo, IQueryable<T> underlyingQuery)
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
            private readonly EfEntityRepository<TEntityContract, TBaseImpl, TEntityImpl> _ownerRepo;
            private readonly IQueryable _underlyingQuery;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InterceptingQuery(EfEntityRepository<TEntityContract, TBaseImpl, TEntityImpl> ownerRepo, IQueryable underlyingQuery)
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
            private readonly EfDataRepositoryBase _ownerUnitOfWork;
            private readonly IDomainObjectFactory _domainObjectFactory;
            private readonly IEnumerator<T> _underlyingEnumerator;
            private T _current;
            private bool _hasCurrent;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InterceptingResultEnumerator(EfDataRepositoryBase ownerUnitOfWork, IEnumerator<T> underlyingEnumerator)
            {
                _ownerUnitOfWork = ownerUnitOfWork;
                _domainObjectFactory = ownerUnitOfWork.Components.Resolve<IDomainObjectFactory>();
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
                if (_underlyingEnumerator.MoveNext())
                {
                    var currentEntityObject = _underlyingEnumerator.Current;

                    ObjectUtility.InjectDependenciesToObject(currentEntityObject, _ownerUnitOfWork.Components);

                    _current = _domainObjectFactory.CreateDomainObjectInstance(currentEntityObject);
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
                    if (_hasCurrent)
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
            private readonly EfDataRepositoryBase _ownerUnitOfWork;
            private readonly IDomainObjectFactory _domainObjectFactory;
            private readonly System.Collections.IEnumerator _underlyingEnumerator;
            private object _current;
            private bool _hasCurrent;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InterceptingResultEnumerator(EfDataRepositoryBase ownerUnitOfWork, System.Collections.IEnumerator underlyingEnumerator)
            {
                _ownerUnitOfWork = ownerUnitOfWork;
                _domainObjectFactory = ownerUnitOfWork.Components.Resolve<IDomainObjectFactory>();
                _underlyingEnumerator = underlyingEnumerator;
                _hasCurrent = false;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool MoveNext()
            {
                if ( _underlyingEnumerator.MoveNext() )
                {
                    var currentEntityObject = _underlyingEnumerator.Current;

                    ObjectUtility.InjectDependenciesToObject(currentEntityObject, _ownerUnitOfWork.Components);

                    _current = _domainObjectFactory.CreateDomainObjectInstance(currentEntityObject);
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
                    if (_hasCurrent)
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
            private readonly EfDataRepositoryBase _ownerUnitOfWork;
            private readonly IEnumerable<T> _underlyingEnumerable;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InterceptingResultEnumerable(EfDataRepositoryBase ownerUnitOfWork, IEnumerable<T> underlyingEnumerable)
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntityContract CheckOutOne<TState>(Expression<Func<TEntityContract, bool>> where, Expression<Func<TEntityContract, TState>> stateProperty, TState newStateValue)
        {
            throw new NotImplementedException();
        }
    }
}
