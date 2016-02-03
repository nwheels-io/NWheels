using NWheels.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Domains.SystemMonitor.Contracts
{
    [EntityContract]
    public interface ILogMessageEntity
    {
        DateTime Timestamp { get; set; }
        string MachineName { get; set; }
        string NodeName { get; set; }
        string NodeInstance { get; set; }
        LogLevel Level { get; set; }
        string Logger { get; set; }
        string MessageId { get; set; }
        long? DurationMs { get; set; }
        string ExceptionType { get; set; }
        string ExceptionDetails { get; set; }
        string ThreadLogId { get; set; }
        string CorrelationId { get; set; }
        string[] KeyValues { get; set; }
        string[] AdditionalDetails { get; set; }
    }
}
