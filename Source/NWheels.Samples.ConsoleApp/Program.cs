using System;
using NWheels.Mechanism;
using NWheels.Api;
using NWheels.Api.Microservices;

class Program
{
    static int Main(string[] args)
    {
        var host = new HostProgram(args);

        return host.Run<IFramework, IMicroservice>(
            (framework, microservice) => {
                Console.WriteLine("Hello World!");
            }
        );
    }
}
