using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.UI
{
    public interface IEntityApi<TEntity> : IDomainApi
        where TEntity : class
    {
        TEntity New();
        TEntity RetrieveById(IEntityId<TEntity> id);
        TEntity[] RetrieveByQuery(Func<IQueryable<TEntity>, IQueryable<TEntity>> query);
        void Save(TEntity entity);
        void Delete(TEntity entity);
    }
}
