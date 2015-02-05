using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Puzzle.EntityFramework.Impl
{
    public class ObjectContextEntityRepository<TEntityContract, TEntityImpl> : IEntityRepository<TEntityContract>
        where TEntityContract : class
        where TEntityImpl : class, TEntityContract
    {
        private readonly ApplicationEntityRepositoryBase _ownerRepo;
        private readonly ObjectSet<TEntityImpl> _objectSet;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ObjectContextEntityRepository(ApplicationEntityRepositoryBase ownerRepo)
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
                return _objectSet.AsQueryable().ElementType;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        System.Linq.Expressions.Expression IQueryable.Expression
        {
            get
            {
                return _objectSet.AsQueryable().Expression;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IQueryProvider IQueryable.Provider
        {
            get
            {
                return _objectSet.AsQueryable().Provider;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TEntityContract New()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------


        public IQueryable<TEntityContract> Include(System.Linq.Expressions.Expression<Func<TEntityContract, object>>[] properties)
        {
            throw new NotImplementedException();
        }

        public void Insert(TEntityContract entity)
        {
            throw new NotImplementedException();
        }

        public void Update(TEntityContract entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(TEntityContract entity)
        {
            throw new NotImplementedException();
        }
    }
}
