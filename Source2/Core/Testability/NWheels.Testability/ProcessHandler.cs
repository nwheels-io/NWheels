using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Testability
{
    public interface IProcessHandler : IDisposable
    {
        void Start(ProcessStartInfo startInfo);
        Task<string> ReadOntputLineAsync();
        void CloseInput();
        bool HasExited { get; }
        int ExitCode { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public sealed class RealProcessHandler : IProcessHandler
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

        public Task<string> ReadOntputLineAsync()
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

        public bool HasExited
        {
            get
            {
                ValidateStarted();
                return _process.HasExited;
            }
        }

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
