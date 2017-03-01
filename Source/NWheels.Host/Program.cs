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

                var config = BootConfiguration.LoadFromDirectory(args[1]);
                var host = new MicroserviceHost(config);
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }

            Console.ReadLine();
        }
    }
}