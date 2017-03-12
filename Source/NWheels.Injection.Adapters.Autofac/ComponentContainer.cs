using Autofac;
using Autofac.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWheels.Injection.Adapters.Autofac
{
    public class ComponentContainer : IInternalComponentContainer
    {
        IContainer _container;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ComponentContainer(IContainer container)
        {
            _container = container;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TService Resolve<TService>()
        {
            return _container.Resolve<TService>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<TService> ResolveAll<TService>()
        {
            return _container.Resolve<IEnumerable<TService>>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<Type> GetAllServices(Type baseType)
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
            var componentContainer = (ComponentContainer)containerBuilder.CreateComponentContainer();

            foreach (var componentRegistration in componentContainer._container.ComponentRegistry.Registrations)
            {
                _container.ComponentRegistry.Register(componentRegistration);
            }
        }
    }
}
