using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Domains.SystemMonitor.Contracts
{
    public interface ISystemMonitorContext : IApplicationDataRepository
    {
        IEntityRepository<IKeyValueIndexEntity> KeyValueIndex { get; }
        IEntityRepository<ILogDailySummaryEntity> LogDailySummaries { get; }
        IEntityRepository<ILogMessageEntity> LogMessages { get; }
        IEntityRepository<IThreadLogEntity> ThreadLogs { get; }
    }
}
