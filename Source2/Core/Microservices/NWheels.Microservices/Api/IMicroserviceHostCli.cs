using System;
using System.Collections.Generic;
using System.Text;

namespace NWheels.Microservices.Api
{
    public interface IMicroserviceHostCli
    {
        /// <summary>
        /// Runs command-line interface for the microservice
        /// </summary>
        /// <param name="args">
        /// Command line arguments
        /// </param>
        /// <returns>
        /// Exit code to return to the OS: 
        ///    0 = success, 
        ///   -1 = failure during initialization, 
        ///   -2 = failure during execution
        ///   -3 = daemon didn't stop within allotted timeout
        /// </returns>
        /// <remarks>
        /// If command line arguments are invalid, or help is requested with -h, -?, or --help option, 
        /// this method will print appropriate output, then terminate the process with exit code 1.
        /// </remarks>
        int Run(string[] args);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Microservice host object that is operated by the CLI.
        /// </summary>
        IMicroserviceHost Host { get; }
    }
}
