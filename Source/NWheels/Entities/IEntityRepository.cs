using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Entities
{
    public interface IEntityRepository<TEntity> : IQueryable<TEntity>
    {
        TEntity New();
        IQueryable<TEntity> Include(Expression<Func<TEntity, object>>[] properties);
        void Insert(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
    }
}
