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
                    MapAssemblyLocationsFromSources(bootConfig);
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

        private void MapAssemblyLocationsFromSources(BootConfiguration bootConfig)
        {
            bootConfig.AssemblyMap = new AssemblyLocationMap();

            var solutionFolderPath = Directory.GetParent(bootConfig.ConfigsDirectory).FullName;
            var allProjectNames = Enumerable.Empty<string>()
                .Append(bootConfig.MicroserviceConfig.InjectionAdapter.Assembly)
                .Concat(bootConfig.MicroserviceConfig.FrameworkModules.Select(m => m.Assembly))
                .Concat(bootConfig.MicroserviceConfig.ApplicationModules.Select(m => m.Assembly));

            foreach (var projectName in allProjectNames)
            {
                if (TryFindProjectBinaryFolder(
                    projectName, 
                    solutionFolderPath, 
                    out string projectFilePath,
                    out string binaryFolderPath))
                {
                    var projectAssemblyLocations = MapProjectAssemblyLocations(projectFilePath);
                    bootConfig.AssemblyMap.AddDirectory(binaryFolderPath);
                    bootConfig.AssemblyMap.AddLocations(projectAssemblyLocations);
                }
            }

            bootConfig.AssemblyMap.AddDirectory(AppContext.BaseDirectory);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private bool TryFindProjectBinaryFolder(string projectName, string solutionFolderPath, out string projectFilePath, out string binaryFolderPath)
        {
            projectFilePath = null;
            binaryFolderPath = null;

            var projectFolderPath = Path.Combine(solutionFolderPath, projectName);
            if (!Directory.Exists(projectFolderPath))
            {
                return false;
            }

            projectFilePath = Directory.GetFiles(projectFolderPath, "*.*proj", SearchOption.TopDirectoryOnly).SingleOrDefault();
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

        private IReadOnlyDictionary<string, string> MapProjectAssemblyLocations(string projectFilePath)
        {
            var sdkDirectory = @"C:\Program Files\dotnet\sdk\1.0.0\Sdks\Microsoft.NET.Sdk"; //TODO: determine programmatically
            var tempProjectFilePath = Path.Combine(Path.GetTempPath(), $"nwheels_cli_{Guid.NewGuid().ToString("N")}.proj");
            var tempProjectXml = GenerateResolvePublishAssembliesProject(projectFilePath, sdkDirectory);

            using (var tempFile = File.Create(tempProjectFilePath))
            {
                tempProjectXml.Save(tempFile);
                tempFile.Flush();
            }

            try
            {
                ExecuteProgram(
                    out IEnumerable<string> output, 
                    "dotnet", new[] { "msbuild", tempProjectFilePath, "/nologo" });

                var parsedMap = ParseAssemblyDirectoryMap(output);
                return parsedMap;
            }
            finally
            {
                File.Delete(tempProjectFilePath);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private XElement GenerateResolvePublishAssembliesProject(string projectFilePath, string sdkDirectory)
        {
            XNamespace ns = @"http://schemas.microsoft.com/developer/msbuild/2003";

            var projectElement = new XElement(ns + "Project",
                new XElement(ns + "UsingTask",
                    new XAttribute("TaskName", "ResolvePublishAssemblies"),
                    new XAttribute(
                        "AssemblyFile",  //TODO: determine this path programmatically
                        Path.Combine(sdkDirectory, "tools", "netcoreapp1.0", "Microsoft.NET.Build.Tasks.dll"))),
                new XElement(ns + "Target",
                    new XAttribute("Name", "Build"),
                    new XElement(ns + "ResolvePublishAssemblies",
                        new XAttribute("ProjectPath", projectFilePath),
                        new XAttribute("AssetsFilePath", Path.Combine(Path.GetDirectoryName(projectFilePath), "obj", "project.assets.json")),
                        new XAttribute("TargetFramework", ".NETStandard,Version=v1.6"),
                        new XElement(ns + "Output",
                            new XAttribute("TaskParameter", "AssembliesToPublish"),
                            new XAttribute("ItemName", "ResolvedAssembliesToPublish"))),
                    new XElement(ns + "Message",
                        new XAttribute("Text", "%(DestinationSubPath)=@(ResolvedAssembliesToPublish)"),
                        new XAttribute("Importance", "high"))));

            return projectElement;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IReadOnlyDictionary<string, string> ParseAssemblyDirectoryMap(IEnumerable<string> nameValuePairLines)
        {
            var map = new Dictionary<string, string>();

            foreach (var line in nameValuePairLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var nameValueParts = line.Trim().Split('=');

                if (nameValueParts.Length == 2 && !string.IsNullOrEmpty(nameValueParts[0]) && !string.IsNullOrEmpty(nameValueParts[1]))
                {
                    AddAssemblyDirectoryMapEntry(map, assemblyPart: nameValueParts[0], directoryPart: nameValueParts[1]);
                }
                else
                {
                    LogWarning($"Assembly directory pair could not be parsed: {line}");
                }
            }

            return map;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void AddAssemblyDirectoryMapEntry(Dictionary<string, string> map, string assemblyPart, string directoryPart)
        {
            var fileName = Path.GetFileName(assemblyPart);
            var fileExtension = Path.GetExtension(fileName).ToLower();
            string assemblyName;

            if (fileExtension == ".exe" || fileExtension == ".dll" || fileExtension == ".so")
            {
                assemblyName = fileName.Substring(0, fileName.Length - fileExtension.Length);
            }
            else
            {
                assemblyName = fileName;
            }

            if (!map.ContainsKey(assemblyName))
            {
                map.Add(assemblyName, directoryPart);
            }
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
