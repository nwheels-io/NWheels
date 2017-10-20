using System;

namespace NWheels.Kernel.Api.Injection
{
    public interface IAdapterInjectionPort
    {
        int PortKey { get; }
        Type AdapterInterfaceType { get; }
        Type AdapterConfigurationType { get; }
        Type AdapterComponentType { get; }
    }
}
