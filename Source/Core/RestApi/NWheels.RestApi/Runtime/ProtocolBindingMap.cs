using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NWheels.RestApi.Api;

namespace NWheels.RestApi.Runtime
{
    public class ProtocolBindingMap : Dictionary<string, IResourceBinding>
    {
        public ProtocolBindingMap()
            : base(1024, StringComparer.InvariantCultureIgnoreCase)
        {
        }
    }
}
