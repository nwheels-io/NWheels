using Autofac;
using NWheels.Microservices;
using System;

namespace NWheels.Injection.Adapters.Autofac
{
    public class ComponentContainerBuilder : IComponentContainerBuilder
    {
        private readonly ContainerBuilder _containerBuilder;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ComponentContainerBuilder()
        {
            _containerBuilder = new ContainerBuilder();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ContributeLifecycleListener<T>() where T : ILifecycleListenerComponent
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentContainer CreateComponentContainer()
        {
            var container = _containerBuilder.Build();
            var containerWrapper = new ComponentContainer(container);
            return containerWrapper;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Register<TComponent>(LifeStyle lifeStyle = LifeStyle.Singleton)
        {
            //TODO: apply lifestyle setting
            _containerBuilder.RegisterType<TComponent>().AsSelf();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Register<TInterface, TImplementation>(LifeStyle lifeStyle = LifeStyle.Singleton) 
            where TImplementation : TInterface
        {
            //TODO: apply lifestyle setting
            _containerBuilder.RegisterType<TImplementation>().AsSelf().As<TInterface>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Register<TInterface1, TInterface2, TImplementation>(LifeStyle lifeStyle = LifeStyle.Singleton) 
            where TImplementation : TInterface1, TInterface2
        {
            //TODO: apply lifestyle setting
            _containerBuilder.RegisterType<TImplementation>().AsSelf().As<TInterface1, TInterface2>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Register<TInterface1, TInterface2, TInterface3, TImplementation>(LifeStyle lifeStyle = LifeStyle.Singleton) 
            where TImplementation : TInterface1, TInterface2, TInterface3
        {
            //TODO: apply lifestyle setting
            _containerBuilder.RegisterType<TImplementation>().AsSelf().As<TInterface1, TInterface2, TInterface3>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Register(Type type, LifeStyle lifeStyle = LifeStyle.Singleton)
        {
            //TODO: apply lifestyle setting
            _containerBuilder.RegisterType(type).AsSelf();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Register<TInterface>(Type type, LifeStyle lifeStyle = LifeStyle.Singleton)
        {
            //TODO: apply lifestyle setting
            _containerBuilder.RegisterType(type).AsSelf().As<TInterface>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Register<TInterface1, TInterface2>(Type type, LifeStyle lifeStyle = LifeStyle.Singleton)
        {
            //TODO: apply lifestyle setting
            _containerBuilder.RegisterType(type).AsSelf().As<TInterface1, TInterface2>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Register<TInterface1, TInterface2, TInterface3>(Type type, LifeStyle lifeStyle = LifeStyle.Singleton)
        {
            //TODO: apply lifestyle setting
            _containerBuilder.RegisterType(type).AsSelf().As<TInterface1, TInterface2, TInterface3>();
        }
    }
}
