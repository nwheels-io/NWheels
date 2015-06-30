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
        long ElapsedThreadMilliseconds { get; }
        ActivityLogNode RootActivity { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    internal interface IThreadLog : IReadOnlyThreadLog
    {
        void NotifyActivityClosed(ActivityLogNode activity);
        ActivityLogNode CurrentActivity { get; }
    }
}
