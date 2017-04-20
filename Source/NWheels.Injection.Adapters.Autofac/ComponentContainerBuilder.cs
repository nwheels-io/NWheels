using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using NWheels.Microservices;
using System;
using System.Collections.Generic;

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

        public IComponentInstantiationBuilder RegisterComponentType<TComponent>()
        {
            var registration = _containerBuilder.RegisterType<TComponent>().AsSelf();
            return new RegistrationBuilderWrapper<TComponent, ConcreteReflectionActivatorData, SingleRegistrationStyle>(registration);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentInstantiationBuilder RegisterComponentType(Type componentType)
        {
            var registration = _containerBuilder.RegisterType(componentType).AsSelf();
            return new RegistrationBuilderWrapper<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>(registration);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentRegistrationBuilder RegisterComponentInstance<TComponent>(TComponent componentInstance)
            where TComponent : class
        {
            var registration = _containerBuilder.RegisterInstance<TComponent>(componentInstance).AsSelf();
            return new RegistrationBuilderWrapper<TComponent, SimpleActivatorData, SingleRegistrationStyle>(registration);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IInternalComponentContainer CreateComponentContainer(bool isRootContainer)
        {
            var underlyingContainer = _containerBuilder.Build();
            var wrappingContainer = new ComponentContainer(underlyingContainer);

            if (isRootContainer)
            {
                RegisterSelf(underlyingContainer, wrappingContainer);
            }

            return wrappingContainer;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Register<TService>(object instance)
        {
            _containerBuilder.RegisterInstance(instance).As<TService>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentInstantiationBuilder Register<TComponent>(LifeStyle lifeStyle = LifeStyle.Singleton)
        {
            //TODO: apply lifestyle setting
            var registration = _containerBuilder.RegisterType<TComponent>().AsSelf();
            return new RegistrationBuilderWrapper<TComponent, ConcreteReflectionActivatorData, SingleRegistrationStyle>(registration);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentInstantiationBuilder Register<TService, TComponent>(LifeStyle lifeStyle = LifeStyle.Singleton) 
            where TComponent : TService
        {
            //TODO: apply lifestyle setting
            var registration = _containerBuilder.RegisterType<TComponent>().AsSelf().As<TService>();
            return new RegistrationBuilderWrapper<TComponent, ConcreteReflectionActivatorData, SingleRegistrationStyle>(registration);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentInstantiationBuilder Register<TService1, TService2, TComponent>(LifeStyle lifeStyle = LifeStyle.Singleton) 
            where TComponent : TService1, TService2
        {
            //TODO: apply lifestyle setting
            var registration = _containerBuilder.RegisterType<TComponent>().AsSelf().As<TService1, TService2>();
            return new RegistrationBuilderWrapper<TComponent, ConcreteReflectionActivatorData, SingleRegistrationStyle>(registration);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentInstantiationBuilder Register<TService1, TService2, TService3, TComponent>(LifeStyle lifeStyle = LifeStyle.Singleton) 
            where TComponent : TService1, TService2, TService3
        {
            //TODO: apply lifestyle setting
            var registration = _containerBuilder.RegisterType<TComponent>().AsSelf().As<TService1, TService2, TService3>();
            return new RegistrationBuilderWrapper<TComponent, ConcreteReflectionActivatorData, SingleRegistrationStyle>(registration);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentInstantiationBuilder Register(Type type, LifeStyle lifeStyle = LifeStyle.Singleton)
        {
            //TODO: apply lifestyle setting
            var registration = _containerBuilder.RegisterType(type).AsSelf();
            return new RegistrationBuilderWrapper<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>(registration);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentInstantiationBuilder Register<TService>(Type type, LifeStyle lifeStyle = LifeStyle.Singleton)
        {
            //TODO: apply lifestyle setting
            var registration = _containerBuilder.RegisterType(type).AsSelf().As<TService>();
            return new RegistrationBuilderWrapper<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>(registration);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentInstantiationBuilder Register<TService1, TService2>(Type type, LifeStyle lifeStyle = LifeStyle.Singleton)
        {
            //TODO: apply lifestyle setting
            var registration = _containerBuilder.RegisterType(type).AsSelf().As<TService1, TService2>();
            return new RegistrationBuilderWrapper<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>(registration);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentInstantiationBuilder Register<TService1, TService2, TService3>(Type type, LifeStyle lifeStyle = LifeStyle.Singleton)
        {
            //TODO: apply lifestyle setting
            var registration = _containerBuilder.RegisterType(type).AsSelf().As<TService1, TService2, TService3>();
            return new RegistrationBuilderWrapper<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>(registration);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RegisterInstance<TService>(object componentInstance)
        {
            _containerBuilder.RegisterInstance(componentInstance).As <TService>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RegisterInstance<TService1, TService2>(object componentInstance)
        {
            _containerBuilder.RegisterInstance(componentInstance).As<TService1, TService2>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RegisterInstance<TService1, TService2, TService3>(object componentInstance)
        {
            _containerBuilder.RegisterInstance(componentInstance).As<TService1, TService2, TService3>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RegisterSelf(IContainer underlyingContainer, ComponentContainer wrappingContainer)
        {
            underlyingContainer.ComponentRegistry.Register(
                new ComponentRegistration(
                    Guid.NewGuid(),
                    new ProvidedInstanceActivator(wrappingContainer),
                    new CurrentScopeLifetime(),
                    InstanceSharing.Shared,
                    InstanceOwnership.ExternallyOwned,
                    new Service[] {
                        new TypedService(typeof(IComponentContainer)),
                        new TypedService(typeof(IInternalComponentContainer))
                    },
                    new Dictionary<string, object>()
                )
            );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class RegistrationBuilderWrapper<TLimit, TActivatorData, TRegistrationStyle> : IComponentRegistrationBuilder, IComponentInstantiationBuilder
        {
            private readonly IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> _inner;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public RegistrationBuilderWrapper(IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> inner)
            {
                _inner = inner;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IComponentInstantiationBuilder WithParameter<T>(T value)
            {
                InnerAsReflectionActivator().WithParameter(new TypedParameter(typeof(T), value));
                return this;
            }

            private IRegistrationBuilder<TLimit, ReflectionActivatorData, TRegistrationStyle> InnerAsReflectionActivator()
            {
                return (IRegistrationBuilder<TLimit, ReflectionActivatorData, TRegistrationStyle>)_inner;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IComponentRegistrationBuilder SingleInstance()
            {
                _inner.SingleInstance();
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IComponentRegistrationBuilder InstancePerDependency()
            {
                _inner.InstancePerDependency();
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IComponentRegistrationBuilder ForService<TService>()
            {
                _inner.As<TService>();
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IComponentRegistrationBuilder ForServices<TService1, TService2>()
            {
                _inner.As<TService1, TService2>();
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IComponentRegistrationBuilder ForServices<TService1, TService2, TService3>()
            {
                _inner.As<TService1, TService2, TService3>();
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IComponentRegistrationBuilder ForServices(params Type[] serviceTypes)
            {
                _inner.As(serviceTypes);
                return this;
            }
        }
    }
}
