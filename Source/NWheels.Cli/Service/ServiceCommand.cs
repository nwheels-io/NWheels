using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Text;

namespace NWheels.Cli.Service
{
    public class ServiceCommand : ICommand
    {
        private string _sourceFolderPath;
        private string _workingFolderPath;
        private string _environmentFilePath;
        private bool _shouldBuild;
        private bool _shouldRun;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DefineArguments(ArgumentSyntax syntax)
        {
            syntax.DefineOption("b|build", ref _shouldBuild, requireValue: false, help: "Build microservice working directory from sources");
            syntax.DefineOption("r|run", ref _shouldRun, requireValue: false, help: "Run microservice from working directory that was built");
            syntax.DefineOption("s|src", ref _sourceFolderPath, requireValue: false, help: "Source project folder of microservice (where .csproj resides)");
            syntax.DefineOption("w|work", ref _workingFolderPath, requireValue: false, help: "Microservice working folder, from where it can be run");
            syntax.DefineOption("e|env", ref _environmentFilePath, requireValue: false, help: "Path to environment.xml file to use");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ValidateArguments(ArgumentSyntax syntax)
        {
            if (!_shouldBuild && !_shouldRun)
            {
                syntax.ReportError("neither --build nor --run was specified.");
            }

            if (_shouldBuild && string.IsNullOrEmpty(_sourceFolderPath))
            {
                syntax.ReportError("when --build is specified, --src must be also specified.");
            }

            if (_shouldBuild && !Directory.Exists(_sourceFolderPath))
            {
                syntax.ReportError($"source folder does not exist: {_sourceFolderPath}");
            }

            if (_shouldRun && !string.IsNullOrEmpty(_workingFolderPath) && !Directory.Exists(_workingFolderPath))
            {
                syntax.ReportError($"working folder does not exist: {_workingFolderPath}");
            }

            if (!string.IsNullOrEmpty(_environmentFilePath) && !File.Exists(_environmentFilePath))
            {
                syntax.ReportError($"environment file does not exist: {_environmentFilePath}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Execute()
        {
            Console.WriteLine("EXECUTING SERVICE COMMAND!");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name => "service";
        public string HelpText => "Microservice execution and management";
    }
}
