using System;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Logging;

namespace NWheels.Domains.DevOps.SystemLogs.Entities
{
    [EntityContract]
    public interface ILogMessageEntity : IBaseLogDimensionsEntity
    {
        [PropertyContract.Calculated]
        DateTime Timestamp { get; }
        
        [PropertyContract.Calculated]
        LogLevel Level { get; }

        [PropertyContract.Calculated]
        string Logger { get; }
        
        [PropertyContract.Calculated]
        string MessageId { get; }

        [PropertyContract.Calculated]
        string AlertId { get; }

        [PropertyContract.Calculated]
        long? DurationMs { get; }
        
        [PropertyContract.Calculated]
        string ExceptionType { get; }
        
        [PropertyContract.Calculated]
        string ExceptionDetails { get; }
        
        [PropertyContract.Calculated]
        string ThreadLogId { get; }
        
        [PropertyContract.Calculated]
        string CorrelationId { get; }
        
        [PropertyContract.Calculated]
        string[] KeyValues { get; }

        [PropertyContract.Calculated]
        string[] AdditionalDetails { get; }
    }
}
