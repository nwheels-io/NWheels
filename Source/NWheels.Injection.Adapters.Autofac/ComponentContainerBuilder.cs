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

            public void AsFallback()
            {
                ((IRegistrationBuilder<TLimit, TActivatorData, SingleRegistrationStyle>)_inner).PreserveExistingDefaults();
            }
        }
    }
}
