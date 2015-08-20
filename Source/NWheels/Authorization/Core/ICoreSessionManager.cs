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
        ISession GetCurrentSession();
        ISession[] GetOpenSessions();
        void DropSession(string sessionId);
    }
}
