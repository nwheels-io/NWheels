using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NWheels.Configuration.Api;

namespace NWheels.Communication.Api
{
    [ConfigurationElement]
    public interface IEndpointConfigElement
    {
        [ConfigurationKey]
        string Name { get; set; }
    }
}
