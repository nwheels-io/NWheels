using NWheels.Entities;

namespace NWheels.Domains.DevOps.SystemLogs.Entities
{
    public interface ISystemLogContext : IApplicationDataRepository
    {
        IEntityRepository<ILogLevelSummaryEntity> LogLevelDailySummaries { get; }
        IEntityRepository<ILogMessageSummaryEntity> LogMessageDailySummaries { get; }
        IEntityRepository<ILogMessageEntity> LogMessages { get; }
        IEntityRepository<IThreadLogEntity> ThreadLogs { get; }

        IThreadLogUINodeEntity NewThreadLogNode();
        IRootThreadLogUINodeEntity NewRootThreadLogNode();
    }
}
