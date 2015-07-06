using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NWheels.Hosting;

namespace NWheels.Testing
{
    public static class BootConfigurationExtensions
    {
        public static BootConfiguration AddStackModule(
            this BootConfiguration configuration, 
            string assemblyNameWithoutDll, 
            params string[] featureLoaderClassNames)
        {
            AddModule(configuration.FrameworkModules, assemblyNameWithoutDll, featureLoaderClassNames);
            return configuration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static BootConfiguration AddDomainModule(
            this BootConfiguration configuration,
            string assemblyNameWithoutDll,
            params string[] featureLoaderClassNames)
        {
            AddModule(configuration.ApplicationModules, assemblyNameWithoutDll, featureLoaderClassNames);
            return configuration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static BootConfiguration AddApplicationModule(
            this BootConfiguration configuration,
            string assemblyNameWithoutDll,
            params string[] featureLoaderClassNames)
        {
            AddModule(configuration.ApplicationModules, assemblyNameWithoutDll, featureLoaderClassNames);
            return configuration;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void AddModule(
            List<BootConfiguration.ModuleConfig> destination,
            string assemblyNameWithoutDll,
            string[] featureLoaderClassNames)
        {
            destination.Add(new BootConfiguration.ModuleConfig() {
                Assembly = assemblyNameWithoutDll + ".dll",
                Features = featureLoaderClassNames.Select(className => new BootConfiguration.FeatureConfig() {
                    LoaderClass = className
                }).ToList()
            });
        }
    }
}
