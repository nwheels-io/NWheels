using System;
using NWheels.Api.Logging;

namespace NWheels.Api.Concurrency
{
    public interface ISyncLockHolderInfo
    {
        string ResourceName { get; }

        IThreadLog Holder { get; }

        SyncAccessTypes Access { get; }

        ILogEvent Reason { get; }

        TimeSpan Duration { get; }

        TimeSpan? DurationHint { get; }
    }
}