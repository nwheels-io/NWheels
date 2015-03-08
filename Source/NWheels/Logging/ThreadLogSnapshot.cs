using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Logging
{
    [DataContract(Namespace = "NWheels.Logging", Name = "ThreadLog")]
    public class ThreadLogSnapshot
    {
        [DataMember]
        public string EnvironmentName { get; set; }
        [DataMember]
        public string NodeName { get; set; }
        [DataMember]
        public string NodeInstance { get; set; }
        [DataMember]
        public string MachineName { get; set; }
        [DataMember]
        public int ProcessId { get; set; }
        [DataMember]
        public Guid LogId { get; set; }
        [DataMember]
        public Guid CorrelationId { get; set; }
        [DataMember]
        public DateTime StartedAtUtc { get; set; }
        [DataMember]
        public ThreadTaskType TaskType { get; set; }
        [DataMember]
        public LogNodeSnapshot RootActivity { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = "NWheels.Logging", Name = "Node")]
        public class LogNodeSnapshot
        {
            [DataMember]
            public string MessageId { get; set; }
            [DataMember]
            public bool IsActivity { get; set; }
            [DataMember]
            public LogContentTypes ContentTypes { get; set; }
            [DataMember]
            public LogLevel Level { get; set; }
            [DataMember]
            public long MillisecondsTimestamp { get; set; }
            [DataMember]
            public long Duration { get; set; }
            [DataMember]
            public string ExceptionTypeName { get; set; }
            [DataMember]
            public IList<NameValuePairSnapshot> NameValuePairs { get; set; }
            [DataMember]
            public IList<LogNodeSnapshot> SubNodes { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = "NWheels.Logging", Name = "Pair")]
        public class NameValuePairSnapshot
        {
            [DataMember]
            public string Name { get; set; }
            [DataMember]
            public string Value { get; set; }
            [DataMember]
            public bool IsDetail { get; set; }
        }
    }
}
