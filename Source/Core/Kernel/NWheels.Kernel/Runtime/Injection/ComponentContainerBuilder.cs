using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using System;
using System.Collections.Generic;
using NWheels.Kernel.Api.Injection;

namespace NWheels.Kernel.Runtime.Injection
{
    public class ComponentContainerBuilder : IInternalComponentContainerBuilder
    {
        private readonly ContainerBuilder _containerBuilder;
        private readonly IInternalComponentContainer _rootContainer;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ComponentContainerBuilder(IInternalComponentContainer rootContainer = null)
        {
            _containerBuilder = new ContainerBuilder();
            _rootContainer = rootContainer;
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

        public void RegisterAdapterPort<TPort>(TPort port)
            where TPort : class, IAdapterInjectionPort
        {
            _containerBuilder.RegisterInstance(port).Keyed<TPort>(port.PortKey).As<TPort, IAdapterInjectionPort>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IComponentRegistrationBuilder RegisterAdapterComponentType<TAdapterInterface, TAdapterConfig>(
            AdapterInjectionPort<TAdapterInterface, TAdapterConfig> adapterInjectionPort, 
            Type adapterComponentType)
            where TAdapterInterface : class
        {
            var registration = _containerBuilder.RegisterType(adapterComponentType)
                .Keyed<TAdapterInterface>(adapterInjectionPort.PortKey)
                .WithParameter(PortConfigParameter.FromPort(adapterInjectionPort));
            
            return new RegistrationBuilderWrapper<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>(registration);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IInternalComponentContainer CreateComponentContainer()
        {
            var underlyingContainer = _containerBuilder.Build();
            var wrappingContainer = new ComponentContainer(underlyingContainer);
            var isRootContainer = (_rootContainer == null);

            if (isRootContainer)
            {
                RegisterSelf(underlyingContainer, wrappingContainer);
            }

            return wrappingContainer;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IInternalComponentContainer RootContainer => _rootContainer;

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

        private class RegistrationBuilderWrapper<TLimit, TActivatorData, TRegistrationStyle> : 
            IComponentRegistrationBuilder, 
            IComponentInstantiationBuilder,
            IComponentConditionBuilder
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

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IComponentInstantiationBuilder WithAdapterParameter<TAdapter, TConfig>(AdapterInjectionPort<TAdapter, TConfig> injectionPort)
                where TAdapter : class
            {
                InnerAsReflectionActivator().WithParameter(PortAdapterParameter.FromPort(injectionPort));
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IComponentInstantiationBuilder WithAdapterConfigurationParameter<TAdapter, TConfig>(AdapterInjectionPort<TAdapter, TConfig> injectionPort)
                where TAdapter : class
            {
                InnerAsReflectionActivator().WithParameter(PortConfigParameter.FromPort(injectionPort));
                return this;
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

            public IComponentConditionBuilder ForService<TService>()
            {
                _inner.As<TService>();
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IComponentConditionBuilder ForServices<TService1, TService2>()
            {
                _inner.As<TService1, TService2>();
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IComponentConditionBuilder ForServices<TService1, TService2, TService3>()
            {
                _inner.As<TService1, TService2, TService3>();
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IComponentConditionBuilder ForServices(params Type[] serviceTypes)
            {
                _inner.As(serviceTypes);
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IComponentConditionBuilder NamedForService<TService>(string name)
            {
                _inner.Named<TService>(name);
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IComponentConditionBuilder NamedForServices<TService1, TService2>(string name)
            {
                _inner.Named<TService1>(name).Named<TService2>(name);
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IComponentConditionBuilder NamedForServices<TService1, TService2, TService3>(string name)
            {
                _inner.Named<TService1>(name).Named<TService2>(name).Named<TService3>(name);
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public IComponentConditionBuilder NamedForServices(string name, params Type[] serviceTypes)
            {
                foreach (var type in serviceTypes)
                {
                    _inner.Named(name, type);
                }
                return this;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void AsFallback()
            {
                ((IRegistrationBuilder<TLimit, TActivatorData, SingleRegistrationStyle>)_inner).PreserveExistingDefaults();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private IRegistrationBuilder<TLimit, ReflectionActivatorData, TRegistrationStyle> InnerAsReflectionActivator()
            {
                return (IRegistrationBuilder<TLimit, ReflectionActivatorData, TRegistrationStyle>)_inner;
            }
        }
    }
}
