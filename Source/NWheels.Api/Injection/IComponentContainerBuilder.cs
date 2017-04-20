using System;

namespace NWheels.Injection
{
    public interface IComponentContainerBuilder
    {
        //TODO: refactor the following APIs to be more self-documenting
        //for example: 
        //  RegisterComponent<...>().ForServices<...>()

        IComponentInstantiationBuilder RegisterComponentType<TComponent>();
        IComponentInstantiationBuilder RegisterComponentType(Type componentType);
        IComponentRegistrationBuilder RegisterComponentInstance<TComponent>(TComponent componentInstance)
            where TComponent : class;
    }
}
