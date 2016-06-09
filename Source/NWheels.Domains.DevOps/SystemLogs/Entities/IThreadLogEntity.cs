using System;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Logging;

namespace NWheels.Domains.DevOps.SystemLogs.Entities
{
    [EntityContract]
    public interface IThreadLogEntity : IBaseLogDimensionsEntity
    {
        [PropertyContract.Calculated]
        DateTime Timestamp { get; }

        [PropertyContract.Calculated]
        ThreadTaskType TaskType { get; }

        [PropertyContract.Calculated]
        string RootMessageId { get; }

        [PropertyContract.Calculated]
        string RootActivity { get; }

        [PropertyContract.Calculated]
        long DurationMs { get; }

        [PropertyContract.Calculated]
        LogLevel Level { get; }

        [PropertyContract.Calculated]
        string ExceptionType { get; }

        [PropertyContract.Calculated]
        string CorrelationId { get; }

        [PropertyContract.Calculated]
        ThreadLogSnapshot Snapshot { get; }
    }
}
