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
using System.Linq;

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
        public void CanServeStaticFile()
        {
            //-- act

            var response = MakeHttpRequest(5500, HttpMethod.Get, "/static/files/one/test.js");

            //-- assert

            AssertHttpResponse(response, HttpStatusCode.OK, "application/javascript", "System/wwwroot/Static1/test.js");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanServeStaticFilesFromMultipleFolders()
        {
            //-- act

            var response1 = MakeHttpRequest(5500, HttpMethod.Get, "/static/files/one/test.js");
            var response2 = MakeHttpRequest(5500, HttpMethod.Get, "/static/files/two/index.html");

            //-- assert

            AssertHttpResponse(response1, HttpStatusCode.OK, "application/javascript", "System/wwwroot/Static1/test.js");
            AssertHttpResponse(response2, HttpStatusCode.OK, "text/html", "System/wwwroot/Static2/index.html");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanServeStaticFileAsFolderDefault()
        {
            //-- act

            var response = MakeHttpRequest(5500, HttpMethod.Get, "/static/files/two/"); // should serve /static/files/two/index.html

            //-- assert

            AssertHttpResponse(
                response, 
                HttpStatusCode.OK, 
                expectedContentType: "text/html", 
                expectedContentFilePath: "System/wwwroot/Static2/index.html");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanRedirectToStaticFileAsFolderDefault()
        {
            //-- act

            var response = MakeHttpRequest(5500, HttpMethod.Get, "/static/files/two"); // should redirect to /static/files/two/

            //-- assert

            AssertHttpResponse(
                response,
                HttpStatusCode.OK,
                expectedContentType: "text/html",
                expectedContentFilePath: "System/wwwroot/Static2/index.html");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanServeStaticDefaultFile()
        {
            //-- act

            var response1 = MakeHttpRequest(5500, HttpMethod.Get, "/static/files/one/test.js");
            var response2 = MakeHttpRequest(5500, HttpMethod.Get, "/static/files/two/");
            var response3 = MakeHttpRequest(5500, HttpMethod.Get, "/static/files/two/non-existent-resource.xyz");

            //-- assert

            AssertHttpResponse(response1, HttpStatusCode.OK, "application/javascript", "System/wwwroot/Static1/test.js");
            AssertHttpResponse(response2, HttpStatusCode.OK, "text/html", "System/wwwroot/Static2/index.html");
            AssertHttpResponse(response3, HttpStatusCode.NotFound, null, null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanHandleNonStaticRequest()
        {
            //-- act

            var response = MakeHttpRequest(5501, HttpMethod.Get, "/my/dynamic/path");

            //-- assert

            AssertHttpResponse(
                response, 
                expectedStatusCode: HttpStatusCode.OK, 
                expectedContentType: "application/json", 
                expectedContentFilePath: null, 
                expectedContentAsString: "{my:1,dynamic:2,path:3}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanServeStaticFileInMixedEndpoint()
        {
            //-- act

            var response = MakeHttpRequest(5502, HttpMethod.Get, "/static/files/one/test.js");

            //-- assert

            AssertHttpResponse(response, HttpStatusCode.OK, "application/javascript", "System/wwwroot/Static1/test.js");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanHandleNonStaticRequestInMixedEndpoint()
        {
            //-- act

            var response = MakeHttpRequest(5502, HttpMethod.Get, "/my/dynamic/path");

            //-- assert

            AssertHttpResponse(
                response,
                expectedStatusCode: HttpStatusCode.OK,
                expectedContentType: "application/json",
                expectedContentFilePath: null,
                expectedContentAsString: "{my:1,dynamic:2,path:3}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private HttpResponseMessage MakeHttpRequest(int endpointPort, HttpMethod method, string path)
        {
            using (var client = new HttpClient())
            {
                var requestUri = $"http://localhost:{endpointPort}/{path.TrimStart('/')}";
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
            string expectedContentFilePath,
            string expectedContentAsString = null)
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

            if (expectedContentAsString != null)
            {
                var actualContentAsString = response.Content.ReadAsStringAsync().Result;
                actualContentAsString.Should().Be(expectedContentAsString, "content as string");
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
                await Task.Yield(); // simulate async execution

                var parts = context.Request.Path.Value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                var properties = string.Join(",", parts.Select((p, index) => $"{p}:{index + 1}"));

                context.Response.ContentType = "application/json";

                using (var writer = new StreamWriter(context.Response.Body))
                {
                    writer.Write("{" + properties + "}");
                    writer.Flush();
                }
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
                    .UseApplicationFeature<TestFeatureLoader>()
                    .Build();

                _microservice.Start();
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

        [FeatureLoader(Name = "Test")]
        public class TestFeatureLoader : FeatureLoaderBase
        {
            public override void ContributeConfigSections(IComponentContainerBuilder newComponents)
            {
                newComponents.RegisterComponentInstance(new TestMessagingPlatformConfiguration()).ForService<IMessagingPlatformConfiguration>();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void ContributeConfiguration(IComponentContainer existingComponents)
            {
                base.ContributeConfiguration(existingComponents);

                var configuration = existingComponents.Resolve<IMessagingPlatformConfiguration>();

                configuration.Endpoints["Static"] = new TestHttpEndpointConfiguration() {
                    Name = "Static",
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
                configuration.Endpoints["Dynamic"] = new TestHttpEndpointConfiguration() {
                    Name = "Dynamic",
                    Port = 5501
                };
                configuration.Endpoints["Mixed"] = new TestHttpEndpointConfiguration() {
                    Name = "Mixed",
                    Port = 5502,
                    StaticFolders = new IHttpStaticFolderConfig[] {
                        new TestHttpStaticFolderConfig {
                            LocalRootPath = Path.Combine(_s_binaryFolder, "System/wwwroot/Static1".ToPathString()),
                            RequestBasePath = "/static/files/one"
                        }
                    }
                };
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override void ContributeComponents(IComponentContainer existingComponents, IComponentContainerBuilder newComponents)
            {
                newComponents.RegisterComponentType<NonStaticTestRequestHandler>().InstancePerDependency();

                newComponents.ContributeHttpEndpoint(
                    "Static",
                    handler: null);

                newComponents.ContributeHttpEndpoint(
                    "Dynamic",
                    handler: context => {
                        var handler = existingComponents.Resolve<NonStaticTestRequestHandler>();
                        return handler.HandleRequest(context);
                    });

                newComponents.ContributeHttpEndpoint(
                    "Mixed",
                    handler: context => {
                        var handler = existingComponents.Resolve<NonStaticTestRequestHandler>();
                        return handler.HandleRequest(context);
                    });
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestMessagingPlatformConfiguration : IMessagingPlatformConfiguration
        {
            public TestMessagingPlatformConfiguration()
            {
                this.Endpoints = new Dictionary<string, IEndpointConfig>();
            }

            public IDictionary<string, IEndpointConfig> Endpoints { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TestHttpEndpointConfiguration : IHttpEndpointConfig
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
