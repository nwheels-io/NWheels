using System;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Reflection;

namespace NWheels.Stacks.EntityFramework
{
    public static class QueryExpressionSpecializer
    {
        public static Expression Specialize(Expression general)
        {
            var visitor = new SpecializingVisitor();
            return visitor.Visit(general);
        }

		//-------------------------------------------------------------------------------------------------------------------------------------------------

		private static MethodInfo GetMethodInfo<TLambda>(TLambda lambda) where TLambda : LambdaExpression
		{
			return ((MethodCallExpression)lambda.Body).Method;
		}

        //-------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static PropertyInfo GetPropertyInfo<TLambda>(TLambda lambda) where TLambda : LambdaExpression
        {
            return (PropertyInfo)((MemberExpression)lambda.Body).Member;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly MethodInfo _s_dbFunctions_TruncateTime_NullableOfDateTime = 
            GetMethodInfo<Expression<Func<DateTime?, DateTime?>>>(d => DbFunctions.TruncateTime(d));

        private static readonly PropertyInfo _s_nullableOfDateTime_Value =
            GetPropertyInfo<Expression<Func<DateTime?, DateTime>>>(d => d.Value);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class SpecializingVisitor : ExpressionVisitor
        {
            protected override Expression VisitMember(MemberExpression node)
            {
                if ( node.Expression.Type == typeof(DateTime?) && node.Member.Name == "Date" )
                {
                    return Expression.Call(_s_dbFunctions_TruncateTime_NullableOfDateTime, node.Expression);
                }
                
                if ( node.Expression.Type == typeof(DateTime) && node.Member.Name == "Date" )
                {
                    return Expression.Property(
                        Expression.Call(
                            _s_dbFunctions_TruncateTime_NullableOfDateTime, 
                            Expression.Convert(
                                node.Expression, 
                                typeof(DateTime?)
                            )
                        ),
                        _s_nullableOfDateTime_Value
                    );
                }

                return base.VisitMember(node);
            }
        }
    }
}
