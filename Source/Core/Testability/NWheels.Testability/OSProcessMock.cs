using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NWheels.Kernel.Api.Primitives;
using Xunit;
using Xunit.Sdk;

namespace NWheels.Testability
{
    public class OSProcessMock : IOperatingSystemProcess
    {
        private readonly ManualResetEvent _exitEvent;
        private readonly Queue<Step> _script;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public OSProcessMock(params Step[] script)
        {
            _exitEvent = new ManualResetEvent(false);
            _script = new Queue<Step>(script);

            foreach (var step in script)
            {
                step.Owner = this;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            _exitEvent.Dispose();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Start(ProcessStartInfo startInfo)
        {
            if (this.StartInfo != null)
            {
                throw new InvalidOperationException("Process already started.");
            }

            this.StartInfo = startInfo;
            this.Starting?.Invoke(startInfo);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public async Task<string> ReadOutputLineAsync()
        {
            ValidateStarted();

            while (_script.Count > 0)
            {
                var step = _script.Dequeue();

                bool hasStdoutLine = false;
                string stdoutLine = null;

                await step.Execute(onStdoutLine: line => {
                    hasStdoutLine = true;
                    stdoutLine = line;
                });

                if (hasStdoutLine)
                {
                    return stdoutLine;
                }
            }

            // end of script: simulate a finished process
            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CloseInput()
        {
            ValidateStarted();
            this.InputClosed?.Invoke();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool WaitForExit(TimeSpan timeout)
        {
            return _exitEvent.WaitOne(timeout);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool WasStarted => this.StartInfo != null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        bool IOperatingSystemProcess.HasExited
        {
            get
            {
                ValidateStarted();
                return this.HasExited;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        int IOperatingSystemProcess.ExitCode
        {
            get
            {
                ValidateStarted();
                return this.ExitCode;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void AssertEndOfScript()
        {
            _script.Should().BeEmpty(because: "all script steps must be executed");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ProcessStartInfo StartInfo { get; private set; }
        public bool HasExited { get; private set; }
        public int ExitCode { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event Action<ProcessStartInfo> Starting;
        public event Action InputClosed;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ValidateStarted()
        {
            if (this.StartInfo == null)
            {
                throw new InvalidOperationException("Process was not started.");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class Step
        {
            public abstract Task Execute(Action<string> onStdoutLine);
            public OSProcessMock Owner { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class DelegatingStep : Step
        {
            private readonly Func<Action<string>, Task> _onExecute;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DelegatingStep(Func<Action<string>, Task> onExecute)
            {
                _onExecute = onExecute;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override Task Execute(Action<string> onStdoutLine)
            {
                return _onExecute(onStdoutLine);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class StdoutStep : Step
        {
            private readonly string _line;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public StdoutStep(string line)
            {
                _line = line;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override Task Execute(Action<string> onStdoutLine)
            {
                onStdoutLine(_line);
                return Task.CompletedTask;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class DelayStep : Step
        {
            private readonly int _milliseconds;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public DelayStep(int milliseconds)
            {
                _milliseconds = milliseconds;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override Task Execute(Action<string> onStdoutLine)
            {
                return Task.Delay(_milliseconds);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ExitStep : Step
        {
            private readonly int _exitCode;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ExitStep(int exitCode)
            {
                _exitCode = exitCode;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override Task Execute(Action<string> onStdoutLine)
            {
                Owner.ExitCode = _exitCode;
                Owner.HasExited = true;
                Owner._exitEvent.Set();

                return Task.CompletedTask;
            }
        }
    }
}
