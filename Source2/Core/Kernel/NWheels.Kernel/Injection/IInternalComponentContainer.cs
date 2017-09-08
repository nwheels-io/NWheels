using NWheels.Injection;
using System;
using System.Collections.Generic;

namespace NWheels.Injection
{
    public interface IInternalComponentContainer : IComponentContainer
    {
        void Merge(IInternalComponentContainerBuilder containerBuilder);
    }
}
