using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Logging;

namespace NWheels.Domains.DevOps.SystemLogs.Entities
{
    [EntityContract]
    public interface ILogMessageSummaryEntity : IBaseLogDimensionsEntity
    {
        [PropertyContract.Calculated]
        LogLevel Level { get; }
        
        [PropertyContract.Calculated]
        string Logger { get; }
        
        [PropertyContract.Calculated]
        string MessageId { get; }
        
        [PropertyContract.Calculated]
        string ExceptionType { get; }

        [PropertyContract.Calculated]
        int Count { get; }
    }
}
