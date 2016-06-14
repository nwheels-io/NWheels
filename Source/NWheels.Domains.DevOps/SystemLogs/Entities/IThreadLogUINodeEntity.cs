using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
        string MessageId { get; }

        [PropertyContract.Calculated]
        DateTime Timestamp { get; }

        [PropertyContract.Calculated]
        LogLevel Level { get; }

        [PropertyContract.Calculated]
        string Icon { get; }

        [PropertyContract.Calculated]
        string Text { get; }

        [PropertyContract.Calculated]
        string TimeText { get; }

        [PropertyContract.Calculated]
        long DurationMicroseconds { get; }

        [PropertyContract.Calculated]
        long DbDurationMicroseconds { get; }

        [PropertyContract.Calculated]
        long DbCount { get; }

        [PropertyContract.Calculated]
        long CpuTimeMicroseconds { get; }

        [PropertyContract.Calculated]
        long CpuCycles { get; }

        [PropertyContract.Calculated]
        string[] KeyValues { get; }

        [PropertyContract.Calculated]
        string[] AdditionalDetails { get; }

        [PropertyContract.Calculated]
        string Exception { get; }

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
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class ThreadLogNodeDetails
    {
        [JsonProperty(Order = 1)]
        public string Time { get; set; }

        [JsonProperty(Order = 2)]
        public string Message { get; set; }

        [JsonProperty(Order = 3)]
        public Dictionary<string, string> Values { get; set; }

        [JsonProperty(Order = 4)]
        public SessionDetails Session { get; set; }

        [JsonProperty(Order = 5)]
        public Dictionary<string, string> AllValues { get; set; }

        [JsonProperty(Order = 6, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Exception { get; set; }
        
        [JsonProperty(Order = 7, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Dictionary<string, decimal> Counters { get; set; }

        [JsonProperty(Order = 8)]
        public string[] Stack { get; set; }

        [JsonProperty(Order = 9)]
        public MessageDetails[] StackDetailed { get; set; }
        
        [JsonProperty(Order = 10)]
        public ThreadDetails Thread { get; set; }
        
        [JsonProperty(Order = 11)]
        public ProcessDetails Process { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class MessageDetails
        {
            [JsonProperty(Order = 1)]
            public string Time { get; set; }

            [JsonProperty(Order = 2)]
            public string Message { get; set; }
            
            [JsonProperty(Order = 3)]
            public Dictionary<string, string> Values { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ProcessDetails
        {
            [JsonProperty(Order = 1)]
            public string Application { get; set; }

            [JsonProperty(Order = 2)]
            public string Environment { get; set; }

            [JsonProperty(Order = 3)]
            public string Node { get; set; }

            [JsonProperty(Order = 4, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string Instance { get; set; }

            [JsonProperty(Order = 5, DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string Replica { get; set; }

            [JsonProperty(Order = 6)]
            public string Machine { get; set; }

            [JsonProperty(Order = 7)]
            public int ProcessId { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ThreadDetails
        {
            [JsonProperty(Order = 1), JsonConverter(typeof(IsoDateTimeConverter))]
            public DateTime StartedAtUtc { get; set; }

            [JsonProperty(Order = 2)]
            public ThreadTaskType TaskType { get; set; }

            [JsonProperty(Order = 3)]
            public string LogId { get; set; }

            [JsonProperty(Order = 4)]
            public string CorrelationId { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class SessionDetails
        {
            [JsonProperty(Order = 1), JsonConverter(typeof(IsoDateTimeConverter))]
            public string User { get; set; }

            [JsonProperty(Order = 2), JsonConverter(typeof(IsoDateTimeConverter))]
            public string[] Claims { get; set; }

            [JsonProperty(Order = 3)]
            public string SessionId { get; set; }

            [JsonProperty(Order = 4)]
            public string StartedAtUtc { get; set; }

            [JsonProperty(Order = 5)]
            public TimeSpan SessionElapsedTime { get; set; }
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
