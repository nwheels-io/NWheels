using System;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Logging;

namespace NWheels.Domains.DevOps.SystemLogs.Entities
{
    [EntityContract]
    public interface IThreadLogEntity : IBaseLogDimensionsEntity
    {
        [PropertyContract.EntityId]
        string LogId { get; set; }
        
        [PropertyContract.Calculated]
        DateTime Timestamp { get; set; }

        [PropertyContract.Calculated]
        ThreadTaskType TaskType { get; set; }

        [PropertyContract.Calculated]
        string CorrelationId { get; set; }

        [PropertyContract.Calculated]
        LogLevel Level { get; set; }

        [PropertyContract.Calculated]
        ThreadLogSnapshot Snapshot { get; set; }
    }
}
