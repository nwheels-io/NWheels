using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Endpoints
{
    public interface IDuplexNetworkEndpointApiProxy
    {
        IDuplexNetworkEndpointTransport Transport { get; }
        Type RemoteContract { get; }
        Type LocalContract { get; }
        object LocalServer { get; }
    }
}
