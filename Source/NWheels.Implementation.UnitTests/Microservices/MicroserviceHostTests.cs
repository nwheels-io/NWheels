using NWheels.Injection;
using NWheels.Injection.Adapters.Autofac;
using NWheels.Microservices;
using Xunit;

namespace NWheels.Implementation.UnitTests.Microservices
{
    public class MicroserviceHostTests
    {
        [Fact]
        public void Configure()
        {
            //-- arrange

            var host = new MicroserviceHost(CreateBootConfiguration());
            AssemblyLoadEventArgs containerEventArgs;
            AssemblyLoadEventArgs featureLoaderEventArgs;
            host.AssemblyLoad += (object sender, AssemblyLoadEventArgs e) =>
            {
                if (e.ImplementedInterface == typeof(IComponentContainerBuilder))
                {
                    e.Destination.Add(typeof(ComponentContainerBuilder));
                    containerEventArgs = e;
                }
                if (e.ImplementedInterface == typeof(IFeatureLoader))
                {
                    e.Destination.Add(typeof(FirstFeatureLoader));
                    e.Destination.Add(typeof(SecondFeatureLoader));
                    e.Destination.Add(typeof(ThirdFeatureLoader));
                    e.Destination.Add(typeof(ForthFeatureLoader));
                    e.Destination.Add(typeof(NamedFeatureLoader));
                    featureLoaderEventArgs = e;
                }
            };

            //-- act

            host.Configure();

            //-- assert
        }

        private BootConfiguration CreateBootConfiguration()
        {
            return new BootConfiguration()
            {
                ConfigsDirectory = "ConfigsDirectory",
                ModulesDirectory = "ModulesDirectory",
                MicroserviceConfig = new MicroserviceConfig()
                {
                    Name = "MicroserviceName",
                    InjectionAdapter = new MicroserviceConfig.InjectionAdapterElement()
                    {
                        Assembly = "InjectionAdapter"
                    },
                    ApplicationModules = new MicroserviceConfig.ModuleConfig[] 
                    {
                        new MicroserviceConfig.ModuleConfig()
                        {
                            Assembly = "ModuleAssembly",
                            Features = new MicroserviceConfig.ModuleConfig.FeatureConfig[]
                            {
                                new MicroserviceConfig.ModuleConfig.FeatureConfig()
                                {
                                    Name = "ThirdFeatureLoader"
                                },
                                new MicroserviceConfig.ModuleConfig.FeatureConfig()
                                {
                                    Name = "NamedLoader"
                                }
                            }
                        }
                    },
                    FrameworkModules = new MicroserviceConfig.ModuleConfig[] { }
                },
                EnvironmentConfig = new EnvironmentConfig()
                {
                    Name = "EnvironmentName",
                    Variables = new EnvironmentConfig.VariableConfig[] { }
                }
            };
        }

        private class TestFeatureLoaderBase : FeatureLoaderBase
        {
            public override void RegisterComponents(IComponentContainerBuilder containerBuilder)
            {
                RegisterComponentsCounter++;
            }

            public override void RegisterConfigSections()
            {
                RegisterConfigSectionsCounter++;
            }

            public int RegisterComponentsCounter { get; private set; }

            public int RegisterConfigSectionsCounter { get; private set; }
        }

        [DefaultFeatureLoader]
        private class FirstFeatureLoader : TestFeatureLoaderBase
        {
        }

        [DefaultFeatureLoader]
        private class SecondFeatureLoader : TestFeatureLoaderBase
        {
        }

        
        private class ThirdFeatureLoader : TestFeatureLoaderBase
        {
        }

        private class ForthFeatureLoader : TestFeatureLoaderBase
        {
        }

        [FeatureLoader(Name = "NamedLoader")]
        private class NamedFeatureLoader : TestFeatureLoaderBase
        {
        }
    }
}
