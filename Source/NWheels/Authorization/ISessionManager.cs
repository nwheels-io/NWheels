using System;
using System.Security.Principal;
using NWheels.Endpoints.Core;

namespace NWheels.Authorization
{
    public interface ISessionManager
    {
        IDisposable OpenSession(IPrincipal userPrincipal, IEndpoint originatorEndpoint);
        IDisposable JoinSession(string sessionId);
        IDisposable JoinAnonymous();
        IDisposable JoinSystem();
    }
}