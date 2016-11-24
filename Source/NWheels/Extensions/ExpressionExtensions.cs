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
            Expression callTarget;
            Expression[] callArguments;
            var method = GetMethodInfo(lambda, out callTarget, out callArguments);
            
            callArgumentValues = callArguments.Cast<ConstantExpression>().Select(arg => arg.Value).ToArray();
            return method;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static MethodInfo GetMethodInfo(this LambdaExpression lambda, out Expression[] callArguments)
        {
            Expression callTarget;
            return GetMethodInfo(lambda, out callTarget, out callArguments);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static MethodInfo GetMethodInfo(this LambdaExpression lambda, out Expression callTarget, out Expression[] callArguments)
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

            callTarget = callExpression.Object;
            callArguments = callExpression.Arguments.ToArray();
            
            return callExpression.Method;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static string ToNormalizedNavigationString(this LambdaExpression expression, params string[] formalParameterNames)
        {
            if (expression == null)
            {
                return null;
            }

            if ( expression.Body is UnaryExpression && expression.Body.NodeType == ExpressionType.Convert )
            {
                expression = Expression.Lambda(((UnaryExpression)expression.Body).Operand, expression.Parameters);
            }

            for ( int i = 0 ; i < formalParameterNames.Length ; i++ )
            {
                EnsureParameterName(expression.Parameters[i], formalParameterNames[i]);
            }

            var expressionString = expression.ToString();
            var arrowToken = "=>";
            var leftSideLength = expressionString.IndexOf(arrowToken) + arrowToken.Length;

            var rightSideString = expressionString.Substring(leftSideLength).TrimStart().TrimLead("Convert(").Replace(")", "");
            return rightSideString;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static string ToNormalizedNavigationString(this LambdaExpression expression, int skipSteps, params string[] formalParameterNames)
        {
            IEnumerable<string> stepStrings = ToNormalizedNavigationStringArray(expression, formalParameterNames);

            if (skipSteps > 1)
            {
                stepStrings = stepStrings.Skip(skipSteps - 1);
            }

            return string.Join(".", stepStrings);
        }


        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static LambdaExpression ToNormalizedNavigationExpression(this LambdaExpression expression, params string[] formalParameterNames)
        {
            if (expression == null)
            {
                return null;
            }

            if (expression.Body is UnaryExpression && expression.Body.NodeType == ExpressionType.Convert)
            {
                expression = Expression.Lambda(((UnaryExpression)expression.Body).Operand, expression.Parameters);
            }

            for (int i = 0; i < formalParameterNames.Length; i++)
            {
                EnsureParameterName(expression.Parameters[i], formalParameterNames[i]);
            }

            return expression;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static string[] ToNormalizedNavigationStringArray(this LambdaExpression navigationExpression, params string[] formalParameterNames)
        {
            var expressionString = navigationExpression.ToNormalizedNavigationString(formalParameterNames);
            return expressionString.Split('.').Skip(1).ToArray();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static void ParseNormalizedMethodAndParameters(
            this LambdaExpression expression, 
            out MethodInfo method, 
            out string[] parameterExpressions, 
            params string[] formalParameterNames)
        {
            if ( expression.Body is UnaryExpression && expression.Body.NodeType == ExpressionType.Convert )
            {
                expression = Expression.Lambda(((UnaryExpression)expression.Body).Operand, expression.Parameters);
            }

            for ( int i = 0; i < formalParameterNames.Length; i++ )
            {
                EnsureParameterName(expression.Parameters[i], formalParameterNames[i]);
            }

            Expression callTarget;
            Expression[] callArguments;
            method = expression.GetMethodInfo(out callTarget, out callArguments);

            parameterExpressions = callArguments.Where(arg => arg != callTarget).Select(arg => arg.ToString()).ToArray();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static string NormalizeKnownTokens(string s)
        {
            if (s == "True")
            {
                return "true";
            }

            if (s == "False")
            {
                return "false";
            }

            return s;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private static void EnsureParameterName(ParameterExpression parameter, string name)
        {
            if ( parameter.Name != name )
            {
                _s_parameterExpressionNameField.SetValue(parameter, name);
            }
        }
    }
}
