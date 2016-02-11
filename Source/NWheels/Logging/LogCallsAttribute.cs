using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Logging
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class LogCallsAttribute : Attribute
    {
        public string MessageId { get; set; }
        public string AuditAction { get; set; }
    }
}
