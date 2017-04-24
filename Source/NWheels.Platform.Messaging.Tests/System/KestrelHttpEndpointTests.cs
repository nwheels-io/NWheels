using NWheels.Injection;
using NWheels.Microservices;
using NWheels.Testability.Microservices;
using NWheels.Extensions;
using System;
using Xunit;
using System.Collections.Generic;
using NWheels.Platform.Messaging.Adapters.AspNetKestrel;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net;
using FluentAssertions;

namespace NWheels.Platform.Messaging.Tests.System
{
    [Trait("Purpose", "SystemTest")]
    public class KestrelHttpEndpointTests : IClassFixture<KestrelHttpEndpointTests.ClassFixture>
    {
        private readonly ClassFixture _fixture;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public KestrelHttpEndpointTests(ClassFixture fixture)
        {
            _fixture = fixture;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanServeStaticContent()
        {
            //-- arrange

            _fixture.Microservice.Start();

            //-- act



        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //private void MakeHttpRequest(
        //    string path, 
        //    HttpStatusCode expectedStatusCode, 
        //    string expectedContentType, 
        //    string expectedContentFile)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        var httpTask = client.GetAsync("http://localhost:5500/" + path, HttpCompletionOption.ResponseContentRead);
        //        var completed = httpTask.Wait(10000);
        //        Assert.True(completed, "HTTP request didn't complete within allotted timeout.");

        //        var response = httpTask.Result;
        //        response.StatusCode.Should().Be(expectedStatusCode);
        //        var responseText = response.Content.ReadAsStringAsync().Result;
        //        responseText.Should().Be("this-is-a-test");
        //    }
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class NonStaticTestRequestHandler
        {
            public async Task HandleRequest(HttpContext context)
            {

            }
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
                    .UseFrameworkFeature<KestrelFeatureLoader>()
                    .UseApplicationFeature<TestKestrelEndpointFeatureLoader>()
                    .Build();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                _microservice.StopOrThrow(10000);
                _microservice.AssertNoErrors();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public MicroserviceHostController Microservice => _microservice;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [FeatureLoader(Name = "TestKestrelEndpoint")]
        public class TestKestrelEndpointFeatureLoader : FeatureLoaderBase
        {
            public override void ContributeConfigSections(IComponentContainerBuilder newComponents)
            {
                var binaryFolder = Path.GetDirectoryName(this.GetType().GetTypeInfo().Assembly.Location);

                var httpConfig = new TestHttpEndpointConfiguration() {
                    Name = "Test",
                    Port = 5500,
                    StaticFolders = new IHttpStaticFolderConfig[] {
                        new TestHttpStaticFolderConfig {
                            LocalRootPath = Path.Combine(binaryFolder, "System/wwwroot/Static1".ToPathString()),
                            RequestBasePath = "/static/files/one"
                        },
                        new TestHttpStaticFolderConfig {
                            LocalRootPath = Path.Combine(binaryFolder, "System/wwwroot/Static2".ToPathString()),
                            RequestBasePath = "/static/files/two",
                            DefaultDocuments = new string[] {
                                "index.html"
                            }
                        }
                    }
                };

                newComponents.RegisterComponentInstance(httpConfig).ForService<IHttpEndpointConfiguration>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                newComponents.RegisterComponentType<NonStaticTestRequestHandler>().InstancePerDependency();

                var httpConfig = existingComponents.Resolve<IHttpEndpointConfiguration>();

                newComponents.ContributeHttpEndpoint(
                    "Test",
                    httpConfig,
                    handler: context => {
                        var handler = existingComponents.Resolve<NonStaticTestRequestHandler>();
                        return handler.HandleRequest(context);
                    });
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestHttpEndpointConfiguration : IHttpEndpointConfiguration
        {
            public TestHttpEndpointConfiguration()
            {
                this.StaticFolders = new List<IHttpStaticFolderConfig>();
            }

            public int Port { get; set; }

            public IHttpsConfig Https { get; set; }

            public IList<IHttpStaticFolderConfig> StaticFolders { get; set; }

            public string Name { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestHttpsConfig : IHttpsConfig
        {
            public int Port { get; set; }

            public bool RequireHttps { get; set; }

            public string CertFilePath { get; set; }

            public string CertFilePassword { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestHttpStaticFolderConfig : IHttpStaticFolderConfig
        {
            public TestHttpStaticFolderConfig()
            {
                this.DefaultDocuments = new List<string>();
            }

            public string RequestBasePath { get; set; }

            public string LocalRootPath { get; set; }

            public IList<string> DefaultDocuments { get; set; }

            public string CacheControl { get; set; }

            public string DefaultContentType { get; set; }

            public bool EnableDirectoryBrowsing { get; set; }
        }
    }
}
