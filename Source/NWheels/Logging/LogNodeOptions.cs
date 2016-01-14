using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Logging
{
    [Flags]
    public enum LogNodeOptions
    {
        None = 0,
        AppendToThreadLog = 0x01,
        AppendToPlainLog = 0x02,
        AppendToNameValuePairLog = 0x04,
        CollectSummary = 0x08,
        CollectStatistics = 0x10,
        PersistConents = 0x20,
    }
}
