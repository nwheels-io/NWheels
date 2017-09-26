using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;
using System.Threading;

namespace NWheels.Microservices.Runtime.Cli
{
    // There is an intention to move ICliCommand and CliCommandBase to NWheels.Microservices.Api
    // The blocker for this is absense of abtraction above System.CommandLine package.
    // Moving to API without the abstraction would result in tight coupling of other modules (NWheels and applications) to System.CommandLine
    // The tihgt coupling will limit our freedom to choose/implement a different command line parser over time.
    // The move can be performed as soon as an abstraction is developed.

    public interface ICliCommand
    {
        void DefineArguments(ArgumentSyntax syntax);
        void ValidateArguments(ArgumentSyntax arguments);
        int Execute(CancellationToken cancellation);
        string Name { get; }
        string HelpText { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class CommandExtensions
    {
        public static void BindToCommandLine(this ICliCommand command, ArgumentSyntax syntax)
        {
            var commandSyntax = syntax.DefineCommand(command.Name);
            commandSyntax.Help = command.HelpText;
            command.DefineArguments(syntax);
        }
    }
}
