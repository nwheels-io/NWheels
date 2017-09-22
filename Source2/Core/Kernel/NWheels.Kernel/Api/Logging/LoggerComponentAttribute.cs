using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Kernel.Api.Logging
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class LoggerComponentAttribute : Attribute
    {
    }
}
