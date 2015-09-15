using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hapil;

namespace NWheels.Processing.Rules.Impl
{
    public static class EvaluationHelpers
    {
        public static bool EvaluateEqual(object x, object y)
        {
            return x.Equals(y);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool EvaluateNotEqual(object x, object y)
        {
            return !x.Equals(y);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool EvaluateGreaterThan(object x, object y)
        {
            return (Comparer.Default.Compare(x, y) > 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool EvaluateGreaterThanOrEqual(object x, object y)
        {
            return (Comparer.Default.Compare(x, y) >= 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool EvaluateLessThan(object x, object y)
        {
            return (Comparer.Default.Compare(x, y) < 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool EvaluateLessThanOrEqual(object x, object y)
        {
            return (Comparer.Default.Compare(x, y) <= 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool EvaluateIn(object x, object enumerableOrInterval)
        {
            var enumerable = enumerableOrInterval as IEnumerable;
            var interval = enumerableOrInterval as IInterval;

            if ( enumerable != null )
            {
                return enumerable.Cast<object>().Contains(x);
            }
            else if ( interval != null )
            {
                return interval.Contains(x);
            }

            throw new NotSupportedException("IN as not supported for right hand side of type " + enumerableOrInterval.GetType().FriendlyName());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool EvaluateLogicalAnd(object x, object y)
        {
            return ((bool)x && (bool)y);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool EvaluateLogicalOr(object x, object y)
        {
            return ((bool)x || (bool)y);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool EvaluateLogicalNot(object x)
        {
            return !((bool)x);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static object EvaluateAdd(object x, object y)
        {
            var result = Convert.ToDecimal(x) + Convert.ToDecimal(y);
            return Convert.ChangeType(result, x.GetType());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static object EvaluateSubtract(object x, object y)
        {
            var result = Convert.ToDecimal(x) - Convert.ToDecimal(y);
            return Convert.ChangeType(result, x.GetType());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static object EvaluateMultiply(object x, object y)
        {
            var result = Convert.ToDecimal(x) * Convert.ToDecimal(y);
            return Convert.ChangeType(result, x.GetType());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static object EvaluateDivide(object x, object y)
        {
            var result = Convert.ToDecimal(x) / Convert.ToDecimal(y);
            return Convert.ChangeType(result, x.GetType());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static object EvaluateNegation(object x)
        {
            var result = -Convert.ToDecimal(x);
            return Convert.ChangeType(result, x.GetType());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static object EvaluateModulo(object x, object y)
        {
            var result = Convert.ToInt64(x) % Convert.ToInt64(y);
            return Convert.ChangeType(result, x.GetType());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static object EvaluatePercentage(object x, object y)
        {
            var result = (Convert.ToDecimal(x) / 100) * Convert.ToDecimal(y);
            return Convert.ChangeType(result, y.GetType());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static object EvaluateMin(object x, object y)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static object EvaluateMax(object x, object y)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static object EvaluateAverage(object x, object y)
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static object EvaluateCoalesce(object x, object y)
        {
            throw new NotImplementedException();
        }
    }
}
