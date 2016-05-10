using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Entities;

namespace NWheels.Domains.DevOps.Logging.Entities
{
    public interface ILogExplorerContext : IApplicationDataRepository
    {
        IEntityRepository<ILogLevelDailySummaryEntity> LogLevelDailySummaries { get; }
        IEntityRepository<ILogMessageDailySummaryEntity> LogMessageDailySummaries { get; }
        IEntityRepository<ILogMessageEntity> LogMessages { get; }
        IEntityRepository<IThreadLogEntity> ThreadLogs { get; }
    }
}
