using NWheels.Microservices;
using System.IO;
using System.Xml.Serialization;

namespace NWheels.Microservices
{
    public class BootConfiguration
    {
        const string EnvironmentConfigFileName = "environment.xml";
        const string MicroserviceConfigFileName = "microservice.xml";

        public MicroserviceConfig MicroserviceConfig { get; set; }

        public EnvironmentConfig EnvironmentConfig { get; set; }

        public string LoadedFromDirectory { get; set; }

        public string ModulesDirectory { get; set; }

        public static BootConfiguration LoadFromDirectory(string dirPath, string modulesPath)
        {
            return new BootConfiguration() {
                LoadedFromDirectory = dirPath,
                ModulesDirectory = modulesPath,
                MicroserviceConfig = Deserialize<MicroserviceConfig>($"{dirPath}\\{MicroserviceConfigFileName}"),
                EnvironmentConfig = Deserialize<EnvironmentConfig>($"{dirPath}\\{EnvironmentConfigFileName}")
            };
        }

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
