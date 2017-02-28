using System;

namespace NWheels.Configuration
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FeatureLoaderAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
