using NWheels.Testability.Microservices;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace NWheels.Samples.FirstHappyPath.HelloService.Tests.System
{
    [Trait("Purpose", "SystemTest")]
    public class HelloServiceTests
    {
        [Fact]
        public void CanStartAndStop()
        {
            var configurationName = MicroserviceControllerBuilder.DefaultProjectConfigurationName;
            var controller = new MicroserviceControllerBuilder()
                .UseMicroserviceAssembly($@"..\..\NWheels.Samples.FirstHappyPath.HelloService\bin\{configurationName}\netcoreapp1.1\hello.dll")
                .Build();

            controller.Start();
            controller.StopOrThrow(10000);
        }
    }
}
