using System;

namespace NWheels.Kernel.Api.Injection
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FeatureLoaderAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
