using System;
using FluentAssertions;
using NWheels.Injection;
using NWheels.Microservices;
using Xunit;

namespace NWheels.Implementation.UnitTests.Microservices
{
    public class MicroserviceHostBuilderTests
    {
        [Fact]
        public void UseApplicationFeatureModifiesBootConfigAccordingly()
        {
            //-- arrange

            var microservice = new MicroserviceHostBuilder("test");

            //-- act

            microservice.UseApplicationFeature<TestFeatureLoader>();

            //-- assert

            microservice.BootConfig.MicroserviceConfig.ApplicationModules[0].Features[0].Name
                .Should().Be("TestFeatureLoader");
        }
        
        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestFeatureLoader : IFeatureLoader
        {
            public void CompileComponents(IComponentContainer existingComponents)
            {
                throw new NotImplementedException();
            }

            public void ContributeAdapterComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                throw new NotImplementedException();
            }

            public void ContributeCompiledComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                throw new NotImplementedException();
            }

            public void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                throw new NotImplementedException();
            }

            public void ContributeConfigSections(IComponentContainerBuilder newComponents)
            {
                throw new NotImplementedException();
            }

            public void ContributeConfiguration(IComponentContainer existingComponents)
            {
                throw new NotImplementedException();
            }
        }
    }
}
