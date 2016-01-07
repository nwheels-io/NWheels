using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NWheels.Extensions;

namespace NWheels.Utilities
{
    public static class ExpressionUtility
    {
        public static MethodInfo GetMethodInfo<TLambda>(TLambda lambda) where TLambda : LambdaExpression
        {
            return lambda.GetMethodInfo();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static PropertyInfo GetPropertyInfo<TLambda>(TLambda lambda) where TLambda : LambdaExpression
        {
            return lambda.GetPropertyInfo();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetPropertyName<TObject>(Expression<Func<TObject, object>> property) 
        {
            return property.GetPropertyInfo().Name;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static object Evaluate(Expression expression, params ParameterExpression[] parameters)
        {
            if ( expression.NodeType == ExpressionType.Constant )
            {
                return ((ConstantExpression)expression).Value;
            }

            var lambda = Expression.Lambda(expression, parameters);
            var compiled = lambda.Compile();
            var value = compiled.DynamicInvoke();

            return value;
        }
    }
}
