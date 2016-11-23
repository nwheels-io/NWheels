using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Processing.Jobs
{
    public enum ApplicationJobStatus
    {
        Pending,
        InProgress,
        Completed,
        Failed,
        TimedOut
    }
}
