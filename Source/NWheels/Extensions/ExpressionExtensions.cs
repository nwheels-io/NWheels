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
        private static readonly FieldInfo _s_parameterExpressionNameField = typeof(ParameterExpression).GetField(
            "_name",
            BindingFlags.Instance | BindingFlags.NonPublic);

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static PropertyInfo GetPropertyInfo(this LambdaExpression lambda)
        {
            var unaryExpression = lambda.Body as UnaryExpression;
            var memberExpression = (MemberExpression)(unaryExpression != null ? unaryExpression.Operand : lambda.Body);

            return (PropertyInfo)memberExpression.Member;
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

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static string ToNormalizedNavigationString(this LambdaExpression expression, params string[] formalParameterNames)
        {
            for ( int i = 0 ; i < formalParameterNames.Length ; i++ )
            {
                EnsureParameterName(expression.Parameters[i], formalParameterNames[i]);
            }

            var expressionString = expression.ToString();
            var arrowToken = "=>";
            var leftSideLength = expressionString.IndexOf(arrowToken) + arrowToken.Length;

            var rightSideString = expressionString.Substring(leftSideLength).TrimStart();
            var camelCaseRightSideString = string.Join(".", rightSideString.Split('.').Select(s => s.ConvertToCamelCase()));

            return NormalizeKnownTokens(camelCaseRightSideString);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private static void EnsureParameterName(ParameterExpression parameter, string name)
        {
            if ( parameter.Name != name )
            {
                _s_parameterExpressionNameField.SetValue(parameter, name);
            }
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private static string NormalizeKnownTokens(string s)
        {
            if ( s == "True" )
            {
                return "true";
            }

            if ( s == "False" )
            {
                return "false";
            }

            return s;
        }
    }
}
