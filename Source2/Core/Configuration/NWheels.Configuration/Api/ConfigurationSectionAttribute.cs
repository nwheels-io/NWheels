using System;

namespace NWheels.Configuration.Api
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited  =true)]
    public class ConfigurationSectionAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
