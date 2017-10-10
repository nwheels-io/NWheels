using System;
using System.Collections.Generic;
using System.Text;
using NWheels.Kernel.Api.Execution;
using NWheels.Kernel.Api.Injection;

namespace NWheels.Kernel.Api.Extensions
{
    public static class ComponentContainerExtensions
    {
        public static IInternalComponentContainer AsInternal(this IComponentContainer container)
        {
            return (IInternalComponentContainer)container;
        }
    }
}
