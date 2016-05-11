using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Domains.DevOps.SystemLogs.Entities
{
    [EntityContract]
    public interface ILogLevelSummaryEntity : IBaseLogDimensionsEntity
    {
        [PropertyContract.Calculated]
        int DebugCount { get; set; }

        [PropertyContract.Calculated]
        int VerboseCount { get; set; }

        [PropertyContract.Calculated]
        int InfoCount { get; set; }

        [PropertyContract.Calculated]
        int WarningCount { get; set; }

        [PropertyContract.Calculated]
        int ErrorCount { get; set; }

        [PropertyContract.Calculated]
        int CriticalCount { get; set; }
    }
}
