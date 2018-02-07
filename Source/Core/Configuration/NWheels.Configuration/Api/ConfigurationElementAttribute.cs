using System;

namespace NWheels.Configuration.Api
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class ConfigurationElementAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
