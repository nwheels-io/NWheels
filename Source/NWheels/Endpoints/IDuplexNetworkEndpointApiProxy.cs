using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Endpoints
{
    public interface IDuplexNetworkEndpointApiProxy : IDisposable
    {
        void NotifySessionClosed(SessionCloseReason reason);
        IDuplexNetworkEndpointTransport Transport { get; }
        Type RemoteContract { get; }
        Type LocalContract { get; }
        object LocalServer { get; }
        event Action Disposing;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public enum SessionCloseReason
    {
        Unknown,
        ByContract,
        LocalPartyShutDown,
        RemotePartyNotReachable,
        ProtocolError
    }
}
