using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;
using System.Threading;
using NWheels.Kernel.Api.Logging;
using NWheels.Microservices.Api;

namespace NWheels.Microservices.Runtime.Cli
{
    public class RunCommand : CliCommandBase
    {
        private readonly MicroserviceHost _host;
        private readonly MutableBootConfiguration _bootConfig;
        private bool _debugLogLevel;
        private bool _verboseLogLevel;
        private bool _useStdinForSignal;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RunCommand(MicroserviceHost host, MutableBootConfiguration bootConfig) 
            : base("run", "Run microservice in the daemon mode")
        {
            _host = host;
            _bootConfig = bootConfig;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void DefineArguments(ArgumentSyntax syntax)
        {
            syntax.DefineOption("d|debug", ref _debugLogLevel, "Set log level to Debug");
            syntax.DefineOption("v|verbose", ref _verboseLogLevel, "Set log level to Verbose");
            syntax.DefineOption("s|stdin-signal", ref _useStdinForSignal, "Treat end of standard input as signal to exit");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void ValidateArguments(ArgumentSyntax arguments)
        {
            if (_debugLogLevel)
            {
                _bootConfig.LogLevel = LogLevel.Debug;
            }
            else if (_verboseLogLevel)
            {
                _bootConfig.LogLevel = LogLevel.Verbose;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override int Execute(CancellationToken cancellation)
        {
            _host.RunDaemon(cancellation, TimeSpan.FromSeconds(30), out bool stoppedWithinTimeout);
            return (stoppedWithinTimeout ? 0 : -3);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public override bool UseStdinForSignal => _useStdinForSignal;
    }
}
