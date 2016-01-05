using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NWheels.Logging;

namespace NWheels.Stacks.MongoDb.Logging
{
    public class ThreadLogRecord
    {
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

        public string MachineName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string NodeName { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string NodeInstance { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [BsonRepresentation(BsonType.String)]
        public LogLevel Level { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //TODO: replace this with a more optimal representation
        public ThreadLogSnapshot Snapshot { get; set; }
    }
}
