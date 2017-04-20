using NWheels.Compilation.Mechanism.Factories;
using NWheels.Injection;
using NWheels.Microservices;

namespace NWheels.Injection
{
    public static class ComponentContainerBuilderExtensions
    {
        public static void ContributeLifecycleListener<TComponent>(this IComponentContainerBuilder containerBuilder) 
            where TComponent : ILifecycleListenerComponent
        {
            containerBuilder.RegisterComponentType<TComponent>()
                .SingleInstance()
                .ForService<ILifecycleListenerComponent>();
        }
    }
}
