using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NWheels.Kernel.Api.Injection;
using NWheels.Kernel.Api.Logging;
using NWheels.Kernel.Api.Primitives;

namespace NWheels.Microservices.Runtime
{
    public class MutableBootConfiguration : IBootConfiguration
    {
        private readonly List<ModuleConfiguration> _frameworkModules = new List<ModuleConfiguration>();
        private readonly List<ModuleConfiguration> _applicationModules = new List<ModuleConfiguration>();
        private readonly List<ModuleConfiguration> _customizationModules = new List<ModuleConfiguration>();
        private readonly Dictionary<string, string> _environmentVariables = new Dictionary<string, string>();
        private readonly HostComponentsRegistration _hostComponents = new HostComponentsRegistration();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MutableBootConfiguration()
        {

        } 

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Validate()
        {
            throw new NotImplementedException();
        }

        //TODO: move to MutableBootConfiguration.Validate
        //private IAssemblyLocationMap GetAssemblyLocationMap()
        //{
        //    if (_bootConfig.AssemblyLocationMap != null)
        //    {
        //        return _bootConfig.AssemblyLocationMap;
        //    }

        //    var defaultMap = new AssemblyLocationMap();
        //    //defaultMap.AddDirectory(_bootConfig.ConfigsDirectory);
        //    defaultMap.AddDirectory(AppContext.BaseDirectory);
        //    return defaultMap;
        //}


        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddFeatures(List<ModuleConfiguration> moduleList, string moduleAssemblyName, params string[] featureNames)
        {
            AddFeatures(moduleList, moduleAssemblyName, (IEnumerable<string>)featureNames);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddFeatures(List<ModuleConfiguration> moduleList, string moduleAssemblyName, IEnumerable<string> featureNames)
        {
            var moduleItem = moduleList.FirstOrDefault(m => m.ModuleName == moduleAssemblyName);

            if (moduleItem == null)
            {
                moduleItem = new ModuleConfiguration(moduleAssemblyName);
                moduleList.Add(moduleItem);
            }

            foreach (var featureName in featureNames)
            {
                if (!moduleItem.Features.Any(f => f.FeatureName == featureName))
                {
                    moduleItem.Features.Add(new FeatureConfiguration(featureName));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AddFeatures(List<ModuleConfiguration> moduleList, Assembly moduleAssembly, params Type[] featureLoaderTypes)
        {
            var moduleItem = moduleList.FirstOrDefault(m => m.RuntimeAssembly == moduleAssembly);

            if (moduleItem == null)
            {
                moduleItem = new ModuleConfiguration(moduleAssembly);
                moduleList.Add(moduleItem);
            }

            foreach (var loaderType in featureLoaderTypes)
            {
                var featureName = FeatureLoaderAttribute.GetFeatureNameOrThrow(loaderType);

                if (!moduleItem.Features.Any(f => f.FeatureLoaderRuntimeType == loaderType || f.FeatureName == featureName))
                {
                    moduleItem.Features.Add(new FeatureConfiguration(featureName));
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string MicroserviceName { get; set; }
        public bool IsPrecompiledMode { get; set; }
        public bool IsBatchJobMode { get; set; }
        public string ClusterName { get; }
        public string ClusterPartition { get; }
        public LogLevel LogLevel { get; set; }
        public IAssemblyLocationMap AssemblyLocationMap { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool IsDebugMode => (this.LogLevel == LogLevel.Debug);
        public bool IsClusteredMode => !string.IsNullOrEmpty(this.ClusterName);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IReadOnlyList<IModuleConfiguration> IBootConfiguration.FrameworkModules => _frameworkModules;
        IReadOnlyList<IModuleConfiguration> IBootConfiguration.ApplicationModules => _applicationModules;
        IReadOnlyList<IModuleConfiguration> IBootConfiguration.CustomizationModules => _customizationModules;
        IReadOnlyDictionary<string, string> IBootConfiguration.EnvironmentVariables => _environmentVariables;
        IHostComponentsRegistration IBootConfiguration.HostComponents => _hostComponents;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<ModuleConfiguration> FrameworkModules => _frameworkModules;
        public List<ModuleConfiguration> ApplicationModules => _applicationModules;
        public List<ModuleConfiguration> CustomizationModules => _customizationModules;
        public Dictionary<string, string> EnvironmentVariables => _environmentVariables;
        public HostComponentsRegistration HostComponents => _hostComponents;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static readonly Assembly KernelAssembly = typeof(IFeatureLoader).Assembly;
        public static readonly string KernelAssemblyName = typeof(IFeatureLoader).Assembly.GetName().Name;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class HostComponentsRegistration : IHostComponentsRegistration
        {
            private readonly List<Action<IComponentContainerBuilder>> _registrations = new List<Action<IComponentContainerBuilder>>();

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            void IHostComponentsRegistration.Contribute(IComponentContainerBuilder builder)
            {
                foreach (var registration in _registrations)
                {
                    registration(builder);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Register(Action<IComponentContainerBuilder> registration)
            {
                _registrations.Add(registration);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ModuleConfiguration : IModuleConfiguration
        {
            private readonly List<FeatureConfiguration> _features = new List<FeatureConfiguration>();

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ModuleConfiguration(string assemblyName)
            {
                this.AssemblyName = assemblyName;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ModuleConfiguration(Assembly runtimeAssembly)
            {
                this.RuntimeAssembly = runtimeAssembly;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string ModuleName
            {
                get
                {
                    return (RuntimeAssembly != null ? RuntimeAssembly.GetName().Name : AssemblyName);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsKernelModule
            {
                get
                {
                    return (this.RuntimeAssembly == KernelAssembly || this.AssemblyName == KernelAssemblyName);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string AssemblyName { get; }
            public Assembly RuntimeAssembly { get; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public List<FeatureConfiguration> Features => _features;
            IReadOnlyList<IFeatureConfiguration> IModuleConfiguration.Features => _features;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public class FeatureConfiguration : IFeatureConfiguration
        {
            public FeatureConfiguration(string featureName)
            {
                this.FeatureName = featureName;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public FeatureConfiguration(Type featureLoaderType)
            {
                this.FeatureLoaderRuntimeType = featureLoaderType;
                this.FeatureName = FeatureLoaderAttribute.GetFeatureNameOrThrow(featureLoaderType);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string FeatureName { get; }
            public Type FeatureLoaderRuntimeType { get; }
        }
    }
}
