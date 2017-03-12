using System;
using System.Collections.Generic;

namespace NWheels.Injection
{
    public interface IComponentContainer : IDisposable
    {
        TService Resolve<TService>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IEnumerable<TService> ResolveAll<TService>();
    }
}
