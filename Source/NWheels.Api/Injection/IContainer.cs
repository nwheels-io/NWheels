using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Injection
{
    public interface IContainer
    {
        TInterface Resolve<TInterface>();

        IEnumerable<TInterface> ResolveAll<TInterface>();
    }
}
