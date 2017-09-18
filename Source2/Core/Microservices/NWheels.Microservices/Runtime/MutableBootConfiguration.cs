using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NWheels.Kernel.Api.Injection;

namespace NWheels.Microservices.Runtime
{
    public class MutableBootConfiguration : IBootConfiguration
    {
        private readonly List<ModuleConfiguration> _frameworkModules = new List<ModuleConfiguration>();
        private readonly List<ModuleConfiguration> _applicationModules = new List<ModuleConfiguration>();
        private readonly List<ModuleConfiguration> _customizationModules = new List<ModuleConfiguration>();
        private readonly Dictionary<string, string> _environmentVariables = new Dictionary<string, string>();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MutableBootConfiguration()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Validate()
        {
            throw new NotImplementedException();
        }

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
        public IAssemblyLocationMap AssemblyLocationMap { get; set; }

        IReadOnlyList<IModuleConfiguration> IBootConfiguration.FrameworkModules => _frameworkModules;
        IReadOnlyList<IModuleConfiguration> IBootConfiguration.ApplicationModules => _applicationModules;
        IReadOnlyList<IModuleConfiguration> IBootConfiguration.CustomizationModules => _customizationModules;
        IReadOnlyDictionary<string, string> IBootConfiguration.EnvironmentVariables => _environmentVariables;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public List<ModuleConfiguration> FrameworkModules => _frameworkModules;
        public List<ModuleConfiguration> ApplicationModules => _applicationModules;
        public List<ModuleConfiguration> CustomizationModules => _customizationModules;
        public Dictionary<string, string> EnvironmentVariables => _environmentVariables;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static readonly Assembly KernelAssembly = typeof(IFeatureLoader).Assembly;
        public static readonly string KernelAssemblyName = typeof(IFeatureLoader).Assembly.GetName().Name;

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
