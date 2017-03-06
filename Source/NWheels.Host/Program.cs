using NWheels.Microservices;
using System;

namespace NWheels.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            //--configPath
            //--modulesPath

            try
            {
                Console.WriteLine($"configPath {args[1]}");
                Console.WriteLine($"modulesPath {args[3]}");

                var config = BootConfiguration.LoadFromDirectory(args[1], args[3]);
                var host = new MicroserviceHost(config, args[3]);

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