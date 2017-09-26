using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;
using System.Threading;

namespace NWheels.Microservices.Runtime.Cli
{
    public class RunCommand : CliCommandBase
    {
        private readonly MicroserviceHost _host;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected RunCommand(MicroserviceHost host) 
            : base("run", "Run microservice in the daemon mode")
        {
            _host = host;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void DefineArguments(ArgumentSyntax syntax)
        {
            //TBD
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void ValidateArguments(ArgumentSyntax arguments)
        {
            //TBD
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override int Execute(CancellationToken cancellation)
        {
            _host.RunDaemon(cancellation, TimeSpan.FromSeconds(30), out bool stoppedWithinTimeout);
            return (stoppedWithinTimeout ? 0 : -3);
        }
    }
}
