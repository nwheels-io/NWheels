using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Concurrency
{
    public interface IResourceAccessHolderInfo
    {
        ResourceAccessType AccessType { get; }
        DateTime AcquiredAtUtc { get; }
        DateTime ThreadStartedAtUtc { get; }
        ThreadTaskType TaskType { get; }
        string RootActivityText { get; }
        Guid LogId { get; }
        int PromisedDurationMs { get; }
        int ActualDurationMs { get; }
    }
}
