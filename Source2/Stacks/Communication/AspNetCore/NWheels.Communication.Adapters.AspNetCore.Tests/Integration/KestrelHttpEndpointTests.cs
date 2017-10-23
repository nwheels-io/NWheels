using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using NWheels.Communication.Adapters.AspNetCore.Api;
using NWheels.Kernel.Api.Injection;
using NWheels.Kernel.Api.Extensions;
using NWheels.Microservices.Api;
using NWheels.Microservices.Runtime;
using NWheels.Testability;
using NWheels.Communication.Api.Extensions;
using Xunit;
using FluentAssertions;
using NWheels.Communication.Api.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NWheels.Communication.Api;
using System.Linq;

namespace NWheels.Communication.Adapters.AspNetCore.Tests.Integration
{
    public class KestrelHttpEndpointTests : TestBase.UnitTest
    {
        [Fact]
        public void CanServeStaticFile()
        {
            //-- arrange

            Action<MicroserviceHostBuilder> testCaseFeatures = (hostBuilder) =>  {
                hostBuilder.UseHttpEndpoint(configure: endpoint => endpoint
                    .ListenOnPort(5000)
                    .StaticFolder("/static/files/one", localPath: new[] { TestFilesFolderPath, "wwwroot/Static1" }));
            };

            //-- act

            HttpResponseMessage response;

            using (var microservice = StartTestMicroservice(testCaseFeatures))
            {
                response = MakeHttpRequest(5000, HttpMethod.Get, "/static/files/one/test.js");
            }

            //-- assert

            AssertHttpResponse(response, HttpStatusCode.OK, "application/javascript", "wwwroot/Static1/test.js");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanServeStaticFilesFromMultipleFolders()
        {
            //-- arrange

            Action<MicroserviceHostBuilder> testCaseFeatures = (hostBuilder) =>  {
                hostBuilder.UseHttpEndpoint(configure: endpoint => endpoint
                    .ListenOnPort(5000)
                    .StaticFolder("/static/files/one", localPath: new[] { TestFilesFolderPath, "wwwroot/Static1" })
                    .StaticFolder("/static/files/two", localPath: new[] { TestFilesFolderPath, "wwwroot/Static2" }));
            };

            //-- act

            HttpResponseMessage response1, response2;

            using (var microservice = StartTestMicroservice(testCaseFeatures))
            {
                response1 = MakeHttpRequest(5000, HttpMethod.Get, "/static/files/one/test.js");
                response2 = MakeHttpRequest(5000, HttpMethod.Get, "/static/files/two/index.html");
            }

            //-- assert

            AssertHttpResponse(response1, HttpStatusCode.OK, "application/javascript", "wwwroot/Static1/test.js");
            AssertHttpResponse(response2, HttpStatusCode.OK, "text/html", "wwwroot/Static2/index.html");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanServeStaticFileAsFolderDefault()
        {
            //-- arrange

            Action<MicroserviceHostBuilder> testCaseFeatures = (hostBuilder) =>  {
                hostBuilder.UseHttpEndpoint(configure: endpoint => endpoint
                    .ListenOnPort(5000)
                    .StaticFolder(
                        "/static", 
                        localPath: new[] { TestFilesFolderPath, "wwwroot/Static2" },
                        defaultFiles: new[] { "index.html" }));
            };

            //-- act

            HttpResponseMessage response1, response2;

            using (var microservice = StartTestMicroservice(testCaseFeatures))
            {
                response1 = MakeHttpRequest(5000, HttpMethod.Get, "/static/");
                response2 = MakeHttpRequest(5000, HttpMethod.Get, "/static");
            }

            //-- assert

            AssertHttpResponse(response1, HttpStatusCode.OK, "text/html", expectedContentFilePath: "wwwroot/Static2/index.html");
            AssertHttpResponse(response2, HttpStatusCode.OK, "text/html", expectedContentFilePath: "wwwroot/Static2/index.html");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanServeDynamicContent()
        {
            //-- arrange

            Action<MicroserviceHostBuilder> testCaseFeatures = (hostBuilder) =>  {
                hostBuilder.UseHttpEndpoint(configure: endpoint => endpoint
                    .ListenOnPort(5000)
                    .Middleware<TestDynamicContentMiddleware>()
                );
            };

            //-- act

            HttpResponseMessage response;

            using (var microservice = StartTestMicroservice(testCaseFeatures))
            {
                response = MakeHttpRequest(5000, HttpMethod.Get, "/my/dynamic/path");
            }

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
        public void CanMixStaticAndDynamicContent()
        {
            //-- arrange

            Action<MicroserviceHostBuilder> testCaseFeatures = (hostBuilder) =>  {
                hostBuilder.UseHttpEndpoint(configure: endpoint => endpoint
                    .ListenOnPort(5000)
                    .StaticFolder("/static/one", localPath: new[] { TestFilesFolderPath, "wwwroot/Static1" })
                    .StaticFolder("/static/two", localPath: new[] { TestFilesFolderPath, "wwwroot/Static2" }, defaultFiles: new[] { "index.html" })
                    .Middleware<TestDynamicContentMiddleware>());
            };

            //-- act

            HttpResponseMessage response1, response2, response3;

            using (var microservice = StartTestMicroservice(testCaseFeatures))
            {
                response1 = MakeHttpRequest(5000, HttpMethod.Get, "/static/one/test.png");
                response2 = MakeHttpRequest(5000, HttpMethod.Get, "/static/two");
                response3 = MakeHttpRequest(5000, HttpMethod.Get, "/my/dynamic/path");
            }

            //-- assert

            AssertHttpResponse(response1, HttpStatusCode.OK, "image/png", expectedContentFilePath: "wwwroot/Static1/test.png");
            AssertHttpResponse(response2, HttpStatusCode.OK, "text/html", expectedContentFilePath: "wwwroot/Static2/index.html");
            AssertHttpResponse(response3, HttpStatusCode.OK, "application/json", expectedContentAsString: "{my:1,dynamic:2,path:3}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanServeContentOverHttps()
        {
            //-- arrange

            Action<MicroserviceHostBuilder> testCaseFeatures = (hostBuilder) =>  {
                hostBuilder.UseHttpEndpoint(configure: endpoint => endpoint
                    .ListenOnPort(5000)
                    .HttpsListenOnPort(5001, "TestFiles/cert/sslcert.pfx".ToPathString(), "12345")
                    .StaticFolder("/static/one", localPath: new[] { TestFilesFolderPath, "wwwroot/Static1" })
                    .Middleware<TestDynamicContentMiddleware>());
            };

            //-- act

            HttpResponseMessage response1, response2;

            using (var microservice = StartTestMicroservice(testCaseFeatures))
            {
                response1 = MakeHttpRequest(true, 5001, HttpMethod.Get, "/static/one/test.png");
                response2 = MakeHttpRequest(true, 5001, HttpMethod.Get, "/my/dynamic/path");
            }

            //-- assert

            AssertHttpResponse(response1, HttpStatusCode.OK, "image/png", expectedContentFilePath: "wwwroot/Static1/test.png");
            AssertHttpResponse(response2, HttpStatusCode.OK, "application/json", expectedContentAsString: "{my:1,dynamic:2,path:3}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MicroserviceHost StartTestMicroservice(Action<MicroserviceHostBuilder> testCaseFeatures)
        {
            var hostBuilder = new MicroserviceHostBuilder("test");
            
            hostBuilder.UseAspNetCore();
            hostBuilder.UseFrameworkFeature<TestConfigurationFeature>();
            testCaseFeatures?.Invoke(hostBuilder);
            
            var host = (MicroserviceHost)hostBuilder.BuildHost();
            host.Start(CancellationToken.None);

            return host;
        } 

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private HttpResponseMessage MakeHttpRequest(int endpointPort, HttpMethod method, string pathAndQuery)
        {
            return MakeHttpRequest(false, endpointPort, method, pathAndQuery);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private HttpResponseMessage MakeHttpRequest(bool https, int endpointPort, HttpMethod method, string pathAndQuery)
        {
            using (var client = new HttpClient())
            {
                var protocol = (https ? "https" : "http");
                var requestUri = $"{protocol}://localhost:{endpointPort}/{pathAndQuery.TrimStart('/')}";
                var httpTask = client.SendAsync(new HttpRequestMessage(method, requestUri), HttpCompletionOption.ResponseContentRead, CancellationToken.None);
                var completed = httpTask.Wait(10000);

                completed.Should().BeTrue(because: "HTTP request must complete within allotted timeout.");

                var response = httpTask.Result;
                return response;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AssertHttpResponse(
            HttpResponseMessage response,
            HttpStatusCode expectedStatusCode,
            string expectedContentType,
            string expectedContentFilePath = null,
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
            var expectedFileFullPath = Path.Combine(TestFilesFolderPath, expectedContentFilePath.ToPathString());

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

        [FeatureLoader(Name = "TestConfigElementImpl")]
        public class TestConfigurationFeature : BasicFeature
        {
            public override void ContributeConfigSections(IComponentContainerBuilder newComponents)
            {
                newComponents.RegisterComponentType<TestHttpEndpointConfiguration>()
                    .InstancePerDependency()
                    .ForService<IHttpEndpointConfigElement>();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public class TestDynamicContentMiddleware : ICommunicationMiddleware<HttpContext>
        {
            public async Task OnMessage(HttpContext context, Func<Task> next)
            {
                await Task.Yield(); // simulate async execution

                var parts = context.Request.Path.Value.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                var properties = string.Join(",", parts.Select((p, index) => $"{p}:{index + 1}"));

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 200;

                using (var writer = new StreamWriter(context.Response.Body))
                {
                    writer.Write("{" + properties + "}");
                    writer.Flush();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
            
            public void OnError(Exception error, Action next)
            {
                next();
            }
        }
    }
}
