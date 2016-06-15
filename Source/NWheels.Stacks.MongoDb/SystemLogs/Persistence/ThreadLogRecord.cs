using System;
using System.Collections.Generic;
using System.Diagnostics;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NWheels.Logging;

namespace NWheels.Stacks.MongoDb.SystemLogs.Persistence
{
    [BsonIgnoreExtraElements]
    public class ThreadLogRecord : LogRecordBase
    {
        public ThreadLogRecord(ThreadLogSnapshot snapshot)
        {
            this.LogId = snapshot.LogId.ToString("N");
            this.Timestamp = snapshot.StartedAtUtc;
            this.TaskType = snapshot.TaskType;
            this.CorrelationId = snapshot.CorrelationId.ToString("N");
            this.Level = snapshot.RootActivity.Level;
            this.EnvironmentName = snapshot.EnvironmentName;
            this.EnvironmentType = snapshot.EnvironmentType;
            this.MachineName = MongoDbThreadLogPersistor.MachineName;
            this.ProcessId = MongoDbThreadLogPersistor.ProcessId;
            this.NodeName = snapshot.NodeName;
            this.NodeInstance = snapshot.NodeInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ThreadLogRecord(IReadOnlyThreadLog threadLog)
        {
            this.LogId = threadLog.LogId.ToString("N");
            this.Timestamp = threadLog.ThreadStartedAtUtc;
            this.TaskType = threadLog.TaskType;
            this.CorrelationId = threadLog.CorrelationId.ToString("N");
            this.Level = threadLog.RootActivity.Level;

            var currentNode = threadLog.Node;

            this.MachineName = MongoDbThreadLogPersistor.MachineName;
            this.ProcessId = MongoDbThreadLogPersistor.ProcessId;
            this.NodeName = currentNode.NodeName;
            this.NodeInstance = currentNode.InstanceId;
            this.NodeInstanceReplica = null; // TODO: set to current node replica id
            this.EnvironmentName = currentNode.EnvironmentName;
            this.EnvironmentType = currentNode.EnvironmentType;
            this.RootActivityMessageId = threadLog.RootActivity.MessageId;
            this.RootActivityText = threadLog.RootActivity.SingleLineText;
            this.ContentTypes = threadLog.RootActivity.ContentTypes;
            this.DurationMicroseconds = threadLog.ElapsedThreadMicroseconds;

            this.VolatileSnapshot = threadLog.TakeSnapshot();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [BsonId]
        public string LogId { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Timestamp { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [BsonRepresentation(BsonType.String)]
        public ThreadTaskType TaskType { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string CorrelationId { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [BsonRepresentation(BsonType.String)]
        public LogLevel Level { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string EnvironmentName { get; set; }
        public string EnvironmentType { get; set; }
        public int ProcessId { get; set; }
        public string RootActivityMessageId { get; set; }
        public string RootActivityText { get; set; }
        public LogContentTypes ContentTypes { get; set; }
        public long DurationMicroseconds { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionDetails { get; set; }
        public string[] NameValuePairs { get; set; }
        public long CpuCycles { get; set; }
        public long CpuTime { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [BsonIgnore]
        public ThreadLogSnapshot VolatileSnapshot { get; set; }
    }
}
