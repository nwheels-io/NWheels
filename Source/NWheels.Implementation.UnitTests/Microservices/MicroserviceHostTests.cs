using NWheels.Injection;
using NWheels.Microservices;
using System.Collections.Generic;
using Xunit;

namespace NWheels.Implementation.UnitTests.Microservices
{
    public class MicroserviceHostTests
    {
        [Fact]
        public void Configure()
        {
            //-- arrange

            /*var bootConfig = new BootConfiguration()
            {
                MicroserviceConfig = new MicroserviceConfig()
                {
                },
                EnvironmentConfig = new EnvironmentConfig()
                {
                }
            };
            var moduleLoader = new ModuleLoader();
            var host = new MicroserviceHost(new BootConfiguration(), moduleLoader);

            //-- act

            host.Configure();*/

            //-- assert
        }

        private class ModuleLoader : ModuleLoaderBase
        {
            public override List<IFeatureLoader> LoadAllFeatures()
            {
                return new List<IFeatureLoader>()
                {
                    new FirstFeatureLoader(),
                    new SecondFeatureLoader()
                };
            }
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

        private class FirstFeatureLoader : TestFeatureLoaderBase
        {
        }

        private class SecondFeatureLoader : TestFeatureLoaderBase
        {
        }
    }
}
