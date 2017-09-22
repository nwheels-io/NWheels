using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using NWheels.Kernel.Api.Injection;

namespace NWheels.Microservices.Runtime
{
    public interface IModuleLoader
    {
        IEnumerable<IFeatureLoader> GetBootFeatureLoaders();
        IEnumerable<Type> GetModulePublicTypes(IModuleConfiguration moduleConfig);
    }
}
