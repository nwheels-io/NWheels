using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Conventions.Core;
using NWheels.Entities;

namespace NWheels.Testing.Entity
{
    public class TestEntityRepository<TEntity> : IEntityRepository<TEntity>
    {
        private readonly EntityObjectFactory _objectFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TestEntityRepository(EntityObjectFactory objectFactory)
        {
            _objectFactory = objectFactory;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TEntity IEntityRepository<TEntity>.New()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IQueryable<TEntity> IEntityRepository<TEntity>.Include(params System.Linq.Expressions.Expression<Func<TEntity, object>>[] properties)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IEntityRepository<TEntity>.Insert(TEntity entity)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IEntityRepository<TEntity>.Update(TEntity entity)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IEntityRepository<TEntity>.Delete(TEntity entity)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        TEntity IEntityRepository<TEntity>.CheckOutOne<TState>(System.Linq.Expressions.Expression<Func<TEntity, bool>> where, System.Linq.Expressions.Expression<Func<TEntity, TState>> stateProperty, TState newStateValue)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Type IQueryable.ElementType
        {
            get { throw new NotImplementedException(); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        System.Linq.Expressions.Expression IQueryable.Expression
        {
            get { throw new NotImplementedException(); }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IQueryProvider IQueryable.Provider
        {
            get { throw new NotImplementedException(); }
        }
    }
}
