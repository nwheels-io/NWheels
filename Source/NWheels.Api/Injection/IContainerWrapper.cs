using System;
using System.Collections.Generic;

namespace NWheels.Injection
{
    public interface IContainerWrapper : IDisposable
    {
        TInterface Resolve<TInterface>();

        IEnumerable<TInterface> ResolveAll<TInterface>();
    }
}
