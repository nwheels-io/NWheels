using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace NWheels.Microservices
{
    public class BootConfiguration
    {
        public const string EnvironmentConfigFileName = "environment.xml";
        public const string MicroserviceConfigFileName = "microservice.xml";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceConfig MicroserviceConfig { get; set; }
        public EnvironmentConfig EnvironmentConfig { get; set; }
        public string ConfigsDirectory { get; set; }
        public AssemblyLocationMap AssemblyMap { get; set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static BootConfiguration LoadFromDirectory(string configsPath)
        {
            var bootConfig = LoadFromFiles(
                Path.Combine(configsPath, MicroserviceConfigFileName),
                Path.Combine(configsPath, EnvironmentConfigFileName));

            bootConfig.ConfigsDirectory = configsPath;

            return bootConfig;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static BootConfiguration LoadFromFiles(string microserviceFilePath, string environmentFilePath)
        {
            return new BootConfiguration() {
                MicroserviceConfig = Deserialize<MicroserviceConfig>(microserviceFilePath),
                EnvironmentConfig = Deserialize<EnvironmentConfig>(environmentFilePath)
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static TConfig Deserialize<TConfig>(string filePath)
        {
            using (var file = File.OpenRead(filePath))
            {
                var serializer = new XmlSerializer(typeof(TConfig));
                var config = (TConfig)serializer.Deserialize(file);
                return config;
            }
        }
    }
}
