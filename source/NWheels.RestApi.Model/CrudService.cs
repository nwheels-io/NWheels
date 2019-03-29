using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NWheels.RestApi.Model
{
    public class CrudService<T>
    {
        void Create(T item, bool upsert = false)
        {
        }

        void Create(IEnumerable<T> items, bool upsert = false)
        {
        }

        Task<T> Retrieve<TId>(TId id) => null;
        
        Task<IQueryable<T>> Retrieve<TId>(IEnumerable<TId> ids) => null;
        
        Task<IQueryable<T>> Retrieve(Expression<Func<T, bool>> query) => null;

        void Update(T item)
        {
        }

        void Update(Expression<Func<T, bool>> query, Action<UpdateBuilder<T>> update)
        {
        }

        void Delete(T item)
        {
        }

        void Delete<TId>(TId id)
        {
        }

        void Delete<TId>(IEnumerable<TId> ids)
        {
        }

        void Delete(Expression<Func<T, bool>> query)
        {
        }
    }

    public class UpdateBuilder<T>
    {
        UpdateBuilder<T> Set<V>(
            Expression<Func<T, V>> target,
            Expression<Func<T, V>> value
        ) => null;
    }
}
