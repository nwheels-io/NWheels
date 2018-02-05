using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NWheels.Kernel.Api.Extensions;
using NWheels.Microservices.Runtime;
using Xunit;
using Xunit.Sdk;

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
            processMock.InputClosed += () => Assert.False(true, "CloseInput should not be called.");

            //-- act

            var microserviceProcess = new MicroserviceProcess(@"MyProject\my.csproj", processMock, environmentMock);
            microserviceProcess.RunBatchJob(new string[0], TimeSpan.FromSeconds(5));

            //-- assert

            processMock.AssertEndOfScript();

            void assertStartInfo(ProcessStartInfo startInfo)
            {
                startInfo.FileName.Should().Be("dotnet.exe");
                startInfo.Arguments.Should().Be($@"run --project {PathUtility.ExpandPathFromBinary(@"MyProject\my.csproj")} --no-restore --no-build");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RunBatchJob_BadExitCode_Throw()
        {
            //-- arrange

            var environmentMock = new OSEnvironmentMock() {
                Platform = OSPlatform.Windows
            };

            var processMock = new OSProcessMock(
                new OSProcessMock.StdoutStep("Hello world"),
                new OSProcessMock.DelayStep(50),
                new OSProcessMock.StdoutStep("FAILED!"),
                new OSProcessMock.ExitStep(exitCode: 12));
            processMock.InputClosed += assertInputNotClosed;

            var microserviceProcess = new MicroserviceProcess(@"MyProject\my.csproj", processMock, environmentMock);

            Action act = () => {
                microserviceProcess.RunBatchJob(new string[0], TimeSpan.FromSeconds(5));
            };

            //-- act & assert

            act.ShouldThrow<MicroserviceProcessException>(because: "process returned non-0 exit code");

            void assertInputNotClosed()
            {
                Assert.False(true, "CloseInput should not be called.");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RunBatchJob_OutputReadException_Throw()
        {
            //-- arrange

            var environmentMock = new OSEnvironmentMock() {
                Platform = OSPlatform.Windows
            };

            var testException = new Exception("TEST-ERROR");
            var processMock = new OSProcessMock(
                new OSProcessMock.StdoutStep("Hello world"),
                new OSProcessMock.DelegatingStep(onStdoutLine => throw testException));
            processMock.InputClosed += assertInputNotClosed;

            var microserviceProcess = new MicroserviceProcess(@"MyProject\my.csproj", processMock, environmentMock);

            Action act = () => {
                microserviceProcess.RunBatchJob(new string[0], TimeSpan.FromSeconds(55));
            };

            //-- act & assert

            act.ShouldThrow<MicroserviceProcessException>(because: "failed to read process output")
                .Which.InnerException.Should().BeSameAs(testException);

            void assertInputNotClosed()
            {
                Assert.False(true, "CloseInput should not be called.");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public void RunBatchJob_Timeout_Throw()
        {
            //-- arrange

            var environmentMock = new OSEnvironmentMock() {
                Platform = OSPlatform.Windows
            };

            var processMock = new OSProcessMock(
                new OSProcessMock.StdoutStep("Hello world"),
                new OSProcessMock.DelayStep(10000));
            processMock.InputClosed += assertInputNotClosed;

            var microserviceProcess = new MicroserviceProcess(@"MyProject\my.csproj", processMock, environmentMock);

            Action act = () => {
                microserviceProcess.RunBatchJob(new string[0], TimeSpan.FromMilliseconds(50));
            };

            //-- act & assert

            act.ShouldThrow<MicroserviceProcessException>(because: "process should time out")
                .Which.InnerException.Should().BeOfType<TimeoutException>();

            void assertInputNotClosed()
            {
                Assert.False(true, "CloseInput should not be called.");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        //[Fact] // ignored -- not completed
        public void RunDaemon_Success()
        {
            //-- arrange

            List<string> log = new List<string>();
            ManualResetEvent inputClosedEvent = new ManualResetEvent(false);

            var environmentMock = new OSEnvironmentMock() {
                Platform = OSPlatform.Windows
            };

            var processMock = new OSProcessMock(
                new OSProcessMock.StdoutStep("Hello world"),
                new OSProcessMock.StdoutStep(nameof(IMicroserviceHostLogger.RunningInDaemonMode)),
                new OSProcessMock.DelegatingStep(onStdoutLine => {
                    inputClosedEvent.WaitOne(5000).Should().BeTrue(because: "InputClosed should be called");
                    return Task.CompletedTask;
                }),
                new OSProcessMock.ExitStep(exitCode: 0));

            processMock.Starting += assertStartInfo;

            processMock.InputClosed += () => {
                log.Add("InputClosed");
                inputClosedEvent.Set();
            };

            //-- act

            var microserviceProcess = new MicroserviceProcess(@"MyProject\my.csproj", processMock, environmentMock);
            microserviceProcess.RunDaemon(
                arguments: new string[0], 
                onUpAndRunning: () => log.Add("UpAndRunning"),
                startTimeout: TimeSpan.FromSeconds(5),
                stopTimeout: TimeSpan.FromSeconds(5));

            //-- assert

            processMock.AssertEndOfScript();
            log.Should().Equal("Starting", "UpAndRunning", "InputClosed");

            void assertStartInfo(ProcessStartInfo startInfo)
            {
                log.Add("Starting");
                startInfo.FileName.Should().Be("dotnet.exe");
                startInfo.Arguments.Should().Be(
                    $@"run --project {PathUtility.ExpandPathFromBinary(@"MyProject\my.csproj")} --no-restore --no-build --stdin-signal");
            }
        }
    }
}
