using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace NWheels.Microservices.Runtime.Cli
{
    public abstract class CliCommandBase : ICliCommand
    {
        protected CliCommandBase(string name, string helpText)
        {
            this.Name = name;
            this.HelpText = helpText;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract void DefineArguments(ArgumentSyntax syntax);
        public abstract void ValidateArguments(ArgumentSyntax arguments);
        public abstract int Execute(CancellationToken cancellation);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name { get; }
        public string HelpText { get; }
        public virtual bool UseStdinForSignal => false;
    }
}
