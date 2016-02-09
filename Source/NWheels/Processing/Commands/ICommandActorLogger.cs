using System;
using System.Security.Principal;
using NWheels.Authorization;
using NWheels.Endpoints.Core;
using NWheels.Entities;
using NWheels.Exceptions;
using NWheels.Logging;

namespace NWheels.Processing.Commands
{
    public interface ICommandActorLogger : IApplicationEventLogger
    {
        [LogDebug]
        void LookingUpEntity(IEntityId id);

        [LogError]
        EntityNotFoundException EntityNotFound(IEntityId id);

        [LogActivity(LogLevel.Verbose, ToAuditLog = true)]
        ILogActivity ExecutingCommand(AbstractCommandMessage command, IEndpoint endpoint, ISession session, IPrincipal principal);

        [LogError]
        void CommandFailed(AbstractCommandMessage command, IEndpoint endpoint, ISession session, IPrincipal principal, Exception error);
    }
}
