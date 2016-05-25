using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Logging;

namespace NWheels.Domains.DevOps.SystemLogs.Entities
{
    [EntityContract]
    public interface IThreadLogUINodeEntity
    {
        [PropertyContract.EntityId]
        string Id { get; set; }

        [PropertyContract.Calculated]
        ThreadLogNodeType NodeType { get; }

        [PropertyContract.Calculated]
        string Icon { get; }

        [PropertyContract.Calculated]
        string Text { get; }

        [PropertyContract.Calculated]
        string TimeText { get; }

        [PropertyContract.Calculated]
        long DurationMs { get; }

        [PropertyContract.Calculated]
        long DbDurationMs { get; }

        [PropertyContract.Calculated]
        long DbCount { get; }

        [PropertyContract.Calculated]
        long CpuTimeMs { get; }

        [PropertyContract.Calculated]
        long CpuCycles { get; }

        [PropertyContract.Calculated]
        string[] KeyValues { get; }

        [PropertyContract.Calculated]
        string[] AdditionalDetails { get; }

        [PropertyContract.Calculated]
        ICollection<IThreadLogUINodeEntity> SubNodes { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    [EntityContract]
    public interface IRootThreadLogUINodeEntity : IThreadLogUINodeEntity
    {
        [PropertyContract.Calculated]
        string LogId { get; }

        [PropertyContract.Calculated]
        ThreadTaskType TaskType { get; }

        [PropertyContract.Calculated]
        string CorrelationId { get; }

        [PropertyContract.Calculated]
        string Machine { get; }

        [PropertyContract.Calculated]
        string Environment { get; }

        [PropertyContract.Calculated]
        string Node { get; }

        [PropertyContract.Calculated]
        string Instance { get; }

        [PropertyContract.Calculated]
        string Replica { get; }

        [PropertyContract.Calculated]
        DateTime Timestamp { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum ThreadLogNodeType
    {
        ThreadSuccess,
        ThreadWarning,
        ThreadError,
        ThreadCritical,
        ActivitySuccess,
        ActivityWarning,
        ActivityError,
        ActivityCritical,
        LogDebug,
        LogVerbose,
        LogInfo,
        LogWarning,
        LogError,
        LogCritical
    }
}
