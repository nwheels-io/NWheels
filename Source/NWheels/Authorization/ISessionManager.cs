using System;
using System.Security.Principal;
using NWheels.Endpoints.Core;

namespace NWheels.Authorization
{
    public interface ISessionManager
    {
        IDisposable JoinSession(string sessionId);
        IDisposable JoinSessionOrOpenAnonymous(string sessionId, IEndpoint endpoint);
        IDisposable JoinAnonymous();
        IDisposable JoinSystem();
        ISession CurrentSession { get; }
    }
}