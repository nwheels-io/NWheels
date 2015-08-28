using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using NWheels.Authorization;
using NWheels.Endpoints.Core;
using NWheels.Entities;
using NWheels.Exceptions;
using NWheels.Logging;

namespace NWheels.Processing.Commands.Impl
{
    public interface ICommandActorLogger : IApplicationEventLogger
    {
        [LogDebug]
        void LookingUpEntity(IEntityId id);

        [LogError]
        EntityNotFoundException EntityNotFound(IEntityId id);

        [LogActivity]
        ILogActivity ExecutingCommand(AbstractCommandMessage command, IEndpoint endpoint, ISession session, IPrincipal principal);

        [LogError]
        void CommandFailed(AbstractCommandMessage command, IEndpoint endpoint, ISession session, IPrincipal principal, Exception error);
    }
}
