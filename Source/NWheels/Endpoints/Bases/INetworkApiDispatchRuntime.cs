using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Endpoints.Bases
{
    public interface INetworkApiDispatchRuntime
    {
        void ConnectMessage(byte[] headersAndBody);
        void DisconnectMessage(byte[] headersAndBody);
        void ApplicationMessage(byte[] headersAndBody);
        event Action<byte[]> Connected;
        event Action<byte[]> ConnectDeclined;
        event Action<byte[]> ApplicationEvent;
    }
}
