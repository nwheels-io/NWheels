using System;

namespace NWheels.Configuration.Api
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ConfigurationKeyAttribute : Attribute
    {
    }
}
