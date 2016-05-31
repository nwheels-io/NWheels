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
        IList<IThreadLogUINodeEntity> SubNodes { get; }

        [PropertyContract.Calculated]
        ThreadLogNodeDetails Details { get; }
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

    public class ThreadLogNodeDetails
    {
        public MessageDetails Message { get; set; }
        public MessageDetails[] Stack { get; set; }
        public ThreadDetails Thread { get; set; }
        public ApplicationUnitDetails Application { get; set; }
        public string Exception { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class MessageDetails
        {
            public string Id { get; set; }
            public LogLevel Level { get; set; }
            public LogContentTypes ContentTypes { get; set; }
            public NamedValue[] Values { get; set; }            
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ApplicationUnitDetails
        {
            public string Application { get; set; }
            public string Environment { get; set; }
            public string Node { get; set; }
            public string Instance { get; set; }
            public string Replica { get; set; }
            public string Machine { get; set; }
            public int ProcessId { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ThreadDetails
        {
            public string LogId { get; set; }
            public ThreadTaskType TaskType { get; set; }
            public string CorrelationId { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class NamedValue 
        {
            public string Name { get; set; }
            public object Value { get; set; }
        }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum ThreadLogMessageType
    {
        Log,
        Activity,
        ThreadRootActivity
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
