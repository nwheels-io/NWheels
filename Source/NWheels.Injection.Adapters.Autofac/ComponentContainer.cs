using Autofac;
using Autofac.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWheels.Injection.Adapters.Autofac
{
    public class ComponentContainer : IComponentContainer
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

        public TInterface Resolve<TInterface>()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IEnumerable<TInterface> ResolveAll<TInterface>()
        {
            return _container.Resolve<IEnumerable<TInterface>>();
        }
    }
}
