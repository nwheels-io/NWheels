using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NWheels.Logging;

namespace NWheels.Stacks.MongoDb.SystemLogs.Persistence
{
    public class ThreadLogRecord : LogRecordBase
    {
        public ThreadLogRecord(ThreadLogSnapshot snapshot)
        {
            this.LogId = snapshot.LogId.ToString("N");
            this.Timestamp = snapshot.StartedAtUtc;
            this.TaskType = snapshot.TaskType;
            this.CorrelationId = snapshot.CorrelationId.ToString("N");
            this.Level = snapshot.RootActivity.Level;

            this.MachineName = MongoDbThreadLogPersistor.MachineName;
            this.NodeName = snapshot.NodeName;
            this.NodeInstance = snapshot.NodeInstance;

            this.Snapshot = snapshot;
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
            this.NodeName = currentNode.NodeName;
            this.NodeInstance = currentNode.InstanceId;

            this.Snapshot = threadLog.TakeSnapshot();
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

        //TODO: replace this with a more optimal representation
        public ThreadLogSnapshot Snapshot { get; set; }
    }
}
