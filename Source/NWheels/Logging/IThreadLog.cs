using System;

namespace NWheels.Logging
{
    internal interface IThreadLog
    {
        void NotifyActivityClosed(ActivityLogNode activity);
        ThreadTaskType TaskType { get; }
        Guid CorrelationId { get; }
        DateTime ThreadStartedAtUtc { get; }
        long ElapsedThreadMilliseconds { get; }
        ActivityLogNode RootActivity { get; }
        ActivityLogNode CurrentActivity { get; }
    }
}
