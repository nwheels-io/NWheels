using System;
using Xunit;
using FluentAssertions;
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
                startTimeout: TimeSpan.FromSeconds(120),
                stopTimeout: TimeSpan.FromSeconds(120));

            //-- assert

            microservice.ExitCode.Should().Be(0);
            microservice.Output.Should().NotBeEmpty();
            upAndRunningCount.Should().Be(1);
        }
    }
}