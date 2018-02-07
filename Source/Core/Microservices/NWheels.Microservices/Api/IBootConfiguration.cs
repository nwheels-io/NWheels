using NWheels.Microservices.Api;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System;
using System.Reflection;
using NWheels.Kernel.Api.Primitives;
using NWheels.Kernel.Api.Logging;
using NWheels.Kernel.Api.Injection;

namespace NWheels.Microservices.Api
{
    public interface IBootConfiguration
    {
        void Validate();
        string MicroserviceName { get; }
        bool IsPrecompiledMode { get; }
        bool IsBatchJobMode { get; }
        bool IsClusteredMode { get; }
        bool IsDebugMode { get; }
        string ClusterName { get; }
        string ClusterPartition { get; }
        LogLevel LogLevel { get; }
        IReadOnlyDictionary<string, string> EnvironmentVariables { get; }
        IAssemblyLocationMap AssemblyLocationMap { get; }
        IBootComponentRegistrations BootComponents { get; }
        IReadOnlyList<IModuleConfiguration> FrameworkModules { get; }
        IReadOnlyList<IModuleConfiguration> ApplicationModules { get; }
        IReadOnlyList<IModuleConfiguration> CustomizationModules { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public interface IBootComponentRegistrations
    {
        void Contribute(IComponentContainerBuilder builder);
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
