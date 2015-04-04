using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Extensions
{
    public static class ValueTypeExtensions
    {
        public static bool IsIn<T>(this T value, params T[] valueList)
            where T : struct
        {
            if ( valueList != null )
            {
                for ( int i = 0 ; i < valueList.Length ; i++ )
                {
                    if ( value.Equals(valueList[i]) )
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
