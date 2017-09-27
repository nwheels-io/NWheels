using NWheels.Microservices;
using System;
using NWheels.Microservices.Api;
using NWheels.Microservices.Runtime;

namespace NWheels.Samples.HelloWorld.HelloService
{
    class Program
    {
        static int Main(string[] args)
        {
            return new MicroserviceHostBuilder("HelloService")
                .UseLifecycleComponent<HelloComponent>()
                .RunCli(args);
        }

        public class HelloComponent : LifecycleComponentBase
        {
            public override void MicroserviceActivated()
            {
                Console.WriteLine("HELLO WORLD!!!");
            }
        }
    }
}
