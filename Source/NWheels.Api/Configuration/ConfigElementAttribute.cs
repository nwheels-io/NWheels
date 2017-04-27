using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Configuration
{
    /// <summary>
    /// Applies to an interfaces that represent configuration elements
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class ConfigElementAttribute : Attribute
    {
    }
}
