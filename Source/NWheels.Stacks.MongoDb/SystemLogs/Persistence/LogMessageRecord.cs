using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NWheels.Logging;

namespace NWheels.Stacks.MongoDb.SystemLogs.Persistence
{
    public class LogMessageRecord
    {
        public LogMessageRecord(LogNode node)
        {
            var timestamp = node.GetUtcTimestamp();
            var environment = node.ThreadLog.Node;

            this.Id = ObjectId.GenerateNewId(timestamp);
            this.Timestamp = timestamp;
            this.MachineName = MongoDbThreadLogPersistor.MachineName;
            this.NodeName = environment.NodeName;
            this.NodeInstance = environment.InstanceId;

            var periodPosition = node.MessageId.IndexOf('.');
            if ( periodPosition > 0 )
            {
                this.Logger = node.MessageId.Substring(0, periodPosition);
            }

            this.Level = node.Level;
            this.MessageId = node.MessageId;
            this.ThreadLogId = node.ThreadLog.LogId.ToString("N");
            this.CorrelationId = node.ThreadLog.CorrelationId.ToString("N");
            this.ExceptionType = node.ExceptionTypeName;

            if ( node.Exception != null )
            {
                this.ExceptionDetails = node.Exception.ToString();
            }

            this.KeyValues = node.NameValuePairs.Where(nvp => !nvp.IsBaseValue()).Select(nvp => nvp.FormatLogString()).ToArray();
            //TODO: fill AdditionalDetails

            var activityNode = (node as ActivityLogNode);

            if ( activityNode != null )
            {
                this.DurationMs = activityNode.MillisecondsDuration;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [BsonId]
        public ObjectId Id { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Timestamp { get; set; }

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

        public string Logger { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string MessageId { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public long? DurationMs { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ExceptionType { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ExceptionDetails { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string ThreadLogId { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string CorrelationId { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string[] KeyValues { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string[] AdditionalDetails { get; set; }
    }
}
