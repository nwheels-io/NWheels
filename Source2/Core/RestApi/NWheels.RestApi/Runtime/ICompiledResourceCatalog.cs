using System;
using System.Collections.Generic;
using NWheels.RestApi.Api;

namespace NWheels.RestApi.Runtime
{
    public interface ICompiledResourceCatalog
    {
        ResourceCatalogDescription GetDescription();
        void PopulateRouter(IDictionary<string, ProtocolBindingMap> bindingMapByProtocol);
    }
}
