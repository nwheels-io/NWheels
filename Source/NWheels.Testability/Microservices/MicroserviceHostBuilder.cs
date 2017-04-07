using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace NWheels.Testability.Microservices
{
    public class MicroserviceHostBuilder
    {
        private readonly MicroserviceConfig _microserviceConfig;
        private readonly EnvironmentConfig _environmentConfig;
        private string _cliDirectory;
        private string _microserviceDirectory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder(string microserviceName)
        {
            _microserviceConfig = new MicroserviceConfig {
                Name = microserviceName,
                ApplicationModules = new MicroserviceConfig.ModuleConfig[0],
                FrameworkModules = new MicroserviceConfig.ModuleConfig[0]
            };

            _environmentConfig = new EnvironmentConfig {
                Name = "TEST",
                Variables = new EnvironmentConfig.VariableConfig[0]
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder UseMicroserviceDirectory(string directoryPath)
        {
            _microserviceDirectory = directoryPath;
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder UseMicroserviceFromSource(string relativeProjectDirectoryPath, [CallerFilePath] string sourceFilePath = "")
        {
            relativeProjectDirectoryPath = relativeProjectDirectoryPath
                .Replace('\\', Path.DirectorySeparatorChar)
                .Replace('/', Path.DirectorySeparatorChar);

            _microserviceDirectory = Path.Combine(Path.GetDirectoryName(sourceFilePath), relativeProjectDirectoryPath);

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder UseCliDirectoryFromSource(
            string relativeProjectDirectoryPath, 
            string cliProjectConfiguration = null, 
            bool allowOverrideByEnvironmentVar = false,
            [CallerFilePath] string sourceFilePath = "")
        {
            if (allowOverrideByEnvironmentVar)
            {
                var environmentValue = GetCliDirectoryFromEnvironment();
                if (!string.IsNullOrEmpty(environmentValue))
                {
                    _cliDirectory = environmentValue;
                    return this;
                }
            }

            var effectiveCliProjectConfiguration = cliProjectConfiguration ?? DefaultProjectConfigurationName;

            relativeProjectDirectoryPath = relativeProjectDirectoryPath
                .Replace('\\', Path.DirectorySeparatorChar)
                .Replace('/', Path.DirectorySeparatorChar);

            _cliDirectory = Path.Combine(
                Path.GetDirectoryName(sourceFilePath),
                relativeProjectDirectoryPath,
                "..",
                $"NWheels.Cli/bin/{effectiveCliProjectConfiguration}/netcoreapp1.1".Replace('/', Path.DirectorySeparatorChar));

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder UseCliDirectoryFromEnvironment()
        {
            _cliDirectory = GetCliDirectoryFromEnvironment();
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder UseAutofacInjectionAdapter()
        {
            _microserviceConfig.InjectionAdapter = new MicroserviceConfig.InjectionAdapterElement {
                Assembly = "NWheels.Injection.Adapters.Autofac"
            };

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder UseApplicationModuleFor<T>()
        {
            _microserviceConfig.ApplicationModules = EnsureModuleListed(
                typeof(T),
                _microserviceConfig.ApplicationModules,
                out MicroserviceConfig.ModuleConfig module);

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder UseApplicationFeature<T>()
            where T : IFeatureLoader
        {
            _microserviceConfig.ApplicationModules = EnsureModuleListed(
                typeof(T),
                _microserviceConfig.ApplicationModules,
                out MicroserviceConfig.ModuleConfig module);

            EnsureFeatureListed(typeof(T), module);

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder UseFrameworkFeature<T>() 
            where T : IFeatureLoader
        {
            _microserviceConfig.FrameworkModules = EnsureModuleListed(
                typeof(T),
                _microserviceConfig.FrameworkModules,
                out MicroserviceConfig.ModuleConfig module);

            EnsureFeatureListed(typeof(T), module);

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder UseFrameworkModuleFor<T>()
        {
            _microserviceConfig.FrameworkModules = EnsureModuleListed(
                typeof(T), 
                _microserviceConfig.FrameworkModules, 
                out MicroserviceConfig.ModuleConfig module);

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostController Build()
        {
            return new MicroserviceHostController(_cliDirectory, _microserviceDirectory, _microserviceConfig, _environmentConfig);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MicroserviceConfig.ModuleConfig[] EnsureModuleListed(
            Type typeFromModuleAssembly,
            MicroserviceConfig.ModuleConfig[] moduleList, 
            out MicroserviceConfig.ModuleConfig module)
        {
            var assemblyName = typeFromModuleAssembly.GetTypeInfo().Assembly.GetName().Name;
            module = moduleList.FirstOrDefault(m => m.Assembly.Equals(assemblyName, StringComparison.OrdinalIgnoreCase));

            if (module != null)
            {
                return moduleList;
            }

            module = new MicroserviceConfig.ModuleConfig {
                Assembly = assemblyName,
                Features = Array.Empty<MicroserviceConfig.ModuleConfig.FeatureConfig>()
            };

            return moduleList.Append(module).ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void EnsureFeatureListed(Type featureLoaderType, MicroserviceConfig.ModuleConfig moduleConfig)
        {
            var namedFeatureAttribute = featureLoaderType.GetTypeInfo().GetCustomAttribute<FeatureLoaderAttribute>();

            if (namedFeatureAttribute != null)
            {
                var feature = moduleConfig.Features.FirstOrDefault(f => f.Name == namedFeatureAttribute.Name);

                if (feature == null)
                {
                    moduleConfig.Features = moduleConfig.Features.Append(new MicroserviceConfig.ModuleConfig.FeatureConfig() {
                        Name = namedFeatureAttribute.Name
                    }).ToArray();
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetCliDirectoryFromEnvironment()
        {
            return Environment.GetEnvironmentVariable("NWHEELS_CLI");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string DefaultProjectConfigurationName =>
#if DEBUG
                "Debug"
#else
                "Release"
#endif
        ;
    }
}
