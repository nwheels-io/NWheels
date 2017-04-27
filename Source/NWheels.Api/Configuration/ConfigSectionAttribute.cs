using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Configuration
{
    /// <summary>
    /// Applies to an interfaces that represent configuration sections
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class ConfigSectionAttribute : Attribute
    {
    }
}
