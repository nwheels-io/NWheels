using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Endpoints
{
    public interface INetworkProtocolConfigContainer
    {
        IAbstractNetwrokTransportConfig ConcreteConfig { get; set; }
    }
}
