using System;

namespace NWheels.Injection
{
    public interface IComponentContainerBuilder
    {
        IComponentInstantiationBuilder RegisterComponentType<TComponent>();
        IComponentInstantiationBuilder RegisterComponentType(Type componentType);
        IComponentRegistrationBuilder RegisterComponentInstance<TComponent>(TComponent componentInstance)
            where TComponent : class;
    }
}
