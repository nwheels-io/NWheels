using NWheels.Microservices;
using System;
using NWheels.Microservices.Runtime;

namespace NWheels.Samples.HelloWorld.HelloService
{
    class Program
    {
        static int Main(string[] args)
        {
            return new MicroserviceHostBuilder("HelloService").RunCli(args);
        }
    }
}
