using NWheels.Cli.Publish;
using NWheels.Microservices;
using NWheels.Microservices.Mocks;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;

namespace NWheels.Cli.Run
{
    public class RunCommand : ICommand
    {
        private readonly ICommandContext _context;
        private string _microserviceFolder;
        private bool _isPublishFolder;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RunCommand(ICommandContext context)
        {
            _context = context;
            _microserviceFolder = _context.GetCurrentDirectory();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void DefineArguments(ArgumentSyntax syntax)
        {
            syntax.DefineParameter("microservice-folder", ref _microserviceFolder, "microservice publish or source folder");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void ValidateArguments()
        {
            if (string.IsNullOrEmpty(_microserviceFolder))
            {
                throw new BadArgumentsException("microservice folder must be specified");
            }

            if (!_context.DirectoryExists(_microserviceFolder))
            {
                throw new BadArgumentsException($"folder does not exist: {_microserviceFolder}");
            }

            if (!DetermineFolderType())
            {
                throw new BadArgumentsException("specified microservice folder is neither source nor publish");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Execute()
        {
            if (!_isPublishFolder)
            {
                PublishMicroservice();
            }

            RunMicroservice();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string Name => "run";
        public string HelpText => "runs microservice from publish folder (optionally performs publish first)";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool DetermineFolderType()
        {
            if (_context.FindFiles(_microserviceFolder, "System.*.dll", recursive: false).Any() &&
                _context.FileExists(Path.Combine(_microserviceFolder, BootConfiguration.MicroserviceConfigFileName)) &&
                _context.FileExists(Path.Combine(_microserviceFolder, BootConfiguration.EnvironmentConfigFileName)))
            {
                _isPublishFolder = true;
                return true;
            }

            if (_context.FindFiles(_microserviceFolder, "*.*proj", recursive: false).Length == 1 &&
                _context.FileExists(Path.Combine(_microserviceFolder, BootConfiguration.MicroserviceConfigFileName)))
            {
                _isPublishFolder = false;
                return true;
            }

            // folder type cannot be determined
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void PublishMicroservice()
        {
            var publish = new PublishCommand(_context);
            ArgumentSyntax.Parse(
                new[] { publish.Name, "--src", _microserviceFolder },
                syntax => publish.BindToCommandLine(syntax)
            );
            publish.ValidateArguments();
            publish.Execute();

            _microserviceFolder = publish.PublishFolderPath;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RunMicroservice()
        {
            // copied almost as is from NWheels.Host
            try
            {
                Console.WriteLine($"configPath {_microserviceFolder}");

                var config = BootConfiguration.LoadFromDirectory(configsPath: _microserviceFolder);
                var host = new MicroserviceHost(config, new MicroserviceHostLoggerMock());

                host.Configure();
                host.LoadAndActivate();

                Console.WriteLine("Microservice is up.");
                Console.Write("Press ENTER to go down.");
                Console.ReadLine();

                host.DeactivateAndUnload();
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }

            Console.Write("Press ENTER to exit.");
            Console.ReadLine();
        }
    }
}
