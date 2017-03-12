using NWheels.Microservices;
using System;

namespace NWheels.Injection
{
    public interface IComponentContainerBuilder
    {
        void ContributeLifecycleListener<T>() where T : ILifecycleListenerComponent;

        //TODO: refactor the following APIs to be more self-documenting
        //for example: 
        //  RegisterComponent<...>().ForServices<...>()
        //  or
        //  RegisterImplementation<...>().ForInterfaces<...>()

        void Register<TInterface, TImplementation>(LifeStyle lifeStyle = LifeStyle.Singleton) 
            where TImplementation : TInterface;

        void Register<TInterface1, TInterface2, TImplementation>(LifeStyle lifeStyle = LifeStyle.Singleton) 
            where TImplementation : TInterface1, TInterface2;

        void Register<TInterface1, TInterface2, TInterface3, TImplementation>(LifeStyle lifeStyle = LifeStyle.Singleton) 
            where TImplementation : TInterface1, TInterface2, TInterface3;

        void Register<TInterface>(Type type, LifeStyle lifeStyle = LifeStyle.Singleton);
        void Register<TInterface1, TInterface2>(Type type, LifeStyle lifeStyle = LifeStyle.Singleton);
        void Register<TInterface1, TInterface2, TInterface3>(Type type, LifeStyle lifeStyle = LifeStyle.Singleton);

        IComponentContainer CreateComponentContainer();
    }
}
