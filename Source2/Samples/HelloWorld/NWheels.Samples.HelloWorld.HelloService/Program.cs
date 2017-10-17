using System;
using System.Threading.Tasks;
using NWheels.Microservices.Api;

namespace NWheels.Samples.HelloWorld.HelloService
{
    public class Program
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

        public class HelloTx
        {
            public async Task<string> Hello(string name)
            {
                await Task.Delay(100);
                return $"Hello, {name}!";
            }
        }
    }
}
