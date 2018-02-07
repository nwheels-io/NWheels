using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Microservices.Runtime.Cli
{
    //TODO: implement according to guidelines
    public class CliCommandFailedException : Exception
    {
        public CliCommandFailedException(Exception innerException, int exitCode)
            : base((innerException?.Message ?? "Error not available"), innerException)
        {
            this.ExitCode = exitCode;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public int ExitCode { get; }
    }
}
