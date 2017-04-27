using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Configuration
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ConfigElementKeyAttribute : Attribute
    {
    }
}
