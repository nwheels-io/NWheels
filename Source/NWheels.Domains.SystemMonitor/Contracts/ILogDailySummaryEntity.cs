using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.DataObjects;
using NWheels.Entities;
using NWheels.Logging;

namespace NWheels.Domains.SystemMonitor.Contracts
{
    [EntityContract]
    public interface ILogDailySummaryEntity
    {
        [PropertyContract.EntityId]
        string Id { get; set; }
        
        DateTime Date { get; set; }
        string MachineName { get; set; }
        string NodeName { get; set; }
        string NodeInstance { get; set; }
        LogLevel Level { get; set; }
        string Logger { get; set; }
        string MessageId { get; set; }
        string ExceptionType { get; set; }
        Dictionary<string, int> Hour { get; set; }
        Dictionary<string, Dictionary<string, int>> Minute { get; set; }
    }
}
