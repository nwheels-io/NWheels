using Autofac;
using NWheels.Microservices;
using System;

namespace NWheels.Injection.Adapters.Autofac
{
    public class ComponentContainerBuilder : IInternalComponentContainerBuilder
    {
        private readonly ContainerBuilder _containerBuilder;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ComponentContainerBuilder()
        {
            _containerBuilder = new ContainerBuilder();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IInternalComponentContainer CreateComponentContainer()
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

        public void Register<TService, TComponent>(LifeStyle lifeStyle = LifeStyle.Singleton) 
            where TComponent : TService
        {
            //TODO: apply lifestyle setting
            _containerBuilder.RegisterType<TComponent>().AsSelf().As<TService>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Register<TService1, TService2, TComponent>(LifeStyle lifeStyle = LifeStyle.Singleton) 
            where TComponent : TService1, TService2
        {
            //TODO: apply lifestyle setting
            _containerBuilder.RegisterType<TComponent>().AsSelf().As<TService1, TService2>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Register<TService1, TService2, TService3, TComponent>(LifeStyle lifeStyle = LifeStyle.Singleton) 
            where TComponent : TService1, TService2, TService3
        {
            //TODO: apply lifestyle setting
            _containerBuilder.RegisterType<TComponent>().AsSelf().As<TService1, TService2, TService3>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Register(Type type, LifeStyle lifeStyle = LifeStyle.Singleton)
        {
            //TODO: apply lifestyle setting
            _containerBuilder.RegisterType(type).AsSelf();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Register<TService>(Type type, LifeStyle lifeStyle = LifeStyle.Singleton)
        {
            //TODO: apply lifestyle setting
            _containerBuilder.RegisterType(type).AsSelf().As<TService>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Register<TService1, TService2>(Type type, LifeStyle lifeStyle = LifeStyle.Singleton)
        {
            //TODO: apply lifestyle setting
            _containerBuilder.RegisterType(type).AsSelf().As<TService1, TService2>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Register<TService1, TService2, TService3>(Type type, LifeStyle lifeStyle = LifeStyle.Singleton)
        {
            //TODO: apply lifestyle setting
            _containerBuilder.RegisterType(type).AsSelf().As<TService1, TService2, TService3>();
        }
    }
}
