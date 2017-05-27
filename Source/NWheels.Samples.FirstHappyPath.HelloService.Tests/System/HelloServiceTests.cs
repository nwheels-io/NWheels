using FluentAssertions;
using Newtonsoft.Json;
using NWheels.Testability.Microservices;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Xunit;

namespace NWheels.Samples.FirstHappyPath.HelloService.Tests.System
{
    [Trait("Purpose", "SystemTest")]
    public class HelloServiceTests : IDisposable
    {
        private MicroserviceController _controller;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public HelloServiceTests()
        {
            var configurationName = MicroserviceControllerBuilder.DefaultProjectConfigurationName;

            _controller = new MicroserviceControllerBuilder()
                .RunMicroserviceAssembly($@"..\..\NWheels.Samples.FirstHappyPath.HelloService\bin\{configurationName}\netcoreapp1.1\hello.dll")
                .Build();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            _controller.StopOrThrow(10000);
            _controller.AssertNoErrors();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanStartAndStop()
        {
            _controller.Start();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void CanInvokeHelloWorldTx()
        {
            //-- arrange

            _controller.Start();

            //-- act

            var txResponseJson = MakeJsonHttpRequest(5000, HttpMethod.Post, "tx/HelloWorld/Hello", "{name: 'ABCD1234'}");

            //-- assert

            AssertJsonEqual(txResponseJson, "{result: 'Hello world, from ABCD1234!'}");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string MakeJsonHttpRequest(int endpointPort, HttpMethod method, string path, string jsonData)
        {
            using (var client = new HttpClient())
            {
                var requestUri = $"http://localhost:{endpointPort}/{path.TrimStart('/')}";
                var request = new HttpRequestMessage(method, requestUri) {
                    Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
                };
                var httpTask = client.SendAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
                var completed = httpTask.Wait(10000);

                Assert.True(completed, "HTTP request didn't complete within allotted timeout.");

                var response = httpTask.Result;
                response.StatusCode.Should().Be(HttpStatusCode.OK, because: "requets must complete successfully");
                response.Content.Headers.ContentType.MediaType.Should().Be("application/json", because: "response must be JSON");

                var responseText = response.Content.ReadAsStringAsync().Result;
                return responseText;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AssertJsonEqual(string actual, string expected, string because = "", params object[] becauseArgs)
        {
            dynamic actualDeserialized = JsonConvert.DeserializeObject(actual);
            dynamic expectedDeserialized = JsonConvert.DeserializeObject(expected);

            string actualNormalized = JsonConvert.SerializeObject(actualDeserialized, Formatting.None);
            string expectedNormalized = JsonConvert.SerializeObject(expectedDeserialized, Formatting.None);

            actualNormalized.Should().Be(expectedNormalized, because, becauseArgs);
        }
    }
}
