using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Concurrency
{
    public enum PromiseStatus
    {
        InProgress = 0,
        Fulfilled = 1,
        Failed,
        Cancelled,
        TimedOut
    }
}
