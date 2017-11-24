using System;
using System.IO;
using Xunit;
using FluentAssertions;
using NWheels.Communication.Api.Http;
using NWheels.Kernel.Api.Extensions;
using NWheels.Testability;
using NWheels.Samples.HelloWorld;

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
                startTimeout: TimeSpan.FromSeconds(10),
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
            
            dynamic jsonResponse = null;
            string htmlResponse = null;
            
            microservice.RunDaemon(
                arguments: new[] { "run" }, 
                onUpAndRunning: () => {
                    var bot = new HttpBot(baseUrl: "http://127.0.0.1:5000");
                    jsonResponse = bot.Post("/api/tx/Hello/Hello?name=TEST").ResponseBodyAsJsonDynamic();
                    htmlResponse = bot.Get("/files").ResponseBodyAsString();
                },
                startTimeout: TimeSpan.FromSeconds(10),
                stopTimeout: TimeSpan.FromSeconds(10));

            //-- assert

            microservice.ExitCode.Should().Be(0);

            ((object)jsonResponse).Should().NotBeNull();
            string result = jsonResponse.result;
            result.Should().Be("Hello, TEST!");

            htmlResponse.Should().NotBeNull();
            
            var indexHtmlFilePath = PathUtility.ExpandPathFromBinary(Path.Combine(
                Path.GetDirectoryName(MicroserviceProjectRelativePath), 
                "WebFiles", 
                "index.html"));
            
            var expectedHtmlResponse = File.ReadAllText(indexHtmlFilePath);
            htmlResponse.Should().Be(expectedHtmlResponse);
        }
    }
}
