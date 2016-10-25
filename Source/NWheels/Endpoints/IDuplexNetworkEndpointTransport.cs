using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Endpoints
{
    public interface IDuplexNetworkEndpointTransport
    {
        void SendBytes(byte[] bytes);
        event Action<byte[]> BytesReceived;
        event Action<Exception> SendFailed;
        event Action<Exception> ReceiveFailed;
    }
}
