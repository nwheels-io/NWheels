using System;

namespace NWheels.Kernel.Api.Injection
{
    public interface IComponentContainerBuilder
    {
        IComponentInstantiationBuilder RegisterComponentType<TComponent>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        IComponentInstantiationBuilder RegisterComponentType(Type componentType);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IComponentRegistrationBuilder RegisterComponentInstance<TComponent>(TComponent componentInstance)
            where TComponent : class;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void RegisterAdapterPort<TPort>(TPort port)
            where TPort : class, IAdapterInjectionPort;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void RegisterAdapterComponentType<TAdapterInterface, TAdapterConfig>(
            AdapterInjectionPort<TAdapterInterface, TAdapterConfig> adapterInjectionPort, 
            Type adapterComponentType)
            where TAdapterInterface : class;
    }
}
