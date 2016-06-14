using System;
using NWheels.Hosting;

namespace NWheels.Logging
{
    public interface IReadOnlyThreadLog
    {
        ThreadLogSnapshot TakeSnapshot();
        INodeConfiguration Node { get; }
        ThreadTaskType TaskType { get; }
        Guid LogId { get; }
        Guid CorrelationId { get; }
        DateTime ThreadStartedAtUtc { get; }
        ulong ThreadStartCpuCycles { get; }
        long ElapsedThreadMilliseconds { get; }
        long ElapsedThreadMicroseconds { get; }
        ulong UsedThreadCpuCycles { get; }
        ActivityLogNode RootActivity { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IThreadLog : IReadOnlyThreadLog
    {
        void NotifyActivityClosed(ActivityLogNode activity);
        ActivityLogNode CurrentActivity { get; }
    }
}
