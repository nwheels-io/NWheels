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
using NWheels.Microservices.Runtime;
using Xunit;

namespace NWheels.Testability
{
    public class MicroserviceProcess
    {
        private static readonly string _s_useCoverage = 
            System.Environment.GetEnvironmentVariable(EnvironmentVariables.UseCoverage);
        private static readonly string _s_coverageExecutable = 
            System.Environment.GetEnvironmentVariable(EnvironmentVariables.CoverageExecutable);
        private static readonly string _s_coverageArgsTemplate = 
            System.Environment.GetEnvironmentVariable(EnvironmentVariables.CoverageArgsTemplate);
        private static readonly string _s_coverageProjectPlaceholder = 
            System.Environment.GetEnvironmentVariable(EnvironmentVariables.CoverageProjectPlaceholder);
        private static readonly string _s_coverageArgumentsPlaceholder = 
            System.Environment.GetEnvironmentVariable(EnvironmentVariables.CoverageArgumentsPlaceholder);
        private static readonly string _s_testBinaryFolderPath = 
            Path.GetDirectoryName(typeof(MicroserviceProcess).Assembly.Location);

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly string _projectFilePath;
        private readonly CancellationTokenSource _timeoutCancellation;
        private readonly Stopwatch _clock;
        private readonly List<string> _output; 
        private string[] _arguments;
        private TimeSpan? _timeout;
        private Process _process;
        private List<Exception> _errors;
        private int? _exitCode;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceProcess(string projectFileRelativePath)
        {
            _projectFilePath = Path.Combine(_s_testBinaryFolderPath, projectFileRelativePath);

            _timeoutCancellation = new CancellationTokenSource();
            _clock = new Stopwatch();
            _output = new List<string>();
            _errors = new List<Exception>();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void RunBatchJob(string[] arguments, TimeSpan timeout)
        {
            StartProcess(arguments, timeout);

            try
            {
                ReadStandardOutput();
            }
            catch (Exception e)
            {
                _errors.Add(e);
            }
            finally
            {
                TryStopTimely();
            }

            if (_process.HasExited)
            {
                _exitCode = _process.ExitCode;
            }

            MicroserviceProcessException.ThrowIfFailed(this);
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public void RunDaemon(string[] arguments, Action onUpAndRunning, TimeSpan? startTimeout = null, TimeSpan? stopTimeout = null)
        {
            var effectiveArguments = arguments.Concat(new[] { "--stdin-signal" }).ToArray();
            StartProcess(effectiveArguments);

            try
            {
                // TODO: replace with IPC listener
                var upAndRunningMessage = nameof(IMicroserviceHostLogger.RunningInDaemonMode);

                var status = ReadStandardOutput(
                    shouldContinueAfter: line => !line.Contains(upAndRunningMessage),
                    timeout: startTimeout);

                if (status == OutputReadStatus.StopCondition)
                {
                    onUpAndRunning();
                }
            }
            catch (Exception e)
            {
                _errors.Add(e);
            }
            finally
            {
                _process.StandardInput.Close();
                TryStopTimely(stopTimeout);
            }

            if (_process.HasExited)
            {
                _exitCode = _process.ExitCode;
            }

            MicroserviceProcessException.ThrowIfFailed(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string GetExecutableFileName()
        {
            if (IsCoverageEnabled)
            {
                return _s_coverageExecutable;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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

            if (IsCoverageEnabled)
            {
                var resolvedArguments = _s_coverageArgsTemplate
                    .Replace(_s_coverageProjectPlaceholder, escapeSpaces(_projectFilePath))
                    .Replace(_s_coverageArgumentsPlaceholder, arguments);

                return resolvedArguments;
            }
            else
            {
                return $"run --project {escapeSpaces(_projectFilePath)} --no-restore --no-build -- {arguments}";
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

        private void StartProcess(string[] arguments, TimeSpan? timeout = null)
        {
            _arguments = arguments;
            _timeout = timeout;
            _errors = new List<Exception>();

            if (timeout.HasValue)
            {
                _timeoutCancellation.CancelAfter(timeout.Value);
            }

            var info = new ProcessStartInfo() {
                FileName = GetExecutableFileName(),
                Arguments = GetExecutableArguments(),
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };

            // Assert.False(
            //     true, 
            //     $"MicroserviceProcess:\r\n- executable > {info.FileName}\r\n- arguments  > {info.Arguments}");

            _process = Process.Start(info);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private OutputReadStatus ReadStandardOutput(Func<string, bool> shouldContinueAfter = null, TimeSpan? timeout = null)
        {
            var effectiveTimeoutCancellation = (
                timeout.HasValue 
                ? new CancellationTokenSource(timeout.Value)
                : _timeoutCancellation);   

            while (!effectiveTimeoutCancellation.IsCancellationRequested)
            {
                var readLineTask = _process.StandardOutput.ReadLineAsync();
                try
                {
                    readLineTask.Wait(effectiveTimeoutCancellation.Token);
                }
                catch (OperationCanceledException) 
                {
                    break;
                }
                
                var line = readLineTask.Result;
                if (line == null)
                {
                    return OutputReadStatus.EndOfFile;
                }

                _output.Add(line);

                if (shouldContinueAfter != null && !shouldContinueAfter(line))
                {
                    return OutputReadStatus.StopCondition;
                }
            }

            var timeoutMessage = $"Timed out waiting for microservice process to {(shouldContinueAfter != null ? "start" : "exit")}.";
            throw new TimeoutException(timeoutMessage);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool TryStopTimely(TimeSpan? stopTimeout = null)
        {
            try
            {
                ReadStandardOutput(timeout: stopTimeout);
                return true;
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

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool IsCoverageEnabled
        {
            get
            {
                return (
                    !string.IsNullOrEmpty(_s_useCoverage) && 
                    !string.IsNullOrEmpty(_s_coverageExecutable) && 
                    !string.IsNullOrEmpty(_s_coverageArgsTemplate));
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public static class EnvironmentVariables
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
            StopCondition,
            ReadFailure
        }
    }
}
