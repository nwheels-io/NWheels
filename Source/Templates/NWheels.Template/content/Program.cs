using System;
using NWheels.Microservices.Api;

namespace My.Service
{
    class Program
    {
        static int Main(string[] args)
        {
            var cli = new MicroserviceHostBuilder("ServiceName")
                .UseLifecycleComponent<HelloWorldComponent>()
                .BuildCli();

            return cli.Run(args);
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
