using NWheels.Microservices;
using NWheels.Microservices.Mocks;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;

namespace NWheels.Cli.Publish
{
    public class PublishCommand : ICommand
    {
        public const string DefaultPulishSubFolder = "bin/microservice.working";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly ICommandContext _context;
        private string _sourceFolder;
        private string _publishFolder;
        private string _environmentFile;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PublishCommand(ICommandContext context)
        {
            _context = context;
            _sourceFolder = _context.GetCurrentDirectory();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DefineArguments(ArgumentSyntax syntax)
        {
            syntax.DefineOption("o|out", ref _publishFolder, requireValue: false, help: "working folder to publish to");
            syntax.DefineOption("e|env", ref _environmentFile, requireValue: false, help: "path to environment XML file to use");
            syntax.DefineParameter("source-folder", ref _sourceFolder, "micoservice source folder*");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ValidateArguments()
        {
            if (string.IsNullOrEmpty(_sourceFolder))
            {
                throw new BadArgumentsException("microservice folder must be specified");
            }

            if (!_context.DirectoryExists(_sourceFolder))
            {
                throw new BadArgumentsException($"folder does not exist: {_sourceFolder}");
            }

            if (string.IsNullOrEmpty(_publishFolder))
            {
                _publishFolder = Path.Combine(_sourceFolder, DefaultPulishSubFolder);
            }

            if (!_context.DirectoryExists(_publishFolder))
            {
                _context.CreateDirectory(_publishFolder);
            }

            if (string.IsNullOrEmpty(_environmentFile))
            {
                _environmentFile = Path.Combine(_sourceFolder, BootConfiguration.EnvironmentConfigFileName);
            }

            if (!File.Exists(_environmentFile))
            {
                throw new BadArgumentsException($"environment file does not exist: {_environmentFile}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Execute()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name => "publish";
        public string HelpText => "creates working folder with all files required to run a microservice";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string PublishFolderPath => _publishFolder;
    }
}
