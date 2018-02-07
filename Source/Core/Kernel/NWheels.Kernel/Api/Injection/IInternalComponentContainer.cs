using System;
using System.Collections.Generic;

namespace NWheels.Kernel.Api.Injection
{
    public interface IInternalComponentContainer : IComponentContainer
    {
        void Merge(IInternalComponentContainerBuilder containerBuilder);
        TAdapterInterface ResolveAdapter<TAdapterInterface, TAdapterConfig>(AdapterInjectionPort<TAdapterInterface, TAdapterConfig> port)
            where TAdapterInterface : class;
    }
}
