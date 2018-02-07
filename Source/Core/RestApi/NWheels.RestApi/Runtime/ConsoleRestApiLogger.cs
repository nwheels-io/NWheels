using System;
using NWheels.Kernel.Api.Logging;
using NWheels.Microservices.Api;
using NWheels.Microservices.Runtime.Cli;
using NWheels.RestApi.Api;

namespace NWheels.RestApi.Runtime
{
    public class ConsoleRestApiLogger : IRestApiLogger
    {
        private readonly IBootConfiguration _bootConfig;
        private readonly LogLevel _logLevel;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ConsoleRestApiLogger(IBootConfiguration bootConfig)
        {
            _bootConfig = bootConfig;
            _logLevel = bootConfig.LogLevel;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------
        
        public void RestApiRequestCompleted(string resourceUrl, string verb)
        {
            if (_logLevel <= LogLevel.Verbose)
            {
                ColorConsole.Log(LogLevel.Verbose, $"{nameof(RestApiRequestCompleted)}: {nameof(resourceUrl)}={resourceUrl}, {nameof(verb)}={verb}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RestApiRequestFailed(string resourceUrl, string verb, Exception error)
        {
            if (_logLevel <= LogLevel.Error)
            {
                ColorConsole.Log(
                    LogLevel.Error, 
                    $"{nameof(RestApiRequestFailed)}: {nameof(resourceUrl)}={resourceUrl}, {nameof(verb)}={verb}, {nameof(error)}={error}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RestApiBadRequest(string resourceUrl, string verb)
        {
            if (_logLevel <= LogLevel.Error)
            {
                ColorConsole.Log(LogLevel.Error, $"{nameof(RestApiBadRequest)}: {nameof(resourceUrl)}={resourceUrl}, {nameof(verb)}={verb}");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void RestApiBadRequest(string resourceUrl, string verb, Exception error)
        {
            if (_logLevel <= LogLevel.Error)
            {
                ColorConsole.Log(
                    LogLevel.Error, 
                    $"{nameof(RestApiBadRequest)}: {nameof(resourceUrl)}={resourceUrl}, {nameof(verb)}={verb}, {nameof(error)}={error}");
            }
        }
    }
}