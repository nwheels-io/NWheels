using Autofac;
using Autofac.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NWheels.Kernel.Api.Injection;
using System.Reflection;

namespace NWheels.Kernel.Runtime.Injection
{
    public class ComponentContainer : IInternalComponentContainer
    {
        private readonly IContainer _container;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ComponentContainer(IContainer container)
        {
            _container = container;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            _container.Dispose();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryResolve<TService>(out TService instance)
        {
            return _container.TryResolve<TService>(out instance);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TService Resolve<TService>()
        {
            return _container.Resolve<TService>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TService ResolveWithArguments<TService, TArg1>(TArg1 arg1)
        {
            return _container.Resolve<TService>(TypedParameter.From(arg1));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TService ResolveWithArguments<TService, TArg1, TArg2>(TArg1 arg1, TArg2 arg2)
        {
            return _container.Resolve<TService>(TypedParameter.From(arg1), TypedParameter.From(arg2));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TService ResolveWithArguments<TService>(params object[] arguments)
        {
            return _container.Resolve<TService>(MakeTypedParameters(arguments));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TService ResolveNamed<TService>(string name)
        {
            return _container.ResolveNamed<TService>(name);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TService ResolveNamedWithArguments<TService, TArg1>(string name, TArg1 arg1)
        {
            return _container.ResolveNamed<TService>(name, TypedParameter.From(arg1));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TService ResolveNamedWithArguments<TService, TArg1, TArg2>(string name, TArg1 arg1, TArg2 arg2)
        {
            return _container.ResolveNamed<TService>(name, TypedParameter.From(arg1), TypedParameter.From(arg2));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TService ResolveNamedWithArguments<TService>(string name, params object[] arguments)
        {
            return _container.ResolveNamed<TService>(name, MakeTypedParameters(arguments));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<TService> ResolveAll<TService>()
        {
            return _container.Resolve<IEnumerable<TService>>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryResolve(Type serviceType, out object instance)
        {
            return _container.TryResolve(serviceType, out instance);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object Resolve(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object ResolveWithArguments(Type serviceType, params object[] arguments)
        {
            return _container.Resolve(serviceType, MakeTypedParameters(arguments));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object ResolveNamed(Type serviceType, string name)
        {
            return _container.ResolveNamed(name, serviceType);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public object ResolveNamedWithArguments(Type serviceType, string name, params object[] arguments)
        {
            return _container.ResolveNamed(name, serviceType, MakeTypedParameters(arguments));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<object> ResolveAll(Type serviceType)
        {
            var resolved = _container.Resolve(typeof(IEnumerable<>).MakeGenericType(serviceType));
            return ((IEnumerable)resolved).Cast<object>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<Type> GetAllServiceTypes(Type baseType)
        {
            return _container
                .ComponentRegistry
                .Registrations
                .SelectMany(r => r.Services)
                .OfType<IServiceWithType>()
                .Select(s => s.ServiceType)
                .Where(type => baseType.IsAssignableFrom(type))
                .Distinct();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Merge(IInternalComponentContainerBuilder containerBuilder)
        {
            var componentContainer = (ComponentContainer)containerBuilder.CreateComponentContainer();

            foreach (var componentRegistration in componentContainer._container.ComponentRegistry.Registrations)
            {
                _container.ComponentRegistry.Register(componentRegistration);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TAdapterInterface ResolveAdapter<TAdapterInterface, TAdapterConfig>(AdapterInjectionPort<TAdapterInterface, TAdapterConfig> port)
            where TAdapterInterface : class
        {
            var adapterInstance = (TAdapterInterface)_container.ResolveKeyed(
                port.PortKey, 
                typeof(TAdapterInterface));

            return adapterInstance;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        private IEnumerable<TypedParameter> MakeTypedParameters(IEnumerable<object> arguments)
        {
            return arguments.Select(arg => new TypedParameter(
                type: arg?.GetType() ?? typeof(Object), 
                value: arg
            ));
        }
    }
}
