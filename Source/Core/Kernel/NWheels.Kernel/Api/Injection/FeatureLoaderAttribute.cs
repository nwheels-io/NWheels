using System;
using System.Reflection;
using NWheels.Kernel.Api.Exceptions;

namespace NWheels.Kernel.Api.Injection
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FeatureLoaderAttribute : Attribute
    {
        public string Name { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetFeatureNameOrThrow(Type featureLoaderType)
        {
            var attribute = featureLoaderType.GetCustomAttribute<FeatureLoaderAttribute>();
            var name = attribute?.Name;

            if (string.IsNullOrEmpty(name))
            {
                throw FeatureLoaderException.FeatureNameMissingOrInvalid(featureLoaderType);
            }

            return name;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsNamedFeature(Type featureLoaderType)
        {
            var attribute = featureLoaderType.GetCustomAttribute<FeatureLoaderAttribute>();
            var name = attribute?.Name;

            return !string.IsNullOrEmpty(name);
        }
    }
}
