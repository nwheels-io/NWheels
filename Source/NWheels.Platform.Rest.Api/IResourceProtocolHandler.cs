using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Platform.Rest
{
    public interface IResourceProtocolHandler
    {
        Type ProtocolInterface { get; }
        string Name { get; }
    }
}
