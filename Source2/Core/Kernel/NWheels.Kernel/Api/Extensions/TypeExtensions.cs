using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NWheels.Kernel.Api.Extensions
{
    public static class TypeExtensions
    {
        public static string AssemblyShortName(this Type type)
        {
            return type.GetTypeInfo().Assembly.GetName().Name;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string FriendlyName(this Type type)
        {
            if (type.GetTypeInfo().IsGenericType)
            {
                var nameBuilder = new StringBuilder();
                AppendFriendlyName(type, nameBuilder);

                return nameBuilder.ToString();
            }
            else
            {
                return type.Name;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static Type GetClosedDeclaringType(this Type nestedType, out Type[] nestedTypeArguments)
        {
            var nestedTypeInfo = nestedType.GetTypeInfo();
            if (nestedType.DeclaringType != null && !nestedTypeInfo.IsGenericTypeDefinition)
            {
                var declaringTypeInfo = nestedType.DeclaringType.GetTypeInfo();
                if (declaringTypeInfo.IsGenericType && declaringTypeInfo.IsGenericTypeDefinition)
                {
                    var declaringTypeArgumentCount = declaringTypeInfo.GetGenericArguments().Length;
                    var allTypeArguments = nestedTypeInfo.GetGenericArguments();

                    var declaringTypeArguments = allTypeArguments.Take(declaringTypeArgumentCount).ToArray();
                    nestedTypeArguments = allTypeArguments.Skip(declaringTypeArgumentCount).ToArray();

                    return nestedType.DeclaringType.MakeGenericType(declaringTypeArguments);
                }
            }

            nestedTypeArguments = null;
            return nestedType.DeclaringType;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void AppendFriendlyName(Type type, StringBuilder output)
        {
            Type[] nestedTypeArguments = null;

            if (type.IsNested && type.DeclaringType != null)
            {
                AppendFriendlyName(type.GetClosedDeclaringType(out nestedTypeArguments), output);
                output.Append(".");
            }

            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType && (nestedTypeArguments == null || nestedTypeArguments.Length > 0))
            {
                var backquoteIndex = type.Name.IndexOf('`');

                output.Append(backquoteIndex > 0 ? type.Name.Substring(0, backquoteIndex) : type.Name);
                output.Append('<');

                var typeArguments = nestedTypeArguments ?? typeInfo.GetGenericArguments();

                for (int i = 0; i < typeArguments.Length; i++)
                {
                    AppendFriendlyName(typeArguments[i], output);

                    if (i < typeArguments.Length - 1)
                    {
                        output.Append(',');
                    }
                }

                output.Append('>');
            }
            else
            {
                output.Append(type.Name);
            }
        }
    }
}