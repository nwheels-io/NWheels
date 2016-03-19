using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NWheels.Conventions.Core;
using NWheels.DataObjects;
using NWheels.DataObjects.Core;
using NWheels.Entities;
using NWheels.Entities.Core;
using NWheels.Exceptions;
using NWheels.Extensions;
using NWheels.Testing.Entities.Impl;
using NWheels.Utilities;

namespace NWheels.Testing.Entities
{
    public class TestEntityRepository<TEntity, TEntityId> : IEntityRepository<TEntity>, IEntityRepository, IQueryable<TEntity>
        where TEntity : class
    {
        private readonly EntityObjectFactory _objectFactory;
        private readonly HashSet<TEntity> _storedEntities;
        private readonly IDomainObjectFactory _domainObjectFactory;
        private readonly IComponentContext _components;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TestEntityRepository(IComponentContext components, EntityObjectFactory objectFactory, IDomainObjectFactory domainObjectFactory)
        {
            _components = components;
            _objectFactory = objectFactory;
            _storedEntities = new HashSet<TEntity>();
            _domainObjectFactory = domainObjectFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IEntityRepository

        public IQueryable<TEntity> AsQueryable()
        {
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEnumerable IEntityRepository.AsEnumerabe()
        {
            return this.AsQueryable();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object IEntityRepository.New()
        {
            return ((IEntityRepository<TEntity>)this).New();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object IEntityRepository.New(Type concreteContract)
        {
            return ((IEntityRepository<TEntity>)this).New(concreteContract);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object IEntityRepository.TryGetById(IEntityId id)
        {
            return TryGetById(id);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object[] IEntityRepository.GetByIdList(object[] idValues)
        {
            return idValues.Cast<TEntityId>().Select(id => TryGetById<TEntityId>(id)).Cast<object>().ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        object[] IEntityRepository.GetByForeignKeyList(IPropertyMetadata foreignKeyProperty, object[] foreignKeyValues)
        {
            var results = new List<object>();

            foreach ( var entity in _storedEntities )
            {
                var value = foreignKeyProperty.ReadValue(entity);
                
                if ( foreignKeyValues.Any(v => (v == null && value == null) || (v != null && v.Equals(value))) )
                {
                    results.Add(entity);
                }
            }

            return results.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntity TryGetById<TId>(TId id)
        {
            IEntityId entityId = new EntityId<TEntity, TId>(id);
            return TryGetById(entityId);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntity GetById(IEntityId id)
        {
            var entity = TryGetById(id);

            if ( entity != null )
            {
                return entity;
            }

            throw new EntityNotFoundException(typeof(TEntity), id.Value);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntity GetById<TId>(TId id)
        {
            var entity = TryGetById(id);

            if ( entity != null )
            {
                return entity;
            }

            throw new EntityNotFoundException(typeof(TEntity), id);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEntityId MakeEntityId(object value)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntity TryGetById(IEntityId id)
        {
            return _storedEntities.FirstOrDefault(e => e.As<IEntityObject>().GetId().Equals(id));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IEntityRepository.Save(object entity)
        {
            ((IEntityRepository<TEntity>)this).Save((TEntity)entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IEntityRepository.Insert(object entity)
        {
            ((IEntityRepository<TEntity>)this).Insert((TEntity)entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IEntityRepository.Update(object entity)
        {
            ((IEntityRepository<TEntity>)this).Update((TEntity)entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IEntityRepository.Delete(object entity)
        {
            ((IEntityRepository<TEntity>)this).Delete((TEntity)entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        EntityChangeMessage IEntityRepository.CreateChangeMessage(IEnumerable<IDomainObject> entities, EntityState state)
        {
            return EntityChangeMessage.Create<TEntity>(_components.Resolve<IFramework>(), entities, state);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IEntityRepository.ContractType
        {
            get
            {
                return typeof(TEntity);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IEntityRepository.ImplementationType
        {
            get
            {
                return typeof(TEntity);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Type PersistableObjectFactoryType
        {
            get
            {
                return typeof(TestEntityObjectFactory);
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

        TEntity IEntityRepository<TEntity>.New()
        {
            return _domainObjectFactory.CreateDomainObjectInstance(_objectFactory.NewEntity<TEntity>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TConcreteEntity IEntityRepository<TEntity>.New<TConcreteEntity>() 
        {
            return _domainObjectFactory.CreateDomainObjectInstance(_objectFactory.NewEntity<TConcreteEntity>());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TEntity IEntityRepository<TEntity>.New(Type concreteContract)
        {
            return _domainObjectFactory.CreateDomainObjectInstance((TEntity)_objectFactory.NewEntity(concreteContract));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IQueryable<TEntity> IEntityRepository<TEntity>.Include(params System.Linq.Expressions.Expression<Func<TEntity, object>>[] properties)
        {
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IEntityRepository<TEntity>.Save(TEntity entity)
        {
            _storedEntities.Add(entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IEntityRepository<TEntity>.Insert(TEntity entity)
        {
            _storedEntities.Add(entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IEntityRepository<TEntity>.Update(TEntity entity)
        {
            _storedEntities.Add(entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IEntityRepository<TEntity>.Delete(TEntity entity)
        {
            _storedEntities.Remove(entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TEntity IEntityRepository<TEntity>.CheckOutOne<TState>(System.Linq.Expressions.Expression<Func<TEntity, bool>> where, System.Linq.Expressions.Expression<Func<TEntity, TState>> stateProperty, TState newStateValue)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            return _storedEntities.GetEnumerator();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _storedEntities.GetEnumerator();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IQueryable.ElementType
        {
            get { return _storedEntities.AsQueryable().ElementType; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        System.Linq.Expressions.Expression IQueryable.Expression
        {
            get { return _storedEntities.AsQueryable().Expression; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IQueryProvider IQueryable.Provider
        {
            get { return _storedEntities.AsQueryable().Provider; }
        }
    }
}
