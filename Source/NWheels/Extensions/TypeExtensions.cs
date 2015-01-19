using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsExceptionType(this Type type)
        {
            return typeof(Exception).IsAssignableFrom(type);
        }
    }
}
