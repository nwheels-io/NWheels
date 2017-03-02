using Autofac;
using System;
using System.Collections.Generic;

namespace NWheels.Injection.Adapters.Autofac
{
    public class ContainerWrapper : IContainerWrapper
    {
        IContainer _container;

        public ContainerWrapper(IContainer container)
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
            throw new NotImplementedException();
        }
    }
}
