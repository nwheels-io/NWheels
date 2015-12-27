using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Extensions
{
    public static class MemberInfoExtensions
    {
        public static bool HasAttribute<T>(this MemberInfo member) where T : Attribute
        {
            return (member.GetCustomAttribute<T>() != null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Expression<Func<TEntity, TProperty>> PropertyExpression<TEntity, TProperty>(this PropertyInfo property)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            return Expression.Lambda<Func<TEntity, TProperty>>(Expression.Convert(Expression.Property(parameter, property), typeof(TProperty)), new[] { parameter });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static Expression<Func<TEntity, object>> PropertyAsObjectExpression<TEntity, TProperty>(this PropertyInfo property)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            return Expression.Lambda<Func<TEntity, object>>(Expression.Convert(Expression.Property(parameter, property), typeof(object)), new[] { parameter });
        }
    }
}
