using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Kernel.Api.Logging
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class LogWarningAttribute : Attribute
    {
    }
}
