using NWheels.Injection;
using NWheels.Microservices;
using NWheels.Testability.Microservices;
using System;
using Xunit;

namespace NWheels.Platform.Messaging.Tests.System
{
    [Trait("Purpose", "System")]
    public class AspNetKestrelHttpEndpointTests // : IClassFixture<AspNetKestrelHttpEndpointTests.ClassFixture>
    {
        //[Fact]
        public void BasicHttpServerTest()
        {

        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ClassFixture : IDisposable
        {
            private readonly MicroserviceHostController _microservice;
            
            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ClassFixture()
            {
                _microservice = new MicroserviceHostBuilder(microserviceName: "AspNetKestrelTest")
                    .UseCliDirectoryFromSource(relativeProjectDirectoryPath: "..", allowOverrideByEnvironmentVar: true)
                    .UseMicroserviceFromSource(relativeProjectDirectoryPath: "..")
                    .UseAutofacInjectionAdapter()
                    .UseApplicationFeature<TestEndpointFeatureLoader>()
                    .Build();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [FeatureLoader(Name = "AspNetKestrelTestEndpoint")]
        public class TestEndpointFeatureLoader : FeatureLoaderBase
        {
            public override void ContributeComponents(IComponentContainerBuilder containerBuilder)
            {
                throw new NotImplementedException();
                //containerBuilder.ContributeLifecycleListener<MicroserviceHostSmokeTest.SmokeTestComponent>();
            }
        }
    }
}
