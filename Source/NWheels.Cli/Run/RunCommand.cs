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
    public class RunCommand : CommandBase
    {
        private string _microserviceFolderPath;
        private MicroserviceFolderType _microserviceFolderType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RunCommand() : base(
            "run",
            "runs microservice from publish folder (optionally performs publish first)")
        {
            _microserviceFolderPath = Directory.GetCurrentDirectory();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void DefineArguments(ArgumentSyntax syntax)
        {
            syntax.DefineParameter("microservice-folder", ref _microserviceFolderPath, "microservice publish or source folder");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void ValidateArguments(ArgumentSyntax arguments)
        {
            if (string.IsNullOrEmpty(_microserviceFolderPath))
            {
                arguments.ReportError("microservice folder must be specified");
            }

            if (!Directory.Exists(_microserviceFolderPath))
            {
                arguments.ReportError($"folder does not exist: {_microserviceFolderPath}");
            }

            if (!DetermineFolderType(out _microserviceFolderType))
            {
                arguments.ReportError("specified microservice folder is neither source nor publish");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Execute()
        {
            if (_microserviceFolderType == MicroserviceFolderType.Source)
            {
                PublishMicroservice();
            }

            RunMicroservice();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool DetermineFolderType(out MicroserviceFolderType type)
        {
            if (Directory.GetFiles(_microserviceFolderPath, "System.*.dll", SearchOption.TopDirectoryOnly).Any() &&
                File.Exists(Path.Combine(_microserviceFolderPath, BootConfiguration.MicroserviceConfigFileName)) &&
                File.Exists(Path.Combine(_microserviceFolderPath, BootConfiguration.EnvironmentConfigFileName)))
            {
                type = MicroserviceFolderType.Publish;
                return true;
            }

            if (Directory.GetFiles(_microserviceFolderPath, "*.*proj", SearchOption.TopDirectoryOnly).Length == 1 &&
                File.Exists(Path.Combine(_microserviceFolderPath, BootConfiguration.MicroserviceConfigFileName)))
            {
                type = MicroserviceFolderType.Source;
                return true;
            }

            // folder type cannot be determined
            type = MicroserviceFolderType.Unknown;
            return false;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void PublishMicroservice()
        {
            var publish = new PublishCommand();
            var arguments = ArgumentSyntax.Parse(
                new[] { publish.Name, _microserviceFolderPath, "--no-cli" },
                syntax => publish.BindToCommandLine(syntax)
            );
            publish.ValidateArguments(arguments);
            publish.Execute();

            _microserviceFolderPath = publish.PublishFolderPath;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RunMicroservice()
        {
            // copied almost as is from NWheels.Host
            try
            {
                Console.WriteLine($"configPath {_microserviceFolderPath}");

                var config = BootConfiguration.LoadFromDirectory(configsPath: _microserviceFolderPath);
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private enum MicroserviceFolderType
        {
            Unknown,
            Source,
            Publish
        }
    }
}
