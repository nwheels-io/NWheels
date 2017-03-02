using Autofac;
using NWheels.Microservices;
using System;

namespace NWheels.Injection.Adapters.Autofac
{
    public class ContainerBuilderWrapper : IContainerBuilderWrapper
    {
        readonly ContainerBuilder _containerBuilder;

        public ContainerBuilderWrapper()
        {
            _containerBuilder = new ContainerBuilder();
        }

        public void ContributeLifecycleListener<T>() where T : ILifecycleListenerComponent
        {
            throw new NotImplementedException();
        }

        public IContainerWrapper CreateContainer()
        {
            var container = _containerBuilder.Build();
            var containerWrapper = new ContainerWrapper(container);
            return containerWrapper;
        }

        public void Register<TInterface, TImplementation>(LifeStyle lifeStyle = LifeStyle.Singleton) where TImplementation : TInterface
        {
            
        }
    }
}
