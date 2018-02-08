using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using NWheels.Communication.Api.Http;
using NWheels.Kernel.Api.Extensions;
using NWheels.Testability;
using NWheels.Samples.HelloWorld;
using NWheels.Testability.Extensions;

namespace NWheels.Samples.HelloWorld.Tests.SystemApi
{
    public class HelloServiceApiTests : TestBase.SystemApiTest
    {
        public const string MicroserviceProjectRelativePath =
            @"..\..\..\..\NWheels.Samples.HelloWorld.HelloService\NWheels.Samples.HelloWorld.HelloService.csproj";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Fact]
        public async Task CanStartAndStop()
        {
            //-- arrange

            var microservice = new MicroserviceProcess(MicroserviceProjectRelativePath);
            var upAndRunningCount = 0;       

            //-- act

            await microservice.RunDaemonAsync(
                arguments: new[] { "run" }, 
                onUpAndRunningAsync: () => { 
                    upAndRunningCount++;
                    return Task.CompletedTask;
                },
                startTimeout: TimeSpan.FromSeconds(30),
                stopTimeout: TimeSpan.FromSeconds(10));

            //-- assert

            microservice.ExitCode.Should().Be(0);
            microservice.Output.Should().NotBeEmpty();
            upAndRunningCount.Should().Be(1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Fact]
        public async Task HelloWorldTx_InvokeProperly_Success()
        {
            //-- arrange

            var microservice = new MicroserviceProcess(MicroserviceProjectRelativePath);        

            //-- act
            
            string jsonResponse = null;
            string htmlResponse = null;

            await microservice.RunDaemonAsync(
                arguments: new[] { "run" },
                onUpAndRunningAsync: async () => {
                    jsonResponse = await HttpAssert.MakeLocalHttpRequest(5000, HttpMethod.Post, "/api/tx/Hello/Hello", "{name:'TEST'}");
                    htmlResponse = await HttpAssert.MakeLocalHttpRequest(5000, HttpMethod.Get, "/", expectedContentType: "text/html");
                },
                startTimeout: TimeSpan.FromSeconds(30),
                stopTimeout: TimeSpan.FromSeconds(10));

            //-- assert

            AssertMicroserviceOutput(microservice, () => {

                jsonResponse.Should().BeJson("{result:'Hello, TEST!'}");
                htmlResponse.Should().NotBeNull();

                var indexHtmlFilePath = PathUtility.ExpandPathFromBinary(Path.Combine(
                    Path.GetDirectoryName(MicroserviceProjectRelativePath),
                    "WebFiles",
                    "index.html"));

                var expectedHtmlResponse = File.ReadAllText(indexHtmlFilePath);
                htmlResponse.Should().Be(expectedHtmlResponse);

            });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task HelloWorldTx_SendInvalidVerb_ReceiveBadRequest()
        {
            //-- arrange

            var microservice = new MicroserviceProcess(MicroserviceProjectRelativePath);

            //-- act

            string jsonResponse = null;

            await microservice.RunDaemonAsync(
                arguments: new[] { "run" },
                onUpAndRunningAsync: async () => {
                    jsonResponse = await HttpAssert.MakeLocalHttpRequest(
                        5000, HttpMethod.Get, "/api/tx/Hello/Hello", "{name:'TEST'}", 
                        expectedStatusCode: HttpStatusCode.BadRequest, expectedContentType: null);
                },
                startTimeout: TimeSpan.FromSeconds(30),
                stopTimeout: TimeSpan.FromSeconds(10));

            //-- assert

            AssertMicroserviceOutput(microservice, () => { });
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task HelloWorldTx_SendInvalidJson_ReceiveBadRequest()
        {
            //-- arrange

            var microservice = new MicroserviceProcess(MicroserviceProjectRelativePath);

            //-- act

            string jsonResponse = null;

            await microservice.RunDaemonAsync(
                arguments: new[] { "run" },
                onUpAndRunningAsync: async () => {
                    jsonResponse = await HttpAssert.MakeLocalHttpRequest(
                        5000, HttpMethod.Post, "/api/tx/Hello/Hello", "{bad:'BAD'}",
                        expectedStatusCode: HttpStatusCode.BadRequest, expectedContentType: null);
                },
                startTimeout: TimeSpan.FromSeconds(30),
                stopTimeout: TimeSpan.FromSeconds(10));

            //-- assert

            AssertMicroserviceOutput(microservice, () => { });
        }
    }
}
