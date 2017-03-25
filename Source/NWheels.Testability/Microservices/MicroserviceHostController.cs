using NWheels.Microservices;
using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Testability.Microservices
{
    public class MicroserviceHostController
    {
        private readonly string _workingDirectory;
        private readonly MicroserviceConfig _microserviceConfig;
        private readonly EnvironmentConfig _environmentConfig;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MicroserviceHostController(string workingDirectory, MicroserviceConfig microserviceConfig, EnvironmentConfig environmentConfig)
        {
            _workingDirectory = workingDirectory;
            _microserviceConfig = microserviceConfig;
            _environmentConfig = environmentConfig;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void Start()
        {
            throw new NotImplementedException();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public bool Stop(int millisecondsTimeout)
        {
            throw new NotImplementedException();
        }
    }
}
