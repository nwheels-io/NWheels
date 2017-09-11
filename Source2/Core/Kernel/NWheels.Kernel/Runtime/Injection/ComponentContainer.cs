using Autofac;
using Autofac.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NWheels.Injection;

namespace NWheels.Runtime.Injection
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

        public TService ResolveNamed<TService>(string name)
        {
            return _container.ResolveNamed<TService>(name);
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

        public object ResolveNamed(Type serviceType, string name)
        {
            return _container.ResolveNamed(name, serviceType);
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
                .ComponentRegistry.RegistrationsFor(new TypedService(baseType))
                .SelectMany(r => r.Services)
                .OfType<IServiceWithType>()
                .Select(s => s.ServiceType)
                .Distinct();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Merge(IInternalComponentContainerBuilder containerBuilder)
        {
            var componentContainer = (ComponentContainer)containerBuilder.CreateComponentContainer(isRootContainer: false);

            foreach (var componentRegistration in componentContainer._container.ComponentRegistry.Registrations)
            {
                _container.ComponentRegistry.Register(componentRegistration);
            }
        }
    }
}
