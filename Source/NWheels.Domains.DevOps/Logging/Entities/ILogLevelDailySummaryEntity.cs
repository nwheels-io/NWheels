using System;
using System.Collections.Generic;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Logging;

namespace NWheels.Domains.DevOps.Logging.Entities
{
    [EntityContract]
    public interface ILogLevelDailySummaryEntity : IBaseLogDimensionsEntity
    {
        [PropertyContract.Calculated]
        DateTime Date { get; }
        
        [PropertyContract.Calculated]
        int InfoCount { get; }

        [PropertyContract.Calculated]
        int WarningCount { get; }

        [PropertyContract.Calculated]
        int ErrorCount { get; }

        [PropertyContract.Calculated]
        int CriticalCount { get; }
    }
}
