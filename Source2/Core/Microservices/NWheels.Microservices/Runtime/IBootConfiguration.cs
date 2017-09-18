using NWheels.Microservices.Api;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System;
using System.Reflection;

namespace NWheels.Microservices.Runtime
{
    public interface IBootConfiguration
    {
        void Validate();
        string MicroserviceName { get; }
        IAssemblyLocationMap AssemblyLocationMap { get; }
        IReadOnlyList<IModuleConfiguration> FrameworkModules { get; }
        IReadOnlyList<IModuleConfiguration> ApplicationModules { get; }
        IReadOnlyList<IModuleConfiguration> CustomizationModules { get; }
        IReadOnlyDictionary<string, string> EnvironmentVariables { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IModuleConfiguration
    {
        string ModuleName { get; }
        string AssemblyName { get; }
        Assembly RuntimeAssembly { get; }
        IReadOnlyList<IFeatureConfiguration> Features { get; }
        bool IsKernelModule { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IFeatureConfiguration
    {
        string FeatureName { get; }
        Type FeatureLoaderRuntimeType { get; }
    }
}
