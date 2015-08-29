using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using NWheels.Endpoints.Core;

namespace NWheels.Authorization.Core
{
    public interface ICoreSessionManager
    {
        ISession OpenSession(IPrincipal userPrincipal, IEndpoint originatorEndpoint);
        ISession AuthorieSession(IPrincipal userPrincipal);
        ISession[] GetOpenSessions();
        void CloseCurrentSession();
        void DropSession(string sessionId);
        byte[] EncryptSessionId(string clearSessionId);
        string DecryptSessionId(byte[] encryptedSessionId);
        string SessionIdCookieName { get; }
    }
}
