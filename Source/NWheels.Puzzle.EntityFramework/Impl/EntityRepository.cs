using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using NWheels.Entities;

namespace NWheels.Puzzle.EntityFramework.Impl
{
    public class EntityRepository<TEntityContract, TEntityImpl> : IEntityRepository<TEntityContract>
        where TEntityContract : class
        where TEntityImpl : class, TEntityContract
    {
        private readonly ApplicationDataRepositoryBase _ownerRepo;
        private readonly ObjectSet<TEntityImpl> _objectSet;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public EntityRepository(ApplicationDataRepositoryBase ownerRepo)
        {
            _ownerRepo = ownerRepo;
            _objectSet = ownerRepo.CreateObjectSet<TEntityImpl>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEnumerator<TEntityContract> IEnumerable<TEntityContract>.GetEnumerator()
        {
            _ownerRepo.ValidateOperationalState();
            return _objectSet.AsEnumerable().GetEnumerator();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            _ownerRepo.ValidateOperationalState();
            return _objectSet.AsEnumerable().GetEnumerator();
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
                return _objectSet.AsQueryable().Provider;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntityContract New()
        {
            _ownerRepo.ValidateOperationalState();
            return _objectSet.CreateObject<TEntityImpl>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------


        public IQueryable<TEntityContract> Include(Expression<Func<TEntityContract, object>>[] properties)
        {
            _ownerRepo.ValidateOperationalState();
            return QueryWithIncludedProperties(_objectSet, properties);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Insert(TEntityContract entity)
        {
            _ownerRepo.ValidateOperationalState();
            _objectSet.AddObject((TEntityImpl)entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Update(TEntityContract entity)
        {
            _ownerRepo.ValidateOperationalState();
            
            _objectSet.Attach((TEntityImpl)entity);
            _ownerRepo.ObjectContext.ObjectStateManager.ChangeObjectState(entity, EntityState.Modified);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Delete(TEntityContract entity)
        {
            _ownerRepo.ValidateOperationalState();
            _objectSet.DeleteObject((TEntityImpl)entity);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static IQueryable<TEntityContract> QueryWithIncludedProperties(
            IQueryable<TEntityContract> query,
            IEnumerable<Expression<Func<TEntityContract, object>>> propertiesToInclude)
        {
            return propertiesToInclude.Aggregate(query, (current, property) => current.Include(property));
        }
    }
}
