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
using NWheels.Testability.Extensions;
using Xunit;
using Xunit.Sdk;

namespace NWheels.Testability.Tests.Unit
{
    public class MicroserviceProcessTests : TestBase.UnitTest
    {
        [Fact]
        public async Task RunBatchJob_Success()
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
            processMock.InputClosed += AssertInputNotClosed;

            //-- act

            var microserviceProcess = new MicroserviceProcess(@"MyProject\my.csproj", processMock, environmentMock);
            await microserviceProcess.RunBatchJobAsync(new string[0], TimeSpan.FromSeconds(20));

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
        public async Task RunBatchJob_BadExitCode_Throw()
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
            processMock.InputClosed += AssertInputNotClosed;

            var microserviceProcess = new MicroserviceProcess(@"MyProject\my.csproj", processMock, environmentMock);

            Func<Task> act = async () => {
                await microserviceProcess.RunBatchJobAsync(new string[0], TimeSpan.FromSeconds(20));
            };

            //-- act & assert

            await act.ShouldThrowExceptionAsync<MicroserviceProcessException>(because: "process returned non-0 exit code");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task RunBatchJob_OutputReadException_Throw()
        {
            //-- arrange

            var environmentMock = new OSEnvironmentMock() {
                Platform = OSPlatform.Windows
            };

            var testException = new Exception("TEST-ERROR");
            var processMock = new OSProcessMock(
                new OSProcessMock.StdoutStep("Hello world"),
                new OSProcessMock.DelegatingStep(onStdoutLine => throw testException));
            processMock.InputClosed += AssertInputNotClosed;

            var microserviceProcess = new MicroserviceProcess(@"MyProject\my.csproj", processMock, environmentMock);

            Func<Task> act = async () => {
                await microserviceProcess.RunBatchJobAsync(new string[0], TimeSpan.FromSeconds(20));
            };

            //-- act & assert

            var exception = await act.ShouldThrowExceptionAsync<MicroserviceProcessException>(because: "failed to read process output");
            exception.InnerException.Should().BeSameAs(testException);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task RunBatchJob_Timeout_Throw()
        {
            //-- arrange

            var environmentMock = new OSEnvironmentMock() {
                Platform = OSPlatform.Windows
            };

            var processMock = new OSProcessMock(
                new OSProcessMock.StdoutStep("Hello world"),
                new OSProcessMock.DelayStep(10000));
            processMock.InputClosed += AssertInputNotClosed;

            var microserviceProcess = new MicroserviceProcess(@"MyProject\my.csproj", processMock, environmentMock);

            Func<Task> act = async () => {
                await microserviceProcess.RunBatchJobAsync(new string[0], TimeSpan.FromMilliseconds(20));
            };

            //-- act & assert

            var exception = await act.ShouldThrowExceptionAsync<MicroserviceProcessException>(because: "process should time out");
            exception.InnerException.Should().BeOfType<TimeoutException>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact] 
        public async Task RunDaemon_Success()
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
            await microserviceProcess.RunDaemonAsync(
                arguments: new string[0], 
                onUpAndRunningAsync: async () => {
                    await Task.Delay(1);
                    log.Add("UpAndRunning");
                },
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
                    $@"run --project {PathUtility.ExpandPathFromBinary(@"MyProject\my.csproj")} --no-restore --no-build -- --stdin-signal");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public async void RunDaemon_FailToStart_Throw()
        {
            //-- arrange

            var environmentMock = new OSEnvironmentMock() {
                Platform = OSPlatform.Windows
            };

            var processMock = new OSProcessMock(
                new OSProcessMock.StdoutStep("Hello world"),
                new OSProcessMock.StdoutStep("FAILURE!"),
                new OSProcessMock.ExitStep(exitCode: 11));

            processMock.InputClosed += AssertInputNotClosed;

            var microserviceProcess = new MicroserviceProcess(@"MyProject\my.csproj", processMock, environmentMock);

            Func<Task> act = async () => {
                await microserviceProcess.RunDaemonAsync(
                    arguments: new string[0],
                    onUpAndRunningAsync: AssertUpAndRunningNotCalled,
                    startTimeout: TimeSpan.FromSeconds(5),
                    stopTimeout: TimeSpan.FromSeconds(5));
            };

            //-- act & assert

            var exception = await act.ShouldThrowExceptionAsync<MicroserviceProcessException>(because: "daemon failed to start");
            exception.InnerException.Should().BeNull();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task RunDaemon_StartTimedOut_Throw()
        {
            //-- arrange

            var environmentMock = new OSEnvironmentMock() {
                Platform = OSPlatform.Windows
            };

            var processMock = new OSProcessMock(
                new OSProcessMock.StdoutStep("Hello world"),
                new OSProcessMock.StdoutStep("Starting...."),
                new OSProcessMock.DelayStep(10000));

            processMock.InputClosed += AssertInputNotClosed;

            var microserviceProcess = new MicroserviceProcess(@"MyProject\my.csproj", processMock, environmentMock);

            Func<Task> act = async () => {
                await microserviceProcess.RunDaemonAsync(
                    arguments: new string[0],
                    onUpAndRunningAsync: AssertUpAndRunningNotCalled,
                    startTimeout: TimeSpan.FromMilliseconds(50),
                    stopTimeout: TimeSpan.FromSeconds(5));
            };

            //-- act & assert

            var exception = await act.ShouldThrowExceptionAsync<MicroserviceProcessException>(because: "daemon failed to start");
            exception.InnerException.Should().BeOfType<TimeoutException>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact]
        public async Task UseCoverage_StartInfoConfiguredFromEnvVars()
        {
            //-- arrange

            var environmentMock = new OSEnvironmentMock(OSPlatform.Windows, new Dictionary<string, string> {
                { MicroserviceProcess.EnvironmentVariableNames.UseCoverage, "1" },
                { MicroserviceProcess.EnvironmentVariableNames.CoverageExecutable, @"C:\tools\cover.exe" },
                { MicroserviceProcess.EnvironmentVariableNames.CoverageArgsTemplate, "MY-TEMPLATE [[PROJECT]] MY-TEMPLATE-MID [[ARGS]] MY-TEMPLATE-END" },
                { MicroserviceProcess.EnvironmentVariableNames.CoverageArgumentsPlaceholder, "[[ARGS]]" },
                { MicroserviceProcess.EnvironmentVariableNames.CoverageProjectPlaceholder, "[[PROJECT]]" },
            });

            var processMock = new OSProcessMock(
                new OSProcessMock.StdoutStep("Hello world"),
                new OSProcessMock.StdoutStep("DONE"),
                new OSProcessMock.ExitStep(exitCode: 0));
            processMock.Starting += assertStartInfo;
            processMock.InputClosed += AssertInputNotClosed;

            //-- act

            var microserviceProcess = new MicroserviceProcess(@"MyProject\my.csproj", processMock, environmentMock);
            await microserviceProcess.RunBatchJobAsync(
                arguments: new[] { "--arg1", "v1", "--arg2", "v2" }, 
                timeout: TimeSpan.FromSeconds(20));

            //-- assert

            processMock.AssertEndOfScript();

            void assertStartInfo(ProcessStartInfo startInfo)
            {
                startInfo.FileName.Should().Be(@"C:\tools\cover.exe");
                startInfo.Arguments.Should().Be(
                    $@"MY-TEMPLATE {PathUtility.ExpandPathFromBinary(@"MyProject\my.csproj")} MY-TEMPLATE-MID --arg1 v1 --arg2 v2 MY-TEMPLATE-END");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AssertInputNotClosed()
        {
            Assert.False(true, "CloseInput should not be called.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Task AssertUpAndRunningNotCalled()
        {
            Assert.False(true, "onUpAndRunning callback should not be called.");
            return Task.CompletedTask;
        }
    }
}
