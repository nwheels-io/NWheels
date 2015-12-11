using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using Autofac;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Stacks.EntityFramework.Factories;
using NWheels.TypeModel.Core.Factories;
using NWheels.Utilities;
using EntityState = System.Data.Entity.EntityState;

namespace NWheels.Stacks.EntityFramework
{
    public class EfEntityRepository<TEntityContract, TBaseImpl, TEntityImpl> : IEntityRepository<TEntityContract>, IEntityRepository
        where TEntityContract : class
        where TBaseImpl : class
        where TEntityImpl : class, TEntityContract, TBaseImpl
    {
        private readonly IFramework _framework;
        private readonly EfDataRepositoryBase _ownerRepo;
        private readonly IDomainObjectFactory _domainObjectFactory;
        private readonly ObjectSet<TBaseImpl> _objectSet;
        private readonly System.Data.Entity.Core.Metadata.Edm.EdmMember _entityKeyMember;
        private InterceptingQueryProvider _queryProvider;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EfEntityRepository(EfDataRepositoryBase ownerRepo)
        {
            _ownerRepo = ownerRepo;
            _framework = ownerRepo.Components.Resolve<IFramework>();
            _domainObjectFactory = ownerRepo.Components.Resolve<IDomainObjectFactory>();
            _objectSet = ownerRepo.CreateObjectSet<TBaseImpl>();
            _entityKeyMember = _objectSet.EntitySet.ElementType.KeyMembers.First();
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
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntityContract TryGetById(IEntityId id)
        {
            var entityKey = new System.Data.Entity.Core.EntityKey(_objectSet.EntitySet.Name, _entityKeyMember.Name, id.Value);
            object persistableObject;

            if ( _ownerRepo.ObjectContext.TryGetObjectByKey(entityKey, out persistableObject) )
            {
                var domainObject = _domainObjectFactory.CreateDomainObjectInstance<TEntityContract>((TEntityContract)persistableObject);
                return domainObject;
            }
            else
            {
                return null;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IEntityRepository.Save(object entity)
        {
            this.Save((TEntityContract)entity);
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

        EntityChangeMessage IEntityRepository.CreateChangeMessage(IEnumerable<IDomainObject> entities, NWheels.Entities.EntityState state)
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
                return typeof(EfEntityObjectFactory);
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
            var persistableObject = _ownerRepo.PersistableObjectFactory.NewEntity<TEntityContract>();
            var domainObject = _domainObjectFactory.CreateDomainObjectInstance<TEntityContract>(persistableObject);

            return domainObject;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TConcreteEntity New<TConcreteEntity>() where TConcreteEntity : class, TEntityContract
        {
            var persistableObject = _ownerRepo.PersistableObjectFactory.NewEntity<TConcreteEntity>();
            var domainObject = _domainObjectFactory.CreateDomainObjectInstance<TConcreteEntity>(persistableObject);

            return domainObject;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntityContract New(Type concreteContract)
        {
            var persistableObject = (TEntityContract)_ownerRepo.PersistableObjectFactory.NewEntity(concreteContract);
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

        public void Save(TEntityContract entity)
        {
            var domainObject = entity.As<IDomainObject>();
            var domainObjectState = domainObject.State;

            if ( domainObjectState != NWheels.Entities.EntityState.RetrievedPristine )
            {
                _ownerRepo.ValidateOperationalState();
                var persistableObject = entity.As<IPersistableObject>();

                _objectSet.AddObject((TEntityImpl)persistableObject);
                _ownerRepo.ObjectContext.ObjectStateManager.ChangeObjectState(persistableObject, GetEfEntityState(domainObjectState));
            }
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

        private IQueryable<TEntityContract> QueryWithIncludedProperties(
            IQueryable<TEntityContract> query,
            IEnumerable<Expression<Func<TEntityContract, object>>> propertiesToInclude)
        {
            var queryWithIncludes = propertiesToInclude.Aggregate(query, (current, property) => current.Include(property));
            return new InterceptingQueryable<TEntityContract>(this, queryWithIncludes);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private System.Data.Entity.EntityState GetEfEntityState(Entities.EntityState domainObjectState)
        {
            if ( domainObjectState.IsDeleted() )
            {
                return EntityState.Deleted;
            }

            if ( domainObjectState.IsNew() )
            {
                return EntityState.Added;
            }

            if ( domainObjectState.IsModified() )
            {
                return EntityState.Modified;
            }

            return EntityState.Unchanged;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class InterceptingQueryable<T> : IQueryable<T>
        {
            private readonly EfEntityRepository<TEntityContract, TBaseImpl, TEntityImpl> _ownerRepo;
            private readonly IQueryable<T> _underlyingQueryable;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public InterceptingQueryable(EfEntityRepository<TEntityContract, TBaseImpl, TEntityImpl> ownerRepo, IQueryable<T> underlyingQueryable)
            {
                _ownerRepo = ownerRepo;
                _underlyingQueryable = underlyingQueryable;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IEnumerable

            public IEnumerator<T> GetEnumerator()
            {
                return new InterceptingResultEnumerator<T>(_ownerRepo._ownerRepo, _underlyingQueryable.GetEnumerator());
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IQueryable

            public Expression Expression
            {
                get
                {
                    return _underlyingQueryable.Expression;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Type ElementType
            {
                get
                {
                    return ((IQueryable<TEntityContract>)_ownerRepo).ElementType;
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public IQueryProvider Provider
            {
                get
                {
                    return ((IQueryable<TEntityContract>)_ownerRepo).Provider;
                }
            }

            #endregion
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
                    return _ownerRepo._domainObjectFactory.CreateDomainObjectInstance<TEntityContract>((TEntityContract)entity);
                }

                return result;
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

                    _current = (currentEntityObject is IObject ? _domainObjectFactory.CreateDomainObjectInstance(currentEntityObject) : currentEntityObject);
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

                    _current = (currentEntityObject is IObject ? _domainObjectFactory.CreateDomainObjectInstance(currentEntityObject) : currentEntityObject);
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
