using FluentAssertions;
using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xunit;

namespace NWheels.Testability.Microservices
{
    public class MicroserviceHostController
    {
        private readonly MicroserviceConfig _microserviceConfig;
        private readonly EnvironmentConfig _environmentConfig;
        private string _cliDirectory;
        private string _microserviceDirectory;
        private string _microserviceFilePath;
        private string _environmentFilePath;
        private Process _process;
        private OutputReader _stdOut;
        private OutputReader _stdErr;
        private int? _exitCode;
        private volatile ImmutableList<Exception> _exceptions;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostController(
            string cliDirectory,
            string microserviceDirectory,
            MicroserviceConfig microserviceConfig,
            EnvironmentConfig environmentConfig)
        {
            _cliDirectory = cliDirectory;
            _microserviceDirectory = microserviceDirectory;
            _microserviceConfig = microserviceConfig;
            _environmentConfig = environmentConfig;
            _exceptions = ImmutableList<Exception>.Empty;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Start()
        {
            _exceptions = ImmutableList<Exception>.Empty;
            _exitCode = null;
            _microserviceFilePath = Path.Combine(Path.GetTempPath(), $"microservice_{Guid.NewGuid().ToString("N")}.xml");
            _environmentFilePath = Path.Combine(Path.GetTempPath(), $"environment_{Guid.NewGuid().ToString("N")}.xml");

            WriteConfigurationXml(_microserviceConfig, _microserviceFilePath);
            WriteConfigurationXml(_environmentConfig, _environmentFilePath);

            var arguments =
                $"{Path.Combine(_cliDirectory, "nwheels.dll")} run --no-publish " +
                _microserviceDirectory +
                $" --microservice-xml {_microserviceFilePath}" +
                $" --environment-xml {_environmentFilePath}";

            var info = new ProcessStartInfo() {
                FileName = "dotnet",
                Arguments = arguments,
                WorkingDirectory = _microserviceDirectory,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            _process = Process.Start(info);

            ValidateStartedOrThrow();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void StopOrThrow(int millisecondsTimeout)
        {
            var stopped = TryStop(millisecondsTimeout);
            stopped.Should().Be(true, "Microservice host didn't stop within allotted timeout.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool TryStop(int millisecondsTimeout)
        {
            if (!_process.HasExited)
            {
                try
                {
                    // answer the "press ENTER to stop" prompt;
                    _process.StandardInput.WriteLine();
                    _process.StandardInput.Dispose();
                }
                catch (Exception e)
                {
                    _exceptions = _exceptions.Add(e);
                }
            }

            var exited = _process.WaitForExit(millisecondsTimeout);
            _exitCode = (exited ? _process.ExitCode : (int?)null);

            _stdOut.Wait(5000);
            _stdErr.Wait(5000);

            _process.Dispose();
            _process = null;

            File.Delete(_microserviceFilePath);
            File.Delete(_environmentFilePath);

            return exited;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FormatOutput(StringBuilder output)
        {
            if (_stdOut != null)
            {
                _stdOut.Format("stdout", output);
            }

            if (_stdErr != null)
            {
                _stdErr.Format("stderr", output);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void FormatSummary(StringBuilder output)
        {
            if (!HasErrors())
            {
                output.AppendLine("Microservice execution completed successfully");
                return;
            }

            output.AppendLine("Microservice execution produced error(s).");

            var exceptionSet = new HashSet<Exception>();

            if (_exitCode.HasValue && _exitCode.Value != 0)
            {
                output.AppendLine($"Process exited with code {_exitCode}.");
            }

            foreach (var exception in _exceptions)
            {
                if (exception is AggregateException aggregate)
                {
                    foreach (var innerException in aggregate.Flatten().InnerExceptions)
                    {
                        addExceptionText(innerException);
                    }
                }
                else
                {
                    addExceptionText(exception);
                }
            }

            FormatOutput(output);

            void addExceptionText(Exception e)
            {
                if (exceptionSet.Add(e))
                {
                    output.AppendLine("------ exception ------");
                    output.AppendLine($"{e.GetType().Name}: {e.Message}");
                    output.AppendLine(e.StackTrace);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AssertNoErrors()
        {
            if (HasErrors())
            {
                var message = new StringBuilder();
                FormatSummary(message);
                throw new Exception(message.ToString());
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool HasErrors()
        {
            var stdErr = _stdErr;

            if (stdErr != null && stdErr.Lines.Count > 0)
            {
                return true;
            }

            if (_exceptions.Count > 0)
            {
                return true;
            }
                    
            if (_exitCode.HasValue && _exitCode.Value != 0)
            {
                return true;
            }

            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ImmutableList<string> StdOut => _stdOut?.Lines ?? ImmutableList<string>.Empty;
        public ImmutableList<string> StdErr => _stdErr?.Lines ?? ImmutableList<string>.Empty;
        public ImmutableList<Exception> Exceptions => _exceptions;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteConfigurationXml<TConfig>(TConfig configuration, string filePath)
        {
            using (var file = File.Create(filePath))
            {
                var serializer = new XmlSerializer(typeof(TConfig));
                serializer.Serialize(file, configuration);
                file.Flush();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ValidateStartedOrThrow()
        {
            try
            {
                _stdErr = new OutputReader(_process.StandardError);

                if (!WaitUntilMicroserviceIsUp(10000, out _stdOut))
                {
                    throw new TimeoutException("Microservice didn't start withen allotted timeout (10 sec).");
                }

                if (_process.HasExited)
                {
                    _process.ExitCode.Should().Be(0, "microservice exit code");
                }
            }
            catch (Exception e)
            {
                _exceptions = _exceptions.Add(e);

                try
                {
                    StopOrThrow(10000);
                }
                catch (Exception e2)
                {
                    _exceptions = _exceptions.Add(e2);
                }

                AssertNoErrors();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool WaitUntilMicroserviceIsUp(int millisecondsTimeout, out OutputReader stdOut)
        {
            using (var signal = new ManualResetEvent(initialState: false))
            {
                stdOut = new OutputReader(_process.StandardOutput, lineCallback: signalOnMicroserviceUp);

                return signal.WaitOne(millisecondsTimeout);

                bool signalOnMicroserviceUp(string line)
                {
                    if (line.ToLower().Contains("microservice is up"))
                    {
                        signal.Set();
                        return false;
                    }
                    return true;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class OutputReader
        {
            private readonly StreamReader _stream;
            private readonly Func<string, bool> _lineCallback;
            private readonly Task _readerTask;
            private volatile ImmutableList<string> _lines;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public OutputReader(StreamReader stream, Func<string, bool> lineCallback = null)
            {
                _stream = stream;
                _lineCallback = lineCallback;
                _lines = ImmutableList<string>.Empty;
                _readerTask = ReadOutput();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Format(string title, StringBuilder output)
            {
                output.AppendLine($"{Environment.NewLine}------ {title} -------{Environment.NewLine}");

                if (_lines.Count > 0)
                {
                    output.AppendLine(string.Join(Environment.NewLine, _lines));
                }

                output.AppendLine($"{Environment.NewLine}--- end of {title} ---{Environment.NewLine}");
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public bool Wait(int millisecondsTimeout)
            {
                return _readerTask.Wait(millisecondsTimeout);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ImmutableList<string> Lines => _lines;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private async Task ReadOutput()
            {
                try
                {
                    var shouldInvokeCallback = true;

                    while (true)
                    {
                        var line = await _stream.ReadLineAsync();

                        if (line == null)
                        {
                            break;
                        }

                        if (_lineCallback != null && shouldInvokeCallback)
                        {
                            shouldInvokeCallback = _lineCallback(line);
                        }

                        _lines = _lines.Add(line);
                    }
                }
                catch (Exception e)
                {
                    _lines = _lines.Add("OUTPUT READER FAILURE! " + e.ToString());
                }
            }
        }
    }
}
