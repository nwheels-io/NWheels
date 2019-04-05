using System;
using System.IO;
using NWheels.Build;

namespace NWheels.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("NWheels build tool");
            Console.WriteLine($"Building project: {args[0]}");

            int exitCode;

            Console.WriteLine($"------ BUILD STARTING ------");

            try
            {
                var options = new BuildOptions(args[0]);
                var engine = new BuildEngine(options);
                var success = engine.Build(); 

                exitCode = success ? 0 : 1;
            }
            catch (BuildErrorException e)
            {
                exitCode = 1;
                Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                exitCode = 100;
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine($"------ BUILD {(exitCode == 0 ? "SUCCESS" : "FAILURE")} ------");

            return exitCode;
        }
    }
}