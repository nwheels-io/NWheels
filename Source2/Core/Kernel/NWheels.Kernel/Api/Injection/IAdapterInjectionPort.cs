using System;

namespace NWheels.Kernel.Api.Injection
{
    public interface IAdapterInjectionPort
    {
        void Configure(IComponentContainerBuilder newComponents); 
        int PortKey { get; }
        Type AdapterInterfaceType { get; }
        Type AdapterConfigurationType { get; }
        Type AdapterComponentType { get; }
    }
}
