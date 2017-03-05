using Autofac;
using NWheels.Microservices;
using System;

namespace NWheels.Injection.Adapters.Autofac
{
    public class ComponentContainerBuilder : IComponentContainerBuilder
    {
        readonly ContainerBuilder _containerBuilder;

        public ComponentContainerBuilder()
        {
            _containerBuilder = new ContainerBuilder();
        }

        public void ContributeLifecycleListener<T>() where T : ILifecycleListenerComponent
        {
            throw new NotImplementedException();
        }

        public IComponentContainer CreateComponentContainer()
        {
            var container = _containerBuilder.Build();
            var containerWrapper = new ComponentContainer(container);
            return containerWrapper;
        }

        public void Register<TInterface, TImplementation>(LifeStyle lifeStyle = LifeStyle.Singleton) where TImplementation : TInterface
        {
            _containerBuilder.RegisterType<TImplementation>().As<TInterface>();
        }

        public void Register<TInterface>(Type type, LifeStyle lifeStyle = LifeStyle.Singleton)
        {
            _containerBuilder.RegisterType(type).As<TInterface>();
        }
    }
}
