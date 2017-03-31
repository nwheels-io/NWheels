using NWheels.Microservices;
using NWheels.Microservices.Mocks;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NWheels.Cli.Publish
{
    public class PublishCommand : CommandBase
    {
        public static readonly string DefaultPulishSubFolder = "bin/microservice.working".Replace('/', Path.DirectorySeparatorChar);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string _sourceFolderPath;
        private string _publishFolderPath;
        private string _environmentFilePath;
        private bool _suppressPublishCli;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PublishCommand() : base(
            "publish",
            "creates working folder with all files required to run a microservice")
        {
            _sourceFolderPath = Directory.GetCurrentDirectory();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void DefineArguments(ArgumentSyntax syntax)
        {
            syntax.DefineOption("o|out", ref _publishFolderPath, requireValue: false, help: "working folder to publish to");
            syntax.DefineOption("e|env", ref _environmentFilePath, requireValue: false, help: "path to environment XML file to use");
            syntax.DefineOption("n|no-cli", ref _suppressPublishCli, requireValue: false, help: "suppress publish CLI (useful for F5 debug)");
            syntax.DefineParameter("source-folder", ref _sourceFolderPath, "micoservice source folder*");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void ValidateArguments(ArgumentSyntax arguments)
        {
            if (string.IsNullOrEmpty(_sourceFolderPath))
            {
                arguments.ReportError("microservice folder must be specified");
            }

            if (!Directory.Exists(_sourceFolderPath))
            {
                arguments.ReportError($"folder does not exist: {_sourceFolderPath}");
            }

            if (string.IsNullOrEmpty(_publishFolderPath))
            {
                _publishFolderPath = Path.Combine(_sourceFolderPath, DefaultPulishSubFolder);
            }

            if (!Directory.Exists(_publishFolderPath))
            {
                Directory.CreateDirectory(_publishFolderPath);
            }

            if (string.IsNullOrEmpty(_environmentFilePath))
            {
                _environmentFilePath = Path.Combine(_sourceFolderPath, BootConfiguration.EnvironmentConfigFileName);
            }

            if (!File.Exists(_environmentFilePath))
            {
                arguments.ReportError($"environment file does not exist: {_environmentFilePath}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Execute()
        {
            File.Copy(
                Path.Combine(_sourceFolderPath, BootConfiguration.MicroserviceConfigFileName),
                Path.Combine(_publishFolderPath, BootConfiguration.MicroserviceConfigFileName),
                overwrite: true);

            File.Copy(
                _environmentFilePath,
                Path.Combine(_publishFolderPath, BootConfiguration.EnvironmentConfigFileName),
                overwrite: true);

            var bootConfig = BootConfiguration.LoadFromDirectory(_publishFolderPath);

            //TODO: the following code assumes that NWheels and the application reside in the same solution,
            //      which obviously will not be the case with real users.
            //      the full implementation of this command should bring NWheels assemblies from NuGet

            DotNetPublish(bootConfig.MicroserviceConfig.InjectionAdapter.Assembly);

            if (!_suppressPublishCli)
            {
                DotNetPublish("NWheels.Cli");
            }

            var allModules = bootConfig.MicroserviceConfig.FrameworkModules.Concat(bootConfig.MicroserviceConfig.ApplicationModules);

            foreach (var module in allModules)
            {
                DotNetPublish(module.Assembly);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string PublishFolderPath => _publishFolderPath;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void DotNetPublish(string assemblyName)
        {
            LogImportant($"publish > {assemblyName}");

            var projectFolderPath = Path.Combine(Path.GetDirectoryName(_sourceFolderPath), assemblyName);

            if (ValidateProjectFolder(projectFolderPath, out string projectFilePath))
            {
                ExecuteProgram(
                    "dotnet",
                    new[] { "publish", projectFilePath, "-o", _publishFolderPath });
            }
            else
            {
                LogFatal($"project not found: {assemblyName}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static bool ValidateProjectFolder(string path, out string projectFilePath)
        {
            if (Directory.Exists(path))
            {
                projectFilePath = Directory.GetFiles(path, "*.*proj", SearchOption.TopDirectoryOnly).FirstOrDefault();
                return (projectFilePath != null);
            }

            projectFilePath = null;
            return false;
        }
    }
}
