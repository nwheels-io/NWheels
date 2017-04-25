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
using System.Threading;

namespace NWheels.Platform.Messaging.Tests.System
{
    [Trait("Purpose", "SystemTest")]
    public class KestrelHttpEndpointTests : IClassFixture<KestrelHttpEndpointTests.ClassFixture>
    {
        private static readonly string _s_binaryFolder = 
            Path.GetDirectoryName(typeof(KestrelHttpEndpointTests).GetTypeInfo().Assembly.Location);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

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

            var response1 = MakeHttpRequest(HttpMethod.Get, "/static/files/one/test.js");
            var response2 = MakeHttpRequest(HttpMethod.Get, "/static/files/two/");
            var response3 = MakeHttpRequest(HttpMethod.Get, "/static/files/two/non-existent-resource.xyz");

            //-- assert

            AssertHttpResponse(response1, HttpStatusCode.OK, "application/javascript", "System/wwwroot/Static1/test.js");
            AssertHttpResponse(response2, HttpStatusCode.OK, "text/html", "System/wwwroot/Static2/index.html");
            AssertHttpResponse(response3, HttpStatusCode.NotFound, null, null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private HttpResponseMessage MakeHttpRequest(HttpMethod method, string path)
        {
            using (var client = new HttpClient())
            {
                var requestUri = "http://localhost:5500/" + path.TrimStart('/');
                var httpTask = client.SendAsync(new HttpRequestMessage(method, requestUri), HttpCompletionOption.ResponseContentRead, CancellationToken.None);
                var completed = httpTask.Wait(10000);

                Assert.True(completed, "HTTP request didn't complete within allotted timeout.");

                var response = httpTask.Result;
                return response;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AssertHttpResponse(
            HttpResponseMessage response,
            HttpStatusCode expectedStatusCode,
            string expectedContentType,
            string expectedContentFilePath)
        {
            response.StatusCode.Should().Be(expectedStatusCode, "HTTP status code");

            if (expectedContentType != null)
            {
                response.Content?.Headers?.ContentType?.ToString().Should().Be(expectedContentType, "content type");
            }

            if (expectedContentFilePath != null)
            {
                AssertResponseContentsEqual(response, expectedContentFilePath);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AssertResponseContentsEqual(HttpResponseMessage response, string expectedContentFilePath)
        {
            var actualContents = response.Content.ReadAsStreamAsync().Result;
            var expectedFileFullPath = Path.Combine(_s_binaryFolder, expectedContentFilePath.ToPathString());

            using (var expectedContents = File.OpenRead(expectedFileFullPath))
            {
                AssertStreamsEqual(actualContents, expectedContents);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AssertStreamsEqual(Stream actual, Stream expected)
        {
            var bufferActual = new byte[1024];
            var bufferExpected = new byte[1024];

            int lengthActual;
            int lengthExpected;

            int offset = 0;

            while (true)
            {
                lengthActual = actual.Read(bufferActual, 0, bufferActual.Length);
                lengthExpected = expected.Read(bufferExpected, 0, bufferExpected.Length);

                (offset + lengthActual).Should().Be(offset + lengthExpected, "size of the stream");

                if (lengthActual == 0)
                {
                    break;
                }

                bufferActual.Should().Equal(bufferExpected, $"contents, {lengthActual} bytes at offset {offset}");

                offset += lengthActual;
            }
        }

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
                var httpConfig = new TestHttpEndpointConfiguration() {
                    Name = "Test",
                    Port = 5500,
                    StaticFolders = new IHttpStaticFolderConfig[] {
                        new TestHttpStaticFolderConfig {
                            LocalRootPath = Path.Combine(_s_binaryFolder, "System/wwwroot/Static1".ToPathString()),
                            RequestBasePath = "/static/files/one"
                        },
                        new TestHttpStaticFolderConfig {
                            LocalRootPath = Path.Combine(_s_binaryFolder, "System/wwwroot/Static2".ToPathString()),
                            RequestBasePath = "/static/files/two",
                            DefaultFiles = new string[] {
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
                    handler: null /*context => {
                        var handler = existingComponents.Resolve<NonStaticTestRequestHandler>();
                        return handler.HandleRequest(context);
                    }*/);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestHttpEndpointConfiguration : IHttpEndpointConfiguration
        {
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
            public string RequestBasePath { get; set; }

            public string LocalRootPath { get; set; }

            public IList<string> DefaultFiles { get; set; }

            public string CacheControl { get; set; }

            public string DefaultContentType { get; set; }

            public bool EnableDirectoryBrowsing { get; set; }
        }
    }
}
