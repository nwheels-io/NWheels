using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.DataObjects
{
    public interface IDataProcedureBuilder
    {
        //void Replace<T>(Expression<Func<T>> destination, Expression<Func<T>> newValue);
        
        //void AddOne<T>(Expression<Func<ICollection<T>>> destination, Expression<Func<T>> newItem);
        //void ReplaceOne<T>(Expression<Func<T, object>> key, Expression<Func<ICollection<T>>> destination, Expression<Func<T>> newItem);
        //void AddOrReplaceOne<T>(Expression<Func<T, object>> key, Expression<Func<ICollection<T>>> destination, Expression<Func<T>> newItem);
        
        //void InsertMany<T>(Expression<Func<ICollection<T>>> destination, Expression<Func<IEnumerable<T>>> items);
        //void ReplaceMany<T>(Expression<Func<T, object>> key, Expression<Func<ICollection<T>>> destination, Expression<Func<IEnumerable<T>>> items);
        //void InsertOrReplaceMany<T>(Expression<Func<T, object>> key, Expression<Func<ICollection<T>>> destination, Expression<Func<IEnumerable<T>>> items);
        
        //void UpdateMany<T>(Expression<Func<ICollection<T>>> destination, Expression<Func<T, bool>> Expression<Func<IEnumerable<T>>> items);
    }
}
