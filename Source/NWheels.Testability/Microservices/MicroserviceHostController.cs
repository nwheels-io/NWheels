using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Serialization;

namespace NWheels.Testability.Microservices
{
    public class MicroserviceHostController
    {
        private readonly MicroserviceConfig _microserviceConfig;
        private readonly EnvironmentConfig _environmentConfig;
        private string _cliDirectory;
        private string _microserviceDirectory;
        private string _microserviceFilePath;
        private string _environmentFilePath;
        private Process _process;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostController(
            string cliDirectory,
            string microserviceDirectory,
            MicroserviceConfig microserviceConfig,
            EnvironmentConfig environmentConfig)
        {
            _cliDirectory = cliDirectory;
            _microserviceDirectory = microserviceDirectory;
            _microserviceConfig = microserviceConfig;
            _environmentConfig = environmentConfig;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Start()
        {
            _microserviceFilePath = Path.Combine(Path.GetTempPath(), $"microservice_{Guid.NewGuid().ToString("N")}.xml");
            _environmentFilePath = Path.Combine(Path.GetTempPath(), $"environment_{Guid.NewGuid().ToString("N")}.xml");

            WriteConfigurationXml(_microserviceConfig, _microserviceFilePath);
            WriteConfigurationXml(_environmentConfig, _environmentFilePath);

            var arguments =
                $"{Path.Combine(_cliDirectory, "nwheels.dll")} run --no-publish " +
                _microserviceDirectory + 
                $" --microservice-xml {_microserviceFilePath}" +
                $" --environment-xml {_environmentFilePath}";

            var info = new ProcessStartInfo() {
                FileName = "dotnet",
                Arguments = arguments,
                WorkingDirectory = _microserviceDirectory,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };

            _process = Process.Start(info);
            _process.StandardOutput.ReadLine();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Stop(int millisecondsTimeout, out int exitCode)
        {
            if (!_process.HasExited)
            {
                try
                {
                    // answer the "press ENTER to stop" prompt;
                    _process.StandardInput.WriteLine();
                    _process.StandardInput.Dispose();
                }
                catch
                {
                }

                while (_process.StandardOutput.ReadLine() != null) ;
            }

            var exited = _process.WaitForExit(millisecondsTimeout);
            exitCode = (exited ? _process.ExitCode : -1);

            _process.Dispose();
            _process = null;

            File.Delete(_microserviceFilePath);
            File.Delete(_environmentFilePath);

            return exited;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteConfigurationXml<TConfig>(TConfig configuration, string filePath)
        {
            using (var file = File.Create(filePath))
            {
                var serializer = new XmlSerializer(typeof(TConfig));
                serializer.Serialize(file, configuration);
                file.Flush();
            }
        }
    }
}
