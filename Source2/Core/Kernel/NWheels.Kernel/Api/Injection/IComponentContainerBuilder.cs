using System;

namespace NWheels.Kernel.Api.Injection
{
    public interface IComponentContainerBuilder
    {
        IComponentInstantiationBuilder RegisterComponentType<TComponent>();
        IComponentInstantiationBuilder RegisterComponentType(Type componentType);
        IComponentRegistrationBuilder RegisterComponentInstance<TComponent>(TComponent componentInstance)
            where TComponent : class;
    }
}
