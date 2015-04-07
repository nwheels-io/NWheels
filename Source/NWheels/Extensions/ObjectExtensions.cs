using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Extensions
{
    public static class ObjectExtensions
    {
        public static string ToStringOrNull<T>(this T value) where T : class
        {
            return (value != null ? value.ToString() : null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string ToStringOrNull<T>(this T? value) where T : struct
        {
            return (value.HasValue ? value.Value.ToString() : null);
        }
    }
}
