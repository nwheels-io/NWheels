using NWheels.Microservices;
using System;

namespace NWheels.Injection
{
    public interface IComponentContainerBuilder
    {
        void ContributeLifecycleListener<T>() where T : ILifecycleListenerComponent;

        void Register<TInterface, TImplementation>(LifeStyle lifeStyle = LifeStyle.Singleton) where TImplementation : TInterface;

        void Register<TInterface>(Type type, LifeStyle lifeStyle = LifeStyle.Singleton);

        IComponentContainer CreateComponentContainer();
    }
}
