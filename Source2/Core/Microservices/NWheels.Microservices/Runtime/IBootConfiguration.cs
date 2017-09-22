using NWheels.Microservices.Api;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System;
using System.Reflection;
using NWheels.Kernel.Api.Primitives;

namespace NWheels.Microservices.Runtime
{
    public interface IBootConfiguration
    {
        void Validate();
        string MicroserviceName { get; }
        bool IsPrecompiledMode { get; }
        bool IsBatchJobMode { get; }
        bool IsClusteredMode { get; }
        IReadOnlyDictionary<string, string> EnvironmentVariables { get; }
        IAssemblyLocationMap AssemblyLocationMap { get; }
        MicroserviceModuleLoaderFactory ModuleLoaderFactory { get; }
        MicroserviceHostLoggerFactory LoggerFactory { get; }
        MicroserviceStateCodeBehindFactory StateCodeBehindFactory { get; }
        IReadOnlyList<IModuleConfiguration> FrameworkModules { get; }
        IReadOnlyList<IModuleConfiguration> ApplicationModules { get; }
        IReadOnlyList<IModuleConfiguration> CustomizationModules { get; }
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

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public delegate IModuleLoader MicroserviceModuleLoaderFactory(MicroserviceHost host);
    public delegate IMicroserviceHostLogger MicroserviceHostLoggerFactory(MicroserviceHost host);
    public delegate IStateMachineCodeBehind<MicroserviceState, MicroserviceTrigger> MicroserviceStateCodeBehindFactory(MicroserviceStateMachineOptions options);
}
