using System;
using System.Linq;

namespace NWheels.Api.Ddd
{
    public interface IEntityRepository<T>
        where T : class
    {
        IQueryable<T> AsQueryable();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void Insert(T entity);
    }
}

