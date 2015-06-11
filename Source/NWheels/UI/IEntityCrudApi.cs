using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.UI
{
    public interface IEntityCrudApi<TEntity> : IDomainApi
        where TEntity : class
    {
        TEntity RetrieveById(IEntityId<TEntity> id);
        TEntity[] Retrieve(Func<IQueryable<TEntity>, IQueryable<TEntity>> query);
        void Save(TEntity entity);
        void Delete(TEntity entity);
    }
}
