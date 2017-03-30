using System;
using System.CommandLine;

namespace NWheels.Cli
{
    public interface ICommand
    {
        void DefineArguments(ArgumentSyntax syntax);
        void ValidateArguments();
        void Execute();
        string Name { get; }
        string HelpText { get; }
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class CommandExtensions
    {
        public static void BindToCommandLine(this ICommand command, ArgumentSyntax syntax)
        {
            var commandSyntax = syntax.DefineCommand(command.Name);
            commandSyntax.Help = command.HelpText;
            command.DefineArguments(syntax);
        }
    }
}
