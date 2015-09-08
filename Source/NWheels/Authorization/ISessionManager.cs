using System;
using System.Security.Principal;
using NWheels.Endpoints.Core;

namespace NWheels.Authorization
{
    public interface ISessionManager
    {
        IDisposable JoinSession(string sessionId);
        IDisposable OpenAnonymous(IEndpoint endpoint);
        IDisposable JoinSessionOrOpenAnonymous(string sessionId, IEndpoint endpoint);
        IDisposable JoinGlobalAnonymous();
        IDisposable JoinGlobalSystem();
        ISession CurrentSession { get; }
    }
}