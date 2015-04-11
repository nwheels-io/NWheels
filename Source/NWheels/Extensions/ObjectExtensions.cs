using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Extensions
{
    public static class ObjectExtensions
    {
        public static string ToStringOrDefault<T>(this T value, string defaultValue = null) where T : class
        {
            return (value != null ? value.ToString() : defaultValue);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string ToStringOrDefault<T>(this T? value, string defaultValue = null) where T : struct
        {
            return (value.HasValue ? value.Value.ToString() : defaultValue);
        }
    }
}
