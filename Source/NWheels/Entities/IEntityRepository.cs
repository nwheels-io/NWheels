using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities
{
    public interface IEntityRepository<TEntity>
    {
        IQueryable<TEntity> AsQueryable();
        void Delete(TEntity entity);
        IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties);
        TEntity First(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties);
        IEnumerable<TEntity> GetAll(params Expression<Func<TEntity, object>>[] includeProperties);
        void Insert(TEntity entity);
        TEntity Single(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includeProperties);
        void Update(TEntity entity);
    }
}
