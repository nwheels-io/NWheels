using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using NWheels.Injection;

namespace NWheels.Testability.Microservices
{
    public class MicroserviceControllerBuilder
    {
        private readonly MicroserviceHostBuilder _hostBuilder;
        private string _microservicePath;
        private MicroserviceController.RunMode? _runMode;
        private string _cliDirectory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceControllerBuilder()
            : this(string.Empty)
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceControllerBuilder(string microserviceName)
        {
            _hostBuilder = new MicroserviceHostBuilder(microserviceName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceControllerBuilder RunMicroserviceAssembly(
            string assemblyFilePath, 
            [CallerFilePath] string sourceFilePath = "")
        {
            _runMode = MicroserviceController.RunMode.DotNetCliAssembly;

            string absolutePath = (
                Path.IsPathRooted(assemblyFilePath)
                ? assemblyFilePath
                : Path.Combine(Path.GetDirectoryName(sourceFilePath), assemblyFilePath));

            _microservicePath = absolutePath;
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceControllerBuilder RunMicroserviceProject(
            string directoryPath, 
            [CallerFilePath] string sourceFilePath = "")
        {
            if (!_runMode.HasValue)
            {
                _runMode = MicroserviceController.RunMode.DotNetCliProject;
            }

            string absolutePath = (
                Path.IsPathRooted(directoryPath)
                ? directoryPath
                : Path.Combine(Path.GetDirectoryName(sourceFilePath), directoryPath));

            _microservicePath = absolutePath;
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceControllerBuilder SetMicroserviceDirectory(
            string directoryPath,
            [CallerFilePath] string sourceFilePath = "")
        {
            string absolutePath = (
                Path.IsPathRooted(directoryPath)
                ? directoryPath
                : Path.Combine(Path.GetDirectoryName(sourceFilePath), directoryPath));

            _microservicePath = absolutePath;
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceControllerBuilder UseNWheelsCliFromSolution(
            string relativeProjectDirectoryPath, 
            string cliProjectConfiguration = null, 
            bool allowOverrideByEnvironmentVar = false,
            [CallerFilePath] string sourceFilePath = "")
        {
            _runMode = MicroserviceController.RunMode.NWheelsCli;

            if (allowOverrideByEnvironmentVar)
            {
                var environmentValue = GetCliDirectoryFromEnvironment();
                if (!string.IsNullOrEmpty(environmentValue))
                {
                    _cliDirectory = environmentValue;
                    return this;
                }
            }

            var effectiveCliProjectConfiguration = cliProjectConfiguration ?? DefaultProjectConfigurationName;

            relativeProjectDirectoryPath = relativeProjectDirectoryPath
                .Replace('\\', Path.DirectorySeparatorChar)
                .Replace('/', Path.DirectorySeparatorChar);

            _cliDirectory = Path.Combine(
                Path.GetDirectoryName(sourceFilePath),
                relativeProjectDirectoryPath,
                "..",
                $"NWheels.Cli/bin/{effectiveCliProjectConfiguration}/netcoreapp1.1".Replace('/', Path.DirectorySeparatorChar));

            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceControllerBuilder UseNWheelsCliFromEnvironment()
        {
            _runMode = MicroserviceController.RunMode.NWheelsCli;
            _cliDirectory = GetCliDirectoryFromEnvironment();
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceControllerBuilder BuildMicroservice(Func<IMicroserviceHostBuilder, IMicroserviceHostBuilder> builder)
        {
            builder(_hostBuilder);
            return this;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public MicroserviceController Build()
        {
            if (!_runMode.HasValue)
            {
                throw new InvalidOperationException(
                    "Run mode not specified. " +
                    "Call one of: UseMicroserviceAssembly, UseMicroserviceProjectDirectory, UseCliDirectoryFromSolution, UseCliDirectoryFromEnvironment");
            }

            var controller = new MicroserviceController(
                _runMode.Value, 
                _microservicePath, 
                _hostBuilder.BootConfig.MicroserviceConfig,
                _hostBuilder.BootConfig.EnvironmentConfig,
                _cliDirectory);

            return controller;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string GetCliDirectoryFromEnvironment()
        {
            return Environment.GetEnvironmentVariable("NWHEELS_CLI");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string DefaultProjectConfigurationName =>
#if DEBUG
                "Debug"
#else
                "Release"
#endif
        ;
    }
}
