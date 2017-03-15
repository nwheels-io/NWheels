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

        public string ConfigsDirectory { get; set; }

        public static BootConfiguration LoadFromDirectory(string configsPath)
        {
            return new BootConfiguration() {
                ConfigsDirectory = configsPath,
                MicroserviceConfig = Deserialize<MicroserviceConfig>($"{configsPath}\\{MicroserviceConfigFileName}"),
                EnvironmentConfig = Deserialize<EnvironmentConfig>($"{configsPath}\\{EnvironmentConfigFileName}")
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
