using System;

namespace NWheels.Cli
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World from NWheels CLI!");

            for (int i = 0 ; i < args.Length ; i++)
            {
                Console.WriteLine("- arg[{0}] = {1}", i, args[i]);
            }
        }
    }
}
