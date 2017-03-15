using NWheels.Microservices;
using NWheels.Microservices.Mocks;
using System;

namespace NWheels.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            //--configPath

            try
            {
                Console.WriteLine($"configPath {args[1]}");

                var config = BootConfiguration.LoadFromDirectory(configsPath: args[1]);
                var host = new MicroserviceHost(config, new MicroserviceHostLoggerMock());

                host.Configure();
                host.LoadAndActivate();

                Console.WriteLine("Microservice is up.");
                Console.Write("Press ENTER to go down.");  
                Console.ReadLine();

                host.DeactivateAndUnload();
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }

            Console.Write("Press ENTER to exit.");
            Console.ReadLine();
        }
    }
}