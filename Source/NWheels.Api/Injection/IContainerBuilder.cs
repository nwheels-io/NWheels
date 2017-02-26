using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Injection
{
    public interface IContainerBuilder
    {
        void ContributeLifecycleListener<T>() where T : ILifecycleListenerComponent;

        void Register<TInterface, TImplementation>(LifeStyle lifeStyle = LifeStyle.Singleton) where TImplementation : TInterface;
    }
}
