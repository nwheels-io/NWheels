using Autofac;
using System;
using System.Collections.Generic;

namespace NWheels.Injection.Adapters.Autofac
{
    public class ComponentContainer : IComponentContainer
    {
        IContainer _container;

        public ComponentContainer(IContainer container)
        {
            _container = container;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public TInterface Resolve<TInterface>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TInterface> ResolveAll<TInterface>()
        {
            return _container.Resolve<IEnumerable<TInterface>>();
        }
    }
}
