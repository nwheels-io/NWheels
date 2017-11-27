using System;
using System.IO;
using System.Net.Http;
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
        public void CanStartAndStop()
        {
            //-- arrange

            var microservice = new MicroserviceProcess(MicroserviceProjectRelativePath);
            var upAndRunningCount = 0;       

            //-- act

            microservice.RunDaemon(
                arguments: new[] { "run" }, 
                onUpAndRunning: () => { 
                    upAndRunningCount++; 
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
        public void CanInvokeHelloWorldTx()
        {
            //-- arrange

            var microservice = new MicroserviceProcess(MicroserviceProjectRelativePath);        

            //-- act
            
            string jsonResponse = null;
            string htmlResponse = null;

            microservice.RunDaemon(
                arguments: new[] { "run" },
                onUpAndRunning: () => {
                    jsonResponse = MakeHttpRequest(5000, HttpMethod.Post, "/api/tx/Hello/Hello", "{name:'TEST'}").Result;
                    htmlResponse = MakeHttpRequest(5000, HttpMethod.Get, "/files", expectedContentType: "text/html").Result;
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
    }
}
