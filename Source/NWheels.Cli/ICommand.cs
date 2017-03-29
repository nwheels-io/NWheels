using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;

namespace NWheels.Cli
{
    public interface ICommand
    {
        void DefineArguments(ArgumentSyntax syntax);
        void ValidateArguments(ArgumentSyntax syntax);
        void Execute();
        string Name { get; }
        string HelpText { get; }
    }
}
