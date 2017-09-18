using System;
using System.Reflection;

namespace NWheels.Kernel.Api.Injection
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FeatureLoaderAttribute : Attribute
    {
        public string Name { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetFeatureNameOrThrow(Type type)
        {
            var attribute = type.GetCustomAttribute<FeatureLoaderAttribute>();
            var name = attribute?.Name;

            if (string.IsNullOrEmpty(name))
            {
                throw new Exception(); //TODO: throw correct exception
            }

            return name;
        }
    }
}
