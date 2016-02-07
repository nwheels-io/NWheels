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
        [LogWarning]
        SecurityException DuplicateSessionId(string sessionId);

        [LogWarning]
        SecurityException SessionNotFound(string sessionId);

        [LogVerbose]
        void SessionOpened(string sessionId, IPrincipal user, IEndpoint endpoint);

        [LogVerbose]
        void SessionClosed(string sessionId, IPrincipal user, IEndpoint endpoint);

        [LogVerbose]
        void ThreadJoiningSession(string sessionId, IPrincipal user, IEndpoint endpoint);

        [LogVerbose]
        void ThreadLeavingSession(string sessionId, IPrincipal user, IEndpoint endpoint);

        [LogVerbose]
        void ThreadJoiningAnonymousSession();

        [LogVerbose]
        void ThreadJoiningSystemSession();

        [LogVerbose]
        void DroppingSession(string sessionId);

        [LogVerbose]
        void ClosingSession(string sessionId);
    }
}
