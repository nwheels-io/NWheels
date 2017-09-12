using System;
using System.Collections.Generic;

namespace NWheels.Kernel.Api.Injection
{
    public interface IInternalComponentContainer : IComponentContainer
    {
        void Merge(IInternalComponentContainerBuilder containerBuilder);
    }
}
