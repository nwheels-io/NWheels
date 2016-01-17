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
        CollectCount = 0x08,
        CollectStats = 0x10,
        RetainDetails = 0x20,
        RetainThreadLog = 0x40,
        PublishStats = 0x100,
    }
}
