using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using NWheels.Microservices.Api;
using NWheels.Kernel.Api.Injection;

namespace NWheels.Microservices.Runtime
{
    public class MicroserviceHostBuilder : IMicroserviceHostBuilder
    {
        private readonly List<Action<IComponentContainer, IComponentContainerBuilder>> _componentContributions;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostBuilder(string name)
        {
            this.BootConfig = new BootConfiguration() {
                MicroserviceConfig = new MicroserviceConfig {
                    Name = name,
                    ApplicationModules = new MicroserviceConfig.ModuleConfig[0],
                    FrameworkModules = new MicroserviceConfig.ModuleConfig[0]
                },
                EnvironmentConfig = new EnvironmentConfig() {
                },
            };

            _componentContributions = new List<Action<IComponentContainer, IComponentContainerBuilder>>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IMicroserviceHostBuilder UseFrameworkFeature<TFeature>()
            where TFeature : IFeatureLoader
        {
            BootConfig.MicroserviceConfig.FrameworkModules = EnsureModuleListed(
                typeof(TFeature),
                BootConfig.MicroserviceConfig.FrameworkModules,
                out MicroserviceConfig.ModuleConfig module);

            EnsureFeatureListed(typeof(TFeature), module);

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IMicroserviceHostBuilder UseApplicationFeature<TFeature>()
            where TFeature : IFeatureLoader
        {
            BootConfig.MicroserviceConfig.ApplicationModules = EnsureModuleListed(
                typeof(TFeature),
                BootConfig.MicroserviceConfig.ApplicationModules,
                out MicroserviceConfig.ModuleConfig module);

            EnsureFeatureListed(typeof(TFeature), module);

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IMicroserviceHostBuilder ContributeComponents(Action<IComponentContainer, IComponentContainerBuilder> contributor)
        {
            _componentContributions.Add(contributor);
            UseApplicationFeature<MicroserviceHostBuilderContributionsFeatureLoader>();
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHost Build()
        {
            return new MicroserviceHost(this.BootConfig, new Mocks.MicroserviceHostLoggerMock(), OnRegisterHostComponents);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BootConfiguration BootConfig { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal void ApplyComponentContributions(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
        {
            foreach (var contributor in _componentContributions)
            {
                contributor(existingComponents, newComponents);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void OnRegisterHostComponents(IComponentContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterComponentInstance<MicroserviceHostBuilder>(this);
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
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class MicroserviceHostBuilderExtensions
    {
        public static MicroserviceHost Build(this IMicroserviceHostBuilder hostBuilder)
        {
            return ((MicroserviceHostBuilder)hostBuilder).Build();
        }
    }
}
