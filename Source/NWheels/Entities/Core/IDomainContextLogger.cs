using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Logging;

namespace NWheels.Entities.Core
{
    public interface IDomainContextLogger : IApplicationEventLogger
    {
        [LogDebug]
        void NewRootUnitOfWork(string domainContext);

        [LogDebug]
        void NewNestedUnitOfWork(string domainContext);

        [LogDebug]
        void EndOfNestedUnitOfWork(string domainContext);

        [LogDebug]
        void EndOfRootUnitOfWork(string domainContext);

        [LogActivity]
        ILogActivity CommittingChanges(string domainContext);

        [LogActivity]
        ILogActivity RollingChangesBack(string domainContext);

        [LogActivity]
        ILogActivity ExecutingValidationPhase();

        [LogActivity]
        ILogActivity ExecutingBeforeSavePhase();

        [LogActivity]
        ILogActivity ExecutingAfterSavePhase();

        [LogActivity]
        ILogActivity CommittingChangesToPersistenceLayer();

        [LogActivity]
        ILogActivity ValidateObject(string obj);

        [LogActivity]
        ILogActivity BeforeSaveObject(string obj);

        [LogActivity]
        ILogActivity AfterSaveObject(string obj);

        [LogError]
        void CommitChangesFailed();

        [LogError]
        void RollbackChangesFailed();

        [LogWarning]
        void FailedToVisitNestedObjects(Exception e);

        [LogVerbose]
        void CreatRepositoryPartition(Type entity, object partitionValue);

        [LogCritical]
        void StaleUnitOfWorkEncountered(string domainContext, string initializedBy);
    }
}
