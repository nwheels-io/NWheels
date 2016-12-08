using NWheels.Domains.DevOps.Alerts.Entities;
using NWheels.Entities;

namespace NWheels.Domains.DevOps.SystemLogs.Entities
{
    public interface ISystemLogContext : IApplicationDataRepository
    {
        IEntityRepository<ILogLevelSummaryEntity> LogLevelDailySummaries { get; }
        IEntityRepository<ILogMessageSummaryEntity> LogMessageDailySummaries { get; }
        IEntityRepository<ILogMessageEntity> LogMessages { get; }
        IEntityRepository<IThreadLogEntity> ThreadLogs { get; }
        IEntityRepository<IAlertEntity> Alerts { get; }

        IThreadLogUINodeEntity NewThreadLogNode();
        IRootThreadLogUINodeEntity NewRootThreadLogNode();

        IEntityPartAlertAction NewEntityPartAlertAction();
        IEntityPartAlertByEmail NewEntityPartAlertByEmail();
        IEntityPartEmailRecipient NewEntityPartEmailRecipient();
        IEntityPartEmailAddressRecipient NewEntityPartEmailAddressRecipient();
        IEntityPartUserAccountEmailRecipient NewEntityPartUserAccountEmailRecipient();
    }
}
