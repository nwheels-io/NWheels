using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace NWheels.Testability.Tests.Unit
{
    public class MicroserviceProcessTests : TestBase.UnitTest
    {
        private class MockProcessHandler : IProcessHandler
        {
            public void Dispose()
            {
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void Start(ProcessStartInfo startInfo)
            {
                if (this.StartInfo != null)
                {
                    throw new InvalidOperationException("Process already started.");
                }

                this.StartInfo = startInfo;
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public Task<string> ReadOntputLineAsync()
            {
                ValidateStarted();
                throw new NotImplementedException();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public void CloseInput()
            {
                ValidateStarted();
                throw new NotImplementedException();
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public ProcessStartInfo StartInfo { get; private set; }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            bool IProcessHandler.HasExited
            {
                get
                {
                    ValidateStarted();
                    return this.HasExited;
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            int IProcessHandler.ExitCode 
            {
                get
                {
                    ValidateStarted();
                    return this.ExitCode;
                }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            public bool HasExited { get; set; }
            public int ExitCode { get; set; }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            private void ValidateStarted()
            {
                if (this.StartInfo == null)
                {
                    throw new InvalidOperationException("Process was not started.");
                }
            }
        }
    }
}
