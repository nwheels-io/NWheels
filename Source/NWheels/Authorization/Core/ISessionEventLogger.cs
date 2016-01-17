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
        [LogError(AuditLog = true)]
        SecurityException DuplicateSessionId(string sessionId);

        [LogWarning(AuditLog = true)]
        SecurityException SessionNotFound(string sessionId);

        [LogVerbose(AuditLog = true)]
        void SessionOpened(string sessionId, IPrincipal user, IEndpoint endpoint);

        [LogVerbose(AuditLog = true)]
        void SessionClosed(string sessionId, IPrincipal user, IEndpoint endpoint);

        [LogVerbose(AuditLog = true)]
        void ThreadJoiningSession(string sessionId, IPrincipal user, IEndpoint endpoint);

        [LogVerbose(AuditLog = true)]
        void ThreadLeavingSession(string sessionId, IPrincipal user, IEndpoint endpoint);

        [LogVerbose(AuditLog = true)]
        void ThreadJoiningAnonymousSession();

        [LogVerbose(AuditLog = true)]
        void ThreadJoiningSystemSession();

        [LogVerbose(AuditLog = true)]
        void DroppingSession(string sessionId);

        [LogVerbose(AuditLog = true)]
        void ClosingSession(string sessionId);
    }
}
