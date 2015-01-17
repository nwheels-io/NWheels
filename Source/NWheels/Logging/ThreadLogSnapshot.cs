using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Logging
{
    [DataContract(Namespace = "NWheels.Logging", Name = "ThreadLog")]
    [KnownType(typeof(LogNodeSnapshot))]
    [KnownType(typeof(ActivityNodeSnapshot))]
    public class ThreadLogSnapshot
    {
        [DataMember]
        public Guid LogId { get; set; }
        [DataMember]
        public Guid CorrelationId { get; set; }
        [DataMember]
        public DateTime StartedAtUtc { get; set; }
        [DataMember]
        public ThreadTaskType TaskType { get; set; }
        [DataMember]
        public ActivityNodeSnapshot RootActivity { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = "NWheels.Logging", Name = "Log")]
        public class LogNodeSnapshot
        {
            [DataMember]
            public LogContentTypes ContentTypes { get; set; }
            [DataMember]
            public LogLevel Level { get; set; }
            [DataMember]
            public long MillisecondsTimestamp { get; set; }
            [DataMember]
            public string SingleLineText { get; set; }
            [DataMember]
            public string FullDetailsText { get; set; }
            [DataMember]
            public string ExceptionTypeName { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = "NWheels.Logging", Name = "Activity")]
        public class ActivityNodeSnapshot : LogNodeSnapshot
        {
            [DataMember]
            public long MillisecondsDuration { get; set; }
            [DataMember]
            public LogNodeSnapshot[] SubNodes { get; set; }
        }
    }
}
