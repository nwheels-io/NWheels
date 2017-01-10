using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Injection.Mechanism
{
    public interface IComponentContainer
    {
        T Resolve<T>(string key = null);
        IEnumerable<T> ResolveAll<T>(string key = null);
        IComponentPipe<T> ResolvePipe<T>(string key = null);

        object Resolve(Type serviceType, string key = null);
        System.Collections.IEnumerable ResolveAll(Type serviceType, string key = null);
        IComponentPipe<object> ResolvePipe(Type serviceType, string key = null);
    }
}
