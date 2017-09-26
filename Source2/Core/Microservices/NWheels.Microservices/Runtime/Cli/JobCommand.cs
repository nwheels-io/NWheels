using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;
using System.Threading;

namespace NWheels.Microservices.Runtime.Cli
{
    public class JobCommand : CliCommandBase
    {
        public JobCommand() 
            : base("job", "Execute a service job ad-hoc, and exit")
        {
        }

        public override void DefineArguments(ArgumentSyntax syntax)
        {
            //TBD
        }

        public override void ValidateArguments(ArgumentSyntax arguments)
        {
            //TBD
        }

        public override int Execute(CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }
    }
}
