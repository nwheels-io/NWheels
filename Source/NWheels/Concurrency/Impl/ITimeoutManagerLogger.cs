using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Concurrency.Impl
{
    public interface ITimeoutManagerLogger : IApplicationEventLogger
    {
        [LogThread(ThreadTaskType.ScheduledJob)]
        ILogActivity TimeoutCallback(string timerName, string timerInstanceId);
    }
}
