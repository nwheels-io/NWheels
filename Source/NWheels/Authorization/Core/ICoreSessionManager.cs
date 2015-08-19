using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Authorization.Core
{
    public interface ICoreSessionManager
    {
        ISession[] GetOpenSessions();
        void DropSession(string sessionId);
    }
}
