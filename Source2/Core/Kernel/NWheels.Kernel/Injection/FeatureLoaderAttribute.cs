using System;

namespace NWheels.Injection
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FeatureLoaderAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
