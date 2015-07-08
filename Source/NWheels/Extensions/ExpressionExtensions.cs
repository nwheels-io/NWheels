using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Extensions
{
    public static class ExpressionExtensions
    {
        public static PropertyInfo GetPropertyInfo(this LambdaExpression lambda)
        {
            return (PropertyInfo)((MemberExpression)lambda.Body).Member;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static MethodInfo GetMethodInfo(this LambdaExpression lambda)
        {
            Expression[] callArguments;
            return GetMethodInfo(lambda, out callArguments);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static MethodInfo GetMethodInfo(this LambdaExpression lambda, out object[] callArgumentValues)
        {
            Expression[] callArguments;
            var method = GetMethodInfo(lambda, out callArguments);
            
            callArgumentValues = callArguments.Cast<ConstantExpression>().Select(arg => arg.Value).ToArray();
            return method;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static MethodInfo GetMethodInfo(this LambdaExpression lambda, out Expression[] callArguments)
        {
            MethodCallExpression callExpression = null;

            if ( lambda.Body is LambdaExpression )
            {
                callExpression = ((MethodCallExpression)((LambdaExpression)lambda.Body).Body);
            }
            else if ( lambda.Body is MethodCallExpression )
            {
                callExpression = (MethodCallExpression)lambda.Body;
            }
            else
            {
                throw new NotSupportedException("Specified lambda expression cannot be converted into method declaration.");
            }

            callArguments = callExpression.Arguments.ToArray();
            return callExpression.Method;
        }
    }
}
