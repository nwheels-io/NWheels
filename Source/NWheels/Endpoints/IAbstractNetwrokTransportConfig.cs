using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Configuration;

namespace NWheels.Endpoints
{
    [ConfigurationElement(IsAbstract = true)]
    public interface IAbstractNetwrokTransportConfig : IConfigurationElement
    {
        //INetworkEndpointTransport CreateConfiguredComponent();
    }
}
