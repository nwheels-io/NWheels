using MongoDB.Bson.Serialization.Attributes;

namespace NWheels.Stacks.MongoDb.SystemLogs.Persistence
{
    public class LogRecordBase
    {
        public string MachineName { get; set; }
        public string ApplicationName { get; set; }
        public string NodeName { get; set; }
        public string NodeInstance { get; set; }
        public string NodeInstanceReplica { get; set; }

        [BsonIgnore]
        public string EnvironmentName { get; set; }
    }
}
