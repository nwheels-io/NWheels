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
    public interface IThreadLogEntity
    {
        [PropertyContract.EntityId]
        string LogId { get; set; }
        
        DateTime Timestamp { get; set; }
        ThreadTaskType TaskType { get; set; }
        string CorrelationId { get; set; }
        string MachineName { get; set; }
        string NodeName { get; set; }
        string NodeInstance { get; set; }
        LogLevel Level { get; set; }
        ThreadLogSnapshot Snapshot { get; set; }
    }
}
