using NWheels.DataObjects;
using NWheels.Entities;

namespace NWheels.Domains.DevOps.SystemLogs.Entities
{
    [EntityContract]
    public interface ILogLevelSummaryEntity : IBaseLogDimensionsEntity
    {
        [PropertyContract.Calculated]
        int DebugCount { get; }

        [PropertyContract.Calculated]
        int VerboseCount { get; }

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
