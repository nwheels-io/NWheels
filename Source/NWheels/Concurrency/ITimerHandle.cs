using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Concurrency
{
    public interface ITimerHandle : IDisposable
    {
        void Expand(TimeSpan delta);
        void Expand(int deltaMilliseconds);
        string TimerName { get; }
        string TimerInstanceId { get; }
        TimeSpan InitialDueTime { get; }
        TimeSpan? RecurringPeriod { get; }
        DateTime DueTimeUtc { get; }
        int TickCount { get; }
    }
}
