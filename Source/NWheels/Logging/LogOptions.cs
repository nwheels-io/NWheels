using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Logging
{
    [Flags]
    public enum LogOptions
    {
        None = 0,
        ThreadLog = 0x01,
        PlainLog = 0x02,
        AuditLog = 0x04,
        AuditLogIfFailure = 0x08,
        CollectCount = 0x10,
        CollectStats = 0x20,
        RetainDetails = 0x40,
        RetainThreadLog = 0x80,
        PublishStats = 0x100,
        ThrowOnFailure = 0x200,
    }
}
