using System;
using System.Collections.Generic;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Logging;

namespace NWheels.Domains.DevOps.Logging.Entities
{
    [EntityContract]
    public interface ILogMessageDailySummaryEntity : IBaseLogDimensionsEntity
    {
        int GetTotalCount(DateTime from, DateTime until);
        Dictionary<DateTime, int> GetCountPerHour(DateTime from, DateTime until);
        Dictionary<DateTime, int> GetCountPerMinute(DateTime from, DateTime until);

        [PropertyContract.Calculated]
        DateTime Date { get; }
        
        [PropertyContract.Calculated]
        LogLevel Level { get; }
        
        [PropertyContract.Calculated]
        string Logger { get; }
        
        [PropertyContract.Calculated]
        string MessageId { get; }
        
        [PropertyContract.Calculated]
        string ExceptionType { get; }
    }
}
