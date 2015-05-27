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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string AssemblyQualifiedNameNonVersioned(this Type type)
        {
            return string.Format("{0}, {1}", type.FullName, type.Assembly.GetName().Name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsConstructedGenericTypeOf(this Type type, Type genericTypeDefinition)
        {
            return (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == genericTypeDefinition);
        }
    }
}
