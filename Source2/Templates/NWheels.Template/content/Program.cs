using System;
using NWheels.Microservices;
using NWheels.Microservices.Api;
using NWheels.Microservices.Runtime;

namespace My.Service
{
    class Program
    {
        static int Main(string[] args)
        {
            return new MicroserviceHostBuilder("ServiceName")
                .UseLifecycleComponent<HelloWorldComponent>()
                .RunCli(args);
        }

        public class HelloWorldComponent : LifecycleComponentBase
        {
            public override void MicroserviceActivated()
            {
                Console.WriteLine("Hello World!");
            }
        }
    }
}
