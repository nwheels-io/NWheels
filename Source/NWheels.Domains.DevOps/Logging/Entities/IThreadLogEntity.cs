using System;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Logging;

namespace NWheels.Domains.DevOps.Logging.Entities
{
    [EntityContract]
    public interface IThreadLogEntity
    {
        [PropertyContract.EntityId]
        string LogId { get; set; }
        
        DateTime Timestamp { get; set; }
        ThreadTaskType TaskType { get; set; }
        string CorrelationId { get; set; }
        string MachineName { get; set; }
        string NodeName { get; set; }
        string NodeInstance { get; set; }
        LogLevel Level { get; set; }
        ThreadLogSnapshot Snapshot { get; set; }
    }
}
