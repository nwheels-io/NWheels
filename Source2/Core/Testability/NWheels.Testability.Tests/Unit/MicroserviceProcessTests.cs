using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NWheels.Kernel.Api.Extensions;
using Xunit;

namespace NWheels.Testability.Tests.Unit
{
    public class MicroserviceProcessTests : TestBase.UnitTest
    {
        [Fact]
        public void RunBatchJob_Success()
        {
            //-- arrange

            var environmentMock = new OSEnvironmentMock() {
                Platform = OSPlatform.Windows
            };

            var processMock = new OSProcessMock(
                new OSProcessMock.StdoutStep("Hello world"),
                new OSProcessMock.DelayStep(50),
                new OSProcessMock.StdoutStep("DONE"),
                new OSProcessMock.ExitStep(exitCode: 0));
            processMock.Starting += assertStartInfo;

            var microserviceProcess = new MicroserviceProcess(@"MyProject\my.csproj", processMock, environmentMock);

            //-- act

            microserviceProcess.RunBatchJob(new string[0], TimeSpan.FromSeconds(1000));

            //-- assert

            processMock.AssertEndOfScript();

            void assertStartInfo(ProcessStartInfo startInfo)
            {
                startInfo.FileName.Should().Be("dotnet.exe");
                startInfo.Arguments.Should().Be($@"run --project {PathUtility.ExpandPathFromBinary(@"MyProject\my.csproj")} --no-restore --no-build");
            }
        }
    }
}
