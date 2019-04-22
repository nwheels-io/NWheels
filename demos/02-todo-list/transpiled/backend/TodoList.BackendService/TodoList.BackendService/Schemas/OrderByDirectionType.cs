using System;
using System.Linq;
using System.Linq.Expressions;
using GraphQL.Types;

namespace TodoList.BackendService.Schemas
{
    public enum OrderByDirection
    {
        Ascending,
        Descending
    }

    public class OrderByDirectionType : EnumerationGraphType
    {
        public OrderByDirectionType()
        {
            Name = "OrderByDirection";
            AddValue("ASC", "ascending", value: (int)OrderByDirection.Ascending);
            AddValue("DESC", "descending", value: (int)OrderByDirection.Descending);
        }
    }
    
    public static class QueryableExtensions
    {
        public static IQueryable<T> OrderBy<T, TKey>(
            this IQueryable<T> source, 
            Expression<Func<T, TKey>> keySelector, 
            OrderByDirection direction)
        {
            switch (direction)
            {
                case OrderByDirection.Descending:
                    return source.OrderByDescending(keySelector);
                default:
                    return source.OrderBy(keySelector);
            }
        }
    }
}
