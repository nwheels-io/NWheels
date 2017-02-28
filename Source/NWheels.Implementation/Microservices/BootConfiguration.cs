using NWheels.Configuration;

namespace NWheels.Microservices
{
    public class BootConfiguration
    {
        const string MicroserviceConfigFileName = "microservice.xml";
        const string EnvironmentConfigFileName = "environment.xml";

        public MicroserviceConfig MicroserviceConfig { get; set; }

        public EnvironmentConfig EnvironmentConfig { get; set; }
    }
}
