using NWheels.Cli.Publish;
using NWheels.Microservices;
using NWheels.Microservices.Mocks;
using NWheels.Extensions;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace NWheels.Cli.Run
{
    public class RunCommand : CommandBase
    {
        private string _microserviceFolderPath;
        private bool _noPublish;
        private string _projectConfigurationName;
        private MicroserviceFolderType _microserviceFolderType;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RunCommand() : base(
            "run",
            "runs microservice from publish folder (optionally performs publish first)")
        {
            _microserviceFolderPath = Directory.GetCurrentDirectory();
            _projectConfigurationName = "Debug";
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void DefineArguments(ArgumentSyntax syntax)
        {
            syntax.DefineOption("n|no-publish", 
                ref _noPublish, 
                help: "run without publish: load modules from where they were compiled");

            syntax.DefineOption("p|project-config", 
                ref _projectConfigurationName, requireValue: true, 
                help: "project configuration name, when used with source folder (default: Debug)");

            syntax.DefineParameter("microservice-folder", 
                ref _microserviceFolderPath, 
                "microservice publish or source folder");
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

            if (_noPublish && _microserviceFolderType != MicroserviceFolderType.Source)
            {
                arguments.ReportError("--no-publish cannot be used with a published microservice");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void Execute()
        {
            if (_microserviceFolderType == MicroserviceFolderType.Source && !_noPublish)
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
                new[] { publish.Name, _microserviceFolderPath, "--no-cli", "--project-config", _projectConfigurationName },
                syntax => publish.BindToCommandLine(syntax)
            );
            publish.ValidateArguments(arguments);
            publish.Execute();

            _microserviceFolderPath = publish.PublishFolderPath;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void RunMicroservice()
        {
            try
            {
                LogImportant($"run > {_microserviceFolderPath}");
                var bootConfig = BootConfiguration.LoadFromDirectory(configsPath: _microserviceFolderPath);

                if (_noPublish)
                {
                    ListSourceProjectAssemblyDirectories(bootConfig);
                }

                using (var host = new MicroserviceHost(bootConfig, new MicroserviceHostLoggerMock()))
                {
                    host.Configure();
                    host.LoadAndActivate();

                    LogSuccess("Microservice is up.");
                    LogSuccess("Press ENTER to go down.");

                    Console.ReadLine();

                    host.DeactivateAndUnload();
                }
            }
            catch (Exception ex)
            {
                ReportFatalError(ex);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void ListSourceProjectAssemblyDirectories(BootConfiguration bootConfig)
        {
            var directorySet = new HashSet<string>();

            var solutionFolderPath = Directory.GetParent(bootConfig.ConfigsDirectory).FullName;
            var allProjectNames = Enumerable.Empty<string>()
                .Append(bootConfig.MicroserviceConfig.InjectionAdapter.Assembly)
                .Concat(bootConfig.MicroserviceConfig.FrameworkModules.Select(m => m.Assembly))
                .Concat(bootConfig.MicroserviceConfig.ApplicationModules.Select(m => m.Assembly));

            foreach (var projectName in allProjectNames)
            {
                if (TryFindProjectBinaryFolder(projectName, solutionFolderPath, out string binaryFolderPath))
                {
                    directorySet.Add(binaryFolderPath);
                }
            }

            bootConfig.AssemblyDirectories = new List<string>();
            bootConfig.AssemblyDirectories.AddRange(directorySet);
            bootConfig.AssemblyDirectories.Add(AppContext.BaseDirectory);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool TryFindProjectBinaryFolder(string projectName, string solutionFolderPath, out string binaryFolderPath)
        {
            binaryFolderPath = null;

            var projectFolderPath = Path.Combine(solutionFolderPath, projectName);
            if (!Directory.Exists(projectFolderPath))
            {
                return false;
            }

            var projectFilePath = Directory.GetFiles(projectFolderPath, "*.*proj", SearchOption.TopDirectoryOnly).SingleOrDefault();
            if (projectFilePath == null)
            {
                return false;
            }

            var projectElement = XElement.Parse(File.ReadAllText(projectFilePath));
            var sdkAttribute = projectElement.Attribute("Sdk");
            if (projectElement.Name != "Project" || sdkAttribute == null || sdkAttribute.Value != "Microsoft.NET.Sdk")
            {
                return false;
            }

            var targetFrameworkElement = projectElement.XPathSelectElement("PropertyGroup/TargetFramework");
            var outputPathElement = projectElement.XPathSelectElement("PropertyGroup/OutputPath");

            var slash = Path.DirectorySeparatorChar;
            var targetFramework = (targetFrameworkElement?.Value.DefaultIfNullOrEmpty(null) ?? "netstandard1.6");
            var outputPath = (outputPathElement?.Value.DefaultIfNullOrEmpty(null) ?? $"bin{slash}{_projectConfigurationName}{slash}{targetFramework}");

            binaryFolderPath = Path.Combine(projectFolderPath, outputPath);
            return Directory.Exists(binaryFolderPath);
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
