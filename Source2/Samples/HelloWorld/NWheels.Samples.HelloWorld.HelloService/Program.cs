using System;
using NWheels.Microservices.Api;

namespace NWheels.Samples.HelloWorld.HelloService
{
    class Program
    {
        static int Main(string[] args)
        {
            var cli = new MicroserviceHostBuilder("HelloService")
                .UseLifecycleComponent<HelloComponent>()
                .BuildCli();

            return cli.Run(args);
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
