using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Configuration;
using NWheels.Endpoints;

namespace NWheels.Stacks.Network
{
    [ConfigurationElement(XmlName = "TcpBinaryTransport")]
    [ConfiguredComponent(typeof(TcpBinaryTransport))]
    public interface ITcpBinaryTransportConfiguration : IAbstractNetwrokTransportConfig
    {
        AddressListenMode ListenMode { set; get; }
        int ReceiveBufferSize { get; set; }      // Default: 1000 ????
        int IncomingPort { get; set; }
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------------

    //
    // when waiting for an incomming connection - whom to open the connection to:
    //
    public enum AddressListenMode
    {
        None,           // This is not an endpoint which waits for external connections
        External,       // Only outside of the local host
        Internal,       // Only internally to the local host
        Any             // All incoming connections are accepted
    }

}
