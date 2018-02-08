using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NWheels.Configuration.Api;

namespace NWheels.Communication.Api
{
    [ConfigurationSection(Name = "Communication")]
    public interface ICommunicationConfigSection
    {
        IList<IEndpointConfigElement> Endpoints { get; }
    }
}
