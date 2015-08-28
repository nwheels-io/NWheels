using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using NWheels.Endpoints.Core;
using NWheels.Exceptions;
using NWheels.Logging;

namespace NWheels.Authorization.Core
{
    public interface ISessionEventLogger : IApplicationEventLogger
    {
        [LogError(Audit = true)]
        SecurityException DuplicateSessionId(string sessionId);

        [LogWarning(Audit = true)]
        SecurityException SessionNotFound(string sessionId);

        [LogVerbose(Audit = true)]
        void SessionOpened(string sessionId, IPrincipal user, IEndpoint endpoint);

        [LogVerbose(Audit = true)]
        void SessionClosed(string sessionId, IPrincipal user, IEndpoint endpoint);

        [LogVerbose(Audit = true)]
        void ThreadJoiningSession(string sessionId, IPrincipal user, IEndpoint endpoint);

        [LogVerbose(Audit = true)]
        void ThreadLeavingSession(string sessionId, IPrincipal user, IEndpoint endpoint);

        [LogVerbose(Audit = true)]
        void ThreadJoiningAnonymousSession();

        [LogVerbose(Audit = true)]
        void ThreadJoiningSystemSession();

        [LogVerbose(Audit = true)]
        void DroppingSession(string sessionId);
    }
}
