using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Concurrency
{
    public interface ITimeoutHandle : IDisposable
    {
        void ResetDueTime(TimeSpan newInitialDueTime);
        string TimerName { get; }
        string TimerInstanceId { get; }
        TimeSpan InitialDueTime { get; }
        DateTime DueTimeUtc { get; }

        void CancelTimer();
    }
}
