using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Kernel.Api.Primitives
{
    public interface IOperatingSystemProcess : IDisposable
    {
        void Start(ProcessStartInfo startInfo);
        Task<string> ReadOutputLineAsync();
        void CloseInput();
        bool WaitForExit(TimeSpan timeout);
        bool WasStarted { get; }
        bool HasExited { get; }
        int ExitCode { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public sealed class RealOperatingSystemProcess : IOperatingSystemProcess
    {
        private Process _process;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Start(ProcessStartInfo startInfo)
        {
            if (_process != null)
            {
                throw new InvalidOperationException("Process already started.");
            }

            _process = Process.Start(startInfo);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Dispose()
        {
            _process?.Dispose();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public Task<string> ReadOutputLineAsync()
        {
            ValidateStarted();
            return _process.StandardOutput.ReadLineAsync();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CloseInput()
        {
            ValidateStarted();
            _process.StandardInput.Close();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool WaitForExit(TimeSpan timeout)
        {
            ValidateStarted();
            return _process.WaitForExit((int) timeout.TotalMilliseconds);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool WasStarted => _process != null;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool HasExited => _process != null && _process.HasExited;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int ExitCode
        {
            get
            {
                ValidateStarted();
                return _process.ExitCode;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ValidateStarted()
        {
            if (_process == null)
            {
                throw new InvalidOperationException("Process was not started.");
            }
        }
    }
}
