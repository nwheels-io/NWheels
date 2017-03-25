using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NWheels.Testability.Microservices
{
    public class MicroserviceHostBuilder
    {
        private readonly MicroserviceConfig _microserviceConfig;
        private readonly EnvironmentConfig _environmentConfig;
        private string _workingDirectory;

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

        public MicroserviceHostBuilder UseWorkingDirectory(string directoryPath)
        {
            if (Path.IsPathRooted(directoryPath))
            {
                _workingDirectory = directoryPath;
            }
            else
            {
                _workingDirectory = Path.Combine(
                    Path.GetDirectoryName(this.GetType().GetTypeInfo().Assembly.Location),
                    "system_test",
                    directoryPath);
            }

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

        public MicroserviceHostBuilder AddApplicationModuleOf<T>()
        {
            var moduleAssemblyName = typeof(T).GetTypeInfo().Assembly.GetName().Name;

            if (!_microserviceConfig.ApplicationModules.Any(m => m.Assembly.Equals(moduleAssemblyName, StringComparison.OrdinalIgnoreCase)))
            {
                var moduleConfig = new MicroserviceConfig.ModuleConfig {
                    Assembly = moduleAssemblyName
                };

                _microserviceConfig.ApplicationModules = _microserviceConfig.ApplicationModules.Append(moduleConfig).ToArray();
            }

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostController GetHostController()
        {
            return new MicroserviceHostController(_workingDirectory, _microserviceConfig, _environmentConfig);
        }
    }
}
