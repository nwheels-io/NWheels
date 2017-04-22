using NWheels.Injection;
using NWheels.Microservices;
using NWheels.Testability.Microservices;
using System;
using Xunit;
using System.Collections.Generic;

namespace NWheels.Platform.Messaging.Tests.System
{
    [Trait("Purpose", "System")]
    public class KestrelHttpEndpointTests // : IClassFixture<AspNetKestrelHttpEndpointTests.ClassFixture>
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
                    .UseApplicationFeature<TestKestrelEndpointFeatureLoader>()
                    .Build();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [FeatureLoader(Name = "TestKestrelEndpoint")]
        public class TestKestrelEndpointFeatureLoader : FeatureLoaderBase
        {
            public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                //newComponents.ContributeHttpEndpoint(
                //    "Test", 
                //    new TestConfiguration(), 
                //    handler: context => existingComponents.Resolve<>)
            }

            public override void ContributeAdapterComponents(IComponentContainer input, IComponentContainerBuilder output)
            {
                
            }

            private class TestConfiguration : IHttpEndpointConfiguration
            {
                public int Port => throw new NotImplementedException();

                public IHttpsConfig Https => throw new NotImplementedException();

                public IList<IHttpStaticFolderConfig> StaticFolders => throw new NotImplementedException();

                public string Name => throw new NotImplementedException();
            }

            private class TestHttpsConfig : IHttpsConfig
            {
                public int Port => throw new NotImplementedException();

                public bool RequireHttps => throw new NotImplementedException();

                public string CertFilePath => throw new NotImplementedException();

                public string CertFilePassword => throw new NotImplementedException();
            }

            private class TestHttpStaticFolderConfig : IHttpStaticFolderConfig
            {
                public string RequestBasePath => throw new NotImplementedException();

                public string LocalRootPath => throw new NotImplementedException();

                public IList<string> DefaultDocuments => throw new NotImplementedException();

                public string CacheControl => throw new NotImplementedException();

                public string DefaultContentType => throw new NotImplementedException();

                public bool EnableDirectoryBrowsing => throw new NotImplementedException();
            }
        }
    }
}
