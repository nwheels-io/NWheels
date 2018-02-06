using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NWheels.Kernel.Api.Extensions;
using NWheels.Kernel.Api.Primitives;
using NWheels.Microservices.Runtime;
using NWheels.Testability.Extensions;
using Xunit;

namespace NWheels.Testability
{
    public class MicroserviceProcess
    {
        private static readonly string _s_testBinaryFolderPath = 
            Path.GetDirectoryName(typeof(MicroserviceProcess).Assembly.Location);

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly string _projectFilePath;
        private readonly Stopwatch _clock;
        private readonly List<string> _output;
        private readonly IOperatingSystemProcess _processHandler;
        private readonly IOperatingSystemEnvironment _environmentHandler;
        private readonly EnvironmentVariables _environment;
        private string[] _arguments;
        private List<Exception> _errors;
        private int? _exitCode;
        
        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceProcess(string projectFileRelativePath)
            : this(projectFileRelativePath, new RealOperatingSystemProcess(), new RealOperatingSystemEnvironment())
        {
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceProcess(
            string projectFileRelativePath, 
            IOperatingSystemProcess processHandler, 
            IOperatingSystemEnvironment environmentHandler)
        {
            _projectFilePath = PathUtility.CombineNormalize(_s_testBinaryFolderPath, projectFileRelativePath);
            _processHandler = processHandler;
            _environmentHandler = environmentHandler;

            _environment = new EnvironmentVariables(environmentHandler);
            _clock = new Stopwatch();
            _output = new List<string>();
            _errors = new List<Exception>();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public async Task RunBatchJobAsync(string[] arguments, TimeSpan timeout)
        {
            Stopwatch clock = Stopwatch.StartNew();
            StartProcess(arguments);

            try
            {
                await ReadStandardOutput(timeout);
            }
            catch (Exception e)
            {
                _errors.Add(e.Flatten());
            }
            finally
            {
                if (!_processHandler.HasExited && !HasTimedOut())
                {
                    await TryStopTimely(timeout.Subtract(clock.Elapsed));
                }
            }

            if (_processHandler.HasExited)
            {
                _exitCode = _processHandler.ExitCode;
            }

            MicroserviceProcessException.ThrowIfFailed(this);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public async Task RunDaemonAsync(string[] arguments, Func<Task> onUpAndRunningAsync, TimeSpan? startTimeout = null, TimeSpan? stopTimeout = null)
        {
            var effectiveArguments = arguments.Concat(new[] { "--stdin-signal" }).ToArray();
            StartProcess(effectiveArguments);

            try
            {
                var upAndRunningMessage = nameof(IMicroserviceHostLogger.RunningInDaemonMode);

                var status = await ReadStandardOutput(
                    startTimeout ?? TimeSpan.FromMinutes(1),
                    shouldBreakIf: line => line.Contains(upAndRunningMessage));

                if (status == OutputReadStatus.BreakCondition)
                {
                    await onUpAndRunningAsync();
                }
            }
            catch (Exception e)
            {
                _errors.Add(e);
            }
            finally
            {
                if (!_processHandler.HasExited && !HasTimedOut())
                {
                    _processHandler.CloseInput();
                    await TryStopTimely(stopTimeout ?? TimeSpan.FromMinutes(1));
                }
            }

            if (_processHandler.HasExited)
            {
                _exitCode = _processHandler.ExitCode;
            }

            MicroserviceProcessException.ThrowIfFailed(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string GetExecutableFileName()
        {
            if (_environment.IsCoverageEnabled)
            {
                return _environment.CoverageExecutable;
            }
            else if (_environmentHandler.IsOSPlatform(OSPlatform.Windows))
            {
                return "dotnet.exe";
            }
            else
            {
                return "dotnet";
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string GetExecutableArguments()
        {
            var quoteChar = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? '"' : '\'');
            var arguments = string.Join(" ", _arguments.Select(escapeSpaces));

            if (_environment.IsCoverageEnabled)
            {
                var resolvedArguments = _environment.CoverageArgsTemplate
                    .Replace(_environment.CoverageProjectPlaceholder, escapeSpaces(_projectFilePath))
                    .Replace(_environment.CoverageArgumentsPlaceholder, arguments);

                return resolvedArguments;
            }
            else
            {
                return (
                    $"run --project {escapeSpaces(_projectFilePath)} --no-restore --no-build" + 
                    (arguments.Length > 0 ? $" -- {arguments}" : ""));
            }

            string escapeSpaces(string s)
            {
                return (s.Contains(" ") ? quoteChar + s + quoteChar : s);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CopyOutput(StringBuilder destination)
        {
            foreach (var line in _output)
            {
                destination.AppendLine(line);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string GetFormattedOutput(string startWith = null)
        {
            var formatted = new StringBuilder();

            if (startWith != null)
            {
                formatted.Append(startWith);
            }

            formatted.AppendLine("------ Output ------");
            CopyOutput(formatted);
            formatted.AppendLine("--- End of Output ---");

            return formatted.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IReadOnlyList<string> Output => _output;
        public int? ExitCode => _exitCode;
        public IReadOnlyList<Exception> Errors => _errors;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void StartProcess(string[] arguments)
        {
            _arguments = arguments;
            _errors = new List<Exception>();

            var info = new ProcessStartInfo() {
                FileName = GetExecutableFileName(),
                Arguments = GetExecutableArguments(),
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };

            // Assert.False(
            //     true, 
            //     $"MicroserviceProcess:\r\n- executable > {info.FileName}\r\n- arguments  > {info.Arguments}");

            _processHandler.Start(info);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private async Task<OutputReadStatus> ReadStandardOutput(TimeSpan timeout, Func<string, bool> shouldBreakIf = null)
        {
            var timeoutCancellation = new CancellationTokenSource();
            var timeoutTask = Task.Delay(timeout, timeoutCancellation.Token);

            while (true)
            {
                var readLineTask = _processHandler.ReadOutputLineAsync();
                var isTimedOut = (await Task.WhenAny(readLineTask, timeoutTask) == timeoutTask);

                if (isTimedOut)
                {
                    var timeoutMessage = $"Timed out waiting for microservice process to {(shouldBreakIf != null ? "start" : "exit")}.";
                    throw new TimeoutException(timeoutMessage);
                }

                var line = readLineTask.Result;
                if (line == null)
                {
                    timeoutCancellation.Cancel();
                    return OutputReadStatus.EndOfFile;
                }

                _output.Add(line);

                if (shouldBreakIf != null && shouldBreakIf(line))
                {
                    timeoutCancellation.Cancel();
                    return OutputReadStatus.BreakCondition;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private async Task<bool> TryStopTimely(TimeSpan stopTimeout)
        {
            var minStopTimeout = TimeSpan.FromSeconds(3);
            var effectiveStopTimeout = (stopTimeout < minStopTimeout ? minStopTimeout : stopTimeout);

            try
            {
                var status = await ReadStandardOutput(effectiveStopTimeout);
                return (status == OutputReadStatus.EndOfFile);
            }
            catch (Exception e)
            {
                _errors.Add(e);
                return false;
            }
            //int stopTimeoutMilliseconds = 1000;

            //if (stopTimeout.HasValue)
            //{
            //    stopTimeoutMilliseconds = (int)stopTimeout.Value.TotalMilliseconds;
            //} 
            //else if (_timeout.HasValue)
            //{
            //    stopTimeoutMilliseconds = (int)Math.Max(100, _timeout.Value.Subtract(_clock.Elapsed).TotalMilliseconds);
            //}
            
            //if (!_process.WaitForExit(stopTimeoutMilliseconds) && _errorReadingOutput == null)
            //{
            //    throw new TimeoutException("Timed out waiting for microservice process to complete.");
            //}
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool HasTimedOut()
        {
            return Errors.OfType<TimeoutException>().Any();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static class EnvironmentVariableNames
        {
            public const string UseCoverage = "NW_SYSTEST_USE_COVER";
            public const string CoverageExecutable = "NW_SYSTEST_COVER_EXE";
            public const string CoverageArgsTemplate = "NW_SYSTEST_COVER_ARGS_TEMPLATE";
            public const string CoverageProjectPlaceholder = "NW_SYSTEST_COVER_PROJECT_PLACEHOLDER";
            public const string CoverageArgumentsPlaceholder = "NW_SYSTEST_COVER_ARGS_PLACEHOLDER";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private enum OutputReadStatus
        {
            EndOfFile,
            BreakCondition,
            ReadFailure
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private struct EnvironmentVariables
        {
            public EnvironmentVariables(IOperatingSystemEnvironment osEnvironment)
            {
                UseCoverage = osEnvironment.GetEnvironmentVariable(EnvironmentVariableNames.UseCoverage);
                CoverageExecutable = osEnvironment.GetEnvironmentVariable(EnvironmentVariableNames.CoverageExecutable);
                CoverageArgsTemplate = osEnvironment.GetEnvironmentVariable(EnvironmentVariableNames.CoverageArgsTemplate);
                CoverageProjectPlaceholder = osEnvironment.GetEnvironmentVariable(EnvironmentVariableNames.CoverageProjectPlaceholder);
                CoverageArgumentsPlaceholder = osEnvironment.GetEnvironmentVariable(EnvironmentVariableNames.CoverageArgumentsPlaceholder);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool IsCoverageEnabled => (
                !string.IsNullOrEmpty(UseCoverage) &&
                !string.IsNullOrEmpty(CoverageExecutable) &&
                !string.IsNullOrEmpty(CoverageArgsTemplate));

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string UseCoverage { get; }
            public string CoverageExecutable { get; }
            public string CoverageArgsTemplate { get; }
            public string CoverageProjectPlaceholder { get; }
            public string CoverageArgumentsPlaceholder { get; }
        }
    }
}
