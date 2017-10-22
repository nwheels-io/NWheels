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

namespace NWheels.Communication.Adapters.AspNetCore.Tests.Integration
{
    public class KestrelHttpEndpointTests : TestBase.UnitTest
    {
        [Fact]
        public void CanServeStaticFile()
        {
            //-- arrange

            Action<MicroserviceHostBuilder> testCaseFeatures = (hostBuilder) => {
                hostBuilder.UseHttpEndpoint(configure: endpoint => {
                    endpoint.Port = 5000;
                    endpoint.StaticFolders.Add(new TestHttpStaticFolderConfig() {
                        LocalRootPath = Path.Combine(TestFilesFolderPath, "wwwroot/Static1".ToPathString()),
                        RequestBasePath = "/static/files/one"
                    });
                });
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

        private HttpResponseMessage MakeHttpRequest(int endpointPort, HttpMethod method, string path)
        {
            using (var client = new HttpClient())
            {
                var requestUri = $"http://localhost:{endpointPort}/{path.TrimStart('/')}";
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
            var expectedFileFullPath = Path.Combine(TestBinaryFolderPath, expectedContentFilePath.ToPathString());

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
    }
}
